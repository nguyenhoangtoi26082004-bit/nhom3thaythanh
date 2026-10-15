using System;
using System.Data;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // FORM TRUY VẤN KHÔNG GIAN
    //
    // Dùng cho dữ liệu học tập/mô phỏng của đề tài.
    // =============================================================
    public partial class SpatialQueryForm : Form
    {
        private sealed class QueryOption
        {
            public string Code { get; private set; }
            public string Text { get; private set; }

            public QueryOption(
                string code,
                string text)
            {
                Code = code;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private sealed class AreaOption
        {
            public int Id { get; private set; }
            public string Text { get; private set; }

            public AreaOption(
                int id,
                string text)
            {
                Id = id;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private readonly AxMapWinGIS.AxMap _map;
        private readonly int _unitLayerHandle;
        private readonly int _routeLayerHandle;
        private readonly int _areaLayerHandle;

        private readonly SpatialQueryService _service;

        private DataTable _resultTable;

        public SpatialQueryForm(
            AxMapWinGIS.AxMap map,
            int unitLayerHandle,
            int routeLayerHandle,
            int areaLayerHandle)
        {
            InitializeComponent();

            _map =
                map;

            _unitLayerHandle =
                unitLayerHandle;

            _routeLayerHandle =
                routeLayerHandle;

            _areaLayerHandle =
                areaLayerHandle;

            _service =
                new SpatialQueryService();

            Text =
                "Truy vấn không gian - Hà Nội | EPSG:32648";

            Load +=
                SpatialQueryForm_Load;
        }

        // =========================================================
        // LOAD
        // =========================================================
        private void SpatialQueryForm_Load(
            object sender,
            EventArgs e)
        {
            try
            {
                UpdateRuntimeLabels();
                LoadQueryTypes();
                LoadAdministrativeAreas();
                UpdateParameterVisibility();

                lblStatus.Text =
                    "Chọn loại truy vấn rồi bấm Thực hiện.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi tạo truy vấn không gian.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // LOẠI TRUY VẤN
        // =========================================================
        private void LoadQueryTypes()
        {
            cboQueryType.Items.Clear();

            cboQueryType.Items.Add(
                new QueryOption(
                    "units_in_admin",
                    "Đơn vị theo xã/phường"));

            cboQueryType.Items.Add(
                new QueryOption(
                    "unit_boundary_check",
                    "Kiểm tra Point đại diện - xã/phường"));

            cboQueryType.Items.Add(
                new QueryOption(
                    "routes_boundary",
                    "Kiểm tra geometry tuyến"));

            cboQueryType.Items.Add(
                new QueryOption(
                    "missing_geometry",
                    "Đối tượng thiếu geometry"));

            cboQueryType.Items.Add(
                new QueryOption(
                    "invalid_geometry",
                    "Geometry lỗi / sai SRID"));

            if (cboQueryType.Items.Count > 0)
            {
                cboQueryType.SelectedIndex =
                    0;
            }
        }

        // =========================================================
        // ĐỊA GIỚI
        // =========================================================
        private void LoadAdministrativeAreas()
        {
            cboAdministrativeArea.Items.Clear();

            DataTable table =
                _service.GetAdministrativeAreas();

            foreach (DataRow row
                in table.Rows)
            {
                int id =
                    Convert.ToInt32(
                        row["id"]);

                string name =
                    row["ten_hanh_chinh"] == DBNull.Value
                        ? ""
                        : row["ten_hanh_chinh"].ToString();

                string display =
                    name;

                cboAdministrativeArea.Items.Add(
                    new AreaOption(
                        id,
                        display));
            }

            if (cboAdministrativeArea.Items.Count > 0)
            {
                cboAdministrativeArea.SelectedIndex =
                    0;
            }
        }

        // =========================================================
        // THAY LOẠI TRUY VẤN
        // =========================================================
        private void cboQueryType_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            UpdateParameterVisibility();
            ClearResults();
        }

        private void UpdateParameterVisibility()
        {
            QueryOption option =
                cboQueryType.SelectedItem
                as QueryOption;

            bool needAdministrativeArea =
                option != null &&
                option.Code ==
                "units_in_admin";

            lblAdministrativeArea.Visible =
                needAdministrativeArea;

            cboAdministrativeArea.Visible =
                needAdministrativeArea;

            if (option == null)
            {
                lblDescription.Text =
                    "";
                return;
            }

            if (option.Code ==
                "units_in_admin")
            {
                lblDescription.Text =
                    "Lọc đơn vị theo xã/phường và kiểm tra Point đại diện hành chính có khớp polygon xã/phường đã gán.";
            }
            else if (option.Code ==
                "unit_boundary_check")
            {
                lblDescription.Text =
                    "Kiểm tra tính nhất quán giữa xa_phuong_id và Point đại diện hành chính của từng đơn vị.";
            }
            else if (option.Code ==
                "routes_boundary")
            {
                lblDescription.Text =
                    "Kiểm tra geometry tuyến: có dữ liệu, không rỗng, đúng EPSG:32648 và hợp lệ.";
            }
            else if (option.Code ==
                "missing_geometry")
            {
                lblDescription.Text =
                    "Tìm các bản ghi Point/LineString/Polygon chưa có geometry hoặc geometry rỗng.";
            }
            else if (option.Code ==
                "invalid_geometry")
            {
                lblDescription.Text =
                    "Tìm geometry rỗng, sai SRID hoặc không hợp lệ bằng ST_IsValid/ST_IsValidReason.";
            }
        }

        // =========================================================
        // THỰC HIỆN
        // =========================================================
        private void btnExecute_Click(
            object sender,
            EventArgs e)
        {
            ExecuteSelectedQuery();
        }

        private void ExecuteSelectedQuery()
        {
            QueryOption option =
                cboQueryType.SelectedItem
                as QueryOption;

            if (option == null)
                return;

            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                lblStatus.Text =
                    "Đang thực hiện truy vấn không gian...";

                DisposeResultTable();

                if (option.Code ==
                    "units_in_admin")
                {
                    AreaOption area =
                        cboAdministrativeArea.SelectedItem
                        as AreaOption;

                    if (area == null)
                    {
                        MessageBox.Show(
                            "Danh sách xã/phường chưa có dữ liệu để lựa chọn.",
                            "Chưa có xã/phường",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    _resultTable =
                        _service.GetUnitsInAdministrativeArea(
                            area.Id);
                }
                else if (option.Code ==
                    "unit_boundary_check")
                {
                    _resultTable =
                        _service.GetUnitBoundaryCheck();
                }
                else if (option.Code ==
                    "routes_boundary")
                {
                    _resultTable =
                        _service.GetRoutesIntersectingOwnBoundary();
                }
                else if (option.Code ==
                    "missing_geometry")
                {
                    _resultTable =
                        _service.GetObjectsWithoutGeometry();
                }
                else if (option.Code ==
                    "invalid_geometry")
                {
                    _resultTable =
                        _service.GetInvalidGeometries();
                }

                gridResults.AutoGenerateColumns =
                    true;

                gridResults.DataSource =
                    _resultTable;

                ConfigureGrid();

                int count =
                    _resultTable != null
                        ? _resultTable.Rows.Count
                        : 0;

                lblResultCount.Text =
                    "Kết quả: " +
                    count +
                    " đối tượng";

                lblStatus.Text =
                    "Truy vấn hoàn tất.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể thực hiện truy vấn không gian.\n\n" +
                    ex.Message,
                    "Lỗi truy vấn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Truy vấn thất bại.";
            }
            finally
            {
                Cursor.Current =
                    oldCursor;
            }
        }

        // =========================================================
        // GRID
        // =========================================================
        private void ConfigureGrid()
        {
            // ---------------------------------------------
            // CỘT KỸ THUẬT
            // ---------------------------------------------
            HideColumn(
                "layer_key");

            if (gridResults.Columns.Contains(
                "object_id"))
            {
                gridResults.Columns[
                    "object_id"].HeaderText =
                    "ID";

                gridResults.Columns[
                    "object_id"].Width =
                    55;
            }

            // ---------------------------------------------
            // ĐƠN VỊ
            // ---------------------------------------------
            SetColumn(
                "ma_don_vi",
                "Mã đơn vị",
                105);

            SetColumn(
                "ten_don_vi",
                "Tên đơn vị",
                210);

            SetColumn(
                "ky_hieu_bien_so",
                "Ký hiệu",
                105);

            SetColumn(
                "ten_cap",
                "Cấp đơn vị",
                135);

            SetColumn(
                "ten_loai",
                "Loại đơn vị",
                145);

            SetColumn(
                "ten_xa_phuong",
                "Xã/Phường",
                165);

            SetColumn(
                "loai_xa_phuong",
                "Loại",
                85);

            SetColumn(
                "dia_chi_hien_hanh",
                "Địa chỉ hiện hành",
                210);

            SetColumn(
                "trang_thai_quan_ly",
                "Trạng thái",
                125);

            SetColumn(
                "ket_qua_khong_gian",
                "Kiểm tra không gian",
                190);

            // ---------------------------------------------
            // KIỂM TRA POINT ĐẠI DIỆN
            // ---------------------------------------------
            SetColumn(
                "ket_qua",
                "Kết quả",
                210);

            SetColumn(
                "so_ranh_gioi",
                "Geometry xã/phường",
                125);

            // ---------------------------------------------
            // TUYẾN
            // ---------------------------------------------
            SetColumn(
                "ma_tuyen",
                "Mã tuyến",
                110);

            SetColumn(
                "ten_tuyen",
                "Tên tuyến",
                190);

            SetColumn(
                "loai_tuyen",
                "Loại tuyến",
                135);

            SetColumn(
                "chieu_dai_m",
                "Chiều dài (m)",
                105);

            // Service mới giữ cột này để tương thích form cũ,
            // nhưng không còn dùng phân tích ranh giới cơ sở.
            HideColumn(
                "ranh_gioi_lien_quan");

            // ---------------------------------------------
            // KIỂM TRA GEOMETRY
            // ---------------------------------------------
            SetColumn(
                "loai_doi_tuong",
                "Loại đối tượng",
                145);

            SetColumn(
                "ma_doi_tuong",
                "Mã đối tượng",
                120);

            SetColumn(
                "ten_doi_tuong",
                "Tên đối tượng",
                210);

            SetColumn(
                "ly_do",
                "Lý do",
                260);

            // Các cột cũ nếu có từ service phiên bản trước thì ẩn.
            HideColumn(
                "dia_ban");

            HideColumn(
                "ten_hanh_chinh");

            HideColumn(
                "cap_hanh_chinh");
        }

        private void SetColumn(
            string name,
            string header,
            int width)
        {
            if (!gridResults.Columns.Contains(
                name))
            {
                return;
            }

            gridResults.Columns[name].HeaderText =
                header;

            gridResults.Columns[name].Width =
                width;
        }

        private void HideColumn(
            string name)
        {
            if (!gridResults.Columns.Contains(
                name))
            {
                return;
            }

            gridResults.Columns[name].Visible =
                false;
        }

        private void gridResults_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 ||
                e.ColumnIndex >=
                    gridResults.Columns.Count)
            {
                return;
            }

            string field =
                gridResults.Columns[
                    e.ColumnIndex]
                    .DataPropertyName ?? "";

            if (field ==
                "trang_thai_quan_ly")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyStatus(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (field ==
                "loai_tuyen")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyRouteType(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (field ==
                "loai_xa_phuong")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyAdministrativeType(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (field ==
                "so_ranh_gioi")
            {
                int value =
                    0;

                if (e.Value != null &&
                    e.Value != DBNull.Value)
                {
                    int.TryParse(
                        e.Value.ToString(),
                        out value);
                }

                e.Value =
                    value > 0
                        ? "Có"
                        : "Không";

                e.FormattingApplied =
                    true;
            }
        }

        // =========================================================
        // CHỌN / ZOOM
        // =========================================================
        private void gridResults_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            SelectCurrentObject(
                true);
        }

        private void btnSelectOnMap_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentObject(
                false);
        }

        private void btnZoomToObject_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentObject(
                true);
        }

        private void SelectCurrentObject(
            bool zoom)
        {
            DataRowView row =
                GetCurrentRow();

            if (row == null)
            {
                MessageBox.Show(
                    "Hãy chọn một dòng kết quả.",
                    "Chưa chọn đối tượng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!row.Row.Table.Columns.Contains(
                "layer_key")
                ||
                !row.Row.Table.Columns.Contains(
                    "object_id"))
            {
                return;
            }

            string layerKey =
                row["layer_key"] == DBNull.Value
                    ? ""
                    : row["layer_key"].ToString();

            int objectId;

            if (!int.TryParse(
                row["object_id"].ToString(),
                out objectId))
            {
                return;
            }

            int layerHandle =
                GetLayerHandle(
                    layerKey);

            if (layerHandle < 0)
            {
                MessageBox.Show(
                    "Lớp \"" +
                    GetLayerDisplayName(
                        layerKey) +
                    "\" chưa được tải trên bản đồ.\n\n" +
                    "Hãy nhấn Kết nối PostGIS trước.",
                    "Chưa tải lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool success =
                AttributeTableService
                    .SelectShapeByDatabaseId(
                        _map,
                        layerHandle,
                        objectId,
                        zoom);

            if (!success)
            {
                MessageBox.Show(
                    "Không thể chọn đối tượng trên bản đồ. " +
                    "Đối tượng có thể chưa có geometry.",
                    "Không thể định vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (string.Equals(
                layerKey,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.Text =
                    zoom
                        ? "Đã thu phóng tới Point đại diện hành chính của đơn vị."
                        : "Đã chọn Point đại diện hành chính trên bản đồ.";
            }
            else
            {
                lblStatus.Text =
                    zoom
                        ? "Đã thu phóng tới đối tượng."
                        : "Đã chọn đối tượng trên bản đồ.";
            }
        }

        private int GetLayerHandle(
            string layerKey)
        {
            if (string.Equals(
                layerKey,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return _unitLayerHandle;
            }

            if (string.Equals(
                layerKey,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return _routeLayerHandle;
            }

            if (string.Equals(
                layerKey,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return _areaLayerHandle;
            }

            return -1;
        }

        private string GetLayerDisplayName(
            string layerKey)
        {
            if (layerKey ==
                "don_vi_quan_doi")
            {
                return "Đơn vị quân đội";
            }

            if (layerKey ==
                "tuyen_duong_don_vi")
            {
                return "Tuyến đường đơn vị";
            }

            if (layerKey ==
                "khu_vuc_don_vi")
            {
                return "Khu vực đơn vị";
            }

            return layerKey;
        }

        private DataRowView GetCurrentRow()
        {
            if (gridResults.CurrentRow == null)
                return null;

            return
                gridResults.CurrentRow
                    .DataBoundItem
                    as DataRowView;
        }

        // =========================================================
        // NHÃN GIAO DIỆN HÀ NỘI-ONLY
        //
        // Không sửa Designer.cs ở bước này.
        // =========================================================
        private void UpdateRuntimeLabels()
        {
            Text =
                "Truy vấn không gian - Hà Nội | EPSG:32648";

            if (lblAdministrativeArea != null)
            {
                lblAdministrativeArea.Text =
                    "Xã/Phường:";
            }
        }

        // =========================================================
        // LÀM MỚI / ĐÓNG
        // =========================================================
        private void btnRefresh_Click(
            object sender,
            EventArgs e)
        {
            LoadAdministrativeAreas();
            ExecuteSelectedQuery();
        }

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void ClearResults()
        {
            gridResults.DataSource =
                null;

            DisposeResultTable();

            lblResultCount.Text =
                "Kết quả: 0 đối tượng";
        }

        private void DisposeResultTable()
        {
            if (_resultTable != null)
            {
                _resultTable.Dispose();
                _resultTable = null;
            }
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                DisposeResultTable();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(
                disposing);
        }
    }
}