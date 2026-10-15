
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // PHÂN TÍCH KHÔNG GIAN GIS HÀ NỘI 2.0
    //
    // CSDL: HQTCSDL
    // Schema: ql_don_vi
    // VIEW: v_phan_tich_xa_phuong
    // CRS: EPSG:32648
    //
    // Chỉ đọc dữ liệu, không sửa CSDL.
    //
    // Dùng chung điểm mở chức năng cũ:
    // OpenSpatialQueryForm()
    //
    // Không cần chỉnh SpatialQueryForm.Designer.cs.
    // ============================================================

    public partial class Form1
    {
        private bool _spatialQueryToolsInitialized = false;

        private ToolStripMenuItem _menuSpatialDataQuery;

        // ========================================================
        // 1. KHỞI TẠO MENU
        // ========================================================
        private void InitSpatialQueryTools()
        {
            if (_spatialQueryToolsInitialized)
                return;

            if (menuAnalysis == null)
                return;

            ToolStripItem existing =
                menuAnalysis.DropDownItems[
                    "menuSpatialDataQuery"];

            if (existing != null)
            {
                _menuSpatialDataQuery =
                    existing as ToolStripMenuItem;

                if (_menuSpatialDataQuery != null)
                {
                    _menuSpatialDataQuery.Text =
                        "Phân tích theo xã/phường";

                    _menuSpatialDataQuery.Click -=
                        menuSpatialDataQuery_Click;

                    _menuSpatialDataQuery.Click +=
                        menuSpatialDataQuery_Click;
                }
            }
            else
            {
                _menuSpatialDataQuery =
                    new ToolStripMenuItem();

                _menuSpatialDataQuery.Name =
                    "menuSpatialDataQuery";

                _menuSpatialDataQuery.Text =
                    "Phân tích theo xã/phường";

                _menuSpatialDataQuery.Click +=
                    menuSpatialDataQuery_Click;

                menuAnalysis.DropDownItems.Insert(
                    0, _menuSpatialDataQuery);
            }

            _spatialQueryToolsInitialized = true;
        }

        private void menuSpatialDataQuery_Click(
            object sender,
            EventArgs e)
        {
            OpenSpatialQueryForm();
        }

        // ========================================================
        // 2. MỞ GIAO DIỆN PHÂN TÍCH MỚI
        //
        // Giữ tên hàm cũ để menu hiện tại vẫn hoạt động.
        // ========================================================
        private void OpenSpatialQueryForm()
        {
            try
            {
                using (PhanTichXaPhuongForm form =
                    new PhanTichXaPhuongForm(
                        DocDuLieuPhanTichXaPhuong,
                        ChonXaPhuongPhanTichTrenBanDo))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "PhanTichKhongGian ERROR: " + ex);

                MessageBox.Show(
                    "Không thể mở phân hệ Phân tích " +
                    "không gian.\n\n" + ex.Message,
                    "Lỗi phân tích",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // 3. ĐỌC VIEW PHÂN TÍCH
        //
        // Dùng cấu hình kết nối hiện hành của Form1.
        // Không viết lại mật khẩu trong file này.
        // ========================================================
        private DataTable DocDuLieuPhanTichXaPhuong()
        {
            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host = PG_HOST;
            builder.Port = Convert.ToInt32(PG_PORT);
            builder.Database = PG_DB;
            builder.Username = PG_USER;
            builder.Password = PG_PASS;
            builder.Timeout = 5;
            builder.CommandTimeout = 30;
            builder.Pooling = true;

            const string sql = @"
SELECT
    xa_phuong_id,
    ma_xa_phuong,
    ten_xa_phuong,
    loai_xa_phuong,
    dien_tich_km2,
    so_ho_so_don_vi,
    so_diem_khop,
    so_diem_khong_khop,
    so_ho_so_chua_kiem_tra_duoc,
    ty_le_diem_khop_phan_tram
FROM ql_don_vi.v_phan_tich_xa_phuong
ORDER BY
    so_ho_so_don_vi DESC,
    ten_xa_phuong;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    builder.ConnectionString))
            using (NpgsqlDataAdapter adapter =
                new NpgsqlDataAdapter(sql, connection))
            {
                DataTable table =
                    new DataTable("PhanTichXaPhuong");

                adapter.Fill(table);

                return table;
            }
        }

        // ========================================================
        // 4. CHỌN RANH GIỚI XÃ/PHƯỜNG TRÊN BẢN ĐỒ
        //
        // Không định vị cơ sở quân đội.
        // ========================================================
        private bool ChonXaPhuongPhanTichTrenBanDo(
            int xaPhuongId,
            bool thuPhong)
        {
            if (axMap2 == null || xaPhuongId <= 0)
                return false;

            const string layerKey =
                "xa_phuong_ha_noi";

            if (!EnsureLayerReady(layerKey, true))
                return false;

            int handle =
                GetSpatialQueryLayerHandle(layerKey);

            if (handle < 0)
                return false;

            bool selected =
                AttributeTableService
                    .SelectShapeByDatabaseId(
                        axMap2,
                        handle,
                        xaPhuongId,
                        thuPhong);

            if (selected && lblStatus != null)
            {
                lblStatus.Text = thuPhong
                    ? "Đã thu phóng tới ranh giới xã/phường."
                    : "Đã chọn ranh giới xã/phường.";
            }

            if (selected)
            {
                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }
            }

            return selected;
        }

        // ========================================================
        // 5. LẤY HANDLE LỚP ĐÃ NẠP
        // ========================================================
        private int GetSpatialQueryLayerHandle(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(layerKey))
                return -1;

            if (_loadedLayers == null)
                return -1;

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey, out handle))
            {
                return -1;
            }

            return handle >= 0 ? handle : -1;
        }

        // ========================================================
        // 6. CỬA SỔ PHÂN TÍCH THEO ĐỊA BÀN
        //
        // Xây dựng trực tiếp bằng WinForms.
        // Không cần tệp Designer mới.
        // ========================================================
        private sealed class PhanTichXaPhuongForm : Form
        {
            private readonly Func<DataTable> _taiDuLieu;

            private readonly Func<int, bool, bool>
                _chonTrenBanDo;

            private DataTable _bangGoc;
            private DataTable _bangLoc;

            private ComboBox _cboDieuKien;
            private TextBox _txtTimKiem;
            private TextBox _txtChiTiet;
            private DataGridView _grid;

            private Label _lblTongHop;
            private Label _lblTrangThai;
            private Label _lblSoLuong;

            private Button _btnChon;
            private Button _btnThuPhong;

            // ====================================================
            // CONSTRUCTOR
            // ====================================================
            public PhanTichXaPhuongForm(
                Func<DataTable> taiDuLieu,
                Func<int, bool, bool> chonTrenBanDo)
            {
                _taiDuLieu = taiDuLieu;
                _chonTrenBanDo = chonTrenBanDo;

                TaoGiaoDien();

                Load += delegate
                {
                    NapDuLieu();
                };
            }

            // ====================================================
            // GIAO DIỆN
            // ====================================================
            private void TaoGiaoDien()
            {
                Text =
                    "Phân tích không gian - Hà Nội";

                StartPosition =
                    FormStartPosition.CenterParent;

                Size = new Size(1150, 720);
                MinimumSize = new Size(950, 580);

                BackColor =
                    Color.FromArgb(9, 17, 33);

                ForeColor = Color.White;

                Font = new Font("Segoe UI", 9F);

                TableLayoutPanel layout =
                    new TableLayoutPanel();

                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 1;
                layout.RowCount = 4;

                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Absolute, 95));

                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Absolute, 70));

                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent, 100));

                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Absolute, 90));

                Controls.Add(layout);

                // ----------------------------------------------
                // PHẦN ĐẦU
                // ----------------------------------------------
                Panel header = new Panel();

                header.Dock = DockStyle.Fill;

                header.BackColor =
                    Color.FromArgb(15, 23, 42);

                Label title = new Label();

                title.Text =
                    "PHÂN TÍCH KHÔNG GIAN THEO ĐỊA BÀN";

                title.Font = new Font(
                    "Segoe UI", 15F, FontStyle.Bold);

                title.Location = new Point(19, 12);
                title.AutoSize = true;
                title.ForeColor = Color.White;

                _lblTongHop = new Label();

                _lblTongHop.Text =
                    "Đang chờ dữ liệu phân tích...";

                _lblTongHop.Location =
                    new Point(21, 51);

                _lblTongHop.AutoSize = false;
                _lblTongHop.Size =
                    new Size(1030, 34);

                _lblTongHop.ForeColor =
                    Color.FromArgb(125, 211, 252);

                header.Controls.Add(title);
                header.Controls.Add(_lblTongHop);

                layout.Controls.Add(header, 0, 0);

                // ----------------------------------------------
                // BỘ LỌC
                // ----------------------------------------------
                FlowLayoutPanel filters =
                    new FlowLayoutPanel();

                filters.Dock = DockStyle.Fill;
                filters.WrapContents = false;
                filters.AutoScroll = true;

                filters.Padding =
                    new Padding(15, 14, 8, 5);

                filters.BackColor =
                    Color.FromArgb(15, 23, 42);

                Label lblFilter = TaoNhan(
                    "Điều kiện:", 78);

                _cboDieuKien = new ComboBox();

                _cboDieuKien.Width = 260;

                _cboDieuKien.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                _cboDieuKien.Items.Add(
                    "Tất cả xã/phường");

                _cboDieuKien.Items.Add(
                    "Có hồ sơ đơn vị");

                _cboDieuKien.Items.Add(
                    "Chưa có hồ sơ đơn vị");

                _cboDieuKien.Items.Add(
                    "Có điểm không khớp địa bàn");

                _cboDieuKien.Items.Add(
                    "Có hồ sơ chưa kiểm tra được");

                _cboDieuKien.SelectedIndex = 0;

                _cboDieuKien.SelectedIndexChanged +=
                    delegate
                    {
                        ApDungBoLoc();
                    };

                Label lblTim = TaoNhan(
                    "Tên/mã xã phường:", 145);

                lblTim.Margin =
                    new Padding(16, 3, 2, 3);

                _txtTimKiem = new TextBox();

                _txtTimKiem.Width = 250;

                _txtTimKiem.Font =
                    new Font("Segoe UI", 10F);

                _txtTimKiem.TextChanged +=
                    delegate
                    {
                        ApDungBoLoc();
                    };

                Button btnXoaLoc =
                    TaoNut("Xóa lọc", 90);

                btnXoaLoc.Click += delegate
                {
                    _cboDieuKien.SelectedIndex = 0;
                    _txtTimKiem.Clear();
                };

                filters.Controls.Add(lblFilter);
                filters.Controls.Add(_cboDieuKien);
                filters.Controls.Add(lblTim);
                filters.Controls.Add(_txtTimKiem);
                filters.Controls.Add(btnXoaLoc);

                layout.Controls.Add(filters, 0, 1);

                // ----------------------------------------------
                // BẢNG PHÂN TÍCH + CHI TIẾT
                // ----------------------------------------------
                TableLayoutPanel body =
                    new TableLayoutPanel();

                body.Dock = DockStyle.Fill;

                body.Padding =
                    new Padding(12, 10, 12, 8);

                body.ColumnCount = 2;

                body.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent, 74F));

                body.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent, 26F));

                _grid = new DataGridView();

                _grid.Dock = DockStyle.Fill;

                _grid.ReadOnly = true;
                _grid.AutoGenerateColumns = false;

                _grid.AllowUserToAddRows = false;
                _grid.AllowUserToDeleteRows = false;
                _grid.AllowUserToResizeRows = false;

                _grid.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;

                _grid.MultiSelect = false;
                _grid.RowHeadersVisible = false;

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
                    "ma_xa_phuong",
                    "Mã xã/phường", 125);

                ThemCot(
                    "ten_xa_phuong",
                    "Tên xã/phường", 185);

                ThemCot(
                    "dien_tich_km2",
                    "Diện tích (km²)", 115);

                ThemCot(
                    "so_ho_so_don_vi",
                    "Số hồ sơ", 90);

                ThemCot(
                    "so_diem_khop",
                    "Điểm khớp", 90);

                ThemCot(
                    "so_diem_khong_khop",
                    "Điểm không khớp", 135);

                ThemCot(
                    "so_ho_so_chua_kiem_tra_duoc",
                    "Chưa kiểm tra được", 145);

                ThemCot(
                    "ty_le_diem_khop_phan_tram",
                    "Tỷ lệ khớp (%)", 105);

                _grid.SelectionChanged += delegate
                {
                    HienThiChiTiet();
                };

                _grid.CellDoubleClick += delegate (
                    object sender,
                    DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex >= 0)
                        MoBanDo(true);
                };

                // Panel chi tiết
                Panel detail = new Panel();

                detail.Dock = DockStyle.Fill;

                detail.Margin =
                    new Padding(12, 0, 0, 0);

                detail.BackColor =
                    Color.FromArgb(15, 23, 42);

                Label detailTitle = new Label();

                detailTitle.Text =
                    "KẾT QUẢ PHÂN TÍCH";

                detailTitle.Dock = DockStyle.Top;
                detailTitle.Height = 48;

                detailTitle.Padding =
                    new Padding(13, 14, 0, 0);

                detailTitle.Font =
                    new Font(
                        "Segoe UI", 10F,
                        FontStyle.Bold);

                _txtChiTiet = new TextBox();

                _txtChiTiet.Dock = DockStyle.Fill;

                _txtChiTiet.Multiline = true;
                _txtChiTiet.ReadOnly = true;

                _txtChiTiet.ScrollBars =
                    ScrollBars.Vertical;

                _txtChiTiet.BorderStyle =
                    BorderStyle.None;

                _txtChiTiet.BackColor =
                    Color.FromArgb(15, 23, 42);

                _txtChiTiet.ForeColor =
                    Color.FromArgb(203, 213, 225);

                _txtChiTiet.Text =
                    "Chọn một xã/phường để xem kết quả.";

                detail.Controls.Add(_txtChiTiet);
                detail.Controls.Add(detailTitle);

                body.Controls.Add(_grid, 0, 0);
                body.Controls.Add(detail, 1, 0);

                layout.Controls.Add(body, 0, 2);

                // ----------------------------------------------
                // THANH CHỨC NĂNG
                // ----------------------------------------------
                Panel footer = new Panel();

                footer.Dock = DockStyle.Fill;

                footer.BackColor =
                    Color.FromArgb(15, 23, 42);

                FlowLayoutPanel actions =
                    new FlowLayoutPanel();

                actions.Dock = DockStyle.Top;
                actions.Height = 46;

                actions.WrapContents = false;
                actions.AutoScroll = true;

                actions.Padding =
                    new Padding(12, 9, 0, 0);

                _btnChon =
                    TaoNut("Chọn trên bản đồ", 148);

                _btnThuPhong =
                    TaoNut("Thu phóng xã/phường", 170);

                Button btnReload =
                    TaoNut("Làm mới", 100);

                Button btnClose =
                    TaoNut("Đóng", 85);

                _btnChon.Click += delegate
                {
                    MoBanDo(false);
                };

                _btnThuPhong.Click += delegate
                {
                    MoBanDo(true);
                };

                btnReload.Click += delegate
                {
                    NapDuLieu();
                };

                btnClose.Click += delegate
                {
                    Close();
                };

                actions.Controls.Add(_btnChon);
                actions.Controls.Add(_btnThuPhong);
                actions.Controls.Add(btnReload);
                actions.Controls.Add(btnClose);

                _lblSoLuong = new Label();

                _lblSoLuong.AutoSize = true;

                _lblSoLuong.ForeColor =
                    Color.FromArgb(52, 211, 153);

                _lblSoLuong.Location =
                    new Point(17, 56);

                _lblTrangThai = new Label();

                _lblTrangThai.AutoSize = true;

                _lblTrangThai.ForeColor =
                    Color.FromArgb(148, 163, 184);

                _lblTrangThai.Location =
                    new Point(225, 56);

                footer.Controls.Add(actions);
                footer.Controls.Add(_lblSoLuong);
                footer.Controls.Add(_lblTrangThai);

                layout.Controls.Add(footer, 0, 3);

                CapNhatNut();
            }

            // ====================================================
            // CÁC THÀNH PHẦN GIAO DIỆN CHUNG
            // ====================================================
            private Label TaoNhan(
                string text,
                int width)
            {
                Label label = new Label();

                label.Text = text;
                label.Width = width;
                label.Height = 30;

                label.ForeColor = Color.White;

                label.TextAlign =
                    ContentAlignment.MiddleLeft;

                return label;
            }

            private Button TaoNut(
                string text,
                int width)
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
                    DataGridViewColumnSortMode.Automatic;

                if (field == "dien_tich_km2" ||
                    field == "ty_le_diem_khop_phan_tram")
                {
                    column.DefaultCellStyle.Format = "N2";
                }

                _grid.Columns.Add(column);
            }

            // ====================================================
            // TẢI DỮ LIỆU VIEW
            // ====================================================
            private void NapDuLieu()
            {
                Cursor oldCursor = Cursor.Current;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    _lblTrangThai.Text =
                        "Đang đọc dữ liệu PostGIS...";

                    _grid.DataSource = null;

                    GiaiPhongBang();

                    _bangGoc = _taiDuLieu();

                    if (_bangGoc == null)
                    {
                        _lblTrangThai.Text =
                            "Không có dữ liệu.";
                        return;
                    }

                    CapNhatTongHop();

                    ApDungBoLoc();

                    _lblTrangThai.Text =
                        "Dữ liệu từ v_phan_tich_xa_phuong.";
                }
                catch (Exception ex)
                {
                    _grid.DataSource = null;

                    GiaiPhongBang();

                    _lblTongHop.Text =
                        "Không tải được số liệu.";

                    _lblSoLuong.Text = "0 bản ghi";

                    _lblTrangThai.Text =
                        "Lỗi kết nối hoặc truy vấn.";

                    MessageBox.Show(
                        "Không thể đọc VIEW phân tích.\n\n" +
                        ex.Message +
                        "\n\nHãy kiểm tra VIEW " +
                        "ql_don_vi.v_phan_tich_xa_phuong.",
                        "Lỗi PostGIS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = oldCursor;
                    CapNhatNut();
                }
            }

            // ====================================================
            // THỐNG KÊ TỔNG HỢP
            // ====================================================
            private void CapNhatTongHop()
            {
                if (_bangGoc == null)
                    return;

                long tongHoSo = 0;
                long diemKhop = 0;
                long diemKhongKhop = 0;
                long chuaKiemTra = 0;

                foreach (DataRow row in _bangGoc.Rows)
                {
                    tongHoSo += So(row, "so_ho_so_don_vi");

                    diemKhop += So(row, "so_diem_khop");

                    diemKhongKhop +=
                        So(row, "so_diem_khong_khop");

                    chuaKiemTra +=
                        So(row,
                            "so_ho_so_chua_kiem_tra_duoc");
                }

                _lblTongHop.Text = string.Format(
                    "{0:N0} xã/phường  |  " +
                    "{1:N0} hồ sơ  |  " +
                    "{2:N0} điểm khớp  |  " +
                    "{3:N0} không khớp  |  " +
                    "{4:N0} chưa kiểm tra được",
                    _bangGoc.Rows.Count,
                    tongHoSo,
                    diemKhop,
                    diemKhongKhop,
                    chuaKiemTra);
            }

            // ====================================================
            // BỘ LỌC THỐNG KÊ
            // ====================================================
            private void ApDungBoLoc()
            {
                if (_bangGoc == null)
                    return;

                int filterIndex =
                    _cboDieuKien.SelectedIndex;

                string keyword =
                    _txtTimKiem.Text.Trim();

                DataTable filtered = _bangGoc.Clone();

                foreach (DataRow row in _bangGoc.Rows)
                {
                    long soHoSo =
                        So(row, "so_ho_so_don_vi");

                    long soSai =
                        So(row, "so_diem_khong_khop");

                    long soChua =
                        So(row,
                            "so_ho_so_chua_kiem_tra_duoc");

                    bool matchCondition = true;

                    switch (filterIndex)
                    {
                        case 1:
                            matchCondition = soHoSo > 0;
                            break;

                        case 2:
                            matchCondition = soHoSo == 0;
                            break;

                        case 3:
                            matchCondition = soSai > 0;
                            break;

                        case 4:
                            matchCondition = soChua > 0;
                            break;
                    }

                    if (!matchCondition)
                        continue;

                    string ten =
                        Chuoi(row, "ten_xa_phuong");

                    string ma =
                        Chuoi(row, "ma_xa_phuong");

                    bool matchKeyword =
                        string.IsNullOrWhiteSpace(keyword) ||
                        ten.IndexOf(
                            keyword,
                            StringComparison.CurrentCultureIgnoreCase)
                        >= 0 ||
                        ma.IndexOf(
                            keyword,
                            StringComparison.CurrentCultureIgnoreCase)
                        >= 0;

                    if (matchKeyword)
                    {
                        filtered.ImportRow(row);
                    }
                }

                _grid.DataSource = null;

                if (_bangLoc != null)
                    _bangLoc.Dispose();

                _bangLoc = filtered;

                _grid.DataSource = _bangLoc;

                _lblSoLuong.Text = string.Format(
                    "Hiển thị: {0}/{1} xã/phường",
                    _bangLoc.Rows.Count,
                    _bangGoc.Rows.Count);

                HienThiChiTiet();
            }

            // ====================================================
            // ĐỌC GIÁ TRỊ AN TOÀN
            // ====================================================
            private long So(
                DataRow row,
                string field)
            {
                if (row == null ||
                    !row.Table.Columns.Contains(field) ||
                    row[field] == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToInt64(row[field]);
            }

            private string Chuoi(
                DataRow row,
                string field)
            {
                if (row == null ||
                    !row.Table.Columns.Contains(field) ||
                    row[field] == DBNull.Value)
                {
                    return "";
                }

                return Convert.ToString(row[field]);
            }

            private string GiaTriHienThi(
                DataRow row,
                string field,
                string format)
            {
                if (row[field] == DBNull.Value)
                    return "Chưa có";

                object value = row[field];

                if (string.IsNullOrWhiteSpace(format))
                    return Convert.ToString(value);

                return string.Format(
                    "{0:" + format + "}", value);
            }

            // ====================================================
            // BẢN GHI ĐƯỢC CHỌN
            // ====================================================
            private DataRow LayDongDangChon()
            {
                if (_grid.CurrentRow == null)
                    return null;

                DataRowView view =
                    _grid.CurrentRow.DataBoundItem
                        as DataRowView;

                return view == null ? null : view.Row;
            }

            // ====================================================
            // XEM CHI TIẾT KẾT QUẢ
            // ====================================================
            private void HienThiChiTiet()
            {
                DataRow row = LayDongDangChon();

                if (row == null)
                {
                    _txtChiTiet.Text =
                        "Chọn một xã/phường để xem phân tích.";

                    CapNhatNut();
                    return;
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(
                    "THÔNG TIN ĐỊA BÀN");

                sb.AppendLine();

                sb.AppendLine(
                    "Tên: " +
                    Chuoi(row, "ten_xa_phuong"));

                sb.AppendLine(
                    "Mã: " +
                    Chuoi(row, "ma_xa_phuong"));

                sb.AppendLine(
                    "Loại: " +
                    Chuoi(row, "loai_xa_phuong"));

                sb.AppendLine(
                    "Diện tích: " +
                    GiaTriHienThi(
                        row, "dien_tich_km2", "N2") +
                    " km²");

                sb.AppendLine();

                sb.AppendLine(
                    "PHÂN TÍCH HỒ SƠ");

                sb.AppendLine();

                sb.AppendLine(
                    "Số hồ sơ: " +
                    So(row, "so_ho_so_don_vi"));

                sb.AppendLine(
                    "Điểm đại diện khớp: " +
                    So(row, "so_diem_khop"));

                sb.AppendLine(
                    "Điểm đại diện không khớp: " +
                    So(row, "so_diem_khong_khop"));

                sb.AppendLine(
                    "Chưa kiểm tra được: " +
                    So(row,
                        "so_ho_so_chua_kiem_tra_duoc"));

                sb.AppendLine(
                    "Tỷ lệ khớp: " +
                    GiaTriHienThi(
                        row,
                        "ty_le_diem_khop_phan_tram",
                        "N2") +
                    "%");

                sb.AppendLine();
                sb.AppendLine(
                    "LƯU Ý:");

                sb.AppendLine(
                    "Đây là thống kê hồ sơ gắn với " +
                    "địa bàn hành chính.");

                sb.AppendLine();

                sb.AppendLine(
                    "Điểm GIS của đơn vị là điểm " +
                    "đại diện hành chính, không phải " +
                    "tọa độ chính xác của cơ sở quân đội.");

                _txtChiTiet.Text = sb.ToString();

                CapNhatNut();
            }

            // ====================================================
            // CHỌN / THU PHÓNG RANH GIỚI
            // ====================================================
            private void MoBanDo(bool thuPhong)
            {
                DataRow row = LayDongDangChon();

                if (row == null)
                {
                    MessageBox.Show(
                        "Hãy chọn một xã/phường trước.",
                        "Chưa chọn địa bàn",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                int id = Convert.ToInt32(
                    row["xa_phuong_id"]);

                try
                {
                    bool success =
                        _chonTrenBanDo(id, thuPhong);

                    if (!success)
                    {
                        MessageBox.Show(
                            "Chưa thể định vị xã/phường.\n\n" +
                            "Hãy kiểm tra lớp ranh giới " +
                            "xã/phường trên bản đồ.",
                            "Không thể định vị",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    DialogResult = DialogResult.OK;

                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể thực hiện thao tác " +
                        "với bản đồ.\n\n" +
                        ex.Message,
                        "Lỗi bản đồ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            private void CapNhatNut()
            {
                bool selected =
                    LayDongDangChon() != null;

                if (_btnChon != null)
                    _btnChon.Enabled = selected;

                if (_btnThuPhong != null)
                    _btnThuPhong.Enabled = selected;
            }

            // ====================================================
            // GIẢI PHÓNG DỮ LIỆU
            // ====================================================
            private void GiaiPhongBang()
            {
                if (_bangLoc != null)
                {
                    _bangLoc.Dispose();
                    _bangLoc = null;
                }

                if (_bangGoc != null)
                {
                    _bangGoc.Dispose();
                    _bangGoc = null;
                }
            }

            protected override void Dispose(
                bool disposing)
            {
                if (disposing)
                {
                    if (_grid != null)
                        _grid.DataSource = null;

                    GiaiPhongBang();
                }

                base.Dispose(disposing);
            }
        }
    }
}
