using System;
using System.IO;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // MAP.CS - HÀ NỘI ONLY / EPSG:32648
    //
    // Basemap cục bộ ưu tiên:
    //   Basemaps\HANOI_SATELLITE_2M_32648.tif
    //
    // Nếu file 2 m chưa có, hệ thống tự fallback sang:
    //   Basemaps\HANOI_SATELLITE_32648.tif
    //
    // Các basemap dự kiến:
    //   HANOI_SATELLITE_2M_32648.tif
    //   HANOI_ROADS_32648.tif
    //   HANOI_TACTICAL_32648.tif
    //
    // Toàn hệ thống:
    //   EPSG:32648
    //   Đơn vị: mét
    //
    // KHÔNG dùng tile online trong MapWinGIS.
    // =============================================================
    public partial class Form1
    {
        private bool _tilesConfigured = false;
        private bool _tilesShuttingDown = false;

        private const int HANOI_MAP_SRID = 32648;

        // =========================================================
        // QUẢN LÝ BASEMAP
        // =========================================================
        private int _basemapLayerHandle = -1;
        private MapWinGIS.Image _basemapImage = null;

        // Ảnh vệ tinh độ phân giải 2 m/pixel - ưu tiên sử dụng.
        private const string SATELLITE_FILE_NAME =
            "HANOI_SATELLITE_2M_32648.tif";

        // File cũ giữ làm dự phòng để project vẫn chạy nếu
        // chưa chép file 2 m vào thư mục Basemaps.
        private const string SATELLITE_FALLBACK_FILE_NAME =
            "HANOI_SATELLITE_32648.tif";

        private const string TRAFFIC_FILE_NAME =
            "HANOI_ROADS_32648.tif";

        private const string TACTICAL_FILE_NAME =
            "HANOI_TACTICAL_32648.tif";


        // =========================================================
        // ENTRY POINT CŨ - GIỮ ĐỂ TƯƠNG THÍCH
        // =========================================================
        private void InitOpenStreetMap()
        {
            InitHanoiOnlyMap();
        }


        // =========================================================
        // KHỞI TẠO MAP HÀ NỘI
        // =========================================================
        private void InitHanoiOnlyMap()
        {
            if (_tilesConfigured ||
                axMap2 == null ||
                IsDisposed ||
                Disposing)
            {
                return;
            }

            try
            {
                CrashLogger.Write(
                    "InitHanoiOnlyMap EPSG:" +
                    HANOI_MAP_SRID +
                    ": begin");

                _tilesConfigured = true;
                _tilesShuttingDown = false;

                // -------------------------------------------------
                // 1. TẮT TOÀN BỘ TILE ONLINE
                // -------------------------------------------------
                DisableOnlineTiles();

                // -------------------------------------------------
                // 2. MAP CRS = EPSG:32648
                //
                // Hàm này đang nằm trong Navigation.cs của project.
                // -------------------------------------------------
                EnsureLocalMapProjection32648();

                // -------------------------------------------------
                // 3. CẤU HÌNH TƯƠNG TÁC MAP
                // -------------------------------------------------
                axMap2.CursorMode =
                    tkCursorMode.cmPan;

                axMap2.ShowCoordinates =
                    tkCoordinatesDisplay.cdmMapUnits;

                axMap2.ScalebarVisible =
                    true;

                axMap2.ScalebarUnits =
                    tkScalebarUnits.Metric;

                axMap2.MapUnits =
                    tkUnitsOfMeasure.umMeters;

                try
                {
                    axMap2.InertiaOnPanning =
                        tkCustomState.csFalse;
                }
                catch
                {
                }

                try
                {
                    axMap2.AnimationOnZooming =
                        tkCustomState.csFalse;
                }
                catch
                {
                }

                try
                {
                    axMap2.UseSeamlessPan =
                        false;
                }
                catch
                {
                }

                try
                {
                    axMap2.SendMouseMove =
                        false;
                }
                catch
                {
                }

                try
                {
                    axMap2.DisableWaitCursor =
                        true;
                }
                catch
                {
                }

                // -------------------------------------------------
                // 4. NẠP BASEMAP VỆ TINH NGAY KHI KHỞI ĐỘNG
                //
                // Đây là phần còn thiếu ở bản Map.cs trước:
                // trước đây mới chỉ có ShowSatelliteBasemap(),
                // nhưng chưa gọi nó trong InitHanoiOnlyMap().
                // -------------------------------------------------
                bool satelliteLoaded =
                    LoadSatelliteBasemap(true);

                // -------------------------------------------------
                // 5. FALLBACK NẾU KHÔNG NẠP ĐƯỢC RASTER
                // -------------------------------------------------
                if (!satelliteLoaded)
                {
                    try
                    {
                        if (_loadedLayers == null ||
                            _loadedLayers.Count == 0)
                        {
                            ZoomToHanoiReferenceExtent();
                        }
                    }
                    catch
                    {
                    }
                }

                // -------------------------------------------------
                // 6. VẼ LẠI MAP
                // -------------------------------------------------
                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                // -------------------------------------------------
                // 7. TRẠNG THÁI
                // -------------------------------------------------
                if (lblStatus != null)
                {
                    if (satelliteLoaded)
                    {
                        lblStatus.Text =
                            "Vệ tinh Hà Nội | EPSG:" +
                            HANOI_MAP_SRID +
                            " | Sẵn sàng";
                    }
                    else
                    {
                        lblStatus.Text =
                            "Bản đồ Hà Nội | EPSG:" +
                            HANOI_MAP_SRID +
                            " | Chưa nạp được nền vệ tinh";
                    }
                }

                if (lblStatusProjection != null)
                {
                    lblStatusProjection.Text =
                        "CRS: EPSG:" +
                        HANOI_MAP_SRID +
                        " | mét";
                }

                CrashLogger.Write(
                    "InitHanoiOnlyMap EPSG:" +
                    HANOI_MAP_SRID +
                    ": OK | Satellite=" +
                    satelliteLoaded);
            }
            catch (Exception ex)
            {
                _tilesConfigured = false;

                CrashLogger.Write(
                    "InitHanoiOnlyMap ERROR: " +
                    ex);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không khởi tạo được bản đồ Hà Nội.";
                }

                MessageBox.Show(
                    "Không khởi tạo được bản đồ Hà Nội EPSG:" +
                    HANOI_MAP_SRID +
                    ".\n\n" +
                    ex.Message,
                    "Lỗi bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }


        // =========================================================
        // TÌM FILE BASEMAP
        //
        // Hỗ trợ các tình huống chạy:
        //   Project\bin\Debug
        //   Project\bin\Release
        //   Basemaps nằm cạnh EXE
        //
        // Cấu trúc hiện tại:
        //
        // MapWinGIS_TOI\
        // ├── Basemaps\
        // │   ├── HANOI_SATELLITE_2M_32648.tif
        // │   └── HANOI_SATELLITE_32648.tif   (dự phòng)
        // ├── Map.cs
        // ├── Form1.cs
        // └── bin\
        //     └── Debug\
        // =========================================================
        private string FindBasemapFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string startup =
                Application.StartupPath;

            string[] candidates =
            {
                // Basemaps nằm cạnh file EXE
                Path.Combine(
                    startup,
                    "Basemaps",
                    fileName),

                // EXE có thể nằm sâu 1 cấp
                Path.GetFullPath(
                    Path.Combine(
                        startup,
                        @"..\Basemaps",
                        fileName)),

                // Trường hợp chuẩn:
                // Project\bin\Debug -> Project\Basemaps
                Path.GetFullPath(
                    Path.Combine(
                        startup,
                        @"..\..\Basemaps",
                        fileName)),

                // Dự phòng nếu cấu trúc output sâu hơn
                Path.GetFullPath(
                    Path.Combine(
                        startup,
                        @"..\..\..\Basemaps",
                        fileName))
            };

            foreach (string path in candidates)
            {
                try
                {
                    CrashLogger.Write(
                        "FindBasemapFile checking: " +
                        path);

                    if (File.Exists(path))
                    {
                        CrashLogger.Write(
                            "FindBasemapFile FOUND: " +
                            path);

                        return path;
                    }
                }
                catch (Exception ex)
                {
                    CrashLogger.Write(
                        "FindBasemapFile candidate ERROR: " +
                        ex.Message);
                }
            }

            CrashLogger.Write(
                "FindBasemapFile NOT FOUND: " +
                fileName);

            return null;
        }


        // =========================================================
        // XÓA BASEMAP HIỆN TẠI
        // =========================================================
        private void RemoveCurrentBasemap()
        {
            if (axMap2 == null)
            {
                return;
            }

            int oldHandle =
                _basemapLayerHandle;

            _basemapLayerHandle = -1;

            try
            {
                if (oldHandle >= 0)
                {
                    axMap2.RemoveLayer(
                        oldHandle);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "RemoveCurrentBasemap RemoveLayer ERROR: " +
                    ex.Message);
            }

            try
            {
                if (_basemapImage != null)
                {
                    _basemapImage.Close();
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "RemoveCurrentBasemap Image.Close ERROR: " +
                    ex.Message);
            }
            finally
            {
                _basemapImage = null;
            }
        }


        // =========================================================
        // NẠP BASEMAP GEOTIFF - OVERLOAD MẶC ĐỊNH
        //
        // Không thay đổi mức zoom khi người dùng chuyển basemap.
        // =========================================================
        private bool LoadLocalBasemap(
            string fileName,
            string displayName)
        {
            return LoadLocalBasemap(
                fileName,
                displayName,
                false);
        }


        // =========================================================
        // NẠP BASEMAP GEOTIFF
        //
        // zoomToBasemap = true:
        //   dùng khi khởi động để bảo đảm raster hiện ngay.
        //
        // zoomToBasemap = false:
        //   dùng khi đổi nền để giữ nguyên vùng đang xem.
        // =========================================================
        private bool LoadLocalBasemap(
            string fileName,
            string displayName,
            bool zoomToBasemap)
        {
            if (axMap2 == null)
            {
                return false;
            }

            string path =
                FindBasemapFile(fileName);

            if (string.IsNullOrWhiteSpace(path) ||
                !File.Exists(path))
            {
                CrashLogger.Write(
                    "LoadLocalBasemap FILE NOT FOUND: " +
                    fileName);

                MessageBox.Show(
                    "Không tìm thấy bản đồ nền:\n\n" +
                    fileName +
                    "\n\n" +
                    "Thư mục cần kiểm tra:\n" +
                    "Basemaps",
                    "Thiếu bản đồ nền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return false;
            }

            try
            {
                CrashLogger.Write(
                    "LoadLocalBasemap begin: " +
                    path);

                // ---------------------------------------------
                // XÓA BASEMAP CŨ TRƯỚC KHI THÊM BASEMAP MỚI
                // ---------------------------------------------
                RemoveCurrentBasemap();

                MapWinGIS.Image image =
                    new MapWinGIS.Image();

                // false = không nạp toàn bộ raster 271 MB vào RAM.
                // MapWinGIS đọc trực tiếp từ file.
                bool opened =
                    image.Open(
                        path,
                        ImageType.USE_FILE_EXTENSION,
                        false,
                        null);

                if (!opened)
                {
                    string errorText = "";

                    try
                    {
                        errorText =
                            image.get_ErrorMsg(
                                image.LastErrorCode);
                    }
                    catch
                    {
                    }

                    CrashLogger.Write(
                        "LoadLocalBasemap Image.Open FAILED | " +
                        path +
                        " | " +
                        errorText);

                    try
                    {
                        image.Close();
                    }
                    catch
                    {
                    }

                    MessageBox.Show(
                        "MapWinGIS không mở được file GeoTIFF:\n\n" +
                        path +
                        "\n\n" +
                        errorText,
                        "Lỗi bản đồ nền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
                }

                CrashLogger.Write(
                    "LoadLocalBasemap Image.Open OK: " +
                    path);

                // ---------------------------------------------
                // THÊM RASTER VÀO MAP
                // ---------------------------------------------
                int handle =
                    axMap2.AddLayer(
                        image,
                        true);

                if (handle < 0)
                {
                    string mapError = "";

                    try
                    {
                        mapError =
                            axMap2.get_ErrorMsg(
                                axMap2.LastErrorCode);
                    }
                    catch
                    {
                    }

                    try
                    {
                        image.Close();
                    }
                    catch
                    {
                    }

                    CrashLogger.Write(
                        "LoadLocalBasemap AddLayer FAILED | " +
                        mapError);

                    MessageBox.Show(
                        "Không thể thêm bản đồ nền vào MapWinGIS.\n\n" +
                        mapError,
                        "Lỗi bản đồ nền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
                }

                _basemapImage =
                    image;

                _basemapLayerHandle =
                    handle;

                // ---------------------------------------------
                // ĐẶT TÊN LỚP
                // ---------------------------------------------
                try
                {
                    axMap2.set_LayerName(
                        handle,
                        displayName);
                }
                catch
                {
                }

                // ---------------------------------------------
                // ĐƯA BASEMAP XUỐNG DƯỚI CÙNG
                // để các lớp PostGIS nằm phía trên.
                // ---------------------------------------------
                try
                {
                    axMap2.MoveLayerBottom(
                        handle);
                }
                catch (Exception ex)
                {
                    CrashLogger.Write(
                        "MoveLayerBottom WARNING: " +
                        ex.Message);
                }

                // ---------------------------------------------
                // KHI KHỞI ĐỘNG:
                // zoom thẳng tới raster vừa thêm để tránh
                // trường hợp map trắng do extent hiện tại nằm
                // ngoài vùng Hà Nội.
                // ---------------------------------------------
                if (zoomToBasemap)
                {
                    try
                    {
                        axMap2.ZoomToLayer(
                            handle);

                        CrashLogger.Write(
                            "LoadLocalBasemap ZoomToLayer OK");
                    }
                    catch (Exception ex)
                    {
                        CrashLogger.Write(
                            "LoadLocalBasemap ZoomToLayer ERROR: " +
                            ex.Message);

                        try
                        {
                            ZoomToHanoiReferenceExtent();
                        }
                        catch
                        {
                        }
                    }
                }

                // ---------------------------------------------
                // VẼ LẠI TOÀN BỘ MAP
                // ---------------------------------------------
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
                        displayName +
                        " | EPSG:" +
                        HANOI_MAP_SRID;
                }

                CrashLogger.Write(
                    "LoadLocalBasemap OK | handle=" +
                    handle +
                    " | " +
                    path);

                return true;
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "LoadLocalBasemap ERROR: " +
                    ex);

                MessageBox.Show(
                    "Không thể nạp bản đồ nền.\n\n" +
                    ex.Message,
                    "Lỗi bản đồ nền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }
        }


        // =========================================================
        // NẠP BASEMAP VỆ TINH
        //
        // Ưu tiên ảnh 2 m/pixel. Nếu không tìm thấy thì tự động
        // dùng lại file vệ tinh cũ để ứng dụng không bị trắng map.
        // =========================================================
        private bool LoadSatelliteBasemap(bool zoomToBasemap)
        {
            string highResolutionPath =
                FindBasemapFile(SATELLITE_FILE_NAME);

            if (!string.IsNullOrWhiteSpace(highResolutionPath) &&
                File.Exists(highResolutionPath))
            {
                return LoadLocalBasemap(
                    SATELLITE_FILE_NAME,
                    "Vệ tinh Hà Nội 2 m",
                    zoomToBasemap);
            }

            string fallbackPath =
                FindBasemapFile(SATELLITE_FALLBACK_FILE_NAME);

            if (!string.IsNullOrWhiteSpace(fallbackPath) &&
                File.Exists(fallbackPath))
            {
                CrashLogger.Write(
                    "Không tìm thấy ảnh vệ tinh 2 m; dùng file dự phòng: " +
                    fallbackPath);

                return LoadLocalBasemap(
                    SATELLITE_FALLBACK_FILE_NAME,
                    "Vệ tinh Hà Nội",
                    zoomToBasemap);
            }

            MessageBox.Show(
                "Không tìm thấy bản đồ nền vệ tinh.\n\n" +
                "Ưu tiên:\n" +
                SATELLITE_FILE_NAME +
                "\n\nDự phòng:\n" +
                SATELLITE_FALLBACK_FILE_NAME +
                "\n\nHãy kiểm tra thư mục Basemaps.",
                "Thiếu bản đồ nền",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return false;
        }


        // =========================================================
        // BASEMAP: VỆ TINH
        // =========================================================
        private void ShowSatelliteBasemap()
        {
            LoadSatelliteBasemap(false);
        }


        // =========================================================
        // BASEMAP: GIAO THÔNG
        //
        // File HANOI_ROADS_32648.tif sẽ tạo ở bước tiếp theo.
        // =========================================================
        private void ShowTrafficBasemap()
        {
            LoadLocalBasemap(
                TRAFFIC_FILE_NAME,
                "Giao thông Hà Nội",
                false);
        }


        // =========================================================
        // BASEMAP: TÁC CHIẾN
        //
        // File HANOI_TACTICAL_32648.tif sẽ tạo sau.
        // =========================================================
        private void ShowTacticalBasemap()
        {
            LoadLocalBasemap(
                TACTICAL_FILE_NAME,
                "Tác chiến Hà Nội",
                false);
        }


        // =========================================================
        // TẮT BASEMAP
        // =========================================================
        private void HideBasemap()
        {
            RemoveCurrentBasemap();

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
                    "Bản đồ Hà Nội | Không có nền";
            }
        }


        // =========================================================
        // TẮT TILE / ONLINE HOÀN TOÀN
        // =========================================================
        private void DisableOnlineTiles()
        {
            if (axMap2 == null)
            {
                return;
            }

            try
            {
                axMap2.Tiles.Visible =
                    false;

                axMap2.Tiles.UseServer =
                    false;
            }
            catch
            {
            }

            try
            {
                axMap2.Tiles.set_UseCache(
                    tkCacheType.RAM,
                    false);

                axMap2.Tiles.set_DoCaching(
                    tkCacheType.RAM,
                    false);
            }
            catch
            {
            }

            try
            {
                axMap2.Tiles.set_UseCache(
                    tkCacheType.Disk,
                    false);

                axMap2.Tiles.set_DoCaching(
                    tkCacheType.Disk,
                    false);
            }
            catch
            {
            }
        }


        // =========================================================
        // DỪNG MAP AN TOÀN
        //
        // Giữ tên cũ để Form1.cs và các partial class khác
        // không bị lỗi.
        // =========================================================
        private void StopOpenStreetMapSafely()
        {
            if (_tilesShuttingDown)
            {
                return;
            }

            _tilesShuttingDown = true;

            if (axMap2 == null)
            {
                return;
            }

            try
            {
                axMap2.SendMouseMove =
                    false;
            }
            catch
            {
            }

            DisableOnlineTiles();

            RemoveCurrentBasemap();

            try
            {
                axMap2.ShowCoordinates =
                    tkCoordinatesDisplay.cdmNone;
            }
            catch
            {
            }

            CrashLogger.Write(
                "StopHanoiOnlyMap: completed");
        }
    }
}
