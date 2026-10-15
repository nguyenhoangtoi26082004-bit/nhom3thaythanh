
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ============================================================
    // GIAO DIEN HE THONG GIS - PHIEN BAN 7.3
    //
    // De tai:
    // Xay dung CSDL GIS tra cuu thong tin va quan ly
    // cac don vi quan doi tren dia ban Ha Noi.
    //
    // Khac phuc:
    // - Banner Tong quan bi thu hep.
    // - Thanh thao tac ngang ban do bi che khuat.
    // - Nhan dieu huong bi xuong dong.
    // - Trung lap nut chuc nang.
    //
    // Khong thay doi du lieu, MapWinGIS hay Designer.
    // ============================================================

    public partial class Form1
    {
        private bool _giaoDienHeThongDaKhoiTao;
        private bool _daGanSuKienDieuHuong;

        private Panel _thongTinThanhBen;
        private Panel _thanhThaoTacBanDo;

        private FlowLayoutPanel _luongDieuHuong;

        private readonly Color _mauNenChinh =
            Color.FromArgb(8, 15, 29);

        private readonly Color _mauThanhBen =
            Color.FromArgb(10, 18, 35);

        private readonly Color _mauThe =
            Color.FromArgb(20, 31, 51);

        private readonly Color _mauVien =
            Color.FromArgb(43, 61, 83);

        private readonly Color _mauXanh =
            Color.FromArgb(5, 150, 105);

        private readonly Color _mauChuPhu =
            Color.FromArgb(148, 163, 184);

        // ========================================================
        // 1. KHOI TAO GIAO DIEN
        // ========================================================

        private void ApDungGiaoDienHeThong()
        {
            if (_giaoDienHeThongDaKhoiTao)
                return;

            if (_modernHeader == null ||
                _modernNav == null)
                return;

            SuspendLayout();

            try
            {
                BackColor = _mauNenChinh;

                ChinhThanhTieuDe();
                ChinhThanhChucNang();
                ThemThongTinHeThong();

                // Khong tao thanh cong cu ngang nam de
                // len MapWinGIS ActiveX.
                ThemThanhThaoTacBanDo();

                // Chinh sua banner cua Tong quan.
                SuaBoCucTongQuan();

                UpdateModernSidebarBounds();

                Shown -= Form1_GiaoDienHeThong_Shown;
                Shown += Form1_GiaoDienHeThong_Shown;

                _giaoDienHeThongDaKhoiTao = true;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        // ========================================================
        // 2. THANH TIEU DE
        // ========================================================

        private void ChinhThanhTieuDe()
        {
            if (_modernHeader == null)
                return;

            _modernHeader.SuspendLayout();

            try
            {
                _modernHeader.BackColor =
                    _mauNenChinh;

                _modernHeader.Height = 86;

                DinhDangNenThanhTieuDe(
                    _modernHeader);

                ChinhNhanTieuDe(
                    _modernHeader);

                if (_modernQuickSearch != null)
                {
                    _modernQuickSearch.BackColor =
                        Color.FromArgb(13, 24, 43);

                    _modernQuickSearch.ForeColor =
                        Color.White;

                    _modernQuickSearch.Font =
                        new Font("Segoe UI", 9.5F);

                    _modernQuickSearch.BorderStyle =
                        BorderStyle.FixedSingle;
                }

                Control duongVien =
                    TimControlTheoTen(
                        _modernHeader,
                        "duongVienTieuDe");

                if (duongVien == null)
                {
                    Panel vien = new Panel
                    {
                        Name = "duongVienTieuDe",
                        Dock = DockStyle.Bottom,
                        Height = 2,
                        BackColor =
                            Color.FromArgb(22, 63, 75)
                    };

                    _modernHeader.Controls.Add(vien);
                    vien.BringToFront();
                }

                ChinhNutQuanLyHoSo();
            }
            finally
            {
                _modernHeader.ResumeLayout(true);
            }
        }

        // ========================================================
        // 3. MAU NEN THANH TIEU DE
        // ========================================================

        private void DinhDangNenThanhTieuDe(
            Control control)
        {
            if (control == null)
                return;

            foreach (Control child in control.Controls)
            {
                if (child is Panel ||
                    child is TableLayoutPanel ||
                    child is FlowLayoutPanel)
                {
                    child.BackColor = _mauNenChinh;
                }

                DinhDangNenThanhTieuDe(child);
            }
        }

        // ========================================================
        // 4. NHAN TIEU DE
        // ========================================================

        private void ChinhNhanTieuDe(
            Control control)
        {
            if (control == null)
                return;

            foreach (Control child in control.Controls)
            {
                Label label = child as Label;

                if (label != null)
                {
                    string text = label.Text ?? "";

                    if (text.StartsWith(
                        "HỆ THỐNG CSDL GIS",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        label.ForeColor = Color.White;

                        label.Font = new Font(
                            "Segoe UI",
                            11F,
                            FontStyle.Bold);
                    }
                    else if (text.StartsWith(
                        "Tra cứu thông tin",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        label.ForeColor =
                            Color.FromArgb(
                                147, 197, 253);
                    }
                }

                ChinhNhanTieuDe(child);
            }
        }

        // ========================================================
        // 5. NUT QUAN LY HO SO
        // ========================================================

        private void ChinhNutQuanLyHoSo()
        {
            Button nutCu = TimNutTrongControl(
                _modernHeader,
                "+ Thêm đơn vị");

            if (nutCu == null ||
                nutCu.Parent == null)
                return;

            Control parent = nutCu.Parent;

            // An nut them du lieu cu.
            nutCu.Visible = false;

            Button nutQuanLy = TimNutTheoTen(
                parent,
                "btnQuanLyHoSoChinh");

            if (nutQuanLy == null)
            {
                nutQuanLy = TaoNutHeThong(
                    "Quản lý hồ sơ",
                    132);

                nutQuanLy.Name =
                    "btnQuanLyHoSoChinh";

                nutQuanLy.Margin =
                    new Padding(4, 1, 3, 0);

                nutQuanLy.Click += delegate
                {
                    Button nav = TimNutDieuHuong(
                        "Quản lý dữ liệu");

                    if (nav != null)
                    {
                        nav.PerformClick();
                    }
                    else
                    {
                        MoQuanLyDuLieuTongHop();
                    }
                };

                parent.Controls.Add(nutQuanLy);
            }

            nutQuanLy.Visible = true;
            nutQuanLy.BringToFront();
        }

        // ========================================================
        // 6. THANH DIEU HUONG BEN TRAI
        // ========================================================

        private void ChinhThanhChucNang()
        {
            if (_modernNav == null)
                return;

            _modernNav.BackColor = _mauThanhBen;

            _luongDieuHuong = null;

            foreach (Control control
                in _modernNav.Controls)
            {
                FlowLayoutPanel flow =
                    control as FlowLayoutPanel;

                if (flow != null)
                {
                    _luongDieuHuong = flow;
                    break;
                }
            }

            if (_luongDieuHuong != null)
            {
                _luongDieuHuong.BackColor =
                    _mauThanhBen;

                _luongDieuHuong.FlowDirection =
                    FlowDirection.TopDown;

                _luongDieuHuong.WrapContents = false;
                _luongDieuHuong.AutoScroll = true;

                if (!_daGanSuKienDieuHuong)
                {
                    _luongDieuHuong.Resize +=
                        LuongDieuHuong_Resize;

                    _daGanSuKienDieuHuong = true;
                }
            }

            foreach (Button button
                in _modernNavButtons)
            {
                if (button == null ||
                    button.IsDisposed)
                    continue;

                ChinhNutDieuHuong(button);

                if ((button.Text ?? "").Trim() ==
                    "Tổng quan")
                {
                    // Kiem tra lai banner sau khi
                    // chuc nang Tong quan hien thi.
                    button.Click -= TongQuan_GiaoDien_Click;
                    button.Click += TongQuan_GiaoDien_Click;
                }
            }

            AnChucNangMauTrungLap();

            CapNhatKichThuocNutDieuHuong();
        }

        // ========================================================
        // 7. DINH DANG NUT DIEU HUONG
        // ========================================================

        private void ChinhNutDieuHuong(Button button)
        {
            button.AutoSize = false;

            button.Height = 42;
            button.Width = 172;

            button.Margin =
                new Padding(0, 2, 0, 3);

            button.Padding =
                new Padding(27, 0, 2, 0);

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.Font = new Font(
                "Segoe UI",
                8.25F,
                FontStyle.Bold);

            button.AutoEllipsis = true;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;

            button.FlatAppearance.MouseOverBackColor =
                _mauThe;

            button.Cursor = Cursors.Hand;

            button.Paint -= VeBieuTuongDieuHuong;
            button.Paint += VeBieuTuongDieuHuong;
        }

        // ========================================================
        // 8. TU DONG CHINH CHIEU RONG MENU
        // ========================================================

        private void LuongDieuHuong_Resize(
            object sender,
            EventArgs e)
        {
            CapNhatKichThuocNutDieuHuong();
        }

        private void CapNhatKichThuocNutDieuHuong()
        {
            if (_luongDieuHuong == null ||
                _luongDieuHuong.IsDisposed)
                return;

            int width =
                _luongDieuHuong.ClientSize.Width;

            if (width <= 0)
                return;

            int usableWidth =
                width -
                _luongDieuHuong.Padding.Left -
                _luongDieuHuong.Padding.Right - 5;

            if (_luongDieuHuong.VerticalScroll.Visible)
            {
                usableWidth -=
                    SystemInformation.VerticalScrollBarWidth;
            }

            usableWidth = Math.Max(
                130,
                Math.Min(172, usableWidth));

            foreach (Button button
                in _modernNavButtons)
            {
                if (button == null ||
                    button.IsDisposed ||
                    button.Parent != _luongDieuHuong)
                    continue;

                if (button.Width != usableWidth)
                {
                    button.Width = usableWidth;
                }
            }
        }

        // ========================================================
        // 9. VE BIEU TUONG MENU
        // ========================================================

        private void VeBieuTuongDieuHuong(
            object sender,
            PaintEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
                return;

            bool active = Object.ReferenceEquals(
                button,
                _activeNavButton);

            string symbol = "■";

            switch ((button.Text ?? "").Trim())
            {
                case "Tổng quan":
                    symbol = "▦";
                    break;

                case "Bản đồ GIS":
                    symbol = "◇";
                    break;

                case "Tra cứu đơn vị":
                case "Tra cứu hồ sơ":
                    symbol = "⌕";
                    break;

                case "Quản lý dữ liệu":
                    symbol = "▤";
                    break;

                case "Phân tích không gian":
                case "Truy vấn không gian":
                    symbol = "◎";
                    break;

                case "Thống kê và báo cáo":
                    symbol = "▥";
                    break;

                case "Kiểm tra dữ liệu":
                case "Kiểm tra CSDL":
                    symbol = "✓";
                    break;

                case "Nhật ký hệ thống":
                    symbol = "≡";
                    break;
            }

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using (Font font = new Font(
                "Segoe UI Symbol", 12F))
            using (Brush brush = new SolidBrush(
                active
                    ? Color.White
                    : _mauChuPhu))
            {
                e.Graphics.DrawString(
                    symbol,
                    font,
                    brush,
                    new PointF(8, 10));
            }

            if (active)
            {
                using (Brush brush = new SolidBrush(
                    Color.FromArgb(110, 231, 183)))
                {
                    e.Graphics.FillRectangle(
                        brush,
                        0,
                        6,
                        3,
                        Math.Max(0, button.Height - 12));
                }
            }
        }

        // ========================================================
        // 10. AN NUT CHUC NANG TRUNG LAP
        // ========================================================

        private void AnChucNangMauTrungLap()
        {
            if (_modernNav == null)
                return;

            Control nutCu = TimControlTheoTen(
                _modernNav,
                "btnDuLieuWebMau");

            if (nutCu != null)
                nutCu.Visible = false;

            AnNutDuLieuMauTheoNhan(_modernNav);
        }

        private void AnNutDuLieuMauTheoNhan(
            Control parent)
        {
            if (parent == null)
                return;

            foreach (Control child in parent.Controls)
            {
                Button button = child as Button;

                if (button != null)
                {
                    string text = button.Text ?? "";

                    if (text.IndexOf(
                        "Bộ dữ liệu Web",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        button.Visible = false;
                    }
                }

                AnNutDuLieuMauTheoNhan(child);
            }
        }

        // ========================================================
        // 11. THONG TIN HE THONG
        // ========================================================

        private void ThemThongTinHeThong()
        {
            if (_modernNav == null)
                return;

            if (_thongTinThanhBen != null &&
                !_thongTinThanhBen.IsDisposed)
                return;

            _thongTinThanhBen = new Panel
            {
                Name = "panelThongTinHeThong",
                Dock = DockStyle.Bottom,
                Height = 119,
                BackColor = _mauThanhBen,
                Padding = new Padding(11, 8, 8, 7)
            };

            Label title = new Label
            {
                Text = "THÔNG TIN HỆ THỐNG",
                Dock = DockStyle.Top,
                Height = 23,
                ForeColor =
                    Color.FromArgb(52, 211, 153),
                Font = new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold)
            };

            Label info = new Label
            {
                Text =
                    "Cơ sở dữ liệu: PostGIS\r\n" +
                    "Hệ tọa độ: EPSG:32648\r\n" +
                    "Phạm vi: Hà Nội\r\n" +
                    "Dữ liệu minh họa cần xác minh",
                Dock = DockStyle.Fill,
                ForeColor = _mauChuPhu,
                Font = new Font("Segoe UI", 7.6F)
            };

            _thongTinThanhBen.Controls.Add(info);
            _thongTinThanhBen.Controls.Add(title);

            _modernNav.Controls.Add(
                _thongTinThanhBen);

            _thongTinThanhBen.BringToFront();
        }

        // ========================================================
        // 12. SUA LOI BANNER TONG QUAN BI HEP
        //
        // Banner nam trong hang dau cua TableLayoutPanel.
        // Can dat Dock=Fill de chiem het chieu rong.
        // ========================================================

        private void SuaBoCucTongQuan()
        {
            if (_tongQuanControl == null ||
                _tongQuanControl.IsDisposed)
                return;

            Label tieuDe = TimNhanTongQuan(
                _tongQuanControl);

            if (tieuDe == null)
                return;

            Panel banner = tieuDe.Parent as Panel;

            if (banner == null)
                return;

            // Chi can thiep khi banner la mot the
            // cua bang bo cuc Tong quan.
            TableLayoutPanel bang =
                banner.Parent as TableLayoutPanel;

            if (bang == null)
                return;

            banner.SuspendLayout();

            try
            {
                banner.AutoSize = false;
                banner.MinimumSize = Size.Empty;

                banner.Dock = DockStyle.Fill;
                banner.Margin = new Padding(7);

                // Khong tu dat Width co dinh.
                // TableLayoutPanel se cap chieu rong.
                banner.Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right;
            }
            finally
            {
                banner.ResumeLayout(true);
            }

            bang.PerformLayout();
            banner.PerformLayout();
        }

        private static Label TimNhanTongQuan(
            Control parent)
        {
            if (parent == null)
                return null;

            foreach (Control child in parent.Controls)
            {
                Label label = child as Label;

                if (label != null &&
                    (label.Text ?? "").StartsWith(
                        "TỔNG QUAN",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return label;
                }

                Label found = TimNhanTongQuan(child);

                if (found != null)
                    return found;
            }

            return null;
        }

        private void TongQuan_GiaoDien_Click(
            object sender,
            EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            // Hoan tat xu ly su kien hien thi Tong quan
            // truoc khi tinh lai bo cuc.
            BeginInvoke((MethodInvoker)delegate
            {
                if (!IsDisposed && !Disposing)
                {
                    SuaBoCucTongQuan();
                }
            });
        }

        // ========================================================
        // 13. KHAC PHUC THANH CONG CU NGANG BAN DO
        //
        // MapWinGIS ActiveX co the che cac control nam
        // trong cung panel. Khong dat thanh ngang len
        // panelMap; su dung thanh cong cu doc hien co.
        // ========================================================

        private void ThemThanhThaoTacBanDo()
        {
            if (panelMap == null)
                return;

            // Goi ham nay van an toan trong Form1.cs
            // va khong tao them control moi.
            panelMap.Resize -= PanelMap_GiaoDien_Resize;

            if (_thanhThaoTacBanDo != null &&
                !_thanhThaoTacBanDo.IsDisposed)
            {
                panelMap.Controls.Remove(
                    _thanhThaoTacBanDo);

                _thanhThaoTacBanDo.Dispose();
            }

            _thanhThaoTacBanDo = null;

            Control thanhCu = TimControlTheoTen(
                panelMap,
                "panelThaoTacBanDoGIS");

            if (thanhCu == null)
            {
                thanhCu = TimControlTheoTen(
                    panelMap,
                    "thanhThaoTacBanDo");
            }

            if (thanhCu != null &&
                !Object.ReferenceEquals(
                    thanhCu, panelMap))
            {
                if (thanhCu.Parent != null)
                    thanhCu.Parent.Controls.Remove(thanhCu);

                thanhCu.Dispose();
            }
        }

        private void PanelMap_GiaoDien_Resize(
            object sender,
            EventArgs e)
        {
            DinhViThanhThaoTacBanDo();
        }

        private void DinhViThanhThaoTacBanDo()
        {
            // Da bo thanh ngang bi che.
            // Cong cu doc MapWinGIS duoc giu nguyen.
        }

        // ========================================================
        // 14. QUAY LAI BAN DO
        // ========================================================

        private void HienTrangBanDo()
        {
            Button nav = TimNutDieuHuong(
                "Bản đồ GIS");

            if (nav != null)
                nav.PerformClick();
            else
                AnTongQuan();
        }

        // ========================================================
        // 15. TRANG DAU TIEN
        // ========================================================

        private void Form1_GiaoDienHeThong_Shown(
            object sender,
            EventArgs e)
        {
            Shown -= Form1_GiaoDienHeThong_Shown;

            if (IsDisposed || Disposing)
                return;

            BeginInvoke((MethodInvoker)delegate
            {
                if (IsDisposed || Disposing)
                    return;

                CapNhatKichThuocNutDieuHuong();

                Button tongQuan = TimNutDieuHuong(
                    "Tổng quan");

                if (tongQuan != null)
                {
                    tongQuan.PerformClick();
                }
                else
                {
                    HienThiTongQuan();
                }

                SuaBoCucTongQuan();
            });
        }

        // ========================================================
        // 16. TAO NUT HE THONG
        // ========================================================

        private Button TaoNutHeThong(
            string text,
            int width)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 33,
                BackColor = _mauXanh,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign =
                    ContentAlignment.MiddleCenter
            };

            button.FlatAppearance.BorderSize = 0;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(4, 120, 87);

            return button;
        }

        // ========================================================
        // 17. TIM NUT DIEU HUONG
        // ========================================================

        private Button TimNutDieuHuong(string ten)
        {
            foreach (Button button in _modernNavButtons)
            {
                if (button == null ||
                    button.IsDisposed ||
                    !button.Visible)
                    continue;

                if (string.Equals(
                    (button.Text ?? "").Trim(),
                    ten,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return button;
                }
            }

            return null;
        }

        // ========================================================
        // 18. TIM NUT THEO CHU
        // ========================================================

        private static Button TimNutTrongControl(
            Control root,
            string text)
        {
            if (root == null)
                return null;

            foreach (Control child in root.Controls)
            {
                Button button = child as Button;

                if (button != null &&
                    string.Equals(
                        (button.Text ?? "").Trim(),
                        text,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return button;
                }

                Button found = TimNutTrongControl(
                    child, text);

                if (found != null)
                    return found;
            }

            return null;
        }

        // ========================================================
        // 19. TIM NUT THEO NAME
        // ========================================================

        private static Button TimNutTheoTen(
            Control root,
            string name)
        {
            return TimControlTheoTen(root, name)
                as Button;
        }

        // ========================================================
        // 20. TIM CONTROL THEO NAME
        // ========================================================

        private static Control TimControlTheoTen(
            Control root,
            string name)
        {
            if (root == null)
                return null;

            if (string.Equals(
                root.Name,
                name,
                StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            foreach (Control child in root.Controls)
            {
                Control found = TimControlTheoTen(
                    child, name);

                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
