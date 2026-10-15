
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace MapWinGIS_TOI
{
    // ============================================================
    // PHÂN HỆ THỐNG KÊ VÀ BÁO CÁO - GIS HÀ NỘI
    //
    // CSDL: HQTCSDL
    // Schema: ql_don_vi
    //
    // Chức năng:
    // 1. Thống kê theo cấp đơn vị.
    // 2. Thống kê theo loại đơn vị.
    // 3. Thống kê theo xã/phường.
    // 4. Thống kê theo trạng thái.
    // 5. Biểu đồ thanh.
    // 6. Lọc và xuất CSV.
    //
    // Chỉ thực hiện SELECT, không sửa dữ liệu.
    // ============================================================

    public enum LoaiThongKeBaoCaoV2
    {
        TheoCapDonVi,
        TheoLoaiDonVi,
        TheoXaPhuong,
        TheoTrangThai
    }

    // ============================================================
    // KẾT NỐI PHÂN HỆ VỚI FORM1
    // ============================================================
    public partial class Form1
    {
        private bool _baoCaoDaKhoiTao = false;

        // Được gọi khi Form chính xuất hiện.
        // Không cần sửa Form1.cs.
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!_baoCaoDaKhoiTao)
            {
                BeginInvoke(
                    new Action(KhoiTaoThongKeBaoCao));
            }
        }

        private void KhoiTaoThongKeBaoCao()
        {
            if (_baoCaoDaKhoiTao)
                return;

            if (_modernNav == null)
                return;

            FlowLayoutPanel flow = null;

            foreach (Control control in _modernNav.Controls)
            {
                flow = control as FlowLayoutPanel;

                if (flow != null)
                    break;
            }

            if (flow == null)
                return;

            // Ẩn mục báo cáo cũ chưa được triển khai.
            foreach (Button button in _modernNavButtons)
            {
                if (button == null)
                    continue;

                string ten = button.Text == null
                    ? ""
                    : button.Text.Trim();

                if (ten == "Thống kê báo cáo" ||
                    ten == "Thống kê và báo cáo")
                {
                    button.Visible = false;
                }
            }

            // Thêm mục báo cáo thực sự hoạt động.
            Button btnBaoCao = CreateNavButton(
                "Thống kê và báo cáo",
                delegate
                {
                    AnTongQuan();
                    MoThongKeBaoCao();
                },
                false);

            flow.Controls.Add(btnBaoCao);

            // Đặt sau các chức năng chính.
            int viTri = Math.Min(
                6,
                flow.Controls.Count - 1);

            flow.Controls.SetChildIndex(
                btnBaoCao,
                viTri);

            _baoCaoDaKhoiTao = true;
        }

        private void MoThongKeBaoCao()
        {
            try
            {
                using (ThongKeBaoCaoFormV2 form =
                    new ThongKeBaoCaoFormV2(
                        DocDuLieuThongKeBaoCao))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "ThongKeBaoCao ERROR: " + ex);

                MessageBox.Show(
                    "Không thể mở phân hệ thống kê.\n\n" +
                    ex.Message,
                    "Lỗi thống kê",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ========================================================
        // ĐỌC DỮ LIỆU THỐNG KÊ TỪ POSTGIS
        //
        // Sử dụng cấu hình PostgreSQL có sẵn trong Form1.
        // Không khai báo thêm mật khẩu.
        // ========================================================
        private DataTable DocDuLieuThongKeBaoCao(
            LoaiThongKeBaoCaoV2 loai)
        {
            string sql;

            switch (loai)
            {
                case LoaiThongKeBaoCaoV2.TheoCapDonVi:

                    sql = @"
SELECT
    COALESCE(
        NULLIF(BTRIM(c.ten_cap), ''),
        'Chưa phân loại'
    ) AS nhom,
    COUNT(d.id)::bigint AS so_luong
FROM ql_don_vi.cap_don_vi c
LEFT JOIN ql_don_vi.don_vi_quan_doi d
    ON d.cap_don_vi_id = c.id
GROUP BY c.id, c.ten_cap
ORDER BY so_luong DESC, nhom;";
                    break;

                case LoaiThongKeBaoCaoV2.TheoLoaiDonVi:

                    sql = @"
SELECT
    COALESCE(
        NULLIF(BTRIM(l.ten_loai), ''),
        'Chưa phân loại'
    ) AS nhom,
    COUNT(d.id)::bigint AS so_luong
FROM ql_don_vi.loai_don_vi l
LEFT JOIN ql_don_vi.don_vi_quan_doi d
    ON d.loai_don_vi_id = l.id
GROUP BY l.id, l.ten_loai
ORDER BY so_luong DESC, nhom;";
                    break;

                case LoaiThongKeBaoCaoV2.TheoXaPhuong:

                    sql = @"
SELECT
    CONCAT(
        CASE
            WHEN loai_xa_phuong = 'phuong'
                THEN 'Phường '
            WHEN loai_xa_phuong = 'xa'
                THEN 'Xã '
            ELSE ''
        END,
        ten_xa_phuong
    ) AS nhom,
    so_ho_so_don_vi::bigint AS so_luong
FROM ql_don_vi.v_phan_tich_xa_phuong
ORDER BY so_luong DESC, nhom;";
                    break;

                case LoaiThongKeBaoCaoV2.TheoTrangThai:

                    sql = @"
SELECT
    CASE trang_thai_quan_ly
        WHEN 'dang_quan_ly'
            THEN 'Đang quản lý'
        WHEN 'tam_ngung'
            THEN 'Tạm ngừng'
        ELSE COALESCE(
            NULLIF(BTRIM(trang_thai_quan_ly), ''),
            'Chưa xác định'
        )
    END AS nhom,
    COUNT(*)::bigint AS so_luong
FROM ql_don_vi.don_vi_quan_doi
GROUP BY trang_thai_quan_ly
ORDER BY so_luong DESC, nhom;";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        "loai");
            }

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

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    builder.ConnectionString))
            using (NpgsqlDataAdapter adapter =
                new NpgsqlDataAdapter(sql, connection))
            {
                DataTable result = new DataTable();

                adapter.Fill(result);

                return result;
            }
        }
    }

    // ============================================================
    // GIAO DIỆN THỐNG KÊ VÀ BÁO CÁO
    // ============================================================
    public sealed class ThongKeBaoCaoFormV2 : Form
    {
        private readonly Func<
            LoaiThongKeBaoCaoV2,
            DataTable> _taiDuLieu;

        private ComboBox _cboBaoCao;
        private TextBox _txtTimKiem;
        private CheckBox _chkCoDuLieu;

        private DataGridView _grid;
        private Panel _panelBieuDo;

        private Label _lblTongHop;
        private Label _lblTrangThai;
        private Label _lblSoLuong;

        private DataTable _bangGoc;
        private DataTable _bangLoc;

        private long _tongHoSo;

        public ThongKeBaoCaoFormV2(
            Func<LoaiThongKeBaoCaoV2, DataTable> taiDuLieu)
        {
            if (taiDuLieu == null)
            {
                throw new ArgumentNullException(
                    "taiDuLieu");
            }

            _taiDuLieu = taiDuLieu;

            KhoiTaoGiaoDien();

            Load += delegate
            {
                NapBaoCao();
            };
        }

        // ========================================================
        // KHỞI TẠO GIAO DIỆN
        // ========================================================
        private void KhoiTaoGiaoDien()
        {
            Text = "Thống kê và báo cáo - GIS Hà Nội";

            StartPosition =
                FormStartPosition.CenterParent;

            Size = new Size(1200, 740);

            MinimumSize = new Size(950, 590);

            BackColor = Color.FromArgb(9, 17, 33);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.Padding = new Padding(0);

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 105));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 90));

            Controls.Add(layout);

            // ----------------------------------------------------
            // TIÊU ĐỀ
            // ----------------------------------------------------
            Panel header = new Panel();

            header.Dock = DockStyle.Fill;

            header.BackColor =
                Color.FromArgb(15, 23, 42);

            Label title = new Label();

            title.Text = "THỐNG KÊ VÀ BÁO CÁO";

            title.Font = new Font(
                "Segoe UI", 16F, FontStyle.Bold);

            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(20, 13);

            Label subtitle = new Label();

            subtitle.Text =
                "Thống kê hồ sơ quản lý và " +
                "dữ liệu hành chính trên địa bàn Hà Nội";

            subtitle.ForeColor =
                Color.FromArgb(148, 163, 184);

            subtitle.AutoSize = true;
            subtitle.Location = new Point(22, 53);

            _lblTongHop = new Label();

            _lblTongHop.Text =
                "Đang tải số liệu...";

            _lblTongHop.ForeColor =
                Color.FromArgb(52, 211, 153);

            _lblTongHop.AutoSize = true;
            _lblTongHop.Location = new Point(22, 78);

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(_lblTongHop);

            layout.Controls.Add(header, 0, 0);

            // ----------------------------------------------------
            // CHỌN LOẠI BÁO CÁO
            // ----------------------------------------------------
            FlowLayoutPanel filter =
                new FlowLayoutPanel();

            filter.Dock = DockStyle.Fill;
            filter.WrapContents = false;
            filter.AutoScroll = true;
            filter.Padding = new Padding(15, 14, 5, 5);

            filter.BackColor =
                Color.FromArgb(15, 23, 42);

            filter.Controls.Add(
                TaoNhan("Loại thống kê:", 110));

            _cboBaoCao = new ComboBox();

            _cboBaoCao.Width = 220;

            _cboBaoCao.DropDownStyle =
                ComboBoxStyle.DropDownList;

            _cboBaoCao.Items.Add(
                "Theo cấp đơn vị");

            _cboBaoCao.Items.Add(
                "Theo loại đơn vị");

            _cboBaoCao.Items.Add(
                "Theo xã/phường");

            _cboBaoCao.Items.Add(
                "Theo trạng thái quản lý");

            _cboBaoCao.SelectedIndexChanged +=
                delegate
                {
                    NapBaoCao();
                };

            filter.Controls.Add(_cboBaoCao);

            filter.Controls.Add(
                TaoNhan("Tìm nhóm:", 90));

            _txtTimKiem = new TextBox();

            _txtTimKiem.Width = 195;

            _txtTimKiem.TextChanged +=
                delegate
                {
                    ApDungBoLoc();
                };

            filter.Controls.Add(_txtTimKiem);

            _chkCoDuLieu = new CheckBox();

            _chkCoDuLieu.Text =
                "Chỉ hiện nhóm có hồ sơ";

            _chkCoDuLieu.ForeColor = Color.White;
            _chkCoDuLieu.AutoSize = true;
            _chkCoDuLieu.Margin =
                new Padding(15, 7, 4, 2);

            _chkCoDuLieu.CheckedChanged +=
                delegate
                {
                    ApDungBoLoc();
                };

            filter.Controls.Add(_chkCoDuLieu);

            layout.Controls.Add(filter, 0, 1);

            // ----------------------------------------------------
            // KẾT QUẢ: BẢNG VÀ BIỂU ĐỒ
            // ----------------------------------------------------
            TableLayoutPanel body =
                new TableLayoutPanel();

            body.Dock = DockStyle.Fill;
            body.Padding = new Padding(13, 10, 13, 10);
            body.ColumnCount = 2;
            body.RowCount = 1;

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 53F));

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 47F));

            // BẢNG SỐ LIỆU
            _grid = new DataGridView();

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AutoGenerateColumns = false;

            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.RowHeadersVisible = false;

            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _grid.MultiSelect = false;

            _grid.BackgroundColor =
                Color.FromArgb(9, 17, 33);

            _grid.GridColor =
                Color.FromArgb(51, 65, 85);

            _grid.BorderStyle = BorderStyle.None;
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

            ThemCot("nhom", "Nhóm thống kê", 235);
            ThemCot("so_luong", "Số hồ sơ", 105);
            ThemCot("ty_le", "Tỷ lệ (%)", 105);

            body.Controls.Add(_grid, 0, 0);

            // BIỂU ĐỒ
            Panel chartContainer = new Panel();

            chartContainer.Dock = DockStyle.Fill;

            chartContainer.Margin =
                new Padding(12, 0, 0, 0);

            chartContainer.BackColor =
                Color.FromArgb(15, 23, 42);

            Label chartTitle = new Label();

            chartTitle.Text =
                "BIỂU ĐỒ PHÂN BỐ HỒ SƠ";

            chartTitle.Font =
                new Font(
                    "Segoe UI", 10F, FontStyle.Bold);

            chartTitle.Dock = DockStyle.Top;
            chartTitle.Height = 48;

            chartTitle.Padding =
                new Padding(14, 14, 0, 0);

            _panelBieuDo = new Panel();

            _panelBieuDo.Dock = DockStyle.Fill;

            _panelBieuDo.BackColor =
                Color.FromArgb(15, 23, 42);

            _panelBieuDo.Paint +=
                VeBieuDo;

            _panelBieuDo.Resize += delegate
            {
                _panelBieuDo.Invalidate();
            };

            Label chartNote = new Label();

            chartNote.Text =
                "Hiển thị tối đa 10 nhóm có số hồ sơ lớn nhất.";

            chartNote.Dock = DockStyle.Bottom;
            chartNote.Height = 45;

            chartNote.Padding =
                new Padding(14, 7, 0, 0);

            chartNote.ForeColor =
                Color.FromArgb(148, 163, 184);

            chartContainer.Controls.Add(_panelBieuDo);
            chartContainer.Controls.Add(chartTitle);
            chartContainer.Controls.Add(chartNote);

            body.Controls.Add(chartContainer, 1, 0);

            layout.Controls.Add(body, 0, 2);

            // ----------------------------------------------------
            // CÁC THAO TÁC
            // ----------------------------------------------------
            Panel footer = new Panel();

            footer.Dock = DockStyle.Fill;

            footer.BackColor =
                Color.FromArgb(15, 23, 42);

            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock = DockStyle.Top;
            actions.Height = 47;

            actions.Padding = new Padding(14, 10, 0, 0);

            Button btnLamMoi =
                TaoNut("Làm mới", 105);

            Button btnXuat =
                TaoNut("Xuất CSV", 115);

            Button btnDong =
                TaoNut("Đóng", 85);

            btnLamMoi.Click += delegate
            {
                NapBaoCao();
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
                new Point(19, 60);

            _lblTrangThai = new Label();

            _lblTrangThai.AutoSize = true;

            _lblTrangThai.ForeColor =
                Color.FromArgb(148, 163, 184);

            _lblTrangThai.Location =
                new Point(220, 60);

            footer.Controls.Add(actions);
            footer.Controls.Add(_lblSoLuong);
            footer.Controls.Add(_lblTrangThai);

            layout.Controls.Add(footer, 0, 3);

            // Thiết lập sau khi giao diện hoàn chỉnh.
            _cboBaoCao.SelectedIndex = 0;
        }

        // ========================================================
        // ĐIỀU KHIỂN CHUNG
        // ========================================================
        private Label TaoNhan(string text, int width)
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

        private Button TaoNut(string text, int width)
        {
            Button button = new Button();

            button.Text = text;
            button.Width = width;
            button.Height = 30;

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

            if (field == "ty_le")
            {
                column.DefaultCellStyle.Format = "N2";
            }

            _grid.Columns.Add(column);
        }

        // ========================================================
        // TẢI BÁO CÁO
        // ========================================================
        private void NapBaoCao()
        {
            if (_cboBaoCao == null ||
                _cboBaoCao.SelectedIndex < 0 ||
                _lblTrangThai == null)
            {
                return;
            }

            Cursor oldCursor = Cursor.Current;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                _lblTrangThai.Text =
                    "Đang đọc số liệu...";

                _grid.DataSource = null;

                GiaiPhongBang();

                LoaiThongKeBaoCaoV2 loai =
                    (LoaiThongKeBaoCaoV2)
                    _cboBaoCao.SelectedIndex;

                _bangGoc = _taiDuLieu(loai);

                if (_bangGoc == null)
                {
                    throw new InvalidOperationException(
                        "Không có dữ liệu báo cáo.");
                }

                if (!_bangGoc.Columns.Contains("nhom") ||
                    !_bangGoc.Columns.Contains("so_luong"))
                {
                    throw new InvalidOperationException(
                        "Báo cáo thiếu cột bắt buộc.");
                }

                _tongHoSo = 0;

                foreach (DataRow row in _bangGoc.Rows)
                {
                    _tongHoSo +=
                        Convert.ToInt64(row["so_luong"]);
                }

                _bangGoc.Columns.Add(
                    "ty_le",
                    typeof(decimal));

                foreach (DataRow row in _bangGoc.Rows)
                {
                    long soLuong =
                        Convert.ToInt64(row["so_luong"]);

                    row["ty_le"] =
                        _tongHoSo == 0
                            ? 0m
                            : Math.Round(
                                100m * soLuong / _tongHoSo,
                                2);
                }

                _lblTongHop.Text =
                    string.Format(
                        "Tổng cộng: {0:N0} hồ sơ  |  " +
                        "{1:N0} nhóm thống kê",
                        _tongHoSo,
                        _bangGoc.Rows.Count);

                ApDungBoLoc();

                _lblTrangThai.Text =
                    "Dữ liệu cập nhật từ PostGIS.";
            }
            catch (Exception ex)
            {
                _grid.DataSource = null;

                GiaiPhongBang();

                _tongHoSo = 0;

                _lblTongHop.Text =
                    "Không đọc được số liệu.";

                _lblSoLuong.Text = "0 nhóm";

                _lblTrangThai.Text =
                    "Tải báo cáo thất bại.";

                _panelBieuDo.Invalidate();

                MessageBox.Show(
                    "Không thể tải báo cáo.\n\n" +
                    ex.Message,
                    "Lỗi báo cáo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }

        // ========================================================
        // LỌC NHÓM THỐNG KÊ
        // ========================================================
        private void ApDungBoLoc()
        {
            if (_bangGoc == null ||
                _txtTimKiem == null ||
                _chkCoDuLieu == null)
            {
                return;
            }

            string keyword =
                _txtTimKiem.Text.Trim();

            DataTable filtered =
                _bangGoc.Clone();

            foreach (DataRow row in _bangGoc.Rows)
            {
                string ten =
                    Convert.ToString(row["nhom"]);

                long count =
                    Convert.ToInt64(row["so_luong"]);

                bool khopTen =
                    string.IsNullOrWhiteSpace(keyword) ||
                    ten.IndexOf(
                        keyword,
                        StringComparison.CurrentCultureIgnoreCase)
                    >= 0;

                bool khopDuLieu =
                    !_chkCoDuLieu.Checked ||
                    count > 0;

                if (khopTen && khopDuLieu)
                {
                    filtered.ImportRow(row);
                }
            }

            _grid.DataSource = null;

            if (_bangLoc != null)
                _bangLoc.Dispose();

            _bangLoc = filtered;

            _grid.DataSource = _bangLoc;

            _lblSoLuong.Text =
                string.Format(
                    "Hiển thị: {0}/{1} nhóm",
                    _bangLoc.Rows.Count,
                    _bangGoc.Rows.Count);

            _panelBieuDo.Invalidate();
        }

        // ========================================================
        // VẼ BIỂU ĐỒ THANH
        //
        // Dùng GDI+ có sẵn trong WinForms.
        // Không cần thêm thư viện Chart.
        // ========================================================
        private void VeBieuDo(
            object sender,
            PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.FromArgb(15, 23, 42));

            if (_bangLoc == null ||
                _bangLoc.Rows.Count == 0)
            {
                using (Brush brush =
                    new SolidBrush(
                        Color.FromArgb(148, 163, 184)))
                {
                    g.DrawString(
                        "Không có dữ liệu để vẽ biểu đồ.",
                        Font,
                        brush,
                        18,
                        20);
                }

                return;
            }

            // Chọn tối đa 10 nhóm có số hồ sơ lớn nhất
            // trong dữ liệu đang hiển thị.
            DataView view = new DataView(_bangLoc);

            view.Sort = "so_luong DESC, nhom ASC";

            int soThanh = Math.Min(10, view.Count);

            long giaTriLonNhat = 0;

            for (int i = 0; i < soThanh; i++)
            {
                long value = Convert.ToInt64(
                    view[i]["so_luong"]);

                if (value > giaTriLonNhat)
                    giaTriLonNhat = value;
            }

            int left = 175;
            int top = 20;
            int barHeight = 18;
            int rowHeight = 31;

            int barMaxWidth =
                Math.Max(20, _panelBieuDo.Width - 245);

            int soDongVeDuoc =
                Math.Max(
                    1,
                    (_panelBieuDo.Height - top - 20)
                    / rowHeight);

            soThanh = Math.Min(
                soThanh,
                soDongVeDuoc);

            using (Font chartFont =
                new Font("Segoe UI", 8F))
            using (Brush labelBrush =
                new SolidBrush(
                    Color.FromArgb(203, 213, 225)))
            using (Brush barBrush =
                new SolidBrush(
                    Color.FromArgb(16, 185, 129)))
            using (Brush valueBrush =
                new SolidBrush(Color.White))
            {
                for (int i = 0; i < soThanh; i++)
                {
                    string label =
                        Convert.ToString(view[i]["nhom"]);

                    long value =
                        Convert.ToInt64(
                            view[i]["so_luong"]);

                    if (label.Length > 23)
                    {
                        label =
                            label.Substring(0, 20) + "...";
                    }

                    int y = top + i * rowHeight;

                    g.DrawString(
                        label,
                        chartFont,
                        labelBrush,
                        new RectangleF(
                            8, y, 162, 26));

                    int width = 0;

                    if (giaTriLonNhat > 0)
                    {
                        width = (int)Math.Round(
                            barMaxWidth *
                            (double)value /
                            giaTriLonNhat);
                    }

                    if (width > 0)
                    {
                        g.FillRectangle(
                            barBrush,
                            left,
                            y + 2,
                            width,
                            barHeight);
                    }

                    g.DrawString(
                        value.ToString("N0"),
                        chartFont,
                        valueBrush,
                        left + width + 6,
                        y + 2);
                }
            }
        }

        // ========================================================
        // XUẤT BẢNG ĐANG HIỂN THỊ RA CSV
        //
        // UTF-8 BOM để Excel Windows đọc tiếng Việt.
        // ========================================================
        private void XuatCsv()
        {
            if (_bangLoc == null ||
                _bangLoc.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Không có dữ liệu để xuất.",
                    "Xuất báo cáo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Lưu báo cáo thống kê";

                dialog.Filter =
                    "Tệp CSV (*.csv)|*.csv";

                dialog.FileName =
                    "ThongKeGIS_HaNoi_" +
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
                    StringBuilder sb =
                        new StringBuilder();

                    sb.AppendLine(
                        "Nhóm thống kê,Số hồ sơ,Tỷ lệ (%)");

                    foreach (DataRow row
                        in _bangLoc.Rows)
                    {
                        string nhom =
                            Convert.ToString(row["nhom"]);

                        string count =
                            Convert.ToString(row["so_luong"]);

                        string ratio =
                            Convert.ToDecimal(
                                row["ty_le"])
                            .ToString(
                                "0.00",
                                System.Globalization
                                    .CultureInfo.InvariantCulture);

                        sb.Append(
                            ChuanHoaCsv(nhom));

                        sb.Append(",");

                        sb.Append(
                            ChuanHoaCsv(count));

                        sb.Append(",");

                        sb.AppendLine(
                            ChuanHoaCsv(ratio));
                    }

                    File.WriteAllText(
                        dialog.FileName,
                        sb.ToString(),
                        new UTF8Encoding(true));

                    _lblTrangThai.Text =
                        "Đã xuất báo cáo CSV.";

                    MessageBox.Show(
                        "Xuất báo cáo thành công.\n\n" +
                        dialog.FileName,
                        "Xuất báo cáo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể xuất báo cáo.\n\n" +
                        ex.Message,
                        "Lỗi xuất CSV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private static string ChuanHoaCsv(string value)
        {
            if (value == null)
                value = "";

            // Hạn chế công thức được thực thi
            // khi mở CSV trong Excel.
            if (value.StartsWith("=") ||
                value.StartsWith("+") ||
                value.StartsWith("-") ||
                value.StartsWith("@") ||
                value.StartsWith("\t") ||
                value.StartsWith("\r"))
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

        protected override void Dispose(bool disposing)
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
