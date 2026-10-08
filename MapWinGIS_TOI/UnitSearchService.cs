using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // =============================================================
    // TIÊU CHÍ TRA CỨU ĐƠN VỊ - HÀ NỘI ONLY
    //
    // Mô hình hành chính:
    // Thành phố Hà Nội -> Xã/Phường.
    //
    // DiaBan được giữ tạm để tương thích với UnitSearchForm.cs cũ.
    // Khi UnitSearchForm.cs được cập nhật, nên dùng XaPhuongId.
    // =============================================================
    public sealed class UnitSearchCriteria
    {
        public string Keyword { get; set; }

        public int? CapDonViId { get; set; }

        public int? LoaiDonViId { get; set; }

        public int? XaPhuongId { get; set; }

        public string TrangThai { get; set; }

        // ---------------------------------------------------------
        // TƯƠNG THÍCH GIAO DIỆN CŨ
        //
        // Không còn cột dia_ban trong CSDL.
        // Giá trị này chỉ được hiểu như tên xã/phường để tránh
        // làm hỏng UnitSearchForm.cs trước khi form đó được sửa.
        // ---------------------------------------------------------
        public string DiaBan { get; set; }
    }

    // =============================================================
    // UNIT SEARCH SERVICE - HÀ NỘI ONLY
    //
    // Tra cứu theo:
    // - từ khóa;
    // - cấp đơn vị;
    // - loại đơn vị;
    // - xã/phường;
    // - trạng thái quản lý.
    //
    // Không sử dụng các cột CSDL cũ:
    // - dia_ban
    // - dia_chi_hanh_chinh
    // =============================================================
    public sealed class UnitSearchService
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
        public UnitSearchService()
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
        // TRA CỨU ĐƠN VỊ
        // =========================================================
        public DataTable Search(
            UnitSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria =
                    new UnitSearchCriteria();
            }

            List<string> conditions =
                new List<string>();

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    _connectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand())
            {
                command.Connection =
                    connection;

                // -------------------------------------------------
                // TỪ KHÓA
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    criteria.Keyword))
                {
                    conditions.Add(
                        "(" +
                        "COALESCE(dv.ma_don_vi, '') ILIKE @keyword " +
                        "OR COALESCE(dv.ten_don_vi, '') ILIKE @keyword " +
                        "OR COALESCE(dv.ky_hieu_bien_so, '') ILIKE @keyword " +
                        "OR COALESCE(cdv.ten_cap, '') ILIKE @keyword " +
                        "OR COALESCE(ldv.ten_loai, '') ILIKE @keyword " +
                        "OR COALESCE(captren.ten_don_vi, '') ILIKE @keyword " +
                        "OR COALESCE(xp.ma_xa_phuong, '') ILIKE @keyword " +
                        "OR COALESCE(xp.ten_xa_phuong, '') ILIKE @keyword " +
                        "OR COALESCE(dv.dia_chi_hien_hanh, '') ILIKE @keyword " +
                        "OR COALESCE(dv.thong_tin_lien_he, '') ILIKE @keyword " +
                        "OR COALESCE(dv.nguon_tham_khao, '') ILIKE @keyword " +
                        ")");

                    command.Parameters.Add(
                        "@keyword",
                        NpgsqlDbType.Text).Value =
                        "%" +
                        criteria.Keyword.Trim() +
                        "%";
                }

                // -------------------------------------------------
                // CẤP ĐƠN VỊ
                // -------------------------------------------------
                if (criteria.CapDonViId.HasValue)
                {
                    conditions.Add(
                        "dv.cap_don_vi_id = @cap_don_vi_id");

                    command.Parameters.Add(
                        "@cap_don_vi_id",
                        NpgsqlDbType.Bigint).Value =
                        Convert.ToInt64(
                            criteria.CapDonViId.Value);
                }

                // -------------------------------------------------
                // LOẠI ĐƠN VỊ
                // -------------------------------------------------
                if (criteria.LoaiDonViId.HasValue)
                {
                    conditions.Add(
                        "dv.loai_don_vi_id = @loai_don_vi_id");

                    command.Parameters.Add(
                        "@loai_don_vi_id",
                        NpgsqlDbType.Bigint).Value =
                        Convert.ToInt64(
                            criteria.LoaiDonViId.Value);
                }

                // -------------------------------------------------
                // XÃ / PHƯỜNG - CÁCH MỚI
                // -------------------------------------------------
                if (criteria.XaPhuongId.HasValue)
                {
                    conditions.Add(
                        "dv.xa_phuong_id = @xa_phuong_id");

                    command.Parameters.Add(
                        "@xa_phuong_id",
                        NpgsqlDbType.Bigint).Value =
                        Convert.ToInt64(
                            criteria.XaPhuongId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(
                    criteria.DiaBan))
                {
                    // ---------------------------------------------
                    // TƯƠNG THÍCH UnitSearchForm.cs CŨ
                    //
                    // criteria.DiaBan không còn ánh xạ vào cột
                    // dia_ban. Nó được hiểu là tên xã/phường.
                    // ---------------------------------------------
                    conditions.Add(
                        "(" +
                        "COALESCE(xp.ten_xa_phuong, '') = @dia_ban " +
                        "OR " +
                        "CASE " +
                        "WHEN xp.loai = 'phuong' " +
                        "THEN 'Phường ' || xp.ten_xa_phuong " +
                        "WHEN xp.loai = 'xa' " +
                        "THEN 'Xã ' || xp.ten_xa_phuong " +
                        "ELSE xp.ten_xa_phuong " +
                        "END = @dia_ban " +
                        ")");

                    command.Parameters.Add(
                        "@dia_ban",
                        NpgsqlDbType.Text).Value =
                        criteria.DiaBan.Trim();
                }

                // -------------------------------------------------
                // TRẠNG THÁI QUẢN LÝ
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    criteria.TrangThai))
                {
                    conditions.Add(
                        "dv.trang_thai_quan_ly = @trang_thai");

                    command.Parameters.Add(
                        "@trang_thai",
                        NpgsqlDbType.Text).Value =
                        criteria.TrangThai.Trim();
                }

                string whereSql =
                    conditions.Count > 0
                        ? " WHERE " +
                          string.Join(
                              " AND ",
                              conditions.ToArray())
                        : "";

                // -------------------------------------------------
                // KẾT QUẢ
                //
                // Có thêm 2 alias tương thích:
                // - dia_ban
                // - dia_chi_hanh_chinh
                //
                // Đây chỉ là tên cột KẾT QUẢ để UnitSearchForm.cs cũ
                // không lỗi; CSDL không còn hai cột này.
                // -------------------------------------------------
                command.CommandText =
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
                    "dv.trang_thai_quan_ly, " +
                    "CASE " +
                    "WHEN dv.geom IS NULL OR ST_IsEmpty(dv.geom) " +
                    "THEN FALSE ELSE TRUE " +
                    "END AS co_diem_dai_dien_gis, " +

                    // Alias tương thích giao diện cũ.
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

                    whereSql +

                    " ORDER BY " +
                    "xp.ten_xa_phuong, " +
                    "dv.ten_don_vi, " +
                    "dv.ma_don_vi;";

                using (NpgsqlDataAdapter adapter =
                    new NpgsqlDataAdapter(
                        command))
                {
                    DataTable table =
                        new DataTable(
                            "ket_qua_tra_cuu_don_vi");

                    adapter.Fill(
                        table);

                    return table;
                }
            }
        }

        // =========================================================
        // DANH MỤC CẤP ĐƠN VỊ
        // =========================================================
        public DataTable GetCapDonVi()
        {
            return ExecuteDataTable(
                "SELECT " +
                "id, ma_cap, ten_cap " +
                "FROM " +
                QuoteQualifiedTable(
                    "cap_don_vi") +
                " ORDER BY thu_tu_hien_thi, ten_cap;",
                "cap_don_vi");
        }

        // =========================================================
        // DANH MỤC LOẠI ĐƠN VỊ
        // =========================================================
        public DataTable GetLoaiDonVi()
        {
            return ExecuteDataTable(
                "SELECT " +
                "id, ma_loai, ten_loai " +
                "FROM " +
                QuoteQualifiedTable(
                    "loai_don_vi") +
                " ORDER BY ten_loai;",
                "loai_don_vi");
        }

        // =========================================================
        // DANH MỤC 126 XÃ / PHƯỜNG HÀ NỘI
        //
        // Dùng cho UnitSearchForm.cs mới.
        // =========================================================
        public DataTable GetXaPhuong()
        {
            return ExecuteDataTable(
                "SELECT " +
                "id, " +
                "ma_xa_phuong, " +
                "ten_xa_phuong, " +
                "loai, " +
                "CASE " +
                "WHEN loai = 'phuong' " +
                "THEN 'Phường ' || ten_xa_phuong " +
                "WHEN loai = 'xa' " +
                "THEN 'Xã ' || ten_xa_phuong " +
                "ELSE ten_xa_phuong " +
                "END AS ten_hien_thi " +
                "FROM " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " WHERE tinh_thanh_pho = 'Thành phố Hà Nội' " +
                "ORDER BY " +
                "CASE WHEN loai = 'phuong' THEN 0 ELSE 1 END, " +
                "ten_xa_phuong;",
                "xa_phuong");
        }

        // =========================================================
        // TƯƠNG THÍCH UnitSearchForm.cs CŨ
        //
        // Không còn dữ liệu "địa bàn" riêng.
        // Hàm này trả danh sách xã/phường dưới tên cột dia_ban để
        // form cũ vẫn chạy cho đến khi được cập nhật.
        // =========================================================
        public DataTable GetDiaBan()
        {
            return ExecuteDataTable(
                "SELECT " +
                "id AS xa_phuong_id, " +
                "CASE " +
                "WHEN loai = 'phuong' " +
                "THEN 'Phường ' || ten_xa_phuong " +
                "WHEN loai = 'xa' " +
                "THEN 'Xã ' || ten_xa_phuong " +
                "ELSE ten_xa_phuong " +
                "END AS dia_ban " +
                "FROM " +
                QuoteQualifiedTable(
                    "dm_xa_phuong") +
                " WHERE tinh_thanh_pho = 'Thành phố Hà Nội' " +
                "ORDER BY dia_ban;",
                "dia_ban");
        }

        // =========================================================
        // EXECUTE SELECT -> DATATABLE
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