using System;
using System.Data;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class UnitSearchForm : Form
    {
        private sealed class SearchOption
        {
            public object Value { get; private set; }
            public string Text { get; private set; }

            public SearchOption(object value, string text)
            {
                Value = value;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private readonly AxMapWinGIS.AxMap _map;
        private readonly int _unitLayerHandle;
        private readonly UnitSearchService _service;
        private readonly string _initialKeyword;

        private DataTable _resultTable;

        public UnitSearchForm(
            AxMapWinGIS.AxMap map,
            int unitLayerHandle)
            : this(
                map,
                unitLayerHandle,
                "")
        {
        }

        // =========================================================
        // CONSTRUCTOR DÙNG CHO TRA CỨU NHANH
        // =========================================================
        public UnitSearchForm(
            AxMapWinGIS.AxMap map,
            int unitLayerHandle,
            string initialKeyword)
        {
            InitializeComponent();

            _map = map;
            _unitLayerHandle = unitLayerHandle;
            _service = new UnitSearchService();
            _initialKeyword = initialKeyword ?? "";

            Text =
                "Tra cứu đơn vị quân đội - Hà Nội";

            Load -= UnitSearchForm_Load;
            Load += UnitSearchForm_Load;
        }

        private void UnitSearchForm_Load(
            object sender,
            EventArgs e)
        {
            try
            {
                UpdateRuntimeLabels();
                LoadFilters();

                if (!string.IsNullOrWhiteSpace(
                    _initialKeyword))
                {
                    txtKeyword.Text =
                        _initialKeyword.Trim();

                    txtKeyword.SelectionStart =
                        txtKeyword.Text.Length;
                }

                ExecuteSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi tạo chức năng tra cứu.\n\n" +
                    ex.Message,
                    "Lỗi tra cứu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadFilters()
        {
            LoadCapDonVi();
            LoadLoaiDonVi();
            LoadXaPhuong();
            LoadTrangThai();
        }

        private void LoadCapDonVi()
        {
            cboCapDonVi.Items.Clear();
            cboCapDonVi.Items.Add(
                new SearchOption(
                    null,
                    "Tất cả cấp đơn vị"));

            DataTable table =
                _service.GetCapDonVi();

            foreach (DataRow row in table.Rows)
            {
                cboCapDonVi.Items.Add(
                    new SearchOption(
                        Convert.ToInt32(row["id"]),
                        row["ten_cap"].ToString()));
            }

            if (cboCapDonVi.Items.Count > 0)
                cboCapDonVi.SelectedIndex = 0;
        }

        private void LoadLoaiDonVi()
        {
            cboLoaiDonVi.Items.Clear();
            cboLoaiDonVi.Items.Add(
                new SearchOption(
                    null,
                    "Tất cả loại đơn vị"));

            DataTable table =
                _service.GetLoaiDonVi();

            foreach (DataRow row in table.Rows)
            {
                cboLoaiDonVi.Items.Add(
                    new SearchOption(
                        Convert.ToInt32(row["id"]),
                        row["ten_loai"].ToString()));
            }

            if (cboLoaiDonVi.Items.Count > 0)
                cboLoaiDonVi.SelectedIndex = 0;
        }

        // =========================================================
        // XÃ / PHƯỜNG HÀ NỘI
        //
        // Vẫn dùng control cboDiaBan để tương thích Designer cũ.
        // Ý nghĩa nghiệp vụ của control này hiện là Xã/Phường.
        // =========================================================
        private void LoadXaPhuong()
        {
            cboDiaBan.Items.Clear();

            cboDiaBan.Items.Add(
                new SearchOption(
                    null,
                    "Tất cả xã/phường"));

            DataTable table =
                _service.GetXaPhuong();

            foreach (DataRow row
                in table.Rows)
            {
                object id =
                    row["id"];

                string text =
                    row["ten_hien_thi"].ToString();

                cboDiaBan.Items.Add(
                    new SearchOption(
                        id,
                        text));
            }

            if (cboDiaBan.Items.Count > 0)
            {
                cboDiaBan.SelectedIndex =
                    0;
            }
        }

        private void LoadTrangThai()
        {
            cboTrangThai.Items.Clear();

            cboTrangThai.Items.Add(
                new SearchOption(
                    null,
                    "Tất cả trạng thái"));

            cboTrangThai.Items.Add(
                new SearchOption(
                    "dang_quan_ly",
                    "Đang quản lý"));

            cboTrangThai.Items.Add(
                new SearchOption(
                    "tam_ngung",
                    "Tạm ngừng"));

            cboTrangThai.Items.Add(
                new SearchOption(
                    "khong_con_quan_ly",
                    "Không còn quản lý"));

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;
        }

        private void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            ExecuteSearch();
        }

        private void txtKeyword_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ExecuteSearch();
            }
        }

        private void ExecuteSearch()
        {
            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                lblStatus.Text =
                    "Đang tra cứu dữ liệu...";

                UnitSearchCriteria criteria =
                    new UnitSearchCriteria();

                criteria.Keyword =
                    txtKeyword.Text.Trim();

                criteria.CapDonViId =
                    GetSelectedIntValue(cboCapDonVi);

                criteria.LoaiDonViId =
                    GetSelectedIntValue(
                        cboLoaiDonVi);

                criteria.XaPhuongId =
                    GetSelectedIntValue(
                        cboDiaBan);

                criteria.TrangThai =
                    GetSelectedStringValue(
                        cboTrangThai);

                if (_resultTable != null)
                {
                    _resultTable.Dispose();
                    _resultTable = null;
                }

                _resultTable =
                    _service.Search(criteria);

                gridResults.AutoGenerateColumns = true;
                gridResults.DataSource = _resultTable;

                ConfigureGridColumns();

                lblResultCount.Text =
                    "Kết quả: " +
                    _resultTable.Rows.Count +
                    " đơn vị";

                lblStatus.Text =
                    "Tra cứu hoàn tất.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tra cứu dữ liệu.\n\n" +
                    ex.Message,
                    "Lỗi tra cứu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Tra cứu thất bại.";
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }

        private void btnClearFilters_Click(
            object sender,
            EventArgs e)
        {
            txtKeyword.Clear();

            if (cboCapDonVi.Items.Count > 0)
                cboCapDonVi.SelectedIndex = 0;

            if (cboLoaiDonVi.Items.Count > 0)
                cboLoaiDonVi.SelectedIndex = 0;

            if (cboDiaBan.Items.Count > 0)
            {
                cboDiaBan.SelectedIndex =
                    0;
            }

            if (cboTrangThai.Items.Count > 0)
                cboTrangThai.SelectedIndex = 0;

            ExecuteSearch();
            txtKeyword.Focus();
        }

        private void ConfigureGridColumns()
        {
            foreach (DataGridViewColumn column
                in gridResults.Columns)
            {
                string field =
                    column.DataPropertyName ?? "";

                column.SortMode =
                    DataGridViewColumnSortMode.Automatic;

                // ---------------------------------------------
                // CÁC CỘT KỸ THUẬT / TƯƠNG THÍCH KHÔNG HIỂN THỊ
                // ---------------------------------------------
                if (string.Equals(
                    field,
                    "xa_phuong_id",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    field,
                    "dia_ban",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    field,
                    "dia_chi_hanh_chinh",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    field,
                    "co_vi_tri_gis",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    field,
                    "ten_xa_phuong_hien_thi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Visible =
                        false;

                    continue;
                }

                column.HeaderText =
                    AttributeTableService
                        .GetFriendlyFieldName(
                            field);

                // ---------------------------------------------
                // ĐỘ RỘNG CỘT
                // ---------------------------------------------
                if (string.Equals(
                    field,
                    "id",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        55;
                }
                else if (string.Equals(
                    field,
                    "ma_don_vi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        110;
                }
                else if (string.Equals(
                    field,
                    "ten_don_vi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        230;
                }
                else if (string.Equals(
                    field,
                    "ten_cap",
                    StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                    field,
                    "ten_loai",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        145;
                }
                else if (string.Equals(
                    field,
                    "ten_don_vi_cap_tren",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        190;
                }
                else if (string.Equals(
                    field,
                    "ten_xa_phuong",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        165;
                }
                else if (string.Equals(
                    field,
                    "loai_xa_phuong",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        105;
                }
                else if (string.Equals(
                    field,
                    "dia_chi_hien_hanh",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        220;
                }
                else if (string.Equals(
                    field,
                    "nguon_tham_khao",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        180;
                }
                else if (string.Equals(
                    field,
                    "muc_do_xac_minh",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.Width =
                        150;
                }
                else if (string.Equals(
                    field,
                    "co_diem_dai_dien_gis",
                    StringComparison.OrdinalIgnoreCase))
                {
                    column.HeaderText =
                        "Điểm đại diện GIS";

                    column.Width =
                        115;
                }
                else
                {
                    column.Width =
                        135;
                }
            }
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

            if (string.Equals(
                field,
                "trang_thai_quan_ly",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyStatus(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (string.Equals(
                field,
                "loai_xa_phuong",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyAdministrativeType(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (string.Equals(
                field,
                "muc_do_xac_minh",
                StringComparison.OrdinalIgnoreCase))
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyVerificationLevel(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (string.Equals(
                field,
                "co_diem_dai_dien_gis",
                StringComparison.OrdinalIgnoreCase))
            {
                bool hasGeometry =
                    false;

                if (e.Value != null &&
                    e.Value != DBNull.Value)
                {
                    bool.TryParse(
                        e.Value.ToString(),
                        out hasGeometry);
                }

                e.Value =
                    hasGeometry
                        ? "Có"
                        : "Chưa có";

                e.FormattingApplied =
                    true;
            }
        }

        private void gridResults_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            SelectCurrentUnit(false);
        }

        private void gridResults_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            OpenCurrentUnitDetail();
        }

        private void btnSelectOnMap_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentUnit(false);
        }

        private void btnZoomToUnit_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentUnit(true);
        }

        private void SelectCurrentUnit(
            bool zoom)
        {
            int id =
                GetSelectedUnitId();

            if (id <= 0)
            {
                MessageBox.Show(
                    "Hãy chọn một đơn vị trong bảng kết quả.",
                    "Chưa chọn đơn vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (_map == null ||
                _unitLayerHandle < 0)
            {
                MessageBox.Show(
                    "Lớp \"Đơn vị quân đội\" chưa được tải trên bản đồ.\n\n" +
                    "Hãy đóng cửa sổ này, nhấn nút PostGIS rồi tra cứu lại.",
                    "Chưa tải lớp GIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool success =
                AttributeTableService
                    .SelectShapeByDatabaseId(
                        _map,
                        _unitLayerHandle,
                        id,
                        zoom);

            if (!success)
            {
                MessageBox.Show(
                    "Đơn vị đã chọn chưa có Point đại diện hành chính " +
                    "hoặc không còn tồn tại trong lớp đang hiển thị.",
                    "Không thể định vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            lblStatus.Text =
                zoom
                    ? "Đã thu phóng tới Point đại diện hành chính của đơn vị."
                    : "Đã chọn Point đại diện hành chính trên bản đồ.";
        }

        private void btnViewDetail_Click(
            object sender,
            EventArgs e)
        {
            OpenCurrentUnitDetail();
        }

        private void OpenCurrentUnitDetail()
        {
            int id =
                GetSelectedUnitId();

            if (id <= 0)
            {
                MessageBox.Show(
                    "Hãy chọn một đơn vị trong bảng kết quả.",
                    "Chưa chọn đơn vị",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (UnitDetailForm form =
                new UnitDetailForm(
                    id,
                    _map,
                    _unitLayerHandle))
            {
                form.ShowDialog(this);
            }

            // Có thể thông tin đã được sửa trong hồ sơ.
            ExecuteSearch();
        }

        private void btnRefresh_Click(
            object sender,
            EventArgs e)
        {
            LoadFilters();
            ExecuteSearch();
        }

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        // =========================================================
        // CẬP NHẬT NHÃN GIAO DIỆN MÀ KHÔNG SỬA DESIGNER
        //
        // Designer cũ vẫn có control cboDiaBan. Chỉ đổi nội dung
        // hiển thị "Địa bàn" thành "Xã/Phường" ở runtime.
        // =========================================================
        private void UpdateRuntimeLabels()
        {
            Text =
                "Tra cứu đơn vị quân đội - Hà Nội";

            UpdateLabelTextRecursive(
                this,
                "Địa bàn",
                "Xã/Phường");

            UpdateLabelTextRecursive(
                this,
                "Địa bàn:",
                "Xã/Phường:");
        }

        private void UpdateLabelTextRecursive(
            Control parent,
            string oldText,
            string newText)
        {
            if (parent == null)
            {
                return;
            }

            foreach (Control control
                in parent.Controls)
            {
                Label label =
                    control as Label;

                if (label != null &&
                    string.Equals(
                        label.Text,
                        oldText,
                        StringComparison.OrdinalIgnoreCase))
                {
                    label.Text =
                        newText;
                }

                if (control.HasChildren)
                {
                    UpdateLabelTextRecursive(
                        control,
                        oldText,
                        newText);
                }
            }
        }

        private int GetSelectedUnitId()
        {
            if (gridResults.CurrentRow == null)
                return -1;

            DataRowView rowView =
                gridResults.CurrentRow
                    .DataBoundItem
                    as DataRowView;

            if (rowView == null ||
                rowView.Row == null ||
                !rowView.Row.Table.Columns.Contains("id"))
            {
                return -1;
            }

            try
            {
                return Convert.ToInt32(
                    rowView["id"]);
            }
            catch
            {
                return -1;
            }
        }

        private int? GetSelectedIntValue(
            ComboBox combo)
        {
            SearchOption option =
                combo.SelectedItem
                as SearchOption;

            if (option == null ||
                option.Value == null ||
                option.Value == DBNull.Value)
            {
                return null;
            }

            int value;

            if (int.TryParse(
                option.Value.ToString(),
                out value))
            {
                return value;
            }

            return null;
        }

        private string GetSelectedStringValue(
            ComboBox combo)
        {
            SearchOption option =
                combo.SelectedItem
                as SearchOption;

            if (option == null ||
                option.Value == null ||
                option.Value == DBNull.Value)
            {
                return null;
            }

            return option.Value.ToString();
        }
    }
}