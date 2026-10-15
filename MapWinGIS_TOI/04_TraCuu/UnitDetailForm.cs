using System;
using System.Data;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // HỒ SƠ CHI TIẾT ĐƠN VỊ
    // =============================================================
    public partial class UnitDetailForm : Form
    {
        private readonly int _unitId;
        private readonly AxMapWinGIS.AxMap _map;
        private readonly int _unitLayerHandle;

        private readonly UnitDetailService _service;
        private readonly PostGISRepository _repository;

        private DataTable _routesTable;
        private DataTable _areasTable;

        public UnitDetailForm(
            int unitId,
            AxMapWinGIS.AxMap map,
            int unitLayerHandle)
        {
            InitializeComponent();

            _unitId =
                unitId;

            _map =
                map;

            _unitLayerHandle =
                unitLayerHandle;

            _service =
                new UnitDetailService();

            _repository =
                new PostGISRepository();

            Text =
                "Hồ sơ đơn vị quân đội - Hà Nội";

            Load +=
                UnitDetailForm_Load;
        }

        // =========================================================
        // LOAD
        // =========================================================
        private void UnitDetailForm_Load(
            object sender,
            EventArgs e)
        {
            UpdateRuntimeLabels();
            ReloadData();
        }

        // =========================================================
        // ĐỌC TOÀN BỘ HỒ SƠ
        // =========================================================
        private void ReloadData()
        {
            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                lblStatus.Text =
                    "Đang đọc hồ sơ đơn vị...";

                DataRow detail =
                    _service.GetUnitDetail(
                        _unitId);

                if (detail == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy đơn vị ID = " +
                        _unitId +
                        ".",
                        "Không tìm thấy dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    Close();
                    return;
                }

                BindDetail(
                    detail);

                DataRow summary =
                    _service.GetUnitSpatialSummary(
                        _unitId);

                BindSummary(
                    summary);

                LoadRoutes();
                LoadAreas();

                lblStatus.Text =
                    "Đã tải hồ sơ đơn vị.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể đọc hồ sơ đơn vị.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Đọc hồ sơ thất bại.";
            }
            finally
            {
                Cursor.Current =
                    oldCursor;
            }
        }

        // =========================================================
        // THÔNG TIN CƠ BẢN - HÀ NỘI ONLY
        // =========================================================
        private void BindDetail(
            DataRow row)
        {
            lblUnitName.Text =
                GetText(
                    row,
                    "ten_don_vi",
                    "(Chưa có tên)");

            SetValueLabel(
                lblMaDonViValue,
                row,
                "ma_don_vi");

            SetValueLabel(
                lblCapDonViValue,
                row,
                "ten_cap");

            SetValueLabel(
                lblLoaiDonViValue,
                row,
                "ten_loai");

            SetValueLabel(
                lblDonViCapTrenValue,
                row,
                "ten_don_vi_cap_tren");

            // -----------------------------------------------------
            // Mô hình hành chính mới:
            // Thành phố Hà Nội -> Xã/Phường.
            //
            // Giữ control lblDiaBanValue để không sửa Designer.cs,
            // nhưng nội dung hiển thị giờ là xã/phường.
            // -----------------------------------------------------
            lblDiaBanValue.Text =
                GetText(
                    row,
                    "ten_xa_phuong_hien_thi",
                    GetText(
                        row,
                        "ten_xa_phuong",
                        "—"));

            SetValueLabel(
                lblDiaChiValue,
                row,
                "dia_chi_hien_hanh");

            SetValueLabel(
                lblLienHeValue,
                row,
                "thong_tin_lien_he");

            lblTrangThaiValue.Text =
                AttributeTableService
                    .GetFriendlyStatus(
                        row.Table.Columns.Contains(
                            "trang_thai_quan_ly")
                            ? row["trang_thai_quan_ly"]
                            : null);

            txtMoTa.Text =
                GetText(
                    row,
                    "mo_ta",
                    "");

            // -----------------------------------------------------
            // Tận dụng ô Ghi chú hiện có để hiển thị đầy đủ thông
            // tin xác minh mà không phải sửa UnitDetailForm.Designer.
            // -----------------------------------------------------
            txtGhiChu.Text =
                BuildVerificationAndNotesText(
                    row);

            bool hasGeometry =
                false;

            if (row.Table.Columns.Contains(
                "co_diem_dai_dien_gis")
                &&
                row["co_diem_dai_dien_gis"] != DBNull.Value)
            {
                bool.TryParse(
                    row["co_diem_dai_dien_gis"].ToString(),
                    out hasGeometry);
            }
            else if (row.Table.Columns.Contains(
                "co_vi_tri_gis")
                &&
                row["co_vi_tri_gis"] != DBNull.Value)
            {
                // Tương thích service cũ trong trường hợp chưa thay.
                bool.TryParse(
                    row["co_vi_tri_gis"].ToString(),
                    out hasGeometry);
            }

            lblGeometryValue.Text =
                hasGeometry
                    ? "Đã có Point đại diện hành chính"
                    : "Chưa có Point đại diện hành chính";

            Text =
                "Hồ sơ đơn vị - " +
                lblUnitName.Text;
        }

        // =========================================================
        // THỐNG KÊ KHÔNG GIAN
        // =========================================================
        private void BindSummary(
            DataRow row)
        {
            if (row == null)
            {
                lblRouteCountValue.Text = "0";
                lblRouteLengthValue.Text = "0 m";
                lblAreaCountValue.Text = "0";
                lblAreaTotalValue.Text = "0 m²";
                lblPerimeterTotalValue.Text = "0 m";
                return;
            }

            lblRouteCountValue.Text =
                GetText(
                    row,
                    "so_tuyen_duong",
                    "0");

            lblRouteLengthValue.Text =
                GetText(
                    row,
                    "tong_chieu_dai_m",
                    "0") +
                " m";

            lblAreaCountValue.Text =
                GetText(
                    row,
                    "so_khu_vuc",
                    "0");

            lblAreaTotalValue.Text =
                GetText(
                    row,
                    "tong_dien_tich_m2",
                    "0") +
                " m²";

            lblPerimeterTotalValue.Text =
                GetText(
                    row,
                    "tong_chu_vi_m",
                    "0") +
                " m";
        }

        // =========================================================
        // TUYẾN ĐƯỜNG
        // =========================================================
        private void LoadRoutes()
        {
            if (_routesTable != null)
            {
                _routesTable.Dispose();
                _routesTable = null;
            }

            _routesTable =
                _service.GetRoutes(
                    _unitId);

            gridRoutes.DataSource =
                _routesTable;

            ConfigureRouteGrid();
        }

        private void ConfigureRouteGrid()
        {
            SetColumn(
                gridRoutes,
                "id",
                "ID",
                50);

            SetColumn(
                gridRoutes,
                "ma_tuyen",
                "Mã tuyến",
                105);

            SetColumn(
                gridRoutes,
                "ten_tuyen",
                "Tên tuyến",
                180);

            SetColumn(
                gridRoutes,
                "loai_tuyen",
                "Loại tuyến",
                135);

            SetColumn(
                gridRoutes,
                "cap_duong",
                "Cấp đường",
                120);

            SetColumn(
                gridRoutes,
                "be_rong_m",
                "Bề rộng (m)",
                95);

            SetColumn(
                gridRoutes,
                "chieu_dai_m",
                "Chiều dài (m)",
                105);

            SetColumn(
                gridRoutes,
                "trang_thai_quan_ly",
                "Trạng thái",
                125);

            SetColumn(
                gridRoutes,
                "mo_ta",
                "Mô tả",
                180);

            SetColumn(
                gridRoutes,
                "ghi_chu",
                "Ghi chú",
                180);
        }

        // =========================================================
        // KHU VỰC
        // =========================================================
        private void LoadAreas()
        {
            if (_areasTable != null)
            {
                _areasTable.Dispose();
                _areasTable = null;
            }

            _areasTable =
                _service.GetAreas(
                    _unitId);

            gridAreas.DataSource =
                _areasTable;

            ConfigureAreaGrid();
        }

        private void ConfigureAreaGrid()
        {
            SetColumn(
                gridAreas,
                "id",
                "ID",
                50);

            SetColumn(
                gridAreas,
                "ma_khu_vuc",
                "Mã khu vực",
                110);

            SetColumn(
                gridAreas,
                "ten_khu_vuc",
                "Tên khu vực",
                190);

            SetColumn(
                gridAreas,
                "loai_khu_vuc",
                "Loại khu vực",
                140);

            SetColumn(
                gridAreas,
                "dien_tich_m2",
                "Diện tích (m²)",
                115);

            SetColumn(
                gridAreas,
                "chu_vi_m",
                "Chu vi (m)",
                100);

            SetColumn(
                gridAreas,
                "trang_thai_quan_ly",
                "Trạng thái",
                125);

            SetColumn(
                gridAreas,
                "mo_ta",
                "Mô tả",
                180);

            SetColumn(
                gridAreas,
                "ghi_chu",
                "Ghi chú",
                180);
        }

        // =========================================================
        // MAP
        // =========================================================
        private void btnSelectOnMap_Click(
            object sender,
            EventArgs e)
        {
            SelectOnMap(
                false);
        }

        private void btnZoomToUnit_Click(
            object sender,
            EventArgs e)
        {
            SelectOnMap(
                true);
        }

        private void SelectOnMap(
            bool zoom)
        {
            if (_map == null ||
                _unitLayerHandle < 0)
            {
                MessageBox.Show(
                    "Lớp \"Đơn vị quân đội\" chưa được tải trên bản đồ.",
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
                        _unitId,
                        zoom);

            if (!success)
            {
                MessageBox.Show(
                    "Đơn vị chưa có Point đại diện hành chính " +
                    "hoặc không còn tồn tại trong lớp hiển thị.",
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

        // =========================================================
        // SỬA THÔNG TIN
        // =========================================================
        private void btnEdit_Click(
            object sender,
            EventArgs e)
        {
            using (EditObjectForm form =
                new EditObjectForm(
                    _repository,
                    "don_vi_quan_doi",
                    _unitId))
            {
                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    ReloadData();

                    lblStatus.Text =
                        "Đã cập nhật hồ sơ đơn vị.";
                }
            }
        }

        // =========================================================
        // LÀM MỚI / ĐÓNG
        // =========================================================
        private void btnRefresh_Click(
            object sender,
            EventArgs e)
        {
            ReloadData();
        }

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        // =========================================================
        // FORMAT GRID
        // =========================================================
        private void gridRoutes_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 ||
                e.ColumnIndex >= gridRoutes.Columns.Count)
            {
                return;
            }

            string field =
                gridRoutes.Columns[
                    e.ColumnIndex]
                    .DataPropertyName;

            if (field == "loai_tuyen")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyRouteType(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (field == "trang_thai_quan_ly")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyStatus(
                            e.Value);

                e.FormattingApplied =
                    true;
            }
        }

        private void gridAreas_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 ||
                e.ColumnIndex >= gridAreas.Columns.Count)
            {
                return;
            }

            string field =
                gridAreas.Columns[
                    e.ColumnIndex]
                    .DataPropertyName;

            if (field == "loai_khu_vuc")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyAreaType(
                            e.Value);

                e.FormattingApplied =
                    true;

                return;
            }

            if (field == "trang_thai_quan_ly")
            {
                e.Value =
                    AttributeTableService
                        .GetFriendlyStatus(
                            e.Value);

                e.FormattingApplied =
                    true;
            }
        }

        // =========================================================
        // CẬP NHẬT NHÃN GIAO DIỆN MÀ KHÔNG SỬA DESIGNER
        // =========================================================
        private void UpdateRuntimeLabels()
        {
            Text =
                "Hồ sơ đơn vị quân đội - Hà Nội";

            ReplaceLabelTextRecursive(
                this,
                "Địa bàn",
                "Xã/Phường");

            ReplaceLabelTextRecursive(
                this,
                "Địa bàn:",
                "Xã/Phường:");

            ReplaceLabelTextRecursive(
                this,
                "Địa chỉ hành chính",
                "Địa chỉ hiện hành");

            ReplaceLabelTextRecursive(
                this,
                "Địa chỉ hành chính:",
                "Địa chỉ hiện hành:");

            ReplaceLabelTextRecursive(
                this,
                "Vị trí GIS",
                "Point đại diện GIS");

            ReplaceLabelTextRecursive(
                this,
                "Vị trí GIS:",
                "Point đại diện GIS:");

            ReplaceLabelTextRecursive(
                this,
                "Ghi chú",
                "Xác minh / Ghi chú");

            ReplaceLabelTextRecursive(
                this,
                "Ghi chú:",
                "Xác minh / Ghi chú:");
        }

        private void ReplaceLabelTextRecursive(
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
                    ReplaceLabelTextRecursive(
                        control,
                        oldText,
                        newText);
                }
            }
        }

        // =========================================================
        // THÔNG TIN XÁC MINH / GHI CHÚ
        // =========================================================
        private string BuildVerificationAndNotesText(
            DataRow row)
        {
            string kyHieu =
                GetText(
                    row,
                    "ky_hieu_bien_so",
                    "CHƯA XÁC MINH");

            string nguon =
                GetText(
                    row,
                    "nguon_tham_khao",
                    "Nguồn công khai");

            string ngayXacMinh =
                GetText(
                    row,
                    "ngay_xac_minh",
                    "—");

            string mucDo =
                AttributeTableService
                    .GetFriendlyVerificationLevel(
                        row != null &&
                        row.Table != null &&
                        row.Table.Columns.Contains(
                            "muc_do_xac_minh")
                            ? row["muc_do_xac_minh"]
                            : null);

            if (string.IsNullOrWhiteSpace(
                mucDo))
            {
                mucDo =
                    "—";
            }

            string ghiChu =
                GetText(
                    row,
                    "ghi_chu",
                    "Không có");

            return
                "Ký hiệu: " +
                kyHieu +
                Environment.NewLine +
                "Nguồn tham khảo: " +
                nguon +
                Environment.NewLine +
                "Ngày xác minh: " +
                ngayXacMinh +
                Environment.NewLine +
                "Mức độ xác minh: " +
                mucDo +
                Environment.NewLine +
                "Ghi chú: " +
                ghiChu;
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void SetValueLabel(
            Label label,
            DataRow row,
            string columnName)
        {
            if (label == null)
                return;

            label.Text =
                GetText(
                    row,
                    columnName,
                    "—");
        }

        private string GetText(
            DataRow row,
            string columnName,
            string fallback)
        {
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(
                    columnName) ||
                row[columnName] == null ||
                row[columnName] == DBNull.Value ||
                string.IsNullOrWhiteSpace(
                    row[columnName].ToString()))
            {
                return fallback;
            }

            return row[columnName].ToString();
        }

        private void SetColumn(
            DataGridView grid,
            string name,
            string header,
            int width)
        {
            if (grid == null ||
                !grid.Columns.Contains(
                    name))
            {
                return;
            }

            grid.Columns[name].HeaderText =
                header;

            grid.Columns[name].Width =
                width;
        }
    }
}