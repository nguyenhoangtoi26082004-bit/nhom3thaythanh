
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // Chỉ hợp nhất thanh điều hướng.
    // Không sửa nghiệp vụ hoặc dữ liệu.
    public partial class Form1
    {
        private bool _giaoDienV2DaKhoiTao = false;
        private bool _dangChuanHoaMenu = false;
        private bool _daHenCapNhatMenu = false;

        private FlowLayoutPanel _flowChucNangV2;

        private static readonly string[] ThuTuMenuV2 =
        {
            "Tổng quan",
            "Bản đồ GIS",
            "Tra cứu đơn vị",
            "Quản lý dữ liệu",
            "Phân tích không gian",
            "Thống kê và báo cáo",
            "Kiểm tra dữ liệu",
            "Nhật ký hệ thống"
        };

        // Các module khác đã sử dụng OnShown và OnActivated.
        // Giữ OnLoad của file hiện tại.
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            KhoiTaoGopMenuV2();
        }

        private void KhoiTaoGopMenuV2()
        {
            if (_giaoDienV2DaKhoiTao || _modernNav == null)
                return;

            foreach (Control control in _modernNav.Controls)
            {
                FlowLayoutPanel flow =
                    control as FlowLayoutPanel;

                if (flow != null)
                {
                    _flowChucNangV2 = flow;
                    break;
                }
            }

            if (_flowChucNangV2 == null)
                return;

            _giaoDienV2DaKhoiTao = true;

            _flowChucNangV2.ControlAdded +=
                MenuV2_ControlAdded;

            _flowChucNangV2.SizeChanged +=
                MenuV2_SizeChanged;

            // Loại bỏ các nút cũ khỏi thanh bên.
            XoaNutMenuCuV2();

            ChuanHoaMenuV2();
        }

        private void XoaNutMenuCuV2()
        {
            if (_flowChucNangV2 == null)
                return;

            List<Button> canXoa = new List<Button>();

            foreach (Button button in _modernNavButtons)
            {
                if (button == null ||
                    button.IsDisposed ||
                    !_flowChucNangV2.Controls.Contains(button))
                {
                    continue;
                }

                string ten = (button.Text ?? "").Trim();

                // Chức năng vẫn tồn tại tại vị trí chính.
                // Chỉ xóa đường vào cũ trong sidebar.
                if (ten == "Bảng thuộc tính" ||
                    ten == "Vùng đệm" ||
                    ten == "Buffer" ||
                    ten == "Bảng điều khiển" ||
                    ten == "Dashboard" ||
                    ten == "Thống kê báo cáo" ||
                    ten == "Quản lý CSDL PostGIS" ||
                    ten == "Nhật ký hệ thống")
                {
                    canXoa.Add(button);
                }
            }

            foreach (Button button in canXoa)
            {
                XoaNutKhoiSidebarV2(button);
            }
        }

        private void XoaNutKhoiSidebarV2(Button button)
        {
            if (button == null || button.IsDisposed)
                return;

            if (object.ReferenceEquals(
                _activeNavButton, button))
            {
                _activeNavButton = null;
            }

            if (_flowChucNangV2 != null &&
                _flowChucNangV2.Controls.Contains(button))
            {
                _flowChucNangV2.Controls.Remove(button);
            }

            _modernNavButtons.Remove(button);

            button.Dispose();
        }

        private void MenuV2_ControlAdded(
            object sender,
            ControlEventArgs e)
        {
            HenCapNhatMenuV2();
        }

        private void MenuV2_SizeChanged(
            object sender,
            EventArgs e)
        {
            HenCapNhatMenuV2();
        }

        // Gộp các yêu cầu cập nhật giao diện liên tiếp.
        private void HenCapNhatMenuV2()
        {
            if (_dangChuanHoaMenu ||
                _daHenCapNhatMenu ||
                IsDisposed ||
                Disposing ||
                !IsHandleCreated)
            {
                return;
            }

            _daHenCapNhatMenu = true;

            try
            {
                BeginInvoke(
                    new MethodInvoker(delegate
                    {
                        _daHenCapNhatMenu = false;

                        if (!IsDisposed && !Disposing)
                        {
                            ChuanHoaMenuV2();
                        }
                    }));
            }
            catch (InvalidOperationException)
            {
                _daHenCapNhatMenu = false;
            }
        }

        // Chuẩn hóa tên chức năng.
        private static string ChuanHoaTenMenuV2(string ten)
        {
            switch ((ten ?? "").Trim())
            {
                case "Tổng quan":
                case "Bản đồ GIS":
                case "Tra cứu đơn vị":
                case "Quản lý dữ liệu":
                case "Phân tích không gian":
                case "Thống kê và báo cáo":
                case "Kiểm tra dữ liệu":
                case "Nhật ký hệ thống":
                    return ten.Trim();

                case "Tra cứu hồ sơ":
                    return "Tra cứu đơn vị";

                case "Truy vấn không gian":
                    return "Phân tích không gian";

                case "Kiểm tra CSDL":
                    return "Kiểm tra dữ liệu";

                default:
                    return null;
            }
        }

        // Hợp nhất và sắp xếp thanh chức năng.
        private void ChuanHoaMenuV2()
        {
            if (_dangChuanHoaMenu ||
                _flowChucNangV2 == null ||
                _flowChucNangV2.IsDisposed)
            {
                return;
            }

            _dangChuanHoaMenu = true;

            _flowChucNangV2.SuspendLayout();

            try
            {
                // Nếu có hai nút cùng chức năng,
                // giữ nút mới được đăng ký sau cùng.
                Dictionary<string, Button> nutChinh =
                    new Dictionary<string, Button>(
                        StringComparer.OrdinalIgnoreCase);

                foreach (Button button in _modernNavButtons)
                {
                    if (button == null ||
                        button.IsDisposed ||
                        !_flowChucNangV2.Controls.Contains(button))
                    {
                        continue;
                    }

                    string ten =
                        ChuanHoaTenMenuV2(button.Text);

                    if (ten != null)
                    {
                        nutChinh[ten] = button;
                    }
                }

                foreach (Button button in _modernNavButtons)
                {
                    if (button == null ||
                        button.IsDisposed ||
                        !_flowChucNangV2.Controls.Contains(button))
                    {
                        continue;
                    }

                    string ten =
                        ChuanHoaTenMenuV2(button.Text);

                    Button chon;

                    bool hien =
                        ten != null &&
                        nutChinh.TryGetValue(
                            ten, out chon) &&
                        object.ReferenceEquals(button, chon);

                    if (button.Visible != hien)
                    {
                        button.Visible = hien;
                    }

                    if (hien)
                    {
                        if (button.Text != ten)
                        {
                            button.Text = ten;
                        }

                        DinhDangNutMenuV2(button);
                    }
                }

                // Sắp xếp đúng tám mục nghiệp vụ.
                for (int i = ThuTuMenuV2.Length - 1;
                    i >= 0;
                    i--)
                {
                    Button button;

                    if (nutChinh.TryGetValue(
                        ThuTuMenuV2[i], out button) &&
                        button.Visible &&
                        _flowChucNangV2.Controls.Contains(button))
                    {
                        _flowChucNangV2.Controls.SetChildIndex(
                            button, 0);
                    }
                }

                _flowChucNangV2.FlowDirection =
                    FlowDirection.TopDown;

                _flowChucNangV2.WrapContents = false;
                _flowChucNangV2.AutoScroll = true;

                _flowChucNangV2.BackColor =
                    MilitaryTheme.Surface;
            }
            finally
            {
                _flowChucNangV2.ResumeLayout(true);
                _dangChuanHoaMenu = false;
            }
        }

        // Đồng bộ màu và kích thước của các nút.
        private void DinhDangNutMenuV2(Button button)
        {
            int width = Math.Max(
                135,
                _flowChucNangV2.ClientSize.Width -
                _flowChucNangV2.Padding.Horizontal -
                SystemInformation.VerticalScrollBarWidth -
                3);

            if (button.Width != width)
            {
                button.Width = width;
            }

            if (button.Height != 38)
            {
                button.Height = 38;
            }

            Padding margin =
                new Padding(0, 0, 0, 4);

            if (button.Margin != margin)
            {
                button.Margin = margin;
            }

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 0;

            bool active = object.ReferenceEquals(
                button, _activeNavButton);

            Color background = active
                ? MilitaryTheme.Accent
                : MilitaryTheme.Surface;

            Color foreground = active
                ? Color.White
                : MilitaryTheme.TextPrimary;

            if (button.BackColor != background)
            {
                button.BackColor = background;
            }

            if (button.ForeColor != foreground)
            {
                button.ForeColor = foreground;
            }

            button.FlatAppearance.MouseOverBackColor =
                active
                    ? MilitaryTheme.AccentHover
                    : MilitaryTheme.Surface2;

            button.FlatAppearance.MouseDownBackColor =
                active
                    ? MilitaryTheme.AccentHover
                    : MilitaryTheme.Surface3;
        }
    }
}
