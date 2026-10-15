
using System;
using System.Collections.Generic;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // DÙNG CHUNG CẤU HÌNH CSDL CỦA FORM1
    // ============================================================
    public partial class Form1
    {
        internal static string TaoChuoiKetNoiKiemTraCSDL()
        {
            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host = PG_HOST;
            builder.Port = Convert.ToInt32(PG_PORT);
            builder.Database = PG_DB;
            builder.Username = PG_USER;
            builder.Password = PG_PASS;
            builder.Timeout = 5;
            builder.CommandTimeout = 30;
            builder.Pooling = true;

            return builder.ConnectionString;
        }
    }

    // ============================================================
    // KẾT QUẢ CỦA MỘT PHÉP KIỂM TRA
    // Giữ nguyên tên thuộc tính để tương thích DataQualityForm.
    // ============================================================
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
                return ObjectId.HasValue &&
                    !string.IsNullOrWhiteSpace(LayerKey);
            }
        }
    }

    // ============================================================
    // BÁO CÁO KIỂM TRA TỔNG HỢP
    // ============================================================
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
            Items = new List<DataQualityIssue>();
            ExecutedAt = DateTime.Now;
        }

        public int ErrorCount
        {
            get { return CountSeverity("Lỗi"); }
        }

        public int WarningCount
        {
            get { return CountSeverity("Cảnh báo"); }
        }

        public int PassCount
        {
            get { return CountSeverity("Đạt"); }
        }

        private int CountSeverity(string severity)
        {
            int count = 0;

            foreach (DataQualityIssue issue in Items)
            {
                if (issue != null &&
                    string.Equals(
                        issue.Severity,
                        severity,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
    }

    // ============================================================
    // DỊCH VỤ KIỂM TRA CHẤT LƯỢNG CSDL - PHIÊN BẢN 2
    //
    // CSDL: HQTCSDL
    // Schema: ql_don_vi
    // CRS: EPSG:32648
    //
    // Chỉ đọc dữ liệu. Không UPDATE / DELETE / INSERT.
    // ============================================================
    public sealed class DataQualityService
    {
        private const string Schema = "ql_don_vi";
        private const int Srid = 32648;

        private readonly string _connectionString;

        public DataQualityService()
        {
            _connectionString =
                Form1.TaoChuoiKetNoiKiemTraCSDL();
        }

        // ========================================================
        // 1. CHẠY TOÀN BỘ KIỂM TRA
        // ========================================================
        public DataQualityReport RunAllChecks()
        {
            DataQualityReport report =
                new DataQualityReport();

            try
            {
                using (NpgsqlConnection connection =
                    new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    AddPass(
                        report,
                        "Kết nối CSDL",
                        "",
                        "PostgreSQL",
                        "Đã kết nối CSDL HQTCSDL.");

                    CheckPostGIS(connection, report);

                    Dictionary<string, bool> exists =
                        CheckStructure(connection, report);

                    CheckCounts(
                        connection, report, exists);

                    CheckGeometry(
                        connection, report, exists);

                    CheckUnitData(
                        connection, report, exists);

                    CheckReferences(
                        connection, report, exists);

                    CheckSpatialRelation(
                        connection, report, exists);

                    CheckSpatialIndexes(
                        connection, report, exists);
                }
            }
            catch (Exception ex)
            {
                AddIssue(
                    report,
                    "Lỗi",
                    "Kiểm tra CSDL",
                    "",
                    "PostgreSQL/PostGIS",
                    null,
                    "",
                    "Không thể hoàn tất kiểm tra: " +
                    ex.Message);
            }

            report.ExecutedAt = DateTime.Now;

            return report;
        }

        // ========================================================
        // 2. KIỂM TRA POSTGIS
        // ========================================================
        private void CheckPostGIS(
            NpgsqlConnection connection,
            DataQualityReport report)
        {
            try
            {
                object version = ExecuteScalar(
                    connection,
                    "SELECT PostGIS_Version();");

                AddPass(
                    report,
                    "PostGIS",
                    "",
                    "PostGIS",
                    "PostGIS hoạt động: " +
                    Convert.ToString(version));
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
                    "Không thể gọi PostGIS_Version(): " +
                    ex.Message);
            }
        }

        // ========================================================
        // 3. KIỂM TRA CẤU TRÚC
        // ========================================================
        private Dictionary<string, bool> CheckStructure(
            NpgsqlConnection connection,
            DataQualityReport report)
        {
            Dictionary<string, bool> exists =
                new Dictionary<string, bool>(
                    StringComparer.OrdinalIgnoreCase);

            string[] required =
            {
                "cap_don_vi",
                "loai_don_vi",
                "dia_gioi_hanh_chinh",
                "dm_xa_phuong",
                "don_vi_quan_doi",
                "tuyen_duong_don_vi",
                "khu_vuc_don_vi",
                "v_ban_do_ha_noi",
                "v_ban_do_xa_phuong",
                "v_don_vi_ban_do",
                "v_tong_quan_he_thong",
                "v_phan_tich_xa_phuong"
            };

            foreach (string name in required)
            {
                bool found = false;

                using (NpgsqlCommand command =
                    new NpgsqlCommand(
                        "SELECT to_regclass(@name) IS NOT NULL;",
                        connection))
                {
                    command.Parameters.AddWithValue(
                        "name",
                        Schema + "." + name);

                    found = Convert.ToBoolean(
                        command.ExecuteScalar());
                }

                exists[name] = found;

                if (found)
                {
                    AddPass(
                        report,
                        "Cấu trúc CSDL",
                        "",
                        name,
                        "Đã tồn tại " +
                        Schema + "." + name);
                }
                else
                {
                    AddIssue(
                        report,
                        "Lỗi",
                        "Cấu trúc CSDL",
                        "",
                        name,
                        null,
                        "",
                        "Thiếu bảng hoặc VIEW: " +
                        Schema + "." + name);
                }
            }

            return exists;
        }

        // ========================================================
        // 4. KIỂM TRA SỐ LƯỢNG DỮ LIỆU
        // ========================================================
        private void CheckCounts(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            string[] tables =
            {
                "dia_gioi_hanh_chinh",
                "dm_xa_phuong",
                "don_vi_quan_doi",
                "tuyen_duong_don_vi",
                "khu_vuc_don_vi"
            };

            foreach (string table in tables)
            {
                if (!Exists(exists, table))
                    continue;

                long count = Convert.ToInt64(
                    ExecuteScalar(
                        connection,
                        "SELECT COUNT(*) FROM " +
                        Qualified(table)));

                AddPass(
                    report,
                    "Số lượng dữ liệu",
                    LayerKey(table),
                    table,
                    "Hiện có " + count +
                    " bản ghi.");
            }

            // Địa giới nguồn có thể gồm nhiều bản ghi.
            // Không ép dia_gioi_hanh_chinh phải bằng 1.

            if (Exists(exists, "dm_xa_phuong"))
            {
                long count = Convert.ToInt64(
                    ExecuteScalar(
                        connection,
                        "SELECT COUNT(*) FROM " +
                        Qualified("dm_xa_phuong")));

                if (count != 126)
                {
                    AddIssue(
                        report,
                        "Cảnh báo",
                        "Dữ liệu hành chính",
                        "xa_phuong_ha_noi",
                        "Xã/phường",
                        null,
                        "",
                        "Bộ dữ liệu hiện tại của dự án " +
                        "có mốc đối chiếu 126 xã/phường. " +
                        "Số bản ghi hiện tại: " + count);
                }
            }
        }

        // ========================================================
        // 5. KIỂM TRA HÌNH HỌC
        // ========================================================
        private void CheckGeometry(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            CheckGeometryTable(
                connection, report, exists,
                "dia_gioi_hanh_chinh",
                "ST_Polygon,ST_MultiPolygon");

            CheckGeometryTable(
                connection, report, exists,
                "dm_xa_phuong",
                "ST_Polygon,ST_MultiPolygon");

            CheckGeometryTable(
                connection, report, exists,
                "don_vi_quan_doi",
                "ST_Point");

            CheckGeometryTable(
                connection, report, exists,
                "tuyen_duong_don_vi",
                "ST_LineString,ST_MultiLineString");

            CheckGeometryTable(
                connection, report, exists,
                "khu_vuc_don_vi",
                "ST_Polygon,ST_MultiPolygon");
        }

        private void CheckGeometryTable(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists,
            string table,
            string allowedTypes)
        {
            if (!Exists(exists, table))
                return;

            string q = Qualified(table);
            string layer = LayerKey(table);

            // Geometry NULL hoặc rỗng
            CheckRows(
                connection, report,
                "Cảnh báo",
                "Thiếu hình học",
                layer, table,
                "SELECT id::integer, id::text, " +
                "'Hình học NULL hoặc rỗng' " +
                "FROM " + q +
                " WHERE geom IS NULL " +
                "OR ST_IsEmpty(geom);",
                "Không có hình học NULL hoặc rỗng.",
                false);

            // Geometry không hợp lệ
            CheckRows(
                connection, report,
                "Lỗi",
                "Hình học không hợp lệ",
                layer, table,
                "SELECT id::integer, id::text, " +
                "ST_IsValidReason(geom) " +
                "FROM " + q +
                " WHERE geom IS NOT NULL " +
                "AND NOT ST_IsEmpty(geom) " +
                "AND NOT ST_IsValid(geom);",
                "Không phát hiện hình học lỗi.",
                true);

            // Sai hệ tọa độ
            CheckRows(
                connection, report,
                "Lỗi",
                "Sai hệ tọa độ",
                layer, table,
                "SELECT id::integer, id::text, " +
                "'SRID hiện tại: ' || ST_SRID(geom)::text " +
                "FROM " + q +
                " WHERE geom IS NOT NULL " +
                "AND ST_SRID(geom) <> " + Srid + ";",
                "Hình học sử dụng đúng EPSG:32648.",
                true);

            // Sai kiểu hình học
            string[] types =
                allowedTypes.Split(',');

            List<string> values =
                new List<string>();

            foreach (string type in types)
            {
                values.Add("'" + type + "'");
            }

            CheckRows(
                connection, report,
                "Lỗi",
                "Sai kiểu hình học",
                layer, table,
                "SELECT id::integer, id::text, " +
                "'Kiểu hình học: ' || ST_GeometryType(geom) " +
                "FROM " + q +
                " WHERE geom IS NOT NULL " +
                "AND NOT ST_IsEmpty(geom) " +
                "AND ST_GeometryType(geom) NOT IN (" +
                string.Join(",", values.ToArray()) +
                ");",
                "Kiểu hình học phù hợp với lớp dữ liệu.",
                true);
        }

        // ========================================================
        // 6. KIỂM TRA HỒ SƠ ĐƠN VỊ
        // ========================================================
        private void CheckUnitData(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(exists, "don_vi_quan_doi"))
                return;

            string q = Qualified("don_vi_quan_doi");

            CheckRows(
                connection, report,
                "Lỗi",
                "Thiếu mã đơn vị",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT id::integer, id::text, " +
                "'Thiếu mã đơn vị' " +
                "FROM " + q +
                " WHERE ma_don_vi IS NULL " +
                "OR BTRIM(ma_don_vi) = '';",
                "Mọi hồ sơ đều có mã đơn vị.",
                false);

            CheckRows(
                connection, report,
                "Lỗi",
                "Thiếu tên đơn vị",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT id::integer, id::text, " +
                "'Thiếu tên đơn vị' " +
                "FROM " + q +
                " WHERE ten_don_vi IS NULL " +
                "OR BTRIM(ten_don_vi) = '';",
                "Mọi hồ sơ đều có tên đơn vị.",
                false);

            CheckRows(
                connection, report,
                "Lỗi",
                "Trùng mã đơn vị",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT d.id::integer, " +
                "COALESCE(d.ten_don_vi, d.ma_don_vi), " +
                "'Mã bị trùng: ' || d.ma_don_vi " +
                "FROM " + q + " d " +
                "JOIN (SELECT LOWER(BTRIM(ma_don_vi)) ma " +
                "FROM " + q + " " +
                "WHERE ma_don_vi IS NOT NULL " +
                "GROUP BY LOWER(BTRIM(ma_don_vi)) " +
                "HAVING COUNT(*) > 1) x " +
                "ON LOWER(BTRIM(d.ma_don_vi)) = x.ma;",
                "Không phát hiện mã đơn vị trùng.",
                true);

            CheckRows(
                connection, report,
                "Cảnh báo",
                "Ký hiệu chưa xác minh",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT id::integer, " +
                "COALESCE(ten_don_vi, id::text), " +
                "'Ký hiệu chưa được xác minh' " +
                "FROM " + q +
                " WHERE ky_hieu_bien_so IS NULL " +
                "OR BTRIM(ky_hieu_bien_so) = '' " +
                "OR UPPER(BTRIM(ky_hieu_bien_so)) " +
                "= 'CHƯA XÁC MINH';",
                "Không có ký hiệu đang chờ xác minh.",
                true);

            CheckRows(
                connection, report,
                "Lỗi",
                "Thiếu quan hệ danh mục",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT id::integer, " +
                "COALESCE(ten_don_vi, id::text), " +
                "'Thiếu cấp, loại hoặc xã/phường' " +
                "FROM " + q +
                " WHERE cap_don_vi_id IS NULL " +
                "OR loai_don_vi_id IS NULL " +
                "OR xa_phuong_id IS NULL;",
                "Mọi hồ sơ đều có cấp, loại và xã/phường.",
                true);
        }

        // ========================================================
        // 7. KIỂM TRA KHÓA THAM CHIẾU
        // ========================================================
        private void CheckReferences(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(exists, "don_vi_quan_doi"))
                return;

            string q = Qualified("don_vi_quan_doi");

            CheckUnitReference(
                connection, report, exists,
                q, "cap_don_vi_id",
                "cap_don_vi", "Cấp đơn vị");

            CheckUnitReference(
                connection, report, exists,
                q, "loai_don_vi_id",
                "loai_don_vi", "Loại đơn vị");

            CheckUnitReference(
                connection, report, exists,
                q, "xa_phuong_id",
                "dm_xa_phuong", "Xã/phường");

            CheckRows(
                connection, report,
                "Lỗi",
                "Đơn vị cấp trên",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT d.id::integer, " +
                "COALESCE(d.ten_don_vi, d.id::text), " +
                "'Không tìm thấy đơn vị cấp trên ID: ' || " +
                "d.don_vi_cap_tren_id::text " +
                "FROM " + q + " d " +
                "LEFT JOIN " + q + " p " +
                "ON p.id = d.don_vi_cap_tren_id " +
                "WHERE d.don_vi_cap_tren_id IS NOT NULL " +
                "AND p.id IS NULL;",
                "Không có tham chiếu cấp trên bị mất.",
                true);

            CheckSpatialObjectReference(
                connection, report, exists,
                "tuyen_duong_don_vi",
                "Tuyến đường đơn vị");

            CheckSpatialObjectReference(
                connection, report, exists,
                "khu_vuc_don_vi",
                "Khu vực đơn vị");
        }

        private void CheckUnitReference(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists,
            string unitTable,
            string field,
            string targetTable,
            string label)
        {
            if (!Exists(exists, targetTable))
                return;

            CheckRows(
                connection, report,
                "Lỗi",
                "Tham chiếu " + label,
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                "SELECT d.id::integer, " +
                "COALESCE(d.ten_don_vi, d.id::text), " +
                "'Không tìm thấy " + label + " ID: ' || " +
                "d." + field + "::text " +
                "FROM " + unitTable + " d " +
                "LEFT JOIN " + Qualified(targetTable) + " x " +
                "ON x.id = d." + field + " " +
                "WHERE d." + field + " IS NOT NULL " +
                "AND x.id IS NULL;",
                "Các tham chiếu " + label +
                " đều hợp lệ.",
                true);
        }

        private void CheckSpatialObjectReference(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists,
            string table,
            string label)
        {
            if (!Exists(exists, table))
                return;

            CheckRows(
                connection, report,
                "Lỗi",
                "Tham chiếu đơn vị",
                table, label,
                "SELECT o.id::integer, o.id::text, " +
                "'Không tìm thấy đơn vị ID: ' || " +
                "o.don_vi_id::text " +
                "FROM " + Qualified(table) + " o " +
                "LEFT JOIN " +
                Qualified("don_vi_quan_doi") + " d " +
                "ON d.id = o.don_vi_id " +
                "WHERE o.don_vi_id IS NOT NULL " +
                "AND d.id IS NULL;",
                "Không có tham chiếu đơn vị bị mất.",
                true);
        }

        // ========================================================
        // 8. KIỂM TRA QUAN HỆ KHÔNG GIAN
        //
        // Đối chiếu xa_phuong_id, KHÔNG dùng ma_xa_phuong
        // trong bảng don_vi_quan_doi.
        // ========================================================
        private void CheckSpatialRelation(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            if (!Exists(exists, "don_vi_quan_doi") ||
                !Exists(exists, "dm_xa_phuong"))
            {
                return;
            }

            CheckRows(
                connection, report,
                "Lỗi",
                "Điểm đại diện ngoài địa bàn",
                "don_vi_quan_doi",
                "Đơn vị quân đội",
                @"SELECT
                    d.id::integer,
                    COALESCE(d.ten_don_vi, d.id::text),
                    'Điểm đại diện không nằm trong xã/phường được gán'
                  FROM ql_don_vi.don_vi_quan_doi d
                  JOIN ql_don_vi.dm_xa_phuong x
                    ON x.id = d.xa_phuong_id
                  WHERE d.geom IS NOT NULL
                    AND x.geom IS NOT NULL
                    AND CASE
                        WHEN ST_IsEmpty(d.geom)
                          OR ST_IsEmpty(x.geom)
                          OR ST_SRID(d.geom) <> 32648
                          OR ST_SRID(x.geom) <> 32648
                          OR NOT ST_IsValid(d.geom)
                          OR NOT ST_IsValid(x.geom)
                          OR ST_GeometryType(d.geom) <> 'ST_Point'
                        THEN FALSE
                        ELSE NOT ST_Covers(x.geom, d.geom)
                    END;",
                "Điểm đại diện khớp địa bàn được gán.",
                true);
        }

        // ========================================================
        // 9. KIỂM TRA CHỈ MỤC KHÔNG GIAN
        // ========================================================
        private void CheckSpatialIndexes(
            NpgsqlConnection connection,
            DataQualityReport report,
            Dictionary<string, bool> exists)
        {
            string[] tables =
            {
                "dia_gioi_hanh_chinh",
                "dm_xa_phuong",
                "don_vi_quan_doi",
                "tuyen_duong_don_vi",
                "khu_vuc_don_vi"
            };

            foreach (string table in tables)
            {
                if (!Exists(exists, table))
                    continue;

                using (NpgsqlCommand command =
                    new NpgsqlCommand(
                        @"SELECT EXISTS (
                            SELECT 1
                            FROM pg_indexes
                            WHERE schemaname = @schema
                              AND tablename = @table
                              AND indexdef ILIKE '%USING gist%'
                        );",
                        connection))
                {
                    command.Parameters.AddWithValue(
                        "schema", Schema);

                    command.Parameters.AddWithValue(
                        "table", table);

                    bool hasIndex = Convert.ToBoolean(
                        command.ExecuteScalar());

                    if (hasIndex)
                    {
                        AddPass(
                            report,
                            "Chỉ mục không gian",
                            LayerKey(table),
                            table,
                            "Đã có chỉ mục GiST.");
                    }
                    else
                    {
                        AddIssue(
                            report,
                            "Cảnh báo",
                            "Chỉ mục không gian",
                            LayerKey(table),
                            table,
                            null,
                            "",
                            "Chưa tìm thấy chỉ mục GiST.");
                    }
                }
            }
        }

        // ========================================================
        // 10. HÀM CHẠY TRUY VẤN KIỂM TRA
        // ========================================================
        private void CheckRows(
            NpgsqlConnection connection,
            DataQualityReport report,
            string severity,
            string checkName,
            string layerKey,
            string layerName,
            string sql,
            string passMessage,
            bool canZoom)
        {
            int issues = 0;

            try
            {
                using (NpgsqlCommand command =
                    new NpgsqlCommand(sql, connection))
                using (NpgsqlDataReader reader =
                    command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int? id = reader.IsDBNull(0)
                            ? (int?)null
                            : Convert.ToInt32(
                                reader.GetValue(0));

                        string objectName =
                            reader.IsDBNull(1)
                                ? ""
                                : Convert.ToString(
                                    reader.GetValue(1));

                        string message =
                            reader.IsDBNull(2)
                                ? ""
                                : Convert.ToString(
                                    reader.GetValue(2));

                        AddIssue(
                            report,
                            severity,
                            checkName,
                            canZoom ? layerKey : "",
                            layerName,
                            id,
                            objectName,
                            message);

                        issues++;
                    }
                }

                if (issues == 0)
                {
                    AddPass(
                        report,
                        checkName,
                        layerKey,
                        layerName,
                        passMessage);
                }
            }
            catch (Exception ex)
            {
                AddIssue(
                    report,
                    "Lỗi",
                    "Không thể kiểm tra: " + checkName,
                    "",
                    layerName,
                    null,
                    "",
                    ex.Message);
            }
        }

        // ========================================================
        // 11. CÁC HÀM HỖ TRỢ
        // ========================================================
        private bool Exists(
            Dictionary<string, bool> exists,
            string name)
        {
            bool value;

            return exists != null &&
                exists.TryGetValue(name, out value) &&
                value;
        }

        private string Qualified(string table)
        {
            // Chỉ nhận tên bảng cố định trong mã nguồn.
            // Không nhận tên bảng từ nhập liệu người dùng.
            return "\"" + Schema + "\".\"" + table + "\"";
        }

        private string LayerKey(string table)
        {
            if (table == "dm_xa_phuong")
                return "xa_phuong_ha_noi";

            return table;
        }

        private object ExecuteScalar(
            NpgsqlConnection connection,
            string sql)
        {
            using (NpgsqlCommand command =
                new NpgsqlCommand(sql, connection))
            {
                return command.ExecuteScalar();
            }
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
            report.Items.Add(
                new DataQualityIssue
                {
                    Severity = severity ?? "",
                    CheckName = checkName ?? "",
                    LayerKey = layerKey ?? "",
                    LayerName = layerName ?? "",
                    ObjectId = objectId,
                    ObjectName = objectName ?? "",
                    Message = message ?? ""
                });
        }
    }
}
