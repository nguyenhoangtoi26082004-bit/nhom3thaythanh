
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
        private bool _professionalUiApplied = false;
        private Panel _modernHeader;
        private Panel _modernNav;
        private FlowLayoutPanel _modernMapTools;
        private System.Windows.Forms.Timer _modernZoomTimer;
        private TextBox _modernQuickSearch;
        private System.Windows.Forms.Label _modernDbStatus;
        private ToolTip _modernToolTip;

        private readonly List<Button> _modernNavButtons =
            new List<Button>();

        private Button _activeNavButton;

        private static readonly Font _navFontThuongV2 =
            new Font("Segoe UI", 8.6F, FontStyle.Regular);

        private static readonly Font _navFontDamV2 =
            new Font("Segoe UI", 8.6F, FontStyle.Bold);

        private void ApplyProfessionalUI()
        {
            if (_professionalUiApplied) return;

            SuspendLayout();

            try
            {
                ConfigureModernWindow();
                MilitaryTheme.ApplyToForm(this);
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

                _professionalUiApplied = true;
                MilitaryTheme.StartAutoTheme();

                try
                {
                    BeginInvoke((MethodInvoker)delegate
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
                ResumeLayout(true);
            }
        }

        private void ConfigureModernWindow()
        {
            Text = "Hệ thống CSDL GIS - Quản lý đơn vị trên địa bàn Hà Nội";
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            MinimumSize = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = MilitaryTheme.AppBackground;
        }

        private void HideLegacyTopBars()
        {
            if (menuStrip1 != null)
                menuStrip1.Visible = false;

            if (toolStripTools != null)
                toolStripTools.Visible = false;

            if (toolStripStandard != null)
                toolStripStandard.Visible = false;
        }

        private void BuildModernHeader()
        {
            if (_modernHeader != null) return;

            _modernToolTip = new ToolTip();
            _modernToolTip.ShowAlways = true;
            _modernToolTip.InitialDelay = 300;

            _modernHeader = new Panel();
            _modernHeader.Name = "panelModernHeader";
            _modernHeader.Dock = DockStyle.Top;
            _modernHeader.Height = 80;
            _modernHeader.BackColor = MilitaryTheme.AppBackground;
            _modernHeader.Padding = new Padding(16, 8, 16, 7);
            _modernHeader.Margin = new Padding(0);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Margin = new Padding(0);
            layout.Padding = new Padding(0);
            layout.ColumnCount = 3;
            layout.RowCount = 1;
            layout.BackColor = MilitaryTheme.AppBackground;

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 40F));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35F));
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25F));

            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Fill;
            titlePanel.BackColor = MilitaryTheme.AppBackground;
            titlePanel.Margin = new Padding(0);

            PictureBox logoPicture = new PictureBox();
            logoPicture.Name = "picAcademyLogo";
            logoPicture.Size = new Size(64, 64);
            logoPicture.Location = new Point(0, 0);
            logoPicture.SizeMode = PictureBoxSizeMode.CenterImage;
            logoPicture.BackColor = Color.Transparent;
            logoPicture.TabStop = false;

            Image academyLogo = LoadAcademyLogo();

            if (academyLogo != null)
            {
                logoPicture.Image = academyLogo;
            }
            else
            {
                Bitmap fallback = new Bitmap(64, 64);

                using (Graphics g = Graphics.FromImage(fallback))
                {
                    g.Clear(Color.Transparent);

                    using (Font font =
                        new Font("Segoe UI", 11F, FontStyle.Bold))
                    using (Brush brush =
                        new SolidBrush(Color.FromArgb(52, 211, 153)))
                    {
                        StringFormat format = new StringFormat();
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;

                        g.DrawString(
                            "GIS",
                            font,
                            brush,
                            new RectangleF(0, 0, 64, 64),
                            format);

                        format.Dispose();
                    }
                }

                logoPicture.Image = fallback;
            }

            System.Windows.Forms.Label title =
                new System.Windows.Forms.Label();

            title.AutoSize = true;
            title.Location = new Point(78, 10);
            title.Text = "HỆ THỐNG CSDL GIS QUẢN LÝ ĐƠN VỊ HÀ NỘI";
            title.ForeColor = MilitaryTheme.TextPrimary;
            title.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            System.Windows.Forms.Label subtitle =
                new System.Windows.Forms.Label();

            subtitle.AutoSize = true;
            subtitle.Location = new Point(78, 39);
            subtitle.Text =
                "Tra cứu thông tin, quản lý dữ liệu không gian và phân tích GIS trên địa bàn Hà Nội";
            subtitle.ForeColor = MilitaryTheme.TextSecondary;
            subtitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);

            titlePanel.Controls.Add(logoPicture);
            titlePanel.Controls.Add(title);
            titlePanel.Controls.Add(subtitle);

            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.BackColor = MilitaryTheme.AppBackground;
            searchPanel.Margin = new Padding(10, 0, 10, 0);

            System.Windows.Forms.Label searchTitle =
                new System.Windows.Forms.Label();

            searchTitle.AutoSize = true;
            searchTitle.Location = new Point(4, 3);
            searchTitle.Text = "TRA CỨU NHANH";
            searchTitle.ForeColor = MilitaryTheme.TextSecondary;
            searchTitle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);

            _modernQuickSearch = new TextBox();
            _modernQuickSearch.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;
            _modernQuickSearch.Location = new Point(4, 26);
            _modernQuickSearch.Height = 29;
            _modernQuickSearch.BackColor = MilitaryTheme.Surface;
            _modernQuickSearch.ForeColor = MilitaryTheme.TextPrimary;
            _modernQuickSearch.BorderStyle = BorderStyle.FixedSingle;
            _modernQuickSearch.Font = new Font("Segoe UI", 9F);
            _modernQuickSearch.KeyDown += modernQuickSearch_KeyDown;

            Button btnQuickSearch = new Button();
            btnQuickSearch.Text = "Tìm";
            btnQuickSearch.Width = 60;
            btnQuickSearch.Height = 30;
            btnQuickSearch.Anchor =
                AnchorStyles.Top | AnchorStyles.Right;

            btnQuickSearch.Click += delegate
            {
                OpenModernQuickSearch();
            };

            MilitaryTheme.StyleButton(btnQuickSearch);

            searchPanel.Resize += delegate
            {
                int width = searchPanel.ClientSize.Width;
                int buttonWidth = 60;
                int gap = 8;

                _modernQuickSearch.Width = Math.Max(
                    150,
                    width - buttonWidth - gap - 12);

                btnQuickSearch.Left = Math.Max(
                    160,
                    width - buttonWidth - 3);

                btnQuickSearch.Top = 25;
            };

            searchPanel.Controls.Add(searchTitle);
            searchPanel.Controls.Add(_modernQuickSearch);
            searchPanel.Controls.Add(btnQuickSearch);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.WrapContents = false;
            actions.AutoScroll = true;
            actions.Padding = new Padding(2, 15, 0, 0);
            actions.Margin = new Padding(0);
            actions.BackColor = MilitaryTheme.AppBackground;

            Button btnAddUnit = new Button();
            btnAddUnit.AutoSize = true;
            btnAddUnit.Height = 31;
            btnAddUnit.Padding = new Padding(7, 0, 7, 0);
            btnAddUnit.Text = "+ Thêm đơn vị";

            btnAddUnit.Click += delegate
            {
                StartCreateSpatialObject("don_vi_quan_doi");
            };

            MilitaryTheme.StyleButton(btnAddUnit);

            _modernDbStatus = new System.Windows.Forms.Label();
            _modernDbStatus.AutoSize = true;
            _modernDbStatus.Padding = new Padding(9, 7, 9, 7);
            _modernDbStatus.Margin = new Padding(0, 1, 8, 0);
            _modernDbStatus.Text = "● PostGIS | Chưa tải";
            _modernDbStatus.ForeColor = MilitaryTheme.TextSecondary;
            _modernDbStatus.BackColor = MilitaryTheme.Surface2;
            _modernDbStatus.Cursor = Cursors.Hand;

            _modernDbStatus.Click += delegate
            {
                if (btnPostGIS != null)
                    btnPostGIS.PerformClick();
            };

            if (_modernToolTip != null)
            {
                _modernToolTip.SetToolTip(
                    _modernDbStatus,
                    "Kết nối hoặc tải lại dữ liệu PostgreSQL/PostGIS.");
            }

            actions.Controls.Add(btnAddUnit);
            actions.Controls.Add(_modernDbStatus);

            layout.Controls.Add(titlePanel, 0, 0);
            layout.Controls.Add(searchPanel, 1, 0);
            layout.Controls.Add(actions, 2, 0);

            _modernHeader.Controls.Add(layout);
            Controls.Add(_modernHeader);
            _modernHeader.BringToFront();
        }

        private void modernQuickSearch_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OpenModernQuickSearch();
            }
        }

        private void OpenModernQuickSearch()
        {
            string keyword = _modernQuickSearch == null
                ? ""
                : _modernQuickSearch.Text.Trim();

            OpenUnitSearchSmooth(keyword);

            if (lblStatus == null)
                return;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                lblStatus.Text = "Đã mở chức năng tra cứu hồ sơ.";
            }
            else
            {
                lblStatus.Text =
                    "Tra cứu hồ sơ | Từ khóa: " + keyword;
            }
        }

        private void BuildModernNavigation()
        {
            if (_modernNav != null) return;

            _modernNav = new Panel();
            _modernNav.Name = "panelModernSidebar";
            _modernNav.Dock = DockStyle.None;
            _modernNav.Width = 196;
            _modernNav.MinimumSize = new Size(196, 220);
            _modernNav.BackColor = MilitaryTheme.Surface;
            _modernNav.Padding = new Padding(8, 8, 8, 8);
            _modernNav.Margin = new Padding(0);
            _modernNav.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left;

            Panel sidebarHeader = new Panel();
            sidebarHeader.Dock = DockStyle.Top;
            sidebarHeader.Height = 54;
            sidebarHeader.BackColor = MilitaryTheme.Surface;
            sidebarHeader.Padding = new Padding(10, 6, 6, 4);

            System.Windows.Forms.Label sidebarTitle =
                new System.Windows.Forms.Label();

            sidebarTitle.Dock = DockStyle.Top;
            sidebarTitle.Height = 22;
            sidebarTitle.Text = "CHỨC NĂNG";
            sidebarTitle.TextAlign = ContentAlignment.MiddleLeft;
            sidebarTitle.ForeColor = MilitaryTheme.TextPrimary;
            sidebarTitle.Font =
                new Font("Segoe UI", 9.2F, FontStyle.Bold);

            System.Windows.Forms.Label sidebarSubTitle =
                new System.Windows.Forms.Label();

            sidebarSubTitle.Dock = DockStyle.Fill;
            sidebarSubTitle.Text = "Hệ thống GIS Hà Nội";
            sidebarSubTitle.TextAlign = ContentAlignment.MiddleLeft;
            sidebarSubTitle.ForeColor = MilitaryTheme.TextSecondary;
            sidebarSubTitle.Font =
                new Font("Segoe UI", 8F, FontStyle.Regular);

            sidebarHeader.Controls.Add(sidebarSubTitle);
            sidebarHeader.Controls.Add(sidebarTitle);

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.AutoScroll = true;
            flow.BackColor = MilitaryTheme.Surface;
            flow.Padding = new Padding(0, 2, 0, 4);
            flow.Margin = new Padding(0);

            flow.Controls.Add(
                CreateNavButton(
                    "Bản đồ GIS",
                    delegate
                    {
                        ActivatePanMode();
                        SetModernStatus("Chế độ bản đồ GIS.");
                    },
                    true));

            flow.Controls.Add(
                CreateNavButton(
                    "Tra cứu hồ sơ",
                    delegate
                    {
                        OpenUnitSearchSmooth("");
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Bảng thuộc tính",
                    delegate
                    {
                        OpenAttributeTableSmooth();
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Truy vấn không gian",
                    delegate
                    {
                        OpenSpatialQueryForm();
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Vùng đệm",
                    delegate
                    {
                        RunSmartBuffer();
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Kiểm tra CSDL",
                    delegate
                    {
                        OpenDataQualityModule();
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Thống kê báo cáo",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Thống kê báo cáo",
                            "Chức năng thống kê và báo cáo " +
                            "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Bảng điều khiển",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Bảng điều khiển",
                            "Chức năng bảng điều khiển " +
                            "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Quản lý CSDL PostGIS",
                    delegate
                    {
                        if (btnPostGIS != null)
                            btnPostGIS.PerformClick();
                    },
                    false));

            flow.Controls.Add(
                CreateNavButton(
                    "Nhật ký hệ thống",
                    delegate
                    {
                        ShowModernModuleNotice(
                            "Nhật ký hệ thống",
                            "Chức năng nhật ký hệ thống " +
                            "sẽ được phát triển ở bước tiếp theo.");
                    },
                    false));

            _modernNav.Controls.Add(flow);
            _modernNav.Controls.Add(sidebarHeader);

            Controls.Add(_modernNav);

            UpdateModernSidebarBounds();

            _modernNav.BringToFront();

            if (_modernHeader != null)
                _modernHeader.BringToFront();
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

            int top = _modernHeader != null
                ? _modernHeader.Bottom
                : 80;

            int bottomReserved =
                statusStrip1 != null && statusStrip1.Visible
                    ? statusStrip1.Height
                    : 0;

            int height =
                ClientSize.Height - top - bottomReserved;

            if (height < 240)
                height = 240;

            Rectangle boundsMoi =
                new Rectangle(0, top, 196, height);

            if (_modernNav.Bounds != boundsMoi)
                _modernNav.Bounds = boundsMoi;

            _modernNav.BringToFront();

            if (_modernHeader != null)
                _modernHeader.BringToFront();
        }

        private Button CreateNavButton(
            string text,
            EventHandler handler,
            bool active)
        {
            Button button = new Button();

            button.AutoSize = false;
            button.Width = 172;
            button.Height = 34;
            button.Margin = new Padding(0, 0, 0, 2);
            button.Padding = new Padding(12, 0, 8, 0);
            button.Text = text;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;

            ApplyNavButtonState(button, active);

            button.Click += delegate (object sender, EventArgs e)
            {
                SetActiveNavButton(button);

                if (handler != null)
                    handler(sender, e);
            };

            _modernNavButtons.Add(button);

            if (active)
                _activeNavButton = button;

            return button;
        }

        private void SetActiveNavButton(Button activeButton)
        {
            if (activeButton == null || activeButton.IsDisposed)
                return;

            if (object.ReferenceEquals(
                _activeNavButton,
                activeButton))
            {
                return;
            }

            Button nutCu = _activeNavButton;
            _activeNavButton = activeButton;

            if (nutCu != null && !nutCu.IsDisposed)
                ApplyNavButtonState(nutCu, false);

            ApplyNavButtonState(activeButton, true);
        }

        private void ApplyNavButtonState(
            Button button,
            bool active)
        {
            if (button == null || button.IsDisposed)
                return;

            Color mauChu = active
                ? Color.White
                : Color.FromArgb(203, 213, 225);

            Color mauNen = active
                ? MilitaryTheme.Accent
                : MilitaryTheme.Surface;

            Color mauHover = active
                ? MilitaryTheme.AccentHover
                : MilitaryTheme.Surface2;

            Color mauNhan = active
                ? MilitaryTheme.AccentHover
                : MilitaryTheme.Surface3;

            if (button.ForeColor != mauChu)
                button.ForeColor = mauChu;

            if (button.BackColor != mauNen)
                button.BackColor = mauNen;

            if (button.FlatAppearance.MouseOverBackColor != mauHover)
                button.FlatAppearance.MouseOverBackColor = mauHover;

            if (button.FlatAppearance.MouseDownBackColor != mauNhan)
                button.FlatAppearance.MouseDownBackColor = mauNhan;

            Font fontCanDung = active
                ? _navFontDamV2
                : _navFontThuongV2;

            if (!object.ReferenceEquals(button.Font, fontCanDung))
                button.Font = fontCanDung;

            if (button.TextAlign != ContentAlignment.MiddleLeft)
                button.TextAlign = ContentAlignment.MiddleLeft;
        }

        private void ConfigureModernMapToolbar()
        {
            if (panelMap == null || _modernMapTools != null)
                return;

            _modernMapTools = new FlowLayoutPanel();
            _modernMapTools.Name = "panelModernMapTools";
            _modernMapTools.FlowDirection = FlowDirection.TopDown;
            _modernMapTools.WrapContents = false;
            _modernMapTools.AutoSize = false;
            _modernMapTools.Size = new Size(34, 258);
            _modernMapTools.Location = new Point(8, 4);
            _modernMapTools.Anchor =
                AnchorStyles.Top | AnchorStyles.Left;
            _modernMapTools.Padding = new Padding(4, 4, 4, 4);
            _modernMapTools.BackColor = MilitaryTheme.Surface;

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "D",
                    GetToolImage(btnPan),
                    "Di chuyển bản đồ",
                    delegate
                    {
                        if (btnPan != null)
                            btnPan.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "+",
                    GetToolImage(btnZoomIn),
                    "Phóng to",
                    delegate
                    {
                        if (btnZoomIn != null)
                            btnZoomIn.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "-",
                    GetToolImage(btnZoomOut),
                    "Thu nhỏ",
                    delegate
                    {
                        if (btnZoomOut != null)
                            btnZoomOut.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "T",
                    GetToolImage(btnFullExtent),
                    "Toàn bộ dữ liệu",
                    delegate
                    {
                        if (btnFullExtent != null)
                            btnFullExtent.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "i",
                    GetToolImage(btnInfo),
                    "Thông tin đối tượng",
                    delegate
                    {
                        if (btnInfo != null)
                            btnInfo.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "Đ",
                    GetToolImage(btnMeasure),
                    "Đo khoảng cách",
                    delegate
                    {
                        if (btnMeasure != null)
                            btnMeasure.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "V",
                    GetToolImage(btnBuffer),
                    "Tạo vùng đệm",
                    delegate
                    {
                        RunSmartBuffer();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "X",
                    GetToolImage(btnClearSelectionToolbar),
                    "Bỏ chọn",
                    delegate
                    {
                        if (btnClearSelectionToolbar != null)
                            btnClearSelectionToolbar.PerformClick();
                    }));

            _modernMapTools.Controls.Add(
                CreateMapToolButton(
                    "L",
                    GetToolImage(btnRefreshToolbar),
                    "Làm mới bản đồ",
                    delegate
                    {
                        if (btnRefreshToolbar != null)
                            btnRefreshToolbar.PerformClick();
                    }));

            panelMap.Controls.Add(_modernMapTools);
            _modernMapTools.BringToFront();

            ConfigureModernZoomInfo();
        }

        private Image ScaleToolImage(
            Image source,
            int width,
            int height)
        {
            if (source == null)
                return null;

            Bitmap result = new Bitmap(
                width,
                height,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(
                    source,
                    new Rectangle(0, 0, width, height));
            }

            return result;
        }

        private void ConfigureModernZoomInfo()
        {
            if (panelMap == null || axMap2 == null)
                return;

            try
            {
                axMap2.ShowZoomBar = false;
            }
            catch
            {
            }

            InitVerticalZoomSlider();
            PositionModernZoomSlider();
            SyncVerticalZoomSlider();

            if (_modernZoomTimer == null)
            {
                _modernZoomTimer = new System.Windows.Forms.Timer();
                _modernZoomTimer.Interval = 250;
                _modernZoomTimer.Tick += modernZoomTimer_Tick;
                _modernZoomTimer.Start();
            }
        }

        private void modernZoomTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (IsDisposed || Disposing)
            {
                if (_modernZoomTimer != null)
                    _modernZoomTimer.Stop();

                return;
            }

            SyncVerticalZoomSlider();
            PositionModernZoomSlider();
        }

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
                if (_zoomSliderPanel.Parent != panelMap)
                {
                    if (_zoomSliderPanel.Parent != null)
                    {
                        _zoomSliderPanel.Parent.Controls.Remove(
                            _zoomSliderPanel);
                    }

                    panelMap.Controls.Add(_zoomSliderPanel);
                }

                int left = _modernMapTools != null
                    ? _modernMapTools.Left
                    : 8;

                int top = _modernMapTools != null
                    ? _modernMapTools.Bottom + 3
                    : 270;

                _zoomSliderPanel.Location = new Point(left, top);
                _zoomSliderPanel.BringToFront();

                panelMap.Controls.SetChildIndex(
                    _zoomSliderPanel,
                    0);
            }
            catch
            {
            }
        }

        private Image GetToolImage(ToolStripButton button)
        {
            if (button == null)
                return null;

            return button.Image;
        }

        private Button CreateMapToolButton(
            string fallbackText,
            Image image,
            string tooltip,
            EventHandler handler)
        {
            Button button = new Button();

            button.Size = new Size(24, 24);
            button.Margin = new Padding(0, 1, 0, 1);
            button.Padding = new Padding(0);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = MilitaryTheme.Border;
            button.FlatAppearance.MouseOverBackColor = MilitaryTheme.Surface3;
            button.BackColor = MilitaryTheme.AppBackground;
            button.ForeColor = MilitaryTheme.TextPrimary;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            if (image != null)
            {
                button.Image = ScaleToolImage(image, 16, 16);
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.Text = "";
            }
            else
            {
                button.Text = fallbackText;
                button.TextAlign = ContentAlignment.MiddleCenter;
            }

            if (handler != null)
                button.Click += handler;

            if (_modernToolTip != null)
            {
                _modernToolTip.SetToolTip(
                    button,
                    tooltip);
            }

            return button;
        }

        private void ConfigureModernLayerPanel()
        {
            if (mainContainer == null)
                return;

            mainContainer.Panel1Collapsed = false;
            mainContainer.FixedPanel = FixedPanel.Panel1;
            mainContainer.SplitterWidth = 1;
            mainContainer.BackColor = MilitaryTheme.Border;
            mainContainer.Panel1.BackColor = MilitaryTheme.Surface;
            mainContainer.Panel2.BackColor = Color.FromArgb(8, 17, 31);

            try
            {
                if (mainContainer.Width > 700)
                    mainContainer.SplitterDistance = 196;
            }
            catch
            {
            }

            if (panelTocHeader != null)
                panelTocHeader.Visible = false;

            if (lblTocTitle != null)
                lblTocTitle.Visible = false;

            if (panelTocCommands != null)
                panelTocCommands.Visible = false;

            if (treeLayers != null)
            {
                treeLayers.Visible = false;
                treeLayers.BorderStyle = BorderStyle.None;
                treeLayers.Font = new Font("Segoe UI", 9F);
                treeLayers.BackColor = MilitaryTheme.Surface;
                treeLayers.ForeColor = MilitaryTheme.TextPrimary;
                treeLayers.LineColor = MilitaryTheme.Surface3;
                treeLayers.HideSelection = false;
                treeLayers.FullRowSelect = true;
                treeLayers.ItemHeight = 24;
            }
        }

        private void ConfigureModernMapWorkBar()
        {
            if (panelMapWorkspace != null)
                panelMapWorkspace.BackColor = MilitaryTheme.AppBackground;

            if (panelLayerBar != null)
            {
                panelLayerBar.Visible = true;
                panelLayerBar.Height = 42;
                panelLayerBar.BackColor = MilitaryTheme.Surface;
                panelLayerBar.Padding = new Padding(10, 7, 10, 7);
            }

            if (lblCurrentLayer != null)
            {
                lblCurrentLayer.Text = "Lớp làm việc:";
                lblCurrentLayer.ForeColor = MilitaryTheme.TextPrimary;
                lblCurrentLayer.Font =
                    new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            if (cboCurrentLayer != null)
            {
                cboCurrentLayer.DropDownStyle =
                    ComboBoxStyle.DropDownList;
                cboCurrentLayer.BackColor = MilitaryTheme.AppBackground;
                cboCurrentLayer.ForeColor = MilitaryTheme.TextPrimary;
                cboCurrentLayer.FlatStyle = FlatStyle.Flat;
                cboCurrentLayer.Font = new Font("Segoe UI", 9F);
                cboCurrentLayer.FormattingEnabled = true;

                cboCurrentLayer.Format -= cboCurrentLayer_Format;
                cboCurrentLayer.Format += cboCurrentLayer_Format;
            }
        }

        private void ConfigureModernMapArea()
        {
            if (panelMap != null)
            {
                panelMap.Visible = true;
                panelMap.BackColor = Color.FromArgb(8, 17, 31);
                panelMap.BorderStyle = BorderStyle.None;
            }

            if (axMap2 != null)
            {
                axMap2.Visible = true;
                axMap2.Dock = DockStyle.Fill;

                try
                {
                    axMap2.ShowZoomBar = false;
                }
                catch
                {
                }

                axMap2.BringToFront();
            }

            if (lblMapPlaceholder != null)
                lblMapPlaceholder.Visible = false;
        }

        private void cboCurrentLayer_Format(
            object sender,
            ListControlConvertEventArgs e)
        {
            if (e.ListItem == null)
                return;

            string key = e.ListItem.ToString();

            if (string.Equals(
                key, "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "Ranh giới Thành phố Hà Nội";
                return;
            }

            if (string.Equals(
                key, "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "Xã, phường Hà Nội";
                return;
            }

            if (string.Equals(
                key, "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "Đơn vị quân đội";
                return;
            }

            if (string.Equals(
                key, "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "Tuyến đường đơn vị";
                return;
            }

            if (string.Equals(
                key, "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value = "Khu vực đơn vị";
                return;
            }

            e.Value = key;
        }

        private void ConfigureModernStatusBar()
        {
            if (statusStrip1 == null)
                return;

            statusStrip1.BackColor = MilitaryTheme.AppBackground;
            statusStrip1.ForeColor = MilitaryTheme.TextSecondary;
            statusStrip1.Font = new Font("Segoe UI", 8.5F);
            statusStrip1.SizingGrip = false;

            if (lblStatus != null)
            {
                lblStatus.ForeColor = Color.FromArgb(203, 213, 225);
                lblStatus.Text =
                    "Sẵn sàng | Hà Nội | PostgreSQL/PostGIS";
            }

            if (lblStatusLayer != null)
                lblStatusLayer.ForeColor = MilitaryTheme.TextSecondary;

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
                    Color.FromArgb(110, 231, 183);
                lblStatusProjection.Text =
                    "CRS: EPSG:32648 | mét";
            }
        }

        private void EnsureModernLayoutAfterShown()
        {
            try
            {
                ConfigureModernLayerPanel();
                ConfigureModernMapArea();
                UpdateModernSidebarBounds();

                if (_modernMapTools != null)
                {
                    _modernMapTools.Location = new Point(8, 4);
                    _modernMapTools.BringToFront();

                    if (panelMap != null)
                    {
                        panelMap.Controls.SetChildIndex(
                            _modernMapTools, 0);
                    }
                }

                PositionModernZoomSlider();
                SyncVerticalZoomSlider();

                if (_modernNav != null)
                    _modernNav.BringToFront();

                if (_modernHeader != null)
                    _modernHeader.BringToFront();

                UpdateModernDatabaseStatus();
            }
            catch
            {
            }
        }

        private void UpdateModernDatabaseStatus()
        {
            if (_modernDbStatus == null)
                return;

            int loadedPostGIS = 0;

            int totalPostGIS = _postgisTables == null
                ? 5
                : _postgisTables.Length;

            if (_loadedLayers != null && _postgisTables != null)
            {
                for (int i = 0; i < _postgisTables.Length; i++)
                {
                    string key = _postgisTables[i];
                    int handle;

                    if (!_loadedLayers.TryGetValue(key, out handle))
                        continue;

                    try
                    {
                        string layerName = axMap2 == null
                            ? ""
                            : axMap2.get_LayerName(handle);

                        loadedPostGIS++;
                    }
                    catch
                    {
                    }
                }
            }

            if (loadedPostGIS > 0)
            {
                _modernDbStatus.Text =
                    "● PostGIS | " +
                    loadedPostGIS + "/" +
                    totalPostGIS + " lớp";

                _modernDbStatus.ForeColor =
                    loadedPostGIS >= totalPostGIS
                        ? Color.FromArgb(110, 231, 183)
                        : Color.FromArgb(251, 191, 36);

                if (_modernToolTip != null)
                {
                    _modernToolTip.SetToolTip(
                        _modernDbStatus,
                        loadedPostGIS >= totalPostGIS
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

        private void ActivatePanMode()
        {
            try
            {
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

        private Image LoadAcademyLogo()
        {
            try
            {
                string[] candidates =
                {
                    Path.Combine(
                        Application.StartupPath,
                        "Icons", "logohocvien.png"),

                    Path.Combine(
                        Application.StartupPath,
                        "logohocvien.png"),

                    Path.GetFullPath(
                        Path.Combine(
                            Application.StartupPath,
                            @"..\..\..\..\logohocvien.png"))
                };

                for (int i = 0; i < candidates.Length; i++)
                {
                    string path = candidates[i];

                    if (string.IsNullOrWhiteSpace(path))
                        continue;

                    if (!File.Exists(path))
                        continue;

                    using (FileStream stream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    using (Image source = Image.FromStream(stream))
                    using (Bitmap original = new Bitmap(source))
                    using (Bitmap cropped = PrepareLogoForDarkHeader(original))
                    {
                        return RenderLogoHighQuality(cropped, 64);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private Bitmap PrepareLogoForDarkHeader(Bitmap source)
        {
            if (source == null)
                return null;

            Bitmap work = new Bitmap(
                source.Width,
                source.Height,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(work))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(source, 0, 0,
                    source.Width, source.Height);
            }

            bool[,] visited = new bool[work.Width, work.Height];
            Queue<Point> queue = new Queue<Point>();

            for (int x = 0; x < work.Width; x++)
            {
                AddWhiteEdgePixel(
                    work, visited, queue, x, 0);

                AddWhiteEdgePixel(
                    work, visited, queue, x, work.Height - 1);
            }

            for (int y = 0; y < work.Height; y++)
            {
                AddWhiteEdgePixel(
                    work, visited, queue, 0, y);

                AddWhiteEdgePixel(
                    work, visited, queue, work.Width - 1, y);
            }

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();

                work.SetPixel(
                    p.X, p.Y, Color.Transparent);

                AddWhiteEdgePixel(
                    work, visited, queue, p.X - 1, p.Y);

                AddWhiteEdgePixel(
                    work, visited, queue, p.X + 1, p.Y);

                AddWhiteEdgePixel(
                    work, visited, queue, p.X, p.Y - 1);

                AddWhiteEdgePixel(
                    work, visited, queue, p.X, p.Y + 1);
            }

            int minX = work.Width;
            int minY = work.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < work.Height; y++)
            {
                for (int x = 0; x < work.Width; x++)
                {
                    Color c = work.GetPixel(x, y);

                    if (c.A <= 8)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return work;

            int padding = 2;

            minX = Math.Max(0, minX - padding);
            minY = Math.Max(0, minY - padding);

            maxX = Math.Min(
                work.Width - 1, maxX + padding);

            maxY = Math.Min(
                work.Height - 1, maxY + padding);

            Rectangle crop = new Rectangle(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1);

            Bitmap result = new Bitmap(
                crop.Width,
                crop.Height,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);

                g.DrawImage(
                    work,
                    new Rectangle(
                        0, 0,
                        result.Width,
                        result.Height),
                    crop,
                    GraphicsUnit.Pixel);
            }

            work.Dispose();

            return result;
        }

        private Bitmap RenderLogoHighQuality(
            Bitmap source,
            int targetSize)
        {
            if (source == null)
                return null;

            if (targetSize < 1)
                targetSize = 64;

            Bitmap result = new Bitmap(
                targetSize,
                targetSize,
                PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                int margin = 2;
                int available = targetSize - margin * 2;

                double scale = Math.Min(
                    (double)available / source.Width,
                    (double)available / source.Height);

                int drawWidth = Math.Max(
                    1,
                    (int)Math.Round(source.Width * scale));

                int drawHeight = Math.Max(
                    1,
                    (int)Math.Round(source.Height * scale));

                int x = (targetSize - drawWidth) / 2;
                int y = (targetSize - drawHeight) / 2;

                Rectangle dest = new Rectangle(
                    x, y, drawWidth, drawHeight);

                Rectangle src = new Rectangle(
                    0, 0,
                    source.Width, source.Height);

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetWrapMode(WrapMode.TileFlipXY);

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

            visited[x, y] = true;

            Color c = bitmap.GetPixel(x, y);

            bool nearWhite =
                c.A > 0 &&
                c.R >= 238 &&
                c.G >= 238 &&
                c.B >= 238;

            if (!nearWhite)
                return;

            queue.Enqueue(new Point(x, y));
        }

        private void SetModernStatus(string text)
        {
            if (lblStatus != null)
                lblStatus.Text = text;
        }

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
