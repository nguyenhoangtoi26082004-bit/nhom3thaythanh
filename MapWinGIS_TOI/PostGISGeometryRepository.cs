using System;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // =============================================================
    // POSTGIS GEOMETRY REPOSITORY - HÀ NỘI ONLY
    //
    // Hệ tọa độ toàn hệ thống:
    // - MapWinGIS : EPSG:32648
    // - PostGIS   : EPSG:32648
    //
    // Vì Map và CSDL cùng CRS nên KHÔNG còn:
    // - EPSG:3857
    // - ST_Transform(..., 32648)
    //
    // Geometry được lưu trực tiếp theo EPSG:32648.
    //
    // Bảng nghiệp vụ:
    // - don_vi_quan_doi      : Point
    // - tuyen_duong_don_vi   : LineString
    // - khu_vuc_don_vi       : Polygon
    //
    // Lưu ý:
    // - Point của don_vi_quan_doi hiện được Editing.cs tự tạo từ
    //   ST_PointOnSurface() của xã/phường đã chọn.
    // - SaveGeometry() chủ yếu dùng cho tuyến đường và khu vực.
    // =============================================================
    public sealed class PostGISGeometryRepository
    {
        private const string PG_HOST =
            "localhost";

        private const int PG_PORT =
            5433;

        private const string PG_DATABASE =
            "HQTCSDL";

        private const string PG_USERNAME =
            "postgres";

        private const string PG_PASSWORD =
            "123456";

        private const string PG_SCHEMA =
            "ql_don_vi";

        private const int DATA_SRID =
            32648;

        private readonly string _connectionString;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public PostGISGeometryRepository()
        {
            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host =
                PG_HOST;

            builder.Port =
                PG_PORT;

            builder.Database =
                PG_DATABASE;

            builder.Username =
                PG_USERNAME;

            builder.Password =
                PG_PASSWORD;

            builder.Timeout =
                5;

            builder.CommandTimeout =
                20;

            builder.Pooling =
                true;

            _connectionString =
                builder.ConnectionString;
        }

        // =========================================================
        // LƯU / THAY THẾ GEOMETRY
        //
        // WKT đầu vào đã nằm trong EPSG:32648.
        // Không Transform.
        // =========================================================
        public void SaveGeometry(
            string tableName,
            int recordId,
            string wkt32648)
        {
            if (recordId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordId");
            }

            if (string.IsNullOrWhiteSpace(
                wkt32648))
            {
                throw new ArgumentException(
                    "Geometry WKT không được để trống.");
            }

            string geometryType =
                GetGeometryType(
                    tableName);

            if (geometryType.Length == 0)
            {
                throw new InvalidOperationException(
                    "Không hỗ trợ lưu hình học cho bảng \"" +
                    tableName +
                    "\".");
            }

            // Kiểm tra WKT trước khi UPDATE để:
            // - bắt lỗi WKT;
            // - kiểm tra geometry rỗng;
            // - kiểm tra đúng loại Point/LineString/Polygon;
            // - kiểm tra Polygon hợp lệ.
            ValidateGeometry(
                wkt32648,
                geometryType);

            string sql =
                "UPDATE " +
                QuoteQualifiedTable(
                    tableName) +
                " SET geom = " +
                    "ST_Force2D(" +
                        "ST_GeomFromText(@wkt, " +
                        DATA_SRID +
                        ")" +
                    ")::geometry(" +
                        geometryType +
                        ", " +
                        DATA_SRID +
                    "), " +
                    "ngay_cap_nhat = CURRENT_TIMESTAMP " +
                "WHERE id = @id;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                command.Parameters.Add(
                    "@wkt",
                    NpgsqlDbType.Text)
                    .Value =
                    wkt32648;

                command.Parameters.Add(
                    "@id",
                    NpgsqlDbType.Bigint)
                    .Value =
                    Convert.ToInt64(
                        recordId);

                connection.Open();

                int affected =
                    command.ExecuteNonQuery();

                if (affected == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy bản ghi ID = " +
                        recordId +
                        " để lưu hình học.");
                }
            }
        }

        // =========================================================
        // XÓA RIÊNG GEOMETRY, GIỮ THUỘC TÍNH
        //
        // Hàm vẫn được giữ để tương thích code cũ.
        // Với đơn vị quân đội, geometry nên được tạo lại từ xã/phường.
        // =========================================================
        public void ClearGeometry(
            string tableName,
            int recordId)
        {
            if (recordId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordId");
            }

            string geometryType =
                GetGeometryType(
                    tableName);

            if (geometryType.Length == 0)
            {
                throw new InvalidOperationException(
                    "Không hỗ trợ bảng \"" +
                    tableName +
                    "\".");
            }

            string sql =
                "UPDATE " +
                QuoteQualifiedTable(
                    tableName) +
                " SET geom = NULL, " +
                    "ngay_cap_nhat = CURRENT_TIMESTAMP " +
                "WHERE id = @id;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                command.Parameters.Add(
                    "@id",
                    NpgsqlDbType.Bigint)
                    .Value =
                    Convert.ToInt64(
                        recordId);

                connection.Open();

                int affected =
                    command.ExecuteNonQuery();

                if (affected == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy bản ghi ID = " +
                        recordId +
                        " để xóa geometry.");
                }
            }
        }

        // =========================================================
        // BẢN GHI ĐÃ CÓ GEOMETRY HAY CHƯA
        // =========================================================
        public bool HasGeometry(
            string tableName,
            int recordId)
        {
            if (recordId <= 0)
            {
                return false;
            }

            string geometryType =
                GetGeometryType(
                    tableName);

            if (geometryType.Length == 0)
            {
                return false;
            }

            string sql =
                "SELECT " +
                    "geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(geom) " +
                "FROM " +
                QuoteQualifiedTable(
                    tableName) +
                " WHERE id = @id;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                command.Parameters.Add(
                    "@id",
                    NpgsqlDbType.Bigint)
                    .Value =
                    Convert.ToInt64(
                        recordId);

                connection.Open();

                object result =
                    command.ExecuteScalar();

                return
                    result != null &&
                    result != DBNull.Value &&
                    Convert.ToBoolean(
                        result);
            }
        }

        // =========================================================
        // KIỂM TRA GEOMETRY WKT EPSG:32648
        // =========================================================
        private void ValidateGeometry(
            string wkt32648,
            string expectedGeometryType)
        {
            const string sql =
                "WITH g AS (" +
                    "SELECT " +
                        "ST_GeomFromText(@wkt, 32648) AS geom" +
                ") " +
                "SELECT " +
                    "ST_IsEmpty(geom) AS is_empty, " +
                    "GeometryType(geom) AS geometry_type, " +
                    "ST_SRID(geom) AS srid, " +
                    "ST_IsValid(geom) AS is_valid, " +
                    "ST_IsValidReason(geom) AS valid_reason " +
                "FROM g;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                command.Parameters.Add(
                    "@wkt",
                    NpgsqlDbType.Text)
                    .Value =
                    wkt32648;

                connection.Open();

                using (NpgsqlDataReader reader =
                    command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException(
                            "Không kiểm tra được geometry.");
                    }

                    bool isEmpty =
                        reader.GetBoolean(0);

                    string actualType =
                        reader.IsDBNull(1)
                            ? ""
                            : reader.GetString(1);

                    int srid =
                        reader.GetInt32(2);

                    bool isValid =
                        reader.GetBoolean(3);

                    string validReason =
                        reader.IsDBNull(4)
                            ? ""
                            : reader.GetString(4);

                    if (isEmpty)
                    {
                        throw new InvalidOperationException(
                            "Geometry rỗng, không thể lưu.");
                    }

                    if (!string.Equals(
                        actualType,
                        expectedGeometryType,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Sai loại geometry. Cần " +
                            expectedGeometryType +
                            " nhưng nhận được " +
                            actualType +
                            ".");
                    }

                    if (srid != DATA_SRID)
                    {
                        throw new InvalidOperationException(
                            "Sai hệ tọa độ geometry. Cần EPSG:" +
                            DATA_SRID +
                            " nhưng nhận EPSG:" +
                            srid +
                            ".");
                    }

                    if (!isValid)
                    {
                        throw new InvalidOperationException(
                            "Geometry không hợp lệ.\n" +
                            validReason);
                    }
                }
            }
        }

        // =========================================================
        // LOẠI GEOMETRY THEO TABLE
        // =========================================================
        private string GetGeometryType(
            string tableName)
        {
            if (string.Equals(
                tableName,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Point";
            }

            if (string.Equals(
                tableName,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "LineString";
            }

            if (string.Equals(
                tableName,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Polygon";
            }

            return "";
        }

        // =========================================================
        // QUOTE SCHEMA.TABLE
        // =========================================================
        private string QuoteQualifiedTable(
            string tableName)
        {
            return
                QuoteIdentifier(
                    PG_SCHEMA) +
                "." +
                QuoteIdentifier(
                    tableName);
        }

        // =========================================================
        // QUOTE IDENTIFIER
        // =========================================================
        private string QuoteIdentifier(
            string name)
        {
            return
                "\"" +
                (name ?? "")
                    .Replace(
                        "\"",
                        "\"\"") +
                "\"";
        }
    }
}