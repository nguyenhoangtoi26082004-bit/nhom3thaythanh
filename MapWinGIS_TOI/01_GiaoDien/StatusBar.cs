using System;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // STATUSBAR.CS - HÀ NỘI ONLY / EPSG:32648
    //
    // Mục tiêu:
    // - Không dùng MouseMoveEvent của ActiveX;
    // - Không gọi PixelToProj liên tục khi rê chuột;
    // - Dùng chức năng hiển thị tọa độ native của MapWinGIS;
    // - Hiển thị tọa độ theo Map Units của EPSG:32648;
    // - Map Units = mét;
    // - Hiển thị tên lớp thân thiện trên StatusStrip.
    // =============================================================
    public partial class Form1
    {
        private bool _statusBarInitialized =
            false;

        // =========================================================
        // KHỞI TẠO STATUS BAR
        // =========================================================
        private void InitStatusBar()
        {
            if (_statusBarInitialized)
            {
                return;
            }

            if (axMap2 == null)
            {
                return;
            }

            try
            {
                // ---------------------------------------------
                // Không phát MouseMoveEvent liên tục.
                // Cách này nhẹ và ổn định hơn với ActiveX.
                // ---------------------------------------------
                axMap2.SendMouseMove =
                    false;

                // ---------------------------------------------
                // Toàn hệ thống dùng EPSG:32648.
                // Đơn vị tọa độ là mét.
                // ---------------------------------------------
                axMap2.MapUnits =
                    tkUnitsOfMeasure.umMeters;

                // ---------------------------------------------
                // Hiển thị trực tiếp X/Y của hệ tọa độ bản đồ.
                //
                // Không dùng cdmDegrees vì 32648 là CRS chiếu.
                // ---------------------------------------------
                axMap2.ShowCoordinates =
                    tkCoordinatesDisplay.cdmMapUnits;

                // Nền trắng giúp tọa độ dễ đọc hơn.
                try
                {
                    axMap2.ShowCoordinatesBackground =
                        true;
                }
                catch
                {
                    // Một số bản MapWinGIS cũ có thể không hỗ trợ.
                }

                if (lblStatusCoordinates != null)
                {
                    lblStatusCoordinates.Text =
                        "X/Y (m): xem trên bản đồ";
                }

                if (lblStatusProjection != null)
                {
                    lblStatusProjection.Text =
                        "CRS: EPSG:32648 | mét";
                }

                if (cboCurrentLayer != null)
                {
                    cboCurrentLayer.SelectedIndexChanged -=
                        cboCurrentLayer_Status_SelectedIndexChanged;

                    cboCurrentLayer.SelectedIndexChanged +=
                        cboCurrentLayer_Status_SelectedIndexChanged;
                }

                UpdateCurrentLayerStatus();

                _statusBarInitialized =
                    true;
            }
            catch (Exception ex)
            {
                _statusBarInitialized =
                    false;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "StatusBar: " +
                        ex.Message;
                }
            }
        }

        // =========================================================
        // THAY ĐỔI LỚP HIỆN TẠI
        // =========================================================
        private void cboCurrentLayer_Status_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            UpdateCurrentLayerStatus();
        }

        // =========================================================
        // CẬP NHẬT TÊN LỚP TRÊN STATUS BAR
        // =========================================================
        private void UpdateCurrentLayerStatus()
        {
            if (lblStatusLayer == null)
            {
                return;
            }

            string layerKey =
                "";

            // ---------------------------------------------
            // Ưu tiên ComboBox vì đây là "Lớp hiện tại"
            // dùng chung cho Query / Measure / Buffer.
            // ---------------------------------------------
            if (cboCurrentLayer != null &&
                cboCurrentLayer.SelectedItem != null)
            {
                layerKey =
                    cboCurrentLayer
                        .SelectedItem
                        .ToString();
            }
            else if (treeLayers != null &&
                     treeLayers.SelectedNode != null)
            {
                layerKey =
                    GetStatusLayerKeyFromTreeNode(
                        treeLayers.SelectedNode);
            }

            string displayName =
                GetStatusLayerDisplayName(
                    layerKey);

            lblStatusLayer.Text =
                string.IsNullOrWhiteSpace(
                    displayName)
                    ? "Lớp: --"
                    : "Lớp: " +
                      displayName;
        }

        // =========================================================
        // LẤY LOGICAL KEY TỪ TREE NODE
        // =========================================================
        private string GetStatusLayerKeyFromTreeNode(
            System.Windows.Forms.TreeNode node)
        {
            if (node == null)
            {
                return "";
            }

            // Với các node chuẩn do LayerManager tạo,
            // Name thường chính là logical key.
            if (!string.IsNullOrWhiteSpace(
                node.Name))
            {
                return node.Name;
            }

            // Nếu Tag đang chứa key kỹ thuật thì ưu tiên Tag.
            if (node.Tag != null)
            {
                string tagText =
                    node.Tag.ToString();

                if (!string.IsNullOrWhiteSpace(
                    tagText)
                    &&
                    !string.Equals(
                        tagText,
                        "FILE",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    !string.Equals(
                        tagText,
                        "BUFFER",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return tagText;
                }
            }

            return
                node.Text ?? "";
        }

        // =========================================================
        // TÊN LỚP THÂN THIỆN
        // =========================================================
        private string GetStatusLayerDisplayName(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return "";
            }

            if (string.Equals(
                layerKey,
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Ranh giới Thành phố Hà Nội";
            }

            if (string.Equals(
                layerKey,
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Xã, phường Hà Nội";
            }

            if (string.Equals(
                layerKey,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Đơn vị quân đội";
            }

            if (string.Equals(
                layerKey,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Tuyến đường đơn vị";
            }

            if (string.Equals(
                layerKey,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Khu vực đơn vị";
            }

            // ---------------------------------------------
            // BUFFER / LAYER CỤC BỘ:
            // nếu đang load, ưu tiên tên hiển thị thật trên map.
            // ---------------------------------------------
            try
            {
                int handle;

                if (_loadedLayers != null &&
                    _loadedLayers.TryGetValue(
                        layerKey,
                        out handle)
                    &&
                    handle >= 0)
                {
                    string mapLayerName =
                        axMap2.get_LayerName(
                            handle);

                    if (!string.IsNullOrWhiteSpace(
                        mapLayerName))
                    {
                        return
                            mapLayerName;
                    }
                }
            }
            catch
            {
            }

            return
                layerKey;
        }

        // =========================================================
        // DỌN STATUS BAR KHI ĐÓNG FORM
        // =========================================================
        private void StopStatusBar()
        {
            try
            {
                if (cboCurrentLayer != null)
                {
                    cboCurrentLayer.SelectedIndexChanged -=
                        cboCurrentLayer_Status_SelectedIndexChanged;
                }

                if (axMap2 != null)
                {
                    axMap2.SendMouseMove =
                        false;

                    // Tắt phần tọa độ native khi form chuẩn bị đóng.
                    try
                    {
                        axMap2.ShowCoordinates =
                            tkCoordinatesDisplay.cdmNone;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            _statusBarInitialized =
                false;
        }
    }
}