using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        private bool _mainMenuInitialized = false;

        // =========================================================
        // MENU CHÍNH - PHIÊN BẢN CHUẨN HÓA
        //
        // Quy trình nghiệp vụ:
        // Tệp -> Dữ liệu -> Quản lý -> Tra cứu -> Phân tích
        //      -> Hiển thị -> Trợ giúp
        //
        // Không đưa các chức năng giống nhau vào nhiều menu.
        // Toolbar chỉ giữ các thao tác nhanh thường dùng.
        // =========================================================
        private void InitMainMenu()
        {
            if (_mainMenuInitialized)
                return;

            // -----------------------------------------------------
            // TÊN MENU
            // -----------------------------------------------------
            menuFile.Text = "Tệp";
            menuLayer.Text = "Dữ liệu";
            menuEdit.Text = "Quản lý";
            menuQuery.Text = "Tra cứu";
            menuAnalysis.Text = "Phân tích";
            menuView.Text = "Hiển thị";
            menuHelp.Text = "Trợ giúp";

            // =====================================================
            // 1. TỆP
            // =====================================================
            menuFile.DropDownItems.Clear();

            AddMenuItem(
                menuFile,
                "Mở dữ liệu Shapefile...",
                delegate
                {
                    btnOpenLayer_Click(
                        this,
                        EventArgs.Empty);
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

            // =====================================================
            // 2. DỮ LIỆU
            // =====================================================
            menuLayer.DropDownItems.Clear();

            AddMenuItem(
                menuLayer,
                "Kết nối CSDL PostGIS...",
                delegate
                {
                    btnPostGIS_Click(
                        this,
                        EventArgs.Empty);
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
                    if (treeLayers.SelectedNode == null)
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

            // =====================================================
            // 3. QUẢN LÝ
            //
            // Editing.cs sẽ cấu hình nội dung nghiệp vụ sau khi
            // InitMainMenu() hoàn tất.
            // =====================================================
            menuEdit.DropDownItems.Clear();
            menuEdit.Enabled = true;
            menuEdit.Visible = true;

            // =====================================================
            // 4. TRA CỨU
            //
            // UnitSearchIntegration.cs sẽ chèn "Tra cứu đơn vị..."
            // lên đầu menu này.
            // =====================================================
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

            // =====================================================
            // 5. PHÂN TÍCH
            //
            // SpatialQueryIntegration.cs sẽ chèn truy vấn không gian
            // chuyên đề vào đầu menu này.
            // =====================================================
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

            // =====================================================
            // 6. HIỂN THỊ
            // =====================================================
            menuView.DropDownItems.Clear();

            AddMenuItem(
                menuView,
                "Di chuyển bản đồ",
                delegate
                {
                    btnPan_Click(
                        this,
                        EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Phóng to",
                delegate
                {
                    btnZoomIn_Click(
                        this,
                        EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Thu nhỏ",
                delegate
                {
                    btnZoomOut_Click(
                        this,
                        EventArgs.Empty);
                });

            menuView.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuView,
                "Toàn bộ dữ liệu",
                delegate
                {
                    btnFullExtent_Click(
                        this,
                        EventArgs.Empty);
                });

            AddMenuItem(
                menuView,
                "Làm mới bản đồ",
                delegate
                {
                    RefreshMapView();
                },
                Keys.F5);

            // =====================================================
            // 7. TRỢ GIÚP
            // =====================================================
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

            // =====================================================
            // ICON TOOLBAR
            // =====================================================
            ConfigureToolbarIcons();
            CompactToolbarToSingleRow();

            _mainMenuInitialized = true;
        }

        // =========================================================
        // MỞ BẢNG THUỘC TÍNH CỦA LỚP ĐANG CHỌN
        // =========================================================
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

        // =========================================================
        // TẠO MENU ITEM
        // =========================================================
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
                new ToolStripMenuItem(
                    text);

            if (shortcut != Keys.None)
            {
                item.ShortcutKeys =
                    shortcut;

                item.ShowShortcutKeys =
                    true;
            }

            if (clickHandler != null)
            {
                item.Click +=
                    clickHandler;
            }

            parent.DropDownItems.Add(
                item);

            return item;
        }

        // =========================================================
        // ICON TOOLBAR
        // =========================================================
        private void ConfigureToolbarIcons()
        {
            try
            {
                if (toolStripStandard != null)
                {
                    toolStripStandard.ShowItemToolTips =
                        true;

                    toolStripStandard.ImageScalingSize =
                        new Size(
                            20,
                            20);

                    toolStripStandard.AutoSize =
                        false;

                    toolStripStandard.Height =
                        32;
                }

                if (toolStripTools != null)
                {
                    toolStripTools.ShowItemToolTips =
                        true;

                    toolStripTools.ImageScalingSize =
                        new Size(
                            20,
                            20);
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
                        "Không thể nạp icon: " +
                        ex.Message;
                }
            }
        }

        private void SetToolbarIcon(
            ToolStripButton button,
            string fileName,
            string tooltip)
        {
            if (button == null)
                return;

            Image icon =
                LoadToolbarIcon(
                    fileName);

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

                button.Image =
                    icon;
            }

            button.DisplayStyle =
                ToolStripItemDisplayStyle.Image;

            button.ImageScaling =
                ToolStripItemImageScaling.SizeToFit;

            button.Text =
                tooltip;

            button.ToolTipText =
                tooltip;

            button.AutoToolTip =
                true;

            button.AutoSize =
                false;

            button.Size =
                new Size(
                    30,
                    28);

            button.Margin =
                new Padding(
                    2,
                    1,
                    2,
                    1);

            button.Padding =
                new Padding(1);
        }

        private Image LoadToolbarIcon(
            string fileName)
        {
            try
            {
                string runtimePath =
                    Path.Combine(
                        Application.StartupPath,
                        "Icons",
                        fileName);

                if (File.Exists(
                    runtimePath))
                {
                    return LoadImageWithoutLock(
                        runtimePath);
                }

                string projectPath =
                    Path.GetFullPath(
                        Path.Combine(
                            Application.StartupPath,
                            @"..\..\Icons",
                            fileName));

                if (File.Exists(
                    projectPath))
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

        private Image LoadImageWithoutLock(
            string path)
        {
            using (Image source =
                Image.FromFile(
                    path))
            {
                return new Bitmap(
                    source);
            }
        }

        // =========================================================
        // CHUYỂN TOOLBAR THỨ HAI VỀ TOOLBAR CHÍNH
        // =========================================================
        private void CompactToolbarToSingleRow()
        {
            if (toolStripStandard == null)
                return;

            toolStripStandard.AutoSize =
                false;

            toolStripStandard.Height =
                32;

            toolStripStandard.ImageScalingSize =
                new Size(
                    20,
                    20);

            toolStripStandard.Padding =
                new Padding(
                    4,
                    1,
                    4,
                    1);

            toolStripStandard.GripStyle =
                ToolStripGripStyle.Hidden;

            if (toolStripTools != null)
            {
                while (toolStripTools.Items.Count > 0)
                {
                    ToolStripItem item =
                        toolStripTools.Items[0];

                    toolStripTools.Items.RemoveAt(
                        0);

                    toolStripStandard.Items.Add(
                        item);
                }

                toolStripTools.Visible =
                    false;
            }

            toolStripStandard.ShowItemToolTips =
                true;
        }

        // =========================================================
        // HƯỚNG DẪN
        // =========================================================
        private void ShowQuickHelp()
        {
            MessageBox.Show(
                "QUY TRÌNH SỬ DỤNG\n\n" +
                "1. Dữ liệu > Kết nối CSDL PostGIS để nạp dữ liệu.\n" +
                "2. Tra cứu > Tra cứu đơn vị để tìm hồ sơ đơn vị.\n" +
                "3. Quản lý > Thêm mới để cập nhật Point/Line/Polygon.\n" +
                "4. Chuột phải lớp dữ liệu để mở bảng thuộc tính.\n" +
                "5. Phân tích dùng cho truy vấn không gian, đo và vùng đệm.\n" +
                "6. Hiển thị dùng cho điều hướng bản đồ.\n\n" +
                "Toolbar chỉ giữ các thao tác nhanh thường dùng.",
                "Hướng dẫn sử dụng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // GIỚI THIỆU
        // =========================================================
        private void ShowAbout()
        {
            MessageBox.Show(
                "GIS DESKTOP - QUẢN LÝ ĐƠN VỊ TRÊN ĐỊA BÀN HÀ NỘI\n\n" +
                "Nền tảng: C# WinForms + MapWinGIS\n" +
                "CSDL: PostgreSQL / PostGIS\n" +
                "Bản đồ nền: OpenStreetMap\n\n" +
                "Chức năng chính:\n" +
                "• Quản lý dữ liệu Point / Line / Polygon\n" +
                "• Tra cứu và hồ sơ đơn vị\n" +
                "• Biên tập dữ liệu không gian\n" +
                "• Truy vấn và kiểm tra dữ liệu GIS\n" +
                "• Đo đạc và vùng đệm",
                "Giới thiệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}