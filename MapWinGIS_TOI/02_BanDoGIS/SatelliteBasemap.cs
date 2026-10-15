
using System;
using System.IO;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // ============================================================
    // NỀN VỆ TINH HÀ NỘI - PHIÊN BẢN TỐI ƯU
    //
    // Chỉ sử dụng một cơ chế quản lý raster:
    //   _basemapLayerHandle
    //   _basemapImage
    //
    // Hai biến trên thuộc Map.cs.
    //
    // File này chỉ quản lý:
    // - Menu mở ảnh vệ tinh
    // - Kiểm tra CRS
    // - Ẩn / hiện ảnh
    // - Gỡ ảnh
    //
    // Không tự nạp trùng raster khi khởi động.
    // Hệ tọa độ: EPSG:32648.
    // ============================================================

    public partial class Form1
    {
        private const string SATELLITE_LAYER_NAME =
            "Nền vệ tinh Hà Nội";

        private const string DEFAULT_SATELLITE_FOLDER =
            "Data";

        private const string DEFAULT_SATELLITE_FILENAME =
            "HANOI_SATELLITE_32648.tif";

        private bool _satelliteMenuInitialized = false;

        // Chỉ dùng để tránh mở lại cùng tệp được chọn thủ công.
        private string _manualSatellitePath = null;

        // ========================================================
        // 1. KHỞI TẠO MENU NỀN VỆ TINH
        // ========================================================

        private void InitSatelliteBasemapTools()
        {
            if (_satelliteMenuInitialized ||
                menuView == null)
            {
                return;
            }

            menuView.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuView,
                "Mở nền vệ tinh Hà Nội (EPSG:32648)...",
                delegate
                {
                    OpenSatelliteBasemap();
                });

            AddMenuItem(
                menuView,
                "Ẩn / Hiện nền vệ tinh",
                delegate
                {
                    ToggleSatelliteBasemap();
                });

            AddMenuItem(
                menuView,
                "Gỡ nền vệ tinh",
                delegate
                {
                    RemoveSatelliteBasemap();
                });

            _satelliteMenuInitialized = true;

            // Map.cs đã tự mở nền vệ tinh khi khởi động.
            // Chỉ thử nguồn dự phòng nếu chưa có raster.
            TryLoadDefaultSatelliteBasemap();
        }

        // ========================================================
        // 2. KIỂM TRA NỀN DỰ PHÒNG
        //
        // Nếu Map.cs đã nạp raster thì KHÔNG mở lại.
        // ========================================================

        private void TryLoadDefaultSatelliteBasemap()
        {
            if (axMap2 == null ||
                _basemapLayerHandle >= 0)
            {
                return;
            }

            try
            {
                string filename = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    DEFAULT_SATELLITE_FOLDER,
                    DEFAULT_SATELLITE_FILENAME);

                if (File.Exists(filename))
                {
                    LoadSatelliteBasemap(filename);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "TryLoadDefaultSatelliteBasemap: " +
                    ex);
            }
        }

        // ========================================================
        // 3. CHỌN TỆP ẢNH VỆ TINH
        // ========================================================

        private void OpenSatelliteBasemap()
        {
            if (axMap2 == null)
            {
                return;
            }

            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Title =
                    "Chọn ảnh vệ tinh Hà Nội - EPSG:32648";

                dialog.Filter =
                    "Raster GIS (*.tif;*.tiff;*.vrt;*.img;*.jp2;*.ecw)|" +
                    "*.tif;*.tiff;*.vrt;*.img;*.jp2;*.ecw|" +
                    "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff|" +
                    "Tất cả tệp (*.*)|*.*";

                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                LoadSatelliteBasemap(
                    dialog.FileName);
            }
        }

        // ========================================================
        // 4. NẠP ẢNH VỆ TINH
        //
        // Kiểm tra raster trước khi thay thế nền đang dùng.
        // ========================================================

        private void LoadSatelliteBasemap(
            string filename)
        {
            if (axMap2 == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(filename) ||
                !File.Exists(filename))
            {
                MessageBox.Show(
                    "Không tìm thấy tệp ảnh vệ tinh.",
                    "Thiếu dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Đường dẫn raster không hợp lệ.\n\n" +
                    ex.Message,
                    "Lỗi đường dẫn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // ----------------------------------------------------
            // Nếu cùng tệp đang mở thì không mở lại raster.
            // ----------------------------------------------------

            bool sameManualLayer = false;

            try
            {
                sameManualLayer =
                    LaNenVeTinhHienTai() &&
                    string.Equals(
                        axMap2.get_LayerName(
                            _basemapLayerHandle),
                        SATELLITE_LAYER_NAME,
                        StringComparison.CurrentCultureIgnoreCase) &&
                    !string.IsNullOrEmpty(
                        _manualSatellitePath) &&
                    string.Equals(
                        _manualSatellitePath,
                        fullPath,
                        StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                sameManualLayer = false;
            }

            if (sameManualLayer)
            {
                try
                {
                    axMap2.set_LayerVisible(
                        _basemapLayerHandle,
                        true);

                    axMap2.ZoomToLayer(
                        _basemapLayerHandle);
                }
                catch (Exception ex)
                {
                    CrashLogger.Write(
                        "Reuse satellite ERROR: " +
                        ex);
                }

                return;
            }

            Cursor previousCursor =
                Cursor.Current;

            MapWinGIS.Image probe = null;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                EnsureLocalMapProjection32648();

                // ---------------------------------------------
                // Mở tạm để kiểm tra raster và hệ tọa độ.
                // Không thêm vào bản đồ tại bước này.
                // ---------------------------------------------

                probe = new MapWinGIS.Image();

                bool opened = probe.Open(
                    fullPath,
                    ImageType.USE_FILE_EXTENSION,
                    false,
                    null);

                if (!opened)
                {
                    throw new InvalidOperationException(
                        "Không mở được raster.\n\n" +
                        probe.get_ErrorMsg(
                            probe.LastErrorCode));
                }

                ValidateSatelliteProjection32648(
                    probe);

                probe.Close();
                probe = null;

                // ---------------------------------------------
                // Dùng lại hàm nạp raster duy nhất ở Map.cs.
                //
                // Hàm này quản lý AddLayer, RemoveLayer,
                // vị trí raster và bộ nhớ.
                //
                // true = thu phóng tới ảnh vừa chọn.
                // ---------------------------------------------

                bool loaded = LoadLocalBasemap(
                    fullPath,
                    SATELLITE_LAYER_NAME,
                    true);

                if (!loaded)
                {
                    return;
                }

                _manualSatellitePath = fullPath;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Nền vệ tinh Hà Nội | EPSG:32648 | " +
                        Path.GetFileName(fullPath);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "LoadSatelliteBasemap ERROR: " +
                    ex);

                MessageBox.Show(
                    "Không thể mở nền vệ tinh.\n\n" +
                    ex.Message +
                    "\n\nRaster phải có CRS EPSG:32648.",
                    "Lỗi nền vệ tinh",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (probe != null)
                {
                    try
                    {
                        probe.Close();
                    }
                    catch
                    {
                    }
                }

                Cursor.Current =
                    previousCursor;
            }
        }

        // ========================================================
        // 5. KIỂM TRA CRS EPSG:32648
        // ========================================================

        private void ValidateSatelliteProjection32648(
            MapWinGIS.Image image)
        {
            if (image == null ||
                image.GeoProjection == null)
            {
                throw new InvalidOperationException(
                    "Raster thiếu thông tin hệ tọa độ.");
            }

            GeoProjection target =
                new GeoProjection();

            if (!target.ImportFromEPSG(32648))
            {
                throw new InvalidOperationException(
                    "MapWinGIS không khởi tạo được EPSG:32648.");
            }

            bool same = false;

            try
            {
                same =
                    image.GeoProjection.get_IsSame(
                        target);
            }
            catch
            {
                same = false;
            }

            if (!same)
            {
                int epsg = -1;
                bool detected = false;

                try
                {
                    detected =
                        image.GeoProjection.TryAutoDetectEpsg(
                            out epsg);
                }
                catch
                {
                    detected = false;
                }

                if (!detected || epsg != 32648)
                {
                    throw new InvalidOperationException(
                        "Raster không phải EPSG:32648.\n\n" +
                        "CRS nhận diện: " +
                        (
                            detected
                                ? "EPSG:" + epsg
                                : "Không xác định"
                        ) +
                        "\n\nHãy chuyển hệ tọa độ trong QGIS trước.");
                }
            }
        }

        // ========================================================
        // 6. KIỂM TRA NỀN ĐANG SỬ DỤNG
        //
        // Không can thiệp nhầm nền giao thông/tác chiến.
        // ========================================================

        private bool LaNenVeTinhHienTai()
        {
            if (axMap2 == null ||
                _basemapLayerHandle < 0)
            {
                return false;
            }

            try
            {
                string layerName =
                    axMap2.get_LayerName(
                        _basemapLayerHandle);

                return
                    !string.IsNullOrWhiteSpace(layerName) &&
                    layerName.IndexOf(
                        "vệ tinh",
                        StringComparison.CurrentCultureIgnoreCase)
                    >= 0;
            }
            catch
            {
                return false;
            }
        }

        // ========================================================
        // 7. ẨN / HIỆN NỀN VỆ TINH
        // ========================================================

        private void ToggleSatelliteBasemap()
        {
            if (!LaNenVeTinhHienTai())
            {
                MessageBox.Show(
                    "Chưa có nền vệ tinh đang hoạt động.\n\n" +
                    "Hãy chọn Mở nền vệ tinh Hà Nội.",
                    "Nền vệ tinh",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                bool visible =
                    axMap2.get_LayerVisible(
                        _basemapLayerHandle);

                axMap2.set_LayerVisible(
                    _basemapLayerHandle,
                    !visible);

                // Chỉ vẽ lại khi trạng thái thực sự đổi.
                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        !visible
                            ? "Đã bật nền vệ tinh."
                            : "Đã ẩn nền vệ tinh.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể ẩn/hiện nền vệ tinh.\n\n" +
                    ex.Message,
                    "Lỗi bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ========================================================
        // 8. GỠ NỀN VỆ TINH
        //
        // Dùng lại RemoveCurrentBasemap() trong Map.cs.
        // ========================================================

        private void RemoveSatelliteBasemap()
        {
            if (!LaNenVeTinhHienTai())
            {
                return;
            }

            RemoveCurrentBasemap();

            _manualSatellitePath = null;

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Đã gỡ nền vệ tinh Hà Nội.";
            }
        }
    }
}
