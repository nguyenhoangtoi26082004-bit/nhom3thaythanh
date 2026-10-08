namespace MapWinGIS_TOI
{
    partial class UnitDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUnitName;

        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblMaDonVi;
        private System.Windows.Forms.Label lblMaDonViValue;
        private System.Windows.Forms.Label lblCapDonVi;
        private System.Windows.Forms.Label lblCapDonViValue;
        private System.Windows.Forms.Label lblLoaiDonVi;
        private System.Windows.Forms.Label lblLoaiDonViValue;
        private System.Windows.Forms.Label lblDonViCapTren;
        private System.Windows.Forms.Label lblDonViCapTrenValue;
        private System.Windows.Forms.Label lblDiaBan;
        private System.Windows.Forms.Label lblDiaBanValue;
        private System.Windows.Forms.Label lblDiaChi;
        private System.Windows.Forms.Label lblDiaChiValue;
        private System.Windows.Forms.Label lblLienHe;
        private System.Windows.Forms.Label lblLienHeValue;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.Label lblTrangThaiValue;
        private System.Windows.Forms.Label lblGeometry;
        private System.Windows.Forms.Label lblGeometryValue;
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.TextBox txtMoTa;
        private System.Windows.Forms.Label lblGhiChu;
        private System.Windows.Forms.TextBox txtGhiChu;

        private System.Windows.Forms.GroupBox groupSpatialSummary;
        private System.Windows.Forms.Label lblRouteCount;
        private System.Windows.Forms.Label lblRouteCountValue;
        private System.Windows.Forms.Label lblRouteLength;
        private System.Windows.Forms.Label lblRouteLengthValue;
        private System.Windows.Forms.Label lblAreaCount;
        private System.Windows.Forms.Label lblAreaCountValue;
        private System.Windows.Forms.Label lblAreaTotal;
        private System.Windows.Forms.Label lblAreaTotalValue;
        private System.Windows.Forms.Label lblPerimeterTotal;
        private System.Windows.Forms.Label lblPerimeterTotalValue;

        private System.Windows.Forms.TabControl tabSpatial;
        private System.Windows.Forms.TabPage tabRoutes;
        private System.Windows.Forms.TabPage tabAreas;
        private System.Windows.Forms.DataGridView gridRoutes;
        private System.Windows.Forms.DataGridView gridAreas;

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnSelectOnMap;
        private System.Windows.Forms.Button btnZoomToUnit;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_routesTable != null)
                {
                    _routesTable.Dispose();
                    _routesTable = null;
                }

                if (_areasTable != null)
                {
                    _areasTable.Dispose();
                    _areasTable = null;
                }

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUnitName = new System.Windows.Forms.Label();

            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblMaDonVi = new System.Windows.Forms.Label();
            this.lblMaDonViValue = new System.Windows.Forms.Label();
            this.lblCapDonVi = new System.Windows.Forms.Label();
            this.lblCapDonViValue = new System.Windows.Forms.Label();
            this.lblLoaiDonVi = new System.Windows.Forms.Label();
            this.lblLoaiDonViValue = new System.Windows.Forms.Label();
            this.lblDonViCapTren = new System.Windows.Forms.Label();
            this.lblDonViCapTrenValue = new System.Windows.Forms.Label();
            this.lblDiaBan = new System.Windows.Forms.Label();
            this.lblDiaBanValue = new System.Windows.Forms.Label();
            this.lblDiaChi = new System.Windows.Forms.Label();
            this.lblDiaChiValue = new System.Windows.Forms.Label();
            this.lblLienHe = new System.Windows.Forms.Label();
            this.lblLienHeValue = new System.Windows.Forms.Label();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.lblTrangThaiValue = new System.Windows.Forms.Label();
            this.lblGeometry = new System.Windows.Forms.Label();
            this.lblGeometryValue = new System.Windows.Forms.Label();
            this.lblMoTa = new System.Windows.Forms.Label();
            this.txtMoTa = new System.Windows.Forms.TextBox();
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.txtGhiChu = new System.Windows.Forms.TextBox();

            this.groupSpatialSummary = new System.Windows.Forms.GroupBox();
            this.lblRouteCount = new System.Windows.Forms.Label();
            this.lblRouteCountValue = new System.Windows.Forms.Label();
            this.lblRouteLength = new System.Windows.Forms.Label();
            this.lblRouteLengthValue = new System.Windows.Forms.Label();
            this.lblAreaCount = new System.Windows.Forms.Label();
            this.lblAreaCountValue = new System.Windows.Forms.Label();
            this.lblAreaTotal = new System.Windows.Forms.Label();
            this.lblAreaTotalValue = new System.Windows.Forms.Label();
            this.lblPerimeterTotal = new System.Windows.Forms.Label();
            this.lblPerimeterTotalValue = new System.Windows.Forms.Label();

            this.tabSpatial = new System.Windows.Forms.TabControl();
            this.tabRoutes = new System.Windows.Forms.TabPage();
            this.tabAreas = new System.Windows.Forms.TabPage();
            this.gridRoutes = new System.Windows.Forms.DataGridView();
            this.gridAreas = new System.Windows.Forms.DataGridView();

            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnSelectOnMap = new System.Windows.Forms.Button();
            this.btnZoomToUnit = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();

            this.panelHeader.SuspendLayout();
            this.panelInfo.SuspendLayout();
            this.groupSpatialSummary.SuspendLayout();
            this.tabSpatial.SuspendLayout();
            this.tabRoutes.SuspendLayout();
            this.tabAreas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRoutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAreas)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            // Header
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(34, 77, 114);
            this.panelHeader.Controls.Add(this.lblUnitName);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 86;

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblTitle.Location = new System.Drawing.Point(18, 12);
            this.lblTitle.Text = "HỒ SƠ ĐƠN VỊ";

            this.lblUnitName.AutoSize = true;
            this.lblUnitName.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblUnitName.ForeColor = System.Drawing.Color.White;
            this.lblUnitName.Location = new System.Drawing.Point(17, 35);
            this.lblUnitName.Text = "Tên đơn vị";

            // Info panel
            this.panelInfo.BackColor = System.Drawing.Color.White;
            this.panelInfo.Controls.Add(this.lblMaDonVi);
            this.panelInfo.Controls.Add(this.lblMaDonViValue);
            this.panelInfo.Controls.Add(this.lblCapDonVi);
            this.panelInfo.Controls.Add(this.lblCapDonViValue);
            this.panelInfo.Controls.Add(this.lblLoaiDonVi);
            this.panelInfo.Controls.Add(this.lblLoaiDonViValue);
            this.panelInfo.Controls.Add(this.lblDonViCapTren);
            this.panelInfo.Controls.Add(this.lblDonViCapTrenValue);
            this.panelInfo.Controls.Add(this.lblDiaBan);
            this.panelInfo.Controls.Add(this.lblDiaBanValue);
            this.panelInfo.Controls.Add(this.lblDiaChi);
            this.panelInfo.Controls.Add(this.lblDiaChiValue);
            this.panelInfo.Controls.Add(this.lblLienHe);
            this.panelInfo.Controls.Add(this.lblLienHeValue);
            this.panelInfo.Controls.Add(this.lblTrangThai);
            this.panelInfo.Controls.Add(this.lblTrangThaiValue);
            this.panelInfo.Controls.Add(this.lblGeometry);
            this.panelInfo.Controls.Add(this.lblGeometryValue);
            this.panelInfo.Controls.Add(this.lblMoTa);
            this.panelInfo.Controls.Add(this.txtMoTa);
            this.panelInfo.Controls.Add(this.lblGhiChu);
            this.panelInfo.Controls.Add(this.txtGhiChu);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfo.Height = 260;

            int xLabel1 = 18;
            int xValue1 = 145;
            int xLabel2 = 490;
            int xValue2 = 620;

            this.lblMaDonVi.Location = new System.Drawing.Point(xLabel1, 15);
            this.lblMaDonVi.AutoSize = true;
            this.lblMaDonVi.Text = "Mã đơn vị:";
            this.lblMaDonViValue.Location = new System.Drawing.Point(xValue1, 15);
            this.lblMaDonViValue.AutoSize = true;
            this.lblMaDonViValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblCapDonVi.Location = new System.Drawing.Point(xLabel1, 42);
            this.lblCapDonVi.AutoSize = true;
            this.lblCapDonVi.Text = "Cấp đơn vị:";
            this.lblCapDonViValue.Location = new System.Drawing.Point(xValue1, 42);
            this.lblCapDonViValue.AutoSize = true;

            this.lblLoaiDonVi.Location = new System.Drawing.Point(xLabel1, 69);
            this.lblLoaiDonVi.AutoSize = true;
            this.lblLoaiDonVi.Text = "Loại đơn vị:";
            this.lblLoaiDonViValue.Location = new System.Drawing.Point(xValue1, 69);
            this.lblLoaiDonViValue.AutoSize = true;

            this.lblDonViCapTren.Location = new System.Drawing.Point(xLabel1, 96);
            this.lblDonViCapTren.AutoSize = true;
            this.lblDonViCapTren.Text = "Đơn vị cấp trên:";
            this.lblDonViCapTrenValue.Location = new System.Drawing.Point(xValue1, 96);
            this.lblDonViCapTrenValue.AutoSize = true;

            this.lblDiaBan.Location = new System.Drawing.Point(xLabel2, 15);
            this.lblDiaBan.AutoSize = true;
            this.lblDiaBan.Text = "Địa bàn:";
            this.lblDiaBanValue.Location = new System.Drawing.Point(xValue2, 15);
            this.lblDiaBanValue.AutoSize = true;

            this.lblDiaChi.Location = new System.Drawing.Point(xLabel2, 42);
            this.lblDiaChi.AutoSize = true;
            this.lblDiaChi.Text = "Địa chỉ:";
            this.lblDiaChiValue.Location = new System.Drawing.Point(xValue2, 42);
            this.lblDiaChiValue.AutoSize = true;

            this.lblLienHe.Location = new System.Drawing.Point(xLabel2, 69);
            this.lblLienHe.AutoSize = true;
            this.lblLienHe.Text = "Liên hệ:";
            this.lblLienHeValue.Location = new System.Drawing.Point(xValue2, 69);
            this.lblLienHeValue.AutoSize = true;

            this.lblTrangThai.Location = new System.Drawing.Point(xLabel2, 96);
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Text = "Trạng thái:";
            this.lblTrangThaiValue.Location = new System.Drawing.Point(xValue2, 96);
            this.lblTrangThaiValue.AutoSize = true;

            this.lblGeometry.Location = new System.Drawing.Point(xLabel2, 123);
            this.lblGeometry.AutoSize = true;
            this.lblGeometry.Text = "Vị trí GIS:";
            this.lblGeometryValue.Location = new System.Drawing.Point(xValue2, 123);
            this.lblGeometryValue.AutoSize = true;

            this.lblMoTa.Location = new System.Drawing.Point(18, 140);
            this.lblMoTa.AutoSize = true;
            this.lblMoTa.Text = "Mô tả:";
            this.txtMoTa.Location = new System.Drawing.Point(18, 162);
            this.txtMoTa.Size = new System.Drawing.Size(450, 70);
            this.txtMoTa.Multiline = true;
            this.txtMoTa.ReadOnly = true;
            this.txtMoTa.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            this.lblGhiChu.Location = new System.Drawing.Point(490, 140);
            this.lblGhiChu.AutoSize = true;
            this.lblGhiChu.Text = "Ghi chú:";
            this.txtGhiChu.Location = new System.Drawing.Point(490, 162);
            this.txtGhiChu.Size = new System.Drawing.Size(450, 70);
            this.txtGhiChu.Multiline = true;
            this.txtGhiChu.ReadOnly = true;
            this.txtGhiChu.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            // Summary
            this.groupSpatialSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupSpatialSummary.Height = 78;
            this.groupSpatialSummary.Text = "Thống kê dữ liệu không gian";
            this.groupSpatialSummary.Controls.Add(this.lblRouteCount);
            this.groupSpatialSummary.Controls.Add(this.lblRouteCountValue);
            this.groupSpatialSummary.Controls.Add(this.lblRouteLength);
            this.groupSpatialSummary.Controls.Add(this.lblRouteLengthValue);
            this.groupSpatialSummary.Controls.Add(this.lblAreaCount);
            this.groupSpatialSummary.Controls.Add(this.lblAreaCountValue);
            this.groupSpatialSummary.Controls.Add(this.lblAreaTotal);
            this.groupSpatialSummary.Controls.Add(this.lblAreaTotalValue);
            this.groupSpatialSummary.Controls.Add(this.lblPerimeterTotal);
            this.groupSpatialSummary.Controls.Add(this.lblPerimeterTotalValue);

            this.lblRouteCount.Location = new System.Drawing.Point(18, 32);
            this.lblRouteCount.AutoSize = true;
            this.lblRouteCount.Text = "Số tuyến:";
            this.lblRouteCountValue.Location = new System.Drawing.Point(82, 32);
            this.lblRouteCountValue.AutoSize = true;
            this.lblRouteCountValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblRouteLength.Location = new System.Drawing.Point(145, 32);
            this.lblRouteLength.AutoSize = true;
            this.lblRouteLength.Text = "Tổng chiều dài:";
            this.lblRouteLengthValue.Location = new System.Drawing.Point(245, 32);
            this.lblRouteLengthValue.AutoSize = true;
            this.lblRouteLengthValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblAreaCount.Location = new System.Drawing.Point(365, 32);
            this.lblAreaCount.AutoSize = true;
            this.lblAreaCount.Text = "Số khu vực:";
            this.lblAreaCountValue.Location = new System.Drawing.Point(440, 32);
            this.lblAreaCountValue.AutoSize = true;
            this.lblAreaCountValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblAreaTotal.Location = new System.Drawing.Point(500, 32);
            this.lblAreaTotal.AutoSize = true;
            this.lblAreaTotal.Text = "Tổng diện tích:";
            this.lblAreaTotalValue.Location = new System.Drawing.Point(595, 32);
            this.lblAreaTotalValue.AutoSize = true;
            this.lblAreaTotalValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.lblPerimeterTotal.Location = new System.Drawing.Point(730, 32);
            this.lblPerimeterTotal.AutoSize = true;
            this.lblPerimeterTotal.Text = "Tổng chu vi:";
            this.lblPerimeterTotalValue.Location = new System.Drawing.Point(810, 32);
            this.lblPerimeterTotalValue.AutoSize = true;
            this.lblPerimeterTotalValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Tabs
            this.tabSpatial.Controls.Add(this.tabRoutes);
            this.tabSpatial.Controls.Add(this.tabAreas);
            this.tabSpatial.Dock = System.Windows.Forms.DockStyle.Fill;

            this.tabRoutes.Text = "Tuyến đường";
            this.tabRoutes.Controls.Add(this.gridRoutes);

            this.tabAreas.Text = "Khu vực";
            this.tabAreas.Controls.Add(this.gridAreas);

            this.gridRoutes.AllowUserToAddRows = false;
            this.gridRoutes.AllowUserToDeleteRows = false;
            this.gridRoutes.ReadOnly = true;
            this.gridRoutes.RowHeadersVisible = false;
            this.gridRoutes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRoutes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRoutes.BackgroundColor = System.Drawing.Color.White;
            this.gridRoutes.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridRoutes_CellFormatting);

            this.gridAreas.AllowUserToAddRows = false;
            this.gridAreas.AllowUserToDeleteRows = false;
            this.gridAreas.ReadOnly = true;
            this.gridAreas.RowHeadersVisible = false;
            this.gridAreas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAreas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAreas.BackgroundColor = System.Drawing.Color.White;
            this.gridAreas.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridAreas_CellFormatting);

            // Bottom
            this.panelBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 70;
            this.panelBottom.Controls.Add(this.btnSelectOnMap);
            this.panelBottom.Controls.Add(this.btnZoomToUnit);
            this.panelBottom.Controls.Add(this.btnEdit);
            this.panelBottom.Controls.Add(this.btnRefresh);
            this.panelBottom.Controls.Add(this.btnClose);
            this.panelBottom.Controls.Add(this.lblStatus);

            this.btnSelectOnMap.Location = new System.Drawing.Point(14, 9);
            this.btnSelectOnMap.Size = new System.Drawing.Size(135, 30);
            this.btnSelectOnMap.Text = "Chọn trên bản đồ";
            this.btnSelectOnMap.Click += new System.EventHandler(this.btnSelectOnMap_Click);

            this.btnZoomToUnit.Location = new System.Drawing.Point(158, 9);
            this.btnZoomToUnit.Size = new System.Drawing.Size(150, 30);
            this.btnZoomToUnit.Text = "Thu phóng tới đơn vị";
            this.btnZoomToUnit.Click += new System.EventHandler(this.btnZoomToUnit_Click);

            this.btnEdit.Location = new System.Drawing.Point(317, 9);
            this.btnEdit.Size = new System.Drawing.Size(125, 30);
            this.btnEdit.Text = "Sửa thông tin";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            this.btnRefresh.Location = new System.Drawing.Point(451, 9);
            this.btnRefresh.Size = new System.Drawing.Size(90, 30);
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location = new System.Drawing.Point(858, 9);
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.Text = "Đóng";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.lblStatus.Location = new System.Drawing.Point(14, 47);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Text = "Sẵn sàng.";

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 760);
            this.Controls.Add(this.tabSpatial);
            this.Controls.Add(this.groupSpatialSummary);
            this.Controls.Add(this.panelInfo);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 700);
            this.Name = "UnitDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hồ sơ đơn vị";

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.groupSpatialSummary.ResumeLayout(false);
            this.groupSpatialSummary.PerformLayout();
            this.tabSpatial.ResumeLayout(false);
            this.tabRoutes.ResumeLayout(false);
            this.tabAreas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridRoutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAreas)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}   