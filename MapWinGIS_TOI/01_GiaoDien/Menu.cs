
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        private bool _mainMenuInitialized = false;

        // =====================================================
        // 1. KHỞI TẠO MENU CHÍNH
        // =====================================================
        private void InitMainMenu()
        {
            if (_mainMenuInitialized)
                return;

            if (menuFile == null ||
                menuLayer == null ||
                menuEdit == null ||
                menuQuery == null ||
                menuAnalysis == null ||
                menuView == null ||
                menuHelp == null)
            {
                return;
            }

            menuFile.Text = "Tệp";
            menuLayer.Text = "Dữ liệu";
            menuEdit.Text = "Quản lý";
            menuQuery.Text = "Tra cứu";
            menuAnalysis.Text = "Phân tích";
            menuView.Text = "Hiển thị";
            menuHelp.Text = "Trợ giúp";

            // ==========================================
            // TỆP
            // ==========================================
            menuFile.DropDownItems.Clear();

            AddMenuItem(
                menuFile,
                "Mở dữ liệu Shapefile...",
                delegate
                {
                    btnOpenLayer_Click(
                        this, EventArgs.Empty);
                },
                Keys.Control | Keys.O);

            menuFile.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuFile,
                "Thoát",
                delegate
                {
                    Close();
                },
                Keys.Alt | Keys.F4);

            // ==========================================
            // DỮ LIỆU
            // ==========================================
            menuLayer.DropDownItems.Clear();

            AddMenuItem(
                menuLayer,
                "Kết nối CSDL PostGIS...",
                delegate
                {
                    btnPostGIS_Click(
                        this, EventArgs.Empty);
                },
                Keys.Control | Keys.P);

            menuLayer.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuLayer,
                "Mở bảng thuộc tính lớp đang chọn",
                delegate
                {
                    OpenSelectedLayerAttributeTable();
                });

            AddMenuItem(
                menuLayer,
                "Thu phóng tới lớp đang chọn",
                delegate
                {
                    if (treeLayers == null ||
                        treeLayers.SelectedNode == null)
                    {
                        MessageBox.Show(
                            "Hãy chọn một lớp dữ liệu.",
                            "Chưa chọn lớp",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    ZoomToLayer(
                        treeLayers.SelectedNode);
                });

            menuLayer.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuLayer,
                "Bật toàn bộ lớp",
                delegate
                {
                    ShowAllLayers();
                });

            AddMenuItem(
                menuLayer,
                "Tắt toàn bộ lớp",
                delegate
                {
                    HideAllLayers();
                });

            menuLayer.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuLayer,
                "Gỡ lớp đang chọn khỏi bản đồ",
                delegate
                {
                    RemoveSelectedLayer();
                });

            // ==========================================
            // QUẢN LÝ
            // Nội dung được bổ sung bởi Editing.cs
            // ==========================================
            menuEdit.DropDownItems.Clear();
            menuEdit.Enabled = true;
            menuEdit.Visible = true;

            // ==========================================
            // TRA CỨU
            // ==========================================
            menuQuery.DropDownItems.Clear();

            AddMenuItem(
                menuQuery,
                "Thông tin đối tượng trên bản đồ",
                delegate
                {
                    ActivateInfoTool();
                });

            menuQuery.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuQuery,
                "Bỏ chọn tất cả đối tượng",
                delegate
                {
                    ClearAllSelections();
                });

            // ==========================================
            // PHÂN TÍCH
            // ==========================================
            menuAnalysis.DropDownItems.Clear();

            AddMenuItem(
                menuAnalysis,
                "Đo khoảng cách",
                delegate
                {
                    ActivateMeasureTool();
                });

            AddMenuItem(
                menuAnalysis,
                "Tạo vùng đệm",
                delegate
                {
                    ActivateBufferTool();
                });

            menuAnalysis.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuAnalysis,
                "Xóa kết quả đo",
                delegate
                {
                    ClearMeasureTool();
                });

            // ==========================================
            // HIỂN THỊ
            // ==========================================
            menuView.DropDownItems.Clear();

            AddMenuItem(
                menuView,
                "Di chuyển bản đồ",
                delegate
                {
                    btnPan_Click(
                        this, EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Phóng to",
                delegate
                {
                    btnZoomIn_Click(
                        this, EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Thu nhỏ",
                delegate
                {
                    btnZoomOut_Click(
                        this, EventArgs.Empty);
                });

            menuView.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuView,
                "Toàn bộ dữ liệu",
                delegate
                {
                    btnFullExtent_Click(
                        this, EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Làm mới bản đồ",
                delegate
                {
                    RefreshMapView();
                },
                Keys.F5);

            // ==========================================
            // TRỢ GIÚP
            // ==========================================
            menuHelp.DropDownItems.Clear();

            AddMenuItem(
                menuHelp,
                "Hướng dẫn sử dụng",
                delegate
                {
                    ShowQuickHelp();
                });

            menuHelp.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuHelp,
                "Giới thiệu phần mềm",
                delegate
                {
                    ShowAbout();
                });

            ConfigureToolbarIcons();
            CompactToolbarToSingleRow();

            _mainMenuInitialized = true;
        }

        // =====================================================
        // 2. MỞ BẢNG THUỘC TÍNH
        // =====================================================
        private void OpenSelectedLayerAttributeTable()
        {
            if (treeLayers == null ||
                treeLayers.SelectedNode == null)
            {
                MessageBox.Show(
                    "Hãy chọn một lớp trong danh sách dữ liệu.",
                    "Chưa chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            OpenAttributeTable(
                treeLayers.SelectedNode);
        }

        // =====================================================
        // 3. TẠO MỤC MENU
        // =====================================================
        private ToolStripMenuItem AddMenuItem(
            ToolStripMenuItem parent,
            string text,
            EventHandler clickHandler)
        {
            return AddMenuItem(
                parent,
                text,
                clickHandler,
                Keys.None);
        }

        private ToolStripMenuItem AddMenuItem(
            ToolStripMenuItem parent,
            string text,
            EventHandler clickHandler,
            Keys shortcut)
        {
            ToolStripMenuItem item =
                new ToolStripMenuItem(text);

            // ==========================================
            // SỬA LỖI INVALID ENUM ARGUMENT EXCEPTION
            //
            // WinForms không hỗ trợ dùng riêng Esc
            // làm ShortcutKeys của ToolStripMenuItem.
            //
            // Editing.cs đã có KeyDown xử lý Esc.
            // Vì vậy không đăng ký Esc tại đây.
            // ==========================================
            if (shortcut != Keys.None &&
                shortcut != Keys.Escape)
            {
                try
                {
                    item.ShortcutKeys = shortcut;
                    item.ShowShortcutKeys = true;
                }
                catch (InvalidEnumArgumentException)
                {
                    // Phím tắt không hợp lệ.
                    // Giữ mục menu, chỉ bỏ phím tắt.
                    item.ShortcutKeys = Keys.None;
                    item.ShowShortcutKeys = false;
                }
            }

            if (clickHandler != null)
            {
                item.Click += clickHandler;
            }

            if (parent != null)
            {
                parent.DropDownItems.Add(item);
            }

            return item;
        }

        // =====================================================
        // 4. CẤU HÌNH BIỂU TƯỢNG THANH CÔNG CỤ
        // =====================================================
        private void ConfigureToolbarIcons()
        {
            try
            {
                if (toolStripStandard != null)
                {
                    toolStripStandard.ShowItemToolTips = true;
                    toolStripStandard.ImageScalingSize =
                        new Size(20, 20);
                    toolStripStandard.AutoSize = false;
                    toolStripStandard.Height = 32;
                }

                if (toolStripTools != null)
                {
                    toolStripTools.ShowItemToolTips = true;
                    toolStripTools.ImageScalingSize =
                        new Size(20, 20);
                }

                SetToolbarIcon(
                    btnOpenLayer,
                    "open_layer.png",
                    "Mở dữ liệu Shapefile");

                SetToolbarIcon(
                    btnPostGIS,
                    "postgis.png",
                    "Kết nối CSDL PostGIS");

                SetToolbarIcon(
                    btnPan,
                    "pan.png",
                    "Di chuyển bản đồ");

                SetToolbarIcon(
                    btnZoomIn,
                    "zoom_in.png",
                    "Phóng to");

                SetToolbarIcon(
                    btnZoomOut,
                    "zoom_out.png",
                    "Thu nhỏ");

                SetToolbarIcon(
                    btnFullExtent,
                    "full_extent.png",
                    "Toàn bộ dữ liệu");

                SetToolbarIcon(
                    btnRefreshToolbar,
                    "refresh.png",
                    "Làm mới bản đồ");

                SetToolbarIcon(
                    btnInfo,
                    "info.png",
                    "Thông tin đối tượng");

                SetToolbarIcon(
                    btnQueryLocation,
                    "query_location.png",
                    "Truy vấn vị trí");

                SetToolbarIcon(
                    btnQueryAttribute,
                    "query_attribute.png",
                    "Truy vấn thuộc tính");

                SetToolbarIcon(
                    btnClearSelectionToolbar,
                    "clear_selection.png",
                    "Bỏ chọn");

                SetToolbarIcon(
                    btnMeasure,
                    "measure.png",
                    "Đo khoảng cách");

                SetToolbarIcon(
                    btnBuffer,
                    "buffer.png",
                    "Tạo vùng đệm");
            }
            catch (Exception ex)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không thể nạp biểu tượng: " +
                        ex.Message;
                }
            }
        }

        // =====================================================
        // 5. ĐỊNH DẠNG BIỂU TƯỢNG
        // =====================================================
        private void SetToolbarIcon(
            ToolStripButton button,
            string fileName,
            string tooltip)
        {
            if (button == null)
                return;

            Image icon = LoadToolbarIcon(fileName);

            if (icon != null)
            {
                if (button.Image != null)
                {
                    try
                    {
                        button.Image.Dispose();
                    }
                    catch
                    {
                    }
                }

                button.Image = icon;
            }

            button.DisplayStyle =
                ToolStripItemDisplayStyle.Image;

            button.ImageScaling =
                ToolStripItemImageScaling.SizeToFit;

            button.Text = tooltip;
            button.ToolTipText = tooltip;
            button.AutoToolTip = true;
            button.AutoSize = false;
            button.Size = new Size(30, 28);
            button.Margin = new Padding(2, 1, 2, 1);
            button.Padding = new Padding(1);
        }

        // =====================================================
        // 6. NẠP BIỂU TƯỢNG
        // =====================================================
        private Image LoadToolbarIcon(string fileName)
        {
            try
            {
                string runtimePath = Path.Combine(
                    Application.StartupPath,
                    "Icons",
                    fileName);

                if (File.Exists(runtimePath))
                {
                    return LoadImageWithoutLock(
                        runtimePath);
                }

                string projectPath = Path.GetFullPath(
                    Path.Combine(
                        Application.StartupPath,
                        @"..\..\Icons",
                        fileName));

                if (File.Exists(projectPath))
                {
                    return LoadImageWithoutLock(
                        projectPath);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private Image LoadImageWithoutLock(string path)
        {
            using (Image source =
                Image.FromFile(path))
            {
                return new Bitmap(source);
            }
        }

        // =====================================================
        // 7. GỘP THANH CÔNG CỤ
        // =====================================================
        private void CompactToolbarToSingleRow()
        {
            if (toolStripStandard == null)
                return;

            toolStripStandard.AutoSize = false;
            toolStripStandard.Height = 32;
            toolStripStandard.ImageScalingSize =
                new Size(20, 20);
            toolStripStandard.Padding =
                new Padding(4, 1, 4, 1);
            toolStripStandard.GripStyle =
                ToolStripGripStyle.Hidden;

            if (toolStripTools != null)
            {
                while (toolStripTools.Items.Count > 0)
                {
                    ToolStripItem item =
                        toolStripTools.Items[0];

                    toolStripTools.Items.RemoveAt(0);
                    toolStripStandard.Items.Add(item);
                }

                toolStripTools.Visible = false;
            }

            toolStripStandard.ShowItemToolTips = true;
        }

        // =====================================================
        // 8. HƯỚNG DẪN SỬ DỤNG
        // =====================================================
        private void ShowQuickHelp()
        {
            MessageBox.Show(
                "HƯỚNG DẪN SỬ DỤNG\n\n" +
                "1. Kết nối dữ liệu PostGIS.\n" +
                "2. Mở bản đồ GIS để xem các lớp dữ liệu.\n" +
                "3. Tra cứu thông tin đơn vị.\n" +
                "4. Quản lý dữ liệu qua bảng thuộc tính.\n" +
                "5. Sử dụng công cụ phân tích không gian.\n" +
                "6. Xem thống kê và báo cáo.\n\n" +
                "Phím Esc dùng để hủy thao tác đang vẽ.",
                "Hướng dẫn sử dụng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =====================================================
        // 9. THÔNG TIN PHẦN MỀM
        // =====================================================
        private void ShowAbout()
        {
            MessageBox.Show(
                "HỆ THỐNG CSDL GIS QUẢN LÝ ĐƠN VỊ HÀ NỘI\n\n" +
                "Nền tảng: C# Windows Forms + MapWinGIS\n" +
                "Cơ sở dữ liệu: PostgreSQL / PostGIS\n" +
                "Hệ tọa độ: EPSG:32648\n" +
                "Nền bản đồ: Ảnh vệ tinh cục bộ\n\n" +
                "Các phân hệ:\n" +
                "- Tổng quan\n" +
                "- Bản đồ GIS\n" +
                "- Tra cứu dữ liệu\n" +
                "- Quản lý hồ sơ\n" +
                "- Phân tích không gian\n" +
                "- Thống kê và báo cáo\n" +
                "- Kiểm tra chất lượng dữ liệu\n\n" +
                "Bộ dữ liệu Google AI Studio là " +
                "dữ liệu mẫu chưa xác minh.",
                "Giới thiệu hệ thống",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
