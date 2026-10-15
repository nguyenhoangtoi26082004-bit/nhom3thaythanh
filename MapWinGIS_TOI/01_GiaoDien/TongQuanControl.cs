
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ============================================================
    // TONG QUAN HE THONG GIS - PHIEN BAN 7.5
    //
    // Muc tieu:
    // - Khong cuon toan bo trang Tong quan.
    // - Thu nho banner va cac the thong ke.
    // - Bieu do tu gian theo khong gian con lai.
    // - Hien thi cac nhom noi bat.
    // - Cho phep xem toan bo du lieu.
    //
    // Khong thay doi CSDL hay MapWinGIS.
    // ============================================================

    public sealed class TongQuanControl : UserControl
    {
        // ========================================================
        // 1. MAU GIAO DIEN
        // ========================================================

        private static readonly Color MauNen =
            Color.FromArgb(8, 15, 29);

        private static readonly Color MauThe =
            Color.FromArgb(15, 24, 43);

        private static readonly Color MauChu =
            Color.FromArgb(241, 245, 249);

        private static readonly Color MauPhu =
            Color.FromArgb(148, 163, 184);

        private static readonly Color MauXanh =
            Color.FromArgb(16, 185, 129);

        private static readonly Color MauXanhDuong =
            Color.FromArgb(56, 189, 248);

        private static readonly Color MauTim =
            Color.FromArgb(167, 139, 250);

        private static readonly Color MauVang =
            Color.FromArgb(251, 191, 36);

        // ========================================================
        // 2. CAC THANH PHAN
        // ========================================================

        private readonly TableLayoutPanel _boCuc =
            new TableLayoutPanel();

        private readonly Panel _vungNhom =
            new Panel();

        private readonly Panel _vungDiaBan =
            new Panel();

        private readonly List<Tuple<string, int>>
            _duLieuNhom =
                new List<Tuple<string, int>>();

        private readonly List<Tuple<string, int>>
            _duLieuDiaBan =
                new List<Tuple<string, int>>();

        private Label _soHoSo;
        private Label _soDiaBan;
        private Label _soDiem;
        private Label _soNhom;

        private Label _trangThai;
        private Label _ghiChuNhom;
        private Label _ghiChuDiaBan;

        private Label _tieuDeChinh;

        private int _tongHoSo;

        private int _soDongNhomHienThi = -1;
        private int _soDongDiaBanHienThi = -1;

        private bool _dangCapNhatKichThuoc;

        // Giu cac su kien dang duoc
        // TongQuanIntegration.cs su dung.
        public event EventHandler YeuCauMoBanDo;
        public event EventHandler YeuCauMoDuLieuWeb;

        // ========================================================
        // 3. KHOI TAO
        // ========================================================

        public TongQuanControl()
        {
            Dock = DockStyle.Fill;

            BackColor = MauNen;
            Margin = Padding.Empty;
            Padding = Padding.Empty;

            Font = new Font(
                "Segoe UI",
                9F);

            MinimumSize = Size.Empty;

            TaoGiaoDien();

            SizeChanged += delegate
            {
                CapNhatBoCuc();
            };
        }

        // ========================================================
        // 4. TAO NHAN
        // ========================================================

        private static Label TaoNhan(
            string text,
            float size,
            Color color,
            FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                Text = text,
                ForeColor = color,
                BackColor = Color.Transparent,

                Font = new Font(
                    "Segoe UI",
                    size,
                    style),

                TextAlign =
                    ContentAlignment.MiddleLeft,

                AutoSize = false,
                AutoEllipsis = true,
                Margin = Padding.Empty
            };
        }

        // ========================================================
        // 5. TAO PANEL
        // ========================================================

        private static Panel TaoThe(
            Color background)
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = background,
                Margin = new Padding(5),
                Padding = new Padding(12, 8, 12, 7),
                AutoSize = false,
                MinimumSize = Size.Empty
            };
        }

        // ========================================================
        // 6. TAO NUT
        // ========================================================

        private static Button TaoNut(
            string text,
            int width,
            Color background)
        {
            Button button = new Button();

            button.Text = text;

            button.Width = width;
            button.Height = 30;

            button.BackColor = background;
            button.ForeColor = Color.White;

            button.Font = new Font(
                "Segoe UI",
                8.5F,
                FontStyle.Bold);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 0;

            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(5, 130, 100);

            button.UseVisualStyleBackColor = false;

            button.AutoSize = false;

            button.TextAlign =
                ContentAlignment.MiddleCenter;

            button.Cursor = Cursors.Hand;

            button.Margin =
                new Padding(0, 0, 8, 0);

            return button;
        }

        // ========================================================
        // 7. BO CUC CHINH - KHONG THANH CUON
        // ========================================================

        private void TaoGiaoDien()
        {
            _boCuc.Dock = DockStyle.Fill;

            _boCuc.AutoSize = false;
            _boCuc.AutoScroll = false;

            _boCuc.BackColor = MauNen;

            _boCuc.Margin = Padding.Empty;

            _boCuc.Padding =
                new Padding(10, 5, 10, 7);

            _boCuc.ColumnCount = 1;
            _boCuc.RowCount = 4;

            _boCuc.GrowStyle =
                TableLayoutPanelGrowStyle.FixedSize;

            _boCuc.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            // Banner.
            _boCuc.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    132F));

            // Bon the thong ke.
            _boCuc.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    110F));

            // Hai bieu do tu gian theo chieu cao.
            _boCuc.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            // Chan trang.
            _boCuc.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    29F));

            _boCuc.Controls.Add(
                TaoBanner(),
                0, 0);

            _boCuc.Controls.Add(
                TaoDongThongKe(),
                0, 1);

            _boCuc.Controls.Add(
                TaoHaiBangPhanBo(),
                0, 2);

            _boCuc.Controls.Add(
                TaoChanTrang(),
                0, 3);

            Controls.Add(_boCuc);

            CapNhatBoCuc();
        }

        // ========================================================
        // 8. TU THICH UNG CHIEU CAO MAN HINH
        // ========================================================

        private void CapNhatBoCuc()
        {
            if (_dangCapNhatKichThuoc ||
                _boCuc.IsDisposed)
            {
                return;
            }

            if (ClientSize.Height <= 0)
                return;

            _dangCapNhatKichThuoc = true;

            try
            {
                int height = ClientSize.Height;

                if (height >= 580)
                {
                    _boCuc.RowStyles[0].Height = 132F;
                    _boCuc.RowStyles[1].Height = 110F;
                    _boCuc.RowStyles[3].Height = 29F;
                }
                else if (height >= 465)
                {
                    _boCuc.RowStyles[0].Height = 122F;
                    _boCuc.RowStyles[1].Height = 103F;
                    _boCuc.RowStyles[3].Height = 25F;
                }
                else
                {
                    _boCuc.RowStyles[0].Height = 114F;
                    _boCuc.RowStyles[1].Height = 96F;
                    _boCuc.RowStyles[3].Height = 23F;
                }

                // Hang bieu do chiem phan chieu cao
                // con lai. Khong tao thanh cuon trang.
                _boCuc.PerformLayout();

                CapNhatBieuDo();
            }
            finally
            {
                _dangCapNhatKichThuoc = false;
            }
        }

        // ========================================================
        // 9. BANNER THU GON
        // ========================================================

        private Panel TaoBanner()
        {
            Panel banner = TaoThe(
                Color.FromArgb(7, 40, 38));

            banner.Name = "panelBannerTongQuan";

            banner.Padding =
                new Padding(16, 7, 16, 7);

            TableLayoutPanel content =
                new TableLayoutPanel();

            content.Dock = DockStyle.Fill;

            content.RowCount = 4;
            content.ColumnCount = 1;

            content.AutoSize = false;

            content.Margin = Padding.Empty;
            content.Padding = Padding.Empty;

            content.BackColor = banner.BackColor;

            content.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            content.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    19F));

            content.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    31F));

            content.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    22F));

            content.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            // ----------------------------------------
            // NHAN GIOI THIEU
            // ----------------------------------------

            Label tag = TaoNhan(
                "●  HỆ THỐNG CƠ SỞ DỮ LIỆU GIS",
                8F,
                Color.FromArgb(110, 231, 183),
                FontStyle.Bold);

            tag.Dock = DockStyle.Fill;

            content.Controls.Add(tag, 0, 0);

            // ----------------------------------------
            // TIEU DE NHO GON
            // ----------------------------------------

            _tieuDeChinh = TaoNhan(
                "TỔNG QUAN DỮ LIỆU GIS HÀ NỘI",
                17F,
                Color.White,
                FontStyle.Bold);

            _tieuDeChinh.Name =
                "lblTieuDeTongQuan";

            _tieuDeChinh.Dock = DockStyle.Fill;

            content.Controls.Add(
                _tieuDeChinh,
                0, 1);

            // ----------------------------------------
            // MO TA
            // ----------------------------------------

            Label moTa = TaoNhan(
                "Thống kê hồ sơ đơn vị, địa bàn " +
                "và dữ liệu không gian tại Hà Nội.",
                8.5F,
                Color.FromArgb(192, 210, 222));

            moTa.Dock = DockStyle.Fill;

            content.Controls.Add(moTa, 0, 2);

            // ----------------------------------------
            // HAI NUT
            // ----------------------------------------

            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock = DockStyle.Fill;

            actions.FlowDirection =
                FlowDirection.LeftToRight;

            actions.WrapContents = false;

            actions.AutoSize = false;
            actions.AutoScroll = false;

            actions.BackColor = banner.BackColor;

            actions.Margin = Padding.Empty;

            actions.Padding =
                new Padding(0, 2, 0, 0);

            Button btnMap = TaoNut(
                "Mở bản đồ GIS",
                144,
                MauXanh);

            Button btnData = TaoNut(
                "Xem hồ sơ đơn vị",
                162,
                Color.FromArgb(39, 61, 82));

            btnMap.Click += delegate
            {
                if (YeuCauMoBanDo != null)
                {
                    YeuCauMoBanDo(
                        this,
                        EventArgs.Empty);
                }
            };

            btnData.Click += delegate
            {
                if (YeuCauMoDuLieuWeb != null)
                {
                    YeuCauMoDuLieuWeb(
                        this,
                        EventArgs.Empty);
                }
            };

            actions.Controls.Add(btnMap);
            actions.Controls.Add(btnData);

            content.Controls.Add(actions, 0, 3);

            banner.Controls.Add(content);

            return banner;
        }

        // ========================================================
        // 10. BON THE THONG KE THU GON
        // ========================================================

        private TableLayoutPanel TaoDongThongKe()
        {
            TableLayoutPanel row =
                new TableLayoutPanel();

            row.Dock = DockStyle.Fill;

            row.Margin = Padding.Empty;
            row.Padding = Padding.Empty;

            row.RowCount = 1;
            row.ColumnCount = 4;

            row.BackColor = MauNen;

            for (int i = 0; i < 4; i++)
            {
                row.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        25F));
            }

            row.Controls.Add(
                TaoOThongKe(
                    "HỒ SƠ ĐƠN VỊ",
                    "Tổng số hồ sơ quản lý",
                    MauXanh,
                    out _soHoSo),
                0, 0);

            row.Controls.Add(
                TaoOThongKe(
                    "ĐỊA BÀN HÀNH CHÍNH",
                    "Số địa bàn trong bộ dữ liệu",
                    MauXanhDuong,
                    out _soDiaBan),
                1, 0);

            row.Controls.Add(
                TaoOThongKe(
                    "HÌNH HỌC GIS HỢP LỆ",
                    "Đã kiểm tra, EPSG:32648",
                    MauTim,
                    out _soDiem),
                2, 0);

            row.Controls.Add(
                TaoOThongKe(
                    "NHÓM PHÂN LOẠI",
                    "Theo thuộc tính hồ sơ",
                    MauVang,
                    out _soNhom),
                3, 0);

            return row;
        }

        // ========================================================
        // 11. TAO THE SO LIEU
        // ========================================================

        private static Panel TaoOThongKe(
            string title,
            string note,
            Color color,
            out Label value)
        {
            Panel card = TaoThe(MauThe);

            card.Padding =
                new Padding(12, 7, 12, 6);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;

            layout.ColumnCount = 1;
            layout.RowCount = 3;

            layout.Margin = Padding.Empty;
            layout.Padding = Padding.Empty;

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    20F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    39F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            Label heading = TaoNhan(
                title,
                8F,
                MauPhu,
                FontStyle.Bold);

            heading.Dock = DockStyle.Fill;

            value = TaoNhan(
                "—",
                23F,
                color,
                FontStyle.Bold);

            value.Dock = DockStyle.Fill;

            Label description = TaoNhan(
                note,
                8F,
                MauPhu);

            description.Dock = DockStyle.Fill;

            layout.Controls.Add(
                heading, 0, 0);

            layout.Controls.Add(
                value, 0, 1);

            layout.Controls.Add(
                description, 0, 2);

            card.Controls.Add(layout);

            return card;
        }

        // ========================================================
        // 12. HAI BIEU DO PHAN BO
        // ========================================================

        private TableLayoutPanel TaoHaiBangPhanBo()
        {
            TableLayoutPanel row =
                new TableLayoutPanel();

            row.Dock = DockStyle.Fill;

            row.Margin = Padding.Empty;
            row.Padding = Padding.Empty;

            row.RowCount = 1;
            row.ColumnCount = 2;

            row.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            row.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            row.Controls.Add(
                TaoKhungBieuDo(
                    "PHÂN BỐ THEO NHÓM PHÂN LOẠI",
                    _vungNhom,
                    true),
                0, 0);

            row.Controls.Add(
                TaoKhungBieuDo(
                    "PHÂN BỐ THEO ĐỊA BÀN",
                    _vungDiaBan,
                    false),
                1, 0);

            return row;
        }

        // ========================================================
        // 13. KHUNG BIEU DO
        // ========================================================

        private Panel TaoKhungBieuDo(
            string title,
            Panel body,
            bool laNhom)
        {
            Panel card = TaoThe(MauThe);

            card.Padding =
                new Padding(12, 8, 12, 7);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;

            layout.RowCount = 3;
            layout.ColumnCount = 1;

            layout.Margin = Padding.Empty;

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    34F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    20F));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            // ----------------------------------------
            // TIEU DE VA NUT XEM TAT CA
            // ----------------------------------------

            TableLayoutPanel header =
                new TableLayoutPanel();

            header.Dock = DockStyle.Fill;

            header.RowCount = 1;
            header.ColumnCount = 2;

            header.Margin = Padding.Empty;

            header.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            header.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    91F));

            Label heading = TaoNhan(
                title,
                9.5F,
                MauChu,
                FontStyle.Bold);

            heading.Dock = DockStyle.Fill;

            Button all = new Button
            {
                Text = "Xem tất cả",
                Dock = DockStyle.Fill,
                Height = 27,

                BackColor =
                    Color.FromArgb(29, 47, 67),

                ForeColor =
                    Color.FromArgb(110, 231, 183),

                FlatStyle = FlatStyle.Flat,

                Font = new Font(
                    "Segoe UI", 8F),

                Cursor = Cursors.Hand,

                Margin =
                    new Padding(4, 0, 0, 5)
            };

            all.FlatAppearance.BorderSize = 0;

            all.Click += delegate
            {
                MoDanhSachChiTiet(laNhom);
            };

            header.Controls.Add(
                heading, 0, 0);

            header.Controls.Add(
                all, 1, 0);

            // ----------------------------------------
            // NOI DUNG BIEU DO
            // ----------------------------------------

            body.Dock = DockStyle.Fill;

            body.AutoScroll = false;
            body.BackColor = MauThe;

            body.Margin = Padding.Empty;
            body.Padding = Padding.Empty;

            body.SizeChanged += delegate
            {
                CapNhatBieuDo();
            };

            // ----------------------------------------
            // GHI CHU
            // ----------------------------------------

            Label note = TaoNhan(
                "Các nhóm có số hồ sơ nhiều nhất",
                8F,
                MauPhu);

            note.Dock = DockStyle.Fill;

            if (laNhom)
                _ghiChuNhom = note;
            else
                _ghiChuDiaBan = note;

            layout.Controls.Add(
                header, 0, 0);

            layout.Controls.Add(
                body, 0, 1);

            layout.Controls.Add(
                note, 0, 2);

            card.Controls.Add(layout);

            return card;
        }

        // ========================================================
        // 14. TAO DONG BIEU DO
        // ========================================================

        private static Panel TaoDongBieuDo(
            string name,
            int quantity,
            int total,
            Color color)
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 47,
                BackColor = MauThe,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            double ratio = total > 0
                ? (double)quantity / total
                : 0;

            ratio = Math.Max(
                0,
                Math.Min(1, ratio));

            Label title = TaoNhan(
                name,
                8.5F,
                MauChu,
                FontStyle.Bold);

            title.Top = 1;
            title.Left = 0;
            title.Height = 23;

            Label count = TaoNhan(
                quantity.ToString("N0") +
                " (" +
                (ratio * 100).ToString("0") +
                "%)",
                8F,
                MauPhu);

            count.Top = 1;
            count.Height = 23;

            count.TextAlign =
                ContentAlignment.MiddleRight;

            Panel track = new Panel
            {
                Left = 0,
                Top = 29,
                Height = 8,

                BackColor =
                    Color.FromArgb(35, 51, 73)
            };

            Panel progress = new Panel
            {
                Left = 0,
                Top = 0,
                Height = 8,
                BackColor = color
            };

            track.Controls.Add(progress);

            row.Controls.Add(title);
            row.Controls.Add(count);
            row.Controls.Add(track);

            EventHandler resize = delegate
            {
                int width = Math.Max(
                    20,
                    row.ClientSize.Width - 8);

                title.Width = Math.Max(
                    20,
                    width - 104);

                count.Width = 102;
                count.Left = Math.Max(
                    0,
                    width - 102);

                track.Width = width;

                progress.Width =
                    (int)Math.Round(
                        width * ratio);
            };

            row.Resize += resize;

            resize(row, EventArgs.Empty);

            return row;
        }

        // ========================================================
        // 15. HIEN THI CAC NHOM NOI BAT
        // ========================================================

        private void CapNhatBieuDo()
        {
            if (_dangCapNhatKichThuoc)
                return;

            CapNhatMotBieuDo(
                _vungNhom,
                _duLieuNhom,
                _tongHoSo,
                MauXanh,
                ref _soDongNhomHienThi,
                _ghiChuNhom);

            CapNhatMotBieuDo(
                _vungDiaBan,
                _duLieuDiaBan,
                _tongHoSo,
                MauXanhDuong,
                ref _soDongDiaBanHienThi,
                _ghiChuDiaBan);
        }

        private static void CapNhatMotBieuDo(
            Panel body,
            List<Tuple<string, int>> data,
            int total,
            Color color,
            ref int soDongDaHienThi,
            Label note)
        {
            if (body.IsDisposed)
                return;

            int height = body.ClientSize.Height;

            if (height <= 0)
                return;

            // Tu tinh so dong theo chieu cao con lai.
            // Khong tao thanh cuon cho trang.
            int visible = Math.Max(
                1,
                Math.Min(
                    6,
                    (height - 2) / 47));

            visible = Math.Min(
                visible,
                data.Count);

            // Tranh tao lai control khi khong can.
            if (visible == soDongDaHienThi &&
                body.Controls.Count == visible)
            {
                return;
            }

            soDongDaHienThi = visible;

            body.SuspendLayout();

            try
            {
                while (body.Controls.Count > 0)
                {
                    Control old = body.Controls[0];

                    body.Controls.RemoveAt(0);
                    old.Dispose();
                }

                // DockStyle.Top:
                // Them theo thu tu nguoc de dong dau
                // xuat hien tren cung.
                for (int i = visible - 1; i >= 0; i--)
                {
                    Tuple<string, int> item =
                        data[i];

                    body.Controls.Add(
                        TaoDongBieuDo(
                            item.Item1,
                            item.Item2,
                            total,
                            color));
                }

                if (note != null)
                {
                    note.Text =
                        "Đang hiển thị " +
                        visible +
                        "/" +
                        data.Count +
                        " nhóm. Nhấn Xem tất cả để xem đầy đủ.";
                }
            }
            finally
            {
                body.ResumeLayout(true);
            }
        }

        // ========================================================
        // 16. MO BANG DU LIEU DAY DU
        // ========================================================

        private void MoDanhSachChiTiet(bool laNhom)
        {
            List<Tuple<string, int>> source =
                laNhom
                    ? _duLieuNhom
                    : _duLieuDiaBan;

            string title = laNhom
                ? "Phân bố theo nhóm phân loại"
                : "Phân bố theo địa bàn";

            using (Form form = new Form())
            using (DataTable table = new DataTable())
            {
                form.Text = title;

                form.StartPosition =
                    FormStartPosition.CenterParent;

                form.Size = new Size(820, 590);

                form.MinimumSize =
                    new Size(540, 380);

                form.BackColor = MauNen;
                form.ForeColor = MauChu;

                form.Font =
                    new Font("Segoe UI", 9F);

                table.Columns.Add(
                    "Nhóm thống kê",
                    typeof(string));

                table.Columns.Add(
                    "Số hồ sơ",
                    typeof(int));

                table.Columns.Add(
                    "Tỷ lệ (%)",
                    typeof(double));

                foreach (Tuple<string, int> item
                    in source)
                {
                    double percent = _tongHoSo > 0
                        ? 100.0 * item.Item2 / _tongHoSo
                        : 0.0;

                    table.Rows.Add(
                        item.Item1,
                        item.Item2,
                        Math.Round(percent, 2));
                }

                Label heading = TaoNhan(
                    title.ToUpperInvariant(),
                    15F,
                    MauChu,
                    FontStyle.Bold);

                heading.Dock = DockStyle.Top;
                heading.Height = 56;

                heading.Padding =
                    new Padding(16, 0, 0, 0);

                DataGridView grid =
                    new DataGridView();

                grid.Dock = DockStyle.Fill;

                grid.DataSource = table;

                grid.ReadOnly = true;

                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;

                grid.AllowUserToResizeRows = false;

                grid.RowHeadersVisible = false;

                grid.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;

                grid.MultiSelect = false;

                grid.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;

                grid.BackgroundColor = MauNen;

                grid.BorderStyle = BorderStyle.None;

                grid.GridColor =
                    Color.FromArgb(42, 60, 80);

                grid.EnableHeadersVisualStyles = false;

                grid.ColumnHeadersHeight = 35;

                grid.ColumnHeadersDefaultCellStyle.BackColor =
                    Color.FromArgb(20, 38, 57);

                grid.ColumnHeadersDefaultCellStyle.ForeColor =
                    Color.White;

                grid.DefaultCellStyle.BackColor =
                    MauThe;

                grid.DefaultCellStyle.ForeColor =
                    Color.White;

                grid.DefaultCellStyle.SelectionBackColor =
                    Color.FromArgb(5, 110, 92);

                grid.DefaultCellStyle.SelectionForeColor =
                    Color.White;

                grid.AlternatingRowsDefaultCellStyle.BackColor =
                    Color.FromArgb(12, 28, 46);

                grid.RowTemplate.Height = 31;

                Button close = TaoNut(
                    "Đóng",
                    90,
                    Color.FromArgb(39, 61, 82));

                close.DialogResult = DialogResult.OK;

                Panel bottom = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 55,
                    BackColor = MauNen,
                    Padding = new Padding(12)
                };

                close.Dock = DockStyle.Right;

                bottom.Controls.Add(close);

                form.Controls.Add(grid);
                form.Controls.Add(heading);
                form.Controls.Add(bottom);

                form.AcceptButton = close;
                form.CancelButton = close;

                form.ShowDialog(this);

                grid.DataSource = null;
            }
        }

        // ========================================================
        // 17. CHAN TRANG THU GON
        // ========================================================

        private Panel TaoChanTrang()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = MauNen,
                Margin = new Padding(5, 2, 5, 0),
                Padding = Padding.Empty
            };

            _trangThai = TaoNhan(
                "Dữ liệu thống kê từ PostgreSQL/PostGIS.",
                8F,
                MauPhu);

            _trangThai.Dock = DockStyle.Fill;

            panel.Controls.Add(_trangThai);

            return panel;
        }

        // ========================================================
        // 18. HAM DOC GIA TRI
        // ========================================================

        private static int LaySo(
            DataRow row,
            string column)
        {
            if (row == null ||
                !row.Table.Columns.Contains(column) ||
                row.IsNull(column))
            {
                return 0;
            }

            return Convert.ToInt32(
                row[column]);
        }

        private static string LayChu(
            DataRow row,
            string column)
        {
            if (row == null ||
                !row.Table.Columns.Contains(column) ||
                row.IsNull(column))
            {
                return "";
            }

            return Convert.ToString(
                row[column]);
        }

        // ========================================================
        // 19. CAP NHAT DU LIEU TONG QUAN
        //
        // Giu ten ham de tuong thich voi
        // TongQuanIntegration.cs hien tai.
        // ========================================================

        public void HienThiWeb(DataTable data)
        {
            if (data == null ||
                data.Rows.Count == 0)
            {
                HienThiLoi(
                    "Không có dữ liệu thống kê.");

                return;
            }

            DataRow first = data.Rows[0];

            _tongHoSo = LaySo(
                first,
                "so_ho_so_web");

            _soHoSo.Text =
                _tongHoSo.ToString("N0");

            _soDiaBan.Text =
                LaySo(
                    first,
                    "so_quan_huyen_mau"
                ).ToString("N0");

            _soDiem.Text =
                LaySo(
                    first,
                    "so_diem_hop_le"
                ).ToString("N0");

            _soNhom.Text =
                LaySo(
                    first,
                    "so_nhom"
                ).ToString("N0");

            _duLieuNhom.Clear();
            _duLieuDiaBan.Clear();

            foreach (DataRow row in data.Rows)
            {
                string type = LayChu(
                    row,
                    "nhom_loai");

                string name = LayChu(
                    row,
                    "ten_nhom");

                int quantity = LaySo(
                    row,
                    "so_luong");

                if (type == "NHOM")
                {
                    _duLieuNhom.Add(
                        Tuple.Create(
                            name,
                            quantity));
                }
                else if (type == "DIA_BAN")
                {
                    _duLieuDiaBan.Add(
                        Tuple.Create(
                            name,
                            quantity));
                }
            }

            _duLieuNhom.Sort(
                (a, b) =>
                    b.Item2.CompareTo(a.Item2));

            _duLieuDiaBan.Sort(
                (a, b) =>
                    b.Item2.CompareTo(a.Item2));

            _soDongNhomHienThi = -1;
            _soDongDiaBanHienThi = -1;

            CapNhatBieuDo();

            _trangThai.Text =
                "Tổng hợp " +
                _tongHoSo.ToString("N0") +
                " hồ sơ | EPSG:32648 | " +
                "Dữ liệu minh họa, vị trí chưa xác minh.";
        }

        // ========================================================
        // 20. TUONG THICH VOI DU LIEU TONG QUAN CU
        // ========================================================

        public void HienThi(DataRow data)
        {
            if (data == null)
            {
                HienThiLoi(
                    "Không có dữ liệu tổng quan.");

                return;
            }

            _tongHoSo = LaySo(
                data,
                "tong_don_vi");

            _soHoSo.Text =
                _tongHoSo.ToString("N0");

            _soDiaBan.Text =
                LaySo(
                    data,
                    "tong_xa_phuong"
                ).ToString("N0");

            _soDiem.Text =
                LaySo(
                    data,
                    "don_vi_co_diem_gis"
                ).ToString("N0");

            _soNhom.Text = "—";

            _duLieuNhom.Clear();
            _duLieuDiaBan.Clear();

            _soDongNhomHienThi = -1;
            _soDongDiaBanHienThi = -1;

            CapNhatBieuDo();

            _trangThai.Text =
                "Dữ liệu thống kê quản lý hiện hành.";
        }

        // ========================================================
        // 21. THONG BAO LOI
        // ========================================================

        public void HienThiLoi(string message)
        {
            _tongHoSo = 0;

            _soHoSo.Text = "—";
            _soDiaBan.Text = "—";
            _soDiem.Text = "—";
            _soNhom.Text = "—";

            _duLieuNhom.Clear();
            _duLieuDiaBan.Clear();

            _soDongNhomHienThi = -1;
            _soDongDiaBanHienThi = -1;

            CapNhatBieuDo();

            _trangThai.Text =
                "Không thể cập nhật thống kê: " +
                message;
        }
    }
}
