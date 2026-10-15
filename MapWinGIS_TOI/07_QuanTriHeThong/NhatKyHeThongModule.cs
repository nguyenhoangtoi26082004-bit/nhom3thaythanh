
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;

namespace MapWinGIS_TOI
{
    // ============================================================
    // NHẬT KÝ HỆ THỐNG - GIS HÀ NỘI 2.0
    //
    // CSDL: HQTCSDL
    // Schema: ql_don_vi
    // Bảng: nhat_ky_thay_doi
    //
    // Chức năng:
    // - Xem lịch sử thêm, sửa, xóa
    // - Lọc theo ngày, bảng, thao tác và ID
    // - Xuất CSV
    //
    // Chỉ đọc dữ liệu, không thay đổi CSDL.
    // ============================================================

    public partial class Form1
    {
        private bool _nhatKyDaKhoiTao = false;

        // Dùng OnActivated để tránh trùng OnShown
        // với phân hệ Thống kê và báo cáo.
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!_nhatKyDaKhoiTao)
            {
                KhoiTaoNhatKyHeThong();
            }
        }

        private void KhoiTaoNhatKyHeThong()
        {
            if (_nhatKyDaKhoiTao ||
                _modernNav == null)
            {
                return;
            }

            FlowLayoutPanel flow = null;

            foreach (Control control in _modernNav.Controls)
            {
                flow = control as FlowLayoutPanel;

                if (flow != null)
                    break;
            }

            if (flow == null)
                return;

            // Ẩn nút cũ chưa có chức năng thực.
            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                if (string.Equals(
                    button.Text.Trim(),
                    "Nhật ký hệ thống",
                    StringComparison.OrdinalIgnoreCase))
                {
                    button.Visible = false;
                }
            }

            // Tạo nút mới hoạt động thực sự.
            Button btnNhatKy = CreateNavButton(
                "Nhật ký hệ thống",
                delegate
                {
                    AnTongQuan();
                    MoNhatKyHeThong();
                },
                false);

            flow.Controls.Add(btnNhatKy);

            _nhatKyDaKhoiTao = true;
        }

        private void MoNhatKyHeThong()
        {
            try
            {
                using (NhatKyHeThongFormV2 form =
                    new NhatKyHeThongFormV2(
                        DocNhatKyHeThong))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể mở Nhật ký hệ thống.\n\n" +
                    ex.Message,
                    "Lỗi nhật ký",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // ĐỌC NHẬT KÝ TỪ POSTGRESQL
        //
        // Lọc tại CSDL, dùng tham số SQL.
        // Tối đa 1000 bản ghi mới nhất phù hợp bộ lọc.
        // ========================================================
        private DataTable DocNhatKyHeThong(
            DateTime? tuNgay,
            DateTime? denNgay,
            string tenBang,
            string hanhDong,
            long? idBanGhi)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@"
SELECT
    id,
    to_char(
        thoi_diem AT TIME ZONE 'Asia/Ho_Chi_Minh',
        'DD/MM/YYYY HH24:MI:SS'
    ) AS thoi_diem,
    ten_bang,
    id_ban_ghi,
    hanh_dong,
    tai_khoan_csdl,
    COALESCE(ung_dung, '') AS ung_dung,
    ma_giao_dich
FROM ql_don_vi.nhat_ky_thay_doi
WHERE 1 = 1
");

            if (tuNgay.HasValue)
            {
                sql.Append(@"
AND thoi_diem >=
    (@tu_ngay::date::timestamp
        AT TIME ZONE 'Asia/Ho_Chi_Minh')
");
            }

            if (denNgay.HasValue)
            {
                sql.Append(@"
AND thoi_diem <
    ((@den_ngay::date + 1)::timestamp
        AT TIME ZONE 'Asia/Ho_Chi_Minh')
");
            }

            if (!string.IsNullOrWhiteSpace(tenBang))
            {
                sql.Append(
                    " AND ten_bang = @ten_bang");
            }

            if (!string.IsNullOrWhiteSpace(hanhDong))
            {
                sql.Append(
                    " AND hanh_dong = @hanh_dong");
            }

            if (idBanGhi.HasValue)
            {
                sql.Append(
                    " AND id_ban_ghi = @id_ban_ghi");
            }

            sql.Append(@"
ORDER BY thoi_diem DESC, id DESC
LIMIT 1000;
");

            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            // Dùng lại cấu hình PostGIS đang có trong Form1.
            builder.Host = PG_HOST;
            builder.Port = Convert.ToInt32(PG_PORT);
            builder.Database = PG_DB;
            builder.Username = PG_USER;
            builder.Password = PG_PASS;

            builder.Timeout = 5;
            builder.CommandTimeout = 30;
            builder.Pooling = true;

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    builder.ConnectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql.ToString(),
                    connection))
            {
                if (tuNgay.HasValue)
                {
                    command.Parameters.Add(
                        "tu_ngay",
                        NpgsqlDbType.Date).Value =
                        tuNgay.Value.Date;
                }

                if (denNgay.HasValue)
                {
                    command.Parameters.Add(
                        "den_ngay",
                        NpgsqlDbType.Date).Value =
                        denNgay.Value.Date;
                }

                if (!string.IsNullOrWhiteSpace(tenBang))
                {
                    command.Parameters.Add(
                        "ten_bang",
                        NpgsqlDbType.Text).Value =
                        tenBang;
                }

                if (!string.IsNullOrWhiteSpace(hanhDong))
                {
                    command.Parameters.Add(
                        "hanh_dong",
                        NpgsqlDbType.Varchar).Value =
                        hanhDong;
                }

                if (idBanGhi.HasValue)
                {
                    command.Parameters.Add(
                        "id_ban_ghi",
                        NpgsqlDbType.Bigint).Value =
                        idBanGhi.Value;
                }

                using (NpgsqlDataAdapter adapter =
                    new NpgsqlDataAdapter(command))
                {
                    DataTable result = new DataTable();

                    adapter.Fill(result);

                    return result;
                }
            }
        }
    }

    // ============================================================
    // CỬA SỔ NHẬT KÝ HỆ THỐNG
    // ============================================================
    public sealed class NhatKyHeThongFormV2 : Form
    {
        private readonly Func<
            DateTime?,
            DateTime?,
            string,
            string,
            long?,
            DataTable> _docDuLieu;

        private DateTimePicker _dtTuNgay;
        private DateTimePicker _dtDenNgay;

        private ComboBox _cboBang;
        private ComboBox _cboThaoTac;

        private TextBox _txtId;
        private TextBox _txtChiTiet;

        private DataGridView _grid;

        private Label _lblSoLuong;
        private Label _lblTrangThai;

        private DataTable _duLieu;

        public NhatKyHeThongFormV2(
            Func<
                DateTime?,
                DateTime?,
                string,
                string,
                long?,
                DataTable> docDuLieu)
        {
            if (docDuLieu == null)
                throw new ArgumentNullException(
                    "docDuLieu");

            _docDuLieu = docDuLieu;

            KhoiTaoGiaoDien();

            Load += delegate
            {
                TaiDuLieu();
            };
        }

        // ========================================================
        // KHỞI TẠO GIAO DIỆN
        // ========================================================
        private void KhoiTaoGiaoDien()
        {
            Text = "Nhật ký hệ thống - GIS Hà Nội";

            StartPosition =
                FormStartPosition.CenterParent;

            Size = new Size(1210, 730);
            MinimumSize = new Size(990, 570);

            BackColor =
                Color.FromArgb(9, 17, 33);

            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            TableLayoutPanel main =
                new TableLayoutPanel();

            main.Dock = DockStyle.Fill;

            main.ColumnCount = 1;
            main.RowCount = 4;

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 88));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 120));

            main.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 86));

            Controls.Add(main);

            // ----------------------------------------------------
            // TIÊU ĐỀ
            // ----------------------------------------------------
            Panel header = new Panel();

            header.Dock = DockStyle.Fill;

            header.BackColor =
                Color.FromArgb(15, 23, 42);

            Label title = new Label();

            title.Text = "NHẬT KÝ HỆ THỐNG";

            title.Font = new Font(
                "Segoe UI",
                16F,
                FontStyle.Bold);

            title.ForeColor = Color.White;

            title.AutoSize = true;
            title.Location = new Point(20, 12);

            Label subtitle = new Label();

            subtitle.Text =
                "Lịch sử thay đổi dữ liệu " +
                "được ghi nhận tự động bởi PostgreSQL";

            subtitle.ForeColor =
                Color.FromArgb(148, 163, 184);

            subtitle.AutoSize = true;
            subtitle.Location = new Point(22, 54);

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            main.Controls.Add(header, 0, 0);

            // ----------------------------------------------------
            // BỘ LỌC NHẬT KÝ
            // ----------------------------------------------------
            FlowLayoutPanel filters =
                new FlowLayoutPanel();

            filters.Dock = DockStyle.Fill;

            filters.WrapContents = true;
            filters.AutoScroll = true;

            filters.Padding =
                new Padding(15, 10, 10, 8);

            filters.BackColor =
                Color.FromArgb(15, 23, 42);

            filters.Controls.Add(
                TaoNhan("Từ ngày:", 72));

            _dtTuNgay = new DateTimePicker();

            _dtTuNgay.Width = 145;
            _dtTuNgay.Format =
                DateTimePickerFormat.Short;

            _dtTuNgay.ShowCheckBox = true;
            _dtTuNgay.Checked = false;

            filters.Controls.Add(_dtTuNgay);

            filters.Controls.Add(
                TaoNhan("Đến ngày:", 78));

            _dtDenNgay = new DateTimePicker();

            _dtDenNgay.Width = 145;
            _dtDenNgay.Format =
                DateTimePickerFormat.Short;

            _dtDenNgay.ShowCheckBox = true;
            _dtDenNgay.Checked = false;

            filters.Controls.Add(_dtDenNgay);

            filters.Controls.Add(
                TaoNhan("Bảng:", 48));

            _cboBang = new ComboBox();

            _cboBang.Width = 185;

            _cboBang.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cboBang.Items.Add("Tất cả bảng");
            _cboBang.Items.Add("Hồ sơ đơn vị");
            _cboBang.Items.Add("Tuyến đường");
            _cboBang.Items.Add("Khu vực");

            _cboBang.SelectedIndex = 0;

            filters.Controls.Add(_cboBang);

            filters.Controls.Add(
                TaoNhan("Thao tác:", 78));

            _cboThaoTac = new ComboBox();

            _cboThaoTac.Width = 130;

            _cboThaoTac.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cboThaoTac.Items.Add("Tất cả");
            _cboThaoTac.Items.Add("Thêm mới");
            _cboThaoTac.Items.Add("Cập nhật");
            _cboThaoTac.Items.Add("Xóa");

            _cboThaoTac.SelectedIndex = 0;

            filters.Controls.Add(_cboThaoTac);

            filters.Controls.Add(
                TaoNhan("ID bản ghi:", 90));

            _txtId = new TextBox();

            _txtId.Width = 130;
            _txtId.Font =
                new Font("Segoe UI", 10F);

            filters.Controls.Add(_txtId);

            Button btnLoc = TaoNut(
                "Tra cứu nhật ký", 135);

            btnLoc.Click += delegate
            {
                TaiDuLieu();
            };

            filters.Controls.Add(btnLoc);

            main.Controls.Add(filters, 0, 1);

            // ----------------------------------------------------
            // BẢNG VÀ THÔNG TIN CHI TIẾT
            // ----------------------------------------------------
            TableLayoutPanel body =
                new TableLayoutPanel();

            body.Dock = DockStyle.Fill;

            body.Padding =
                new Padding(14, 10, 14, 10);

            body.ColumnCount = 2;

            body.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 76F));

            body.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 24F));

            _grid = new DataGridView();

            _grid.Dock = DockStyle.Fill;

            _grid.ReadOnly = true;
            _grid.AutoGenerateColumns = false;

            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;

            _grid.MultiSelect = false;

            _grid.RowHeadersVisible = false;

            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _grid.BackgroundColor =
                Color.FromArgb(9, 17, 33);

            _grid.BorderStyle = BorderStyle.None;

            _grid.GridColor =
                Color.FromArgb(51, 65, 85);

            _grid.EnableHeadersVisualStyles = false;

            _grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 41, 59);

            _grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _grid.DefaultCellStyle.BackColor =
                Color.FromArgb(15, 23, 42);

            _grid.DefaultCellStyle.ForeColor =
                Color.White;

            _grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(5, 150, 105);

            _grid.DefaultCellStyle.SelectionForeColor =
                Color.White;

            _grid.RowTemplate.Height = 29;

            ThemCot(
                "thoi_diem",
                "Thời điểm", 165);

            ThemCot(
                "ten_bang",
                "Bảng dữ liệu", 160);

            ThemCot(
                "id_ban_ghi",
                "ID bản ghi", 90);

            ThemCot(
                "hanh_dong",
                "Thao tác", 105);

            ThemCot(
                "tai_khoan_csdl",
                "Tài khoản CSDL", 130);

            ThemCot(
                "ung_dung",
                "Ứng dụng", 135);

            ThemCot(
                "ma_giao_dich",
                "Mã giao dịch", 115);

            _grid.CellFormatting +=
                DinhDangGiaTri;

            _grid.SelectionChanged += delegate
            {
                HienThiChiTiet();
            };

            body.Controls.Add(_grid, 0, 0);

            // Chi tiết bản ghi nhật ký
            Panel detail = new Panel();

            detail.Dock = DockStyle.Fill;

            detail.Margin =
                new Padding(12, 0, 0, 0);

            detail.BackColor =
                Color.FromArgb(15, 23, 42);

            Label detailTitle = new Label();

            detailTitle.Text =
                "CHI TIẾT NHẬT KÝ";

            detailTitle.Dock = DockStyle.Top;
            detailTitle.Height = 48;

            detailTitle.Padding =
                new Padding(13, 14, 0, 0);

            detailTitle.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            _txtChiTiet = new TextBox();

            _txtChiTiet.Dock = DockStyle.Fill;

            _txtChiTiet.ReadOnly = true;
            _txtChiTiet.Multiline = true;

            _txtChiTiet.ScrollBars =
                ScrollBars.Vertical;

            _txtChiTiet.BorderStyle =
                BorderStyle.None;

            _txtChiTiet.BackColor =
                Color.FromArgb(15, 23, 42);

            _txtChiTiet.ForeColor =
                Color.FromArgb(203, 213, 225);

            _txtChiTiet.Text =
                "Chọn một dòng nhật ký để xem.";

            detail.Controls.Add(_txtChiTiet);
            detail.Controls.Add(detailTitle);

            body.Controls.Add(detail, 1, 0);

            main.Controls.Add(body, 0, 2);

            // ----------------------------------------------------
            // THANH CHỨC NĂNG
            // ----------------------------------------------------
            Panel footer = new Panel();

            footer.Dock = DockStyle.Fill;

            footer.BackColor =
                Color.FromArgb(15, 23, 42);

            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock = DockStyle.Top;
            actions.Height = 45;

            actions.WrapContents = false;

            actions.Padding =
                new Padding(13, 8, 0, 0);

            Button btnLamMoi =
                TaoNut("Làm mới", 100);

            Button btnXuat =
                TaoNut("Xuất CSV", 110);

            Button btnDong =
                TaoNut("Đóng", 85);

            btnLamMoi.Click += delegate
            {
                TaiDuLieu();
            };

            btnXuat.Click += delegate
            {
                XuatCsv();
            };

            btnDong.Click += delegate
            {
                Close();
            };

            actions.Controls.Add(btnLamMoi);
            actions.Controls.Add(btnXuat);
            actions.Controls.Add(btnDong);

            _lblSoLuong = new Label();

            _lblSoLuong.AutoSize = true;

            _lblSoLuong.ForeColor =
                Color.FromArgb(52, 211, 153);

            _lblSoLuong.Location =
                new Point(18, 58);

            _lblTrangThai = new Label();

            _lblTrangThai.AutoSize = true;

            _lblTrangThai.ForeColor =
                Color.FromArgb(148, 163, 184);

            _lblTrangThai.Location =
                new Point(250, 58);

            footer.Controls.Add(actions);
            footer.Controls.Add(_lblSoLuong);
            footer.Controls.Add(_lblTrangThai);

            main.Controls.Add(footer, 0, 3);
        }

        // ========================================================
        // THÀNH PHẦN GIAO DIỆN CHUNG
        // ========================================================
        private Label TaoNhan(string text, int width)
        {
            Label label = new Label();

            label.Text = text;
            label.Width = width;
            label.Height = 30;

            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.ForeColor = Color.White;

            return label;
        }

        private Button TaoNut(string text, int width)
        {
            Button button = new Button();

            button.Text = text;

            button.Width = width;
            button.Height = 30;

            button.FlatStyle = FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 1;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(51, 65, 85);

            button.BackColor =
                Color.FromArgb(30, 41, 59);

            button.ForeColor = Color.White;

            button.Cursor = Cursors.Hand;

            button.Margin =
                new Padding(3, 0, 3, 0);

            return button;
        }

        private void ThemCot(
            string field,
            string title,
            int width)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.Name = field;

            column.DataPropertyName = field;

            column.HeaderText = title;
            column.Width = width;

            column.SortMode =
                DataGridViewColumnSortMode.NotSortable;

            _grid.Columns.Add(column);
        }

        // ========================================================
        // ĐỌC CÁC GIÁ TRỊ BỘ LỌC
        // ========================================================
        private string LayTenBang()
        {
            switch (_cboBang.SelectedIndex)
            {
                case 1:
                    return "don_vi_quan_doi";

                case 2:
                    return "tuyen_duong_don_vi";

                case 3:
                    return "khu_vuc_don_vi";

                default:
                    return "";
            }
        }

        private string LayThaoTac()
        {
            switch (_cboThaoTac.SelectedIndex)
            {
                case 1:
                    return "INSERT";

                case 2:
                    return "UPDATE";

                case 3:
                    return "DELETE";

                default:
                    return "";
            }
        }

        // ========================================================
        // TẢI NHẬT KÝ
        // ========================================================
        private void TaiDuLieu()
        {
            DateTime? tuNgay = _dtTuNgay.Checked
                ? (DateTime?)_dtTuNgay.Value.Date
                : null;

            DateTime? denNgay = _dtDenNgay.Checked
                ? (DateTime?)_dtDenNgay.Value.Date
                : null;

            if (tuNgay.HasValue &&
                denNgay.HasValue &&
                tuNgay.Value > denNgay.Value)
            {
                MessageBox.Show(
                    "Ngày bắt đầu không được lớn hơn " +
                    "ngày kết thúc.",
                    "Bộ lọc ngày",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            long? idBanGhi = null;

            if (!string.IsNullOrWhiteSpace(
                _txtId.Text))
            {
                long id;

                if (!long.TryParse(
                    _txtId.Text.Trim(),
                    out id) || id <= 0)
                {
                    MessageBox.Show(
                        "ID bản ghi phải là số nguyên dương.",
                        "Kiểm tra ID",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                idBanGhi = id;
            }

            Cursor oldCursor = Cursor.Current;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                _lblTrangThai.Text =
                    "Đang đọc nhật ký từ PostgreSQL...";

                DataTable result = _docDuLieu(
                    tuNgay,
                    denNgay,
                    LayTenBang(),
                    LayThaoTac(),
                    idBanGhi);

                _grid.DataSource = null;

                if (_duLieu != null)
                    _duLieu.Dispose();

                _duLieu = result;

                _grid.DataSource = _duLieu;

                _lblSoLuong.Text =
                    "Hiển thị: " +
                    _duLieu.Rows.Count +
                    " bản ghi";

                _lblTrangThai.Text =
                    "Tối đa 1.000 bản ghi mới nhất " +
                    "phù hợp bộ lọc.";

                HienThiChiTiet();
            }
            catch (Exception ex)
            {
                _lblTrangThai.Text =
                    "Không thể đọc nhật ký.";

                MessageBox.Show(
                    "Không thể tải nhật ký từ PostGIS.\n\n" +
                    ex.Message +
                    "\n\nHãy kiểm tra bảng " +
                    "ql_don_vi.nhat_ky_thay_doi.",
                    "Lỗi nhật ký",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }

        // ========================================================
        // HIỂN THỊ GIÁ TRỊ TIẾNG VIỆT
        // ========================================================
        private void DinhDangGiaTri(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                e.Value == null)
            {
                return;
            }

            string field =
                _grid.Columns[e.ColumnIndex]
                    .DataPropertyName;

            string value =
                Convert.ToString(e.Value);

            if (field == "hanh_dong")
            {
                e.Value = TenThaoTac(value);
                e.FormattingApplied = true;
            }
            else if (field == "ten_bang")
            {
                e.Value = TenBang(value);
                e.FormattingApplied = true;
            }
        }

        private static string TenThaoTac(string value)
        {
            switch (value)
            {
                case "INSERT":
                    return "Thêm mới";

                case "UPDATE":
                    return "Cập nhật";

                case "DELETE":
                    return "Xóa";

                default:
                    return value ?? "";
            }
        }

        private static string TenBang(string value)
        {
            switch (value)
            {
                case "don_vi_quan_doi":
                    return "Hồ sơ đơn vị";

                case "tuyen_duong_don_vi":
                    return "Tuyến đường";

                case "khu_vuc_don_vi":
                    return "Khu vực";

                default:
                    return value ?? "";
            }
        }

        // ========================================================
        // CHI TIẾT MỘT BẢN GHI NHẬT KÝ
        // ========================================================
        private void HienThiChiTiet()
        {
            if (_grid.CurrentRow == null)
            {
                _txtChiTiet.Text =
                    "Chưa chọn bản ghi nhật ký.";

                return;
            }

            DataRowView view =
                _grid.CurrentRow.DataBoundItem
                    as DataRowView;

            if (view == null)
                return;

            DataRow row = view.Row;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CHI TIẾT THAY ĐỔI");
            sb.AppendLine();

            sb.AppendLine(
                "Mã nhật ký: " + row["id"]);

            sb.AppendLine(
                "Thời điểm: " + row["thoi_diem"]);

            sb.AppendLine();

            sb.AppendLine(
                "Bảng dữ liệu:");

            sb.AppendLine(
                TenBang(Convert.ToString(
                    row["ten_bang"])));

            sb.AppendLine();

            sb.AppendLine(
                "ID bản ghi:");

            sb.AppendLine(
                Convert.ToString(
                    row["id_ban_ghi"]));

            sb.AppendLine();

            sb.AppendLine(
                "Thao tác:");

            sb.AppendLine(
                TenThaoTac(Convert.ToString(
                    row["hanh_dong"])));

            sb.AppendLine();

            sb.AppendLine(
                "Tài khoản CSDL:");

            sb.AppendLine(
                Convert.ToString(
                    row["tai_khoan_csdl"]));

            sb.AppendLine();

            sb.AppendLine(
                "Ứng dụng:");

            sb.AppendLine(
                Convert.ToString(
                    row["ung_dung"]));

            sb.AppendLine();

            sb.AppendLine(
                "Mã giao dịch:");

            sb.AppendLine(
                Convert.ToString(
                    row["ma_giao_dich"]));

            sb.AppendLine();
            sb.AppendLine(
                "Lưu ý: Nhật ký hiện theo dõi " +
                "thao tác và ID, chưa lưu nội dung " +
                "thay đổi trước/sau.");

            _txtChiTiet.Text = sb.ToString();
        }

        // ========================================================
        // XUẤT CSV
        // ========================================================
        private void XuatCsv()
        {
            if (_duLieu == null ||
                _duLieu.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Không có nhật ký để xuất.",
                    "Xuất CSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Xuất nhật ký hệ thống";

                dialog.Filter =
                    "Tệp CSV (*.csv)|*.csv";

                dialog.FileName =
                    "NhatKyHeThong_" +
                    DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss") +
                    ".csv";

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (StreamWriter writer =
                        new StreamWriter(
                            dialog.FileName,
                            false,
                            new UTF8Encoding(true)))
                    {
                        writer.WriteLine(
                            "Thời điểm;Bảng dữ liệu;" +
                            "ID bản ghi;Thao tác;" +
                            "Tài khoản CSDL;Ứng dụng;" +
                            "Mã giao dịch");

                        foreach (DataRow row
                            in _duLieu.Rows)
                        {
                            string[] fields =
                            {
                                Convert.ToString(
                                    row["thoi_diem"]),

                                TenBang(
                                    Convert.ToString(
                                        row["ten_bang"])),

                                Convert.ToString(
                                    row["id_ban_ghi"]),

                                TenThaoTac(
                                    Convert.ToString(
                                        row["hanh_dong"])),

                                Convert.ToString(
                                    row["tai_khoan_csdl"]),

                                Convert.ToString(
                                    row["ung_dung"]),

                                Convert.ToString(
                                    row["ma_giao_dich"])
                            };

                            for (int i = 0;
                                i < fields.Length;
                                i++)
                            {
                                fields[i] =
                                    BaoVeGiaTriCsv(fields[i]);
                            }

                            writer.WriteLine(
                                string.Join(";", fields));
                        }
                    }

                    MessageBox.Show(
                        "Đã xuất nhật ký thành công.\n\n" +
                        dialog.FileName,
                        "Hoàn thành",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể xuất CSV.\n\n" +
                        ex.Message,
                        "Lỗi xuất dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static string BaoVeGiaTriCsv(
            string value)
        {
            if (value == null)
                value = "";

            string kiemTra =
                value.TrimStart(' ', '\t', '\r', '\n');

            if (kiemTra.StartsWith("=") ||
                kiemTra.StartsWith("+") ||
                kiemTra.StartsWith("-") ||
                kiemTra.StartsWith("@"))
            {
                value = "'" + value;
            }

            return "\"" +
                value.Replace("\"", "\"\"") +
                "\"";
        }

        // ========================================================
        // GIẢI PHÓNG DỮ LIỆU
        // ========================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_grid != null)
                    _grid.DataSource = null;

                if (_duLieu != null)
                {
                    _duLieu.Dispose();
                    _duLieu = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
