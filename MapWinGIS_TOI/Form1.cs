using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // FORM1.CS - KHỞI TẠO TOÀN HỆ THỐNG
    //
    // Cấu hình hiện hành:
    // - Hà Nội only;
    // - CRS: EPSG:32648;
    // - Không dùng tile online;
    // - Không tự động nạp PostGIS khi vừa mở chương trình;
    // - Các layer được quản lý bằng logical key trong _loadedLayers.
    // =============================================================
    public partial class Form1 : Form
    {
        private readonly Dictionary<string, int> _loadedLayers =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        private readonly GlobalSettings _globalSettings =
            new GlobalSettings();

        private bool _applicationInitialized =
            false;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public Form1()
        {
            InitializeComponent();

            if (btnPostGIS != null)
            {
                btnPostGIS.Click -=
                    btnPostGIS_Click;

                btnPostGIS.Click +=
                    btnPostGIS_Click;
            }

            FormClosing -=
                Form1_FormClosing;

            FormClosing +=
                Form1_FormClosing;

            FormClosed -=
                Form1_FormClosed;

            FormClosed +=
                Form1_FormClosed;
        }

        // =========================================================
        // LOAD ỨNG DỤNG
        //
        // Thứ tự khởi tạo:
        //
        // 1. Layout
        // 2. Global MapWinGIS settings
        // 3. Navigation + Map EPSG:32648
        // 4. Layer manager
        // 5. Query / Measure
        // 6. Menu nghiệp vụ
        // 7. Editing / Search / Spatial Query
        // 8. Professional UI
        // 8.1. Thanh Zoom dọc
        // 9. StatusBar
        // 10. Đồng bộ trạng thái giao diện
        //
        // Không tự động kết nối PostGIS.
        // =========================================================
        private void Form1_Load(
            object sender,
            EventArgs e)
        {
            if (_applicationInitialized)
            {
                return;
            }

            _applicationInitialized =
                true;

            try
            {
                // -------------------------------------------------
                // 1. LAYOUT
                // -------------------------------------------------
                InitializeLayout();

                // -------------------------------------------------
                // 2. CẤU HÌNH MAPWINGIS CHUNG
                //
                // Dữ liệu của hệ thống được chuẩn hóa về EPSG:32648.
                // Không để MapWinGIS tự reproject âm thầm khi add layer.
                // -------------------------------------------------
                InitializeMapWinGISSettings();

                // -------------------------------------------------
                // 3. ĐIỀU HƯỚNG + MAP
                //
                // InitOpenStreetMap() chỉ là tên hàm tương thích cũ.
                // Bên trong thực tế khởi tạo Hanoi-only EPSG:32648.
                // -------------------------------------------------
                InitNavigationTools();
                InitOpenStreetMap();

                // -------------------------------------------------
                // 4. QUẢN LÝ LỚP
                // -------------------------------------------------
                InitLayerManager();

                // Basemap đã được Map.cs nạp ở bước 3. Sau khi LayerManager
                // khởi tạo, bảo đảm raster vẫn ở dưới cùng.
                EnsureSatelliteBasemapBelowPostGIS();

                // -------------------------------------------------
                // 5. CÔNG CỤ BẢN ĐỒ
                // -------------------------------------------------
                InitQueryTools();
                InitMeasureTools();

                // -------------------------------------------------
                // 6. MENU CHÍNH
                // -------------------------------------------------
                InitMainMenu();
                InitSatelliteBasemapTools();

                // -------------------------------------------------
                // 7. NGHIỆP VỤ
                // -------------------------------------------------
                InitEditingTools();
                InitUnitSearchTools();
                InitSpatialQueryTools();

                // -------------------------------------------------
                // 8. GIAO DIỆN CHUYÊN NGHIỆP
                //
                // Gọi sau khi các module đã thêm menu riêng.
                // -------------------------------------------------
                ApplyProfessionalUI();

                // -------------------------------------------------
                // 8.1. THANH ZOOM DỌC
                //
                // ZoomSlider.cs tạo panel + TrackBar dọc trực tiếp
                // trên vùng bản đồ. Gọi sau ProfessionalUI để tránh
                // bị layout/giao diện chuyên nghiệp vẽ đè.
                // -------------------------------------------------
                InitVerticalZoomSlider();

                // -------------------------------------------------
                // 9. STATUS BAR
                //
                // Gọi sau ProfessionalUI để text CRS/tọa độ cuối cùng
                // không bị module giao diện ghi đè.
                // -------------------------------------------------
                InitStatusBar();

                // -------------------------------------------------
                // 10. ĐỒNG BỘ GIAO DIỆN
                // -------------------------------------------------
                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                if (axMap2 != null)
                {
                    axMap2.CursorMode =
                        tkCursorMode.cmPan;
                }

                if (lblStatusProjection != null)
                {
                    lblStatusProjection.Text =
                        "CRS: EPSG:32648 | mét";
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Sẵn sàng | Hà Nội | EPSG:32648 | " +
                        "Nền vệ tinh cục bộ | Bấm PostGIS để nạp dữ liệu.";
                }
            }
            catch (Exception ex)
            {
                _applicationInitialized =
                    false;

                MessageBox.Show(
                    "Không thể khởi tạo chương trình.\n\n" +
                    ex.Message,
                    "Lỗi khởi tạo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Khởi tạo chương trình thất bại.";
                }
            }
        }

        // =========================================================
        // LAYOUT
        // =========================================================
        private void InitializeLayout()
        {
            if (mainContainer != null)
            {
                const int tocWidth =
                    275;

                mainContainer.FixedPanel =
                    FixedPanel.Panel1;

                mainContainer.IsSplitterFixed =
                    false;

                if (mainContainer.Width >
                    tocWidth + 500)
                {
                    mainContainer.SplitterDistance =
                        tocWidth;
                }
            }

            if (axMap2 != null)
            {
                axMap2.Dock =
                    DockStyle.Fill;
            }

            if (lblMapPlaceholder != null)
            {
                lblMapPlaceholder.Visible =
                    false;
            }

            if (panelLayerBar != null)
            {
                panelLayerBar.Visible =
                    true;
            }
        }

        // =========================================================
        // GLOBAL SETTINGS MAPWINGIS
        //
        // Toàn hệ thống quản lý projection một cách chủ động:
        // - Map: EPSG:32648;
        // - PostGIS: EPSG:32648;
        // - Shapefile khác CRS: Navigation.cs tự reproject về 32648.
        //
        // Vì vậy không bật auto-reproject khi AddLayer.
        // =========================================================
        private void InitializeMapWinGISSettings()
        {
            _globalSettings.ReprojectLayersOnAdding =
                false;

            _globalSettings.AllowProjectionMismatch =
                false;

            _globalSettings.AllowLayersWithoutProjections =
                false;
        }

        // =========================================================
        // FORM CLOSING
        // =========================================================
        private void Form1_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            try
            {
                // ---------------------------------------------
                // Dừng các module có event / trạng thái runtime.
                // ---------------------------------------------
                StopEditingTools();
                StopStatusBar();
                StopOpenStreetMapSafely();

                if (axMap2 == null)
                {
                    return;
                }

                // ---------------------------------------------
                // Xóa kết quả đo.
                // ---------------------------------------------
                try
                {
                    axMap2.Measuring.Clear();
                }
                catch
                {
                }

                // ---------------------------------------------
                // Trở về Pan trước khi gỡ layer.
                // ---------------------------------------------
                try
                {
                    axMap2.CursorMode =
                        tkCursorMode.cmPan;
                }
                catch
                {
                }

                // ---------------------------------------------
                // Gỡ toàn bộ layer khỏi map.
                // ---------------------------------------------
                try
                {
                    axMap2.RemoveAllLayers();
                }
                catch
                {
                }
            }
            catch
            {
                // Không chặn quá trình đóng ứng dụng.
            }
        }

        // =========================================================
        // FORM CLOSED
        // =========================================================
        private void Form1_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            try
            {
                _loadedLayers.Clear();

                if (_layerContextMenu != null)
                {
                    _layerContextMenu.Dispose();
                    _layerContextMenu =
                        null;
                }

                _applicationInitialized =
                    false;
            }
            catch
            {
            }
        }
    }
}