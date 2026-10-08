using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // DATAQUALITYFORM.CS
    //
    // Form được dựng hoàn toàn bằng code:
    // không cần Designer / resx mới.
    // =============================================================
    public partial class DataQualityForm : Form
    {
        private readonly DataQualityService
            _service;

        private readonly Action<DataQualityIssue>
            _zoomCallback;

        private readonly List<DataQualityIssue>
            _allItems =
                new List<DataQualityIssue>();

        private Label _lblPass;
        private Label _lblWarning;
        private Label _lblError;
        private Label _lblExecutedAt;

        private CheckBox _chkOnlyProblems;

        private Button _btnRun;
        private Button _btnZoom;
        private Button _btnClose;

        private DataGridView _grid;
        private Label _lblStatus;


        public DataQualityForm(
            DataQualityService service,
            Action<DataQualityIssue> zoomCallback)
        {
            _service =
                service ??
                new DataQualityService();

            _zoomCallback =
                zoomCallback;

            BuildUi();

            Load +=
                DataQualityForm_Load;
        }


        // =========================================================
        // UI
        // =========================================================
        private void BuildUi()
        {
            Text =
                "Kiểm tra chất lượng CSDL GIS - Hà Nội";

            StartPosition =
                FormStartPosition.CenterParent;

            Width =
                1120;

            Height =
                700;

            MinimumSize =
                new Size(
                    920,
                    560);

            BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            ForeColor =
                Color.White;

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------
            Panel header =
                new Panel();

            header.Dock =
                DockStyle.Top;

            header.Height =
                72;

            header.Padding =
                new Padding(
                    18,
                    10,
                    18,
                    8);

            header.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            Label title =
                new Label();

            title.AutoSize =
                true;

            title.Text =
                "KIỂM TRA CHẤT LƯỢNG CSDL GIS";

            title.ForeColor =
                Color.White;

            title.Font =
                new Font(
                    "Segoe UI",
                    14F,
                    FontStyle.Bold);

            title.Location =
                new Point(
                    18,
                    10);

            Label subTitle =
                new Label();

            subTitle.AutoSize =
                true;

            subTitle.Text =
                "Kiểm tra cấu trúc, thuộc tính, geometry, SRID và quan hệ không gian.";

            subTitle.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            subTitle.Location =
                new Point(
                    20,
                    42);

            header.Controls.Add(
                title);

            header.Controls.Add(
                subTitle);

            // -----------------------------------------------------
            // SUMMARY
            // -----------------------------------------------------
            Panel summary =
                new Panel();

            summary.Dock =
                DockStyle.Top;

            summary.Height =
                66;

            summary.Padding =
                new Padding(
                    18,
                    8,
                    18,
                    8);

            summary.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            _lblPass =
                CreateSummaryLabel(
                    "Đạt: 0",
                    18,
                    Color.FromArgb(
                        52,
                        211,
                        153));

            _lblWarning =
                CreateSummaryLabel(
                    "Cảnh báo: 0",
                    160,
                    Color.FromArgb(
                        251,
                        191,
                        36));

            _lblError =
                CreateSummaryLabel(
                    "Lỗi: 0",
                    335,
                    Color.FromArgb(
                        248,
                        113,
                        113));

            _lblExecutedAt =
                new Label();

            _lblExecutedAt.AutoSize =
                true;

            _lblExecutedAt.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            _lblExecutedAt.Location =
                new Point(
                    500,
                    23);

            summary.Controls.Add(
                _lblPass);

            summary.Controls.Add(
                _lblWarning);

            summary.Controls.Add(
                _lblError);

            summary.Controls.Add(
                _lblExecutedAt);

            // -----------------------------------------------------
            // TOOLBAR
            // -----------------------------------------------------
            Panel toolbar =
                new Panel();

            toolbar.Dock =
                DockStyle.Top;

            toolbar.Height =
                48;

            toolbar.Padding =
                new Padding(
                    18,
                    8,
                    18,
                    6);

            toolbar.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            _btnRun =
                CreateButton(
                    "Chạy kiểm tra",
                    18,
                    126);

            _btnRun.Click +=
                delegate
                {
                    RunChecks();
                };

            _btnZoom =
                CreateButton(
                    "Thu phóng tới lỗi",
                    154,
                    150);

            _btnZoom.Enabled =
                false;

            _btnZoom.Click +=
                delegate
                {
                    ZoomSelectedIssue();
                };

            _chkOnlyProblems =
                new CheckBox();

            _chkOnlyProblems.AutoSize =
                true;

            _chkOnlyProblems.Text =
                "Chỉ hiện lỗi / cảnh báo";

            _chkOnlyProblems.ForeColor =
                Color.FromArgb(
                    203,
                    213,
                    225);

            _chkOnlyProblems.Location =
                new Point(
                    325,
                    15);

            _chkOnlyProblems.CheckedChanged +=
                delegate
                {
                    ApplyFilter();
                };

            toolbar.Controls.Add(
                _btnRun);

            toolbar.Controls.Add(
                _btnZoom);

            toolbar.Controls.Add(
                _chkOnlyProblems);

            // -----------------------------------------------------
            // FOOTER
            // -----------------------------------------------------
            Panel footer =
                new Panel();

            footer.Dock =
                DockStyle.Bottom;

            footer.Height =
                52;

            footer.Padding =
                new Padding(
                    18,
                    8,
                    18,
                    8);

            footer.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            _lblStatus =
                new Label();

            _lblStatus.AutoSize =
                true;

            _lblStatus.Text =
                "Sẵn sàng.";

            _lblStatus.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            _lblStatus.Location =
                new Point(
                    18,
                    17);

            _btnClose =
                CreateButton(
                    "Đóng",
                    0,
                    90);

            _btnClose.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            _btnClose.Top =
                8;

            footer.Resize +=
                delegate
                {
                    _btnClose.Left =
                        Math.Max(
                            18,
                            footer.ClientSize.Width -
                            _btnClose.Width -
                            18);
                };

            _btnClose.Click +=
                delegate
                {
                    Close();
                };

            footer.Controls.Add(
                _lblStatus);

            footer.Controls.Add(
                _btnClose);

            // -----------------------------------------------------
            // GRID
            // -----------------------------------------------------
            _grid =
                new DataGridView();

            _grid.Dock =
                DockStyle.Fill;

            _grid.BackgroundColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            _grid.BorderStyle =
                BorderStyle.None;

            _grid.AutoGenerateColumns =
                false;

            _grid.AllowUserToAddRows =
                false;

            _grid.AllowUserToDeleteRows =
                false;

            _grid.AllowUserToResizeRows =
                false;

            _grid.ReadOnly =
                true;

            _grid.MultiSelect =
                false;

            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _grid.RowHeadersVisible =
                false;

            _grid.EnableHeadersVisualStyles =
                false;

            _grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            _grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _grid.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            _grid.DefaultCellStyle.BackColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            _grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    226,
                    232,
                    240);

            _grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    4,
                    120,
                    87);

            _grid.DefaultCellStyle.SelectionForeColor =
                Color.White;

            _grid.GridColor =
                Color.FromArgb(
                    51,
                    65,
                    85);

            AddGridColumn(
                "Severity",
                "Mức độ",
                90);

            AddGridColumn(
                "CheckName",
                "Nội dung kiểm tra",
                160);

            AddGridColumn(
                "LayerName",
                "Lớp dữ liệu",
                165);

            AddGridColumn(
                "ObjectId",
                "ID",
                58);

            AddGridColumn(
                "ObjectName",
                "Đối tượng",
                205);

            DataGridViewTextBoxColumn messageColumn =
                new DataGridViewTextBoxColumn();

            messageColumn.DataPropertyName =
                "Message";

            messageColumn.HeaderText =
                "Kết quả / lỗi phát hiện";

            messageColumn.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            messageColumn.MinimumWidth =
                260;

            _grid.Columns.Add(
                messageColumn);

            _grid.SelectionChanged +=
                delegate
                {
                    UpdateZoomButton();
                };

            _grid.CellDoubleClick +=
                delegate (
                    object sender,
                    DataGridViewCellEventArgs e)
                {
                    if (e.RowIndex >= 0)
                    {
                        ZoomSelectedIssue();
                    }
                };

            _grid.CellFormatting +=
                Grid_CellFormatting;

            Controls.Add(
                _grid);

            Controls.Add(
                footer);

            Controls.Add(
                toolbar);

            Controls.Add(
                summary);

            Controls.Add(
                header);
        }


        private Label CreateSummaryLabel(
            string text,
            int left,
            Color color)
        {
            Label label =
                new Label();

            label.AutoSize =
                true;

            label.Text =
                text;

            label.ForeColor =
                color;

            label.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            label.Location =
                new Point(
                    left,
                    20);

            return label;
        }


        private Button CreateButton(
            string text,
            int left,
            int width)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Width =
                width;

            button.Height =
                30;

            button.Left =
                left;

            button.Top =
                8;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    71,
                    85,
                    105);

            button.BackColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            button.ForeColor =
                Color.White;

            button.Cursor =
                Cursors.Hand;

            return button;
        }


        private void AddGridColumn(
            string propertyName,
            string header,
            int width)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.DataPropertyName =
                propertyName;

            column.HeaderText =
                header;

            column.Width =
                width;

            column.SortMode =
                DataGridViewColumnSortMode.Automatic;

            _grid.Columns.Add(
                column);
        }


        // =========================================================
        // LOAD / RUN
        // =========================================================
        private void DataQualityForm_Load(
            object sender,
            EventArgs e)
        {
            RunChecks();
        }


        private void RunChecks()
        {
            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                _btnRun.Enabled =
                    false;

                _btnZoom.Enabled =
                    false;

                _lblStatus.Text =
                    "Đang kiểm tra CSDL...";

                Refresh();

                DataQualityReport report =
                    _service.RunAllChecks();

                _allItems.Clear();

                if (report != null &&
                    report.Items != null)
                {
                    _allItems.AddRange(
                        report.Items);
                }

                _lblPass.Text =
                    "Đạt: " +
                    (report == null
                        ? 0
                        : report.PassCount);

                _lblWarning.Text =
                    "Cảnh báo: " +
                    (report == null
                        ? 0
                        : report.WarningCount);

                _lblError.Text =
                    "Lỗi: " +
                    (report == null
                        ? 0
                        : report.ErrorCount);

                _lblExecutedAt.Text =
                    report == null
                        ? ""
                        : "Thời điểm kiểm tra: " +
                          report.ExecutedAt
                              .ToString(
                                  "dd/MM/yyyy HH:mm:ss");

                ApplyFilter();

                if (report != null &&
                    report.ErrorCount == 0 &&
                    report.WarningCount == 0)
                {
                    _lblStatus.Text =
                        "CSDL đạt các kiểm tra hiện tại.";
                }
                else
                {
                    _lblStatus.Text =
                        "Đã kiểm tra xong. Double-click lỗi có ID để thu phóng trên bản đồ.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể chạy kiểm tra CSDL.\n\n" +
                    ex.Message,
                    "Lỗi kiểm tra CSDL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _lblStatus.Text =
                    "Kiểm tra thất bại.";
            }
            finally
            {
                _btnRun.Enabled =
                    true;

                Cursor.Current =
                    oldCursor;
            }
        }


        private void ApplyFilter()
        {
            List<DataQualityIssue> display =
                new List<DataQualityIssue>();

            foreach (DataQualityIssue item
                in _allItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (_chkOnlyProblems != null &&
                    _chkOnlyProblems.Checked &&
                    string.Equals(
                        item.Severity,
                        "Đạt",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                display.Add(
                    item);
            }

            _grid.DataSource =
                null;

            _grid.DataSource =
                display;

            UpdateZoomButton();
        }


        // =========================================================
        // ZOOM
        // =========================================================
        private DataQualityIssue
            GetSelectedIssue()
        {
            if (_grid == null ||
                _grid.CurrentRow == null)
            {
                return null;
            }

            return
                _grid.CurrentRow.DataBoundItem
                as DataQualityIssue;
        }


        private void UpdateZoomButton()
        {
            DataQualityIssue issue =
                GetSelectedIssue();

            _btnZoom.Enabled =
                issue != null &&
                issue.CanZoom &&
                _zoomCallback != null;
        }


        private void ZoomSelectedIssue()
        {
            DataQualityIssue issue =
                GetSelectedIssue();

            if (issue == null ||
                !issue.CanZoom)
            {
                return;
            }

            if (_zoomCallback == null)
            {
                return;
            }

            try
            {
                _zoomCallback(
                    issue);

                _lblStatus.Text =
                    "Đã gửi đối tượng lên bản đồ: " +
                    issue.ObjectName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể thu phóng tới đối tượng.\n\n" +
                    ex.Message,
                    "Lỗi bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }


        // =========================================================
        // MÀU THEO MỨC ĐỘ
        // =========================================================
        private void Grid_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.RowIndex >=
                    _grid.Rows.Count)
            {
                return;
            }

            DataQualityIssue issue =
                _grid.Rows[e.RowIndex]
                    .DataBoundItem
                as DataQualityIssue;

            if (issue == null)
            {
                return;
            }

            Color color =
                Color.FromArgb(
                    226,
                    232,
                    240);

            if (string.Equals(
                issue.Severity,
                "Lỗi",
                StringComparison.OrdinalIgnoreCase))
            {
                color =
                    Color.FromArgb(
                        248,
                        113,
                        113);
            }
            else if (string.Equals(
                issue.Severity,
                "Cảnh báo",
                StringComparison.OrdinalIgnoreCase))
            {
                color =
                    Color.FromArgb(
                        251,
                        191,
                        36);
            }
            else if (string.Equals(
                issue.Severity,
                "Đạt",
                StringComparison.OrdinalIgnoreCase))
            {
                color =
                    Color.FromArgb(
                        110,
                        231,
                        183);
            }

            if (_grid.Columns[e.ColumnIndex]
                .DataPropertyName ==
                "Severity")
            {
                e.CellStyle.ForeColor =
                    color;

                e.CellStyle.Font =
                    new Font(
                        _grid.Font,
                        FontStyle.Bold);
            }
        }
    }
}