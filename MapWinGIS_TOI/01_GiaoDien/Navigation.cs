using System;
using System.IO;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // NAVIGATION.CS - HÀ NỘI ONLY / EPSG:32648
    //
    // Chức năng:
    // - Mở Shapefile cục bộ;
    // - Chuẩn hóa dữ liệu về EPSG:32648 trước khi đưa lên map;
    // - Pan / Zoom In / Zoom Out;
    // - Full Extent theo dữ liệu đang mở;
    // - Nếu chưa có lớp nào thì quay về phạm vi Hà Nội.
    //
    // Toàn hệ thống hiện dùng EPSG:32648.
    // Không dùng nền tile trực tuyến.
    // =============================================================
    public partial class Form1
    {
        // =========================================================
        // CRS CHO SHAPEFILE CỤC BỘ
        //
        // Nếu Shapefile không có CRS, ứng dụng tạm coi dữ liệu nguồn
        // là EPSG:32648 theo quy ước dữ liệu của đề tài.
        // =========================================================
        private const int LOCAL_DEFAULT_SOURCE_EPSG =
            32648;

        private const int LOCAL_MAP_EPSG =
            32648;

        // =========================================================
        // PHẠM VI THAM CHIẾU HÀ NỘI TRONG EPSG:32648
        //
        // Chỉ dùng làm khung nhìn mặc định khi chưa có layer.
        // Không dùng làm dữ liệu nghiệp vụ.
        // =========================================================
        private const double HANOI_XMIN =
            529000.0;

        private const double HANOI_YMIN =
            2273000.0;

        private const double HANOI_XMAX =
            606500.0;

        private const double HANOI_YMAX =
            2366000.0;

        // =========================================================
        // KHỞI TẠO CÔNG CỤ ĐIỀU HƯỚNG
        // =========================================================
        private void InitNavigationTools()
        {
            if (btnOpenLayer != null)
            {
                btnOpenLayer.Click -=
                    btnOpenLayer_Click;

                btnOpenLayer.Click +=
                    btnOpenLayer_Click;
            }

            if (btnPan != null)
            {
                btnPan.Click -=
                    btnPan_Click;

                btnPan.Click +=
                    btnPan_Click;
            }

            if (btnZoomIn != null)
            {
                btnZoomIn.Click -=
                    btnZoomIn_Click;

                btnZoomIn.Click +=
                    btnZoomIn_Click;
            }

            if (btnZoomOut != null)
            {
                btnZoomOut.Click -=
                    btnZoomOut_Click;

                btnZoomOut.Click +=
                    btnZoomOut_Click;
            }

            if (btnFullExtent != null)
            {
                btnFullExtent.Click -=
                    btnFullExtent_Click;

                btnFullExtent.Click +=
                    btnFullExtent_Click;
            }
        }

        // =========================================================
        // MỞ SHAPEFILE CỤC BỘ
        //
        // Luồng:
        // 1. Đảm bảo map là EPSG:32648;
        // 2. Mở Shapefile;
        // 3. Đọc CRS nguồn;
        // 4. Nếu thiếu CRS -> tạm gán EPSG:32648;
        // 5. Nếu CRS khác EPSG:32648 -> Reproject;
        // 6. Add layer đã chuẩn hóa lên map.
        // =========================================================
        private void btnOpenLayer_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dlg =
                new OpenFileDialog())
            {
                dlg.Filter =
                    "Shapefile (*.shp)|*.shp";

                dlg.Title =
                    "Chọn Shapefile - dữ liệu sẽ hiển thị ở EPSG:32648";

                dlg.Multiselect =
                    false;

                if (dlg.ShowDialog(
                    this) !=
                    DialogResult.OK)
                {
                    return;
                }

                if (!File.Exists(
                    dlg.FileName))
                {
                    MessageBox.Show(
                        "File không tồn tại.",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                Shapefile sourceSf =
                    null;

                Shapefile layerToAdd =
                    null;

                bool layerAdded =
                    false;

                bool projectionWasMissing =
                    false;

                try
                {
                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Đang mở lớp: " +
                            Path.GetFileName(
                                dlg.FileName) +
                            "...";
                    }

                    // ---------------------------------------------
                    // 1. MAP = EPSG:32648
                    // ---------------------------------------------
                    EnsureLocalMapProjection32648();

                    // ---------------------------------------------
                    // 2. MỞ SHAPEFILE
                    // ---------------------------------------------
                    sourceSf =
                        new Shapefile();

                    if (!sourceSf.Open(
                        dlg.FileName,
                        null))
                    {
                        MessageBox.Show(
                            "Không mở được file:\n\n" +
                            sourceSf.get_ErrorMsg(
                                sourceSf.LastErrorCode),
                            "Lỗi mở lớp",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    // ---------------------------------------------
                    // 3. KIỂM TRA CRS NGUỒN TRƯỚC KHI GÁN MẶC ĐỊNH
                    // ---------------------------------------------
                    projectionWasMissing =
                        !HasSourceProjection(
                            sourceSf);

                    // ---------------------------------------------
                    // 4. NẾU THIẾU CRS -> TẠM GÁN 32648
                    // ---------------------------------------------
                    if (!EnsureSourceProjection(
                        sourceSf))
                    {
                        MessageBox.Show(
                            "Không xác định được hệ tọa độ của Shapefile.\n\n" +
                            "File: " +
                            dlg.FileName +
                            "\n\nKhông thể gán EPSG:" +
                            LOCAL_DEFAULT_SOURCE_EPSG +
                            " cho dữ liệu.",
                            "Lỗi hệ tọa độ",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    // ---------------------------------------------
                    // 5. PROJECTION ĐÍCH EPSG:32648
                    // ---------------------------------------------
                    GeoProjection targetProjection =
                        new GeoProjection();

                    if (!targetProjection.ImportFromEPSG(
                        LOCAL_MAP_EPSG))
                    {
                        MessageBox.Show(
                            "Không khởi tạo được EPSG:" +
                            LOCAL_MAP_EPSG +
                            ".",
                            "Lỗi hệ tọa độ",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    layerToAdd =
                        sourceSf;

                    bool sameProjection =
                        false;

                    try
                    {
                        sameProjection =
                            sourceSf.GeoProjection
                                .get_IsSame(
                                    targetProjection);
                    }
                    catch
                    {
                        sameProjection =
                            false;
                    }

                    // ---------------------------------------------
                    // 6. CRS KHÁC 32648 -> REPROJECT THẬT
                    // ---------------------------------------------
                    if (!sameProjection)
                    {
                        int reprojectedCount =
                            0;

                        Shapefile reprojected =
                            sourceSf.Reproject(
                                targetProjection,
                                ref reprojectedCount);

                        if (reprojected == null)
                        {
                            MessageBox.Show(
                                "Không thể chuyển Shapefile sang EPSG:" +
                                LOCAL_MAP_EPSG +
                                ".\n\n" +
                                "Hãy kiểm tra CRS thực của dữ liệu.\n\n" +
                                "MapWinGIS:\n" +
                                sourceSf.get_ErrorMsg(
                                    sourceSf.LastErrorCode),
                                "Lỗi chuyển hệ tọa độ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                            return;
                        }

                        if (sourceSf.NumShapes > 0 &&
                            reprojectedCount <= 0)
                        {
                            try
                            {
                                reprojected.Close();
                            }
                            catch
                            {
                            }

                            MessageBox.Show(
                                "Không có đối tượng nào được chuyển hệ tọa độ.\n\n" +
                                "Hãy kiểm tra CRS thực của Shapefile.",
                                "Lỗi chuyển hệ tọa độ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                            return;
                        }

                        layerToAdd =
                            reprojected;

                        try
                        {
                            layerToAdd.GeoProjection
                                .ImportFromEPSG(
                                    LOCAL_MAP_EPSG);
                        }
                        catch
                        {
                        }
                    }

                    // ---------------------------------------------
                    // 7. ADD LAYER
                    //
                    // Geometry đã được chuẩn hóa về 32648 nên không
                    // cho MapWinGIS tự reproject lần hai.
                    // ---------------------------------------------
                    bool oldReproject =
                        _globalSettings
                            .ReprojectLayersOnAdding;

                    bool oldAllowMismatch =
                        _globalSettings
                            .AllowProjectionMismatch;

                    bool oldAllowNoProjection =
                        _globalSettings
                            .AllowLayersWithoutProjections;

                    int handle =
                        -1;

                    try
                    {
                        _globalSettings
                            .ReprojectLayersOnAdding =
                            false;

                        _globalSettings
                            .AllowProjectionMismatch =
                            false;

                        _globalSettings
                            .AllowLayersWithoutProjections =
                            false;

                        handle =
                            axMap2.AddLayer(
                                layerToAdd,
                                true);
                    }
                    finally
                    {
                        _globalSettings
                            .ReprojectLayersOnAdding =
                            oldReproject;

                        _globalSettings
                            .AllowProjectionMismatch =
                            oldAllowMismatch;

                        _globalSettings
                            .AllowLayersWithoutProjections =
                            oldAllowNoProjection;
                    }

                    if (handle < 0)
                    {
                        string mapError =
                            "";

                        try
                        {
                            mapError =
                                axMap2.get_ErrorMsg(
                                    axMap2.LastErrorCode);
                        }
                        catch
                        {
                        }

                        MessageBox.Show(
                            "Không thêm được lớp lên bản đồ.\n\n" +
                            "Map CRS: EPSG:" +
                            LOCAL_MAP_EPSG +
                            "\n\nMapWinGIS:\n" +
                            mapError,
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    layerAdded =
                        true;

                    // ---------------------------------------------
                    // 8. TÊN / KEY KHÔNG TRÙNG
                    // ---------------------------------------------
                    string baseName =
                        Path.GetFileNameWithoutExtension(
                            dlg.FileName);

                    string key =
                        baseName;

                    int suffix =
                        1;

                    while (_loadedLayers.ContainsKey(
                        key))
                    {
                        key =
                            baseName +
                            "_" +
                            suffix;

                        suffix++;
                    }

                    axMap2.set_LayerName(
                        handle,
                        key);

                    _loadedLayers[key] =
                        handle;

                    // ---------------------------------------------
                    // 9. TABLE OF CONTENTS
                    // ---------------------------------------------
                    TreeNode node =
                        new TreeNode(
                            key);

                    node.Name =
                        key;

                    node.Text =
                        key;

                    node.Tag =
                        "FILE";

                    treeLayers.Nodes.Add(
                        node);

                    SetLayerNodeChecked(
                        node,
                        true);

                    treeLayers.SelectedNode =
                        node;

                    RefreshLayerCombo();
                    UpdateCurrentLayerStatus();

                    // ---------------------------------------------
                    // 10. ZOOM
                    // ---------------------------------------------
                    try
                    {
                        axMap2.ZoomToLayer(
                            handle);

                        axMap2.Redraw();
                        SyncVerticalZoomSlider();
                    }
                    catch
                    {
                    }

                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Đã mở lớp: " +
                            key +
                            " | EPSG:" +
                            LOCAL_MAP_EPSG +
                            (
                                projectionWasMissing
                                    ? " | CRS nguồn bị thiếu, tạm giả định EPSG:32648."
                                    : ""
                            );
                    }

                    // ---------------------------------------------
                    // 11. NẾU ĐÃ TẠO BẢN REPROJECT THÌ CÓ THỂ
                    //     ĐÓNG FILE NGUỒN.
                    // ---------------------------------------------
                    if (!ReferenceEquals(
                        layerToAdd,
                        sourceSf))
                    {
                        try
                        {
                            sourceSf.Close();
                        }
                        catch
                        {
                        }

                        sourceSf =
                            null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Có lỗi khi thêm Shapefile.\n\n" +
                        ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    // Nếu add thất bại, giải phóng các đối tượng
                    // Shapefile đã mở/tạo.
                    if (!layerAdded)
                    {
                        if (layerToAdd != null &&
                            !ReferenceEquals(
                                layerToAdd,
                                sourceSf))
                        {
                            try
                            {
                                layerToAdd.Close();
                            }
                            catch
                            {
                            }
                        }

                        if (sourceSf != null)
                        {
                            try
                            {
                                sourceSf.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        // =========================================================
        // ĐẢM BẢO MAP LÀ EPSG:32648
        //
        // Với projection tùy ý, dùng AxMap.GeoProjection thay vì
        // enum AxMap.Projection.
        // =========================================================
        private void EnsureLocalMapProjection32648()
        {
            if (axMap2 == null)
            {
                return;
            }

            GeoProjection targetProjection =
                new GeoProjection();

            if (!targetProjection.ImportFromEPSG(
                LOCAL_MAP_EPSG))
            {
                throw new InvalidOperationException(
                    "Không thể khởi tạo EPSG:" +
                    LOCAL_MAP_EPSG +
                    " cho bản đồ.");
            }

            axMap2.GrabProjectionFromData =
                false;

            axMap2.GeoProjection =
                targetProjection;

            axMap2.MapUnits =
                tkUnitsOfMeasure.umMeters;
        }

        // =========================================================
        // SHAPEFILE ĐÃ CÓ CRS HAY CHƯA
        // =========================================================
        private bool HasSourceProjection(
            Shapefile sf)
        {
            if (sf == null ||
                sf.GeoProjection == null)
            {
                return false;
            }

            try
            {
                string proj4 =
                    sf.GeoProjection
                        .ExportToProj4();

                return
                    !string.IsNullOrWhiteSpace(
                        proj4);
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // ĐẢM BẢO SHAPEFILE CÓ CRS
        //
        // Nếu .prj bị thiếu / không đọc được thì tạm gán EPSG:32648
        // theo quy ước dữ liệu cục bộ của đề tài.
        // =========================================================
        private bool EnsureSourceProjection(
            Shapefile sf)
        {
            if (sf == null ||
                sf.GeoProjection == null)
            {
                return false;
            }

            if (HasSourceProjection(
                sf))
            {
                return true;
            }

            try
            {
                return
                    sf.GeoProjection
                        .ImportFromEPSG(
                            LOCAL_DEFAULT_SOURCE_EPSG);
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // CHUẨN BỊ CHẾ ĐỘ ĐIỀU HƯỚNG
        //
        // - Không cho đổi chế độ giữa lúc đang số hóa tuyến/khu vực;
        // - Tắt Identify của các lớp để tránh click điều hướng vẫn
        //   tiếp tục chọn/highlight đối tượng;
        // - Bỏ selection cũ;
        // - Trả con trỏ về mặc định và đưa focus vào map.
        // =========================================================
        private bool PrepareNavigationMode()
        {
            if (axMap2 == null)
            {
                return false;
            }

            if (_isDigitizing)
            {
                MessageBox.Show(
                    "Anh đang ở chế độ vẽ đối tượng.\n\n" +
                    "Hãy click phải để hoàn tất và lưu hoặc nhấn Esc để hủy " +
                    "trước khi chuyển sang công cụ điều hướng.",
                    "Đang vẽ đối tượng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return false;
            }

            foreach (var kv
                in _loadedLayers)
            {
                try
                {
                    Shapefile sf =
                        axMap2.get_Shapefile(
                            kv.Value);

                    if (sf == null)
                    {
                        continue;
                    }

                    sf.Identifiable =
                        false;

                    sf.SelectNone();
                }
                catch
                {
                }
            }

            try
            {
                axMap2.MapCursor =
                    tkCursor.crsrMapDefault;

                axMap2.Focus();
                axMap2.Redraw();
            }
            catch
            {
            }

            return true;
        }

        // =========================================================
        // PAN
        //
        // Chọn công cụ rồi giữ chuột trái và kéo bản đồ.
        // =========================================================
        private void btnPan_Click(
            object sender,
            EventArgs e)
        {
            if (!PrepareNavigationMode())
            {
                return;
            }

            try
            {
                axMap2.CursorMode =
                    tkCursorMode.cmPan;

                axMap2.Focus();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Di chuyển bản đồ: giữ chuột trái và kéo.";
                }
            }
            catch (Exception ex)
            {
                ShowNavigationError(
                    "Không bật được chế độ di chuyển bản đồ.",
                    ex);
            }
        }

        // =========================================================
        // ZOOM IN
        //
        // Bấm nút/menu:
        //     phóng to ngay 25%.
        //
        // Sau đó vẫn giữ cmZoomIn để người dùng có thể tiếp tục
        // click hoặc kéo khung trên bản đồ.
        // =========================================================
        private void btnZoomIn_Click(
            object sender,
            EventArgs e)
        {
            if (!PrepareNavigationMode())
            {
                return;
            }

            try
            {
                // MapWinGIS: percent phải nằm trong khoảng 0..1.
                axMap2.ZoomIn(
                    0.25);

                axMap2.CursorMode =
                    tkCursorMode.cmZoomIn;

                axMap2.Focus();
                axMap2.Redraw();
                SyncVerticalZoomSlider();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã phóng to 25% | Click hoặc kéo khung trên bản đồ để phóng to tiếp.";
                }
            }
            catch (Exception ex)
            {
                ShowNavigationError(
                    "Không phóng to được bản đồ.",
                    ex);
            }
        }

        // =========================================================
        // ZOOM OUT
        //
        // Bấm nút/menu:
        //     thu nhỏ ngay 25%.
        //
        // Sau đó vẫn giữ cmZoomOut để có thể click tiếp trên map.
        // =========================================================
        private void btnZoomOut_Click(
            object sender,
            EventArgs e)
        {
            if (!PrepareNavigationMode())
            {
                return;
            }

            try
            {
                axMap2.ZoomOut(
                    0.25);

                axMap2.CursorMode =
                    tkCursorMode.cmZoomOut;

                axMap2.Focus();
                axMap2.Redraw();
                SyncVerticalZoomSlider();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã thu nhỏ 25% | Click trên bản đồ để thu nhỏ tiếp.";
                }
            }
            catch (Exception ex)
            {
                ShowNavigationError(
                    "Không thu nhỏ được bản đồ.",
                    ex);
            }
        }

        // =========================================================
        // FULL EXTENT
        //
        // - Có ít nhất một layer đang hiển thị:
        //      zoom toàn bộ các layer visible;
        // - Không có layer visible:
        //      quay về phạm vi Hà Nội.
        // =========================================================
        private void btnFullExtent_Click(
            object sender,
            EventArgs e)
        {
            if (!PrepareNavigationMode())
            {
                return;
            }

            try
            {
                EnsureLocalMapProjection32648();

                int visibleLayerCount =
                    GetVisibleLayerCount();

                if (visibleLayerCount > 0)
                {
                    axMap2.ZoomToMaxVisibleExtents();
                }
                else
                {
                    ZoomToHanoiReferenceExtent();
                }

                axMap2.CursorMode =
                    tkCursorMode.cmPan;

                axMap2.Focus();
                axMap2.Redraw();
                SyncVerticalZoomSlider();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        visibleLayerCount > 0
                            ? "Đã hiển thị toàn bộ phạm vi của các lớp đang bật."
                            : "Không có lớp đang bật | Đã quay về phạm vi Hà Nội.";
                }
            }
            catch (Exception ex)
            {
                ShowNavigationError(
                    "Không thể zoom về toàn bộ dữ liệu.",
                    ex);
            }
        }

        // =========================================================
        // ĐẾM LỚP ĐANG HIỂN THỊ
        // =========================================================
        private int GetVisibleLayerCount()
        {
            if (axMap2 == null)
            {
                return 0;
            }

            int count =
                0;

            foreach (var kv
                in _loadedLayers)
            {
                try
                {
                    if (axMap2.get_LayerVisible(
                        kv.Value))
                    {
                        count++;
                    }
                }
                catch
                {
                }
            }

            return count;
        }

        // =========================================================
        // THÔNG BÁO LỖI ĐIỀU HƯỚNG
        // =========================================================
        private void ShowNavigationError(
            string message,
            Exception ex)
        {
            MessageBox.Show(
                message +
                "\n\n" +
                (ex == null
                    ? ""
                    : ex.Message),
                "Lỗi điều hướng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Công cụ điều hướng gặp lỗi.";
            }
        }

        // =========================================================
        // PHẠM VI HÀ NỘI KHI CHƯA CÓ LỚP
        // =========================================================
        private void ZoomToHanoiReferenceExtent()
        {
            if (axMap2 == null)
            {
                return;
            }

            Extents extents =
                new Extents();

            extents.SetBounds(
                HANOI_XMIN,
                HANOI_YMIN,
                0.0,
                HANOI_XMAX,
                HANOI_YMAX,
                0.0);

            axMap2.Extents =
                extents;

            SyncVerticalZoomSlider();
        }
    }
}