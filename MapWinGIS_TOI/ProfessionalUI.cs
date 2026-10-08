using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        // =========================================================
        // MODERN UI V5 - SIDEBAR TRÁI
        //
        // Chỉ thay đổi giao diện.
        //
        // KHÔNG:
        // - sửa Form1.Designer.cs;
        // - xóa Form1.resx;
        // - thay axMap2.OcxState;
        // - thay logic PostGIS;
        // - thay EPSG:32648.
        // =========================================================

        private bool _professionalUiApplied = false;

        private Panel _modernHeader;
        private Panel _modernNav;

        private FlowLayoutPanel _modernMapTools;

        // Timer chỉ dùng để đồng bộ thanh Zoom dọc với CurrentZoom
        // khi người dùng zoom bằng wheel, công cụ khác hoặc code.
        private System.Windows.Forms.Timer _modernZoomTimer;

        private TextBox _modernQuickSearch;

        private System.Windows.Forms.Label
            _modernDbStatus;

        private ToolTip _modernToolTip;

        private readonly List<Button>
            _modernNavButtons =
                new List<Button>();

        private Button _activeNavButton;

        // =========================================================
        // KHỞI TẠO GIAO DIỆN
        // =========================================================
        private void ApplyProfessionalUI()
        {
            if (_professionalUiApplied)
                return;

            SuspendLayout();

            try
            {
                ConfigureModernWindow();

                // Áp dụng giao diện cho các điều khiển Designer cũ trước.
                //
                // Sau bước này mới tạo Header/Nav mới để tránh
                // tránh cơ chế tự áp dụng giao diện ghi đè màu của giao diện mới.
                MilitaryTheme.ApplyToForm(
                    this);

                HideLegacyTopBars();

                BuildModernHeader();
                BuildModernNavigation();

                Resize -= Form1_ModernUiResize;
                Resize += Form1_ModernUiResize;

                ConfigureModernLayerPanel();
                ConfigureModernMapWorkBar();
                ConfigureModernMapArea();
                ConfigureModernMapToolbar();
                ConfigureModernStatusBar();

                _professionalUiApplied =
                    true;

                MilitaryTheme.StartAutoTheme();

                // Ép layout lần cuối sau khi Form đã có kích thước thật.
                try
                {
                    BeginInvoke(
                        (MethodInvoker)delegate
                        {
                            EnsureModernLayoutAfterShown();
                        });
                }
                catch
                {
                }
            }
            finally
            {
                ResumeLayout(
                    true);
            }
        }

        // =========================================================
        // CỬA SỔ
        // =========================================================
        private void ConfigureModernWindow()
        {
            Text =
                "Hệ thống CSDL GIS - Quản lý đơn vị trên địa bàn Hà Nội";

            Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Regular);

            MinimumSize =
                new Size(
                    1180,
                    720);

            StartPosition =
                FormStartPosition.CenterScreen;

            WindowState =
                FormWindowState.Maximized;

            BackColor =
                MilitaryTheme.AppBackground;
        }

        // =========================================================
        // ẨN MENU / TOOLBAR KIỂU CŨ
        //
        // Không xóa các control.
        // Các chức năng cũ vẫn được gọi bằng PerformClick().
        // =========================================================
        private void HideLegacyTopBars()
        {
            if (menuStrip1 != null)
            {
                menuStrip1.Visible =
                    false;
            }

            if (toolStripTools != null)
            {
                toolStripTools.Visible =
                    false;
            }

            if (toolStripStandard != null)
            {
                // Toolbar Designer cũ chỉ được dùng làm nơi chứa
                // các ToolStripButton có event đã nối.
                //
                // Toolbar mới sẽ gọi PerformClick() vào chúng.
                toolStripStandard.Visible =
                    false;
            }
        }

        // =========================================================
        // TIÊU ĐỀ TRÊN
        // =========================================================
        private void BuildModernHeader()
        {
            if (_modernHeader != null)
                return;

            _modernToolTip =
                new ToolTip();

            _modernToolTip.ShowAlways =
                true;

            _modernToolTip.InitialDelay =
                300;

            _modernHeader =
                new Panel();

            _modernHeader.Name =
                "panelModernHeader";

            _modernHeader.Dock =
                DockStyle.Top;

            _modernHeader.Height =
                80;

            _modernHeader.BackColor =
                MilitaryTheme.AppBackground;

            _modernHeader.Padding =
                new Padding(
                    16,
                    8,
                    16,
                    7);

            _modernHeader.Margin =
                new Padding(0);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock =
                DockStyle.Fill;

            layout.Margin =
                new Padding(0);

            layout.Padding =
                new Padding(0);

            layout.ColumnCount =
                3;

            layout.RowCount =
                1;

            layout.BackColor =
                MilitaryTheme.AppBackground;

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    40F));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    35F));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25F));

            // =====================================================
            // TIÊU ĐỀ
            // =====================================================
            Panel titlePanel =
                new Panel();

            titlePanel.Dock =
                DockStyle.Fill;

            titlePanel.BackColor =
                MilitaryTheme.AppBackground;

            titlePanel.Margin =
                new Padding(0);

            // =====================================================
            // LOGO HỌC VIỆN
            //
            // Không dùng panel nền màu nữa.
            // Ảnh được xử lý để:
            // - xóa nền trắng ở mép ảnh;
            // - cắt phần lề trắng/thừa;
            // - chỉ giữ logo;
            // - phóng logo lớn hơn nhưng không méo.
            // =====================================================
            PictureBox logoPicture =
                new PictureBox();

            logoPicture.Name =
                "picAcademyLogo";

            logoPicture.Size =
                new Size(
                    64,
                    64);

            logoPicture.Location =
                new Point(
                    0,
                    0);

            logoPicture.SizeMode =
                PictureBoxSizeMode.CenterImage;

            logoPicture.BackColor =
                Color.Transparent;

            logoPicture.TabStop =
                false;

            Image academyLogo =
                LoadAcademyLogo();

            if (academyLogo != null)
            {
                logoPicture.Image =
                    academyLogo;
            }
            else
            {
                // Nếu chưa tìm thấy ảnh thì vẫn có chữ GIS dự phòng,
                // không làm chương trình lỗi.
                Bitmap fallback =
                    new Bitmap(
                        64,
                        64);

                using (Graphics g =
                    Graphics.FromImage(
                        fallback))
                {
                    g.Clear(
                        Color.Transparent);

                    using (Font font =
                        new Font(
                            "Segoe UI",
                            11F,
                            FontStyle.Bold))
                    using (Brush brush =
                        new SolidBrush(
                            Color.FromArgb(
                                52,
                                211,
                                153)))
                    {
                        StringFormat format =
                            new StringFormat();

                        format.Alignment =
                            StringAlignment.Center;

                        format.LineAlignment =
                            StringAlignment.Center;

                        g.DrawString(
                            "GIS",
                            font,
                            brush,
                            new RectangleF(
                                0,
                                0,
                                64,
                                64),
                            format);

                        format.Dispose();
                    }
                }

                logoPicture.Image =
                    fallback;
            }

            System.Windows.Forms.Label
                title =
                    new System.Windows.Forms.Label();

            title.AutoSize =
                true;

            title.Location =
                new Point(
                    78,
                    10);

            title.Text =
                "HỆ THỐNG CSDL GIS QUẢN LÝ ĐƠN VỊ HÀ NỘI";

            title.ForeColor =
                MilitaryTheme.TextPrimary;

            title.Font =
                new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold);

            System.Windows.Forms.Label
                subtitle =
                    new System.Windows.Forms.Label();

            subtitle.AutoSize =
                true;

            subtitle.Location =
                new Point(
                    78,
                    39);

            subtitle.Text =
                "Tra cứu thông tin, quản lý dữ liệu không gian và phân tích GIS trên địa bàn Hà Nội";

            subtitle.ForeColor =
                MilitaryTheme.TextSecondary;

            subtitle.Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Regular);

            titlePanel.Controls.Add(
                logoPicture);

            titlePanel.Controls.Add(
                title);

            titlePanel.Controls.Add(
                subtitle);

            // =====================================================
            // TRA CỨU NHANH
            // =====================================================
            Panel searchPanel =
                new Panel();

            searchPanel.Dock =
                DockStyle.Fill;

            searchPanel.BackColor =
                MilitaryTheme.AppBackground;

            searchPanel.Margin =
                new Padding(
                    10,
                    0,
                    10,
                    0);

            System.Windows.Forms.Label
                searchTitle =
                    new System.Windows.Forms.Label();

            searchTitle.AutoSize =
                true;

            searchTitle.Location =
                new Point(
                    4,
                    3);

            searchTitle.Text =
                "TRA CỨU NHANH";

            searchTitle.ForeColor =
                MilitaryTheme.TextSecondary;

            searchTitle.Font =
                new Font(
                    "Segoe UI",
                    7.5F,
                    FontStyle.Bold);

            _modernQuickSearch =
                new TextBox();

            _modernQuickSearch.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;

            _modernQuickSearch.Location =
                new Point(
                    4,
                    26);

            _modernQuickSearch.Height =
                29;

            _modernQuickSearch.BackColor =
                MilitaryTheme.Surface;

            _modernQuickSearch.ForeColor =
                MilitaryTheme.TextPrimary;

            _modernQuickSearch.BorderStyle =
                BorderStyle.FixedSingle;

            _modernQuickSearch.Font =
                new Font(
                    "Segoe UI",
                    9F);

            _modernQuickSearch.KeyDown +=
                modernQuickSearch_KeyDown;

            Button btnQuickSearch =
                new Button();

            btnQuickSearch.Text =
                "Tìm";

            btnQuickSearch.Width =
                60;

            btnQuickSearch.Height =
                30;

            btnQuickSearch.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnQuickSearch.Click +=
                delegate
                {
                    OpenModernQuickSearch();
                };

            MilitaryTheme.StyleButton(
                btnQuickSearch);

            searchPanel.Resize +=
                delegate
                {
                    int width =
                        searchPanel.ClientSize.Width;

                    int buttonWidth =
                        60;

                    int gap =
                        8;

                    _modernQuickSearch.Width =
                        Math.Max(
                            150,
                            width -
                            buttonWidth -
                            gap -
                            12);

                    btnQuickSearch.Left =
                        Math.Max(
                            160,
                            width -
                            buttonWidth -
                            3);

                    btnQuickSearch.Top =
                        25;
                };

            searchPanel.Controls.Add(
                searchTitle);

            searchPanel.Controls.Add(
                _modernQuickSearch);

            searchPanel.Controls.Add(
                btnQuickSearch);

            // =====================================================
            // KHU VỰC THAO TÁC
            // =====================================================
            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock =
                DockStyle.Fill;

            actions.FlowDirection =
                FlowDirection.RightToLeft;

            actions.WrapContents =
                false;

            actions.AutoScroll =
                true;

            actions.Padding =
                new Padding(
                    2,
                    15,
                    0,
                    0);

            actions.Margin =
                new Padding(0);

            actions.BackColor =
                MilitaryTheme.AppBackground;

            Button btnAddUnit =
                new Button();

            btnAddUnit.AutoSize =
                true;

            btnAddUnit.Height =
                31;

            btnAddUnit.Padding =
                new Padding(
                    7,
                    0,
                    7,
                    0);

            btnAddUnit.Text =
                "+ Thêm đơn vị";

            btnAddUnit.Click +=
                delegate
                {
                    StartCreateSpatialObject(
                        "don_vi_quan_doi");
                };

            MilitaryTheme.StyleButton(
                btnAddUnit);

            _modernDbStatus =
                new System.Windows.Forms.Label();

            _modernDbStatus.AutoSize =
                true;

            _modernDbStatus.Padding =
                new Padding(
                    9,
                    7,
                    9,
                    7);

            _modernDbStatus.Margin =
                new Padding(
                    0,
                    1,
                    8,
                    0);

            _modernDbStatus.Text =
                "● PostGIS | Chưa tải";

            _modernDbStatus.ForeColor =
                MilitaryTheme.TextSecondary;

            _modernDbStatus.BackColor =
                MilitaryTheme.Surface2;

            _modernDbStatus.Cursor =
                Cursors.Hand;

            _modernDbStatus.Click +=
                delegate
                {
                    if (btnPostGIS != null)
                    {
                        btnPostGIS.PerformClick();
                    }
                };

            if (_modernToolTip != null)
            {
                _modernToolTip.SetToolTip(
                    _modernDbStatus,
                    "Kết nối hoặc tải lại dữ liệu PostgreSQL/PostGIS.");
            }

            actions.Controls.Add(
                btnAddUnit);

            actions.Controls.Add(
                _modernDbStatus);

            layout.Controls.Add(
                titlePanel,
                0,
                0);

            layout.Controls.Add(
                searchPanel,
                1,
                0);

            layout.Controls.Add(
                actions,
                2,
                0);

            _modernHeader.Controls.Add(
                layout);

            Controls.Add(
                _modernHeader);

            _modernHeader.BringToFront();
        }

        // =========================================================
        // TRA CỨU NHANH
        // =========================================================
        private void modernQuickSearch_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode ==
                Keys.Enter)
            {
                e.SuppressKeyPress =
                    true;

                OpenModernQuickSearch();
            }
        }

        private void OpenModernQuickSearch()
        {
            string keyword =
                _modernQuickSearch == null
                    ? ""
                    : _modernQuickSearch.Text.Trim();

            OpenUnitSearchSmooth(
                keyword);

            if (lblStatus == null)
                return;

            if (string.IsNullOrWhiteSpace(
                keyword))
            {
                lblStatus.Text =
                    "Đã mở chức năng tra cứu hồ sơ.";
            }
            else
            {
                lblStatus.Text =
                    "Tra cứu hồ sơ | Từ khóa: " +
                    keyword;
            }
        }

        // =========================================================
        // THANH CHỨC NĂNG
        // =========================================================
        private void BuildModernNavigation()
        {
            if (_modernNav != null)
                return;

            // =====================================================
            // SIDEBAR CHỨC NĂNG BÊN TRÁI
            //
            // Giữ tên _modernNav để không phải sửa các phần khác
            // của ProfessionalUI.cs.
            // =====================================================
            _modernNav =
                new Panel();

            _modernNav.Name =
                "panelModernSidebar";

            _modernNav.Dock =
                DockStyle.None;

            _modernNav.Width =
                196;

            _modernNav.MinimumSize =
                new Size(
                    196,
                    220);

            _modernNav.BackColor =
                MilitaryTheme.Surface;

            _modernNav.Padding =
                new Padding(
                    8,
                    8,
                    8,
                    8);

            _modernNav.Margin =
                new Padding(0);

            _modernNav.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left;

            // =====================================================
            // TIÊU ĐỀ TRÊN SIDEBAR
            // =====================================================
            Panel sidebarHeader =
                new Panel();

            sidebarHeader.Dock =
                DockStyle.Top;

            sidebarHeader.Height =
                54;

            sidebarHeader.BackColor =
                MilitaryTheme.Surface;

            sidebarHeader.Padding =
                new Padding(
                    10,
                    6,
                    6,
                    4);

            System.Windows.Forms.Label sidebarTitle =
                new System.Windows.Forms.Label();

            sidebarTitle.Dock =
                DockStyle.Top;

            sidebarTitle.Height =
                22;

            sidebarTitle.Text =
                "CHỨC NĂNG";

            sidebarTitle.TextAlign =
                ContentAlignment.MiddleLeft;

            sidebarTitle.ForeColor =
                MilitaryTheme.TextPrimary;

            sidebarTitle.Font =
                new Font(
                    "Segoe UI",
                    9.2F,
                    FontStyle.Bold);

            System.Windows.Forms.Label sidebarSubTitle =
                new System.Windows.Forms.Label();

            sidebarSubTitle.Dock =
                DockStyle.Fill;

            sidebarSubTitle.Text =
                "Hệ thống GIS Hà Nội";

            sidebarSubTitle.TextAlign =
                ContentAlignment.MiddleLeft;

            sidebarSubTitle.ForeColor =
                MilitaryTheme.TextSecondary;

            sidebarSubTitle.Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Regular);

            sidebarHeader.Controls.Add(
                sidebarSubTitle);

            sidebarHeader.Controls.Add(
                sidebarTitle);

            // =====================================================
            // DANH SÁCH CHỨC NĂNG
            // =====================================================
            FlowLayoutPanel flow =
                new FlowLayoutPanel();

            flow.Dock =
                DockStyle.Fill;

            flow.FlowDirection =
                FlowDirection.TopDown;

            flow.WrapContents =
                false;

            flow.AutoScroll =
                true;

            flow.BackColor =
                MilitaryTheme.Surface;

            flow.Padding =
                new Padding(
                    0,
                    2,
                    0,
                    4);

            flow.Margin =
                new Padding(0);

            // -----------------------------------------------------
            // 1. BẢN ĐỒ
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Bản đồ GIS",
                    delegate
                    {
                        ActivatePanMode();

                        SetModernStatus(
                            "Chế độ bản đồ GIS.");
                    },
                    true));

            // -----------------------------------------------------
            // 2. TRA CỨU HỒ SƠ
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Tra cứu hồ sơ",
                    delegate
                    {
                        OpenUnitSearchSmooth(
                            "");
                    },
                    false));

            // -----------------------------------------------------
            // 3. BẢNG THUỘC TÍNH
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Bảng thuộc tính",
                    delegate
                    {
                        OpenAttributeTableSmooth();
                    },
                    false));

            // -----------------------------------------------------
            // 4. TRUY VẤN KHÔNG GIAN
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Truy vấn không gian",
                    delegate
                    {
                        OpenSpatialQueryForm();
                    },
                    false));

            // -----------------------------------------------------
            // 5. BUFFER
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Vùng đệm",
                    delegate
                    {
                        RunSmartBuffer();
                    },
                    false));

            // -----------------------------------------------------
            // 6. KIỂM TRA CSDL
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Kiểm tra CSDL",
                    delegate
                    {
                        OpenDataQualityModule();
                    },
                    false));

            // -----------------------------------------------------
            // 7. THỐNG KÊ BÁO CÁO
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Thống kê báo cáo",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Thống kê báo cáo",
                            "Chức năng thống kê và báo cáo "
                            + "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            // -----------------------------------------------------
            // 8. DASHBOARD
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Bảng điều khiển",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Bảng điều khiển",
                            "Chức năng bảng điều khiển "
                            + "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            // -----------------------------------------------------
            // 9. QUẢN LÝ POSTGIS
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Quản lý CSDL PostGIS",
                    delegate
                    {
                        if (btnPostGIS != null)
                        {
                            btnPostGIS.PerformClick();
                        }
                    },
                    false));

            // -----------------------------------------------------
            // 10. NHẬT KÝ
            // -----------------------------------------------------
            flow.Controls.Add(
                CreateNavButton(
                    "Nhật ký hệ thống",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Nhật ký hệ thống",
                            "Chức năng nhật ký hệ thống "
                            + "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            // Thêm phần nội dung trước, phần tiêu đề sau để DockStyle.Top ổn định.
            _modernNav.Controls.Add(
                flow);

            _modernNav.Controls.Add(
                sidebarHeader);

            Controls.Add(
                _modernNav);

            UpdateModernSidebarBounds();
            _modernNav.BringToFront();

            if (_modernHeader != null)
            {
                _modernHeader.BringToFront();
            }
        }

        private void Form1_ModernUiResize(
            object sender,
            EventArgs e)
        {
            UpdateModernSidebarBounds();
        }

        private void UpdateModernSidebarBounds()
        {
            if (_modernNav == null)
                return;

            // Bắt đầu ngay dưới Header, không chừa khe hở.
            // Khe 8 px trước đây làm lộ TreeView/lớp cũ ở phía sau.
            int top =
                _modernHeader != null
                    ? _modernHeader.Bottom
                    : 80;

            int bottomReserved =
                statusStrip1 != null &&
                statusStrip1.Visible
                    ? statusStrip1.Height
                    : 0;

            int height =
                ClientSize.Height -
                top -
                bottomReserved;

            if (height < 240)
            {
                height = 240;
            }

            _modernNav.Location =
                new Point(
                    0,
                    top);

            _modernNav.Width =
                196;

            _modernNav.Height =
                height;

            // Luôn phủ lên vùng cột trái cũ.
            _modernNav.BringToFront();

            if (_modernHeader != null)
            {
                _modernHeader.BringToFront();
            }
        }

        // =========================================================
        // TẠO NÚT NAV
        // =========================================================
        private Button CreateNavButton(
            string text,
            EventHandler handler,
            bool active)
        {
            Button button =
                new Button();

            // Thanh chức năng rộng 196 px, trừ khoảng đệm hai bên.
            button.AutoSize =
                false;

            button.Width =
                172;

            button.Height =
                34;

            button.Margin =
                new Padding(
                    0,
                    0,
                    0,
                    2);

            button.Padding =
                new Padding(
                    12,
                    0,
                    8,
                    0);

            button.Text =
                text;

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.Cursor =
                Cursors.Hand;

            ApplyNavButtonState(
                button,
                active);

            button.Click +=
                delegate (
                    object sender,
                    EventArgs e)
                {
                    SetActiveNavButton(
                        button);

                    if (handler != null)
                    {
                        handler(
                            sender,
                            e);
                    }
                };

            _modernNavButtons.Add(
                button);

            if (active)
            {
                _activeNavButton =
                    button;
            }

            return button;
        }

        // =========================================================
        // NAV ACTIVE
        // =========================================================
        private void SetActiveNavButton(
            Button activeButton)
        {
            _activeNavButton =
                activeButton;

            for (int i = 0;
                i < _modernNavButtons.Count;
                i++)
            {
                Button button =
                    _modernNavButtons[i];

                ApplyNavButtonState(
                    button,
                    button ==
                    _activeNavButton);
            }
        }

        private void ApplyNavButtonState(
            Button button,
            bool active)
        {
            if (button == null)
                return;

            button.ForeColor =
                active
                    ? Color.White
                    : Color.FromArgb(
                        203,
                        213,
                        225);

            button.BackColor =
                active
                    ? MilitaryTheme.Accent
                    : MilitaryTheme.Surface;

            button.FlatAppearance.MouseOverBackColor =
                active
                    ? MilitaryTheme.AccentHover
                    : MilitaryTheme.Surface2;

            button.FlatAppearance.MouseDownBackColor =
                active
                    ? MilitaryTheme.AccentHover
                    : MilitaryTheme.Surface3;

            button.Font =
                new Font(
                    "Segoe UI",
                    8.6F,
                    active
                        ? FontStyle.Bold
                        : FontStyle.Regular);

            button.TextAlign =
                ContentAlignment.MiddleLeft;
        }

        // =========================================================
        // THANH CÔNG CỤ BẢN ĐỒ NỔI
        //
        // Không di chuyển ToolStrip Designer.
        // Không Items.Clear().
        // =========================================================
        private void ConfigureModernMapToolbar()
        {
            if (panelMap == null)
                return;

            if (_modernMapTools != null)
                return;

            _modernMapTools =
                new FlowLayoutPanel();

            _modernMapTools.Name =
                "panelModernMapTools";

            _modernMapTools.FlowDirection =
                FlowDirection.TopDown;

            _modernMapTools.WrapContents =
                false;

            _modernMapTools.AutoSize =
                false;

            _modernMapTools.Size =
                new Size(
                    34,
                    258);

            _modernMapTools.Location =
                new Point(
                    8,
                    4);

            _modernMapTools.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left;

            _modernMapTools.Padding =
                new Padding(
                    4,
                    4,
                    4,
                    4);

            _modernMapTools.BackColor =
                MilitaryTheme.Surface;

            // Di chuyển bản đồ
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "D",
                    GetToolImage(
                        btnPan),
                    "Di chuyển bản đồ",
                    delegate
                    {
                        if (btnPan != null)
                        {
                            btnPan.PerformClick();
                        }
                    }));

            // Phóng to
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "+",
                    GetToolImage(
                        btnZoomIn),
                    "Phóng to",
                    delegate
                    {
                        if (btnZoomIn != null)
                        {
                            btnZoomIn.PerformClick();
                        }
                    }));

            // Thu nhỏ
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "-",
                    GetToolImage(
                        btnZoomOut),
                    "Thu nhỏ",
                    delegate
                    {
                        if (btnZoomOut != null)
                        {
                            btnZoomOut.PerformClick();
                        }
                    }));

            // Toàn bộ phạm vi
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "T",
                    GetToolImage(
                        btnFullExtent),
                    "Toàn bộ dữ liệu",
                    delegate
                    {
                        if (btnFullExtent != null)
                        {
                            btnFullExtent.PerformClick();
                        }
                    }));

            // Thông tin đối tượng
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "i",
                    GetToolImage(
                        btnInfo),
                    "Thông tin đối tượng",
                    delegate
                    {
                        if (btnInfo != null)
                        {
                            btnInfo.PerformClick();
                        }
                    }));

            // Đo khoảng cách
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "Đ",
                    GetToolImage(
                        btnMeasure),
                    "Đo khoảng cách",
                    delegate
                    {
                        if (btnMeasure != null)
                        {
                            btnMeasure.PerformClick();
                        }
                    }));

            // Buffer
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "V",
                    GetToolImage(
                        btnBuffer),
                    "Tạo vùng đệm",
                    delegate
                    {
                        RunSmartBuffer();
                    }));

            // Bỏ chọn
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "X",
                    GetToolImage(
                        btnClearSelectionToolbar),
                    "Bỏ chọn",
                    delegate
                    {
                        if (btnClearSelectionToolbar != null)
                        {
                            btnClearSelectionToolbar.PerformClick();
                        }
                    }));

            // Làm mới
            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "L",
                    GetToolImage(
                        btnRefreshToolbar),
                    "Làm mới bản đồ",
                    delegate
                    {
                        if (btnRefreshToolbar != null)
                        {
                            btnRefreshToolbar.PerformClick();
                        }
                    }));

            panelMap.Controls.Add(
                _modernMapTools);

            _modernMapTools.BringToFront();

            ConfigureModernZoomInfo();
        }

        // =========================================================
        // THU NHỎ ICON TOOLBAR CHẤT LƯỢNG CAO
        // =========================================================
        private Image ScaleToolImage(
            Image source,
            int width,
            int height)
        {
            if (source == null)
                return null;

            Bitmap result =
                new Bitmap(
                    width,
                    height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g =
                Graphics.FromImage(
                    result))
            {
                g.Clear(
                    Color.Transparent);

                g.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                g.PixelOffsetMode =
                    System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                g.DrawImage(
                    source,
                    new Rectangle(
                        0,
                        0,
                        width,
                        height));
            }

            return result;
        }

        // =========================================================
        // THANH ZOOM DỌC
        //
        // Thay hoàn toàn ô chữ:
        //     Zoom: xx
        //     TL 1:xxxx
        //
        // bằng TrackBar dọc được tạo trong ZoomSlider.cs.
        //
        // ProfessionalUI.cs chịu trách nhiệm:
        // - tắt ZoomBar mặc định của MapWinGIS;
        // - gọi InitVerticalZoomSlider();
        // - đặt slider ngay bên dưới toolbar nổi;
        // - luôn đưa slider lên trước;
        // - đồng bộ slider với axMap2.CurrentZoom.
        // =========================================================
        private void ConfigureModernZoomInfo()
        {
            if (panelMap == null ||
                axMap2 == null)
            {
                return;
            }

            try
            {
                // Không dùng ZoomBar native của MapWinGIS.
                axMap2.ShowZoomBar =
                    false;
            }
            catch
            {
            }

            // -----------------------------------------------------
            // Tạo thanh Zoom dọc từ ZoomSlider.cs.
            // Hàm có guard nên gọi nhiều lần vẫn an toàn.
            // -----------------------------------------------------
            InitVerticalZoomSlider();

            PositionModernZoomSlider();
            SyncVerticalZoomSlider();

            // -----------------------------------------------------
            // Timer nhẹ để slider tự đồng bộ khi:
            // - lăn con lăn chuột;
            // - Zoom In / Zoom Out;
            // - ZoomToLayer;
            // - Full Extent;
            // - code khác thay đổi CurrentZoom.
            // -----------------------------------------------------
            if (_modernZoomTimer == null)
            {
                _modernZoomTimer =
                    new System.Windows.Forms.Timer();

                _modernZoomTimer.Interval =
                    250;

                _modernZoomTimer.Tick +=
                    modernZoomTimer_Tick;

                _modernZoomTimer.Start();
            }
        }


        // =========================================================
        // BỘ ĐẾM ĐỒNG BỘ THU PHÓNG
        // =========================================================
        private void modernZoomTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (IsDisposed ||
                Disposing)
            {
                if (_modernZoomTimer != null)
                {
                    _modernZoomTimer.Stop();
                }

                return;
            }

            SyncVerticalZoomSlider();
            PositionModernZoomSlider();
        }


        // =========================================================
        // ĐẶT THANH ZOOM NGAY DƯỚI TOOLBAR
        // =========================================================
        private void PositionModernZoomSlider()
        {
            if (_zoomSliderPanel == null ||
                _zoomSliderPanel.IsDisposed ||
                panelMap == null)
            {
                return;
            }

            try
            {
                // Đảm bảo slider nằm đúng trong panel bản đồ.
                if (_zoomSliderPanel.Parent != panelMap)
                {
                    if (_zoomSliderPanel.Parent != null)
                    {
                        _zoomSliderPanel.Parent.Controls.Remove(
                            _zoomSliderPanel);
                    }

                    panelMap.Controls.Add(
                        _zoomSliderPanel);
                }

                int left =
                    _modernMapTools != null
                        ? _modernMapTools.Left
                        : 8;

                int top =
                    _modernMapTools != null
                        ? _modernMapTools.Bottom + 3
                        : 270;

                _zoomSliderPanel.Location =
                    new Point(
                        left,
                        top);

                _zoomSliderPanel.BringToFront();

                panelMap.Controls.SetChildIndex(
                    _zoomSliderPanel,
                    0);
            }
            catch
            {
            }
        }


        // =========================================================
        // LẤY ẢNH TỪ TOOLSTRIPBUTTON CŨ
        // =========================================================
        private Image GetToolImage(
            ToolStripButton button)
        {
            if (button == null)
                return null;

            return button.Image;
        }

        // =========================================================
        // NÚT TRÊN THANH CÔNG CỤ
        // =========================================================
        private Button CreateMapToolButton(
            string fallbackText,
            Image image,
            string tooltip,
            EventHandler handler)
        {
            Button button =
                new Button();

            button.Size =
                new Size(
                    24,
                    24);

            button.Margin =
                new Padding(
                    0,
                    1,
                    0,
                    1);

            button.Padding =
                new Padding(0);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.BorderColor =
                MilitaryTheme.Border;

            button.FlatAppearance.MouseOverBackColor =
                MilitaryTheme.Surface3;

            button.BackColor =
                MilitaryTheme.AppBackground;

            button.ForeColor =
                MilitaryTheme.TextPrimary;

            button.Cursor =
                Cursors.Hand;

            button.Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold);

            if (image != null)
            {
                button.Image =
                    ScaleToolImage(
                        image,
                        16,
                        16);

                button.ImageAlign =
                    ContentAlignment.MiddleCenter;

                button.Text =
                    "";
            }
            else
            {
                button.Text =
                    fallbackText;

                button.TextAlign =
                    ContentAlignment.MiddleCenter;
            }

            if (handler != null)
            {
                button.Click +=
                    handler;
            }

            if (_modernToolTip != null)
            {
                _modernToolTip.SetToolTip(
                    button,
                    tooltip);
            }

            return button;
        }

        // =========================================================
        // KHUNG LỚP DỮ LIỆU
        // =========================================================
        private void ConfigureModernLayerPanel()
        {
            if (mainContainer == null)
                return;

            mainContainer.Panel1Collapsed =
                false;

            mainContainer.FixedPanel =
                FixedPanel.Panel1;

            mainContainer.SplitterWidth =
                1;

            mainContainer.BackColor =
                MilitaryTheme.Border;

            mainContainer.Panel1.BackColor =
                MilitaryTheme.Surface;

            mainContainer.Panel2.BackColor =
                Color.FromArgb(
                    8,
                    17,
                    31);

            try
            {
                // Cột trái chỉ dành cho thanh chức năng mới.
                // Giữ đúng 196 px để không còn khoảng trống/lớp cũ
                // lộ ra cạnh thanh chức năng.
                if (mainContainer.Width >
                    700)
                {
                    mainContainer.SplitterDistance =
                        196;
                }
            }
            catch
            {
            }

            // =====================================================
            // TIÊU ĐỀ TRÊN TOC
            // =====================================================
            if (panelTocHeader != null)
            {
                // Thanh chức năng mới đã thay thế phần tiêu đề TOC cũ.
                // Ẩn hoàn toàn để không lộ chữ/lớp phía sau sidebar.
                panelTocHeader.Visible =
                    false;
            }

            if (lblTocTitle != null)
            {
                lblTocTitle.Visible =
                    false;
            }

            // Ẩn thanh thêm/xóa lớp cũ.
            if (panelTocCommands != null)
            {
                panelTocCommands.Visible =
                    false;
            }

            // =====================================================
            // CÂY LỚP DỮ LIỆU
            // =====================================================
            if (treeLayers != null)
            {
                // TreeView vẫn tồn tại để code quản lý lớp sử dụng,
                // nhưng không hiển thị phía sau thanh chức năng.
                treeLayers.Visible =
                    false;

                treeLayers.BorderStyle =
                    BorderStyle.None;

                treeLayers.Font =
                    new Font(
                        "Segoe UI",
                        9F);

                treeLayers.BackColor =
                    MilitaryTheme.Surface;

                treeLayers.ForeColor =
                    MilitaryTheme.TextPrimary;

                treeLayers.LineColor =
                    MilitaryTheme.Surface3;

                treeLayers.HideSelection =
                    false;

                treeLayers.FullRowSelect =
                    true;

                treeLayers.ItemHeight =
                    24;
            }
        }

        // =========================================================
        // THANH LÀM VIỆC BẢN ĐỒ
        // =========================================================
        private void ConfigureModernMapWorkBar()
        {
            if (panelMapWorkspace != null)
            {
                panelMapWorkspace.BackColor =
                    MilitaryTheme.AppBackground;
            }

            if (panelLayerBar != null)
            {
                panelLayerBar.Visible =
                    true;

                panelLayerBar.Height =
                    42;

                panelLayerBar.BackColor =
                    MilitaryTheme.Surface;

                panelLayerBar.Padding =
                    new Padding(
                        10,
                        7,
                        10,
                        7);
            }

            if (lblCurrentLayer != null)
            {
                lblCurrentLayer.Text =
                    "Lớp làm việc:";

                lblCurrentLayer.ForeColor =
                    MilitaryTheme.TextPrimary;

                lblCurrentLayer.Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold);
            }

            if (cboCurrentLayer != null)
            {
                cboCurrentLayer.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                cboCurrentLayer.BackColor =
                    MilitaryTheme.AppBackground;

                cboCurrentLayer.ForeColor =
                    MilitaryTheme.TextPrimary;

                cboCurrentLayer.FlatStyle =
                    FlatStyle.Flat;

                cboCurrentLayer.Font =
                    new Font(
                        "Segoe UI",
                        9F);

                cboCurrentLayer.FormattingEnabled =
                    true;

                cboCurrentLayer.Format -=
                    cboCurrentLayer_Format;

                cboCurrentLayer.Format +=
                    cboCurrentLayer_Format;
            }
        }

        // =========================================================
        // KHU VỰC BẢN ĐỒ
        // =========================================================
        private void ConfigureModernMapArea()
        {
            if (panelMap != null)
            {
                panelMap.Visible =
                    true;

                panelMap.BackColor =
                    Color.FromArgb(
                        8,
                        17,
                        31);

                panelMap.BorderStyle =
                    BorderStyle.None;
            }

            if (axMap2 != null)
            {
                axMap2.Visible =
                    true;

                axMap2.Dock =
                    DockStyle.Fill;

                try
                {
                    axMap2.ShowZoomBar =
                        false;
                }
                catch
                {
                }

                axMap2.BringToFront();
            }

            if (lblMapPlaceholder != null)
            {
                lblMapPlaceholder.Visible =
                    false;
            }
        }

        // =========================================================
        // HIỂN THỊ TÊN LỚP
        // =========================================================
        private void cboCurrentLayer_Format(
            object sender,
            ListControlConvertEventArgs e)
        {
            if (e.ListItem == null)
                return;

            string key =
                e.ListItem.ToString();

            if (string.Equals(
                key,
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    "Ranh giới Thành phố Hà Nội";

                return;
            }

            if (string.Equals(
                key,
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    "Xã, phường Hà Nội";

                return;
            }

            if (string.Equals(
                key,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    "Đơn vị quân đội";

                return;
            }

            if (string.Equals(
                key,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    "Tuyến đường đơn vị";

                return;
            }

            if (string.Equals(
                key,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    "Khu vực đơn vị";

                return;
            }

            e.Value =
                key;
        }

        // =========================================================
        // THANH TRẠNG THÁI
        // =========================================================
        private void ConfigureModernStatusBar()
        {
            if (statusStrip1 == null)
                return;

            statusStrip1.BackColor =
                MilitaryTheme.AppBackground;

            statusStrip1.ForeColor =
                MilitaryTheme.TextSecondary;

            statusStrip1.Font =
                new Font(
                    "Segoe UI",
                    8.5F);

            statusStrip1.SizingGrip =
                false;

            if (lblStatus != null)
            {
                lblStatus.ForeColor =
                    Color.FromArgb(
                        203,
                        213,
                        225);

                lblStatus.Text =
                    "Sẵn sàng | Hà Nội | PostgreSQL/PostGIS";
            }

            if (lblStatusLayer != null)
            {
                lblStatusLayer.ForeColor =
                    MilitaryTheme.TextSecondary;
            }

            if (lblStatusCoordinates != null)
            {
                lblStatusCoordinates.ForeColor =
                    MilitaryTheme.TextSecondary;

                lblStatusCoordinates.Text =
                    "X/Y (m): xem trên bản đồ";
            }

            if (lblStatusProjection != null)
            {
                lblStatusProjection.ForeColor =
                    Color.FromArgb(
                        110,
                        231,
                        183);

                lblStatusProjection.Text =
                    "CRS: EPSG:32648 | mét";
            }
        }

        // =========================================================
        // SAU KHI FORM HIỂN THỊ
        // =========================================================
        private void EnsureModernLayoutAfterShown()
        {
            try
            {
                ConfigureModernLayerPanel();
                ConfigureModernMapArea();

                // Đồng bộ cột trái và sidebar sau khi Form đã có
                // kích thước thực tế, tránh lộ TreeView cũ.
                UpdateModernSidebarBounds();

                if (_modernMapTools != null)
                {
                    _modernMapTools.Location =
                        new Point(
                            8,
                            4);

                    _modernMapTools.BringToFront();

                    if (panelMap != null)
                    {
                        panelMap.Controls.SetChildIndex(
                            _modernMapTools,
                            0);
                    }
                }

                // Đặt lại thanh Zoom dọc sau khi Form đã có
                // kích thước thật và đưa nó lên trên bản đồ.
                PositionModernZoomSlider();
                SyncVerticalZoomSlider();

                if (_modernNav != null)
                {
                    _modernNav.BringToFront();
                }

                if (_modernHeader != null)
                {
                    _modernHeader.BringToFront();
                }

                UpdateModernDatabaseStatus();
            }
            catch
            {
            }
        }

        // =========================================================
        // TRẠNG THÁI CSDL
        // =========================================================
        private void UpdateModernDatabaseStatus()
        {
            if (_modernDbStatus == null)
            {
                return;
            }

            int loadedPostGIS =
                0;

            int totalPostGIS =
                _postgisTables == null
                    ? 5
                    : _postgisTables.Length;

            if (_loadedLayers != null &&
                _postgisTables != null)
            {
                for (int i = 0;
                    i < _postgisTables.Length;
                    i++)
                {
                    string key =
                        _postgisTables[i];

                    int handle;

                    if (!_loadedLayers.TryGetValue(
                        key,
                        out handle))
                    {
                        continue;
                    }

                    try
                    {
                        // Nếu handle còn đọc được tên layer thì xem là hợp lệ.
                        string layerName =
                            axMap2 == null
                                ? ""
                                : axMap2.get_LayerName(
                                    handle);

                        loadedPostGIS++;
                    }
                    catch
                    {
                        // Handle đã mất: không tính.
                    }
                }
            }

            if (loadedPostGIS > 0)
            {
                _modernDbStatus.Text =
                    "● PostGIS | " +
                    loadedPostGIS +
                    "/" +
                    totalPostGIS +
                    " lớp";

                _modernDbStatus.ForeColor =
                    loadedPostGIS >=
                    totalPostGIS
                        ? Color.FromArgb(
                            110,
                            231,
                            183)
                        : Color.FromArgb(
                            251,
                            191,
                            36);

                if (_modernToolTip != null)
                {
                    _modernToolTip.SetToolTip(
                        _modernDbStatus,
                        loadedPostGIS >=
                        totalPostGIS
                            ? "Đã tải đầy đủ dữ liệu PostgreSQL/PostGIS."
                            : "Đã tải một phần dữ liệu PostGIS. Bấm để tải đủ hoặc làm mới.");
                }
            }
            else
            {
                _modernDbStatus.Text =
                    "● PostGIS | Chưa tải";

                _modernDbStatus.ForeColor =
                    MilitaryTheme.TextSecondary;

                if (_modernToolTip != null)
                {
                    _modernToolTip.SetToolTip(
                        _modernDbStatus,
                        "Chưa có lớp PostGIS trên bản đồ. Bấm để tải dữ liệu.");
                }
            }
        }


        // =========================================================
        // CHẾ ĐỘ DI CHUYỂN BẢN ĐỒ
        // =========================================================
        private void ActivatePanMode()
        {
            try
            {
                // Xóa hình đo tạm để tránh mở phần mềm mà vẫn
                // nhìn thấy đường đo cũ.
                if (axMap2 != null)
                {
                    try
                    {
                        axMap2.Measuring.Clear();
                    }
                    catch
                    {
                    }

                    axMap2.CursorMode =
                        MapWinGIS.tkCursorMode.cmPan;

                    axMap2.Redraw();
                    axMap2.Focus();
                }
            }
            catch
            {
            }
        }

        // =========================================================
        // NẠP VÀ XỬ LÝ LOGO HỌC VIỆN
        // =========================================================
        private Image LoadAcademyLogo()
        {
            try
            {
                string[] candidates =
                {
                    // Khuyến nghị: đặt ảnh vào thư mục Icons của project
                    // và để Copy to Output Directory = Copy if newer.
                    Path.Combine(
                        Application.StartupPath,
                        "Icons",
                        "logohocvien.png"),

                    // Hoặc đặt cạnh file .exe.
                    Path.Combine(
                        Application.StartupPath,
                        "logohocvien.png"),

                    // Vị trí file nguồn hiện tại của anh khi chạy Debug:
                    // D:\aNAM5\HK1\HeQuanTriCSDLKG\logohocvien.png
                    Path.GetFullPath(
                        Path.Combine(
                            Application.StartupPath,
                            @"..\..\..\..\logohocvien.png"))
                };

                for (int i = 0;
                    i < candidates.Length;
                    i++)
                {
                    string path =
                        candidates[i];

                    if (string.IsNullOrWhiteSpace(
                        path))
                    {
                        continue;
                    }

                    if (!File.Exists(
                        path))
                    {
                        continue;
                    }

                    using (FileStream stream =
                        new FileStream(
                            path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite))
                    {
                        using (Image source =
                            Image.FromStream(
                                stream))
                        {
                            using (Bitmap original =
                                new Bitmap(
                                    source))
                            {
                                using (Bitmap cropped =
                                    PrepareLogoForDarkHeader(
                                        original))
                                {
                                    return RenderLogoHighQuality(
                                        cropped,
                                        64);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Logo chỉ là giao diện; nếu lỗi thì dùng chữ GIS dự phòng.
            }

            return null;
        }

        // =========================================================
        // XÓA NỀN TRẮNG Ở MÉP + CẮT LỀ THỪA
        //
        // Chỉ xóa vùng trắng nối với mép ngoài của ảnh.
        // Vì vậy các chi tiết trắng nằm bên trong huy hiệu vẫn được giữ.
        // =========================================================
        private Bitmap PrepareLogoForDarkHeader(
            Bitmap source)
        {
            if (source == null)
                return null;

            Bitmap work =
                new Bitmap(
                    source.Width,
                    source.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g =
                Graphics.FromImage(
                    work))
            {
                g.Clear(
                    Color.Transparent);

                g.DrawImage(
                    source,
                    0,
                    0,
                    source.Width,
                    source.Height);
            }

            bool[,] visited =
                new bool[
                    work.Width,
                    work.Height];

            Queue<Point> queue =
                new Queue<Point>();

            // Đưa toàn bộ pixel ở 4 cạnh vào hàng đợi.
            for (int x = 0;
                x < work.Width;
                x++)
            {
                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    x,
                    0);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    x,
                    work.Height - 1);
            }

            for (int y = 0;
                y < work.Height;
                y++)
            {
                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    0,
                    y);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    work.Width - 1,
                    y);
            }

            // Flood fill chỉ vùng trắng nối với mép ngoài.
            while (queue.Count > 0)
            {
                Point p =
                    queue.Dequeue();

                work.SetPixel(
                    p.X,
                    p.Y,
                    Color.Transparent);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    p.X - 1,
                    p.Y);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    p.X + 1,
                    p.Y);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    p.X,
                    p.Y - 1);

                AddWhiteEdgePixel(
                    work,
                    visited,
                    queue,
                    p.X,
                    p.Y + 1);
            }

            // Tìm bounding box của phần logo còn lại.
            int minX =
                work.Width;

            int minY =
                work.Height;

            int maxX =
                -1;

            int maxY =
                -1;

            for (int y = 0;
                y < work.Height;
                y++)
            {
                for (int x = 0;
                    x < work.Width;
                    x++)
                {
                    Color c =
                        work.GetPixel(
                            x,
                            y);

                    if (c.A <= 8)
                        continue;

                    if (x < minX)
                        minX = x;

                    if (y < minY)
                        minY = y;

                    if (x > maxX)
                        maxX = x;

                    if (y > maxY)
                        maxY = y;
                }
            }

            if (maxX < minX ||
                maxY < minY)
            {
                return work;
            }

            // Chừa một lề rất nhỏ 2px quanh huy hiệu.
            int padding =
                2;

            minX =
                Math.Max(
                    0,
                    minX - padding);

            minY =
                Math.Max(
                    0,
                    minY - padding);

            maxX =
                Math.Min(
                    work.Width - 1,
                    maxX + padding);

            maxY =
                Math.Min(
                    work.Height - 1,
                    maxY + padding);

            Rectangle crop =
                new Rectangle(
                    minX,
                    minY,
                    maxX - minX + 1,
                    maxY - minY + 1);

            Bitmap result =
                new Bitmap(
                    crop.Width,
                    crop.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g =
                Graphics.FromImage(
                    result))
            {
                g.Clear(
                    Color.Transparent);

                g.DrawImage(
                    work,
                    new Rectangle(
                        0,
                        0,
                        result.Width,
                        result.Height),
                    crop,
                    GraphicsUnit.Pixel);
            }

            work.Dispose();

            return result;
        }

        // =========================================================
        // KẾT XUẤT LOGO CHẤT LƯỢNG CAO
        //
        // PictureBox không tự phóng/thu nữa. Ảnh được tạo sẵn đúng
        // 64x64 bằng HighQualityBicubic để giảm răng cưa/vỡ nét.
        // =========================================================
        private Bitmap RenderLogoHighQuality(
            Bitmap source,
            int targetSize)
        {
            if (source == null)
                return null;

            if (targetSize < 1)
                targetSize = 64;

            Bitmap result =
                new Bitmap(
                    targetSize,
                    targetSize,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g =
                Graphics.FromImage(
                    result))
            {
                g.Clear(
                    Color.Transparent);

                g.CompositingMode =
                    CompositingMode.SourceOver;

                g.CompositingQuality =
                    CompositingQuality.HighQuality;

                g.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

                g.SmoothingMode =
                    SmoothingMode.AntiAlias;

                g.PixelOffsetMode =
                    PixelOffsetMode.HighQuality;

                // Chừa 2px để logo không chạm sát mép.
                int margin =
                    2;

                int available =
                    targetSize -
                    margin * 2;

                double scale =
                    Math.Min(
                        (double)available / source.Width,
                        (double)available / source.Height);

                int drawWidth =
                    Math.Max(
                        1,
                        (int)Math.Round(
                            source.Width * scale));

                int drawHeight =
                    Math.Max(
                        1,
                        (int)Math.Round(
                            source.Height * scale));

                int x =
                    (targetSize - drawWidth) / 2;

                int y =
                    (targetSize - drawHeight) / 2;

                Rectangle dest =
                    new Rectangle(
                        x,
                        y,
                        drawWidth,
                        drawHeight);

                Rectangle src =
                    new Rectangle(
                        0,
                        0,
                        source.Width,
                        source.Height);

                ImageAttributes attributes =
                    new ImageAttributes();

                attributes.SetWrapMode(
                    WrapMode.TileFlipXY);

                g.DrawImage(
                    source,
                    dest,
                    0,
                    0,
                    source.Width,
                    source.Height,
                    GraphicsUnit.Pixel,
                    attributes);

                attributes.Dispose();
            }

            return result;
        }

        // =========================================================
        // THÊM ĐIỂM ẢNH TRẮNG Ở BIÊN VÀO HÀNG ĐỢI
        // =========================================================
        private void AddWhiteEdgePixel(
            Bitmap bitmap,
            bool[,] visited,
            Queue<Point> queue,
            int x,
            int y)
        {
            if (bitmap == null)
                return;

            if (x < 0 ||
                y < 0 ||
                x >= bitmap.Width ||
                y >= bitmap.Height)
            {
                return;
            }

            if (visited[x, y])
                return;

            visited[x, y] =
                true;

            Color c =
                bitmap.GetPixel(
                    x,
                    y);

            // Ngưỡng tương đối cao để chỉ coi nền trắng/gần trắng
            // là phần cần xóa.
            bool nearWhite =
                c.A > 0 &&
                c.R >= 238 &&
                c.G >= 238 &&
                c.B >= 238;

            if (!nearWhite)
                return;

            queue.Enqueue(
                new Point(
                    x,
                    y));
        }

        // =========================================================
        // STATUS
        // =========================================================
        private void SetModernStatus(
            string text)
        {
            if (lblStatus != null)
            {
                lblStatus.Text =
                    text;
            }
        }

        // =========================================================
        // MODULE CHƯA TRIỂN KHAI
        // =========================================================
        private void ShowModernModuleNotice(
            string title,
            string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}