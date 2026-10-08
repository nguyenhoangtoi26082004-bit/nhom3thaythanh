using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // SPATIAL QUERY INTEGRATION - HÀ NỘI ONLY
    //
    // Chức năng:
    // - Gắn cửa sổ truy vấn không gian vào menu Phân tích;
    // - Truyền đúng handle của 3 lớp nghiệp vụ:
    //      don_vi_quan_doi
    //      tuyen_duong_don_vi
    //      khu_vuc_don_vi
    //
    // Lưu ý:
    // - Xã/phường được SpatialQueryService đọc trực tiếp từ PostGIS;
    // - Không cần truyền handle lớp xa_phuong_ha_noi vào form;
    // - Các truy vấn CSDL vẫn chạy ngay cả khi một lớp chưa được
    //   bật trên bản đồ. Khi đó chỉ chức năng Chọn/Zoom lớp đó
    //   trên map là không thực hiện được.
    // =============================================================
    public partial class Form1
    {
        private bool _spatialQueryToolsInitialized =
            false;

        private ToolStripMenuItem
            _menuSpatialDataQuery;

        // =========================================================
        // KHỞI TẠO MENU TRUY VẤN KHÔNG GIAN
        // =========================================================
        private void InitSpatialQueryTools()
        {
            if (_spatialQueryToolsInitialized)
            {
                return;
            }

            if (menuAnalysis == null)
            {
                return;
            }

            // -----------------------------------------------------
            // Tránh chèn trùng menu nếu hàm được gọi lại.
            // -----------------------------------------------------
            ToolStripItem existingItem =
                menuAnalysis.DropDownItems[
                    "menuSpatialDataQuery"];

            if (existingItem != null)
            {
                _menuSpatialDataQuery =
                    existingItem as ToolStripMenuItem;

                if (_menuSpatialDataQuery != null)
                {
                    _menuSpatialDataQuery.Text =
                        "Truy vấn không gian Hà Nội...";

                    _menuSpatialDataQuery.Click -=
                        menuSpatialDataQuery_Click;

                    _menuSpatialDataQuery.Click +=
                        menuSpatialDataQuery_Click;
                }

                _spatialQueryToolsInitialized =
                    true;

                return;
            }

            _menuSpatialDataQuery =
                new ToolStripMenuItem();

            _menuSpatialDataQuery.Name =
                "menuSpatialDataQuery";

            _menuSpatialDataQuery.Text =
                "Truy vấn không gian Hà Nội...";

            _menuSpatialDataQuery.ToolTipText =
                "Truy vấn xã/phường, Point đại diện và kiểm tra geometry EPSG:32648.";

            _menuSpatialDataQuery.Click +=
                menuSpatialDataQuery_Click;

            menuAnalysis.DropDownItems.Insert(
                0,
                _menuSpatialDataQuery);

            // -----------------------------------------------------
            // Chỉ chèn separator nếu phía sau chưa phải separator.
            // -----------------------------------------------------
            if (menuAnalysis.DropDownItems.Count < 2 ||
                !(menuAnalysis.DropDownItems[1]
                    is ToolStripSeparator))
            {
                menuAnalysis.DropDownItems.Insert(
                    1,
                    new ToolStripSeparator());
            }

            _spatialQueryToolsInitialized =
                true;
        }

        // =========================================================
        // CLICK MENU
        // =========================================================
        private void menuSpatialDataQuery_Click(
            object sender,
            EventArgs e)
        {
            OpenSpatialQueryForm();
        }

        // =========================================================
        // MỞ FORM TRUY VẤN KHÔNG GIAN
        // =========================================================
        private void OpenSpatialQueryForm()
        {
            int unitHandle =
                GetSpatialQueryLayerHandle(
                    "don_vi_quan_doi");

            int routeHandle =
                GetSpatialQueryLayerHandle(
                    "tuyen_duong_don_vi");

            int areaHandle =
                GetSpatialQueryLayerHandle(
                    "khu_vuc_don_vi");

            using (SpatialQueryForm form =
                new SpatialQueryForm(
                    axMap2,
                    unitHandle,
                    routeHandle,
                    areaHandle))
            {
                form.ShowDialog(
                    this);
            }
        }

        // =========================================================
        // LẤY HANDLE LỚP ĐANG LOAD
        //
        // _loadedLayers dùng logical key của ứng dụng.
        //
        // Ví dụ:
        // don_vi_quan_doi có thể được đọc từ view v_don_vi_ban_do,
        // nhưng trong _loadedLayers vẫn dùng key don_vi_quan_doi.
        // =========================================================
        private int GetSpatialQueryLayerHandle(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return -1;
            }

            if (_loadedLayers == null)
            {
                return -1;
            }

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                return -1;
            }

            if (handle < 0)
            {
                return -1;
            }

            return handle;
        }
    }
}