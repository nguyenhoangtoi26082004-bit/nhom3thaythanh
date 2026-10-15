
using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // QUICK SEARCH WORKFLOW - GOOGLE AI STUDIO V6
    //
    // Thống nhất ba đường tra cứu:
    // 1. Menu Tra cứu đơn vị
    // 2. Ô Tra cứu nhanh trên thanh tiêu đề
    // 3. Ctrl + F
    //
    // Tái sử dụng WebMauDuLieuForm V5 đang hoạt động.
    // Hiển thị trực tiếp trong vùng làm việc chính.
    //
    // Dữ liệu: HQTCSDL.web_mau
    // - 50 hồ sơ mẫu
    // - 30 địa bàn mẫu
    //
    // Chỉ đọc dữ liệu; không thay đổi PostgreSQL.
    // ============================================================

    public partial class Form1
    {
        private TraCuuDuLieuForm _traCuuWebNoiDung;

        private bool _daGanDongTraCuuWeb = false;
        private bool _dangChuyenChucNangTraCuu = false;

        // ========================================================
        // 1. MỞ MÀN HÌNH TRA CỨU WEB
        // ========================================================
        private void OpenUnitSearchSmooth(string keyword)
        {
            keyword = keyword == null
                ? ""
                : keyword.Trim();

            if (panelMapWorkspace == null)
            {
                MessageBox.Show(
                    this,
                    "Chưa khởi tạo vùng làm việc chính.",
                    "Tra cứu dữ liệu Web",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                // Ẩn Tổng quan để hiển thị trang Tra cứu.
                AnTongQuan();

                // Không tạo thêm biểu mẫu nếu trang cũ còn mở.
                if (_traCuuWebNoiDung == null ||
                    _traCuuWebNoiDung.IsDisposed)
                {
                    TaoTrangTraCuuWeb(keyword);
                }
                else if (keyword.Length > 0)
                {
                    ApDungTuKhoaWeb(
                        _traCuuWebNoiDung,
                        keyword);
                }

                if (_traCuuWebNoiDung == null ||
                    _traCuuWebNoiDung.IsDisposed)
                {
                    return;
                }

                _traCuuWebNoiDung.Visible = true;
                _traCuuWebNoiDung.BringToFront();

                GanDongTrangKhiChuyenChucNang();
                DanhDauMenuTraCuu();

                if (lblStatus != null)
                {
                    lblStatus.Text = keyword.Length == 0
                        ? "Tra cứu 50 hồ sơ Web mẫu."
                        : "Tra cứu Web mẫu: " + keyword;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Không thể mở trang Tra cứu Web.\n\n" +
                    ex.Message,
                    "Tra cứu đơn vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // 2. TẠO TRANG WEB BÊN TRONG VÙNG LÀM VIỆC
        // ========================================================
        private void TaoTrangTraCuuWeb(string keyword)
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

            TraCuuDuLieuForm trang =
                new TraCuuDuLieuForm(
                    builder.ConnectionString,
                    HienThiWebMauTrenBanDo);

            // Biến Form V5 thành màn hình nội dung.
            // Không mở thêm một cửa sổ riêng.
            trang.TopLevel = false;
            trang.FormBorderStyle = FormBorderStyle.None;
            trang.ShowInTaskbar = false;
            trang.StartPosition = FormStartPosition.Manual;
            trang.MinimumSize = Size.Empty;
            trang.Dock = DockStyle.Fill;
            trang.Margin = Padding.Empty;
            trang.Padding = Padding.Empty;

            // WebMauDuLieuForm đã đăng ký Shown để tải
            // dữ liệu. Sự kiện này được thêm sau nên
            // sẽ áp dụng từ khóa khi dữ liệu đã tải.
            if (keyword.Length > 0)
            {
                string tuKhoaBanDau = keyword;

                trang.Shown += delegate
                {
                    if (!trang.IsDisposed)
                    {
                        ApDungTuKhoaWeb(
                            trang,
                            tuKhoaBanDau);
                    }
                };
            }

            // Xử lý khi đóng trang Tra cứu.
            trang.FormClosed += delegate
            {
                if (Object.ReferenceEquals(
                    _traCuuWebNoiDung, trang))
                {
                    _traCuuWebNoiDung = null;
                }

                if (IsDisposed ||
                    Disposing ||
                    !IsHandleCreated)
                {
                    return;
                }

                bool xemBanDo = trang.YeuCauMoBanDo;
                bool chuyenChucNang =
                    _dangChuyenChucNangTraCuu;

                // Đưa công việc mở bản đồ về vòng lặp UI,
                // sau khi biểu mẫu Tra cứu đã đóng.
                BeginInvoke((MethodInvoker)delegate
                {
                    if (IsDisposed || Disposing)
                        return;

                    if (xemBanDo)
                    {
                        DanhDauMenuBanDo();
                        HienThiWebMauTrenBanDo();
                    }
                    else if (!chuyenChucNang)
                    {
                        // Người dùng đóng trang Tra cứu:
                        // quay lại nền bản đồ đang có.
                        DanhDauMenuBanDo();
                    }
                });
            };

            _traCuuWebNoiDung = trang;

            panelMapWorkspace.Controls.Add(trang);

            trang.Show();
            trang.BringToFront();
        }

        // ========================================================
        // 3. ÁP DỤNG TỪ KHÓA TRA CỨU NHANH
        //
        // WebMauDuLieuForm V5 có ô tìm kiếm riêng.
        // Ta tìm qua cây điều khiển WinForms mà không
        // sửa trường private của biểu mẫu đó.
        // ========================================================
        private void ApDungTuKhoaWeb(
            TraCuuDuLieuForm trang,
            string keyword)
        {
            if (trang == null || trang.IsDisposed)
                return;

            // Chuyển sang nhóm hồ sơ đơn vị trước.
            Button nutHoSo = TimNutTheoChu(
                trang,
                "50 hồ sơ mẫu");

            if (nutHoSo != null)
            {
                nutHoSo.PerformClick();
            }

            TextBox oTim = TimOTextTimKiem(trang);

            if (oTim == null)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không tìm thấy ô tìm kiếm Web V5.";
                }

                return;
            }

            oTim.Text = keyword;
            oTim.SelectionStart = oTim.TextLength;
            oTim.Focus();
        }

        // ========================================================
        // 4. TÌM Ô NHẬP TỪ KHÓA
        // ========================================================
        private static TextBox TimOTextTimKiem(
            Control container)
        {
            foreach (Control child in container.Controls)
            {
                TextBox textbox = child as TextBox;

                if (textbox != null &&
                    !textbox.ReadOnly &&
                    !textbox.Multiline)
                {
                    return textbox;
                }

                TextBox nested = TimOTextTimKiem(child);

                if (nested != null)
                    return nested;
            }

            return null;
        }

        // ========================================================
        // 5. TÌM NÚT THEO TÊN HIỂN THỊ
        // ========================================================
        private static Button TimNutTheoChu(
            Control container,
            string text)
        {
            foreach (Control child in container.Controls)
            {
                Button button = child as Button;

                if (button != null &&
                    string.Equals(
                        button.Text,
                        text,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return button;
                }

                Button nested = TimNutTheoChu(
                    child, text);

                if (nested != null)
                    return nested;
            }

            return null;
        }

        // ========================================================
        // 6. ĐÓNG TRANG TRA CỨU KHI CHUYỂN CHỨC NĂNG
        // ========================================================
        private void GanDongTrangKhiChuyenChucNang()
        {
            if (_daGanDongTraCuuWeb)
                return;

            if (_modernNavButtons == null)
                return;

            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                string ten = button.Text == null
                    ? ""
                    : button.Text.Trim();

                // Không đóng khi chọn chính menu Tra cứu.
                if (ten == "Tra cứu đơn vị" ||
                    ten == "Tra cứu hồ sơ")
                {
                    continue;
                }

                button.Click += delegate
                {
                    DongTrangTraCuuWeb(true);
                };
            }

            _daGanDongTraCuuWeb = true;
        }

        private void DongTrangTraCuuWeb(
            bool doChuyenChucNang)
        {
            if (_traCuuWebNoiDung == null ||
                _traCuuWebNoiDung.IsDisposed)
            {
                return;
            }

            _dangChuyenChucNangTraCuu =
                doChuyenChucNang;

            try
            {
                _traCuuWebNoiDung.Close();
            }
            finally
            {
                _dangChuyenChucNangTraCuu = false;
            }
        }

        // ========================================================
        // 7. ĐỒNG BỘ NÚT ĐIỀU HƯỚNG
        // ========================================================
        private void DanhDauMenuTraCuu()
        {
            DanhDauMenuWeb("Tra cứu đơn vị");
        }

        private void DanhDauMenuBanDo()
        {
            DanhDauMenuWeb("Bản đồ GIS");
        }

        private void DanhDauMenuWeb(string ten)
        {
            foreach (Button button in _modernNavButtons)
            {
                if (button == null ||
                    !button.Visible)
                {
                    continue;
                }

                if (string.Equals(
                    button.Text.Trim(),
                    ten,
                    StringComparison.OrdinalIgnoreCase))
                {
                    SetActiveNavButton(button);
                    break;
                }
            }
        }
    }
}
