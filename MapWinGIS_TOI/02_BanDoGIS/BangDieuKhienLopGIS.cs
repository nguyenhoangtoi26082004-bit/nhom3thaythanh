using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ==========================================================
    // BANG DIEU KHIEN LOP GIS - V4.0
    // - Chi nap lop khi nguoi dung bam nut.
    // - Khong nap khi chuyen Tong quan <-> Ban do GIS.
    // - Giu MapWinGIS ActiveX va nen ve tinh hien co.
    // - Khong SELECT toa do khu doanh trai thuc te.
    // - Khong INSERT, UPDATE, DELETE tren CSDL.
    // ==========================================================
    public partial class Form1
    {
        private Button _nutBangLopGIS;
        private bool _daKhoiTaoBangLopGIS;
        private bool _dangNapLopThuCongGIS;

        private sealed class DongLopGIS
        {
            public int Handle { get; set; }
            public string TenLop { get; set; }
            public bool HienThi { get; set; }
        }

        // ======================================================
        // 1. DANG KY MENU MOT LAN, KHONG OVERRIDE OnShown
        // ======================================================
        private void KhoiTaoBangDieuKhienLopGIS()
        {
            if (_daKhoiTaoBangLopGIS)
                return;

            _daKhoiTaoBangLopGIS = true;

            Shown += delegate
            {
                if (!IsDisposed && !Disposing && IsHandleCreated)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!IsDisposed && !Disposing)
                            GanNutBangDieuKhienLopGIS();
                    });
                }
            };

            if (Visible && IsHandleCreated)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (!IsDisposed && !Disposing)
                        GanNutBangDieuKhienLopGIS();
                });
            }
        }

        private static Button TimNutBanDoGIS(Control root)
        {
            if (root == null || root.IsDisposed || !root.Visible)
                return null;

            foreach (Control child in root.Controls)
            {
                Button button = child as Button;
                if (button != null && button.Visible)
                {
                    string text = (button.Text ?? "").Trim();
                    if (text.IndexOf("Bản đồ GIS",
                            StringComparison.OrdinalIgnoreCase) >= 0 &&
                        text.IndexOf("Mở bản đồ",
                            StringComparison.OrdinalIgnoreCase) < 0)
                        return button;
                }

                Button found = TimNutBanDoGIS(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void GanNutBangDieuKhienLopGIS()
        {
            if (IsDisposed || Disposing)
                return;

            if (_nutBangLopGIS != null && !_nutBangLopGIS.IsDisposed)
                return;

            Button btnBanDo = TimNutBanDoGIS(this);
            FlowLayoutPanel flow = btnBanDo == null
                ? null : btnBanDo.Parent as FlowLayoutPanel;

            Button nut = new Button
            {
                Name = "btnCacLopGIS",
                Text = "Các lớp GIS",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(15, 24, 43),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                AutoSize = false
            };
            nut.FlatAppearance.BorderSize = 0;

            nut.Click += delegate
            {
                MoBangDieuKhienLopGIS();
            };

            if (flow != null)
            {
                nut.Size = btnBanDo.Size;
                nut.Margin = btnBanDo.Margin;
                nut.Padding = btnBanDo.Padding;
                flow.Controls.Add(nut);

                int index = flow.Controls.IndexOf(btnBanDo);
                if (index >= 0)
                    flow.Controls.SetChildIndex(nut,
                        Math.Min(index + 1, flow.Controls.Count - 1));
            }
            else if (panelMap != null && !panelMap.IsDisposed)
            {
                // Du phong neu menu chinh dung bo cuc khac.
                nut.Text = "Các lớp GIS";
                nut.TextAlign = ContentAlignment.MiddleCenter;
                nut.Size = new Size(116, 33);
                nut.Location = new Point(
                    Math.Max(8, panelMap.ClientSize.Width - 132), 12);
                nut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                nut.BackColor = Color.FromArgb(5, 150, 105);
                panelMap.Controls.Add(nut);
                nut.BringToFront();
            }
            else
            {
                nut.Dispose();
                return;
            }

            _nutBangLopGIS = nut;
        }

        // ======================================================
        // 2. CUA SO DIEU KHIEN CAC LOP
        // ======================================================
        private void MoBangDieuKhienLopGIS()
        {
            // Chuyen sang ban do MOT LAN khi nguoi dung chu dong
            // mo bang dieu khien, khong gan vao su kien chuyen trang.
            HienTrangBanDo();

            using (Form form = new Form())
            {
                form.Text = "Quản lý các lớp bản đồ GIS";
                form.StartPosition = FormStartPosition.CenterParent;
                form.Size = new Size(920, 630);
                form.MinimumSize = new Size(770, 490);
                form.BackColor = Color.FromArgb(8, 15, 29);
                form.ForeColor = Color.White;
                form.Font = new Font("Segoe UI", 9F);
                form.ShowInTaskbar = false;

                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.Padding = new Padding(16);
                layout.ColumnCount = 1;
                layout.RowCount = 5;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 49F));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 57F));

                Label title = new Label
                {
                    Text = "CÁC LỚP DỮ LIỆU BẢN ĐỒ GIS",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                layout.Controls.Add(title, 0, 0);

                Label description = new Label
                {
                    Text = "Nạp theo yêu cầu · Bật/tắt lớp · Thu phóng tới lớp · EPSG:32648",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.FromArgb(148, 163, 184),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                layout.Controls.Add(description, 0, 1);

                FlowLayoutPanel tools = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    AutoScroll = false,
                    Margin = Padding.Empty,
                    Padding = new Padding(0, 4, 0, 0)
                };

                Button btnPoints = TaoNutLopGIS(
                    "Nạp điểm tham chiếu", 173,
                    Color.FromArgb(5, 150, 105));
                Button btnAreas = TaoNutLopGIS(
                    "Nạp địa bàn", 136,
                    Color.FromArgb(30, 85, 137));
                Button btnBoth = TaoNutLopGIS(
                    "Nạp cả hai", 127,
                    Color.FromArgb(5, 113, 103));
                Button btnRefresh = TaoNutLopGIS(
                    "Làm mới", 105,
                    Color.FromArgb(35, 51, 73));
                Button btnZoom = TaoNutLopGIS(
                    "Thu phóng", 118,
                    Color.FromArgb(35, 51, 73));

                tools.Controls.Add(btnPoints);
                tools.Controls.Add(btnAreas);
                tools.Controls.Add(btnBoth);
                tools.Controls.Add(btnRefresh);
                tools.Controls.Add(btnZoom);
                layout.Controls.Add(tools, 0, 2);

                DataGridView grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    DataSource = null,
                    AutoGenerateColumns = false,
                    RowHeadersVisible = false,
                    MultiSelect = false,
                    ReadOnly = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.FromArgb(12, 23, 41),
                    BorderStyle = BorderStyle.None,
                    GridColor = Color.FromArgb(38, 54, 75),
                    EnableHeadersVisualStyles = false,
                    ColumnHeadersHeight = 37
                };
                grid.RowTemplate.Height = 35;
                grid.ColumnHeadersDefaultCellStyle.BackColor =
                    Color.FromArgb(30, 46, 67);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.DefaultCellStyle.BackColor = Color.FromArgb(17, 29, 48);
                grid.DefaultCellStyle.ForeColor = Color.White;
                grid.DefaultCellStyle.SelectionBackColor =
                    Color.FromArgb(5, 110, 92);
                grid.DefaultCellStyle.SelectionForeColor = Color.White;
                grid.AlternatingRowsDefaultCellStyle.BackColor =
                    Color.FromArgb(14, 26, 43);

                DataGridViewTextBoxColumn colName =
                    new DataGridViewTextBoxColumn();
                colName.HeaderText = "Tên lớp GIS";
                colName.DataPropertyName = "TenLop";
                colName.FillWeight = 80;
                colName.ReadOnly = true;

                DataGridViewCheckBoxColumn colVisible =
                    new DataGridViewCheckBoxColumn();
                colVisible.HeaderText = "Hiển thị";
                colVisible.DataPropertyName = "HienThi";
                colVisible.FillWeight = 20;

                grid.Columns.Add(colName);
                grid.Columns.Add(colVisible);

                BindingList<DongLopGIS> records =
                    new BindingList<DongLopGIS>();
                grid.DataSource = records;

                layout.Controls.Add(grid, 0, 3);

                Panel bottom = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = form.BackColor
                };
                Label status = new Label
                {
                    Dock = DockStyle.Fill,
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Text = "Chưa kiểm tra lớp GIS.",
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };
                Button btnClose = TaoNutLopGIS(
                    "Đóng", 96, Color.FromArgb(35, 51, 73));
                btnClose.Dock = DockStyle.Right;
                btnClose.DialogResult = DialogResult.OK;
                bottom.Controls.Add(status);
                bottom.Controls.Add(btnClose);
                layout.Controls.Add(bottom, 0, 4);
                form.Controls.Add(layout);
                form.CancelButton = btnClose;

                Action refresh = delegate
                {
                    records.Clear();
                    if (axMap2 == null || axMap2.IsDisposed)
                    {
                        status.Text = "Bản đồ chưa khởi tạo.";
                        return;
                    }
                    try
                    {
                        for (int i = axMap2.NumLayers - 1; i >= 0; i--)
                        {
                            int handle = axMap2.get_LayerHandle(i);
                            if (handle < 0) continue;

                            string name = axMap2.get_LayerName(handle) ?? "";
                            records.Add(new DongLopGIS
                            {
                                Handle = handle,
                                TenLop = TenLopHienThiGIS(name),
                                HienThi = axMap2.get_LayerVisible(handle)
                            });
                        }
                        status.Text = "Đã phát hiện " + records.Count +
                            " lớp trong MapWinGIS. Chỉ thay đổi hiển thị, không sửa CSDL.";
                    }
                    catch (Exception ex)
                    {
                        status.Text = "Không đọc được lớp: " + ex.Message;
                    }
                };

                // Chi cho phep nguoi dung bam nut de nap du lieu.
                Action<bool, bool> load = delegate (bool areas, bool points)
                {
                    if (_dangNapLopThuCongGIS) return;

                    _dangNapLopThuCongGIS = true;
                    bool oldWait = form.UseWaitCursor;
                    form.UseWaitCursor = true;
                    btnPoints.Enabled = false;
                    btnAreas.Enabled = false;
                    btnBoth.Enabled = false;

                    try
                    {
                        int before = axMap2 == null ? 0 : axMap2.NumLayers;

                        // Cac ham nap hien co co the xu ly loi noi bo.
                        // Vi vay phai KIEM TRA lai lop thuc te sau khi goi.
                        if (areas && !CoLopGISV3(
                            "web_mau_quan_huyen", "Địa bàn tham chiếu"))
                        {
                            HienThiDiaBanWebGis();
                        }
                        if (points && !CoLopGISV3(
                            "web_mau_don_vi", "WEB MẪU - Điểm"))
                        {
                            HienThiWebMauTrenBanDo();
                        }

                        bool hasAreas = CoLopGISV3(
                            "web_mau_quan_huyen", "Địa bàn tham chiếu");
                        bool hasPoints = CoLopGISV3(
                            "web_mau_don_vi", "WEB MẪU - Điểm");

                        refresh();
                        int after = axMap2 == null ? 0 : axMap2.NumLayers;
                        status.Text = string.Format(
                            "Trước/sau: {0}/{1} lớp | Điểm: {2} | Địa bàn: {3}",
                            before, after,
                            hasPoints ? "Đã phát hiện" : "Chưa có",
                            hasAreas ? "Đã phát hiện" : "Chưa có");

                        // Khong coi lenh goi la thanh cong neu chua co lop.
                        if ((points && !hasPoints) || (areas && !hasAreas))
                        {
                            MessageBox.Show(form,
                                "Một hoặc nhiều lớp tham chiếu chưa được nạp.\n" +
                                "Anh kiểm tra trạng thái cuối cửa sổ và " +
                                "thông báo MapWinGIS nếu có.",
                                "Kiểm tra dữ liệu GIS",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        status.Text = "Không thể nạp lớp: " + ex.Message;
                        MessageBox.Show(form, ex.Message,
                            "Lỗi nạp lớp GIS", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    finally
                    {
                        form.UseWaitCursor = oldWait;
                        btnPoints.Enabled = true;
                        btnAreas.Enabled = true;
                        btnBoth.Enabled = true;
                        _dangNapLopThuCongGIS = false;
                    }
                };

                btnPoints.Click += delegate { load(false, true); };
                btnAreas.Click += delegate { load(true, false); };
                btnBoth.Click += delegate { load(true, true); };
                btnRefresh.Click += delegate { refresh(); };

                grid.CurrentCellDirtyStateChanged += delegate
                {
                    if (grid.IsCurrentCellDirty)
                    {
                        grid.CommitEdit(
                            DataGridViewDataErrorContexts.Commit);
                    }
                };

                grid.CellValueChanged += delegate (
                    object sender, DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex < 0 || e.ColumnIndex != 1 ||
                        e.RowIndex >= records.Count || axMap2 == null)
                        return;

                    try
                    {
                        DongLopGIS item = records[e.RowIndex];
                        axMap2.set_LayerVisible(item.Handle, item.HienThi);
                        axMap2.Redraw();
                        status.Text = (item.HienThi ? "Đã bật: " : "Đã tắt: ") +
                            item.TenLop;
                    }
                    catch (Exception ex)
                    {
                        status.Text = "Không đổi được lớp: " + ex.Message;
                    }
                };

                btnZoom.Click += delegate
                {
                    DongLopGIS item = grid.CurrentRow == null
                        ? null : grid.CurrentRow.DataBoundItem as DongLopGIS;
                    if (item == null) return;

                    try
                    {
                        axMap2.ZoomToLayer(item.Handle);
                        axMap2.Redraw();
                        status.Text = "Đã thu phóng: " + item.TenLop;
                    }
                    catch (Exception ex)
                    {
                        status.Text = "Không thu phóng được: " + ex.Message;
                    }
                };

                form.Shown += delegate { refresh(); };
                form.ShowDialog(this);
            }
        }

        // ======================================================
        // 3. TEN LOP DE HIEU, KHONG SUA TEN BANG TRONG CSDL
        // ======================================================
        private static string TenLopHienThiGIS(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Lớp dữ liệu GIS";

            if (raw.IndexOf("WEB MẪU - Điểm",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return "Điểm đơn vị tham chiếu (chưa xác minh)";

            if (raw.IndexOf("Địa bàn tham chiếu",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return "Ranh giới địa bàn tham chiếu";

            return raw;
        }

        // ======================================================
        // 4. NUT CHUC NANG PHONG CACH GIAO DIEN HE THONG
        // ======================================================
        private static Button TaoNutLopGIS(
            string text, int width, Color color)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 33,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 8, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}