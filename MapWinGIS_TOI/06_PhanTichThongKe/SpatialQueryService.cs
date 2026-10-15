using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // =============================================================
    // SPATIAL QUERY SERVICE - HÀ NỘI ONLY
    //
    // ĐỀ TÀI:
    // Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    // các đơn vị quân đội trên địa bàn Hà Nội.
    //
    // Mô hình hành chính hiện hành:
    // Thành phố Hà Nội -> Xã/Phường.
    //
    // CRS toàn hệ thống:
    // EPSG:32648.
    //
    // Nhóm truy vấn:
    // 1. Lấy danh sách xã/phường;
    // 2. Tìm đơn vị theo xã/phường bằng Point đại diện hành chính;
    // 3. Kiểm tra Point đại diện có khớp xã/phường đã gán;
    // 4. Kiểm tra chất lượng geometry tuyến;
    // 5. Tìm đối tượng chưa có geometry;
    // 6. Tìm geometry lỗi / sai SRID.
    //
    // Không đọc geometry thô ra giao diện.
    // =============================================================
    public sealed class SpatialQueryService
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
        public SpatialQueryService()
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
                30;

            builder.Pooling =
                true;

            _connectionString =
                builder.ConnectionString;
        }

        // =========================================================
        // DANH SÁCH XÃ / PHƯỜNG HÀ NỘI
        //
        // Giữ tên hàm GetAdministrativeAreas() để không làm vỡ
        // SpatialQueryForm.cs cũ.
        // =========================================================
        public DataTable GetAdministrativeAreas()
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_xa_phuong AS ma_hanh_chinh, " +
                "CASE " +
                    "WHEN loai = 'phuong' " +
                    "THEN 'Phường ' || ten_xa_phuong " +
                    "WHEN loai = 'xa' " +
                    "THEN 'Xã ' || ten_xa_phuong " +
                    "ELSE ten_xa_phuong " +
                "END AS ten_hanh_chinh, " +
                "loai AS cap_hanh_chinh " +
                "FROM " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " WHERE tinh_thanh_pho = 'Thành phố Hà Nội' " +
                "AND geom IS NOT NULL " +
                "AND NOT ST_IsEmpty(geom) " +
                "AND ST_SRID(geom) = " +
                DATA_SRID +
                " ORDER BY " +
                "CASE WHEN loai = 'phuong' THEN 0 ELSE 1 END, " +
                "ten_xa_phuong;";

            return ExecuteDataTable(
                sql,
                "dia_gioi");
        }

        // =========================================================
        // 1. ĐƠN VỊ TRONG XÃ / PHƯỜNG
        //
        // Point đơn vị là Point đại diện hành chính, không phải vị
        // trí cơ sở chính xác.
        //
        // Dùng cả:
        // - khóa ngoại xa_phuong_id;
        // - ST_Covers() để kiểm tra tính nhất quán không gian.
        // =========================================================
        public DataTable GetUnitsInAdministrativeArea(
            int administrativeAreaId)
        {
            if (administrativeAreaId <= 0)
            {
                return CreateEmptyResultTable(
                    "don_vi_trong_dia_gioi");
            }

            string sql =
                "SELECT " +
                "'don_vi_quan_doi'::text AS layer_key, " +
                "dv.id AS object_id, " +
                "dv.ma_don_vi, " +
                "dv.ten_don_vi, " +
                "dv.ky_hieu_bien_so, " +
                "cdv.ten_cap, " +
                "ldv.ten_loai, " +
                "xp.ten_xa_phuong, " +
                "xp.loai AS loai_xa_phuong, " +
                "dv.dia_chi_hien_hanh, " +
                "dv.trang_thai_quan_ly, " +
                "CASE " +
                    "WHEN dv.geom IS NULL OR ST_IsEmpty(dv.geom) " +
                    "THEN 'Chưa có Point đại diện' " +
                    "WHEN ST_Covers(xp.geom, dv.geom) " +
                    "THEN 'Khớp xã/phường' " +
                    "ELSE 'Point không khớp xã/phường' " +
                "END AS ket_qua_khong_gian " +

                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " dv " +

                "JOIN " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " xp " +
                "ON xp.id = dv.xa_phuong_id " +

                "LEFT JOIN " +
                QuoteQualifiedTable(
                    "cap_don_vi") +
                " cdv " +
                "ON cdv.id = dv.cap_don_vi_id " +

                "LEFT JOIN " +
                QuoteQualifiedTable(
                    "loai_don_vi") +
                " ldv " +
                "ON ldv.id = dv.loai_don_vi_id " +

                "WHERE dv.xa_phuong_id = @area_id " +
                "AND xp.geom IS NOT NULL " +
                "AND NOT ST_IsEmpty(xp.geom) " +

                "ORDER BY dv.ten_don_vi;";

            return ExecuteDataTableWithId(
                sql,
                "don_vi_trong_dia_gioi",
                "@area_id",
                administrativeAreaId);
        }

        // =========================================================
        // 2. KIỂM TRA POINT ĐẠI DIỆN CÓ KHỚP XÃ / PHƯỜNG KHÔNG
        //
        // Giữ tên hàm GetUnitBoundaryCheck() để không làm vỡ form
        // cũ, nhưng nghiệp vụ mới là kiểm tra tính nhất quán giữa:
        // xa_phuong_id <-> Point đại diện hành chính.
        // =========================================================
        public DataTable GetUnitBoundaryCheck()
        {
            string sql =
                "SELECT " +
                "'don_vi_quan_doi'::text AS layer_key, " +
                "dv.id AS object_id, " +
                "dv.ma_don_vi, " +
                "dv.ten_don_vi, " +
                "xp.ten_xa_phuong, " +
                "xp.loai AS loai_xa_phuong, " +

                "CASE " +
                    "WHEN dv.geom IS NULL " +
                    "OR ST_IsEmpty(dv.geom) " +
                    "THEN 'Chưa có Point đại diện' " +

                    "WHEN xp.geom IS NULL " +
                    "OR ST_IsEmpty(xp.geom) " +
                    "THEN 'Xã/phường chưa có geometry' " +

                    "WHEN ST_SRID(dv.geom) <> " +
                    DATA_SRID +
                    " THEN 'Point sai SRID' " +

                    "WHEN ST_SRID(xp.geom) <> " +
                    DATA_SRID +
                    " THEN 'Xã/phường sai SRID' " +

                    "WHEN ST_Covers(xp.geom, dv.geom) " +
                    "THEN 'Khớp xã/phường' " +

                    "ELSE 'Point không khớp xã/phường' " +
                "END AS ket_qua, " +

                "CASE " +
                    "WHEN xp.geom IS NULL " +
                    "OR ST_IsEmpty(xp.geom) " +
                    "THEN 0 ELSE 1 " +
                "END AS so_ranh_gioi " +

                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " dv " +

                "LEFT JOIN " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " xp " +
                "ON xp.id = dv.xa_phuong_id " +

                "ORDER BY " +
                "CASE " +
                    "WHEN dv.geom IS NULL " +
                    "OR ST_IsEmpty(dv.geom) " +
                    "THEN 0 " +
                    "WHEN xp.geom IS NULL " +
                    "OR ST_IsEmpty(xp.geom) " +
                    "THEN 1 " +
                    "WHEN ST_SRID(dv.geom) <> " +
                    DATA_SRID +
                    " THEN 2 " +
                    "WHEN ST_SRID(xp.geom) <> " +
                    DATA_SRID +
                    " THEN 3 " +
                    "WHEN ST_Covers(xp.geom, dv.geom) " +
                    "THEN 5 ELSE 4 " +
                "END, " +
                "dv.ten_don_vi;";

            return ExecuteDataTable(
                sql,
                "kiem_tra_point_xa_phuong");
        }

        // =========================================================
        // 3. KIỂM TRA CHẤT LƯỢNG GEOMETRY TUYẾN
        //
        // Giữ tên hàm GetRoutesIntersectingOwnBoundary() để tương
        // thích SpatialQueryForm.cs cũ.
        //
        // Bản mới không phân tích tuyến theo ranh giới cơ sở.
        // Chỉ kiểm tra:
        // - có geometry hay chưa;
        // - geometry có rỗng không;
        // - SRID có phải 32648 không;
        // - geometry có hợp lệ không.
        // =========================================================
        public DataTable GetRoutesIntersectingOwnBoundary()
        {
            string sql =
                "SELECT " +
                "'tuyen_duong_don_vi'::text AS layer_key, " +
                "td.id AS object_id, " +
                "dv.ma_don_vi, " +
                "dv.ten_don_vi, " +
                "td.ma_tuyen, " +
                "td.ten_tuyen, " +
                "td.loai_tuyen, " +

                "CASE " +
                    "WHEN td.geom IS NULL OR ST_IsEmpty(td.geom) " +
                    "THEN NULL " +
                    "ELSE ROUND(ST_Length(td.geom)::numeric, 2) " +
                "END AS chieu_dai_m, " +

                "NULL::text AS ranh_gioi_lien_quan, " +

                "CASE " +
                    "WHEN td.geom IS NULL " +
                    "OR ST_IsEmpty(td.geom) " +
                    "THEN 'Chưa có geometry' " +

                    "WHEN ST_SRID(td.geom) <> " +
                    DATA_SRID +
                    " THEN 'Sai SRID' " +

                    "WHEN NOT ST_IsValid(td.geom) " +
                    "THEN 'Geometry không hợp lệ' " +

                    "ELSE 'Geometry hợp lệ EPSG:32648' " +
                "END AS ket_qua " +

                "FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " td " +

                "LEFT JOIN " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " dv " +
                "ON dv.id = td.don_vi_id " +

                "ORDER BY " +
                "CASE " +
                    "WHEN td.geom IS NULL " +
                    "OR ST_IsEmpty(td.geom) " +
                    "THEN 0 " +
                    "WHEN ST_SRID(td.geom) <> " +
                    DATA_SRID +
                    " THEN 1 " +
                    "WHEN NOT ST_IsValid(td.geom) " +
                    "THEN 2 ELSE 3 " +
                "END, " +
                "dv.ten_don_vi, " +
                "td.ten_tuyen;";

            return ExecuteDataTable(
                sql,
                "kiem_tra_geometry_tuyen");
        }

        // =========================================================
        // 4. ĐỐI TƯỢNG CHƯA CÓ GEOMETRY
        // =========================================================
        public DataTable GetObjectsWithoutGeometry()
        {
            string sql =
                "SELECT " +
                "'don_vi_quan_doi'::text AS layer_key, " +
                "id AS object_id, " +
                "'Đơn vị quân đội'::text AS loai_doi_tuong, " +
                "ma_don_vi AS ma_doi_tuong, " +
                "ten_don_vi AS ten_doi_tuong " +
                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " WHERE geom IS NULL " +
                "OR ST_IsEmpty(geom) " +

                "UNION ALL " +

                "SELECT " +
                "'tuyen_duong_don_vi'::text, " +
                "id, " +
                "'Tuyến đường đơn vị'::text, " +
                "ma_tuyen, " +
                "ten_tuyen " +
                "FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " WHERE geom IS NULL " +
                "OR ST_IsEmpty(geom) " +

                "UNION ALL " +

                "SELECT " +
                "'khu_vuc_don_vi'::text, " +
                "id, " +
                "'Khu vực đơn vị'::text, " +
                "ma_khu_vuc, " +
                "ten_khu_vuc " +
                "FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " WHERE geom IS NULL " +
                "OR ST_IsEmpty(geom) " +

                "ORDER BY " +
                "loai_doi_tuong, " +
                "ten_doi_tuong;";

            return ExecuteDataTable(
                sql,
                "thieu_geometry");
        }

        // =========================================================
        // 5. GEOMETRY KHÔNG HỢP LỆ / SAI SRID
        // =========================================================
        public DataTable GetInvalidGeometries()
        {
            string sql =
                "SELECT " +
                "'don_vi_quan_doi'::text AS layer_key, " +
                "id AS object_id, " +
                "'Đơn vị quân đội'::text AS loai_doi_tuong, " +
                "ma_don_vi AS ma_doi_tuong, " +
                "ten_don_vi AS ten_doi_tuong, " +
                "CASE " +
                    "WHEN ST_IsEmpty(geom) THEN 'Geometry rỗng' " +
                    "WHEN ST_SRID(geom) <> " +
                    DATA_SRID +
                    " THEN 'Sai SRID: ' || ST_SRID(geom)::text " +
                    "WHEN NOT ST_IsValid(geom) " +
                    "THEN ST_IsValidReason(geom) " +
                    "ELSE 'Không xác định' " +
                "END AS ly_do " +
                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " WHERE geom IS NOT NULL " +
                "AND (" +
                    "ST_IsEmpty(geom) " +
                    "OR ST_SRID(geom) <> " +
                    DATA_SRID +
                    " OR NOT ST_IsValid(geom)" +
                ") " +

                "UNION ALL " +

                "SELECT " +
                "'tuyen_duong_don_vi'::text, " +
                "id, " +
                "'Tuyến đường đơn vị'::text, " +
                "ma_tuyen, " +
                "ten_tuyen, " +
                "CASE " +
                    "WHEN ST_IsEmpty(geom) THEN 'Geometry rỗng' " +
                    "WHEN ST_SRID(geom) <> " +
                    DATA_SRID +
                    " THEN 'Sai SRID: ' || ST_SRID(geom)::text " +
                    "WHEN NOT ST_IsValid(geom) " +
                    "THEN ST_IsValidReason(geom) " +
                    "ELSE 'Không xác định' " +
                "END " +
                "FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " WHERE geom IS NOT NULL " +
                "AND (" +
                    "ST_IsEmpty(geom) " +
                    "OR ST_SRID(geom) <> " +
                    DATA_SRID +
                    " OR NOT ST_IsValid(geom)" +
                ") " +

                "UNION ALL " +

                "SELECT " +
                "'khu_vuc_don_vi'::text, " +
                "id, " +
                "'Khu vực đơn vị'::text, " +
                "ma_khu_vuc, " +
                "ten_khu_vuc, " +
                "CASE " +
                    "WHEN ST_IsEmpty(geom) THEN 'Geometry rỗng' " +
                    "WHEN ST_SRID(geom) <> " +
                    DATA_SRID +
                    " THEN 'Sai SRID: ' || ST_SRID(geom)::text " +
                    "WHEN NOT ST_IsValid(geom) " +
                    "THEN ST_IsValidReason(geom) " +
                    "ELSE 'Không xác định' " +
                "END " +
                "FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " WHERE geom IS NOT NULL " +
                "AND (" +
                    "ST_IsEmpty(geom) " +
                    "OR ST_SRID(geom) <> " +
                    DATA_SRID +
                    " OR NOT ST_IsValid(geom)" +
                ") " +

                "ORDER BY " +
                "loai_doi_tuong, " +
                "ten_doi_tuong;";

            return ExecuteDataTable(
                sql,
                "geometry_khong_hop_le");
        }

        // =========================================================
        // PRIVATE - EMPTY TABLE
        // =========================================================
        private DataTable CreateEmptyResultTable(
            string tableName)
        {
            return new DataTable(
                tableName);
        }

        // =========================================================
        // PRIVATE - EXECUTE SELECT
        // =========================================================
        private DataTable ExecuteDataTable(
            string sql,
            string tableName)
        {
            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlDataAdapter adapter =
                new NpgsqlDataAdapter(
                    sql,
                    connection))
            {
                DataTable table =
                    new DataTable(
                        tableName);

                adapter.Fill(
                    table);

                return table;
            }
        }

        // =========================================================
        // PRIVATE - EXECUTE SELECT CÓ ID
        // =========================================================
        private DataTable ExecuteDataTableWithId(
            string sql,
            string tableName,
            string parameterName,
            int id)
        {
            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            using (NpgsqlDataAdapter adapter =
                new NpgsqlDataAdapter(
                    command))
            {
                command.Parameters.Add(
                    parameterName,
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(
                        id);

                DataTable table =
                    new DataTable(
                        tableName);

                adapter.Fill(
                    table);

                return table;
            }
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