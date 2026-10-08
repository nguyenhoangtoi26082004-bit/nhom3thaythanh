using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // LAYERREQUIREMENT.CS
    //
    // Mục tiêu:
    // - Chức năng tự nạp đúng lớp PostGIS khi cần;
    // - Không bắt người dùng phải bấm "Quản lý CSDL PostGIS" trước;
    // - Không bắt người dùng phải chọn "Lớp hiện tại" trước;
    // - Chỉ nạp lớp cần dùng, không reload toàn bộ 5 lớp.
    // =============================================================
    public partial class Form1
    {
        // =========================================================
        // BẢO ĐẢM MỘT LỚP POSTGIS SẴN SÀNG
        // =========================================================
        private bool EnsureLayerReady(
            string layerKey,
            bool makeVisible)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return false;
            }

            try
            {
                int handle;

                // ---------------------------------------------
                // Đã có trong map -> dùng ngay.
                // ---------------------------------------------
                if (_loadedLayers.TryGetValue(
                    layerKey,
                    out handle))
                {
                    try
                    {
                        if (makeVisible)
                        {
                            axMap2.set_LayerVisible(
                                handle,
                                true);
                        }

                        EnsureLayerNodeReady(
                            layerKey,
                            makeVisible);

                        SelectWorkingLayer(
                            layerKey);

                        EnsureSatelliteBasemapBelowPostGIS();
                        UpdateModernDatabaseStatus();

                        return true;
                    }
                    catch
                    {
                        // Handle cũ không còn hợp lệ.
                        _loadedLayers.Remove(
                            layerKey);
                    }
                }

                // ---------------------------------------------
                // Chỉ lazy-load lớp thuộc hệ PostGIS của đề tài.
                // ---------------------------------------------
                if (!IsManagedPostGISTable(
                    layerKey))
                {
                    return false;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang nạp lớp cần thiết: " +
                        GetPostGISDisplayName(
                            layerKey) +
                        "...";
                }

                bool loaded =
                    LoadPostGISLayer(
                        layerKey,
                        false);

                if (!loaded)
                {
                    return false;
                }

                EnsureLayerNodeReady(
                    layerKey,
                    makeVisible);

                SelectWorkingLayer(
                    layerKey);

                EnsureSatelliteBasemapBelowPostGIS();
                UpdateModernDatabaseStatus();

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
                        "Sẵn sàng | " +
                        GetPostGISDisplayName(
                            layerKey);
                }

                return true;
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "EnsureLayerReady ERROR [" +
                    layerKey +
                    "]: " +
                    ex);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không nạp được lớp: " +
                        layerKey;
                }

                return false;
            }
        }


        // =========================================================
        // LẤY HANDLE NẾU LỚP ĐÃ SẴN SÀNG
        // =========================================================
        private int GetLayerHandleSafe(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return -1;
            }

            int handle;

            if (_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                return handle;
            }

            return -1;
        }


        // =========================================================
        // BẢO ĐẢM NODE TREEVIEW CÓ MẶT
        // =========================================================
        private TreeNode EnsureLayerNodeReady(
            string layerKey,
            bool isChecked)
        {
            if (treeLayers == null ||
                string.IsNullOrWhiteSpace(
                    layerKey))
            {
                return null;
            }

            TreeNode node =
                FindLayerNodeRecursive(
                    treeLayers.Nodes,
                    layerKey);

            if (node == null)
            {
                node =
                    new TreeNode();

                node.Name =
                    layerKey;

                node.Text =
                    GetPostGISDisplayName(
                        layerKey);

                node.Tag =
                    POSTGIS_NODE_TAG;

                treeLayers.Nodes.Add(
                    node);
            }

            SetLayerNodeChecked(
                node,
                isChecked);

            return node;
        }


        // =========================================================
        // ĐẶT LỚP LÀM VIỆC HIỆN TẠI
        // =========================================================
        private void SelectWorkingLayer(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return;
            }

            RefreshLayerCombo();

            if (cboCurrentLayer != null &&
                cboCurrentLayer.Items.Contains(
                    layerKey))
            {
                cboCurrentLayer.SelectedItem =
                    layerKey;
            }

            if (treeLayers != null)
            {
                TreeNode node =
                    FindLayerNodeRecursive(
                        treeLayers.Nodes,
                        layerKey);

                if (node != null)
                {
                    treeLayers.SelectedNode =
                        node;
                }
            }

            UpdateCurrentLayerStatus();
        }


        // =========================================================
        // LẤY LỚP ĐANG LÀM VIỆC; NẾU CHƯA CÓ THÌ DÙNG ĐƠN VỊ
        // =========================================================
        private string GetWorkingLayerOrDefault()
        {
            string layerKey =
                cboCurrentLayer != null
                    ? cboCurrentLayer.SelectedItem
                        as string
                    : null;

            if (!string.IsNullOrWhiteSpace(
                layerKey) &&
                _loadedLayers.ContainsKey(
                    layerKey))
            {
                return layerKey;
            }

            return
                "don_vi_quan_doi";
        }
    }
}