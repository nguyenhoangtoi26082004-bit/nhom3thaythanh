using System;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // BUFFERTOOLS.CS - BUFFER MƯỢT
    //
    // Quy tắc:
    // - Có lớp hiện tại -> dùng lớp đó;
    // - Chưa có lớp -> tự dùng Đơn vị quân đội;
    // - PostGIS chưa load -> tự lazy-load;
    // - Có feature đang chọn -> chỉ buffer feature đã chọn;
    // - Không có feature chọn -> buffer toàn bộ lớp (giữ hành vi cũ);
    // - Mỗi lần tạo mới sẽ xóa lớp buffer tạm trước đó.
    // =============================================================
    public partial class Form1
    {
        private const string SMART_BUFFER_LAYER_KEY =
            "__SMART_BUFFER__";


        private void RunSmartBuffer()
        {
            string layerKey =
                GetWorkingLayerOrDefault();

            if (IsManagedPostGISTable(
                layerKey))
            {
                if (!EnsureLayerReady(
                    layerKey,
                    true))
                {
                    MessageBox.Show(
                        "Không thể nạp lớp dữ liệu để tạo vùng đệm.",
                        "Buffer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            int handle =
                GetLayerHandleSafe(
                    layerKey);

            if (handle < 0)
            {
                MessageBox.Show(
                    "Không xác định được lớp dữ liệu để tạo vùng đệm.",
                    "Buffer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            Shapefile sf = null;

            try
            {
                sf =
                    axMap2.get_Shapefile(
                        handle);
            }
            catch
            {
            }

            if (sf == null)
            {
                MessageBox.Show(
                    "Lớp hiện tại không hỗ trợ thao tác Buffer.",
                    "Buffer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string input =
                ShowQueryInputDialog(
                    "Tạo vùng đệm",
                    "Khoảng cách vùng đệm (mét), ví dụ: 500");

            if (string.IsNullOrWhiteSpace(
                input))
            {
                return;
            }

            double distance;

            if (!double.TryParse(
                    input.Trim(),
                    out distance) ||
                distance <= 0)
            {
                MessageBox.Show(
                    "Khoảng cách vùng đệm phải là số lớn hơn 0.",
                    "Khoảng cách không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang tạo vùng đệm " +
                        distance.ToString("0.##") +
                        " m...";
                }

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
                        "Không tạo được vùng đệm.\n\n" +
                        sf.get_ErrorMsg(
                            sf.LastErrorCode),
                        "Lỗi Buffer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                RemovePreviousSmartBuffer();

                StyleSmartBuffer(
                    bufferSf);

                int newHandle =
                    axMap2.AddLayer(
                        bufferSf,
                        true);

                if (newHandle < 0)
                {
                    try
                    {
                        bufferSf.Close();
                    }
                    catch
                    {
                    }

                    MessageBox.Show(
                        "Đã tạo geometry Buffer nhưng không thêm được lên bản đồ.",
                        "Lỗi Buffer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                string displayName =
                    "Buffer " +
                    distance.ToString("0.##") +
                    " m - " +
                    GetPostGISDisplayName(
                        layerKey);

                axMap2.set_LayerName(
                    newHandle,
                    displayName);

                _loadedLayers[
                    SMART_BUFFER_LAYER_KEY] =
                    newHandle;

                TreeNode node =
                    new TreeNode();

                node.Name =
                    SMART_BUFFER_LAYER_KEY;

                node.Text =
                    displayName;

                node.Tag =
                    NODE_TAG_FILE;

                treeLayers.Nodes.Add(
                    node);

                SetLayerNodeChecked(
                    node,
                    true);

                RefreshLayerCombo();

                // Giữ lớp nguồn là lớp làm việc thay vì tự chuyển
                // combo sang lớp buffer tạm.
                if (cboCurrentLayer != null &&
                    cboCurrentLayer.Items.Contains(
                        layerKey))
                {
                    cboCurrentLayer.SelectedItem =
                        layerKey;
                }

                try
                {
                    axMap2.ZoomToLayer(
                        newHandle);

                    axMap2.Redraw();
                }
                catch
                {
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        selectedOnly
                            ? "Đã tạo Buffer cho đối tượng đang chọn | " +
                              distance.ToString("0.##") +
                              " m."
                            : "Đã tạo Buffer cho toàn bộ lớp | " +
                              distance.ToString("0.##") +
                              " m.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tạo vùng đệm.\n\n" +
                    ex.Message,
                    "Lỗi Buffer",
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
        // STYLE BUFFER
        // =========================================================
        private void StyleSmartBuffer(
            Shapefile bufferSf)
        {
            if (bufferSf == null)
            {
                return;
            }

            try
            {
                Utils utils =
                    new Utils();

                bufferSf.DefaultDrawingOptions.FillVisible =
                    true;

                bufferSf.DefaultDrawingOptions.FillColor =
                    utils.ColorByName(
                        tkMapColor.Yellow);

                bufferSf.DefaultDrawingOptions.FillTransparency =
                    75;

                bufferSf.DefaultDrawingOptions.LineColor =
                    utils.ColorByName(
                        tkMapColor.Red);

                bufferSf.DefaultDrawingOptions.LineWidth =
                    2.0f;
            }
            catch
            {
            }
        }


        // =========================================================
        // XÓA BUFFER TẠM TRƯỚC ĐÓ
        // =========================================================
        private void RemovePreviousSmartBuffer()
        {
            int oldHandle;

            if (_loadedLayers.TryGetValue(
                SMART_BUFFER_LAYER_KEY,
                out oldHandle))
            {
                try
                {
                    axMap2.RemoveLayer(
                        oldHandle);
                }
                catch
                {
                }

                _loadedLayers.Remove(
                    SMART_BUFFER_LAYER_KEY);
            }

            TreeNode oldNode =
                FindLayerNodeRecursive(
                    treeLayers.Nodes,
                    SMART_BUFFER_LAYER_KEY);

            if (oldNode != null)
            {
                try
                {
                    oldNode.Remove();
                }
                catch
                {
                }
            }
        }
    }
}