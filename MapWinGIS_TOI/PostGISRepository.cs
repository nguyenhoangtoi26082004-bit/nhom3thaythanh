using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // =============================================================
    // ĐỊNH NGHĨA TRƯỜNG DỮ LIỆU
    // =============================================================
    public sealed class PostGISFieldDefinition
    {
        public string ColumnName { get; private set; }
        public string Label { get; private set; }
        public Type DataType { get; private set; }
        public bool Required { get; private set; }

        public PostGISFieldDefinition(
            string columnName,
            string label,
            Type dataType,
            bool required)
        {
            ColumnName = columnName;
            Label = label;
            DataType = dataType;
            Required = required;
        }
    }

    // =============================================================
    // ĐỊNH NGHĨA BẢNG NGHIỆP VỤ
    // =============================================================
    public sealed class PostGISTableDefinition
    {
        public string TableName { get; private set; }
        public string DisplayName { get; private set; }
        public List<PostGISFieldDefinition> EditableFields { get; private set; }

        public PostGISTableDefinition(
            string tableName,
            string displayName,
            IEnumerable<PostGISFieldDefinition> editableFields)
        {
            TableName = tableName;
            DisplayName = displayName;

            EditableFields =
                new List<PostGISFieldDefinition>(
                    editableFields);
        }
    }

    // =============================================================
    // POSTGIS REPOSITORY
    //
    // ĐỀ TÀI:
    // "Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    //  các đơn vị quân đội trên địa bàn Hà Nội"
    //
    // Quản lý 3 bảng nghiệp vụ:
    // 1. don_vi_quan_doi      - Point đại diện hành chính
    // 2. tuyen_duong_don_vi   - LineString
    // 3. khu_vuc_don_vi       - Polygon
    //
    // Dữ liệu nền chỉ đọc trên Map:
    // - v_ban_do_ha_noi
    // - v_ban_do_xa_phuong
    //
    // Mô hình hành chính hiện hành:
    // Thành phố Hà Nội -> Xã/Phường.
    // Không còn Quận/Huyện trong dữ liệu nghiệp vụ.
    //
    // Geometry:
    // - Repository này quản lý thuộc tính;
    // - geometry hiện có được giữ nguyên khi UPDATE;
    // - bản ghi INSERT mới có thể chưa có geometry;
    // - bước Editing.cs sau sẽ đảm nhiệm vẽ/cập nhật geometry.
    // =============================================================
    public sealed class PostGISRepository
    {
        // ---------------------------------------------------------
        // CẤU HÌNH KẾT NỐI
        // ---------------------------------------------------------
        private const string PG_HOST = "localhost";
        private const int PG_PORT = 5433;
        private const string PG_DATABASE = "HQTCSDL";
        private const string PG_USERNAME = "postgres";

        // Đổi lại nếu mật khẩu PostgreSQL của anh khác.
        private const string PG_PASSWORD = "123456";

        private const string PG_SCHEMA = "ql_don_vi";

        private readonly string _connectionString;

        private readonly Dictionary<string, PostGISTableDefinition>
            _definitions;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public PostGISRepository()
        {
            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host = PG_HOST;
            builder.Port = PG_PORT;
            builder.Database = PG_DATABASE;
            builder.Username = PG_USERNAME;
            builder.Password = PG_PASSWORD;
            builder.Timeout = 5;
            builder.CommandTimeout = 20;
            builder.Pooling = true;

            _connectionString =
                builder.ConnectionString;

            _definitions =
                BuildDefinitions();
        }

        // =========================================================
        // TABLE CÓ ĐƯỢC QUẢN LÝ CRUD THUỘC TÍNH KHÔNG
        // =========================================================
        public bool IsManagedTable(
            string tableName)
        {
            return
                !string.IsNullOrWhiteSpace(tableName)
                &&
                _definitions.ContainsKey(tableName);
        }

        // =========================================================
        // LẤY ĐỊNH NGHĨA TABLE
        // =========================================================
        public PostGISTableDefinition GetDefinition(
            string tableName)
        {
            PostGISTableDefinition definition;

            if (!_definitions.TryGetValue(
                tableName ?? "",
                out definition))
            {
                throw new InvalidOperationException(
                    "Bảng \"" +
                    tableName +
                    "\" chưa được cấu hình quản lý trong PostGISRepository.");
            }

            return definition;
        }

        // =========================================================
        // KIỂM TRA KẾT NỐI
        // =========================================================
        public bool TestConnection(
            out string error)
        {
            error = "";

            try
            {
                using (NpgsqlConnection connection =
                    new NpgsqlConnection(
                        _connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command =
                        new NpgsqlCommand(
                            "SELECT 1;",
                            connection))
                    {
                        command.ExecuteScalar();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // =========================================================
        // ĐỌC BẢNG THUỘC TÍNH MỚI NHẤT TỪ POSTGIS
        //
        // Các trường hình học không đưa lên DataGridView.
        // Chiều dài/diện tích/chu vi được tính trực tiếp bằng PostGIS.
        // =========================================================
        public DataTable GetTableData(
            string tableName)
        {
            PostGISTableDefinition definition =
                GetDefinition(tableName);

            string sql =
                BuildSelectAttributeSql(
                    definition.TableName);

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
                        definition.TableName);

                adapter.Fill(
                    table);

                return table;
            }
        }

        // =========================================================
        // ĐỌC MỘT BẢN GHI GỐC ĐỂ SỬA
        // =========================================================
        public Dictionary<string, object> GetRecord(
            string tableName,
            int id)
        {
            PostGISTableDefinition definition =
                GetDefinition(tableName);

            string sql =
                "SELECT " +
                BuildEditableColumnList(
                    definition) +
                " FROM " +
                QuoteQualifiedTable(
                    definition.TableName) +
                " WHERE id = @id " +
                "LIMIT 1;";

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
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(id);

                connection.Open();

                using (NpgsqlDataReader reader =
                    command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    Dictionary<string, object> values =
                        new Dictionary<string, object>(
                            StringComparer.OrdinalIgnoreCase);

                    foreach (PostGISFieldDefinition field
                        in definition.EditableFields)
                    {
                        int ordinal =
                            reader.GetOrdinal(
                                field.ColumnName);

                        values[field.ColumnName] =
                            reader.IsDBNull(ordinal)
                                ? null
                                : reader.GetValue(ordinal);
                    }

                    return values;
                }
            }
        }

        // =========================================================
        // THÊM BẢN GHI THUỘC TÍNH
        //
        // Không tự tạo geometry.
        // =========================================================
        public int InsertRecord(
            string tableName,
            IDictionary<string, object> values)
        {
            PostGISTableDefinition definition =
                GetDefinition(tableName);

            ValidateRequiredFields(
                definition,
                values);

            List<string> columns =
                new List<string>();

            List<string> parameters =
                new List<string>();

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand())
            {
                command.Connection =
                    connection;

                foreach (PostGISFieldDefinition field
                    in definition.EditableFields)
                {
                    object value =
                        GetDictionaryValue(
                            values,
                            field.ColumnName);

                    columns.Add(
                        QuoteIdentifier(
                            field.ColumnName));

                    string parameterName =
                        "@p_" +
                        field.ColumnName;

                    parameters.Add(
                        parameterName);

                    AddTypedParameter(
                        command,
                        parameterName,
                        field,
                        value);
                }

                command.CommandText =
                    "INSERT INTO " +
                    QuoteQualifiedTable(
                        definition.TableName) +
                    " (" +
                    string.Join(
                        ", ",
                        columns.ToArray()) +
                    ") VALUES (" +
                    string.Join(
                        ", ",
                        parameters.ToArray()) +
                    ") " +
                    "RETURNING id;";

                connection.Open();

                object result =
                    command.ExecuteScalar();

                return Convert.ToInt32(
                    result);
            }
        }

        // =========================================================
        // SỬA THUỘC TÍNH
        //
        // Geometry không bị thay đổi.
        // Trigger DB tự cập nhật ngay_cap_nhat.
        // =========================================================
        public void UpdateRecord(
            string tableName,
            int id,
            IDictionary<string, object> values)
        {
            PostGISTableDefinition definition =
                GetDefinition(tableName);

            ValidateRequiredFields(
                definition,
                values);

            List<string> assignments =
                new List<string>();

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand())
            {
                command.Connection =
                    connection;

                foreach (PostGISFieldDefinition field
                    in definition.EditableFields)
                {
                    object value =
                        GetDictionaryValue(
                            values,
                            field.ColumnName);

                    string parameterName =
                        "@p_" +
                        field.ColumnName;

                    assignments.Add(
                        QuoteIdentifier(
                            field.ColumnName) +
                        " = " +
                        parameterName);

                    AddTypedParameter(
                        command,
                        parameterName,
                        field,
                        value);
                }

                command.Parameters.Add(
                    "@id",
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(id);

                command.CommandText =
                    "UPDATE " +
                    QuoteQualifiedTable(
                        definition.TableName) +
                    " SET " +
                    string.Join(
                        ", ",
                        assignments.ToArray()) +
                    " WHERE id = @id;";

                connection.Open();

                int affected =
                    command.ExecuteNonQuery();

                if (affected == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy bản ghi ID = " +
                        id +
                        " để cập nhật.");
                }
            }
        }

        // =========================================================
        // XÓA BẢN GHI
        // =========================================================
        public void DeleteRecord(
            string tableName,
            int id)
        {
            PostGISTableDefinition definition =
                GetDefinition(tableName);

            string sql =
                "DELETE FROM " +
                QuoteQualifiedTable(
                    definition.TableName) +
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
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(id);

                connection.Open();

                int affected =
                    command.ExecuteNonQuery();

                if (affected == 0)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy bản ghi ID = " +
                        id +
                        " để xóa.");
                }
            }
        }

        // =========================================================
        // DANH MỤC CẤP ĐƠN VỊ
        // =========================================================
        public DataTable GetCapDonViData()
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_cap, " +
                "ten_cap " +
                "FROM " +
                QuoteQualifiedTable(
                    "cap_don_vi") +
                " ORDER BY " +
                "thu_tu_hien_thi, " +
                "ten_cap;";

            return ExecuteDataTable(
                sql,
                "cap_don_vi");
        }

        // =========================================================
        // DANH MỤC LOẠI ĐƠN VỊ
        // =========================================================
        public DataTable GetLoaiDonViData()
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_loai, " +
                "ten_loai " +
                "FROM " +
                QuoteQualifiedTable(
                    "loai_don_vi") +
                " ORDER BY ten_loai;";

            return ExecuteDataTable(
                sql,
                "loai_don_vi");
        }

        // =========================================================
        // DANH MỤC 126 XÃ / PHƯỜNG HÀ NỘI
        //
        // Dùng cho ComboBox khi thêm/sửa đơn vị.
        // =========================================================
        public DataTable GetXaPhuongData()
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_xa_phuong, " +
                "ten_xa_phuong, " +
                "loai, " +
                "CASE " +
                "WHEN loai = 'phuong' THEN 'Phường ' || ten_xa_phuong " +
                "ELSE 'Xã ' || ten_xa_phuong " +
                "END AS ten_hien_thi " +
                "FROM " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " WHERE tinh_thanh_pho = 'Thành phố Hà Nội' " +
                " ORDER BY loai, ten_xa_phuong;";

            return ExecuteDataTable(
                sql,
                "xa_phuong");
        }

        // =========================================================
        // DANH SÁCH ĐƠN VỊ
        //
        // Dùng cho ComboBox khi quản lý tuyến đường/khu vực.
        // =========================================================
        public DataTable GetDonViData()
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_don_vi, " +
                "ten_don_vi " +
                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " ORDER BY ten_don_vi;";

            return ExecuteDataTable(
                sql,
                "don_vi");
        }

        // =========================================================
        // DANH SÁCH ĐƠN VỊ CẤP TRÊN
        // =========================================================
        public DataTable GetDonViCapTrenData(
            int? excludeId)
        {
            string sql =
                "SELECT " +
                "id, " +
                "ma_don_vi, " +
                "ten_don_vi " +
                "FROM " +
                QuoteQualifiedTable(
                    "don_vi_quan_doi") +
                " WHERE (@exclude_id IS NULL OR id <> @exclude_id) " +
                " ORDER BY ten_don_vi;";

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
                NpgsqlParameter parameter =
                    command.Parameters.Add(
                        "@exclude_id",
                        NpgsqlDbType.Bigint);

                parameter.Value =
                    excludeId.HasValue
                        ? (object)Convert.ToInt64(
                            excludeId.Value)
                        : DBNull.Value;

                DataTable table =
                    new DataTable(
                        "don_vi_cap_tren");

                adapter.Fill(
                    table);

                return table;
            }
        }

        // =========================================================
        // DANH MỤC LOẠI TUYẾN
        // =========================================================
        public DataTable GetLoaiTuyenData()
        {
            DataTable table =
                new DataTable(
                    "loai_tuyen");

            table.Columns.Add(
                "ma_loai",
                typeof(string));

            table.Columns.Add(
                "ten_loai",
                typeof(string));

            table.Rows.Add(
                "duong_vao",
                "Đường vào đơn vị");

            table.Rows.Add(
                "duong_tuan_tra",
                "Đường tuần tra");

            table.Rows.Add(
                "khac",
                "Khác");

            return table;
        }

        // =========================================================
        // DANH MỤC LOẠI KHU VỰC
        // =========================================================
        public DataTable GetLoaiKhuVucData()
        {
            DataTable table =
                new DataTable(
                    "loai_khu_vuc");

            table.Columns.Add(
                "ma_loai",
                typeof(string));

            table.Columns.Add(
                "ten_loai",
                typeof(string));

            table.Rows.Add(
                "ranh_gioi",
                "Ranh giới đơn vị");

            table.Rows.Add(
                "khu_chuc_nang",
                "Khu chức năng");

            table.Rows.Add(
                "khu_ky_thuat",
                "Khu kỹ thuật");

            table.Rows.Add(
                "khu_hau_can",
                "Khu hậu cần");

            table.Rows.Add(
                "khac",
                "Khác");

            return table;
        }

        // =========================================================
        // TRA CỨU NHANH ĐƠN VỊ - HÀ NỘI ONLY
        // =========================================================
        public DataTable SearchDonVi(
            string keyword)
        {
            string sql =
                "SELECT " +
                "v.id, " +
                "v.ma_don_vi, " +
                "v.ten_don_vi, " +
                "v.ky_hieu_bien_so, " +
                "v.ten_cap, " +
                "v.ten_loai, " +
                "v.ten_don_vi_cap_tren, " +
                "v.tinh_thanh_pho, " +
                "v.ten_xa_phuong, " +
                "v.loai_xa_phuong, " +
                "v.dia_chi_hien_hanh, " +
                "v.thong_tin_lien_he, " +
                "v.nguon_tham_khao, " +
                "v.ngay_xac_minh, " +
                "v.muc_do_xac_minh, " +
                "v.trang_thai_quan_ly " +
                "FROM " +
                QuoteQualifiedTable(
                    "v_don_vi_tra_cuu") +
                " v " +
                "WHERE " +
                "COALESCE(v.ma_don_vi, '') ILIKE @keyword " +
                "OR COALESCE(v.ten_don_vi, '') ILIKE @keyword " +
                "OR COALESCE(v.ky_hieu_bien_so, '') ILIKE @keyword " +
                "OR COALESCE(v.ten_cap, '') ILIKE @keyword " +
                "OR COALESCE(v.ten_loai, '') ILIKE @keyword " +
                "OR COALESCE(v.ten_don_vi_cap_tren, '') ILIKE @keyword " +
                "OR COALESCE(v.ten_xa_phuong, '') ILIKE @keyword " +
                "OR COALESCE(v.dia_chi_hien_hanh, '') ILIKE @keyword " +
                "OR COALESCE(v.nguon_tham_khao, '') ILIKE @keyword " +
                "ORDER BY v.ten_don_vi;";

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
                    "@keyword",
                    NpgsqlDbType.Text).Value =
                    "%" +
                    (keyword ?? "").Trim() +
                    "%";

                DataTable table =
                    new DataTable(
                        "ket_qua_tra_cuu");

                adapter.Fill(
                    table);

                return table;
            }
        }

        // =========================================================
        // PRIVATE - DEFINITIONS
        // =========================================================
        private Dictionary<string, PostGISTableDefinition>
            BuildDefinitions()
        {
            Dictionary<string, PostGISTableDefinition> result =
                new Dictionary<string, PostGISTableDefinition>(
                    StringComparer.OrdinalIgnoreCase);

            // =====================================================
            // 1. ĐƠN VỊ QUÂN ĐỘI - POINT
            //
            // Không đưa tinh_thanh_pho vào form sửa vì dự án chỉ
            // quản lý Thành phố Hà Nội; DB đã có DEFAULT cố định.
            // ngay_xac_minh được DB tự gán CURRENT_DATE khi thêm mới.
            // =====================================================
            result["don_vi_quan_doi"] =
                new PostGISTableDefinition(
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    new PostGISFieldDefinition[]
                    {
                        new PostGISFieldDefinition(
                            "ma_don_vi",
                            "Mã đơn vị",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "ten_don_vi",
                            "Tên đơn vị",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "ten_viet_tat",
                            "Tên viết tắt",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "cap_don_vi_id",
                            "Cấp đơn vị",
                            typeof(int),
                            true),

                        new PostGISFieldDefinition(
                            "loai_don_vi_id",
                            "Loại đơn vị",
                            typeof(int),
                            true),

                        new PostGISFieldDefinition(
                            "don_vi_cap_tren_id",
                            "Đơn vị cấp trên",
                            typeof(int),
                            false),

                        new PostGISFieldDefinition(
                            "xa_phuong_id",
                            "Xã/Phường",
                            typeof(int),
                            true),

                        new PostGISFieldDefinition(
                            "dia_chi_hien_hanh",
                            "Địa chỉ hiện hành",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "ky_hieu_bien_so",
                            "Ký hiệu",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "thong_tin_lien_he",
                            "Thông tin liên hệ",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "nguon_tham_khao",
                            "Nguồn tham khảo",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "muc_do_xac_minh",
                            "Mức độ xác minh",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "mo_ta",
                            "Mô tả",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "ghi_chu",
                            "Ghi chú",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "trang_thai_quan_ly",
                            "Trạng thái quản lý",
                            typeof(string),
                            false)
                    });

            // =====================================================
            // 2. TUYẾN ĐƯỜNG ĐƠN VỊ - LINESTRING
            // =====================================================
            result["tuyen_duong_don_vi"] =
                new PostGISTableDefinition(
                    "tuyen_duong_don_vi",
                    "Tuyến đường đơn vị",
                    new PostGISFieldDefinition[]
                    {
                        new PostGISFieldDefinition(
                            "don_vi_id",
                            "Đơn vị",
                            typeof(int),
                            true),

                        new PostGISFieldDefinition(
                            "ma_tuyen",
                            "Mã tuyến",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "ten_tuyen",
                            "Tên tuyến",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "loai_tuyen",
                            "Loại tuyến",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "cap_duong",
                            "Cấp đường",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "be_rong_m",
                            "Bề rộng (m)",
                            typeof(double),
                            false),

                        new PostGISFieldDefinition(
                            "mo_ta",
                            "Mô tả",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "ghi_chu",
                            "Ghi chú",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "trang_thai_quan_ly",
                            "Trạng thái quản lý",
                            typeof(string),
                            false)
                    });

            // =====================================================
            // 3. KHU VỰC ĐƠN VỊ - POLYGON
            // =====================================================
            result["khu_vuc_don_vi"] =
                new PostGISTableDefinition(
                    "khu_vuc_don_vi",
                    "Khu vực đơn vị",
                    new PostGISFieldDefinition[]
                    {
                        new PostGISFieldDefinition(
                            "don_vi_id",
                            "Đơn vị",
                            typeof(int),
                            true),

                        new PostGISFieldDefinition(
                            "ma_khu_vuc",
                            "Mã khu vực",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "ten_khu_vuc",
                            "Tên khu vực",
                            typeof(string),
                            true),

                        new PostGISFieldDefinition(
                            "loai_khu_vuc",
                            "Loại khu vực",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "mo_ta",
                            "Mô tả",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "ghi_chu",
                            "Ghi chú",
                            typeof(string),
                            false),

                        new PostGISFieldDefinition(
                            "trang_thai_quan_ly",
                            "Trạng thái quản lý",
                            typeof(string),
                            false)
                    });

            return result;
        }

        // =========================================================
        // SELECT BẢNG THUỘC TÍNH
        // =========================================================
        private string BuildSelectAttributeSql(
            string tableName)
        {
            // -----------------------------------------------------
            // 1. ĐƠN VỊ QUÂN ĐỘI
            // -----------------------------------------------------
            if (string.Equals(
                tableName,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "dv.id, " +
                    "dv.ma_don_vi, " +
                    "dv.ten_don_vi, " +
                    "dv.ten_viet_tat, " +
                    "dv.ky_hieu_bien_so, " +
                    "cdv.ten_cap, " +
                    "ldv.ten_loai, " +
                    "captren.ten_don_vi AS ten_don_vi_cap_tren, " +
                    "dv.tinh_thanh_pho, " +
                    "xp.ma_xa_phuong, " +
                    "xp.ten_xa_phuong, " +
                    "xp.loai AS loai_xa_phuong, " +
                    "dv.dia_chi_hien_hanh, " +
                    "dv.thong_tin_lien_he, " +
                    "dv.nguon_tham_khao, " +
                    "dv.ngay_xac_minh, " +
                    "dv.muc_do_xac_minh, " +
                    "dv.mo_ta, " +
                    "dv.ghi_chu, " +
                    "dv.trang_thai_quan_ly, " +
                    "dv.ngay_tao, " +
                    "dv.ngay_cap_nhat " +

                    "FROM " +
                    QuoteQualifiedTable(
                        "don_vi_quan_doi") +
                    " dv " +

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

                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        "don_vi_quan_doi") +
                    " captren " +
                    "ON captren.id = dv.don_vi_cap_tren_id " +

                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        "dm_xa_phuong") +
                    " xp " +
                    "ON xp.id = dv.xa_phuong_id " +

                    "ORDER BY dv.id;";
            }

            // -----------------------------------------------------
            // 2. TUYẾN ĐƯỜNG ĐƠN VỊ
            // -----------------------------------------------------
            if (string.Equals(
                tableName,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "td.id, " +
                    "td.don_vi_id, " +
                    "dv.ma_don_vi, " +
                    "dv.ten_don_vi, " +
                    "td.ma_tuyen, " +
                    "td.ten_tuyen, " +
                    "td.loai_tuyen, " +
                    "td.cap_duong, " +
                    "td.be_rong_m, " +
                    "ROUND(ST_Length(td.geom)::numeric, 2) AS chieu_dai_m, " +
                    "td.mo_ta, " +
                    "td.ghi_chu, " +
                    "td.trang_thai_quan_ly, " +
                    "td.ngay_tao, " +
                    "td.ngay_cap_nhat " +

                    "FROM " +
                    QuoteQualifiedTable(
                        "tuyen_duong_don_vi") +
                    " td " +

                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        "don_vi_quan_doi") +
                    " dv " +
                    "ON dv.id = td.don_vi_id " +

                    "ORDER BY td.id;";
            }

            // -----------------------------------------------------
            // 3. KHU VỰC ĐƠN VỊ
            // -----------------------------------------------------
            if (string.Equals(
                tableName,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "kv.id, " +
                    "kv.don_vi_id, " +
                    "dv.ma_don_vi, " +
                    "dv.ten_don_vi, " +
                    "kv.ma_khu_vuc, " +
                    "kv.ten_khu_vuc, " +
                    "kv.loai_khu_vuc, " +
                    "ROUND(ST_Area(kv.geom)::numeric, 2) AS dien_tich_m2, " +
                    "ROUND(ST_Perimeter(kv.geom)::numeric, 2) AS chu_vi_m, " +
                    "kv.mo_ta, " +
                    "kv.ghi_chu, " +
                    "kv.trang_thai_quan_ly, " +
                    "kv.ngay_tao, " +
                    "kv.ngay_cap_nhat " +

                    "FROM " +
                    QuoteQualifiedTable(
                        "khu_vuc_don_vi") +
                    " kv " +

                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        "don_vi_quan_doi") +
                    " dv " +
                    "ON dv.id = kv.don_vi_id " +

                    "ORDER BY kv.id;";
            }

            throw new InvalidOperationException(
                "Không hỗ trợ bảng: " +
                tableName);
        }

        // =========================================================
        // BUILD DANH SÁCH CỘT EDIT
        // =========================================================
        private string BuildEditableColumnList(
            PostGISTableDefinition definition)
        {
            List<string> fields =
                new List<string>();

            foreach (PostGISFieldDefinition field
                in definition.EditableFields)
            {
                fields.Add(
                    QuoteIdentifier(
                        field.ColumnName));
            }

            return string.Join(
                ", ",
                fields.ToArray());
        }

        // =========================================================
        // KIỂM TRA TRƯỜNG BẮT BUỘC
        // =========================================================
        private void ValidateRequiredFields(
            PostGISTableDefinition definition,
            IDictionary<string, object> values)
        {
            foreach (PostGISFieldDefinition field
                in definition.EditableFields)
            {
                if (!field.Required)
                    continue;

                object value =
                    GetDictionaryValue(
                        values,
                        field.ColumnName);

                if (value == null ||
                    value == DBNull.Value ||
                    string.IsNullOrWhiteSpace(
                        value.ToString()))
                {
                    throw new InvalidOperationException(
                        "Trường \"" +
                        field.Label +
                        "\" không được để trống.");
                }
            }
        }

        // =========================================================
        // ĐỌC VALUE TỪ DICTIONARY
        // =========================================================
        private object GetDictionaryValue(
            IDictionary<string, object> values,
            string key)
        {
            if (values == null)
                return null;

            object value;

            if (!values.TryGetValue(
                key,
                out value))
            {
                return null;
            }

            return value;
        }

        // =========================================================
        // ADD PARAMETER CÓ KIỂU RÕ RÀNG
        //
        // Tránh lỗi Npgsql khi value = DBNull.Value.
        // =========================================================
        private void AddTypedParameter(
            NpgsqlCommand command,
            string parameterName,
            PostGISFieldDefinition field,
            object value)
        {
            object normalizedValue =
                NormalizeDatabaseValue(
                    field,
                    value);

            NpgsqlDbType dbType =
                GetNpgsqlDbType(
                    field);

            NpgsqlParameter parameter =
                command.Parameters.Add(
                    parameterName,
                    dbType);

            if (normalizedValue == null ||
                normalizedValue == DBNull.Value)
            {
                parameter.Value =
                    DBNull.Value;

                return;
            }

            if (dbType ==
                NpgsqlDbType.Bigint)
            {
                parameter.Value =
                    Convert.ToInt64(
                        normalizedValue);

                return;
            }

            if (dbType ==
                NpgsqlDbType.Integer)
            {
                parameter.Value =
                    Convert.ToInt32(
                        normalizedValue);

                return;
            }

            if (dbType ==
                NpgsqlDbType.Double)
            {
                parameter.Value =
                    Convert.ToDouble(
                        normalizedValue);

                return;
            }

            parameter.Value =
                normalizedValue.ToString();
        }

        // =========================================================
        // XÁC ĐỊNH KIỂU NPGSQL
        // =========================================================
        private NpgsqlDbType GetNpgsqlDbType(
            PostGISFieldDefinition field)
        {
            if (field == null)
                return NpgsqlDbType.Text;

            // Tất cả khóa ngoại *_id của schema ql_don_vi
            // tham chiếu các cột BIGSERIAL/BIGINT.
            if (field.ColumnName.EndsWith(
                "_id",
                StringComparison.OrdinalIgnoreCase))
            {
                return NpgsqlDbType.Bigint;
            }

            if (field.DataType ==
                typeof(int))
            {
                return NpgsqlDbType.Integer;
            }

            if (field.DataType ==
                typeof(double))
            {
                return NpgsqlDbType.Double;
            }

            return NpgsqlDbType.Text;
        }

        // =========================================================
        // CHUẨN HÓA GIÁ TRỊ TRƯỚC KHI GHI DB
        //
        // Chỉ ID tùy chọn = 0 mới đổi thành NULL.
        // Số 0 của bề rộng/giá trị số khác vẫn được giữ.
        // =========================================================
        private object NormalizeDatabaseValue(
            PostGISFieldDefinition field,
            object value)
        {
            if (field == null)
                return value ?? DBNull.Value;

            if (value == null ||
                value == DBNull.Value ||
                string.IsNullOrWhiteSpace(
                    value.ToString()))
            {
                // FK tùy chọn duy nhất của đơn vị.
                if (string.Equals(
                    field.ColumnName,
                    "don_vi_cap_tren_id",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return DBNull.Value;
                }

                // Các cột NOT NULL có giá trị mặc định nghiệp vụ.
                if (string.Equals(
                    field.ColumnName,
                    "dia_chi_hien_hanh",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "CHƯA XÁC MINH";
                }

                if (string.Equals(
                    field.ColumnName,
                    "ky_hieu_bien_so",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "CHƯA XÁC MINH";
                }

                if (string.Equals(
                    field.ColumnName,
                    "thong_tin_lien_he",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "Nguồn công khai";
                }

                if (string.Equals(
                    field.ColumnName,
                    "nguon_tham_khao",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "Nguồn công khai";
                }

                if (string.Equals(
                    field.ColumnName,
                    "muc_do_xac_minh",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "tham_chieu_hanh_chinh";
                }

                if (string.Equals(
                    field.ColumnName,
                    "trang_thai_quan_ly",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "dang_quan_ly";
                }

                if (string.Equals(
                    field.ColumnName,
                    "loai_tuyen",
                    StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                    field.ColumnName,
                    "loai_khu_vuc",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "khac";
                }

                if (string.Equals(
                    field.ColumnName,
                    "cap_duong",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "CHƯA XÁC MINH";
                }

                if (string.Equals(
                    field.ColumnName,
                    "be_rong_m",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return 0.0;
                }

                if (string.Equals(
                    field.ColumnName,
                    "mo_ta",
                    StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                    field.ColumnName,
                    "ghi_chu",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return "Không có";
                }

                return DBNull.Value;
            }

            if (field.ColumnName.EndsWith(
                "_id",
                StringComparison.OrdinalIgnoreCase))
            {
                long idValue;

                if (long.TryParse(
                    value.ToString(),
                    out idValue) &&
                    idValue == 0)
                {
                    return DBNull.Value;
                }
            }

            return value;
        }

        // =========================================================
        // THỰC THI SELECT -> DATATABLE
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