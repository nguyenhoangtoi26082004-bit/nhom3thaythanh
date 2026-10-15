
using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // TICH HOP CAC PHAN HE HE THONG GIS HA NOI
    //
    // De tai:
    // Xay dung CSDL GIS tra cuu thong tin va quan ly
    // cac don vi quan doi tren dia ban Ha Noi.
    //
    // Chuc nang:
    // 1. Tong quan
    // 2. Ban do GIS
    // 3. Tra cuu don vi
    // 4. Quan ly du lieu
    // 5. Phan tich khong gian
    // 6. Thong ke va bao cao
    // 7. Kiem tra chat luong du lieu
    //
    // Su dung PostgreSQL / PostGIS.
    // He toa do EPSG:32648.
    //
    // Khong thay doi cau truc CSDL.
    // ============================================================

    public partial class Form1
    {
        private TongQuanControl _tongQuanControl;

        private bool _tongQuanDaKhoiTao = false;

        // ========================================================
        // 1. KHOI TAO CAC PHAN HE
        // ========================================================

        private void InitTongQuanModule()
        {
            if (_tongQuanDaKhoiTao)
                return;

            if (_modernNav == null ||
                panelMapWorkspace == null)
                return;

            FlowLayoutPanel navFlow = null;

            foreach (Control control in _modernNav.Controls)
            {
                navFlow = control as FlowLayoutPanel;

                if (navFlow != null)
                    break;
            }

            if (navFlow == null)
                return;

            // ----------------------------------------------------
            // Chuan hoa cac chuc nang tren thanh dieu huong.
            // ----------------------------------------------------

            ChuanHoaThanhChucNangV2();

            // ----------------------------------------------------
            // An nut quan ly du lieu cu.
            // ----------------------------------------------------

            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                if (string.Equals(
                    button.Text.Trim(),
                    "Quản lý dữ liệu",
                    StringComparison.OrdinalIgnoreCase))
                {
                    button.Visible = false;
                }
            }

            // ====================================================
            // 2. TAO NUT TONG QUAN
            // ====================================================

            Button btnTongQuan = CreateNavButton(
                "Tổng quan",
                delegate
                {
                    HienThiTongQuan();
                },
                false);

            navFlow.Controls.Add(btnTongQuan);

            navFlow.Controls.SetChildIndex(
                btnTongQuan,
                0);

            // ====================================================
            // 3. TAO NUT QUAN LY DU LIEU THONG NHAT
            // ====================================================

            Button btnQuanLyDuLieu = CreateNavButton(
                "Quản lý dữ liệu",
                delegate
                {
                    // Su dung module quan ly du lieu
                    // dang doc 50 ho so va 30 dia ban.
                    //
                    // Ten ham giu theo phien ban hien tai
                    // de tranh loi tham chieu.
                    MoQuanLyDuLieuTongHop();
                },
                false);

            navFlow.Controls.Add(btnQuanLyDuLieu);

            // Thu tu:
            // Tong quan
            // Ban do GIS
            // Tra cuu don vi
            // Quan ly du lieu

            navFlow.Controls.SetChildIndex(
                btnQuanLyDuLieu,
                3);

            // ====================================================
            // 4. AN TONG QUAN KHI CHUYEN PHAN HE
            // ====================================================

            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                if (object.ReferenceEquals(
                    button,
                    btnTongQuan))
                {
                    continue;
                }

                button.Click += delegate
                {
                    AnTongQuan();
                };
            }

            // ====================================================
            // 5. TAO GIAO DIEN TONG QUAN
            // ====================================================

            _tongQuanControl = new TongQuanControl();

            _tongQuanControl.Dock = DockStyle.Fill;

            _tongQuanControl.Padding =
                new Padding(0, 22, 0, 0);

            _tongQuanControl.Visible = false;

            // ----------------------------------------------------
            // Tu Tong quan -> Ban do GIS.
            // ----------------------------------------------------

            _tongQuanControl.YeuCauMoBanDo += delegate
            {
                AnTongQuan();

                foreach (Button button in _modernNavButtons)
                {
                    if (button == null)
                        continue;

                    if (string.Equals(
                        button.Text.Trim(),
                        "Bản đồ GIS",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        button.PerformClick();
                        break;
                    }
                }
            };

            // ----------------------------------------------------
            // Tu Tong quan -> Tra cuu don vi.
            //
            // Giu ten su kien da duoc dinh nghia trong
            // TongQuanControl.cs de tuong thich.
            // ----------------------------------------------------

            _tongQuanControl.YeuCauMoDuLieuWeb += delegate
            {
                AnTongQuan();

                // Mo trang Tra cuu noi dung chinh.
                OpenUnitSearchSmooth("");
            };

            panelMapWorkspace.Controls.Add(
                _tongQuanControl);

            _tongQuanDaKhoiTao = true;
        }

        // ========================================================
        // 6. CHUAN HOA THANH CHUC NANG
        // ========================================================

        private void ChuanHoaThanhChucNangV2()
        {
            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                string ten =
                    button.Text == null
                        ? ""
                        : button.Text.Trim();

                switch (ten)
                {
                    case "Bản đồ GIS":

                        button.Visible = true;

                        break;

                    case "Tra cứu hồ sơ":

                        button.Text = "Tra cứu đơn vị";
                        button.Visible = true;

                        break;

                    case "Tra cứu đơn vị":

                        button.Visible = true;

                        break;

                    case "Bảng thuộc tính":

                        button.Text = "Quản lý dữ liệu";
                        button.Visible = false;

                        break;

                    case "Truy vấn không gian":

                        button.Visible = true;

                        break;

                    case "Phân tích không gian":

                        button.Visible = true;

                        break;

                    case "Kiểm tra CSDL":

                        button.Text = "Kiểm tra dữ liệu";
                        button.Visible = true;

                        break;

                    case "Kiểm tra dữ liệu":

                        button.Visible = true;

                        break;

                    case "Vùng đệm":

                    case "Thống kê báo cáo":

                    case "Bảng điều khiển":

                    case "Quản lý CSDL PostGIS":

                    case "Nhật ký hệ thống":

                        button.Visible = false;

                        break;
                }
            }
        }

        // ========================================================
        // 7. CHUC NANG QUAN LY DU LIEU CU
        //
        // Giu lai de tuong thich voi cac module chua
        // duoc chuyen doi.
        //
        // Nut "Quan ly du lieu" hien tai KHONG goi ham nay.
        // ========================================================

        private void MoQuanLyDuLieuV2()
        {
            AnTongQuan();

            QuanLyDuLieuYeuCau yeuCau =
                QuanLyDuLieuYeuCau.Khong;

            string bangDuLieu = "";

            int idDuocChon = -1;

            try
            {
                using (QuanLyDuLieuForm form =
                    new QuanLyDuLieuForm(
                        RefreshPostGISLayerAfterEdit))
                {
                    DialogResult result =
                        form.ShowDialog(this);

                    if (result != DialogResult.OK)
                        return;

                    yeuCau = form.YeuCau;

                    bangDuLieu = form.BangDuocChon;

                    idDuocChon = form.IdDuocChon;
                }

                // ------------------------------------------------
                // KHONG CO YEU CAU
                // ------------------------------------------------

                if (yeuCau ==
                    QuanLyDuLieuYeuCau.Khong)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(bangDuLieu))
                {
                    MessageBox.Show(
                        this,
                        "Chưa xác định được nhóm dữ liệu.",
                        "Quản lý dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                // ------------------------------------------------
                // THEM DOI TUONG
                // ------------------------------------------------

                if (yeuCau ==
                    QuanLyDuLieuYeuCau.Them)
                {
                    StartCreateSpatialObject(
                        bangDuLieu);

                    return;
                }

                // ------------------------------------------------
                // CHON DOI TUONG HOAC THU PHONG
                // ------------------------------------------------

                if (yeuCau ==
                    QuanLyDuLieuYeuCau.ChonBanDo ||
                    yeuCau ==
                    QuanLyDuLieuYeuCau.ThuPhong)
                {
                    bool thuPhong =
                        yeuCau ==
                        QuanLyDuLieuYeuCau.ThuPhong;

                    ChonDuLieuTrenBanDoV2(
                        bangDuLieu,
                        idDuocChon,
                        thuPhong);

                    return;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    CrashLogger.Write(
                        "MoQuanLyDuLieuV2 ERROR: " + ex);
                }
                catch
                {
                }

                MessageBox.Show(
                    this,
                    "Không thể thực hiện chức năng " +
                    "quản lý dữ liệu.\n\n" +
                    ex.Message,
                    "Lỗi quản lý dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // 8. CHON DOI TUONG TREN BAN DO
        //
        // Ham nay phuc vu module quan ly GIS cu.
        // Chua dung de chon 50 diem cua bo du lieu moi.
        // ========================================================

        private void ChonDuLieuTrenBanDoV2(
            string layerKey,
            int databaseId,
            bool thuPhong)
        {
            if (databaseId <= 0)
            {
                MessageBox.Show(
                    this,
                    "Chưa chọn bản ghi cần định vị.",
                    "Chưa chọn đối tượng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (axMap2 == null)
                return;

            // ----------------------------------------------------
            // Nap lop du lieu neu chua co.
            // ----------------------------------------------------

            bool ready = EnsureLayerReady(
                layerKey,
                true);

            if (!ready)
            {
                MessageBox.Show(
                    this,
                    "Không thể nạp lớp dữ liệu:\n" +
                    layerKey,
                    "Lỗi lớp bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            int handle = GetLayerHandleSafe(
                layerKey);

            if (handle < 0)
            {
                MessageBox.Show(
                    this,
                    "Không xác định được lớp trên bản đồ.",
                    "Lỗi bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // ----------------------------------------------------
            // Chon theo ID trong CSDL.
            // ----------------------------------------------------

            bool success =
                AttributeTableService
                    .SelectShapeByDatabaseId(
                        axMap2,
                        handle,
                        databaseId,
                        thuPhong);

            if (!success)
            {
                MessageBox.Show(
                    this,
                    "Không tìm thấy hình học hiển thị " +
                    "cho bản ghi ID = " +
                    databaseId +
                    ".\n\n" +
                    "Có thể bản ghi chưa có dữ liệu GIS " +
                    "hoặc lớp chưa được cập nhật.",
                    "Không thể định vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (lblStatus != null)
            {
                lblStatus.Text = thuPhong
                    ? "Đã thu phóng tới đối tượng ID = " +
                      databaseId
                    : "Đã chọn đối tượng ID = " +
                      databaseId;
            }

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }
        }

        // ========================================================
        // 9. HIEN THI TONG QUAN
        // ========================================================

        private void HienThiTongQuan()
        {
            if (_tongQuanControl == null ||
                _tongQuanControl.IsDisposed)
            {
                return;
            }

            Cursor oldCursor = Cursor.Current;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                using (DataTable data = DocThongKeTongQuan())
                {
                    // Ten ham hien co trong TongQuanControl.
                    // Giu nguyen de cac lop lien quan
                    // khong bi loi tham chieu.
                    _tongQuanControl.HienThiWeb(data);
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Tổng quan hệ thống | " +
                        "Dữ liệu minh họa, chưa xác minh.";
                }
            }
            catch (Exception ex)
            {
                _tongQuanControl.HienThiLoi(
                    ex.Message);

                try
                {
                    CrashLogger.Write(
                        "TongQuan ERROR: " + ex);
                }
                catch
                {
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không thể cập nhật Tổng quan.";
                }
            }
            finally
            {
                Cursor.Current = oldCursor;

                _tongQuanControl.Visible = true;
                _tongQuanControl.BringToFront();
            }
        }

        // ========================================================
        // 10. DOC SO LIEU TONG QUAN TU POSTGIS
        //
        // Hien tai giu ten schema web_mau trong SQL.
        // Chi doi schema sau khi da chuyen du lieu.
        // ========================================================

        private DataTable DocThongKeTongQuan()
        {
            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host = PG_HOST;
            builder.Port = Convert.ToInt32(PG_PORT);
            builder.Database = PG_DB;
            builder.Username = PG_USER;
            builder.Password = PG_PASS;

            builder.Timeout = 5;
            builder.CommandTimeout = 20;
            builder.Pooling = true;

            const string sql = @"
WITH tong AS (
    SELECT
        (SELECT count(*)
         FROM web_mau.don_vi)
            AS so_ho_so_web,

        (SELECT count(*)
         FROM web_mau.quan_huyen)
            AS so_quan_huyen_mau,

        (SELECT count(*)
         FROM web_mau.don_vi
         WHERE geom IS NOT NULL
           AND ST_IsValid(geom)
           AND ST_SRID(geom) = 32648)
            AS so_diem_hop_le,

        (SELECT count(DISTINCT
            coalesce(
                nullif(btrim(quan_binh_chung), ''),
                'Chưa phân loại'))
         FROM web_mau.don_vi)
            AS so_nhom
),
phan_bo AS (
    SELECT
        'NHOM'::text AS nhom_loai,
        coalesce(
            nullif(btrim(quan_binh_chung), ''),
            'Chưa phân loại') AS ten_nhom,
        count(*) AS so_luong
    FROM web_mau.don_vi
    GROUP BY 1, 2

    UNION ALL

    SELECT
        'DIA_BAN'::text AS nhom_loai,
        coalesce(
            nullif(btrim(quan_huyen), ''),
            'Chưa rõ địa bàn') AS ten_nhom,
        count(*) AS so_luong
    FROM web_mau.don_vi
    GROUP BY 1, 2
)
SELECT
    p.nhom_loai,
    p.ten_nhom,
    p.so_luong,
    t.so_ho_so_web,
    t.so_quan_huyen_mau,
    t.so_diem_hop_le,
    t.so_nhom
FROM phan_bo p
CROSS JOIN tong t
ORDER BY
    p.nhom_loai,
    p.so_luong DESC,
    p.ten_nhom;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    builder.ConnectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                connection.Open();

                DataTable table = new DataTable();

                using (NpgsqlDataReader reader =
                    command.ExecuteReader())
                {
                    table.Load(reader);
                }

                return table;
            }
        }

        // ========================================================
        // 11. AN GIAO DIEN TONG QUAN
        // ========================================================

        private void AnTongQuan()
        {
            if (_tongQuanControl == null ||
                _tongQuanControl.IsDisposed)
            {
                return;
            }

            _tongQuanControl.Visible = false;
        }
    }
}
