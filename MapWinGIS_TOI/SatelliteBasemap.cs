using System;
using System.IO;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // NỀN VỆ TINH HÀ NỘI - GIỮ NGUYÊN EPSG:32648
    //
    // Lý do dùng raster cục bộ thay vì tile online:
    // - toàn hệ thống hiện dùng EPSG:32648;
    // - đo khoảng cách / buffer / số hóa đang dựa trên mét UTM;
    // - tile online thông thường dùng EPSG:3857.
    //
    // File vệ tinh nên là GeoTIFF/VRT/IMG đã được reproject
    // về EPSG:32648 trước khi mở.
    // =============================================================
    public partial class Form1
    {
        private const string SATELLITE_LAYER_NAME =
            "Nền vệ tinh Hà Nội";

        // Nếu đặt GeoTIFF tại:
        // bin\Debug\Data\HANOI_SATELLITE_32648.tif
        // chương trình sẽ tự mở khi khởi động.
        private const string DEFAULT_SATELLITE_FOLDER =
            "Data";

        private const string DEFAULT_SATELLITE_FILENAME =
            "HANOI_SATELLITE_32648.tif";

        private int _satelliteLayerHandle =
            -1;

        private bool _satelliteMenuInitialized =
            false;

        // =========================================================
        // KHỞI TẠO MENU VỆ TINH
        //
        // Gọi SAU InitMainMenu(), vì InitMainMenu() sẽ Clear menuView.
        // =========================================================
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

            _satelliteMenuInitialized =
                true;

            // Tự động nạp nền vệ tinh mặc định nếu file tồn tại.
            TryLoadDefaultSatelliteBasemap();
        }

        // =========================================================
        // TỰ ĐỘNG NẠP FILE VỆ TINH MẶC ĐỊNH
        //
        // Không hiện lỗi nếu chưa có file; người dùng vẫn có thể
        // vào Hiển thị -> Mở nền vệ tinh Hà Nội... để chọn thủ công.
        // =========================================================
        private void TryLoadDefaultSatelliteBasemap()
        {
            try
            {
                string baseFolder =
                    AppDomain.CurrentDomain.BaseDirectory;

                string filename =
                    Path.Combine(
                        baseFolder,
                        DEFAULT_SATELLITE_FOLDER,
                        DEFAULT_SATELLITE_FILENAME);

                if (!File.Exists(
                    filename))
                {
                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Chưa có nền vệ tinh mặc định. " +
                            "Đặt file Data\\" +
                            DEFAULT_SATELLITE_FILENAME +
                            " hoặc mở thủ công từ menu Hiển thị.";
                    }

                    return;
                }

                LoadSatelliteBasemap(
                    filename);
            }
            catch
            {
                // Không chặn ứng dụng nếu nền vệ tinh mặc định lỗi.
            }
        }

        // =========================================================
        // MỞ RASTER VỆ TINH
        // =========================================================
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

                dialog.Multiselect =
                    false;

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                LoadSatelliteBasemap(
                    dialog.FileName);
            }
        }

        // =========================================================
        // LOAD RASTER VỆ TINH
        // =========================================================
        private void LoadSatelliteBasemap(
            string filename)
        {
            if (string.IsNullOrWhiteSpace(
                filename)
                ||
                !File.Exists(
                    filename))
            {
                MessageBox.Show(
                    "Không tìm thấy tệp ảnh vệ tinh.",
                    "Thiếu dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Cursor oldCursor =
                Cursor.Current;

            MapWinGIS.Image image =
                null;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                EnsureLocalMapProjection32648();

                image =
                    new MapWinGIS.Image();

                if (!image.Open(
                    filename,
                    ImageType.USE_FILE_EXTENSION,
                    false,
                    null))
                {
                    throw new InvalidOperationException(
                        "MapWinGIS không mở được raster.\n\n" +
                        image.get_ErrorMsg(
                            image.LastErrorCode));
                }

                ValidateSatelliteProjection32648(
                    image);

                // Chỉ giữ một nền vệ tinh tại một thời điểm.
                RemoveSatelliteBasemap();

                int handle =
                    axMap2.AddLayer(
                        image,
                        true);

                if (handle < 0)
                {
                    throw new InvalidOperationException(
                        "Không thêm được raster vệ tinh vào bản đồ.\n\n" +
                        axMap2.get_ErrorMsg(
                            axMap2.LastErrorCode));
                }

                _satelliteLayerHandle =
                    handle;

                axMap2.set_LayerName(
                    handle,
                    SATELLITE_LAYER_NAME);

                // AddLayer thường thêm lên trên.
                // Chuyển raster xuống dưới toàn bộ vector.
                int position =
                    axMap2.get_LayerPosition(
                        handle);

                if (position >= 0)
                {
                    axMap2.MoveLayerBottom(
                        position);
                }

                axMap2.set_LayerVisible(
                    handle,
                    true);

                axMap2.ZoomToLayer(
                    handle);

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã mở nền vệ tinh Hà Nội | EPSG:32648 | " +
                        Path.GetFileName(
                            filename);
                }
            }
            catch (Exception ex)
            {
                if (image != null &&
                    _satelliteLayerHandle < 0)
                {
                    try
                    {
                        image.Close();
                    }
                    catch
                    {
                    }
                }

                MessageBox.Show(
                    "Không mở được nền vệ tinh.\n\n" +
                    ex.Message +
                    "\n\n" +
                    "Hãy bảo đảm raster đã được reproject về EPSG:32648.",
                    "Lỗi nền vệ tinh",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current =
                    oldCursor;
            }
        }

        // =========================================================
        // KIỂM TRA CRS RASTER
        // =========================================================
        private void ValidateSatelliteProjection32648(
            MapWinGIS.Image image)
        {
            if (image == null ||
                image.GeoProjection == null)
            {
                throw new InvalidOperationException(
                    "Raster không có thông tin hệ tọa độ.");
            }

            string proj4 =
                "";

            try
            {
                proj4 =
                    image.GeoProjection.ExportToProj4();
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(
                proj4))
            {
                throw new InvalidOperationException(
                    "Raster chưa có CRS hoặc MapWinGIS không đọc được CRS.");
            }

            GeoProjection target =
                new GeoProjection();

            if (!target.ImportFromEPSG(
                32648))
            {
                throw new InvalidOperationException(
                    "MapWinGIS không nạp được EPSG:32648.");
            }

            bool same =
                false;

            try
            {
                same =
                    image.GeoProjection.get_IsSame(
                        target);
            }
            catch
            {
                same =
                    false;
            }

            if (!same)
            {
                int epsg =
                    -1;

                bool detected =
                    false;

                try
                {
                    detected =
                        image.GeoProjection.TryAutoDetectEpsg(
                            out epsg);
                }
                catch
                {
                }

                string detectedText =
                    detected
                        ? "EPSG:" + epsg
                        : "không xác định được EPSG";

                throw new InvalidOperationException(
                    "Raster vệ tinh không phải EPSG:32648.\n\n" +
                    "CRS phát hiện: " +
                    detectedText +
                    ".\n\n" +
                    "Hãy reproject raster về EPSG:32648 trong QGIS trước.");
            }
        }

        // =========================================================
        // ẨN / HIỆN
        // =========================================================
        private void ToggleSatelliteBasemap()
        {
            if (axMap2 == null ||
                _satelliteLayerHandle < 0)
            {
                MessageBox.Show(
                    "Chưa có nền vệ tinh.\n\n" +
                    "Vào Hiển thị → Mở nền vệ tinh Hà Nội...",
                    "Nền vệ tinh",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                bool visible =
                    axMap2.get_LayerVisible(
                        _satelliteLayerHandle);

                axMap2.set_LayerVisible(
                    _satelliteLayerHandle,
                    !visible);

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
                    "Không đổi được trạng thái nền vệ tinh.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // GỠ NỀN VỆ TINH
        // =========================================================
        private void RemoveSatelliteBasemap()
        {
            if (axMap2 == null ||
                _satelliteLayerHandle < 0)
            {
                _satelliteLayerHandle =
                    -1;

                return;
            }

            try
            {
                axMap2.RemoveLayer(
                    _satelliteLayerHandle);
            }
            catch
            {
            }

            _satelliteLayerHandle =
                -1;

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }
        }
    }
}