using System;
using System.Collections.Generic;
using System.Drawing;
using DrawingPoint = System.Drawing.Point;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        // =========================================================
        // LAYER MANAGER - HÀ NỘI ONLY / EPSG:32648
        //
        // ĐỀ TÀI:
        // "Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
        //  các đơn vị quân đội trên địa bàn Hà Nội"
        //
        // Mục tiêu:
        // 1. Quản lý nhóm dữ liệu GIS Hà Nội và dữ liệu bổ sung.
        // 2. Hiển thị đúng biểu tượng Điểm / Đường / Vùng.
        // 3. Checkbox chỉ BẬT / TẮT lớp, không tự zoom.
        // 4. Hạn chế Redraw nhiều lần để thao tác mượt hơn.
        // 5. Menu chuột phải hỗ trợ tra cứu và quản lý lớp.
        // 6. Tất cả nội dung giao diện dùng tiếng Việt.
        //
        // Tương thích với:
        // - PostGIS.cs HÀ NỘI ONLY:
        //      Schema = ql_don_vi
        //      CRS toàn hệ thống = EPSG:32648
        //      Không dùng OpenStreetMap / EPSG:3857.
        //      Lớp logic:
        //          dia_gioi_hanh_chinh -> Ranh giới Thành phố Hà Nội
        //          xa_phuong_ha_noi    -> 126 xã/phường mới
        //          don_vi_quan_doi     -> Point đại diện đơn vị
        //          tuyen_duong_don_vi  -> LineString
        //          khu_vuc_don_vi      -> Polygon
        //      Tag = "POSTGIS"
        //      SetLayerNodeChecked(...)
        //      LoadPostGISLayer(...)
        //
        // - Navigation.cs hiện tại:
        //      Tag = "FILE"
        //      SetLayerNodeChecked(...)
        //
        // - Menu.cs hiện tại:
        //      ShowAllLayers()
        //      HideAllLayers()
        //      ZoomToLayer(...)
        //      RemoveSelectedLayer()
        //      RefreshMapView()
        //      ClearAllSelections()
        // =========================================================

        // =========================================================
        // TAG / NAME CỦA CÁC NHÓM
        // =========================================================
        private const string NODE_TAG_POSTGIS = "POSTGIS";
        private const string NODE_TAG_FILE = "FILE";

        private const string NODE_TAG_GROUP_POSTGIS =
            "GROUP_POSTGIS";

        private const string NODE_TAG_GROUP_LOCAL =
            "GROUP_LOCAL";

        private const string GROUP_POSTGIS_NAME =
            "__GROUP_POSTGIS__";

        private const string GROUP_LOCAL_NAME =
            "__GROUP_LOCAL__";

        // =========================================================
        // TRẠNG THÁI NỘI BỘ
        // =========================================================
        private bool _layerCheckInternalChange = false;

        private ContextMenuStrip _layerContextMenu;

        private ImageList _layerTreeImages;

        // =========================================================
        // KHỞI TẠO QUẢN LÝ LỚP
        // =========================================================
        private void InitLayerManager()
        {
            if (treeLayers == null)
                return;

            // -----------------------------------------------------
            // Cấu hình TreeView
            // -----------------------------------------------------
            treeLayers.CheckBoxes = true;
            treeLayers.HideSelection = false;
            treeLayers.FullRowSelect = true;
            treeLayers.ShowLines = true;
            treeLayers.ShowPlusMinus = true;
            treeLayers.ShowRootLines = true;

            // -----------------------------------------------------
            // Biểu tượng Điểm / Đường / Vùng / Nhóm
            // -----------------------------------------------------
            InitLayerTreeImages();

            // -----------------------------------------------------
            // TreeView events
            // -----------------------------------------------------
            treeLayers.AfterCheck -= treeLayers_AfterCheck;
            treeLayers.AfterCheck += treeLayers_AfterCheck;

            treeLayers.AfterSelect -= treeLayers_AfterSelect;
            treeLayers.AfterSelect += treeLayers_AfterSelect;

            treeLayers.NodeMouseDoubleClick -=
                treeLayers_NodeMouseDoubleClick;

            treeLayers.NodeMouseDoubleClick +=
                treeLayers_NodeMouseDoubleClick;

            treeLayers.NodeMouseClick -=
                treeLayers_NodeMouseClick;

            treeLayers.NodeMouseClick +=
                treeLayers_NodeMouseClick;

            // -----------------------------------------------------
            // Nút bên trái
            // -----------------------------------------------------
            if (btnAddLayer != null)
            {
                btnAddLayer.Click -= btnAddLayer_Click;
                btnAddLayer.Click += btnAddLayer_Click;
            }

            if (btnRemoveLayer != null)
            {
                btnRemoveLayer.Click -= btnRemoveLayer_Click;
                btnRemoveLayer.Click += btnRemoveLayer_Click;
            }

            // -----------------------------------------------------
            // Toolbar
            // -----------------------------------------------------
            if (btnRefreshToolbar != null)
            {
                btnRefreshToolbar.Click -=
                    btnRefreshToolbar_Click;

                btnRefreshToolbar.Click +=
                    btnRefreshToolbar_Click;
            }

            if (btnClearSelectionToolbar != null)
            {
                btnClearSelectionToolbar.Click -=
                    btnClearSelectionToolbar_Click;

                btnClearSelectionToolbar.Click +=
                    btnClearSelectionToolbar_Click;
            }

            // -----------------------------------------------------
            // Menu chuột phải
            // -----------------------------------------------------
            CreateLayerContextMenu();

            // -----------------------------------------------------
            // Nếu trước khi LayerManager khởi tạo đã có node,
            // gom chúng về đúng nhóm.
            // -----------------------------------------------------
            OrganizeExistingLayerNodes();

            // -----------------------------------------------------
            // StatusBar
            // -----------------------------------------------------
            InitStatusBar();

            UpdateCurrentLayerStatus();
        }

        // =========================================================
        // KHỞI TẠO ICON CHO DANH SÁCH LỚP
        //
        // Không cần file PNG ngoài.
        // Icon được vẽ trực tiếp để tránh phụ thuộc tài nguyên.
        // =========================================================
        private void InitLayerTreeImages()
        {
            if (_layerTreeImages != null)
            {
                try
                {
                    _layerTreeImages.Dispose();
                }
                catch
                {
                }

                _layerTreeImages = null;
            }

            _layerTreeImages = new ImageList();

            _layerTreeImages.ImageSize =
                new Size(16, 16);

            _layerTreeImages.ColorDepth =
                ColorDepth.Depth32Bit;

            _layerTreeImages.Images.Add(
                "group",
                CreateGroupIcon());

            _layerTreeImages.Images.Add(
                "point",
                CreatePointIcon());

            _layerTreeImages.Images.Add(
                "line",
                CreateLineIcon());

            _layerTreeImages.Images.Add(
                "polygon",
                CreatePolygonIcon());

            _layerTreeImages.Images.Add(
                "layer",
                CreateGenericLayerIcon());

            treeLayers.ImageList =
                _layerTreeImages;
        }

        // =========================================================
        // ICON NHÓM
        // =========================================================
        private Bitmap CreateGroupIcon()
        {
            Bitmap bmp =
                new Bitmap(16, 16);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                using (SolidBrush brush =
                    new SolidBrush(
                        Color.FromArgb(
                            236,
                            181,
                            47)))
                {
                    g.FillRectangle(
                        brush,
                        2,
                        5,
                        12,
                        8);

                    g.FillRectangle(
                        brush,
                        3,
                        3,
                        6,
                        4);
                }

                using (Pen pen =
                    new Pen(
                        Color.FromArgb(
                            160,
                            115,
                            25)))
                {
                    g.DrawRectangle(
                        pen,
                        2,
                        5,
                        11,
                        7);
                }
            }

            return bmp;
        }

        // =========================================================
        // ICON ĐIỂM
        // =========================================================
        private Bitmap CreatePointIcon()
        {
            Bitmap bmp =
                new Bitmap(16, 16);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                g.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (SolidBrush brush =
                    new SolidBrush(
                        Color.FromArgb(
                            220,
                            50,
                            47)))
                {
                    g.FillEllipse(
                        brush,
                        4,
                        4,
                        8,
                        8);
                }

                using (Pen pen =
                    new Pen(Color.DarkRed))
                {
                    g.DrawEllipse(
                        pen,
                        4,
                        4,
                        8,
                        8);
                }
            }

            return bmp;
        }

        // =========================================================
        // ICON ĐƯỜNG
        // =========================================================
        private Bitmap CreateLineIcon()
        {
            Bitmap bmp =
                new Bitmap(16, 16);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                g.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Pen pen =
                    new Pen(
                        Color.FromArgb(
                            35,
                            98,
                            210),
                        2.5f))
                {
                    g.DrawLines(
                        pen,
                        new DrawingPoint[]
                        {
                            new DrawingPoint(2, 12),
                            new DrawingPoint(6, 8),
                            new DrawingPoint(9, 9),
                            new DrawingPoint(14, 3)
                        });
                }
            }

            return bmp;
        }

        // =========================================================
        // ICON VÙNG
        // =========================================================
        private Bitmap CreatePolygonIcon()
        {
            Bitmap bmp =
                new Bitmap(16, 16);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                DrawingPoint[] points =
                {
                    new DrawingPoint(3, 4),
                    new DrawingPoint(12, 3),
                    new DrawingPoint(14, 10),
                    new DrawingPoint(9, 14),
                    new DrawingPoint(3, 11)
                };

                using (SolidBrush brush =
                    new SolidBrush(
                        Color.FromArgb(
                            130,
                            241,
                            196,
                            15)))
                {
                    g.FillPolygon(
                        brush,
                        points);
                }

                using (Pen pen =
                    new Pen(
                        Color.FromArgb(
                            205,
                            115,
                            35)))
                {
                    g.DrawPolygon(
                        pen,
                        points);
                }
            }

            return bmp;
        }

        // =========================================================
        // ICON LAYER CHUNG
        // =========================================================
        private Bitmap CreateGenericLayerIcon()
        {
            Bitmap bmp =
                new Bitmap(16, 16);

            using (Graphics g =
                Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                using (SolidBrush brush1 =
                    new SolidBrush(
                        Color.FromArgb(
                            76,
                            175,
                            80)))
                using (SolidBrush brush2 =
                    new SolidBrush(
                        Color.FromArgb(
                            33,
                            150,
                            243)))
                using (Pen pen =
                    new Pen(
                        Color.FromArgb(
                            90,
                            90,
                            90)))
                {
                    g.FillRectangle(
                        brush1,
                        3,
                        3,
                        10,
                        5);

                    g.FillRectangle(
                        brush2,
                        3,
                        9,
                        10,
                        4);

                    g.DrawRectangle(
                        pen,
                        3,
                        3,
                        9,
                        9);
                }
            }

            return bmp;
        }

        // =========================================================
        // NÚT THÊM LỚP
        // =========================================================
        private void btnAddLayer_Click(
            object sender,
            EventArgs e)
        {
            btnOpenLayer_Click(
                sender,
                e);
        }

        // =========================================================
        // NÚT XÓA LỚP
        // =========================================================
        private void btnRemoveLayer_Click(
            object sender,
            EventArgs e)
        {
            RemoveSelectedLayer();
        }

        // =========================================================
        // TOOLBAR LÀM MỚI
        // =========================================================
        private void btnRefreshToolbar_Click(
            object sender,
            EventArgs e)
        {
            RefreshMapView();
        }

        // =========================================================
        // TOOLBAR BỎ CHỌN
        // =========================================================
        private void btnClearSelectionToolbar_Click(
            object sender,
            EventArgs e)
        {
            ClearAllSelections();
        }

        // =========================================================
        // LÀM MỚI BẢN ĐỒ HÀ NỘI
        //
        // Hệ thống hiện tại không dùng OpenStreetMap / tile online.
        // Vì vậy thao tác làm mới chỉ:
        // - đồng bộ TreeView + Combo lớp;
        // - redraw các lớp vector EPSG:32648;
        // - không gọi tile server, không reload tile.
        // =========================================================
        private void RefreshMapView()
        {
            if (axMap2 == null)
                return;

            if (btnRefreshToolbar != null)
                btnRefreshToolbar.Enabled = false;

            Cursor oldCursor = Cursor.Current;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang làm mới dữ liệu GIS Hà Nội...";
                }

                // 1. Đồng bộ danh sách lớp / lớp làm việc.
                OrganizeExistingLayerNodes();
                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                // 2. Bảo đảm tile online luôn tắt.
                try
                {
                    axMap2.Tiles.Visible = false;
                    axMap2.Tiles.UseServer = false;
                }
                catch
                {
                }

                // 3. Redraw vector. Tham số false = không reload tile.
                try
                {
                    axMap2.Redraw3(
                        tkRedrawType.RedrawAll,
                        false);
                }
                catch
                {
                    try
                    {
                        axMap2.Redraw();
                    }
                    catch
                    {
                    }
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã làm mới các lớp GIS Hà Nội | EPSG:32648.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể làm mới bản đồ Hà Nội.\n\n" +
                    ex.Message,
                    "Lỗi làm mới bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Làm mới bản đồ thất bại.";
                }
            }
            finally
            {
                Cursor.Current = oldCursor;

                if (btnRefreshToolbar != null)
                    btnRefreshToolbar.Enabled = true;
            }
        }

        // =========================================================
        // BỎ CHỌN TẤT CẢ ĐỐI TƯỢNG
        // =========================================================
        private void ClearAllSelections()
        {
            if (_loadedLayers == null ||
                _loadedLayers.Count == 0)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Chưa có lớp dữ liệu.";
                }

                return;
            }

            int selectedLayerCount = 0;

            foreach (KeyValuePair<string, int> layer
                in _loadedLayers)
            {
                try
                {
                    Shapefile sf =
                        axMap2.get_Shapefile(
                            layer.Value);

                    if (sf == null)
                        continue;

                    if (sf.NumSelected > 0)
                    {
                        sf.SelectNone();

                        selectedLayerCount++;
                    }
                }
                catch
                {
                    // Một layer lỗi không làm dừng toàn bộ thao tác.
                }
            }

            try
            {
                if (axMap2 != null)
                    axMap2.Redraw();
            }
            catch
            {
            }

            if (lblStatus != null)
            {
                lblStatus.Text =
                    selectedLayerCount > 0
                        ? "Đã bỏ chọn tất cả đối tượng."
                        : "Không có đối tượng nào đang được chọn.";
            }
        }

        // =========================================================
        // BỎ CHỌN ĐỐI TƯỢNG RIÊNG TRÊN 1 LỚP
        // =========================================================
        private void ClearLayerSelection(
            TreeNode node)
        {
            if (node == null ||
                IsGroupNode(node))
            {
                return;
            }

            string key =
                GetLayerKeyFromNode(node);

            int handle;

            if (!_loadedLayers.TryGetValue(
                key,
                out handle))
            {
                return;
            }

            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(handle);

                if (sf == null)
                    return;

                sf.SelectNone();

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã bỏ chọn đối tượng trên lớp: " +
                        node.Text;
                }
            }
            catch
            {
            }
        }

        // =========================================================
        // CHỌN NODE
        // =========================================================
        private void treeLayers_AfterSelect(
            object sender,
            TreeViewEventArgs e)
        {
            if (e == null ||
                e.Node == null)
            {
                return;
            }

            // -----------------------------------------------------
            // Nếu chọn nhóm thì không thay đổi "Lớp hiện tại".
            // -----------------------------------------------------
            if (IsGroupNode(e.Node))
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Nhóm lớp: " +
                        e.Node.Text;
                }

                return;
            }

            string key =
                GetLayerKeyFromNode(
                    e.Node);

            if (!string.IsNullOrWhiteSpace(key) &&
                cboCurrentLayer != null &&
                cboCurrentLayer.Items.Contains(key))
            {
                cboCurrentLayer.SelectedItem =
                    key;
            }

            UpdateCurrentLayerStatus();
        }

        // =========================================================
        // CHECKBOX BẬT / TẮT
        //
        // Quan trọng:
        // - Không tự Zoom.
        // - Chỉ Redraw 1 lần.
        // =========================================================
        private void treeLayers_AfterCheck(
            object sender,
            TreeViewEventArgs e)
        {
            if (_layerCheckInternalChange ||
                e == null ||
                e.Node == null)
            {
                return;
            }

            // -----------------------------------------------------
            // CHECK NHÓM
            // -----------------------------------------------------
            if (IsGroupNode(e.Node))
            {
                SetGroupVisibility(
                    e.Node,
                    e.Node.Checked);

                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        e.Node.Checked
                            ? "Đã bật các lớp trong nhóm: " +
                              e.Node.Text
                            : "Đã tắt các lớp trong nhóm: " +
                              e.Node.Text;
                }

                return;
            }

            // -----------------------------------------------------
            // CHECK LAYER
            //
            // Khi người dùng tick/bỏ tick một lớp, đồng thời chọn
            // chính lớp đó làm "Lớp hiện tại". Điều này tránh tình
            // trạng node nhóm "Dữ liệu PostGIS" vẫn đang được chọn,
            // khiến lệnh "Mở bảng thuộc tính" không biết phải mở lớp nào.
            // -----------------------------------------------------
            treeLayers.SelectedNode = e.Node;

            string checkedLayerKey =
                GetLayerKeyFromNode(
                    e.Node);

            if (!string.IsNullOrWhiteSpace(checkedLayerKey) &&
                cboCurrentLayer != null &&
                cboCurrentLayer.Items.Contains(checkedLayerKey))
            {
                cboCurrentLayer.SelectedItem =
                    checkedLayerKey;
            }

            bool success =
                SetLayerVisibilityInternal(
                    e.Node,
                    e.Node.Checked,
                    true);

            if (!success &&
                e.Node.Checked)
            {
                SetLayerNodeChecked(
                    e.Node,
                    false);
            }

            UpdateGroupCheckedState(
                e.Node.Parent);

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

            if (lblStatus != null)
            {
                lblStatus.Text =
                    e.Node.Checked
                        ? "Đã bật lớp: " +
                          e.Node.Text
                        : "Đã tắt lớp: " +
                          e.Node.Text;
            }
        }

        // =========================================================
        // THAY ĐỔI VISIBILITY CỦA 1 LAYER
        //
        // redraw = false ở đây để tránh redraw nhiều lần.
        // =========================================================
        private bool SetLayerVisibilityInternal(
            TreeNode node,
            bool visible,
            bool allowPostGISLoad)
        {
            if (node == null ||
                IsGroupNode(node))
            {
                return false;
            }

            string key =
                GetLayerKeyFromNode(node);

            if (string.IsNullOrWhiteSpace(key))
                return false;

            int handle;

            // -----------------------------------------------------
            // Layer đã được load
            // -----------------------------------------------------
            if (_loadedLayers.TryGetValue(
                key,
                out handle))
            {
                try
                {
                    axMap2.set_LayerVisible(
                        handle,
                        visible);

                    SetLayerNodeChecked(
                        node,
                        visible);

                    UpdateLayerNodeIcon(
                        node);

                    return true;
                }
                catch
                {
                    _loadedLayers.Remove(key);

                    SetLayerNodeChecked(
                        node,
                        false);

                    return false;
                }
            }

            // -----------------------------------------------------
            // PostGIS chưa load nhưng được bật.
            // -----------------------------------------------------
            if (visible &&
                allowPostGISLoad &&
                IsPostGISLayerNode(node))
            {
                bool loaded =
                    LoadPostGISLayer(
                        key,
                        false);

                SetLayerNodeChecked(
                    node,
                    loaded);

                if (loaded)
                {
                    UpdateLayerNodeIcon(
                        node);
                }

                return loaded;
            }

            // -----------------------------------------------------
            // Layer cục bộ đã mất handle thì không thể tự mở lại.
            // -----------------------------------------------------
            SetLayerNodeChecked(
                node,
                false);

            return !visible;
        }

        // =========================================================
        // BẬT MỘT LỚP / NHÓM
        // =========================================================
        private void ShowLayer(
            TreeNode node)
        {
            if (node == null)
                return;

            if (IsGroupNode(node))
            {
                SetGroupVisibility(
                    node,
                    true);

                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                return;
            }

            bool success =
                SetLayerVisibilityInternal(
                    node,
                    true,
                    true);

            if (success)
            {
                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                UpdateGroupCheckedState(
                    node.Parent);

                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã bật lớp: " +
                        node.Text;
                }
            }
        }

        // =========================================================
        // TẮT MỘT LỚP / NHÓM
        // =========================================================
        private void HideLayer(
            TreeNode node)
        {
            if (node == null)
                return;

            if (IsGroupNode(node))
            {
                SetGroupVisibility(
                    node,
                    false);

                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                return;
            }

            SetLayerVisibilityInternal(
                node,
                false,
                false);

            UpdateGroupCheckedState(
                node.Parent);

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Đã tắt lớp: " +
                    node.Text;
            }
        }

        // =========================================================
        // THAY ĐỔI VISIBILITY TOÀN BỘ LỚP TRONG 1 NHÓM
        // =========================================================
        private void SetGroupVisibility(
            TreeNode groupNode,
            bool visible)
        {
            if (groupNode == null ||
                !IsGroupNode(groupNode))
            {
                return;
            }

            bool allSucceeded = true;

            foreach (TreeNode child
                in groupNode.Nodes)
            {
                bool ok =
                    SetLayerVisibilityInternal(
                        child,
                        visible,
                        visible);

                if (!ok && visible)
                    allSucceeded = false;
            }

            SetLayerNodeChecked(
                groupNode,
                visible && allSucceeded);
        }

        // =========================================================
        // BẬT TẤT CẢ LỚP
        // =========================================================
        private void ShowAllLayers()
        {
            if (treeLayers == null ||
                treeLayers.Nodes.Count == 0)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Chưa có lớp dữ liệu.";
                }

                return;
            }

            foreach (TreeNode node
                in treeLayers.Nodes)
            {
                if (IsGroupNode(node))
                {
                    SetGroupVisibility(
                        node,
                        true);
                }
                else
                {
                    SetLayerVisibilityInternal(
                        node,
                        true,
                        true);
                }
            }

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Đã bật toàn bộ lớp.";
            }
        }

        // =========================================================
        // TẮT TẤT CẢ LỚP
        // =========================================================
        private void HideAllLayers()
        {
            if (treeLayers == null)
                return;

            foreach (TreeNode node
                in treeLayers.Nodes)
            {
                if (IsGroupNode(node))
                {
                    SetGroupVisibility(
                        node,
                        false);
                }
                else
                {
                    SetLayerVisibilityInternal(
                        node,
                        false,
                        false);
                }
            }

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Đã tắt toàn bộ lớp.";
            }
        }

        // =========================================================
        // THU PHÓNG TỚI LỚP
        // =========================================================
        private void ZoomToLayer(
            TreeNode node)
        {
            if (node == null)
                return;

            if (IsGroupNode(node))
            {
                MessageBox.Show(
                    "Hãy chọn một lớp dữ liệu cụ thể.",
                    "Chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string layerKey =
                GetLayerKeyFromNode(
                    node);

            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return;
            }

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                MessageBox.Show(
                    "Lớp chưa được tải lên bản đồ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                axMap2.ZoomToLayer(
                    handle);

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã thu phóng tới lớp: " +
                        node.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể thu phóng tới lớp.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // XÓA LỚP KHỎI BẢN ĐỒ
        // =========================================================
        private void RemoveSelectedLayer()
        {
            if (treeLayers == null)
                return;

            TreeNode node =
                treeLayers.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(
                    "Hãy chọn một lớp cần gỡ khỏi bản đồ.",
                    "Chưa chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (IsGroupNode(node))
            {
                MessageBox.Show(
                    "Đây là nhóm lớp.\n" +
                    "Hãy chọn một lớp dữ liệu cụ thể.",
                    "Không thể gỡ lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string layerKey =
                GetLayerKeyFromNode(
                    node);

            int handle;

            if (_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                try
                {
                    axMap2.RemoveLayer(
                        handle);
                }
                catch
                {
                }

                _loadedLayers.Remove(
                    layerKey);
            }

            string removedName =
                node.Text;

            TreeNode parent =
                node.Parent;

            node.Remove();

            RemoveGroupIfEmpty(
                parent);

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

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
                    "Đã gỡ lớp khỏi bản đồ: " +
                    removedName;
            }
        }

        // =========================================================
        // DOUBLE CLICK
        //
        // - Nhóm: Mở rộng / thu gọn
        // - Lớp: Thu phóng tới lớp
        // =========================================================
        private void treeLayers_NodeMouseDoubleClick(
            object sender,
            TreeNodeMouseClickEventArgs e)
        {
            if (e == null ||
                e.Node == null)
            {
                return;
            }

            treeLayers.SelectedNode =
                e.Node;

            if (IsGroupNode(
                e.Node))
            {
                e.Node.Toggle();
                return;
            }

            ZoomToLayer(
                e.Node);
        }

        // =========================================================
        // CHUỘT PHẢI
        // =========================================================
        private void treeLayers_NodeMouseClick(
            object sender,
            TreeNodeMouseClickEventArgs e)
        {
            if (e == null ||
                e.Node == null ||
                e.Button != MouseButtons.Right)
            {
                return;
            }

            treeLayers.SelectedNode =
                e.Node;

            BuildLayerContextMenu(
                e.Node);

            if (_layerContextMenu != null)
            {
                _layerContextMenu.Show(
                    treeLayers,
                    e.Location);
            }
        }

        // =========================================================
        // KHỞI TẠO MENU CHUỘT PHẢI
        // =========================================================
        private void CreateLayerContextMenu()
        {
            if (_layerContextMenu != null)
            {
                _layerContextMenu.Dispose();
                _layerContextMenu = null;
            }

            _layerContextMenu =
                new ContextMenuStrip();

            _layerContextMenu.Font =
                new Font(
                    "Segoe UI",
                    9F);
        }

        // =========================================================
        // TẠO MENU CHUỘT PHẢI THEO NODE
        // =========================================================
        private void BuildLayerContextMenu(
            TreeNode node)
        {
            if (_layerContextMenu == null)
                CreateLayerContextMenu();

            _layerContextMenu.Items.Clear();

            if (node == null)
                return;

            // -----------------------------------------------------
            // MENU CHO NHÓM
            // -----------------------------------------------------
            if (IsGroupNode(node))
            {
                ToolStripMenuItem itemExpand =
                    new ToolStripMenuItem(
                        node.IsExpanded
                            ? "Thu gọn nhóm"
                            : "Mở rộng nhóm");

                itemExpand.Click +=
                    delegate
                    {
                        if (node.IsExpanded)
                            node.Collapse();
                        else
                            node.Expand();
                    };

                ToolStripMenuItem itemShowGroup =
                    new ToolStripMenuItem(
                        "Bật toàn bộ dữ liệu trong nhóm");

                itemShowGroup.Click +=
                    delegate
                    {
                        SetGroupVisibility(
                            node,
                            true);

                        try
                        {
                            axMap2.Redraw();
                        }
                        catch
                        {
                        }

                        RefreshLayerCombo();
                        UpdateCurrentLayerStatus();
                    };

                ToolStripMenuItem itemHideGroup =
                    new ToolStripMenuItem(
                        "Tắt toàn bộ dữ liệu trong nhóm");

                itemHideGroup.Click +=
                    delegate
                    {
                        SetGroupVisibility(
                            node,
                            false);

                        try
                        {
                            axMap2.Redraw();
                        }
                        catch
                        {
                        }

                        RefreshLayerCombo();
                        UpdateCurrentLayerStatus();
                    };

                _layerContextMenu.Items.Add(
                    itemExpand);

                _layerContextMenu.Items.Add(
                    new ToolStripSeparator());

                _layerContextMenu.Items.Add(
                    itemShowGroup);

                _layerContextMenu.Items.Add(
                    itemHideGroup);

                return;
            }

            // -----------------------------------------------------
            // MENU CHO LAYER
            // -----------------------------------------------------
            ToolStripMenuItem itemZoom =
                new ToolStripMenuItem(
                    "Thu phóng tới lớp");

            itemZoom.Click +=
                delegate
                {
                    ZoomToLayer(node);
                };

            ToolStripMenuItem itemShow =
                new ToolStripMenuItem(
                    "Bật lớp");

            itemShow.Click +=
                delegate
                {
                    ShowLayer(node);
                };

            ToolStripMenuItem itemHide =
                new ToolStripMenuItem(
                    "Tắt lớp");

            itemHide.Click +=
                delegate
                {
                    HideLayer(node);
                };

            ToolStripMenuItem itemLookup =
                new ToolStripMenuItem(
                    "Tra cứu thông tin đối tượng");

            itemLookup.Click +=
                delegate
                {
                    ActivateLookupForLayer(
                        node);
                };

            ToolStripMenuItem itemAttributeTable =
                new ToolStripMenuItem(
                    "Mở bảng thuộc tính");

            itemAttributeTable.Click +=
                delegate
                {
                    OpenAttributeTable(
                        node);
                };

            ToolStripMenuItem itemClearSelection =
                new ToolStripMenuItem(
                    "Bỏ chọn đối tượng");

            itemClearSelection.Click +=
                delegate
                {
                    ClearLayerSelection(
                        node);
                };

            ToolStripMenuItem itemRemove =
                new ToolStripMenuItem(
                    "Gỡ lớp khỏi bản đồ");

            itemRemove.Click +=
                delegate
                {
                    treeLayers.SelectedNode =
                        node;

                    RemoveSelectedLayer();
                };

            _layerContextMenu.Items.Add(
                itemZoom);

            _layerContextMenu.Items.Add(
                new ToolStripSeparator());

            _layerContextMenu.Items.Add(
                itemShow);

            _layerContextMenu.Items.Add(
                itemHide);

            _layerContextMenu.Items.Add(
                new ToolStripSeparator());

            _layerContextMenu.Items.Add(
                itemLookup);

            _layerContextMenu.Items.Add(
                itemAttributeTable);

            _layerContextMenu.Items.Add(
                itemClearSelection);

            _layerContextMenu.Items.Add(
                new ToolStripSeparator());

            _layerContextMenu.Items.Add(
                itemRemove);
        }

        // =========================================================
        // TRA CỨU THÔNG TIN ĐỐI TƯỢNG TRÊN MỘT LỚP
        //
        // Đồng bộ lớp hiện tại rồi kích hoạt công cụ truy vấn vị trí
        // đã có trong Query.cs. Người dùng chỉ cần bấm đối tượng trên
        // bản đồ để xem thông tin.
        // =========================================================
        private void ActivateLookupForLayer(
            TreeNode node)
        {
            if (node == null ||
                IsGroupNode(node))
            {
                MessageBox.Show(
                    "Hãy chọn một lớp dữ liệu cụ thể.",
                    "Chưa chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string layerKey =
                GetLayerKeyFromNode(
                    node);

            if (string.IsNullOrWhiteSpace(
                layerKey) ||
                !_loadedLayers.ContainsKey(
                    layerKey))
            {
                MessageBox.Show(
                    "Lớp \"" +
                    node.Text +
                    "\" chưa được tải lên bản đồ.",
                    "Không thể tra cứu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            treeLayers.SelectedNode =
                node;

            if (cboCurrentLayer != null &&
                cboCurrentLayer.Items.Contains(
                    layerKey))
            {
                cboCurrentLayer.SelectedItem =
                    layerKey;
            }

            try
            {
                btnQueryLocation_Click(
                    this,
                    EventArgs.Empty);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Tra cứu \"" +
                        node.Text +
                        "\": bấm vào đối tượng trên bản đồ.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể kích hoạt công cụ tra cứu.\n\n" +
                    ex.Message,
                    "Lỗi tra cứu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // MỞ BẢNG THUỘC TÍNH
        // =========================================================
        private void OpenAttributeTable(
            TreeNode node)
        {
            // -----------------------------------------------------
            // 1. Nếu node hiện tại là NHÓM hoặc chưa chọn node,
            // thử lấy lớp từ Combo "Lớp hiện tại".
            // -----------------------------------------------------
            if (node == null ||
                IsGroupNode(node))
            {
                string currentLayerKey =
                    cboCurrentLayer != null
                        ? cboCurrentLayer.SelectedItem as string
                        : null;

                if (!string.IsNullOrWhiteSpace(
                    currentLayerKey))
                {
                    node =
                        FindLayerNodeRecursive(
                            treeLayers.Nodes,
                            currentLayerKey);
                }
            }

            // -----------------------------------------------------
            // 2. Nếu vẫn chưa xác định được layer thật thì báo rõ.
            // -----------------------------------------------------
            if (node == null ||
                IsGroupNode(node))
            {
                MessageBox.Show(
                    "Hãy chọn một lớp dữ liệu cụ thể như " +
                    "\"Ranh giới Thành phố Hà Nội\", " +
                    "\"Xã, phường Hà Nội\", " +
                    "\"Đơn vị quân đội\", " +
                    "\"Tuyến đường đơn vị\" hoặc " +
                    "\"Khu vực đơn vị\".",
                    "Chưa chọn lớp dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string layerKey =
                GetLayerKeyFromNode(
                    node);

            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return;
            }

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                MessageBox.Show(
                    "Lớp \"" +
                    node.Text +
                    "\" chưa được tải lên bản đồ.",
                    "Không thể mở bảng thuộc tính",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(
                        handle);

                if (sf == null)
                {
                    MessageBox.Show(
                        "Không lấy được lớp dữ liệu \"" +
                        node.Text +
                        "\" từ MapWinGIS.",
                        "Không thể mở bảng thuộc tính",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                // Đồng bộ trạng thái trước khi mở.
                treeLayers.SelectedNode =
                    node;

                if (cboCurrentLayer != null &&
                    cboCurrentLayer.Items.Contains(
                        layerKey))
                {
                    cboCurrentLayer.SelectedItem =
                        layerKey;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang mở bảng thuộc tính: " +
                        node.Text +
                        "...";
                }

                using (AttributeTableForm form =
                    new AttributeTableForm(
                        axMap2,
                        handle,
                        layerKey,
                        node.Text,
                        RefreshPostGISLayerAfterEdit))
                {
                    DialogResult result =
                        form.ShowDialog(
                            this);
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã đóng bảng thuộc tính: " +
                        node.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể mở bảng thuộc tính của lớp \"" +
                    node.Text +
                    "\".\n\n" +
                    ex.Message,
                    "Lỗi mở bảng thuộc tính",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Mở bảng thuộc tính thất bại.";
                }
            }
        }

        // =========================================================
        // REFRESH RIÊNG MỘT LỚP POSTGIS SAU KHI SỬA/XÓA
        //
        // Chỉ reload lớp vừa thay đổi -> nhanh và mượt hơn.
        // =========================================================
        private void RefreshPostGISLayerAfterEdit(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(layerKey) ||
                !IsManagedPostGISTable(layerKey))
            {
                return;
            }

            int oldHandle;
            bool wasVisible = true;

            if (_loadedLayers.TryGetValue(
                layerKey,
                out oldHandle))
            {
                try
                {
                    wasVisible =
                        axMap2.get_LayerVisible(
                            oldHandle);
                }
                catch
                {
                }

                try
                {
                    axMap2.RemoveLayer(
                        oldHandle);
                }
                catch
                {
                }

                _loadedLayers.Remove(
                    layerKey);
            }

            bool loaded =
                LoadPostGISLayer(
                    layerKey,
                    false);

            TreeNode node =
                FindLayerNodeRecursive(
                    treeLayers.Nodes,
                    layerKey);

            if (node != null)
            {
                if (loaded &&
                    !wasVisible)
                {
                    int newHandle;

                    if (_loadedLayers.TryGetValue(
                        layerKey,
                        out newHandle))
                    {
                        try
                        {
                            axMap2.set_LayerVisible(
                                newHandle,
                                false);
                        }
                        catch
                        {
                        }
                    }
                }

                SetLayerNodeChecked(
                    node,
                    loaded && wasVisible);
            }

            RefreshLayerCombo();
            UpdateCurrentLayerStatus();

            try
            {
                axMap2.Redraw();
            }
            catch
            {
            }
        }

        // =========================================================
        // TÌM NODE LAYER ĐỆ QUY
        // =========================================================
        private TreeNode FindLayerNodeRecursive(
            TreeNodeCollection nodes,
            string layerKey)
        {
            if (nodes == null)
                return null;

            foreach (TreeNode node
                in nodes)
            {
                if (!IsGroupNode(node))
                {
                    string key =
                        GetLayerKeyFromNode(
                            node);

                    if (string.Equals(
                        key,
                        layerKey,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return node;
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    TreeNode found =
                        FindLayerNodeRecursive(
                            node.Nodes,
                            layerKey);

                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        // =========================================================
        // GOM CÁC NODE ĐANG CÓ VÀO NHÓM
        // =========================================================
        private void OrganizeExistingLayerNodes()
        {
            if (treeLayers == null)
                return;

            List<TreeNode> layerNodes =
                new List<TreeNode>();

            foreach (TreeNode node
                in treeLayers.Nodes)
            {
                if (!IsGroupNode(node))
                    layerNodes.Add(node);
            }

            if (layerNodes.Count == 0)
                return;

            treeLayers.BeginUpdate();

            try
            {
                foreach (TreeNode node
                    in layerNodes)
                {
                    EnsureNodeGrouped(
                        node);

                    UpdateLayerNodeIcon(
                        node);
                }

                UpdateAllGroupCheckedStates();
            }
            finally
            {
                treeLayers.EndUpdate();
            }
        }

        // =========================================================
        // ĐƯA 1 LAYER VÀO ĐÚNG NHÓM
        //
        // Hàm này được gọi từ SetLayerNodeChecked().
        // Vì PostGIS.cs và Navigation.cs hiện tại đều gọi hàm đó
        // sau khi tạo node nên không cần sửa 2 file kia.
        // =========================================================
        private void EnsureNodeGrouped(
            TreeNode node)
        {
            if (node == null ||
                IsGroupNode(node))
            {
                return;
            }

            TreeNode targetGroup = null;

            if (IsPostGISLayerNode(node))
            {
                targetGroup =
                    GetOrCreatePostGISGroup();
            }
            else if (IsFileLayerNode(node))
            {
                targetGroup =
                    GetOrCreateLocalGroup();
            }
            else
            {
                // Node cũ chưa có Tag:
                // nếu key có trong danh sách layer đang load
                // thì mặc định xem là lớp cục bộ.
                string key =
                    GetLayerKeyFromNode(node);

                if (!string.IsNullOrWhiteSpace(key) &&
                    _loadedLayers.ContainsKey(key))
                {
                    targetGroup =
                        GetOrCreateLocalGroup();

                    node.Tag =
                        NODE_TAG_FILE;
                }
            }

            if (targetGroup == null)
                return;

            // -----------------------------------------------------
            // Nếu đã đúng nhóm thì chỉ cập nhật icon.
            // -----------------------------------------------------
            if (node.Parent == targetGroup)
            {
                UpdateLayerNodeIcon(node);
                return;
            }

            // -----------------------------------------------------
            // Xóa node cũ trùng key trong group.
            //
            // Cần thiết vì PostGIS.cs cũ chỉ xóa node ở root.
            // Khi dùng group, lần tải PostGIS sau có thể node cũ
            // vẫn nằm trong group.
            // -----------------------------------------------------
            string keyToAdd =
                GetLayerKeyFromNode(node);

            for (
                int i = targetGroup.Nodes.Count - 1;
                i >= 0;
                i--)
            {
                TreeNode oldNode =
                    targetGroup.Nodes[i];

                if (ReferenceEquals(
                    oldNode,
                    node))
                {
                    continue;
                }

                string oldKey =
                    GetLayerKeyFromNode(
                        oldNode);

                if (string.Equals(
                    oldKey,
                    keyToAdd,
                    StringComparison.OrdinalIgnoreCase))
                {
                    targetGroup.Nodes.RemoveAt(i);
                }
            }

            // -----------------------------------------------------
            // Di chuyển node vào group.
            // -----------------------------------------------------
            if (node.Parent != null)
            {
                node.Remove();
            }
            else if (treeLayers.Nodes.Contains(node))
            {
                treeLayers.Nodes.Remove(node);
            }

            targetGroup.Nodes.Add(
                node);

            targetGroup.Expand();

            UpdateLayerNodeIcon(
                node);

            UpdateGroupCheckedState(
                targetGroup);
        }

        // =========================================================
        // TẠO / LẤY NHÓM POSTGIS
        // =========================================================
        private TreeNode GetOrCreatePostGISGroup()
        {
            TreeNode group =
                FindRootNodeByName(
                    GROUP_POSTGIS_NAME);

            if (group != null)
                return group;

            group =
                new TreeNode(
                    "DỮ LIỆU GIS - HÀ NỘI");

            group.Name =
                GROUP_POSTGIS_NAME;

            group.Tag =
                NODE_TAG_GROUP_POSTGIS;

            group.ImageKey =
                "group";

            group.SelectedImageKey =
                "group";

            // Nhóm dữ liệu GIS Hà Nội luôn đứng trên dữ liệu bổ sung.
            treeLayers.Nodes.Insert(
                0,
                group);

            group.Expand();

            return group;
        }

        // =========================================================
        // TẠO / LẤY NHÓM DỮ LIỆU CỤC BỘ
        // =========================================================
        private TreeNode GetOrCreateLocalGroup()
        {
            TreeNode group =
                FindRootNodeByName(
                    GROUP_LOCAL_NAME);

            if (group != null)
                return group;

            group =
                new TreeNode(
                    "Dữ liệu bổ sung");

            group.Name =
                GROUP_LOCAL_NAME;

            group.Tag =
                NODE_TAG_GROUP_LOCAL;

            group.ImageKey =
                "group";

            group.SelectedImageKey =
                "group";

            treeLayers.Nodes.Add(
                group);

            group.Expand();

            return group;
        }

        // =========================================================
        // TÌM ROOT NODE THEO NAME
        // =========================================================
        private TreeNode FindRootNodeByName(
            string name)
        {
            if (treeLayers == null ||
                string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (TreeNode node
                in treeLayers.Nodes)
            {
                if (string.Equals(
                    node.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }

            return null;
        }

        // =========================================================
        // XÓA GROUP NẾU KHÔNG CÒN LAYER
        // =========================================================
        private void RemoveGroupIfEmpty(
            TreeNode group)
        {
            if (group == null ||
                !IsGroupNode(group))
            {
                return;
            }

            if (group.Nodes.Count == 0)
            {
                group.Remove();
            }
        }

        // =========================================================
        // CẬP NHẬT CHECK CỦA NHÓM
        //
        // TreeView WinForms không có trạng thái "nửa check".
        // Vì vậy:
        // - Tất cả con bật -> group checked
        // - Chỉ cần 1 lớp tắt -> group unchecked
        // =========================================================
        private void UpdateGroupCheckedState(
            TreeNode group)
        {
            if (group == null ||
                !IsGroupNode(group))
            {
                return;
            }

            if (group.Nodes.Count == 0)
            {
                SetLayerNodeChecked(
                    group,
                    false);

                return;
            }

            bool allChecked = true;

            foreach (TreeNode child
                in group.Nodes)
            {
                if (!child.Checked)
                {
                    allChecked = false;
                    break;
                }
            }

            SetLayerNodeChecked(
                group,
                allChecked);
        }

        // =========================================================
        // CẬP NHẬT CHECK TẤT CẢ GROUP
        // =========================================================
        private void UpdateAllGroupCheckedStates()
        {
            foreach (TreeNode node
                in treeLayers.Nodes)
            {
                if (IsGroupNode(node))
                {
                    UpdateGroupCheckedState(
                        node);
                }
            }
        }

        // =========================================================
        // ICON CHO 1 LAYER
        // =========================================================
        private void UpdateLayerNodeIcon(
            TreeNode node)
        {
            if (node == null)
                return;

            if (IsGroupNode(node))
            {
                node.ImageKey =
                    "group";

                node.SelectedImageKey =
                    "group";

                return;
            }

            string iconKey =
                GetLayerGeometryIconKey(
                    node);

            node.ImageKey =
                iconKey;

            node.SelectedImageKey =
                iconKey;
        }

        // =========================================================
        // XÁC ĐỊNH LOẠI HÌNH HỌC
        // =========================================================
        private string GetLayerGeometryIconKey(
            TreeNode node)
        {
            if (node == null)
                return "layer";

            string key =
                GetLayerKeyFromNode(
                    node);

            int handle;

            if (!string.IsNullOrWhiteSpace(key) &&
                _loadedLayers.TryGetValue(
                    key,
                    out handle))
            {
                try
                {
                    Shapefile sf =
                        axMap2.get_Shapefile(
                            handle);

                    if (sf != null)
                    {
                        int typeCode =
                            (int)sf.ShapefileType;

                        int baseType =
                            typeCode % 10;

                        // SHP_POINT / POINTZ / POINTM
                        if (baseType == 1)
                            return "point";

                        // SHP_POLYLINE / Z / M
                        if (baseType == 3)
                            return "line";

                        // SHP_POLYGON / Z / M
                        if (baseType == 5)
                            return "polygon";

                        // MULTIPOINT
                        if (baseType == 8)
                            return "point";
                    }
                }
                catch
                {
                }
            }

            // -----------------------------------------------------
            // Fallback theo tên bảng PostGIS của đề tài.
            // -----------------------------------------------------
            if (string.Equals(
                key,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "point";
            }

            if (string.Equals(
                key,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "line";
            }

            if (string.Equals(
                key,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "polygon";
            }

            if (string.Equals(
                key,
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return "polygon";
            }

            if (string.Equals(
                key,
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "polygon";
            }

            // -----------------------------------------------------
            // Fallback cho các lớp cục bộ cũ nếu người dùng vẫn mở.
            // -----------------------------------------------------
            if (string.Equals(
                key,
                "point",
                StringComparison.OrdinalIgnoreCase))
            {
                return "point";
            }

            if (string.Equals(
                key,
                "duong_moi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "line";
            }

            if (string.Equals(
                key,
                "vung_moi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "polygon";
            }

            return "layer";
        }

        // =========================================================
        // LẤY KEY CỦA LAYER
        // =========================================================
        private string GetLayerKeyFromNode(
            TreeNode node)
        {
            if (node == null)
                return null;

            if (!string.IsNullOrWhiteSpace(
                node.Name))
            {
                return node.Name;
            }

            return node.Text;
        }

        // =========================================================
        // NODE CÓ PHẢI GROUP KHÔNG
        // =========================================================
        private bool IsGroupNode(
            TreeNode node)
        {
            if (node == null ||
                node.Tag == null)
            {
                return false;
            }

            string tag =
                node.Tag.ToString();

            return
                string.Equals(
                    tag,
                    NODE_TAG_GROUP_POSTGIS,
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    tag,
                    NODE_TAG_GROUP_LOCAL,
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // NODE POSTGIS
        // =========================================================
        private bool IsPostGISLayerNode(
            TreeNode node)
        {
            return
                node != null &&
                node.Tag != null &&
                string.Equals(
                    node.Tag.ToString(),
                    NODE_TAG_POSTGIS,
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // NODE FILE CỤC BỘ
        // =========================================================
        private bool IsFileLayerNode(
            TreeNode node)
        {
            return
                node != null &&
                node.Tag != null &&
                string.Equals(
                    node.Tag.ToString(),
                    NODE_TAG_FILE,
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // ĐỔI CHECKBOX MÀ KHÔNG KÍCH HOẠT AFTERCHECK
        //
        // Đồng thời:
        // - Tự đưa layer về đúng group.
        // - Tự cập nhật icon.
        //
        // PostGIS.cs và Navigation.cs đang gọi hàm này nên
        // việc phân nhóm diễn ra tự động mà không cần sửa 2 file đó.
        // =========================================================
        private void SetLayerNodeChecked(
            TreeNode node,
            bool value)
        {
            if (node == null)
                return;

            bool oldState =
                _layerCheckInternalChange;

            _layerCheckInternalChange =
                true;

            try
            {
                // ---------------------------------------------
                // Chỉ layer thật mới được tự động đưa vào group.
                // ---------------------------------------------
                if (!IsGroupNode(node))
                {
                    EnsureNodeGrouped(
                        node);

                    UpdateLayerNodeIcon(
                        node);
                }

                node.Checked =
                    value;
            }
            finally
            {
                _layerCheckInternalChange =
                    oldState;
            }

            if (node.Parent != null &&
                IsGroupNode(node.Parent))
            {
                UpdateGroupCheckedState(
                    node.Parent);
            }
        }
    }
}