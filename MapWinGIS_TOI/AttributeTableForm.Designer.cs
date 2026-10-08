namespace MapWinGIS_TOI
{
    partial class AttributeTableForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(
            bool disposing)
        {
            if (disposing &&
                components != null)
            {
                components.Dispose();
            }

            base.Dispose(
                disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components =
                new System.ComponentModel.Container();

            this.toolTip1 =
                new System.Windows.Forms.ToolTip(
                    this.components);

            this.layoutRoot =
                new System.Windows.Forms.TableLayoutPanel();

            this.panelHeader =
                new System.Windows.Forms.Panel();

            this.lblTitle =
                new System.Windows.Forms.Label();

            this.lblLayerCaption =
                new System.Windows.Forms.Label();

            this.lblLayerName =
                new System.Windows.Forms.Label();

            this.lblModeBadge =
                new System.Windows.Forms.Label();

            this.lblHeaderCount =
                new System.Windows.Forms.Label();

            this.panelSearch =
                new System.Windows.Forms.Panel();

            this.lblSearch =
                new System.Windows.Forms.Label();

            this.txtSearch =
                new System.Windows.Forms.TextBox();

            this.btnClearSearch =
                new System.Windows.Forms.Button();

            this.lblFilterField =
                new System.Windows.Forms.Label();

            this.cboFilterField =
                new System.Windows.Forms.ComboBox();

            this.lblFilterValue =
                new System.Windows.Forms.Label();

            this.cboFilterValue =
                new System.Windows.Forms.ComboBox();

            this.btnClearFilters =
                new System.Windows.Forms.Button();

            this.splitBody =
                new System.Windows.Forms.SplitContainer();

            this.gridAttributes =
                new System.Windows.Forms.DataGridView();

            this.panelDetails =
                new System.Windows.Forms.Panel();

            this.lblDetailsCaption =
                new System.Windows.Forms.Label();

            this.lblSelectedTitle =
                new System.Windows.Forms.Label();

            this.txtSelectedDetails =
                new System.Windows.Forms.TextBox();

            this.panelCommands =
                new System.Windows.Forms.Panel();

            this.flowCommands =
                new System.Windows.Forms.FlowLayoutPanel();

            this.btnAdd =
                new System.Windows.Forms.Button();

            this.btnEdit =
                new System.Windows.Forms.Button();

            this.btnDelete =
                new System.Windows.Forms.Button();

            this.btnSelectOnMap =
                new System.Windows.Forms.Button();

            this.btnZoomToRecord =
                new System.Windows.Forms.Button();

            this.btnClearSelection =
                new System.Windows.Forms.Button();

            this.btnRefresh =
                new System.Windows.Forms.Button();

            this.btnClose =
                new System.Windows.Forms.Button();

            this.statusStrip1 =
                new System.Windows.Forms.StatusStrip();

            this.lblStatus =
                new System.Windows.Forms.ToolStripStatusLabel();

            this.lblRecordCount =
                new System.Windows.Forms.ToolStripStatusLabel();

            this.layoutRoot.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelSearch.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)
                (this.splitBody)).BeginInit();

            this.splitBody.Panel1.SuspendLayout();
            this.splitBody.Panel2.SuspendLayout();
            this.splitBody.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)
                (this.gridAttributes)).BeginInit();

            this.panelDetails.SuspendLayout();
            this.panelCommands.SuspendLayout();
            this.flowCommands.SuspendLayout();
            this.statusStrip1.SuspendLayout();

            this.SuspendLayout();

            // =====================================================
            // ROOT
            // =====================================================
            this.layoutRoot.ColumnCount =
                1;

            this.layoutRoot.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.layoutRoot.RowCount =
                5;

            this.layoutRoot.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    78F));

            this.layoutRoot.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    88F));

            this.layoutRoot.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.layoutRoot.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    62F));

            this.layoutRoot.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    26F));

            this.layoutRoot.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.layoutRoot.Margin =
                new System.Windows.Forms.Padding(0);

            this.layoutRoot.Padding =
                new System.Windows.Forms.Padding(0);

            this.layoutRoot.Controls.Add(
                this.panelHeader,
                0,
                0);

            this.layoutRoot.Controls.Add(
                this.panelSearch,
                0,
                1);

            this.layoutRoot.Controls.Add(
                this.splitBody,
                0,
                2);

            this.layoutRoot.Controls.Add(
                this.panelCommands,
                0,
                3);

            this.layoutRoot.Controls.Add(
                this.statusStrip1,
                0,
                4);

            // =====================================================
            // HEADER
            // =====================================================
            this.panelHeader.BackColor =
                System.Drawing.Color.FromArgb(
                    35,
                    68,
                    101);

            this.panelHeader.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelHeader.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelHeader.Controls.Add(
                this.lblTitle);

            this.panelHeader.Controls.Add(
                this.lblLayerCaption);

            this.panelHeader.Controls.Add(
                this.lblLayerName);

            this.panelHeader.Controls.Add(
                this.lblModeBadge);

            this.panelHeader.Controls.Add(
                this.lblHeaderCount);

            this.lblTitle.AutoSize =
                true;

            this.lblTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    13F,
                    System.Drawing.FontStyle.Bold);

            this.lblTitle.ForeColor =
                System.Drawing.Color.White;

            this.lblTitle.Location =
                new System.Drawing.Point(
                    18,
                    10);

            this.lblTitle.Text =
                "BẢNG THUỘC TÍNH GIS";

            this.lblLayerCaption.AutoSize =
                true;

            this.lblLayerCaption.ForeColor =
                System.Drawing.Color.Gainsboro;

            this.lblLayerCaption.Location =
                new System.Drawing.Point(
                    19,
                    48);

            this.lblLayerCaption.Text =
                "Lớp:";

            this.lblLayerName.AutoSize =
                true;

            this.lblLayerName.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblLayerName.ForeColor =
                System.Drawing.Color.White;

            this.lblLayerName.Location =
                new System.Drawing.Point(
                    55,
                    48);

            this.lblLayerName.Text =
                "--";

            this.lblModeBadge.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblModeBadge.BackColor =
                System.Drawing.Color.FromArgb(
                    52,
                    101,
                    150);

            this.lblModeBadge.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    8.5F,
                    System.Drawing.FontStyle.Bold);

            this.lblModeBadge.ForeColor =
                System.Drawing.Color.White;

            this.lblModeBadge.Location =
                new System.Drawing.Point(
                    1040,
                    12);

            this.lblModeBadge.Size =
                new System.Drawing.Size(
                    110,
                    26);

            this.lblModeBadge.Text =
                "QUẢN LÝ";

            this.lblModeBadge.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            this.lblHeaderCount.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblHeaderCount.ForeColor =
                System.Drawing.Color.Gainsboro;

            this.lblHeaderCount.Location =
                new System.Drawing.Point(
                    960,
                    48);

            this.lblHeaderCount.Size =
                new System.Drawing.Size(
                    190,
                    20);

            this.lblHeaderCount.Text =
                "0 đối tượng";

            this.lblHeaderCount.TextAlign =
                System.Drawing.ContentAlignment.MiddleRight;

            // =====================================================
            // SEARCH + FILTER
            // =====================================================
            this.panelSearch.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.panelSearch.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelSearch.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelSearch.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelSearch.Controls.Add(
                this.lblSearch);

            this.panelSearch.Controls.Add(
                this.txtSearch);

            this.panelSearch.Controls.Add(
                this.btnClearSearch);

            this.panelSearch.Controls.Add(
                this.lblFilterField);

            this.panelSearch.Controls.Add(
                this.cboFilterField);

            this.panelSearch.Controls.Add(
                this.lblFilterValue);

            this.panelSearch.Controls.Add(
                this.cboFilterValue);

            this.panelSearch.Controls.Add(
                this.btnClearFilters);

            this.lblSearch.AutoSize =
                true;

            this.lblSearch.Location =
                new System.Drawing.Point(
                    18,
                    16);

            this.lblSearch.Text =
                "Tìm nhanh:";

            this.txtSearch.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            this.txtSearch.Location =
                new System.Drawing.Point(
                    92,
                    12);

            this.txtSearch.Size =
                new System.Drawing.Size(
                    920,
                    23);

            this.txtSearch.TabIndex =
                0;

            this.txtSearch.TextChanged +=
                new System.EventHandler(
                    this.txtSearch_TextChanged);

            this.btnClearSearch.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.btnClearSearch.Location =
                new System.Drawing.Point(
                    1025,
                    10);

            this.btnClearSearch.Size =
                new System.Drawing.Size(
                    125,
                    28);

            this.btnClearSearch.Text =
                "Xóa tìm kiếm";

            this.btnClearSearch.Click +=
                new System.EventHandler(
                    this.btnClearSearch_Click);

            this.lblFilterField.AutoSize =
                true;

            this.lblFilterField.Location =
                new System.Drawing.Point(
                    18,
                    54);

            this.lblFilterField.Text =
                "Lọc theo:";

            this.cboFilterField.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboFilterField.Location =
                new System.Drawing.Point(
                    92,
                    50);

            this.cboFilterField.Size =
                new System.Drawing.Size(
                    250,
                    23);

            this.cboFilterField.SelectedIndexChanged +=
                new System.EventHandler(
                    this.cboFilterField_SelectedIndexChanged);

            this.lblFilterValue.AutoSize =
                true;

            this.lblFilterValue.Location =
                new System.Drawing.Point(
                    365,
                    54);

            this.lblFilterValue.Text =
                "Giá trị:";

            this.cboFilterValue.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboFilterValue.Location =
                new System.Drawing.Point(
                    420,
                    50);

            this.cboFilterValue.Size =
                new System.Drawing.Size(
                    320,
                    23);

            this.cboFilterValue.SelectedIndexChanged +=
                new System.EventHandler(
                    this.cboFilterValue_SelectedIndexChanged);

            this.btnClearFilters.Location =
                new System.Drawing.Point(
                    752,
                    48);

            this.btnClearFilters.Size =
                new System.Drawing.Size(
                    110,
                    28);

            this.btnClearFilters.Text =
                "Bỏ bộ lọc";

            this.btnClearFilters.Click +=
                new System.EventHandler(
                    this.btnClearFilters_Click);

            // =====================================================
            // BODY SPLIT
            // =====================================================
            this.splitBody.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.splitBody.Margin =
                new System.Windows.Forms.Padding(0);

            this.splitBody.FixedPanel =
                System.Windows.Forms.FixedPanel.Panel2;

            this.splitBody.IsSplitterFixed =
                false;

            this.splitBody.Panel1MinSize =
                550;

            this.splitBody.Panel2MinSize =
                260;

            this.splitBody.SplitterDistance =
                840;

            this.splitBody.SplitterWidth =
                5;

            this.splitBody.Panel1.Controls.Add(
                this.gridAttributes);

            this.splitBody.Panel2.Controls.Add(
                this.panelDetails);

            // =====================================================
            // GRID
            // =====================================================
            this.gridAttributes.AllowUserToAddRows =
                false;

            this.gridAttributes.AllowUserToDeleteRows =
                false;

            this.gridAttributes.AllowUserToOrderColumns =
                true;

            this.gridAttributes.AllowUserToResizeRows =
                false;

            this.gridAttributes.BackgroundColor =
                System.Drawing.Color.White;

            this.gridAttributes.BorderStyle =
                System.Windows.Forms.BorderStyle.None;

            this.gridAttributes.ColumnHeadersHeight =
                36;

            this.gridAttributes.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            this.gridAttributes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.gridAttributes.EditMode =
                System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;

            this.gridAttributes.EnableHeadersVisualStyles =
                false;

            this.gridAttributes.ColumnHeadersDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(
                    230,
                    235,
                    240);

            this.gridAttributes.ColumnHeadersDefaultCellStyle.ForeColor =
                System.Drawing.Color.FromArgb(
                    35,
                    50,
                    65);

            this.gridAttributes.ColumnHeadersDefaultCellStyle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.gridAttributes.DefaultCellStyle.SelectionBackColor =
                System.Drawing.Color.FromArgb(
                    213,
                    232,
                    249);

            this.gridAttributes.DefaultCellStyle.SelectionForeColor =
                System.Drawing.Color.Black;

            this.gridAttributes.AlternatingRowsDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    250,
                    252);

            this.gridAttributes.GridColor =
                System.Drawing.Color.FromArgb(
                    225,
                    230,
                    235);

            this.gridAttributes.MultiSelect =
                false;

            this.gridAttributes.ReadOnly =
                true;

            this.gridAttributes.RowHeadersVisible =
                false;

            this.gridAttributes.RowTemplate.Height =
                28;

            this.gridAttributes.SelectionMode =
                System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.gridAttributes.CellClick +=
                new System.Windows.Forms.DataGridViewCellEventHandler(
                    this.gridAttributes_CellClick);

            this.gridAttributes.CellDoubleClick +=
                new System.Windows.Forms.DataGridViewCellEventHandler(
                    this.gridAttributes_CellDoubleClick);

            this.gridAttributes.SelectionChanged +=
                new System.EventHandler(
                    this.gridAttributes_SelectionChanged);

            // =====================================================
            // DETAIL PANEL
            // =====================================================
            this.panelDetails.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.panelDetails.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelDetails.Padding =
                new System.Windows.Forms.Padding(
                    14);

            this.panelDetails.Controls.Add(
                this.txtSelectedDetails);

            this.panelDetails.Controls.Add(
                this.lblSelectedTitle);

            this.panelDetails.Controls.Add(
                this.lblDetailsCaption);

            this.lblDetailsCaption.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.lblDetailsCaption.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblDetailsCaption.ForeColor =
                System.Drawing.Color.FromArgb(
                    70,
                    80,
                    90);

            this.lblDetailsCaption.Height =
                28;

            this.lblDetailsCaption.Text =
                "XEM NHANH ĐỐI TƯỢNG";

            this.lblSelectedTitle.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.lblSelectedTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F,
                    System.Drawing.FontStyle.Bold);

            this.lblSelectedTitle.ForeColor =
                System.Drawing.Color.FromArgb(
                    35,
                    68,
                    101);

            this.lblSelectedTitle.Height =
                44;

            this.lblSelectedTitle.Text =
                "CHƯA CHỌN ĐỐI TƯỢNG";

            this.lblSelectedTitle.AutoEllipsis =
                true;

            this.txtSelectedDetails.BackColor =
                System.Drawing.Color.White;

            this.txtSelectedDetails.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.txtSelectedDetails.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.txtSelectedDetails.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.txtSelectedDetails.Multiline =
                true;

            this.txtSelectedDetails.ReadOnly =
                true;

            this.txtSelectedDetails.ScrollBars =
                System.Windows.Forms.ScrollBars.Vertical;

            this.txtSelectedDetails.WordWrap =
                true;

            // =====================================================
            // COMMAND BAR
            // =====================================================
            this.panelCommands.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.panelCommands.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelCommands.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelCommands.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelCommands.Controls.Add(
                this.flowCommands);

            this.flowCommands.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.flowCommands.FlowDirection =
                System.Windows.Forms.FlowDirection.LeftToRight;

            this.flowCommands.Padding =
                new System.Windows.Forms.Padding(
                    12,
                    14,
                    8,
                    8);

            this.flowCommands.WrapContents =
                false;

            ConfigureButton(
                this.btnAdd,
                "Thêm",
                86);

            ConfigureButton(
                this.btnEdit,
                "Sửa",
                86);

            ConfigureButton(
                this.btnDelete,
                "Xóa",
                86);

            ConfigureButton(
                this.btnSelectOnMap,
                "Chọn trên bản đồ",
                132);

            ConfigureButton(
                this.btnZoomToRecord,
                "Thu phóng",
                102);

            ConfigureButton(
                this.btnClearSelection,
                "Bỏ chọn",
                92);

            ConfigureButton(
                this.btnRefresh,
                "Làm mới",
                92);

            ConfigureButton(
                this.btnClose,
                "Đóng",
                86);

            this.btnAdd.BackColor =
                System.Drawing.Color.FromArgb(
                    35,
                    68,
                    101);

            this.btnAdd.ForeColor =
                System.Drawing.Color.White;

            this.btnAdd.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnAdd.FlatAppearance.BorderSize =
                0;

            this.btnDelete.ForeColor =
                System.Drawing.Color.Firebrick;

            this.btnAdd.Click +=
                new System.EventHandler(
                    this.btnAdd_Click);

            this.btnEdit.Click +=
                new System.EventHandler(
                    this.btnEdit_Click);

            this.btnDelete.Click +=
                new System.EventHandler(
                    this.btnDelete_Click);

            this.btnSelectOnMap.Click +=
                new System.EventHandler(
                    this.btnSelectOnMap_Click);

            this.btnZoomToRecord.Click +=
                new System.EventHandler(
                    this.btnZoomToRecord_Click);

            this.btnClearSelection.Click +=
                new System.EventHandler(
                    this.btnClearSelection_Click);

            this.btnRefresh.Click +=
                new System.EventHandler(
                    this.btnRefresh_Click);

            this.btnClose.Click +=
                new System.EventHandler(
                    this.btnClose_Click);

            this.flowCommands.Controls.Add(
                this.btnAdd);

            this.flowCommands.Controls.Add(
                this.btnEdit);

            this.flowCommands.Controls.Add(
                this.btnDelete);

            this.flowCommands.Controls.Add(
                this.btnSelectOnMap);

            this.flowCommands.Controls.Add(
                this.btnZoomToRecord);

            this.flowCommands.Controls.Add(
                this.btnClearSelection);

            this.flowCommands.Controls.Add(
                this.btnRefresh);

            this.flowCommands.Controls.Add(
                this.btnClose);

            // Tooltips
            this.toolTip1.SetToolTip(
                this.btnAdd,
                "Tạo mới đối tượng nghiệp vụ.");

            this.toolTip1.SetToolTip(
                this.btnEdit,
                "Sửa thuộc tính đối tượng đang chọn.");

            this.toolTip1.SetToolTip(
                this.btnSelectOnMap,
                "Highlight đối tượng tương ứng trên bản đồ.");

            this.toolTip1.SetToolTip(
                this.btnZoomToRecord,
                "Thu phóng bản đồ tới đối tượng đang chọn.");

            this.toolTip1.SetToolTip(
                this.txtSearch,
                "Ctrl+F: chuyển nhanh tới ô tìm kiếm.");

            // =====================================================
            // STATUS
            // =====================================================
            this.statusStrip1.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.statusStrip1.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.lblStatus,
                    this.lblRecordCount
                });

            this.lblStatus.Spring =
                true;

            this.lblStatus.Text =
                "Sẵn sàng";

            this.lblStatus.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            this.lblRecordCount.BorderSides =
                System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;

            this.lblRecordCount.Text =
                "Hiển thị: 0 / 0";

            // =====================================================
            // FORM
            // =====================================================
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(
                    96F,
                    96F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Dpi;

            this.BackColor =
                System.Drawing.Color.White;

            this.ClientSize =
                new System.Drawing.Size(
                    1180,
                    720);

            this.Controls.Add(
                this.layoutRoot);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.KeyPreview =
                true;

            this.KeyDown +=
                new System.Windows.Forms.KeyEventHandler(
                    this.AttributeTableForm_KeyDown);

            this.MinimumSize =
                new System.Drawing.Size(
                    1000,
                    580);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterParent;

            this.Text =
                "Bảng thuộc tính GIS";

            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();

            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();

            this.splitBody.Panel1.ResumeLayout(false);
            this.splitBody.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)
                (this.splitBody)).EndInit();

            this.splitBody.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)
                (this.gridAttributes)).EndInit();

            this.panelDetails.ResumeLayout(false);
            this.panelDetails.PerformLayout();

            this.panelCommands.ResumeLayout(false);
            this.flowCommands.ResumeLayout(false);

            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();

            this.ResumeLayout(false);
        }

        private void ConfigureButton(
            System.Windows.Forms.Button button,
            string text,
            int width)
        {
            button.Margin =
                new System.Windows.Forms.Padding(
                    4,
                    0,
                    4,
                    0);

            button.Size =
                new System.Drawing.Size(
                    width,
                    34);

            button.Text =
                text;

            button.UseVisualStyleBackColor =
                true;
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;

        private System.Windows.Forms.TableLayoutPanel layoutRoot;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLayerCaption;
        private System.Windows.Forms.Label lblLayerName;
        private System.Windows.Forms.Label lblModeBadge;
        private System.Windows.Forms.Label lblHeaderCount;

        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnClearSearch;

        private System.Windows.Forms.Label lblFilterField;
        private System.Windows.Forms.ComboBox cboFilterField;
        private System.Windows.Forms.Label lblFilterValue;
        private System.Windows.Forms.ComboBox cboFilterValue;
        private System.Windows.Forms.Button btnClearFilters;

        private System.Windows.Forms.SplitContainer splitBody;
        private System.Windows.Forms.DataGridView gridAttributes;

        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.Label lblDetailsCaption;
        private System.Windows.Forms.Label lblSelectedTitle;
        private System.Windows.Forms.TextBox txtSelectedDetails;

        private System.Windows.Forms.Panel panelCommands;
        private System.Windows.Forms.FlowLayoutPanel flowCommands;

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSelectOnMap;
        private System.Windows.Forms.Button btnZoomToRecord;
        private System.Windows.Forms.Button btnClearSelection;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblRecordCount;
    }
}