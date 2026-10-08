namespace MapWinGIS_TOI
{
    partial class UnitSearchForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.Label lblKeyword;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.Label lblCapDonVi;
        private System.Windows.Forms.ComboBox cboCapDonVi;
        private System.Windows.Forms.Label lblLoaiDonVi;
        private System.Windows.Forms.ComboBox cboLoaiDonVi;
        private System.Windows.Forms.Label lblDiaBan;
        private System.Windows.Forms.ComboBox cboDiaBan;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClearFilters;
        private System.Windows.Forms.DataGridView gridResults;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnSelectOnMap;
        private System.Windows.Forms.Button btnZoomToUnit;
        private System.Windows.Forms.Button btnViewDetail;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblResultCount;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.lblKeyword = new System.Windows.Forms.Label();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.lblCapDonVi = new System.Windows.Forms.Label();
            this.cboCapDonVi = new System.Windows.Forms.ComboBox();
            this.lblLoaiDonVi = new System.Windows.Forms.Label();
            this.cboLoaiDonVi = new System.Windows.Forms.ComboBox();
            this.lblDiaBan = new System.Windows.Forms.Label();
            this.cboDiaBan = new System.Windows.Forms.ComboBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClearFilters = new System.Windows.Forms.Button();
            this.gridResults = new System.Windows.Forms.DataGridView();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnSelectOnMap = new System.Windows.Forms.Button();
            this.btnZoomToUnit = new System.Windows.Forms.Button();
            this.btnViewDetail = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblResultCount = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();

            this.panelHeader.SuspendLayout();
            this.panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(34, 77, 114);
            this.panelHeader.Controls.Add(this.lblSubtitle);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 78;

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(18, 12);
            this.lblTitle.Text = "TRA CỨU ĐƠN VỊ QUÂN ĐỘI";

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblSubtitle.Location = new System.Drawing.Point(20, 48);
            this.lblSubtitle.Text = "Tra cứu theo từ khóa, cấp, loại, địa bàn và trạng thái quản lý.";

            this.panelFilters.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelFilters.Controls.Add(this.lblKeyword);
            this.panelFilters.Controls.Add(this.txtKeyword);
            this.panelFilters.Controls.Add(this.lblCapDonVi);
            this.panelFilters.Controls.Add(this.cboCapDonVi);
            this.panelFilters.Controls.Add(this.lblLoaiDonVi);
            this.panelFilters.Controls.Add(this.cboLoaiDonVi);
            this.panelFilters.Controls.Add(this.lblDiaBan);
            this.panelFilters.Controls.Add(this.cboDiaBan);
            this.panelFilters.Controls.Add(this.lblTrangThai);
            this.panelFilters.Controls.Add(this.cboTrangThai);
            this.panelFilters.Controls.Add(this.btnSearch);
            this.panelFilters.Controls.Add(this.btnClearFilters);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Height = 126;

            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Location = new System.Drawing.Point(18, 17);
            this.lblKeyword.Text = "Từ khóa:";

            this.txtKeyword.Location = new System.Drawing.Point(82, 14);
            this.txtKeyword.Size = new System.Drawing.Size(520, 23);
            this.txtKeyword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtKeyword_KeyDown);

            this.lblCapDonVi.AutoSize = true;
            this.lblCapDonVi.Location = new System.Drawing.Point(18, 67);
            this.lblCapDonVi.Text = "Cấp đơn vị:";

            this.cboCapDonVi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCapDonVi.Location = new System.Drawing.Point(18, 87);
            this.cboCapDonVi.Size = new System.Drawing.Size(244, 23);

            this.lblLoaiDonVi.AutoSize = true;
            this.lblLoaiDonVi.Location = new System.Drawing.Point(282, 67);
            this.lblLoaiDonVi.Text = "Loại đơn vị:";

            this.cboLoaiDonVi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiDonVi.Location = new System.Drawing.Point(282, 87);
            this.cboLoaiDonVi.Size = new System.Drawing.Size(244, 23);

            this.lblDiaBan.AutoSize = true;
            this.lblDiaBan.Location = new System.Drawing.Point(546, 67);
            this.lblDiaBan.Text = "Địa bàn:";

            this.cboDiaBan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDiaBan.Location = new System.Drawing.Point(546, 87);
            this.cboDiaBan.Size = new System.Drawing.Size(204, 23);

            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Location = new System.Drawing.Point(770, 67);
            this.lblTrangThai.Text = "Trạng thái:";

            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Location = new System.Drawing.Point(770, 87);
            this.cboTrangThai.Size = new System.Drawing.Size(188, 23);

            this.btnSearch.Location = new System.Drawing.Point(620, 12);
            this.btnSearch.Size = new System.Drawing.Size(115, 28);
            this.btnSearch.Text = "Tra cứu";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);

            this.btnClearFilters.Location = new System.Drawing.Point(744, 12);
            this.btnClearFilters.Size = new System.Drawing.Size(125, 28);
            this.btnClearFilters.Text = "Xóa điều kiện";
            this.btnClearFilters.UseVisualStyleBackColor = true;
            this.btnClearFilters.Click += new System.EventHandler(this.btnClearFilters_Click);

            this.gridResults.AllowUserToAddRows = false;
            this.gridResults.AllowUserToDeleteRows = false;
            this.gridResults.AllowUserToResizeRows = false;
            this.gridResults.BackgroundColor = System.Drawing.Color.White;
            this.gridResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridResults.MultiSelect = false;
            this.gridResults.ReadOnly = true;
            this.gridResults.RowHeadersVisible = false;
            this.gridResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridResults.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridResults_CellClick);
            this.gridResults.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridResults_CellDoubleClick);
            this.gridResults.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridResults_CellFormatting);

            this.panelBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelBottom.Controls.Add(this.btnSelectOnMap);
            this.panelBottom.Controls.Add(this.btnZoomToUnit);
            this.panelBottom.Controls.Add(this.btnViewDetail);
            this.panelBottom.Controls.Add(this.btnRefresh);
            this.panelBottom.Controls.Add(this.btnClose);
            this.panelBottom.Controls.Add(this.lblResultCount);
            this.panelBottom.Controls.Add(this.lblStatus);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 72;

            this.btnSelectOnMap.Location = new System.Drawing.Point(14, 10);
            this.btnSelectOnMap.Size = new System.Drawing.Size(145, 30);
            this.btnSelectOnMap.Text = "Chọn trên bản đồ";
            this.btnSelectOnMap.UseVisualStyleBackColor = true;
            this.btnSelectOnMap.Click += new System.EventHandler(this.btnSelectOnMap_Click);

            this.btnZoomToUnit.Location = new System.Drawing.Point(168, 10);
            this.btnZoomToUnit.Size = new System.Drawing.Size(165, 30);
            this.btnZoomToUnit.Text = "Thu phóng tới đơn vị";
            this.btnZoomToUnit.UseVisualStyleBackColor = true;
            this.btnZoomToUnit.Click += new System.EventHandler(this.btnZoomToUnit_Click);

            this.btnViewDetail.Location = new System.Drawing.Point(342, 10);
            this.btnViewDetail.Size = new System.Drawing.Size(120, 30);
            this.btnViewDetail.Text = "Xem hồ sơ";
            this.btnViewDetail.UseVisualStyleBackColor = true;
            this.btnViewDetail.Click += new System.EventHandler(this.btnViewDetail_Click);

            this.btnRefresh.Location = new System.Drawing.Point(471, 10);
            this.btnRefresh.Size = new System.Drawing.Size(95, 30);
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.btnClose.Anchor =
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location = new System.Drawing.Point(873, 10);
            this.btnClose.Size = new System.Drawing.Size(95, 30);
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.lblResultCount.Anchor =
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right;
            this.lblResultCount.AutoSize = true;
            this.lblResultCount.Location = new System.Drawing.Point(705, 18);
            this.lblResultCount.Text = "Kết quả: 0 đơn vị";

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(14, 50);
            this.lblStatus.Text = "Sẵn sàng.";

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 620);
            this.Controls.Add(this.gridResults);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 560);
            this.Name = "UnitSearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tra cứu đơn vị quân đội";

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
