
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // QUẢN LÝ DỮ LIỆU GOOGLE AI STUDIO - V6.1
    //
    // Nguồn: HQTCSDL.web_mau
    // - web_mau.don_vi: 50 hồ sơ mẫu
    // - web_mau.quan_huyen: 30 địa bàn mẫu
    //
    // Chức năng:
    // - Quản lý danh sách tập trung.
    // - Tìm kiếm theo mã, tên, địa bàn.
    // - Xem chi tiết / JSON gốc.
    // - Làm mới dữ liệu từ PostgreSQL.
    // - Xuất CSV.
    //
    // Phiên bản này chỉ đọc dữ liệu.
    // Không sửa/xóa bảng ql_don_vi hoặc web_mau.
    // ============================================================

    public sealed class QuanLyWebMauControl : UserControl
    {
        private readonly string _connectionString;

        private readonly Color _nen =
            Color.FromArgb(9, 17, 33);

        private readonly Color _the =
            Color.FromArgb(15, 23, 42);

        private readonly Color _vien =
            Color.FromArgb(40, 56, 79);

        private readonly Color _chu =
            Color.FromArgb(235, 241, 249);

        private readonly Color _phu =
            Color.FromArgb(148, 163, 184);

        private readonly Color _xanh =
            Color.FromArgb(5, 150, 105);

        private ComboBox _nhom;
        private TextBox _tim;
        private TextBox _chiTiet;
        private DataGridView _bang;

        private Label _tongSo;
        private Label _trangThai;
        private Label _tieuDeChiTiet;

        private Button _thongTinButton;
        private Button _jsonButton;

        private DataTable _duLieu;
        private DataTable _ketQua;
        private DataRow _dongChon;

        private bool _dangNap;
        private bool _xemJson;

        public event EventHandler YeuCauMoBanDo;

        public QuanLyWebMauControl(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "Thiếu chuỗi kết nối PostgreSQL.");
            }

            _connectionString = connectionString;

            BackColor = _nen;
            ForeColor = _chu;
            Font = new Font("Segoe UI", 9F);
            Dock = DockStyle.Fill;

            TaoGiaoDien();
        }

        // ========================================================
        // GIAO DIỆN
        // ========================================================

        private Button TaoNut(string text, bool xanh)
        {
            Button b = new Button();
            b.Text = text;
            b.Height = 34;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = xanh
                ? _xanh
                : Color.FromArgb(30, 43, 64);
            b.ForeColor = Color.White;
            b.Cursor = Cursors.Hand;
            b.Font = new Font(
                "Segoe UI", 9F, FontStyle.Bold);

            return b;
        }

        private Label TaoNhan(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = _chu,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
        }

        private void TaoGiaoDien()
        {
            TableLayoutPanel main =
                new TableLayoutPanel();

            main.Dock = DockStyle.Fill;
            main.Padding = new Padding(16, 12, 16, 12);
            main.BackColor = _nen;
            main.ColumnCount = 1;
            main.RowCount = 4;

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 80));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            main.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            Controls.Add(main);

            // ====================================================
            // 1. TIÊU ĐỀ
            // ====================================================

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = _the;
            header.Padding = new Padding(16, 8, 16, 8);

            Label title = TaoNhan(
                "QUẢN LÝ DỮ LIỆU KHÔNG GIAN GIS");

            title.Font = new Font(
                "Segoe UI", 15F, FontStyle.Bold);

            title.ForeColor = Color.White;
            title.Dock = DockStyle.Top;
            title.Height = 34;

            Label description = TaoNhan(
                "Dữ liệu Google AI Studio | PostGIS web_mau | " +
                "50 hồ sơ và 30 địa bàn mẫu");

            description.ForeColor = _phu;
            description.Dock = DockStyle.Top;
            description.Height = 25;

            header.Controls.Add(description);
            header.Controls.Add(title);

            main.Controls.Add(header, 0, 0);

            // ====================================================
            // 2. BỘ LỌC
            // ====================================================

            TableLayoutPanel filters =
                new TableLayoutPanel();

            filters.Dock = DockStyle.Fill;
            filters.BackColor = _the;
            filters.Padding = new Padding(12, 10, 12, 10);
            filters.ColumnCount = 4;

            filters.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 28));

            filters.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 40));

            filters.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 16));

            filters.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 16));

            _nhom = new ComboBox();
            _nhom.Dock = DockStyle.Fill;
            _nhom.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _nhom.BackColor =
                Color.FromArgb(20, 31, 49);

            _nhom.ForeColor = Color.White;
            _nhom.FlatStyle = FlatStyle.Flat;

            _nhom.Items.Add("Hồ sơ đơn vị");
            _nhom.Items.Add("Địa bàn mẫu");

            _tim = new TextBox();
            _tim.Dock = DockStyle.Fill;
            _tim.Font = new Font("Segoe UI", 10F);
            _tim.BackColor = _nen;
            _tim.ForeColor = _chu;
            _tim.BorderStyle =
                BorderStyle.FixedSingle;

            Button lamMoi = TaoNut(
                "Làm mới", false);

            lamMoi.Dock = DockStyle.Fill;

            Button xoaLoc = TaoNut(
                "Xóa lọc", false);

            xoaLoc.Dock = DockStyle.Fill;

            filters.Controls.Add(_nhom, 0, 0);
            filters.Controls.Add(_tim, 1, 0);
            filters.Controls.Add(lamMoi, 2, 0);
            filters.Controls.Add(xoaLoc, 3, 0);

            main.Controls.Add(filters, 0, 1);

            // ====================================================
            // 3. NỘI DUNG CHÍNH
            // ====================================================

            TableLayoutPanel body =
                new TableLayoutPanel();

            body.Dock = DockStyle.Fill;
            body.ColumnCount = 2;
            body.RowCount = 1;
            body.Margin = new Padding(0, 10, 0, 10);

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 65));

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35));

            main.Controls.Add(body, 0, 2);

            _bang = new DataGridView();

            _bang.Dock = DockStyle.Fill;
            _bang.ReadOnly = true;
            _bang.AllowUserToAddRows = false;
            _bang.AllowUserToDeleteRows = false;
            _bang.AllowUserToResizeRows = false;
            _bang.MultiSelect = false;
            _bang.RowHeadersVisible = false;
            _bang.BackgroundColor = _nen;
            _bang.BorderStyle = BorderStyle.None;
            _bang.GridColor = _vien;
            _bang.AutoGenerateColumns = true;
            _bang.ColumnHeadersHeight = 38;

            _bang.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _bang.EnableHeadersVisualStyles = false;

            _bang.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 43, 64);

            _bang.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _bang.DefaultCellStyle.BackColor = _the;
            _bang.DefaultCellStyle.ForeColor = _chu;

            _bang.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(5, 83, 68);

            _bang.DefaultCellStyle.SelectionForeColor =
                Color.White;

            _bang.RowTemplate.Height = 34;

            body.Controls.Add(_bang, 0, 0);

            // ====================================================
            // 4. THÔNG TIN BẢN GHI
            // ====================================================

            TableLayoutPanel details =
                new TableLayoutPanel();

            details.Dock = DockStyle.Fill;
            details.BackColor = _the;
            details.Padding = new Padding(12);
            details.Margin = new Padding(10, 0, 0, 0);
            details.RowCount = 3;

            details.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42));

            details.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42));

            details.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            _tieuDeChiTiet = TaoNhan(
                "THÔNG TIN BẢN GHI");

            _tieuDeChiTiet.Font = new Font(
                "Segoe UI", 11F, FontStyle.Bold);

            details.Controls.Add(
                _tieuDeChiTiet, 0, 0);

            FlowLayoutPanel tabs =
                new FlowLayoutPanel();

            tabs.Dock = DockStyle.Fill;
            tabs.WrapContents = false;

            _thongTinButton =
                TaoNut("Thông tin", true);

            _thongTinButton.Width = 100;

            _jsonButton =
                TaoNut("JSON gốc", false);

            _jsonButton.Width = 100;

            Button saoChep =
                TaoNut("Sao chép", false);

            saoChep.Width = 100;

            tabs.Controls.Add(_thongTinButton);
            tabs.Controls.Add(_jsonButton);
            tabs.Controls.Add(saoChep);

            details.Controls.Add(tabs, 0, 1);

            _chiTiet = new TextBox();
            _chiTiet.Dock = DockStyle.Fill;
            _chiTiet.Multiline = true;
            _chiTiet.ReadOnly = true;
            _chiTiet.WordWrap = true;
            _chiTiet.ScrollBars =
                ScrollBars.Vertical;

            _chiTiet.BackColor = _the;
            _chiTiet.ForeColor = _chu;
            _chiTiet.BorderStyle =
                BorderStyle.None;

            _chiTiet.Font = new Font(
                "Segoe UI", 10F);

            details.Controls.Add(_chiTiet, 0, 2);

            body.Controls.Add(details, 1, 0);

            // ====================================================
            // 5. THANH CHỨC NĂNG DƯỚI
            // ====================================================

            TableLayoutPanel footer =
                new TableLayoutPanel();

            footer.Dock = DockStyle.Fill;
            footer.BackColor = _the;
            footer.Padding = new Padding(8, 6, 8, 6);
            footer.ColumnCount = 2;

            footer.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 60));

            footer.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 40));

            Panel status = new Panel();
            status.Dock = DockStyle.Fill;

            _tongSo = TaoNhan("0 bản ghi");
            _tongSo.Dock = DockStyle.Top;
            _tongSo.Height = 19;
            _tongSo.ForeColor =
                Color.FromArgb(110, 231, 183);

            _trangThai = TaoNhan(
                "Chưa nạp dữ liệu.");

            _trangThai.Dock = DockStyle.Top;
            _trangThai.Height = 19;
            _trangThai.ForeColor = _phu;

            status.Controls.Add(_trangThai);
            status.Controls.Add(_tongSo);

            footer.Controls.Add(status, 0, 0);

            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock = DockStyle.Fill;
            actions.FlowDirection =
                FlowDirection.RightToLeft;
            actions.WrapContents = false;

            Button banDo =
                TaoNut("Bản đồ GIS", true);

            banDo.Width = 115;

            Button xuatCsv =
                TaoNut("Xuất CSV", false);

            xuatCsv.Width = 105;

            actions.Controls.Add(banDo);
            actions.Controls.Add(xuatCsv);

            footer.Controls.Add(actions, 1, 0);

            main.Controls.Add(footer, 0, 3);

            // ====================================================
            // 6. SỰ KIỆN
            // ====================================================

            _nhom.SelectedIndexChanged += delegate
            {
                NapDuLieu();
            };

            _tim.TextChanged += delegate
            {
                LocDuLieu();
            };

            lamMoi.Click += delegate
            {
                NapDuLieu();
            };

            xoaLoc.Click += delegate
            {
                _tim.Clear();
            };

            _bang.SelectionChanged += delegate
            {
                ChonBanGhi();
            };

            _thongTinButton.Click += delegate
            {
                _xemJson = false;
                CapNhatTab();
                HienChiTiet();
            };

            _jsonButton.Click += delegate
            {
                _xemJson = true;
                CapNhatTab();
                HienChiTiet();
            };

            saoChep.Click += delegate
            {
                if (!string.IsNullOrEmpty(_chiTiet.Text))
                {
                    Clipboard.SetText(_chiTiet.Text);
                }
            };

            xuatCsv.Click += delegate
            {
                XuatCsv();
            };

            banDo.Click += delegate
            {
                if (YeuCauMoBanDo != null)
                {
                    YeuCauMoBanDo(
                        this, EventArgs.Empty);
                }
            };

            _nhom.SelectedIndex = 0;
        }

        // ========================================================
        // ĐỌC POSTGIS
        // ========================================================

        public void NapDuLieu()
        {
            if (_dangNap || _nhom.SelectedIndex < 0)
                return;

            const string sqlHoSo = @"
SELECT
    ma_don_vi AS ""Mã đơn vị"",
    ten_don_vi AS ""Tên đơn vị"",
    loai_don_vi AS ""Loại đơn vị"",
    quan_binh_chung AS ""Quân binh chủng"",
    quan_huyen AS ""Quận/huyện"",
    phuong AS ""Phường/xã"",
    dia_chi AS ""Địa chỉ mẫu"",
    jsonb_pretty(du_lieu_web) AS ""__json""
FROM web_mau.don_vi
ORDER BY ma_don_vi;";

            const string sqlDiaBan = @"
SELECT
    ma_dia_ban AS ""Mã địa bàn"",
    ten_dia_ban AS ""Tên địa bàn"",
    loai_dia_ban AS ""Loại địa bàn"",
    dien_tich_km2 AS ""Diện tích mẫu (km²)"",
    jsonb_pretty(du_lieu_web) AS ""__json""
FROM web_mau.quan_huyen
ORDER BY ma_dia_ban;";

            _dangNap = true;

            try
            {
                UseWaitCursor = true;

                DataTable moi = new DataTable();

                using (NpgsqlConnection conn =
                    new NpgsqlConnection(_connectionString))
                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(
                        _nhom.SelectedIndex == 0
                            ? sqlHoSo
                            : sqlDiaBan,
                        conn))
                using (NpgsqlDataAdapter adapter =
                    new NpgsqlDataAdapter(cmd))
                {
                    cmd.CommandTimeout = 25;
                    adapter.Fill(moi);
                }

                _bang.DataSource = null;

                if (_ketQua != null)
                {
                    _ketQua.Dispose();
                    _ketQua = null;
                }

                if (_duLieu != null)
                {
                    _duLieu.Dispose();
                }

                _duLieu = moi;

                _trangThai.Text =
                    "Đã cập nhật từ PostGIS web_mau.";

                LocDuLieu();
            }
            catch (Exception ex)
            {
                _trangThai.Text =
                    "Không thể tải dữ liệu PostGIS.";

                MessageBox.Show(
                    this,
                    ex.Message,
                    "Quản lý dữ liệu Web",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                _dangNap = false;
            }
        }

        // ========================================================
        // LỌC TRONG BỘ NHỚ
        // ========================================================

        private void LocDuLieu()
        {
            if (_duLieu == null)
                return;

            string keyword = _tim.Text.Trim();

            DataTable result = _duLieu.Clone();

            foreach (DataRow row in _duLieu.Rows)
            {
                bool match = keyword.Length == 0;

                if (!match)
                {
                    foreach (DataColumn col
                        in _duLieu.Columns)
                    {
                        if (col.ColumnName == "__json")
                            continue;

                        string value =
                            Convert.ToString(row[col]);

                        if (value.IndexOf(
                            keyword,
                            StringComparison
                                .CurrentCultureIgnoreCase) >= 0)
                        {
                            match = true;
                            break;
                        }
                    }
                }

                if (match)
                    result.ImportRow(row);
            }

            _bang.DataSource = null;

            if (_ketQua != null)
                _ketQua.Dispose();

            _ketQua = result;

            _bang.DataSource = _ketQua;

            if (_bang.Columns.Contains("__json"))
            {
                _bang.Columns["__json"].Visible = false;
            }

            foreach (DataGridViewColumn column
                in _bang.Columns)
            {
                if (column.Name != "__json")
                {
                    column.AutoSizeMode =
                        DataGridViewAutoSizeColumnMode
                            .DisplayedCells;
                }
            }

            _tongSo.Text =
                result.Rows.Count + "/" +
                _duLieu.Rows.Count + " bản ghi";

            _dongChon = result.Rows.Count > 0
                ? result.Rows[0]
                : null;

            HienChiTiet();
        }

        // ========================================================
        // CHỌN BẢN GHI
        // ========================================================

        private void ChonBanGhi()
        {
            if (_bang.CurrentRow == null)
                return;

            DataRowView view =
                _bang.CurrentRow.DataBoundItem
                as DataRowView;

            if (view == null)
                return;

            _dongChon = view.Row;

            HienChiTiet();
        }

        // ========================================================
        // HIỂN THỊ CHI TIẾT
        // ========================================================

        private void CapNhatTab()
        {
            _thongTinButton.BackColor =
                _xemJson
                    ? Color.FromArgb(30, 43, 64)
                    : _xanh;

            _jsonButton.BackColor =
                _xemJson
                    ? _xanh
                    : Color.FromArgb(30, 43, 64);
        }

        private void HienChiTiet()
        {
            if (_dongChon == null)
            {
                _tieuDeChiTiet.Text =
                    "THÔNG TIN BẢN GHI";

                _chiTiet.Text =
                    "Không có bản ghi phù hợp.";

                return;
            }

            string maCot = _nhom.SelectedIndex == 0
                ? "Mã đơn vị"
                : "Mã địa bàn";

            _tieuDeChiTiet.Text =
                "CHI TIẾT: " +
                Convert.ToString(_dongChon[maCot]);

            if (_xemJson)
            {
                _chiTiet.Font = new Font(
                    "Consolas", 9F);

                _chiTiet.Text =
                    Convert.ToString(
                        _dongChon["__json"]);
            }
            else
            {
                _chiTiet.Font = new Font(
                    "Segoe UI", 10F);

                StringBuilder sb =
                    new StringBuilder();

                sb.AppendLine("THÔNG TIN DỮ LIỆU");
                sb.AppendLine(
                    new string('-', 35));

                foreach (DataColumn col
                    in _dongChon.Table.Columns)
                {
                    if (col.ColumnName == "__json")
                        continue;

                    sb.AppendLine(col.ColumnName + ":");

                    sb.AppendLine(
                        "  " +
                        Convert.ToString(
                            _dongChon[col]));

                    sb.AppendLine();
                }

                sb.AppendLine(
                    "LƯU Ý: Dữ liệu mẫu " +
                    "Google AI Studio chưa được xác minh.");

                _chiTiet.Text = sb.ToString();
            }

            _chiTiet.SelectionStart = 0;
            _chiTiet.SelectionLength = 0;
            _chiTiet.ScrollToCaret();
        }

        // ========================================================
        // XUẤT CSV
        // ========================================================

        private static string CsvCell(object value)
        {
            string text = value == null ||
                          value == DBNull.Value
                ? ""
                : Convert.ToString(value);

            // Chống Excel hiểu dữ liệu là công thức.
            string trimmed = text.TrimStart();

            if (trimmed.Length > 0 &&
                "=+-@".IndexOf(trimmed[0]) >= 0)
            {
                text = "'" + text;
            }

            return "\"" +
                   text.Replace("\"", "\"\"") +
                   "\"";
        }

        private void XuatCsv()
        {
            if (_ketQua == null ||
                _ketQua.Rows.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Không có dữ liệu để xuất.",
                    "Xuất CSV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog save =
                new SaveFileDialog())
            {
                save.Filter =
                    "Tệp CSV (*.csv)|*.csv";

                save.FileName =
                    _nhom.SelectedIndex == 0
                        ? "HoSoWebMau.csv"
                        : "DiaBanWebMau.csv";

                if (save.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (StreamWriter writer =
                        new StreamWriter(
                            save.FileName,
                            false,
                            new UTF8Encoding(true)))
                    {
                        bool first = true;

                        foreach (DataColumn column
                            in _ketQua.Columns)
                        {
                            if (column.ColumnName == "__json")
                                continue;

                            if (!first)
                                writer.Write(",");

                            writer.Write(
                                CsvCell(column.ColumnName));

                            first = false;
                        }

                        writer.WriteLine();

                        foreach (DataRow row
                            in _ketQua.Rows)
                        {
                            first = true;

                            foreach (DataColumn column
                                in _ketQua.Columns)
                            {
                                if (column.ColumnName == "__json")
                                    continue;

                                if (!first)
                                    writer.Write(",");

                                writer.Write(
                                    CsvCell(row[column]));

                                first = false;
                            }

                            writer.WriteLine();
                        }
                    }

                    _trangThai.Text =
                        "Đã xuất " +
                        _ketQua.Rows.Count +
                        " bản ghi ra CSV.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        ex.Message,
                        "Không thể xuất CSV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ========================================================
        // DỌN DỮ LIỆU KHI ĐÓNG
        // ========================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bang != null)
                    _bang.DataSource = null;

                if (_ketQua != null)
                    _ketQua.Dispose();

                if (_duLieu != null)
                    _duLieu.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    // ============================================================
    // TÍCH HỢP VÀO FORM1
    // ============================================================

    public partial class Form1
    {
        private QuanLyWebMauControl _quanLyWebMau;

        private bool _daGanDieuHuongQuanLyWeb;

        private void MoQuanLyDuLieuTongHop()
        {
            if (panelMapWorkspace == null)
                return;

            try
            {
                AnTongQuan();

                if (_quanLyWebMau == null ||
                    _quanLyWebMau.IsDisposed)
                {
                    NpgsqlConnectionStringBuilder builder =
                        new NpgsqlConnectionStringBuilder();

                    builder.Host = PG_HOST;
                    builder.Port = Convert.ToInt32(PG_PORT);
                    builder.Database = PG_DB;
                    builder.Username = PG_USER;
                    builder.Password = PG_PASS;
                    builder.Timeout = 5;
                    builder.CommandTimeout = 25;
                    builder.Pooling = true;

                    _quanLyWebMau =
                        new QuanLyWebMauControl(
                            builder.ConnectionString);

                    _quanLyWebMau.Dock =
                        DockStyle.Fill;

                    _quanLyWebMau.YeuCauMoBanDo +=
                        delegate
                        {
                            foreach (Button button
                                in _modernNavButtons)
                            {
                                if (button != null &&
                                    button.Visible &&
                                    button.Text.Trim() ==
                                        "Bản đồ GIS")
                                {
                                    button.PerformClick();
                                    break;
                                }
                            }
                        };

                    panelMapWorkspace.Controls.Add(
                        _quanLyWebMau);
                }

                _quanLyWebMau.Visible = true;
                _quanLyWebMau.BringToFront();

                if (!_daGanDieuHuongQuanLyWeb)
                {
                    foreach (Button button
                        in _modernNavButtons)
                    {
                        if (button == null)
                            continue;

                        string ten = button.Text.Trim();

                        if (ten == "Quản lý dữ liệu")
                            continue;

                        button.Click += delegate
                        {
                            if (_quanLyWebMau != null &&
                                !_quanLyWebMau.IsDisposed)
                            {
                                _quanLyWebMau.Visible = false;
                            }
                        };
                    }

                    _daGanDieuHuongQuanLyWeb = true;
                }

                _quanLyWebMau.NapDuLieu();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Quản lý dữ liệu từ " +
                        "HQTCSDL.web_mau.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Không thể mở Quản lý dữ liệu Web.\n\n" +
                    ex.Message,
                    "Quản lý dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
