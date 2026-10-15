using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // =============================================================
    // UNIT DETAIL SERVICE - HÀ NỘI ONLY
    //
    // ĐỀ TÀI:
    // Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    // các đơn vị quân đội trên địa bàn Hà Nội.
    //
    // Chức năng:
    // - Lấy hồ sơ chi tiết 1 đơn vị;
    // - Hiển thị xã/phường theo mô hình hành chính hiện hành;
    // - Thống kê số tuyến đường;
    // - Thống kê tổng chiều dài tuyến;
    // - Thống kê số khu vực;
    // - Thống kê tổng diện tích;
    // - Thống kê tổng chu vi;
    // - Không đọc geometry thô lên giao diện.
    //
    // CRS dữ liệu không gian: EPSG:32648.
    // Vì đây là hệ tọa độ phẳng theo mét nên:
    // - ST_Length() trả về mét;
    // - ST_Area() trả về m²;
    // - ST_Perimeter() trả về mét.
    //
    // Hai alias dia_ban và dia_chi_hanh_chinh chỉ được giữ tạm
    // để UnitDetailForm.cs cũ không bị lỗi trước khi form được sửa.
    // CSDL thật không còn hai cột này.
    // =============================================================
    public sealed class UnitDetailService
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

        private readonly string _connectionString;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public UnitDetailService()
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
        // HỒ SƠ CHI TIẾT
        // =========================================================
        public DataRow GetUnitDetail(
            int unitId)
        {
            if (unitId <= 0)
            {
                return null;
            }

            string sql =
                "SELECT " +
                "dv.id, " +
                "dv.ma_don_vi, " +
                "dv.ten_don_vi, " +
                "dv.ky_hieu_bien_so, " +
                "cdv.ten_cap, " +
                "ldv.ten_loai, " +
                "captren.ten_don_vi AS ten_don_vi_cap_tren, " +
                "dv.tinh_thanh_pho, " +
                "dv.xa_phuong_id, " +
                "xp.ma_xa_phuong, " +
                "xp.ten_xa_phuong, " +
                "xp.loai AS loai_xa_phuong, " +

                "CASE " +
                "WHEN xp.loai = 'phuong' " +
                "THEN 'Phường ' || xp.ten_xa_phuong " +
                "WHEN xp.loai = 'xa' " +
                "THEN 'Xã ' || xp.ten_xa_phuong " +
                "ELSE xp.ten_xa_phuong " +
                "END AS ten_xa_phuong_hien_thi, " +

                "dv.dia_chi_hien_hanh, " +
                "dv.thong_tin_lien_he, " +
                "dv.nguon_tham_khao, " +
                "dv.ngay_xac_minh, " +
                "dv.muc_do_xac_minh, " +
                "dv.mo_ta, " +
                "dv.ghi_chu, " +
                "dv.trang_thai_quan_ly, " +
                "dv.ngay_tao, " +
                "dv.ngay_cap_nhat, " +

                "CASE " +
                "WHEN dv.geom IS NULL OR ST_IsEmpty(dv.geom) " +
                "THEN FALSE ELSE TRUE " +
                "END AS co_diem_dai_dien_gis, " +

                // -------------------------------------------------
                // ALIAS TƯƠNG THÍCH UNITDETAILFORM.CS CŨ
                // -------------------------------------------------
                "CASE " +
                "WHEN xp.loai = 'phuong' " +
                "THEN 'Phường ' || xp.ten_xa_phuong " +
                "WHEN xp.loai = 'xa' " +
                "THEN 'Xã ' || xp.ten_xa_phuong " +
                "ELSE xp.ten_xa_phuong " +
                "END AS dia_ban, " +

                "dv.dia_chi_hien_hanh AS dia_chi_hanh_chinh, " +

                "CASE " +
                "WHEN dv.geom IS NULL OR ST_IsEmpty(dv.geom) " +
                "THEN FALSE ELSE TRUE " +
                "END AS co_vi_tri_gis " +

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

                "WHERE dv.id = @id " +
                "LIMIT 1;";

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
                    "@id",
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(
                        unitId);

                DataTable table =
                    new DataTable(
                        "ho_so_don_vi");

                adapter.Fill(
                    table);

                if (table.Rows.Count == 0)
                {
                    table.Dispose();

                    return null;
                }

                DataTable clone =
                    table.Clone();

                clone.ImportRow(
                    table.Rows[0]);

                DataRow row =
                    clone.Rows[0];

                table.Dispose();

                return row;
            }
        }

        // =========================================================
        // THỐNG KÊ TUYẾN / KHU VỰC
        //
        // Geometry hiện dùng EPSG:32648 nên phép đo thực hiện trực
        // tiếp trên geometry.
        // =========================================================
        public DataRow GetUnitSpatialSummary(
            int unitId)
        {
            if (unitId <= 0)
            {
                return null;
            }

            string sql =
                "SELECT " +

                "(SELECT COUNT(*) " +
                " FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " td " +
                " WHERE td.don_vi_id = @id " +
                " AND td.geom IS NOT NULL " +
                " AND NOT ST_IsEmpty(td.geom)) " +
                " AS so_tuyen_duong, " +

                "(SELECT " +
                " COALESCE(" +
                " ROUND(" +
                " SUM(ST_Length(td.geom))::numeric, " +
                " 2), " +
                " 0) " +
                " FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " td " +
                " WHERE td.don_vi_id = @id " +
                " AND td.geom IS NOT NULL " +
                " AND NOT ST_IsEmpty(td.geom)) " +
                " AS tong_chieu_dai_m, " +

                "(SELECT COUNT(*) " +
                " FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " kv " +
                " WHERE kv.don_vi_id = @id " +
                " AND kv.geom IS NOT NULL " +
                " AND NOT ST_IsEmpty(kv.geom)) " +
                " AS so_khu_vuc, " +

                "(SELECT " +
                " COALESCE(" +
                " ROUND(" +
                " SUM(ST_Area(kv.geom))::numeric, " +
                " 2), " +
                " 0) " +
                " FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " kv " +
                " WHERE kv.don_vi_id = @id " +
                " AND kv.geom IS NOT NULL " +
                " AND NOT ST_IsEmpty(kv.geom)) " +
                " AS tong_dien_tich_m2, " +

                "(SELECT " +
                " COALESCE(" +
                " ROUND(" +
                " SUM(ST_Perimeter(kv.geom))::numeric, " +
                " 2), " +
                " 0) " +
                " FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " kv " +
                " WHERE kv.don_vi_id = @id " +
                " AND kv.geom IS NOT NULL " +
                " AND NOT ST_IsEmpty(kv.geom)) " +
                " AS tong_chu_vi_m;";

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
                    "@id",
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(
                        unitId);

                DataTable table =
                    new DataTable(
                        "thong_ke_khong_gian");

                adapter.Fill(
                    table);

                if (table.Rows.Count == 0)
                {
                    table.Dispose();

                    return null;
                }

                DataTable clone =
                    table.Clone();

                clone.ImportRow(
                    table.Rows[0]);

                DataRow row =
                    clone.Rows[0];

                table.Dispose();

                return row;
            }
        }

        // =========================================================
        // DANH SÁCH TUYẾN CỦA ĐƠN VỊ
        // =========================================================
        public DataTable GetRoutes(
            int unitId)
        {
            string sql =
                "SELECT " +
                "td.id, " +
                "td.ma_tuyen, " +
                "td.ten_tuyen, " +
                "td.loai_tuyen, " +
                "td.cap_duong, " +
                "td.be_rong_m, " +
                "td.mo_ta, " +
                "td.ghi_chu, " +
                "td.trang_thai_quan_ly, " +

                "CASE " +
                "WHEN td.geom IS NULL OR ST_IsEmpty(td.geom) " +
                "THEN NULL " +
                "ELSE ROUND(" +
                "ST_Length(td.geom)::numeric, " +
                "2) " +
                "END AS chieu_dai_m " +

                "FROM " +
                QuoteQualifiedTable(
                    "tuyen_duong_don_vi") +
                " td " +

                "WHERE td.don_vi_id = @id " +

                "ORDER BY " +
                "td.ten_tuyen, " +
                "td.ma_tuyen;";

            return ExecuteDataTableByUnitId(
                sql,
                "tuyen_duong",
                unitId);
        }

        // =========================================================
        // DANH SÁCH KHU VỰC CỦA ĐƠN VỊ
        // =========================================================
        public DataTable GetAreas(
            int unitId)
        {
            string sql =
                "SELECT " +
                "kv.id, " +
                "kv.ma_khu_vuc, " +
                "kv.ten_khu_vuc, " +
                "kv.loai_khu_vuc, " +
                "kv.mo_ta, " +
                "kv.ghi_chu, " +
                "kv.trang_thai_quan_ly, " +

                "CASE " +
                "WHEN kv.geom IS NULL OR ST_IsEmpty(kv.geom) " +
                "THEN NULL " +
                "ELSE ROUND(" +
                "ST_Area(kv.geom)::numeric, " +
                "2) " +
                "END AS dien_tich_m2, " +

                "CASE " +
                "WHEN kv.geom IS NULL OR ST_IsEmpty(kv.geom) " +
                "THEN NULL " +
                "ELSE ROUND(" +
                "ST_Perimeter(kv.geom)::numeric, " +
                "2) " +
                "END AS chu_vi_m " +

                "FROM " +
                QuoteQualifiedTable(
                    "khu_vuc_don_vi") +
                " kv " +

                "WHERE kv.don_vi_id = @id " +

                "ORDER BY " +
                "kv.ten_khu_vuc, " +
                "kv.ma_khu_vuc;";

            return ExecuteDataTableByUnitId(
                sql,
                "khu_vuc",
                unitId);
        }

        // =========================================================
        // PRIVATE - EXECUTE BY UNIT ID
        // =========================================================
        private DataTable ExecuteDataTableByUnitId(
            string sql,
            string tableName,
            int unitId)
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
                    "@id",
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(
                        unitId);

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