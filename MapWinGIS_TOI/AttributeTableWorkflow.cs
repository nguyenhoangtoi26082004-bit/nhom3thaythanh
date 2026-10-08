using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // ATTRIBUTETABLEWORKFLOW.CS
    //
    // Bảng thuộc tính:
    // - nếu đã có lớp hiện tại -> mở lớp đó;
    // - nếu chưa có -> tự dùng Đơn vị quân đội;
    // - nếu lớp PostGIS chưa load -> tự lazy-load;
    // - không hiện popup "Chưa chọn lớp".
    // =============================================================
    public partial class Form1
    {
        private void OpenAttributeTableSmooth()
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
                        "Không thể nạp lớp \"" +
                        GetPostGISDisplayName(
                            layerKey) +
                        "\" để mở bảng thuộc tính.",
                        "Không thể mở bảng thuộc tính",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            TreeNode node =
                FindLayerNodeRecursive(
                    treeLayers.Nodes,
                    layerKey);

            if (node == null)
            {
                // Fallback an toàn về lớp đơn vị quân đội.
                layerKey =
                    "don_vi_quan_doi";

                if (!EnsureLayerReady(
                    layerKey,
                    true))
                {
                    return;
                }

                node =
                    FindLayerNodeRecursive(
                        treeLayers.Nodes,
                        layerKey);
            }

            if (node == null)
            {
                return;
            }

            SelectWorkingLayer(
                layerKey);

            OpenAttributeTable(
                node);
        }
    }
}