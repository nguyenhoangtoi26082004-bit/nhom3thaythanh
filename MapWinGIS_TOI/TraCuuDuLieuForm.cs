
using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;
using MapWinGIS;

using Label = System.Windows.Forms.Label;
using Point = System.Drawing.Point;

namespace MapWinGIS_TOI
{
    // ============================================================
    // TICH HOP DU LIEU GOOGLE AI STUDIO VAO FORM1
    // ============================================================
    public partial class Form1
    {
        private bool _webMauDaKhoiTao;

        private void KhoiTaoDuLieuWebMau()
        {
            if (_webMauDaKhoiTao || _modernNav == null)
                return;

            Button nut = new Button();
            nut.Name = "btnDuLieuWebMau";
            nut.Text = "Bộ dữ liệu Web (mẫu)";
            nut.Dock = DockStyle.Bottom;
            nut.Height = 42;
            nut.FlatStyle = FlatStyle.Flat;
            nut.FlatAppearance.BorderSize = 1;
            nut.FlatAppearance.BorderColor =
                Color.FromArgb(5, 150, 105);
            nut.BackColor = Color.FromArgb(15, 23, 42);
            nut.ForeColor = Color.FromArgb(110, 231, 183);
            nut.Cursor = Cursors.Hand;
            nut.Font = new Font(
                "Segoe UI", 8.5F, FontStyle.Bold);

            nut.Click += delegate
            {
                MoDuLieuWebMau();
            };

            _modernNav.Controls.Add(nut);
            nut.BringToFront();

            _webMauDaKhoiTao = true;
        }

        // Hien thi lop diem mau tren MapWinGIS.
        private void HienThiWebMauTrenBanDo()
        {
            const string key = "web_mau_don_vi";

            if (axMap2 == null)
                return;

            try
            {
                AnTongQuan();

                int handle;

                if (_loadedLayers.TryGetValue(
                    key, out handle))
                {
                    try
                    {
                        axMap2.set_LayerVisible(
                            handle, true);

                        axMap2.ZoomToLayer(handle);
                        axMap2.Redraw();

                        return;
                    }
                    catch
                    {
                        _loadedLayers.Remove(key);
                    }
                }

                EnsureLocalMapProjection32648();

                bool oldReproject =
                    _globalSettings.ReprojectLayersOnAdding;

                bool oldMismatch =
                    _globalSettings.AllowProjectionMismatch;

                bool oldNoProjection =
                    _globalSettings.AllowLayersWithoutProjections;

                handle = -1;

                try
                {
                    _globalSettings
                        .ReprojectLayersOnAdding = false;

                    _globalSettings
                        .AllowProjectionMismatch = true;

                    _globalSettings
                        .AllowLayersWithoutProjections = true;

                    const string sql =
                        "SELECT row_number() OVER " +
                        "(ORDER BY ma_don_vi)::integer AS id, " +
                        "ma_don_vi, ten_don_vi, geom " +
                        "FROM web_mau.v_don_vi_ban_do " +
                        "WHERE geom IS NOT NULL";

                    handle = axMap2.AddLayerFromDatabase(
                        PgConnectionString,
                        sql,
                        true);
                }
                finally
                {
                    _globalSettings.ReprojectLayersOnAdding =
                        oldReproject;

                    _globalSettings.AllowProjectionMismatch =
                        oldMismatch;

                    _globalSettings.AllowLayersWithoutProjections =
                        oldNoProjection;
                }

                if (handle < 0)
                {
                    throw new InvalidOperationException(
                        "Không thể nạp các điểm mẫu từ web_mau.");
                }

                axMap2.set_LayerName(
                    handle,
                    "WEB MẪU - Điểm đơn vị (chưa xác minh)");

                _loadedLayers[key] = handle;

                SetLoadedLayerProjection32648(handle);

                Shapefile sf = axMap2.get_Shapefile(handle);

                if (sf != null)
                {
                    Utils mau = new Utils();

                    sf.DefaultDrawingOptions.PointSize = 10;

                    sf.DefaultDrawingOptions.FillColor =
                        mau.ColorByName(tkMapColor.Yellow);

                    sf.DefaultDrawingOptions.LineColor =
                        mau.ColorByName(tkMapColor.Black);

                    sf.DefaultDrawingOptions.LineWidth = 1.5f;
                }

                if (treeLayers != null)
                {
                    TreeNode node = new TreeNode(
                        "WEB MẪU - Điểm đơn vị (chưa xác minh)");

                    node.Name = key;
                    node.Tag = "FILE";

                    treeLayers.Nodes.Add(node);

                    SetLayerNodeChecked(node, true);
                }

                try
                {
                    RefreshLayerCombo();
                }
                catch
                {
                }

                axMap2.ZoomToLayer(handle);
                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang xem điểm GIS MẪU WEB - " +
                        "chưa xác minh vị trí.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Lỗi hiển thị điểm mẫu trên bản đồ.\n\n" +
                    ex.Message,
                    "MapWinGIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Mo cua so du lieu web.
        private void MoDuLieuWebMau()
        {
            try
            {
                NpgsqlConnectionStringBuilder b =
                    new NpgsqlConnectionStringBuilder();

                b.Host = PG_HOST;
                b.Port = Convert.ToInt32(PG_PORT);
                b.Database = PG_DB;
                b.Username = PG_USER;
                b.Password = PG_PASS;
                b.Timeout = 5;
                b.CommandTimeout = 30;

                bool moBanDo = false;

                using (TraCuuDuLieuForm form =
                    new TraCuuDuLieuForm(
                        b.ConnectionString,
                        HienThiWebMauTrenBanDo))
                {
                    form.ShowDialog(this);

                    moBanDo = form.YeuCauMoBanDo;
                }

                if (moBanDo)
                {
                    HienThiWebMauTrenBanDo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Không thể mở dữ liệu Google AI Studio.\n\n" +
                    ex.Message,
                    "Dữ liệu Web",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    // ============================================================
    // GIAO DIEN KHAI THAC DU LIEU WEB - PHIEN BAN V5
    // ============================================================
    public sealed class TraCuuDuLieuForm : Form
    {
        private readonly string _connectionString;

        private readonly Color _nen =
            Color.FromArgb(8, 13, 25);

        private readonly Color _the =
            Color.FromArgb(15, 23, 42);

        private readonly Color _vien =
            Color.FromArgb(40, 56, 79);

        private readonly Color _chu =
            Color.FromArgb(229, 237, 247);

        private readonly Color _phu =
            Color.FromArgb(155, 175, 198);

        private readonly Color _xanh =
            Color.FromArgb(5, 150, 105);

        private TextBox _tim;
        private TextBox _chiTiet;

        private ComboBox _loai;
        private ComboBox _diaBan;

        private Button _hoSoButton;
        private Button _diaBanButton;
        private Button _theButton;
        private Button _bangButton;

        private Button _mapButton;
        private Button _jsonButton;
        private Button _copyButton;

        private Label _soLuong;
        private Label _trangThai;
        private Label _tieuDeChiTiet;

        private Panel _ketQuaPanel;

        private FlowLayoutPanel _thePanel;

        private DataGridView _bang;

        private DataTable _hoSo;
        private DataTable _diaBanMau;
        private DataTable _loc;

        private DataRow _dongChon;

        private bool _xemHoSo = true;
        private bool _xemThe = true;
        private bool _dangLoc;
        private bool _dangVe;
        private bool _dangJson;

        private int _rongTheCu = -1;

        public bool YeuCauMoBanDo
        {
            get;
            private set;
        }

        public TraCuuDuLieuForm(
            string connectionString,
            Action hienTrenBanDo)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "Thiếu kết nối PostgreSQL.");
            }

            _connectionString = connectionString;

            TaoGiaoDien();

            Shown += delegate
            {
                DoiNhom(true);
            };
        }

        // ========================================================
        // CAC THANH PHAN GIAO DIEN DUNG CHUNG
        // ========================================================
        private Label Nhan(
            string text,
            bool bold = false)
        {
            return new Label
            {
                Text = text,
                ForeColor = _chu,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    bold
                        ? FontStyle.Bold
                        : FontStyle.Regular)
            };
        }

        private Button Nut(
            string text,
            bool active = false)
        {
            Button b = new Button
            {
                Text = text,
                BackColor = active
                    ? _xanh
                    : Color.FromArgb(30, 43, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                Cursor = Cursors.Hand,
                Height = 34,
                AutoSize = false,
                Margin = new Padding(3, 4, 4, 4)
            };

            b.FlatAppearance.BorderSize = 0;

            return b;
        }

        private ComboBox TaoCombo()
        {
            ComboBox c = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 31, 49),
                ForeColor = _chu,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(4, 3, 12, 1)
            };

            c.SelectedIndexChanged += delegate
            {
                LocTrongBoNho();
            };

            return c;
        }

        // ========================================================
        // XAY DUNG TOAN BO GIAO DIEN
        // ========================================================
        private void TaoGiaoDien()
        {
            Text =
                "Khai thác dữ liệu Google AI Studio - " +
                "bản mẫu chưa xác minh";

            StartPosition = FormStartPosition.CenterParent;

            Size = new Size(1280, 790);

            MinimumSize = new Size(1000, 650);

            BackColor = _nen;
            ForeColor = _chu;

            Font = new Font("Segoe UI", 9F);

            DoubleBuffered = true;

            TableLayoutPanel layout =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = _nen,
                    Padding = new Padding(14),
                    ColumnCount = 1,
                    RowCount = 5
                };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 90F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 51F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 84F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 54F));

            Controls.Add(layout);

            // --------------------------------------------
            // HEADER
            // --------------------------------------------
            TableLayoutPanel header =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = _the,
                    Margin = new Padding(0, 0, 0, 8),
                    Padding = new Padding(14, 7, 14, 7)
                };

            header.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 75F));

            header.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 25F));

            Panel heading = new Panel
            {
                Dock = DockStyle.Fill
            };

            Label title = Nhan(
                "KHAI THÁC DỮ LIỆU GOOGLE AI STUDIO",
                true);

            title.Dock = DockStyle.Top;
            title.Height = 33;

            title.Font = new Font(
                "Segoe UI", 16F, FontStyle.Bold);

            Label hint = Nhan(
                "PostGIS: web_mau | " +
                "Dữ liệu trình diễn, tọa độ chưa xác minh");

            hint.ForeColor = _phu;
            hint.Dock = DockStyle.Top;
            hint.Height = 24;

            heading.Controls.Add(hint);
            heading.Controls.Add(title);

            header.Controls.Add(heading, 0, 0);

            _soLuong = Nhan(
                "Đang tải...",
                true);

            _soLuong.Dock = DockStyle.Fill;

            _soLuong.TextAlign =
                ContentAlignment.MiddleRight;

            _soLuong.ForeColor =
                Color.FromArgb(110, 231, 183);

            header.Controls.Add(_soLuong, 1, 0);

            layout.Controls.Add(header, 0, 0);

            // --------------------------------------------
            // THANH CHUYEN NHOM / CHE DO XEM
            // --------------------------------------------
            TableLayoutPanel thanh =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = new Padding(0),
                    BackColor = _nen
                };

            thanh.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 61F));

            thanh.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 39F));

            FlowLayoutPanel trai =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    AutoScroll = false,
                    Margin = Padding.Empty
                };

            _hoSoButton = Nut(
                "50 hồ sơ mẫu", true);

            _hoSoButton.Width = 160;

            _diaBanButton = Nut(
                "30 địa bàn mẫu");

            _diaBanButton.Width = 172;

            _hoSoButton.Click += delegate
            {
                DoiNhom(true);
            };

            _diaBanButton.Click += delegate
            {
                DoiNhom(false);
            };

            trai.Controls.Add(_hoSoButton);
            trai.Controls.Add(_diaBanButton);

            FlowLayoutPanel phai =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.RightToLeft,
                    WrapContents = false,
                    AutoScroll = false,
                    Margin = Padding.Empty
                };

            _bangButton = Nut("Dạng bảng");
            _bangButton.Width = 110;

            _theButton = Nut("Dạng thẻ", true);
            _theButton.Width = 110;

            _theButton.Click += delegate
            {
                DoiCheDo(true);
            };

            _bangButton.Click += delegate
            {
                DoiCheDo(false);
            };

            phai.Controls.Add(_bangButton);
            phai.Controls.Add(_theButton);

            thanh.Controls.Add(trai, 0, 0);
            thanh.Controls.Add(phai, 1, 0);

            layout.Controls.Add(thanh, 0, 1);

            // --------------------------------------------
            // BO LOC
            // --------------------------------------------
            TableLayoutPanel loc =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = _the,
                    Padding = new Padding(12, 6, 12, 8),
                    Margin = new Padding(0, 0, 0, 8),
                    ColumnCount = 4,
                    RowCount = 2
                };

            loc.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 37F));

            loc.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 25F));

            loc.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 26F));

            loc.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 12F));

            loc.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 24F));

            loc.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100F));

            string[] captions =
            {
                "TỪ KHÓA TÌM KIẾM",
                "LOẠI ĐƠN VỊ / ĐỊA BÀN",
                "QUẬN / HUYỆN",
                ""
            };

            for (int i = 0; i < 4; i++)
            {
                Label l = Nhan(captions[i], true);

                l.ForeColor = _phu;
                l.Dock = DockStyle.Fill;

                l.Font = new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold);

                l.Margin = new Padding(4, 0, 4, 0);

                loc.Controls.Add(l, i, 0);
            }

            _tim = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _nen,
                ForeColor = _chu,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(4, 3, 12, 1)
            };

            _tim.TextChanged += delegate
            {
                LocTrongBoNho();
            };

            _loai = TaoCombo();
            _diaBan = TaoCombo();

            Button xoa = Nut("Xóa lọc");

            xoa.Dock = DockStyle.Fill;

            xoa.Margin =
                new Padding(4, 1, 1, 1);

            xoa.Click += delegate
            {
                _dangLoc = true;

                try
                {
                    _tim.Clear();

                    if (_loai.Items.Count > 0)
                        _loai.SelectedIndex = 0;

                    if (_diaBan.Items.Count > 0)
                        _diaBan.SelectedIndex = 0;
                }
                finally
                {
                    _dangLoc = false;
                }

                LocTrongBoNho();
            };

            loc.Controls.Add(_tim, 0, 1);
            loc.Controls.Add(_loai, 1, 1);
            loc.Controls.Add(_diaBan, 2, 1);
            loc.Controls.Add(xoa, 3, 1);

            layout.Controls.Add(loc, 0, 2);

            // --------------------------------------------
            // VUNG NOI DUNG
            // --------------------------------------------
            TableLayoutPanel body =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = Padding.Empty
                };

            body.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 64F));

            body.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 36F));

            _ketQuaPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _nen,
                Margin = new Padding(0, 2, 8, 5)
            };

            body.Controls.Add(
                _ketQuaPanel, 0, 0);

            // --------------------------------------------
            // BANG THUOC TINH
            // --------------------------------------------
            _bang = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                BorderStyle = BorderStyle.None,
                BackgroundColor = _nen,
                GridColor = _vien,
                ColumnHeadersHeight = 36,
                EnableHeadersVisualStyles = false
            };

            _bang.RowTemplate.Height = 33;

            _bang.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(29, 42, 62);

            _bang.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _bang.DefaultCellStyle.BackColor = _the;
            _bang.DefaultCellStyle.ForeColor = _chu;

            _bang.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(5, 83, 68);

            _bang.DefaultCellStyle.SelectionForeColor =
                Color.White;

            _bang.SelectionChanged += delegate
            {
                ChonDongBang();
            };

            _ketQuaPanel.Controls.Add(_bang);

            // --------------------------------------------
            // DANH SACH DANG THE
            // --------------------------------------------
            _thePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection =
                    FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(4),
                BackColor = _nen
            };

            _thePanel.SizeChanged += delegate
            {
                if (_xemThe &&
                    !_dangVe &&
                    _loc != null &&
                    Math.Abs(
                        _thePanel.ClientSize.Width -
                        _rongTheCu) > 20)
                {
                    VeThe();
                }
            };

            _ketQuaPanel.Controls.Add(_thePanel);

            // --------------------------------------------
            // BANG CHI TIET BEN PHAI
            // --------------------------------------------
            Panel detail = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _the,
                Margin = new Padding(6, 2, 0, 5),
                Padding = new Padding(12)
            };

            body.Controls.Add(detail, 1, 0);

            TableLayoutPanel detailLayout =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3
                };

            detailLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 40F));

            detailLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 42F));

            detailLayout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100F));

            detail.Controls.Add(detailLayout);

            _tieuDeChiTiet = Nhan(
                "THÔNG TIN HỒ SƠ", true);

            _tieuDeChiTiet.Dock = DockStyle.Fill;

            _tieuDeChiTiet.ForeColor = Color.White;

            _tieuDeChiTiet.Font = new Font(
                "Segoe UI",
                11F,
                FontStyle.Bold);

            detailLayout.Controls.Add(
                _tieuDeChiTiet, 0, 0);

            FlowLayoutPanel detailTabs =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    Margin = Padding.Empty,
                    AutoScroll = false
                };

            _jsonButton = Nut("JSON gốc");
            _jsonButton.Width = 96;

            _copyButton = Nut("Sao chép");
            _copyButton.Width = 99;

            Button thongTinButton =
                Nut("Thông tin", true);

            thongTinButton.Width = 96;

            thongTinButton.Click += delegate
            {
                _dangJson = false;

                thongTinButton.BackColor =
                    _xanh;

                _jsonButton.BackColor =
                    Color.FromArgb(30, 43, 64);

                HienChiTiet();
            };

            _jsonButton.Click += delegate
            {
                _dangJson = true;

                _jsonButton.BackColor = _xanh;

                thongTinButton.BackColor =
                    Color.FromArgb(30, 43, 64);

                HienChiTiet();
            };

            _copyButton.Click += delegate
            {
                if (!string.IsNullOrEmpty(
                    _chiTiet.Text))
                {
                    Clipboard.SetText(
                        _chiTiet.Text);
                }
            };

            detailTabs.Controls.Add(thongTinButton);
            detailTabs.Controls.Add(_jsonButton);
            detailTabs.Controls.Add(_copyButton);

            detailLayout.Controls.Add(
                detailTabs, 0, 1);

            _chiTiet = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                BackColor = _the,
                ForeColor = _chu,
                BorderStyle = BorderStyle.None,
                Font = new Font(
                    "Segoe UI", 10F),
                Text =
                    "Chọn một hồ sơ để xem thông tin."
            };

            detailLayout.Controls.Add(
                _chiTiet, 0, 2);

            layout.Controls.Add(body, 0, 3);

            // --------------------------------------------
            // CHAN TRANG
            // --------------------------------------------
            TableLayoutPanel footer =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = _the,
                    Margin = Padding.Empty,
                    Padding = new Padding(8, 5, 8, 5)
                };

            footer.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 60F));

            footer.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 40F));

            _trangThai = Nhan("Sẵn sàng");

            _trangThai.Dock = DockStyle.Fill;

            _trangThai.ForeColor =
                Color.FromArgb(110, 231, 183);

            footer.Controls.Add(
                _trangThai, 0, 0);

            FlowLayoutPanel footerBtns =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.RightToLeft,
                    WrapContents = false,
                    AutoScroll = false,
                    Margin = Padding.Empty
                };

            Button dong = Nut("Đóng");
            dong.Width = 85;

            dong.Click += delegate
            {
                Close();
            };

            _mapButton = Nut(
                "Xem 50 điểm trên bản đồ", true);

            _mapButton.Width = 220;

            _mapButton.Click += delegate
            {
                YeuCauMoBanDo = true;
                Close();
            };

            footerBtns.Controls.Add(dong);
            footerBtns.Controls.Add(_mapButton);

            footer.Controls.Add(
                footerBtns, 1, 0);

            layout.Controls.Add(
                footer, 0, 4);

            DoiCheDo(true);
        }

        // ========================================================
        // DOC DU LIEU POSTGIS
        // ========================================================
        private static string Lay(
            DataRow r,
            string col)
        {
            if (r == null ||
                !r.Table.Columns.Contains(col) ||
                r.IsNull(col))
            {
                return "";
            }

            return Convert.ToString(r[col]);
        }

        private DataTable DocCSDL(bool hoSo)
        {
            if (hoSo && _hoSo != null)
                return _hoSo;

            if (!hoSo && _diaBanMau != null)
                return _diaBanMau;

            const string sqlHoSo = @"
SELECT
    ma_don_vi AS ""Mã đơn vị"",
    ten_don_vi AS ""Tên đơn vị"",
    loai_don_vi AS ""Loại đơn vị"",
    quan_binh_chung AS ""Quân binh chủng"",
    quan_huyen AS ""Quận/huyện"",
    phuong AS ""Phường/xã"",
    round(ST_X(geom)::numeric, 2) AS ""Easting (m)"",
    round(ST_Y(geom)::numeric, 2) AS ""Northing (m)"",
    jsonb_pretty(du_lieu_web) AS ""__json""
FROM web_mau.don_vi
ORDER BY ten_don_vi;";

            const string sqlDiaBan = @"
SELECT
    ma_dia_ban AS ""Mã địa bàn"",
    ten_dia_ban AS ""Tên địa bàn"",
    loai_dia_ban AS ""Loại địa bàn"",
    dien_tich_km2 AS ""Diện tích mẫu (km²)"",
    jsonb_pretty(du_lieu_web) AS ""__json""
FROM web_mau.quan_huyen
ORDER BY ten_dia_ban;";

            DataTable bang = new DataTable();

            using (NpgsqlConnection conn =
                new NpgsqlConnection(
                    _connectionString))

            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    hoSo ? sqlHoSo : sqlDiaBan,
                    conn))

            using (NpgsqlDataAdapter adapter =
                new NpgsqlDataAdapter(command))
            {
                command.CommandTimeout = 30;

                adapter.Fill(bang);
            }

            if (hoSo)
                _hoSo = bang;
            else
                _diaBanMau = bang;

            return bang;
        }

        // ========================================================
        // CHUYEN NHOM DU LIEU
        // ========================================================
        private void DoiNhom(bool hoSo)
        {
            _xemHoSo = hoSo;

            _hoSoButton.BackColor =
                hoSo
                    ? _xanh
                    : Color.FromArgb(30, 43, 64);

            _diaBanButton.BackColor =
                hoSo
                    ? Color.FromArgb(30, 43, 64)
                    : _xanh;

            _mapButton.Enabled = hoSo;

            try
            {
                UseWaitCursor = true;

                DataTable source =
                    DocCSDL(hoSo);

                _dangLoc = true;

                try
                {
                    _tim.Clear();

                    _loai.Items.Clear();
                    _diaBan.Items.Clear();

                    _loai.Items.Add(
                        "Tất cả loại");

                    _diaBan.Items.Add(
                        "Tất cả địa bàn");

                    string cotLoai = hoSo
                        ? "Loại đơn vị"
                        : "Loại địa bàn";

                    foreach (string x in
                        DanhMuc(source, cotLoai))
                    {
                        _loai.Items.Add(x);
                    }

                    if (hoSo)
                    {
                        foreach (string x in
                            DanhMuc(
                                source,
                                "Quận/huyện"))
                        {
                            _diaBan.Items.Add(x);
                        }
                    }

                    _loai.SelectedIndex = 0;
                    _diaBan.SelectedIndex = 0;

                    _diaBan.Enabled = hoSo;
                }
                finally
                {
                    _dangLoc = false;
                }

                LocTrongBoNho();
            }
            catch (Exception ex)
            {
                _trangThai.Text =
                    "Lỗi kết nối dữ liệu web_mau.";

                MessageBox.Show(
                    this,
                    ex.Message,
                    "PostGIS web_mau",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        // ========================================================
        // DANH MUC LOC
        // ========================================================
        private static List<string> DanhMuc(
            DataTable table,
            string column)
        {
            HashSet<string> names =
                new HashSet<string>(
                    StringComparer.CurrentCultureIgnoreCase);

            foreach (DataRow r in table.Rows)
            {
                string value =
                    Lay(r, column).Trim();

                if (value.Length > 0)
                    names.Add(value);
            }

            List<string> result =
                new List<string>(names);

            result.Sort(
                StringComparer.CurrentCultureIgnoreCase);

            return result;
        }

        // ========================================================
        // TIM KIEM VA LOC TRONG BO NHO
        // ========================================================
        private void LocTrongBoNho()
        {
            if (_dangLoc || _bang == null)
                return;

            DataTable source =
                _xemHoSo ? _hoSo : _diaBanMau;

            if (source == null)
                return;

            string search =
                _tim.Text.Trim();

            string loai =
                _loai.SelectedIndex > 0
                    ? Convert.ToString(
                        _loai.SelectedItem)
                    : "";

            string diaBan =
                _diaBan.SelectedIndex > 0
                    ? Convert.ToString(
                        _diaBan.SelectedItem)
                    : "";

            string colType =
                _xemHoSo
                    ? "Loại đơn vị"
                    : "Loại địa bàn";

            string colCode =
                _xemHoSo
                    ? "Mã đơn vị"
                    : "Mã địa bàn";

            string colName =
                _xemHoSo
                    ? "Tên đơn vị"
                    : "Tên địa bàn";

            DataTable result =
                source.Clone();

            foreach (DataRow row in source.Rows)
            {
                if (loai.Length > 0 &&
                    !string.Equals(
                        Lay(row, colType),
                        loai,
                        StringComparison
                            .CurrentCultureIgnoreCase))
                {
                    continue;
                }

                if (diaBan.Length > 0 &&
                    !string.Equals(
                        Lay(row, "Quận/huyện"),
                        diaBan,
                        StringComparison
                            .CurrentCultureIgnoreCase))
                {
                    continue;
                }

                if (search.Length > 0 &&
                    Lay(row, colCode).IndexOf(
                        search,
                        StringComparison
                            .CurrentCultureIgnoreCase) < 0 &&
                    Lay(row, colName).IndexOf(
                        search,
                        StringComparison
                            .CurrentCultureIgnoreCase) < 0)
                {
                    continue;
                }

                result.ImportRow(row);
            }

            _dangLoc = true;

            try
            {
                _bang.DataSource = null;

                if (_loc != null)
                    _loc.Dispose();

                _loc = result;

                _bang.DataSource = result;

                if (_bang.Columns.Contains("__json"))
                {
                    _bang.Columns["__json"].Visible =
                        false;
                }

                foreach (DataGridViewColumn col
                    in _bang.Columns)
                {
                    if (col.Name != "__json")
                    {
                        col.AutoSizeMode =
                            DataGridViewAutoSizeColumnMode
                                .DisplayedCells;
                    }
                }
            }
            finally
            {
                _dangLoc = false;
            }

            _soLuong.Text =
                result.Rows.Count +
                " / " + source.Rows.Count +
                (_xemHoSo ? " hồ sơ" : " địa bàn");

            _trangThai.Text =
                "HQTCSDL.web_mau | " +
                result.Rows.Count +
                " kết quả | Chỉ đọc, chưa xác minh";

            _dongChon =
                result.Rows.Count > 0
                    ? result.Rows[0]
                    : null;

            HienChiTiet();

            _rongTheCu = -1;

            if (_xemThe)
                VeThe();
        }

        // ========================================================
        // CHUYEN CHE DO THE / BANG
        // ========================================================
        private void DoiCheDo(bool the)
        {
            _xemThe = the;

            if (_theButton == null ||
                _bangButton == null)
            {
                return;
            }

            _theButton.BackColor =
                the
                    ? _xanh
                    : Color.FromArgb(30, 43, 64);

            _bangButton.BackColor =
                the
                    ? Color.FromArgb(30, 43, 64)
                    : _xanh;

            _thePanel.Visible = the;
            _bang.Visible = !the;

            if (the)
            {
                _thePanel.BringToFront();

                VeThe();
            }
            else
            {
                _bang.BringToFront();
            }
        }

        // ========================================================
        // VE DANH SACH THE
        // ========================================================
        private void VeThe()
        {
            if (_dangVe ||
                !_xemThe ||
                _thePanel == null ||
                _loc == null)
            {
                return;
            }

            _dangVe = true;

            try
            {
                _thePanel.SuspendLayout();

                while (_thePanel.Controls.Count > 0)
                {
                    Control old =
                        _thePanel.Controls[0];

                    _thePanel.Controls.RemoveAt(0);

                    old.Dispose();
                }

                int width = Math.Max(
                    350,
                    _thePanel.ClientSize.Width - 22);

                int columns =
                    width >= 630 ? 2 : 1;

                int cardWidth = Math.Max(
                    240,
                    (width - 14 * columns) /
                    columns);

                _rongTheCu =
                    _thePanel.ClientSize.Width;

                string maCot =
                    _xemHoSo
                        ? "Mã đơn vị"
                        : "Mã địa bàn";

                string tenCot =
                    _xemHoSo
                        ? "Tên đơn vị"
                        : "Tên địa bàn";

                string loaiCot =
                    _xemHoSo
                        ? "Loại đơn vị"
                        : "Loại địa bàn";

                foreach (DataRow row in _loc.Rows)
                {
                    DataRow current = row;

                    Panel card = new Panel
                    {
                        Width = cardWidth,
                        Height = 181,
                        Margin = new Padding(5),
                        Padding = new Padding(9),
                        BackColor = _the,
                        BorderStyle =
                            BorderStyle.FixedSingle
                    };

                    Label code =
                        Nhan(
                            Lay(current, maCot),
                            true);

                    code.ForeColor =
                        Color.FromArgb(110, 231, 183);

                    code.SetBounds(
                        11, 9, cardWidth - 25, 23);

                    Label name =
                        Nhan(
                            Lay(current, tenCot),
                            true);

                    name.Font = new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold);

                    name.SetBounds(
                        11, 35, cardWidth - 25, 44);

                    Label type = Nhan(
                        "Loại: " +
                        Lay(current, loaiCot));

                    type.ForeColor = _phu;

                    type.SetBounds(
                        11, 86, cardWidth - 25, 21);

                    Label loc = Nhan(
                        _xemHoSo
                            ? "Địa bàn: " +
                                Lay(
                                    current,
                                    "Quận/huyện")
                            : "Diện tích mẫu: " +
                                Lay(
                                    current,
                                    "Diện tích mẫu (km²)") +
                                " km²");

                    loc.ForeColor = _phu;

                    loc.SetBounds(
                        11, 109,
                        cardWidth - 25, 21);

                    Button xem =
                        Nut("Xem chi tiết", true);

                    xem.SetBounds(
                        11, 139,
                        cardWidth - 24, 31);

                    xem.Click += delegate
                    {
                        _dongChon = current;

                        HienChiTiet();
                    };

                    card.Controls.Add(code);
                    card.Controls.Add(name);
                    card.Controls.Add(type);
                    card.Controls.Add(loc);
                    card.Controls.Add(xem);

                    _thePanel.Controls.Add(card);
                }
            }
            finally
            {
                _thePanel.ResumeLayout(true);

                _dangVe = false;
            }
        }

        // ========================================================
        // CHON HANG TRONG BANG
        // ========================================================
        private void ChonDongBang()
        {
            if (_dangLoc ||
                _bang.CurrentRow == null)
            {
                return;
            }

            DataRowView row =
                _bang.CurrentRow.DataBoundItem
                as DataRowView;

            if (row == null)
                return;

            _dongChon = row.Row;

            HienChiTiet();
        }

        // ========================================================
        // THONG TIN / JSON GOC
        // ========================================================
        private void HienChiTiet()
        {
            if (_chiTiet == null ||
                _tieuDeChiTiet == null)
            {
                return;
            }

            if (_dongChon == null)
            {
                _tieuDeChiTiet.Text =
                    "KHÔNG CÓ KẾT QUẢ";

                _chiTiet.Text =
                    "Hãy thay đổi từ khóa hoặc bộ lọc.";

                return;
            }

            string code = Lay(
                _dongChon,
                _xemHoSo
                    ? "Mã đơn vị"
                    : "Mã địa bàn");

            _tieuDeChiTiet.Text =
                "CHI TIẾT: " + code;

            if (_dangJson)
            {
                _chiTiet.Font = new Font(
                    "Consolas", 9F);

                _chiTiet.Text = Lay(
                    _dongChon,
                    "__json");
            }
            else
            {
                _chiTiet.Font = new Font(
                    "Segoe UI", 10F);

                StringBuilder sb =
                    new StringBuilder();

                sb.AppendLine(
                    "THÔNG TIN CƠ BẢN");

                sb.AppendLine(
                    new string('─', 27));

                foreach (DataColumn col
                    in _dongChon.Table.Columns)
                {
                    if (col.ColumnName == "__json")
                        continue;

                    sb.AppendLine(
                        col.ColumnName + ":");

                    sb.AppendLine(
                        "  " + Lay(
                            _dongChon,
                            col.ColumnName));

                    sb.AppendLine();
                }

                sb.AppendLine(
                    "Dữ liệu lấy từ bản trình diễn " +
                    "Google AI Studio.");

                sb.AppendLine(
                    "Vị trí, địa bàn và thuộc tính " +
                    "chưa được xác minh.");

                _chiTiet.Text =
                    sb.ToString();
            }

            _chiTiet.SelectionStart = 0;
            _chiTiet.SelectionLength = 0;
            _chiTiet.ScrollToCaret();
        }

        // ========================================================
        // DON DEP BO NHO
        // ========================================================
        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                if (_bang != null)
                    _bang.DataSource = null;

                if (_loc != null)
                {
                    _loc.Dispose();
                    _loc = null;
                }

                if (_hoSo != null)
                {
                    _hoSo.Dispose();
                    _hoSo = null;
                }

                if (_diaBanMau != null)
                {
                    _diaBanMau.Dispose();
                    _diaBanMau = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
