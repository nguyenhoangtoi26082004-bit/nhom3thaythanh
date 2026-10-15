namespace MapWinGIS_TOI
{
    partial class SpatialQueryForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;

        private System.Windows.Forms.Panel panelParameters;
        private System.Windows.Forms.Label lblQueryType;
        private System.Windows.Forms.ComboBox cboQueryType;
        private System.Windows.Forms.Label lblAdministrativeArea;
        private System.Windows.Forms.ComboBox cboAdministrativeArea;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnExecute;

        private System.Windows.Forms.DataGridView gridResults;

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnSelectOnMap;
        private System.Windows.Forms.Button btnZoomToObject;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.Label lblStatus;

        private void InitializeComponent()
        {
            this.components =
                new System.ComponentModel.Container();

            this.panelHeader =
                new System.Windows.Forms.Panel();

            this.lblTitle =
                new System.Windows.Forms.Label();

            this.lblSubtitle =
                new System.Windows.Forms.Label();

            this.panelParameters =
                new System.Windows.Forms.Panel();

            this.lblQueryType =
                new System.Windows.Forms.Label();

            this.cboQueryType =
                new System.Windows.Forms.ComboBox();

            this.lblAdministrativeArea =
                new System.Windows.Forms.Label();

            this.cboAdministrativeArea =
                new System.Windows.Forms.ComboBox();

            this.lblDescription =
                new System.Windows.Forms.Label();

            this.btnExecute =
                new System.Windows.Forms.Button();

            this.gridResults =
                new System.Windows.Forms.DataGridView();

            this.panelBottom =
                new System.Windows.Forms.Panel();

            this.btnSelectOnMap =
                new System.Windows.Forms.Button();

            this.btnZoomToObject =
                new System.Windows.Forms.Button();

            this.btnRefresh =
                new System.Windows.Forms.Button();

            this.btnClose =
                new System.Windows.Forms.Button();

            this.lblResultCount =
                new System.Windows.Forms.Label();

            this.lblStatus =
                new System.Windows.Forms.Label();

            this.panelHeader.SuspendLayout();
            this.panelParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            // =====================================================
            // HEADER
            // =====================================================
            this.panelHeader.BackColor =
                System.Drawing.Color.FromArgb(
                    34,
                    77,
                    114);

            this.panelHeader.Controls.Add(
                this.lblSubtitle);

            this.panelHeader.Controls.Add(
                this.lblTitle);

            this.panelHeader.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.panelHeader.Height =
                78;

            this.lblTitle.AutoSize =
                true;

            this.lblTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    14F,
                    System.Drawing.FontStyle.Bold);

            this.lblTitle.ForeColor =
                System.Drawing.Color.White;

            this.lblTitle.Location =
                new System.Drawing.Point(
                    18,
                    12);

            this.lblTitle.Text =
                "TRUY VẤN KHÔNG GIAN";

            this.lblSubtitle.AutoSize =
                true;

            this.lblSubtitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.lblSubtitle.ForeColor =
                System.Drawing.Color.WhiteSmoke;

            this.lblSubtitle.Location =
                new System.Drawing.Point(
                    20,
                    48);

            this.lblSubtitle.Text =
                "Kiểm tra quan hệ không gian và chất lượng dữ liệu GIS của hệ thống.";

            // =====================================================
            // PARAMETERS
            // =====================================================
            this.panelParameters.BackColor =
                System.Drawing.Color.WhiteSmoke;

            this.panelParameters.Controls.Add(
                this.lblQueryType);

            this.panelParameters.Controls.Add(
                this.cboQueryType);

            this.panelParameters.Controls.Add(
                this.lblAdministrativeArea);

            this.panelParameters.Controls.Add(
                this.cboAdministrativeArea);

            this.panelParameters.Controls.Add(
                this.lblDescription);

            this.panelParameters.Controls.Add(
                this.btnExecute);

            this.panelParameters.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.panelParameters.Height =
                122;

            this.lblQueryType.AutoSize =
                true;

            this.lblQueryType.Location =
                new System.Drawing.Point(
                    18,
                    16);

            this.lblQueryType.Text =
                "Loại truy vấn:";

            this.cboQueryType.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboQueryType.Location =
                new System.Drawing.Point(
                    110,
                    13);

            this.cboQueryType.Size =
                new System.Drawing.Size(
                    330,
                    23);

            this.cboQueryType.SelectedIndexChanged +=
                new System.EventHandler(
                    this.cboQueryType_SelectedIndexChanged);

            this.lblAdministrativeArea.AutoSize =
                true;

            this.lblAdministrativeArea.Location =
                new System.Drawing.Point(
                    18,
                    50);

            this.lblAdministrativeArea.Text =
                "Địa giới:";

            this.cboAdministrativeArea.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboAdministrativeArea.Location =
                new System.Drawing.Point(
                    110,
                    47);

            this.cboAdministrativeArea.Size =
                new System.Drawing.Size(
                    330,
                    23);

            this.btnExecute.Location =
                new System.Drawing.Point(
                    460,
                    12);

            this.btnExecute.Size =
                new System.Drawing.Size(
                    120,
                    31);

            this.btnExecute.Text =
                "Thực hiện";

            this.btnExecute.UseVisualStyleBackColor =
                true;

            this.btnExecute.Click +=
                new System.EventHandler(
                    this.btnExecute_Click);

            this.lblDescription.AutoSize =
                false;

            this.lblDescription.Location =
                new System.Drawing.Point(
                    18,
                    82);

            this.lblDescription.Size =
                new System.Drawing.Size(
                    900,
                    30);

            this.lblDescription.Text =
                "Mô tả truy vấn.";

            // =====================================================
            // GRID
            // =====================================================
            this.gridResults.AllowUserToAddRows =
                false;

            this.gridResults.AllowUserToDeleteRows =
                false;

            this.gridResults.AllowUserToResizeRows =
                false;

            this.gridResults.BackgroundColor =
                System.Drawing.Color.White;

            this.gridResults.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.gridResults.MultiSelect =
                false;

            this.gridResults.ReadOnly =
                true;

            this.gridResults.RowHeadersVisible =
                false;

            this.gridResults.SelectionMode =
                System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.gridResults.CellDoubleClick +=
                new System.Windows.Forms.DataGridViewCellEventHandler(
                    this.gridResults_CellDoubleClick);

            this.gridResults.CellFormatting +=
                new System.Windows.Forms.DataGridViewCellFormattingEventHandler(
                    this.gridResults_CellFormatting);

            // =====================================================
            // BOTTOM
            // =====================================================
            this.panelBottom.BackColor =
                System.Drawing.Color.WhiteSmoke;

            this.panelBottom.Controls.Add(
                this.btnSelectOnMap);

            this.panelBottom.Controls.Add(
                this.btnZoomToObject);

            this.panelBottom.Controls.Add(
                this.btnRefresh);

            this.panelBottom.Controls.Add(
                this.btnClose);

            this.panelBottom.Controls.Add(
                this.lblResultCount);

            this.panelBottom.Controls.Add(
                this.lblStatus);

            this.panelBottom.Dock =
                System.Windows.Forms.DockStyle.Bottom;

            this.panelBottom.Height =
                72;

            this.btnSelectOnMap.Location =
                new System.Drawing.Point(
                    14,
                    10);

            this.btnSelectOnMap.Size =
                new System.Drawing.Size(
                    145,
                    30);

            this.btnSelectOnMap.Text =
                "Chọn trên bản đồ";

            this.btnSelectOnMap.UseVisualStyleBackColor =
                true;

            this.btnSelectOnMap.Click +=
                new System.EventHandler(
                    this.btnSelectOnMap_Click);

            this.btnZoomToObject.Location =
                new System.Drawing.Point(
                    168,
                    10);

            this.btnZoomToObject.Size =
                new System.Drawing.Size(
                    165,
                    30);

            this.btnZoomToObject.Text =
                "Thu phóng tới đối tượng";

            this.btnZoomToObject.UseVisualStyleBackColor =
                true;

            this.btnZoomToObject.Click +=
                new System.EventHandler(
                    this.btnZoomToObject_Click);

            this.btnRefresh.Location =
                new System.Drawing.Point(
                    342,
                    10);

            this.btnRefresh.Size =
                new System.Drawing.Size(
                    95,
                    30);

            this.btnRefresh.Text =
                "Làm mới";

            this.btnRefresh.UseVisualStyleBackColor =
                true;

            this.btnRefresh.Click +=
                new System.EventHandler(
                    this.btnRefresh_Click);

            this.btnClose.Anchor =
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right;

            this.btnClose.Location =
                new System.Drawing.Point(
                    873,
                    10);

            this.btnClose.Size =
                new System.Drawing.Size(
                    95,
                    30);

            this.btnClose.Text =
                "Đóng";

            this.btnClose.UseVisualStyleBackColor =
                true;

            this.btnClose.Click +=
                new System.EventHandler(
                    this.btnClose_Click);

            this.lblResultCount.Anchor =
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right;

            this.lblResultCount.AutoSize =
                true;

            this.lblResultCount.Location =
                new System.Drawing.Point(
                    700,
                    18);

            this.lblResultCount.Text =
                "Kết quả: 0 đối tượng";

            this.lblStatus.AutoSize =
                true;

            this.lblStatus.Location =
                new System.Drawing.Point(
                    14,
                    50);

            this.lblStatus.Text =
                "Sẵn sàng.";

            // =====================================================
            // FORM
            // =====================================================
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(
                    7F,
                    15F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.ClientSize =
                new System.Drawing.Size(
                    984,
                    640);

            this.Controls.Add(
                this.gridResults);

            this.Controls.Add(
                this.panelBottom);

            this.Controls.Add(
                this.panelParameters);

            this.Controls.Add(
                this.panelHeader);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.MinimumSize =
                new System.Drawing.Size(
                    900,
                    560);

            this.Name =
                "SpatialQueryForm";

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterParent;

            this.Text =
                "Truy vấn không gian";

            this.panelHeader.ResumeLayout(
                false);

            this.panelHeader.PerformLayout();

            this.panelParameters.ResumeLayout(
                false);

            this.panelParameters.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).EndInit();

            this.panelBottom.ResumeLayout(
                false);

            this.panelBottom.PerformLayout();

            this.ResumeLayout(
                false);
        }
    }
}