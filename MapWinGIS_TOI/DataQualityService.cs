using System;
using System.Collections.Generic;
using Npgsql;

namespace MapWinGIS_TOI
{
    // =============================================================
    // DATAQUALITYSERVICE.CS
    //
    // Kiểm tra chất lượng CSDL GIS cho đề tài:
    // "Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    //  các đơn vị quân đội trên địa bàn Hà Nội"
    //
    // Kiểm tra:
    // - kết nối PostgreSQL/PostGIS;
    // - tồn tại các bảng/view bắt buộc;
    // - số lượng bản ghi;
    // - SRID = 32648;
    // - geometry NULL / rỗng / không hợp lệ / sai kiểu;
    // - trùng mã đơn vị;
    // - thiếu dữ liệu bắt buộc;
    // - mã xã/phường không tồn tại;
    // - Point đơn vị nằm sai xã/phường;
    // - tuyến/khu vực tham chiếu đơn vị không tồn tại.
    // =============================================================

    public sealed class DataQualityIssue
    {
        public string Severity { get; set; }
        public string CheckName { get; set; }
        public string LayerKey { get; set; }
        public string LayerName { get; set; }
        public int? ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string Message { get; set; }

        public bool CanZoom
        {
            get
            {
                return
                    ObjectId.HasValue &&
                    !string.IsNullOrWhiteSpace(
                        LayerKey);
            }
        }
    }


    public sealed class DataQualityReport
    {
        public List<DataQualityIssue> Items
        {
            get;
            private set;
        }

        public DateTime ExecutedAt
        {
            get;
            set;
        }

        public DataQualityReport()
        {
            Items =
                new List<DataQualityIssue>();

            ExecutedAt =
                DateTime.Now;
        }

        public int ErrorCount
        {
            get
            {
                return CountSeverity(
                    "Lỗi");
            }
        }

        public int WarningCount
        {
            get
            {
                return CountSeverity(
                    "Cảnh báo");
            }
        }

        public int PassCount
        {
            get
            {
                return CountSeverity(
                    "Đạt");
            }
        }

        private int CountSeverity(
            string severity)
        {
            int count = 0;

            foreach (DataQualityIssue item
                in Items)
            {
                if (item != null &&
                    string.Equals(
                        item.Severity,
                        severity,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
    }


    public sealed class DataQualityService
    {
        // ---------------------------------------------------------
        // Giữ đồng bộ với cấu hình hiện tại của project.
        // ---------------------------------------------------------
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

        private const int EXPECTED_SRID =
            32648;

        private readonly string
            _connectionString;


        public DataQualityService()
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
        // CHẠY TOÀN BỘ KIỂM TRA
        // =========================================================
        public DataQualityReport RunAllChecks()
        {
            DataQualityReport report =
                new DataQualityReport();

            try
            {
                using (NpgsqlConnection connection =
                    new NpgsqlConnection(
                        _connectionString))
                {
                    connection.Open();

                    AddPass(
                        report,
                        "Kết nối CSDL",
                        "",
                        "PostgreSQL/PostGIS",
                        "Kết nối PostgreSQL thành công.");

                    CheckPostGIS(
                        connection,
                        report);

                    Dictionary<string, bool> exists =
                        CheckRequiredObjects(
                            connection,
                            report);

                    CheckRecordCounts(
                        connection,
                        report,
                        exists);

                    CheckGeometryQuality(
                        connection,
                        report,
                        exists);

                    CheckUnitBusinessData(
                        connection,
                        report,
                        exists);

                    CheckSpatialAdministrativeRelation(
                        connection,
                        report,
                        exists);

                    CheckForeignReferences(
                        connection,
                        report,
                        exists);
                }
            }
            catch (Exception ex)
            {
                AddIssue(
                    report,
                    "Lỗi",
                    "Kết nối CSDL",
                    "",
                    "PostgreSQL/PostGIS",
                    null,
                    "",
                    "Không thể kết nối hoặc chạy kiểm tra: " +
                    ex.Message);
            }

            report.ExecutedAt =
                DateTime.Now;

            return report;
        }


        // =========================================================
        // POSTGIS
        // =========================================================
        private void CheckPostGIS(
            NpgsqlConnection connection,
            DataQualityReport report)
        {
            try
            {
                object value =
                    ExecuteScalar(
                        connection,
                        "SELECT PostGIS_Version();");

                string version =
                    value == null ||
                    value == DBNull.Value
                        ? ""
                        : value.ToString();

                if (string.IsNullOrWhiteSpace(
                    version))
                {
                    AddIssue(
                        report,
                        "Lỗi",
                        "PostGIS",
                        "",
                        "PostGIS",
                        null,
                        "",
                        "Không đọc được phiên bản PostGIS.");
                }
                else
                {
                    AddPass(
                        report,
                        "PostGIS",
                        "",
                        "PostGIS",
                        "PostGIS hoạt động. Phiên bản: " +
                        version);
                }
            }
            catch (Exception ex)
            {
                AddIssue(
                    report,
                    "Lỗi",
                    "PostGIS",
                    "",
                    "PostGIS",
                    null,
                    "",
                    "Không gọi được PostGIS_Version(): " +
                    ex.Message);
            }
        }


        // =========================================================
        // OBJECT BẮT BUỘC
        // =========================================================
        private Dictionary<string, bool>
            CheckRequiredObjects(
                NpgsqlConnection connection,
                DataQualityReport report)
        {
            Dictionary<string, bool> exists =
                new Dictionary<string, bool>(
                    StringComparer.OrdinalIgnoreCase);

            CheckObject(
                connection,
                report,
                exists,
                "v_ban_do_ha_noi",
                "dia_gioi_hanh_chinh",
                "Ranh giới Thành phố Hà Nội");

            CheckObject(
                connection,
                report,
                exists,
                "v_ban_do_xa_phuong",
                "xa_phuong_ha_noi",
                "Xã, phường Hà Nội");

            CheckObject(
                connection,
                report,
                exists,
                "don_vi_quan_doi",
                "don_vi_quan_doi",
                "Đơn vị quân đội");

            CheckObject(
                connection,
                report,
                exists,
                "v_don_vi_ban_do",
                "don_vi_quan_doi",
                "View bản đồ đơn vị");

            CheckObject(
                connection,
                report,
                exists,
                "tuyen_duong_don_vi",
                "tuyen_duong_don_vi",
                "Tuyến đường đơn vị");

            CheckObject(
                connection,
                report,
                exists,
                "khu_vuc_don_vi",
                "khu_vuc_don_vi",
                "Khu vực đơn vị");

            return exists;
        }


        private void CheckObject(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists,
            string objectName,
            string layerKey,
            string layerName)
        {
            bool ok = false;

            try
            {
                using (NpgsqlCommand command =
                    new NpgsqlCommand(
                        "SELECT to_regclass(@p_name) IS NOT NULL;",
                        connection))
                {
                    command.Parameters.AddWithValue(
                        "p_name",
                        PG_SCHEMA +
                        "." +
                        objectName);

                    object value =
                        command.ExecuteScalar();

                    ok =
                        value != null &&
                        value != DBNull.Value &&
                        Convert.ToBoolean(
                            value);
                }
            }
            catch
            {
                ok = false;
            }

            exists[objectName] =
                ok;

            if (ok)
            {
                AddPass(
                    report,
                    "Cấu trúc CSDL",
                    layerKey,
                    layerName,
                    "Đã có " +
                    PG_SCHEMA +
                    "." +
                    objectName +
                    ".");
            }
            else
            {
                AddIssue(
                    report,
                    "Lỗi",
                    "Cấu trúc CSDL",
                    layerKey,
                    layerName,
                    null,
                    "",
                    "Thiếu object bắt buộc: " +
                    PG_SCHEMA +
                    "." +
                    objectName +
                    ".");
            }
        }


        // =========================================================
        // SỐ LƯỢNG BẢN GHI
        // =========================================================
        private void CheckRecordCounts(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (Exists(
                exists,
                "v_ban_do_ha_noi"))
            {
                int count =
                    GetObjectCount(
                        connection,
                        "v_ban_do_ha_noi");

                if (count == 1)
                {
                    AddPass(
                        report,
                        "Số lượng dữ liệu",
                        "dia_gioi_hanh_chinh",
                        "Ranh giới Thành phố Hà Nội",
                        "Có đúng 1 đối tượng ranh giới Hà Nội.");
                }
                else
                {
                    AddIssue(
                        report,
                        "Cảnh báo",
                        "Số lượng dữ liệu",
                        "dia_gioi_hanh_chinh",
                        "Ranh giới Thành phố Hà Nội",
                        null,
                        "",
                        "Số đối tượng ranh giới Hà Nội hiện là " +
                        count +
                        ", kỳ vọng 1.");
                }
            }

            if (Exists(
                exists,
                "v_ban_do_xa_phuong"))
            {
                int count =
                    GetObjectCount(
                        connection,
                        "v_ban_do_xa_phuong");

                if (count == 126)
                {
                    AddPass(
                        report,
                        "Số lượng dữ liệu",
                        "xa_phuong_ha_noi",
                        "Xã, phường Hà Nội",
                        "Có đủ 126 xã/phường theo bộ dữ liệu hiện tại.");
                }
                else
                {
                    AddIssue(
                        report,
                        "Cảnh báo",
                        "Số lượng dữ liệu",
                        "xa_phuong_ha_noi",
                        "Xã, phường Hà Nội",
                        null,
                        "",
                        "Hiện có " +
                        count +
                        " xã/phường; bộ dữ liệu hiện tại của dự án kỳ vọng 126.");
                }
            }

            AddCountInformation(
                connection,
                report,
                exists,
                "don_vi_quan_doi",
                "don_vi_quan_doi",
                "Đơn vị quân đội");

            AddCountInformation(
                connection,
                report,
                exists,
                "tuyen_duong_don_vi",
                "tuyen_duong_don_vi",
                "Tuyến đường đơn vị");

            AddCountInformation(
                connection,
                report,
                exists,
                "khu_vuc_don_vi",
                "khu_vuc_don_vi",
                "Khu vực đơn vị");
        }


        private void AddCountInformation(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists,
            string objectName,
            string layerKey,
            string layerName)
        {
            if (!Exists(
                exists,
                objectName))
            {
                return;
            }

            int count =
                GetObjectCount(
                    connection,
                    objectName);

            AddPass(
                report,
                "Số lượng dữ liệu",
                layerKey,
                layerName,
                "Có " +
                count +
                " bản ghi.");
        }


        // =========================================================
        // GEOMETRY
        // =========================================================
        private void CheckGeometryQuality(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (Exists(
                exists,
                "v_ban_do_ha_noi"))
            {
                CheckGeometryLayer(
                    connection,
                    report,
                    "v_ban_do_ha_noi",
                    "dia_gioi_hanh_chinh",
                    "Ranh giới Thành phố Hà Nội",
                    "COALESCE(ten_hanh_chinh, 'Hà Nội')",
                    "MULTIPOLYGON",
                    false);
            }

            if (Exists(
                exists,
                "v_ban_do_xa_phuong"))
            {
                CheckGeometryLayer(
                    connection,
                    report,
                    "v_ban_do_xa_phuong",
                    "xa_phuong_ha_noi",
                    "Xã, phường Hà Nội",
                    "COALESCE(ten_xa_phuong, '')",
                    "MULTIPOLYGON",
                    false);
            }

            if (Exists(
                exists,
                "don_vi_quan_doi"))
            {
                CheckGeometryLayer(
                    connection,
                    report,
                    "don_vi_quan_doi",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "COALESCE(ten_don_vi, ma_don_vi, '')",
                    "POINT",
                    true);
            }

            if (Exists(
                exists,
                "tuyen_duong_don_vi"))
            {
                CheckGeometryLayer(
                    connection,
                    report,
                    "tuyen_duong_don_vi",
                    "tuyen_duong_don_vi",
                    "Tuyến đường đơn vị",
                    "COALESCE(ten_tuyen, ma_tuyen, '')",
                    "LINESTRING",
                    true);
            }

            if (Exists(
                exists,
                "khu_vuc_don_vi"))
            {
                CheckGeometryLayer(
                    connection,
                    report,
                    "khu_vuc_don_vi",
                    "khu_vuc_don_vi",
                    "Khu vực đơn vị",
                    "COALESCE(ten_khu_vuc, ma_khu_vuc, '')",
                    "POLYGON",
                    true);
            }
        }


        private void CheckGeometryLayer(
            NpgsqlConnection connection,
            DataQualityReport report,
            string objectName,
            string layerKey,
            string layerName,
            string displayExpression,
            string expectedGeometryType,
            bool nullIsWarning)
        {
            int problems = 0;

            string qObject =
                QuoteQualified(
                    PG_SCHEMA,
                    objectName);

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    nullIsWarning
                        ? "Cảnh báo"
                        : "Lỗi",
                    "Geometry NULL",
                    layerKey,
                    layerName,
                    "SELECT id, " +
                    displayExpression +
                    " AS object_name, " +
                    "'Chưa có geometry.' AS message " +
                    "FROM " +
                    qObject +
                    " WHERE geom IS NULL;");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Geometry rỗng",
                    layerKey,
                    layerName,
                    "SELECT id, " +
                    displayExpression +
                    " AS object_name, " +
                    "'Geometry đang rỗng.' AS message " +
                    "FROM " +
                    qObject +
                    " WHERE geom IS NOT NULL " +
                    "AND ST_IsEmpty(geom);");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Geometry không hợp lệ",
                    layerKey,
                    layerName,
                    "SELECT id, " +
                    displayExpression +
                    " AS object_name, " +
                    "ST_IsValidReason(geom) AS message " +
                    "FROM " +
                    qObject +
                    " WHERE geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(geom) " +
                    "AND NOT ST_IsValid(geom);");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Sai SRID",
                    layerKey,
                    layerName,
                    "SELECT id, " +
                    displayExpression +
                    " AS object_name, " +
                    "('SRID hiện tại: ' || ST_SRID(geom)::text || " +
                    "'. Yêu cầu EPSG:" +
                    EXPECTED_SRID +
                    ".') AS message " +
                    "FROM " +
                    qObject +
                    " WHERE geom IS NOT NULL " +
                    "AND ST_SRID(geom) <> " +
                    EXPECTED_SRID +
                    ";");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Sai kiểu geometry",
                    layerKey,
                    layerName,
                    "SELECT id, " +
                    displayExpression +
                    " AS object_name, " +
                    "('Kiểu hiện tại: ' || GeometryType(geom) || " +
                    "'. Yêu cầu: " +
                    expectedGeometryType +
                    ".') AS message " +
                    "FROM " +
                    qObject +
                    " WHERE geom IS NOT NULL " +
                    "AND UPPER(GeometryType(geom)) <> '" +
                    expectedGeometryType +
                    "';");

            if (problems == 0)
            {
                AddPass(
                    report,
                    "Chất lượng geometry",
                    layerKey,
                    layerName,
                    "Geometry hợp lệ, đúng kiểu và đúng EPSG:" +
                    EXPECTED_SRID +
                    ".");
            }
        }


        // =========================================================
        // DỮ LIỆU NGHIỆP VỤ ĐƠN VỊ
        // =========================================================
        private void CheckUnitBusinessData(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(
                exists,
                "don_vi_quan_doi"))
            {
                return;
            }

            int problems = 0;

            string qUnit =
                QuoteQualified(
                    PG_SCHEMA,
                    "don_vi_quan_doi");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Thiếu mã đơn vị",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "SELECT id, " +
                    "COALESCE(ten_don_vi, '') AS object_name, " +
                    "'Mã đơn vị đang để trống.' AS message " +
                    "FROM " +
                    qUnit +
                    " WHERE ma_don_vi IS NULL " +
                    "OR BTRIM(ma_don_vi) = '';");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Thiếu tên đơn vị",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "SELECT id, " +
                    "COALESCE(ma_don_vi, '') AS object_name, " +
                    "'Tên đơn vị đang để trống.' AS message " +
                    "FROM " +
                    qUnit +
                    " WHERE ten_don_vi IS NULL " +
                    "OR BTRIM(ten_don_vi) = '';");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Trùng mã đơn vị",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "WITH dup AS (" +
                    " SELECT ma_don_vi " +
                    " FROM " +
                    qUnit +
                    " WHERE ma_don_vi IS NOT NULL " +
                    " AND BTRIM(ma_don_vi) <> '' " +
                    " GROUP BY ma_don_vi " +
                    " HAVING COUNT(*) > 1" +
                    ") " +
                    "SELECT dv.id, " +
                    "COALESCE(dv.ten_don_vi, dv.ma_don_vi, '') AS object_name, " +
                    "('Mã đơn vị bị trùng: ' || dv.ma_don_vi) AS message " +
                    "FROM " +
                    qUnit +
                    " dv JOIN dup ON dup.ma_don_vi = dv.ma_don_vi;");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Cảnh báo",
                    "Thiếu xã/phường",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "SELECT id, " +
                    "COALESCE(ten_don_vi, ma_don_vi, '') AS object_name, " +
                    "'Chưa gán mã xã/phường.' AS message " +
                    "FROM " +
                    qUnit +
                    " WHERE ma_xa_phuong IS NULL " +
                    "OR BTRIM(ma_xa_phuong) = '';");

            if (problems == 0)
            {
                AddPass(
                    report,
                    "Dữ liệu nghiệp vụ",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "Không phát hiện mã trùng hoặc trường bắt buộc bị thiếu.");
            }
        }


        // =========================================================
        // QUAN HỆ ĐƠN VỊ - XÃ/PHƯỜNG
        // =========================================================
        private void CheckSpatialAdministrativeRelation(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(
                    exists,
                    "don_vi_quan_doi")
                ||
                !Exists(
                    exists,
                    "v_ban_do_xa_phuong"))
            {
                return;
            }

            string qUnit =
                QuoteQualified(
                    PG_SCHEMA,
                    "don_vi_quan_doi");

            string qXa =
                QuoteQualified(
                    PG_SCHEMA,
                    "v_ban_do_xa_phuong");

            int problems = 0;

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Mã xã/phường không tồn tại",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "SELECT dv.id, " +
                    "COALESCE(dv.ten_don_vi, dv.ma_don_vi, '') AS object_name, " +
                    "('Không tìm thấy xã/phường có mã: ' || dv.ma_xa_phuong) AS message " +
                    "FROM " +
                    qUnit +
                    " dv " +
                    "WHERE dv.ma_xa_phuong IS NOT NULL " +
                    "AND BTRIM(dv.ma_xa_phuong) <> '' " +
                    "AND NOT EXISTS (" +
                    " SELECT 1 FROM " +
                    qXa +
                    " xp " +
                    " WHERE xp.ma_xa_phuong = dv.ma_xa_phuong" +
                    ");");

            problems +=
                AddIssuesFromQuery(
                    connection,
                    report,
                    "Lỗi",
                    "Point nằm sai xã/phường",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "SELECT dv.id, " +
                    "COALESCE(dv.ten_don_vi, dv.ma_don_vi, '') AS object_name, " +
                    "('Point không nằm trong xã/phường đã gán: ' || dv.ma_xa_phuong) AS message " +
                    "FROM " +
                    qUnit +
                    " dv " +
                    "WHERE dv.geom IS NOT NULL " +
                    "AND dv.ma_xa_phuong IS NOT NULL " +
                    "AND BTRIM(dv.ma_xa_phuong) <> '' " +
                    "AND EXISTS (" +
                    " SELECT 1 FROM " +
                    qXa +
                    " xp0 " +
                    " WHERE xp0.ma_xa_phuong = dv.ma_xa_phuong" +
                    ") " +
                    "AND NOT EXISTS (" +
                    " SELECT 1 FROM " +
                    qXa +
                    " xp " +
                    " WHERE xp.ma_xa_phuong = dv.ma_xa_phuong " +
                    " AND ST_Covers(xp.geom, dv.geom)" +
                    ");");

            if (problems == 0)
            {
                AddPass(
                    report,
                    "Quan hệ không gian",
                    "don_vi_quan_doi",
                    "Đơn vị quân đội",
                    "Mã xã/phường hợp lệ và Point đại diện nằm đúng địa bàn được gán.");
            }
        }


        // =========================================================
        // KHÓA THAM CHIẾU NGHIỆP VỤ
        // =========================================================
        private void CheckForeignReferences(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(
                exists,
                "don_vi_quan_doi"))
            {
                return;
            }

            string qUnit =
                QuoteQualified(
                    PG_SCHEMA,
                    "don_vi_quan_doi");

            if (Exists(
                exists,
                "tuyen_duong_don_vi"))
            {
                string qRoute =
                    QuoteQualified(
                        PG_SCHEMA,
                        "tuyen_duong_don_vi");

                int count =
                    AddIssuesFromQuery(
                        connection,
                        report,
                        "Lỗi",
                        "Tham chiếu đơn vị",
                        "tuyen_duong_don_vi",
                        "Tuyến đường đơn vị",
                        "SELECT td.id, " +
                        "COALESCE(td.ten_tuyen, td.ma_tuyen, '') AS object_name, " +
                        "('don_vi_id không tồn tại: ' || td.don_vi_id::text) AS message " +
                        "FROM " +
                        qRoute +
                        " td LEFT JOIN " +
                        qUnit +
                        " dv ON dv.id = td.don_vi_id " +
                        "WHERE td.don_vi_id IS NOT NULL " +
                        "AND dv.id IS NULL;");

                if (count == 0)
                {
                    AddPass(
                        report,
                        "Tham chiếu dữ liệu",
                        "tuyen_duong_don_vi",
                        "Tuyến đường đơn vị",
                        "Mọi don_vi_id đều tham chiếu tới đơn vị tồn tại.");
                }
            }

            if (Exists(
                exists,
                "khu_vuc_don_vi"))
            {
                string qArea =
                    QuoteQualified(
                        PG_SCHEMA,
                        "khu_vuc_don_vi");

                int count =
                    AddIssuesFromQuery(
                        connection,
                        report,
                        "Lỗi",
                        "Tham chiếu đơn vị",
                        "khu_vuc_don_vi",
                        "Khu vực đơn vị",
                        "SELECT kv.id, " +
                        "COALESCE(kv.ten_khu_vuc, kv.ma_khu_vuc, '') AS object_name, " +
                        "('don_vi_id không tồn tại: ' || kv.don_vi_id::text) AS message " +
                        "FROM " +
                        qArea +
                        " kv LEFT JOIN " +
                        qUnit +
                        " dv ON dv.id = kv.don_vi_id " +
                        "WHERE kv.don_vi_id IS NOT NULL " +
                        "AND dv.id IS NULL;");

                if (count == 0)
                {
                    AddPass(
                        report,
                        "Tham chiếu dữ liệu",
                        "khu_vuc_don_vi",
                        "Khu vực đơn vị",
                        "Mọi don_vi_id đều tham chiếu tới đơn vị tồn tại.");
                }
            }
        }


        // =========================================================
        // HELPERS
        // =========================================================
        private bool Exists(
            Dictionary<string, bool> exists,
            string objectName)
        {
            bool value;

            return
                exists != null &&
                exists.TryGetValue(
                    objectName,
                    out value) &&
                value;
        }


        private int GetObjectCount(
            NpgsqlConnection connection,
            string objectName)
        {
            object value =
                ExecuteScalar(
                    connection,
                    "SELECT COUNT(*) FROM " +
                    QuoteQualified(
                        PG_SCHEMA,
                        objectName) +
                    ";");

            if (value == null ||
                value == DBNull.Value)
            {
                return 0;
            }

            return
                Convert.ToInt32(
                    value);
        }


        private object ExecuteScalar(
            NpgsqlConnection connection,
            string sql)
        {
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                return
                    command.ExecuteScalar();
            }
        }


        private int AddIssuesFromQuery(
            NpgsqlConnection connection,
            DataQualityReport report,
            string severity,
            string checkName,
            string layerKey,
            string layerName,
            string sql)
        {
            int count = 0;

            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            using (NpgsqlDataReader reader =
                command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int? id = null;

                    if (!reader.IsDBNull(0))
                    {
                        id =
                            Convert.ToInt32(
                                reader.GetValue(0));
                    }

                    string objectName =
                        reader.IsDBNull(1)
                            ? ""
                            : reader.GetValue(1)
                                .ToString();

                    string message =
                        reader.IsDBNull(2)
                            ? ""
                            : reader.GetValue(2)
                                .ToString();

                    AddIssue(
                        report,
                        severity,
                        checkName,
                        layerKey,
                        layerName,
                        id,
                        objectName,
                        message);

                    count++;
                }
            }

            return count;
        }


        private void AddPass(
            DataQualityReport report,
            string checkName,
            string layerKey,
            string layerName,
            string message)
        {
            AddIssue(
                report,
                "Đạt",
                checkName,
                layerKey,
                layerName,
                null,
                "",
                message);
        }


        private void AddIssue(
            DataQualityReport report,
            string severity,
            string checkName,
            string layerKey,
            string layerName,
            int? objectId,
            string objectName,
            string message)
        {
            if (report == null)
            {
                return;
            }

            report.Items.Add(
                new DataQualityIssue
                {
                    Severity =
                        severity ?? "",
                    CheckName =
                        checkName ?? "",
                    LayerKey =
                        layerKey ?? "",
                    LayerName =
                        layerName ?? "",
                    ObjectId =
                        objectId,
                    ObjectName =
                        objectName ?? "",
                    Message =
                        message ?? ""
                });
        }


        private string QuoteQualified(
            string schema,
            string objectName)
        {
            string safeSchema =
                (schema ?? "")
                .Replace(
                    "\"",
                    "\"\"");

            string safeObject =
                (objectName ?? "")
                .Replace(
                    "\"",
                    "\"\"");

            return
                "\"" +
                safeSchema +
                "\".\"" +
                safeObject +
                "\"";
        }
    }
}