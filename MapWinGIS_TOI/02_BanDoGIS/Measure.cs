using System;
using System.Globalization;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // MEASURE.CS - HÀ NỘI ONLY / EPSG:32648
    //
    // Chức năng:
    // - Đo khoảng cách trực tiếp trên bản đồ;
    // - Xóa kết quả đo;
    // - Tạo vùng đệm theo mét từ lớp hiện tại;
    // - Có thể buffer toàn bộ lớp hoặc chỉ các đối tượng đang chọn.
    //
    // CRS toàn hệ thống là EPSG:32648 nên đơn vị tọa độ là mét.
    // Vì vậy:
    // - phép đo khoảng cách dùng trực tiếp đơn vị mét;
    // - BufferByDistance() nhận khoảng cách theo mét;
    // - không cần biến đổi hệ tọa độ trước khi buffer.
    //
    // Với lớp đơn vị quân đội, Point chỉ là Point đại diện hành chính.
    // Buffer tạo ra chỉ mang ý nghĩa tham khảo/phân tích GIS.
    // =============================================================
    public partial class Form1
    {
        // =========================================================
        // KHỞI TẠO CÔNG CỤ ĐO / BUFFER
        // =========================================================
        private void InitMeasureTools()
        {
            if (btnMeasure != null)
            {
                btnMeasure.Click -=
                    btnMeasure_Click;

                btnMeasure.Click +=
                    btnMeasure_Click;
            }

            if (btnClearMeasure != null)
            {
                btnClearMeasure.Click -=
                    btnClearMeasure_Click;

                btnClearMeasure.Click +=
                    btnClearMeasure_Click;
            }

            if (btnBuffer != null)
            {
                btnBuffer.Click -=
                    btnBuffer_Click;

                btnBuffer.Click +=
                    btnBuffer_Click;
            }
        }

        // =========================================================
        // ĐO KHOẢNG CÁCH
        // =========================================================
        private void btnMeasure_Click(
            object sender,
            EventArgs e)
        {
            if (axMap2 == null)
            {
                return;
            }

            try
            {
                axMap2.Measuring.MeasuringType =
                    tkMeasuringType.MeasureDistance;

                axMap2.CursorMode =
                    tkCursorMode.cmMeasure;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đo khoảng cách EPSG:32648: " +
                        "bấm các điểm, double-click để kết thúc. " +
                        "Đơn vị: mét.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không bật được công cụ đo.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // XÓA KẾT QUẢ ĐO
        // =========================================================
        private void btnClearMeasure_Click(
            object sender,
            EventArgs e)
        {
            if (axMap2 == null)
            {
                return;
            }

            try
            {
                axMap2.Measuring.Clear();

                axMap2.CursorMode =
                    tkCursorMode.cmPan;

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã xóa kết quả đo và trở về chế độ Pan.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không xóa được kết quả đo.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // BUFFER
        // =========================================================
        private void btnBuffer_Click(
            object sender,
            EventArgs e)
        {
            RefreshLayerCombo();

            string layerKey =
                cboCurrentLayer != null
                    ? cboCurrentLayer.SelectedItem
                        as string
                    : null;

            if (string.IsNullOrWhiteSpace(
                layerKey)
                ||
                !_loadedLayers.ContainsKey(
                    layerKey))
            {
                MessageBox.Show(
                    "Chưa có lớp nào được chọn ở ô \"Lớp hiện tại\".",
                    "Chưa chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int handle =
                _loadedLayers[layerKey];

            Shapefile sf =
                axMap2.get_Shapefile(
                    handle);

            if (sf == null)
            {
                MessageBox.Show(
                    "Lớp hiện tại không hỗ trợ thao tác Buffer.",
                    "Không hỗ trợ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (sf.NumShapes <= 0)
            {
                MessageBox.Show(
                    "Lớp hiện tại chưa có đối tượng để tạo vùng đệm.",
                    "Không có dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string displayName =
                GetMeasureLayerDisplayName(
                    layerKey);

            bool hasSelection =
                sf.NumSelected > 0;

            string prompt =
                "Nhập khoảng cách vùng đệm theo mét.\r\n" +
                "Ví dụ: 500 hoặc 1500.\r\n\r\n" +
                "Lớp: " +
                displayName +
                "\r\n" +
                (
                    hasSelection
                        ? "Phạm vi: chỉ các đối tượng đang được chọn."
                        : "Phạm vi: toàn bộ lớp."
                );

            string input =
                ShowQueryInputDialog(
                    "Vùng đệm - " +
                    displayName,
                    prompt);

            if (string.IsNullOrWhiteSpace(
                input))
            {
                return;
            }

            double distance;

            if (!TryParseBufferDistance(
                input,
                out distance)
                ||
                distance <= 0)
            {
                MessageBox.Show(
                    "Khoảng cách không hợp lệ.\n\n" +
                    "Hãy nhập số lớn hơn 0, ví dụ: 500.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                bool selectedOnly =
                    sf.NumSelected > 0;

                Shapefile bufferSf =
                    sf.BufferByDistance(
                        distance,
                        24,
                        selectedOnly,
                        true);

                if (bufferSf == null)
                {
                    MessageBox.Show(
                        "Không tạo được vùng đệm:\n" +
                        sf.get_ErrorMsg(
                            sf.LastErrorCode),
                        "Lỗi vùng đệm",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                // ---------------------------------------------
                // STYLE BUFFER
                // ---------------------------------------------
                Utils utils =
                    new Utils();

                bufferSf.DefaultDrawingOptions.FillColor =
                    utils.ColorByName(
                        tkMapColor.Yellow);

                bufferSf.DefaultDrawingOptions.FillTransparency =
                    140;

                bufferSf.DefaultDrawingOptions.LineColor =
                    utils.ColorByName(
                        tkMapColor.Red);

                bufferSf.DefaultDrawingOptions.LineWidth =
                    2.0f;

                int newHandle =
                    axMap2.AddLayer(
                        bufferSf,
                        true);

                if (newHandle < 0)
                {
                    MessageBox.Show(
                        "Không thêm được lớp vùng đệm lên bản đồ.",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                string distanceText =
                    FormatBufferDistanceForKey(
                        distance);

                string baseKey =
                    "buffer_" +
                    layerKey +
                    "_" +
                    distanceText +
                    "m";

                string key =
                    baseKey;

                int suffix =
                    1;

                while (_loadedLayers.ContainsKey(
                    key))
                {
                    key =
                        baseKey +
                        "_" +
                        suffix;

                    suffix++;
                }

                string layerName =
                    "Vùng đệm " +
                    FormatBufferDistanceForDisplay(
                        distance) +
                    " m - " +
                    displayName;

                axMap2.set_LayerName(
                    newHandle,
                    layerName);

                _loadedLayers[key] =
                    newHandle;

                TreeNode node =
                    new TreeNode(
                        layerName);

                node.Name =
                    key;

                node.Text =
                    layerName;

                node.Tag =
                    "BUFFER";

                treeLayers.Nodes.Add(
                    node);

                SetLayerNodeChecked(
                    node,
                    true);

                RefreshLayerCombo();

                axMap2.ZoomToLayer(
                    newHandle);

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã tạo vùng đệm " +
                        FormatBufferDistanceForDisplay(
                            distance) +
                        " m quanh \"" +
                        displayName +
                        "\"" +
                        (
                            selectedOnly
                                ? " (đối tượng đã chọn)."
                                : " (toàn bộ lớp)."
                        );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tạo được vùng đệm.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // PARSE KHOẢNG CÁCH
        //
        // Hỗ trợ cả:
        // - 500.5
        // - 500,5
        // tùy thiết lập Windows.
        // =========================================================
        private bool TryParseBufferDistance(
            string input,
            out double distance)
        {
            distance =
                0.0;

            if (string.IsNullOrWhiteSpace(
                input))
            {
                return false;
            }

            string value =
                input.Trim();

            if (double.TryParse(
                value,
                NumberStyles.Float |
                NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture,
                out distance))
            {
                return
                    !double.IsNaN(
                        distance)
                    &&
                    !double.IsInfinity(
                        distance);
            }

            if (double.TryParse(
                value,
                NumberStyles.Float |
                NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out distance))
            {
                return
                    !double.IsNaN(
                        distance)
                    &&
                    !double.IsInfinity(
                        distance);
            }

            string normalized =
                value.Replace(
                    ',',
                    '.');

            if (double.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out distance))
            {
                return
                    !double.IsNaN(
                        distance)
                    &&
                    !double.IsInfinity(
                        distance);
            }

            return false;
        }

        // =========================================================
        // TÊN LỚP THÂN THIỆN
        // =========================================================
        private string GetMeasureLayerDisplayName(
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

            if (layerKey.StartsWith(
                "buffer_",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Lớp vùng đệm";
            }

            return layerKey;
        }

        // =========================================================
        // FORMAT KHOẢNG CÁCH
        // =========================================================
        private string FormatBufferDistanceForDisplay(
            double distance)
        {
            return
                distance.ToString(
                    "0.###",
                    CultureInfo.CurrentCulture);
        }

        private string FormatBufferDistanceForKey(
            double distance)
        {
            return
                distance.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)
                    .Replace(
                        ".",
                        "_");
        }
    }
}