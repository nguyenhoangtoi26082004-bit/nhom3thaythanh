
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ============================================================
    // YÊU CẦU GỬI VỀ FORM1
    // ============================================================
    public enum QuanLyDuLieuYeuCau
    {
        Khong,
        Them,
        ChonBanDo,
        ThuPhong
    }

    // ============================================================
    // QUẢN LÝ DỮ LIỆU GIS HÀ NỘI 2.0
    //
    // 1. Hồ sơ đơn vị quân đội
    // 2. Tuyến đường đơn vị
    // 3. Khu vực đơn vị
    //
    // Tái sử dụng PostGISRepository và EditObjectForm.
    // Không lưu hình học trực tiếp tại cửa sổ này.
    // ============================================================
    public sealed class QuanLyDuLieuForm : Form
    {
        private readonly PostGISRepository _repository;
        private readonly Action<string> _lamMoiBanDo;

        private ComboBox cboNhom;
        private TextBox txtTimKiem;
        private TextBox txtChiTiet;
        private DataGridView gridDuLieu;

        private Label lblSoLuong;
        private Label lblTrangThai;

        private Button btnSua;
        private Button btnXoa;
        private Button btnChon;
        private Button btnThuPhong;

        private DataTable bangGoc;
        private DataTable bangLoc;

        private string bangHienTai = "";

        public QuanLyDuLieuYeuCau YeuCau
        {
            get;
            private set;
        }

        public string BangDuocChon
        {
            get;
            private set;
        }

        public int IdDuocChon
        {
            get;
            private set;
        }

        private sealed class NhomDuLieu
        {
            public string Ma { get; private set; }
            public string Ten { get; private set; }

            public NhomDuLieu(string ma, string ten)
            {
                Ma = ma;
                Ten = ten;
            }

            public override string ToString()
            {
                return Ten;
            }
        }

        // ========================================================
        // KHỞI TẠO
        // ========================================================
        public QuanLyDuLieuForm(
            Action<string> lamMoiBanDo)
        {
            _repository = new PostGISRepository();
            _lamMoiBanDo = lamMoiBanDo;

            YeuCau = QuanLyDuLieuYeuCau.Khong;
            BangDuocChon = "";
            IdDuocChon = -1;

            TaoGiaoDien();
            NapDanhSachNhom();
        }

        // ========================================================
        // GIAO DIỆN CHÍNH
        // ========================================================
        private void TaoGiaoDien()
        {
            Text = "Quản lý dữ liệu GIS - Hà Nội";
            StartPosition = FormStartPosition.CenterParent;

            Size = new Size(1220, 740);
            MinimumSize = new Size(980, 610);

            BackColor = Color.FromArgb(9, 17, 33);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 4;

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 85));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 72));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 103));

            Controls.Add(layout);

            // ----------------------------------------------------
            // 1. TIÊU ĐỀ
            // ----------------------------------------------------
            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.FromArgb(15, 23, 42);

            Label title = new Label();
            title.Text = "QUẢN LÝ DỮ LIỆU GIS";
            title.Font = new Font(
                "Segoe UI", 16F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(20, 12);

            Label subtitle = new Label();
            subtitle.Text =
                "Quản lý thống nhất hồ sơ đơn vị, " +
                "tuyến đường và khu vực tại Hà Nội";
            subtitle.ForeColor =
                Color.FromArgb(148, 163, 184);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(22, 51);

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            layout.Controls.Add(header, 0, 0);

            // ----------------------------------------------------
            // 2. BỘ LỌC
            // ----------------------------------------------------
            FlowLayoutPanel boLoc =
                new FlowLayoutPanel();

            boLoc.Dock = DockStyle.Fill;
            boLoc.WrapContents = false;
            boLoc.AutoScroll = true;
            boLoc.Padding = new Padding(16, 12, 8, 5);
            boLoc.BackColor = Color.FromArgb(15, 23, 42);

            Label lblNhom = TaoNhan(
                "Nhóm dữ liệu:", 112);

            cboNhom = new ComboBox();
            cboNhom.Width = 250;
            cboNhom.DropDownStyle =
                ComboBoxStyle.DropDownList;
            cboNhom.SelectedIndexChanged +=
                CboNhom_SelectedIndexChanged;

            Label lblTim = TaoNhan(
                "Lọc trong bảng:", 115);

            lblTim.Margin = new Padding(18, 3, 3, 3);

            txtTimKiem = new TextBox();
            txtTimKiem.Width = 260;
            txtTimKiem.Font = new Font("Segoe UI", 10F);
            txtTimKiem.TextChanged +=
                TxtTimKiem_TextChanged;

            Button btnXoaLoc = TaoNut("Xóa lọc", 90);

            btnXoaLoc.Click += delegate
            {
                txtTimKiem.Clear();
            };

            boLoc.Controls.Add(lblNhom);
            boLoc.Controls.Add(cboNhom);
            boLoc.Controls.Add(lblTim);
            boLoc.Controls.Add(txtTimKiem);
            boLoc.Controls.Add(btnXoaLoc);

            layout.Controls.Add(boLoc, 0, 1);

            // ----------------------------------------------------
            // 3. BẢNG VÀ THÔNG TIN CHI TIẾT
            // ----------------------------------------------------
            TableLayoutPanel noiDung =
                new TableLayoutPanel();

            noiDung.Dock = DockStyle.Fill;
            noiDung.Padding = new Padding(15, 10, 15, 10);
            noiDung.ColumnCount = 2;

            noiDung.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 70F));

            noiDung.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30F));

            gridDuLieu = new DataGridView();

            gridDuLieu.Dock = DockStyle.Fill;
            gridDuLieu.ReadOnly = true;
            gridDuLieu.AutoGenerateColumns = true;

            gridDuLieu.AllowUserToAddRows = false;
            gridDuLieu.AllowUserToDeleteRows = false;
            gridDuLieu.AllowUserToResizeRows = false;

            gridDuLieu.MultiSelect = false;
            gridDuLieu.RowHeadersVisible = false;

            gridDuLieu.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            gridDuLieu.BackgroundColor =
                Color.FromArgb(9, 17, 33);

            gridDuLieu.BorderStyle =
                BorderStyle.None;

            gridDuLieu.GridColor =
                Color.FromArgb(51, 65, 85);

            gridDuLieu.EnableHeadersVisualStyles = false;

            gridDuLieu.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 41, 59);

            gridDuLieu.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            gridDuLieu.DefaultCellStyle.BackColor =
                Color.FromArgb(15, 23, 42);

            gridDuLieu.DefaultCellStyle.ForeColor =
                Color.White;

            gridDuLieu.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(5, 150, 105);

            gridDuLieu.DefaultCellStyle.SelectionForeColor =
                Color.White;

            gridDuLieu.RowTemplate.Height = 30;

            gridDuLieu.SelectionChanged +=
                Grid_SelectionChanged;

            gridDuLieu.CellDoubleClick += delegate
            {
                SuaBanGhi();
            };

            gridDuLieu.CellFormatting +=
                Grid_CellFormatting;

            Panel panelChiTiet = new Panel();
            panelChiTiet.Dock = DockStyle.Fill;
            panelChiTiet.Margin =
                new Padding(12, 0, 0, 0);
            panelChiTiet.BackColor =
                Color.FromArgb(15, 23, 42);

            Label lblChiTiet = new Label();
            lblChiTiet.Text = "THÔNG TIN BẢN GHI";
            lblChiTiet.Dock = DockStyle.Top;
            lblChiTiet.Height = 50;
            lblChiTiet.Padding =
                new Padding(15, 15, 0, 0);
            lblChiTiet.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);

            txtChiTiet = new TextBox();
            txtChiTiet.Dock = DockStyle.Fill;
            txtChiTiet.Multiline = true;
            txtChiTiet.ReadOnly = true;
            txtChiTiet.ScrollBars =
                ScrollBars.Vertical;
            txtChiTiet.BorderStyle =
                BorderStyle.None;
            txtChiTiet.BackColor =
                Color.FromArgb(15, 23, 42);
            txtChiTiet.ForeColor =
                Color.FromArgb(203, 213, 225);
            txtChiTiet.Text =
                "Chọn một bản ghi để xem thông tin.";

            panelChiTiet.Controls.Add(txtChiTiet);
            panelChiTiet.Controls.Add(lblChiTiet);

            noiDung.Controls.Add(gridDuLieu, 0, 0);
            noiDung.Controls.Add(panelChiTiet, 1, 0);

            layout.Controls.Add(noiDung, 0, 2);

            // ----------------------------------------------------
            // 4. THANH THAO TÁC
            // ----------------------------------------------------
            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor =
                Color.FromArgb(15, 23, 42);

            FlowLayoutPanel thaoTac =
                new FlowLayoutPanel();

            thaoTac.Dock = DockStyle.Top;
            thaoTac.Height = 51;
            thaoTac.WrapContents = false;
            thaoTac.AutoScroll = true;
            thaoTac.Padding =
                new Padding(13, 10, 0, 0);

            Button btnThem = TaoNut("Thêm mới", 105);
            btnSua = TaoNut("Sửa", 75);
            btnXoa = TaoNut("Xóa", 75);

            btnChon = TaoNut(
                "Chọn trên bản đồ", 145);

            btnThuPhong = TaoNut(
                "Thu phóng", 110);

            Button btnLamMoi =
                TaoNut("Làm mới", 95);

            Button btnDong =
                TaoNut("Đóng", 80);

            btnThem.Click += delegate
            {
                GuiYeuCau(
                    QuanLyDuLieuYeuCau.Them,
                    false);
            };

            btnSua.Click += delegate
            {
                SuaBanGhi();
            };

            btnXoa.Click += delegate
            {
                XoaBanGhi();
            };

            btnChon.Click += delegate
            {
                GuiYeuCau(
                    QuanLyDuLieuYeuCau.ChonBanDo,
                    true);
            };

            btnThuPhong.Click += delegate
            {
                GuiYeuCau(
                    QuanLyDuLieuYeuCau.ThuPhong,
                    true);
            };

            btnLamMoi.Click += delegate
            {
                NapDuLieu();
            };

            btnDong.Click += delegate
            {
                Close();
            };

            thaoTac.Controls.Add(btnThem);
            thaoTac.Controls.Add(btnSua);
            thaoTac.Controls.Add(btnXoa);
            thaoTac.Controls.Add(btnChon);
            thaoTac.Controls.Add(btnThuPhong);
            thaoTac.Controls.Add(btnLamMoi);
            thaoTac.Controls.Add(btnDong);

            lblSoLuong = new Label();
            lblSoLuong.AutoSize = true;
            lblSoLuong.Location =
                new Point(19, 66);
            lblSoLuong.ForeColor =
                Color.FromArgb(52, 211, 153);

            lblTrangThai = new Label();
            lblTrangThai.AutoSize = true;
            lblTrangThai.Location =
                new Point(245, 66);
            lblTrangThai.ForeColor =
                Color.FromArgb(148, 163, 184);

            footer.Controls.Add(lblSoLuong);
            footer.Controls.Add(lblTrangThai);
            footer.Controls.Add(thaoTac);

            layout.Controls.Add(footer, 0, 3);

            CapNhatTrangThaiNut();
        }

        // ========================================================
        // ĐIỀU KHIỂN CHUNG
        // ========================================================
        private Label TaoNhan(string text, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Width = width;
            label.Height = 32;
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
            button.Height = 31;
            button.Margin =
                new Padding(3, 0, 3, 0);

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor =
                Color.FromArgb(51, 65, 85);

            button.BackColor =
                Color.FromArgb(30, 41, 59);

            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;

            return button;
        }

        // ========================================================
        // NHÓM DỮ LIỆU
        // ========================================================
        private void NapDanhSachNhom()
        {
            cboNhom.Items.Add(
                new NhomDuLieu(
                    "don_vi_quan_doi",
                    "Hồ sơ đơn vị quân đội"));

            cboNhom.Items.Add(
                new NhomDuLieu(
                    "tuyen_duong_don_vi",
                    "Tuyến đường đơn vị"));

            cboNhom.Items.Add(
                new NhomDuLieu(
                    "khu_vuc_don_vi",
                    "Khu vực đơn vị"));

            cboNhom.SelectedIndex = 0;
        }

        private void CboNhom_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            NhomDuLieu nhom =
                cboNhom.SelectedItem as NhomDuLieu;

            if (nhom == null)
                return;

            bangHienTai = nhom.Ma;

            txtTimKiem.Clear();
            NapDuLieu();
        }

        // ========================================================
        // ĐỌC BẢNG TỪ REPOSITORY
        // ========================================================
        private void NapDuLieu()
        {
            if (string.IsNullOrWhiteSpace(bangHienTai))
                return;

            Cursor oldCursor = Cursor.Current;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                lblTrangThai.Text =
                    "Đang đọc dữ liệu...";

                gridDuLieu.DataSource = null;
                GiaiPhongBang();

                bangGoc =
                    _repository.GetTableData(bangHienTai);

                ApDungBoLoc();

                lblTrangThai.Text =
                    "Đã cập nhật dữ liệu từ PostGIS.";
            }
            catch (Exception ex)
            {
                gridDuLieu.DataSource = null;
                GiaiPhongBang();

                lblSoLuong.Text = "Không có dữ liệu.";
                lblTrangThai.Text = "Tải dữ liệu thất bại.";

                MessageBox.Show(
                    "Không thể đọc dữ liệu từ PostGIS.\n\n" +
                    ex.Message,
                    "Lỗi dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = oldCursor;
                CapNhatTrangThaiNut();
            }
        }

        // ========================================================
        // LỌC NHANH TRONG BẢNG
        //
        // Chỉ lọc dữ liệu đã tải.
        // Không thực hiện truy vấn SQL mỗi lần gõ.
        // ========================================================
        private void TxtTimKiem_TextChanged(
            object sender,
            EventArgs e)
        {
            ApDungBoLoc();
        }

        private void ApDungBoLoc()
        {
            if (bangGoc == null)
                return;

            string keyword = txtTimKiem.Text.Trim();

            DataTable ketQua = bangGoc.Clone();

            foreach (DataRow row in bangGoc.Rows)
            {
                if (KhopTuKhoa(row, keyword))
                {
                    ketQua.ImportRow(row);
                }
            }

            gridDuLieu.DataSource = null;

            if (bangLoc != null)
            {
                bangLoc.Dispose();
            }

            bangLoc = ketQua;
            gridDuLieu.DataSource = bangLoc;

            DinhDangCot();

            lblSoLuong.Text =
                "Hiển thị: " +
                bangLoc.Rows.Count + "/" +
                bangGoc.Rows.Count +
                " bản ghi";

            CapNhatTrangThaiNut();
        }

        private bool KhopTuKhoa(
            DataRow row,
            string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return true;

            foreach (DataColumn column
                in row.Table.Columns)
            {
                object raw = row[column];

                if (raw == null ||
                    raw == DBNull.Value)
                {
                    continue;
                }

                string value =
                    Convert.ToString(raw);

                if (!string.IsNullOrEmpty(value) &&
                    value.IndexOf(
                        keyword,
                        StringComparison.CurrentCultureIgnoreCase)
                    >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        // ========================================================
        // ĐỊNH DẠNG BẢNG
        // ========================================================
        private void DinhDangCot()
        {
            foreach (DataGridViewColumn column
                in gridDuLieu.Columns)
            {
                string field = column.DataPropertyName;

                if (string.IsNullOrWhiteSpace(field))
                    field = column.Name;

                column.HeaderText =
                    AttributeTableService
                        .GetFriendlyFieldName(field);

                column.Width = 145;

                if (field == "id")
                {
                    column.HeaderText = "ID";
                    column.Width = 65;
                }
                else if (field == "ten_don_vi" ||
                         field == "ten_tuyen" ||
                         field == "ten_khu_vuc")
                {
                    column.Width = 240;
                }

                if (field == "geom" ||
                    field == "geom_hien_thi")
                {
                    column.Visible = false;
                }
            }
        }

        private void Grid_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            string field =
                gridDuLieu.Columns[e.ColumnIndex]
                    .DataPropertyName;

            if (field == "trang_thai_quan_ly" ||
                field == "loai_xa_phuong")
            {
                e.Value =
                    AttributeTableService.GetFriendlyValue(
                        field, e.Value);

                e.FormattingApplied = true;
            }
        }

        // ========================================================
        // LẤY ĐÚNG BẢN GHI ĐANG CHỌN
        // ========================================================
        private DataRow LayDongDangChon()
        {
            if (gridDuLieu.CurrentRow == null)
                return null;

            DataRowView view =
                gridDuLieu.CurrentRow.DataBoundItem
                    as DataRowView;

            return view == null
                ? null
                : view.Row;
        }

        private int LayIdDangChon()
        {
            DataRow row = LayDongDangChon();

            if (row == null ||
                !row.Table.Columns.Contains("id"))
                return -1;

            int id;

            return int.TryParse(
                Convert.ToString(row["id"]),
                out id)
                ? id
                : -1;
        }

        // ========================================================
        // THÔNG TIN BẢN GHI
        // ========================================================
        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            DataRow row = LayDongDangChon();

            if (row == null)
            {
                txtChiTiet.Text =
                    "Chưa chọn bản ghi.";

                CapNhatTrangThaiNut();
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (DataColumn column
                in row.Table.Columns)
            {
                string field = column.ColumnName;

                if (field == "geom" ||
                    field == "geom_hien_thi")
                    continue;

                string tenTruong =
                    AttributeTableService
                        .GetFriendlyFieldName(field);

                string giaTri =
                    AttributeTableService
                        .GetFriendlyValue(
                            field,
                            row[field]);

                sb.AppendLine(tenTruong + ":");
                sb.AppendLine(
                    string.IsNullOrWhiteSpace(giaTri)
                        ? "—"
                        : giaTri);
                sb.AppendLine();
            }

            if (bangHienTai == "don_vi_quan_doi")
            {
                sb.AppendLine(
                    "GHI CHÚ: Điểm GIS chỉ là điểm " +
                    "đại diện hành chính theo xã/phường, " +
                    "không phải vị trí cơ sở chính xác.");
            }

            txtChiTiet.Text = sb.ToString();

            CapNhatTrangThaiNut();
        }

        // ========================================================
        // TRẠNG THÁI NÚT
        // ========================================================
        private void CapNhatTrangThaiNut()
        {
            bool coBanGhi = LayIdDangChon() > 0;

            if (btnSua != null)
                btnSua.Enabled = coBanGhi;

            if (btnXoa != null)
                btnXoa.Enabled = coBanGhi;

            if (btnChon != null)
                btnChon.Enabled = coBanGhi;

            if (btnThuPhong != null)
                btnThuPhong.Enabled = coBanGhi;
        }

        // ========================================================
        // SỬA BẢN GHI
        // ========================================================
        private void SuaBanGhi()
        {
            int id = LayIdDangChon();

            if (id <= 0)
                return;

            try
            {
                using (EditObjectForm form =
                    new EditObjectForm(
                        _repository,
                        bangHienTai,
                        id))
                {
                    if (form.ShowDialog(this) !=
                        DialogResult.OK)
                        return;
                }

                DongBoBanDo();
                NapDuLieu();
                ChonLaiId(id);

                lblTrangThai.Text =
                    "Đã cập nhật bản ghi ID = " + id;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể sửa bản ghi.\n\n" +
                    ex.Message,
                    "Lỗi cập nhật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // XÓA BẢN GHI
        //
        // Hai lần xác nhận để hạn chế xóa nhầm.
        // ========================================================
        private void XoaBanGhi()
        {
            int id = LayIdDangChon();

            if (id <= 0)
                return;

            DialogResult confirm1 =
                MessageBox.Show(
                    "Anh muốn xóa bản ghi ID = " + id +
                    " khỏi CSDL HQTCSDL?\n\n" +
                    "Dữ liệu có thể đang liên kết " +
                    "với các bảng khác.",
                    "Xác nhận xóa dữ liệu",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (confirm1 != DialogResult.Yes)
                return;

            DialogResult confirm2 =
                MessageBox.Show(
                    "XÁC NHẬN LẦN CUỐI\n\n" +
                    "Xóa bản ghi ID = " + id +
                    " trong bảng " + bangHienTai +
                    "?\n\n" +
                    "Thao tác không thể hoàn tác " +
                    "trực tiếp từ giao diện.",
                    "Xác nhận lần cuối",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (confirm2 != DialogResult.Yes)
                return;

            try
            {
                _repository.DeleteRecord(
                    bangHienTai, id);

                DongBoBanDo();
                NapDuLieu();

                lblTrangThai.Text =
                    "Đã xóa bản ghi ID = " + id;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể xóa bản ghi.\n\n" +
                    ex.Message +
                    "\n\nNếu bản ghi đang có dữ liệu " +
                    "liên kết, hãy kiểm tra quan hệ " +
                    "trong CSDL trước khi xóa.",
                    "Lỗi xóa dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // CHỌN LẠI ID SAU KHI SỬA
        // ========================================================
        private void ChonLaiId(int id)
        {
            foreach (DataGridViewRow row
                in gridDuLieu.Rows)
            {
                if (row.IsNewRow)
                    continue;

                DataRowView view =
                    row.DataBoundItem as DataRowView;

                if (view == null ||
                    !view.Row.Table.Columns.Contains("id"))
                    continue;

                int rowId;

                if (int.TryParse(
                    Convert.ToString(view.Row["id"]),
                    out rowId) &&
                    rowId == id)
                {
                    gridDuLieu.ClearSelection();
                    row.Selected = true;

                    gridDuLieu.CurrentCell =
                        row.Cells[0];

                    return;
                }
            }
        }

        // ========================================================
        // ĐỒNG BỘ LỚP GIS
        // ========================================================
        private void DongBoBanDo()
        {
            if (_lamMoiBanDo == null)
                return;

            try
            {
                _lamMoiBanDo(bangHienTai);
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "QuanLyDuLieu ERROR refresh: " + ex);

                MessageBox.Show(
                    "Dữ liệu đã được cập nhật, " +
                    "nhưng lớp bản đồ chưa làm mới được.\n\n" +
                    ex.Message,
                    "Cảnh báo đồng bộ bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ========================================================
        // CHUYỂN YÊU CẦU VỀ FORM1
        // ========================================================
        private void GuiYeuCau(
            QuanLyDuLieuYeuCau yeuCau,
            bool canChon)
        {
            if (string.IsNullOrWhiteSpace(bangHienTai))
                return;

            int id = -1;

            if (canChon)
            {
                id = LayIdDangChon();

                if (id <= 0)
                {
                    MessageBox.Show(
                        "Hãy chọn một bản ghi trước.",
                        "Chưa chọn đối tượng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }
            }

            YeuCau = yeuCau;
            BangDuocChon = bangHienTai;
            IdDuocChon = id;

            DialogResult = DialogResult.OK;
            Close();
        }

        // ========================================================
        // GIẢI PHÓNG DỮ LIỆU
        // ========================================================
        private void GiaiPhongBang()
        {
            if (bangLoc != null)
            {
                bangLoc.Dispose();
                bangLoc = null;
            }

            if (bangGoc != null)
            {
                bangGoc.Dispose();
                bangGoc = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (gridDuLieu != null)
                    gridDuLieu.DataSource = null;

                GiaiPhongBang();
            }

            base.Dispose(disposing);
        }
    }
}
