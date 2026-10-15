namespace MapWinGIS_TOI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(Form1));

            // =====================================================
            // MENU
            // =====================================================
            this.menuStrip1 =
                new System.Windows.Forms.MenuStrip();

            this.menuFile =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuLayer =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuEdit =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuView =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuQuery =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuAnalysis =
                new System.Windows.Forms.ToolStripMenuItem();

            this.menuHelp =
                new System.Windows.Forms.ToolStripMenuItem();

            // =====================================================
            // TOOLBAR CHÍNH - CHỈ MỘT HÀNG
            // =====================================================
            this.toolStripStandard =
                new System.Windows.Forms.ToolStrip();

            this.btnOpenLayer =
                new System.Windows.Forms.ToolStripButton();

            this.btnPostGIS =
                new System.Windows.Forms.ToolStripButton();

            this.sep1 =
                new System.Windows.Forms.ToolStripSeparator();

            this.btnPan =
                new System.Windows.Forms.ToolStripButton();

            this.btnZoomIn =
                new System.Windows.Forms.ToolStripButton();

            this.btnZoomOut =
                new System.Windows.Forms.ToolStripButton();

            this.btnFullExtent =
                new System.Windows.Forms.ToolStripButton();

            this.sep2 =
                new System.Windows.Forms.ToolStripSeparator();

            this.btnInfo =
                new System.Windows.Forms.ToolStripButton();

            this.btnQueryLocation =
                new System.Windows.Forms.ToolStripButton();

            this.btnQueryAttribute =
                new System.Windows.Forms.ToolStripButton();

            this.btnClearSelectionToolbar =
                new System.Windows.Forms.ToolStripButton();

            this.sep3 =
                new System.Windows.Forms.ToolStripSeparator();

            this.btnMeasure =
                new System.Windows.Forms.ToolStripButton();

            this.btnBuffer =
                new System.Windows.Forms.ToolStripButton();

            this.sep4 =
                new System.Windows.Forms.ToolStripSeparator();

            this.btnRefreshToolbar =
                new System.Windows.Forms.ToolStripButton();

            // Nút logic ẩn - Measure.cs vẫn sử dụng.
            this.btnClearMeasure =
                new System.Windows.Forms.ToolStripButton();

            // Giữ biến cũ để tránh phá code partial cũ nếu còn tham chiếu.
            // Không đưa lên giao diện.
            this.toolStripTools =
                new System.Windows.Forms.ToolStrip();

            // =====================================================
            // MAIN
            // =====================================================
            this.mainContainer =
                new System.Windows.Forms.SplitContainer();

            // =====================================================
            // DANH SÁCH LỚP
            // =====================================================
            this.panelTocHeader =
                new System.Windows.Forms.Panel();

            this.lblTocTitle =
                new System.Windows.Forms.Label();

            this.panelTocCommands =
                new System.Windows.Forms.Panel();

            this.btnAddLayer =
                new System.Windows.Forms.Button();

            this.btnRemoveLayer =
                new System.Windows.Forms.Button();

            this.treeLayers =
                new System.Windows.Forms.TreeView();

            // =====================================================
            // MAP WORKSPACE
            // =====================================================
            this.panelMapWorkspace =
                new System.Windows.Forms.Panel();

            this.panelLayerBar =
                new System.Windows.Forms.Panel();

            this.lblCurrentLayer =
                new System.Windows.Forms.Label();

            this.cboCurrentLayer =
                new System.Windows.Forms.ComboBox();

            this.panelMap =
                new System.Windows.Forms.Panel();

            this.axMap2 =
                new AxMapWinGIS.AxMap();

            this.lblMapPlaceholder =
                new System.Windows.Forms.Label();

            // =====================================================
            // HIDDEN QUERY CONTROLS
            // =====================================================
            this.txtLayerName =
                new System.Windows.Forms.TextBox();

            this.txtObjectId =
                new System.Windows.Forms.TextBox();

            this.txtXY =
                new System.Windows.Forms.TextBox();

            this.cboAttributes =
                new System.Windows.Forms.ComboBox();

            this.txtDescription =
                new System.Windows.Forms.TextBox();

            // =====================================================
            // STATUS BAR
            // =====================================================
            this.statusStrip1 =
                new System.Windows.Forms.StatusStrip();

            this.lblStatus =
                new System.Windows.Forms.ToolStripStatusLabel();

            this.lblStatusLayer =
                new System.Windows.Forms.ToolStripStatusLabel();

            this.lblStatusCoordinates =
                new System.Windows.Forms.ToolStripStatusLabel();

            this.lblStatusProjection =
                new System.Windows.Forms.ToolStripStatusLabel();

            // =====================================================
            // SUSPEND
            // =====================================================
            this.menuStrip1.SuspendLayout();
            this.toolStripStandard.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)
                (this.mainContainer)).BeginInit();

            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();

            this.panelTocHeader.SuspendLayout();
            this.panelTocCommands.SuspendLayout();

            this.panelMapWorkspace.SuspendLayout();
            this.panelLayerBar.SuspendLayout();
            this.panelMap.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)
                (this.axMap2)).BeginInit();

            this.statusStrip1.SuspendLayout();

            this.SuspendLayout();

            // =====================================================
            // MENU STRIP
            // =====================================================
            this.menuStrip1.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.menuStrip1.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.menuStrip1.GripStyle =
                System.Windows.Forms.ToolStripGripStyle.Hidden;

            this.menuStrip1.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.menuFile,
                    this.menuLayer,
                    this.menuEdit,
                    this.menuView,
                    this.menuQuery,
                    this.menuAnalysis,
                    this.menuHelp
                });

            this.menuStrip1.Location =
                new System.Drawing.Point(
                    0,
                    0);

            this.menuStrip1.Name =
                "menuStrip1";

            this.menuStrip1.Padding =
                new System.Windows.Forms.Padding(
                    6,
                    2,
                    0,
                    2);

            this.menuStrip1.Size =
                new System.Drawing.Size(
                    1400,
                    24);

            this.menuStrip1.TabIndex =
                0;

            this.menuFile.Name =
                "menuFile";

            this.menuFile.Text =
                "Tệp";

            this.menuLayer.Name =
                "menuLayer";

            this.menuLayer.Text =
                "Lớp dữ liệu";

            this.menuEdit.Name =
                "menuEdit";

            this.menuEdit.Text =
                "Chỉnh sửa";

            // Chưa có module chỉnh sửa hoàn chỉnh.
            this.menuEdit.Visible =
                false;

            this.menuView.Name =
                "menuView";

            this.menuView.Text =
                "Xem";

            this.menuQuery.Name =
                "menuQuery";

            this.menuQuery.Text =
                "Truy vấn";

            this.menuAnalysis.Name =
                "menuAnalysis";

            this.menuAnalysis.Text =
                "Phân tích";

            this.menuHelp.Name =
                "menuHelp";

            this.menuHelp.Text =
                "Trợ giúp";

            // =====================================================
            // TOOLBAR CHÍNH
            // =====================================================
            this.toolStripStandard.AutoSize =
                false;

            this.toolStripStandard.BackColor =
                System.Drawing.Color.FromArgb(
                    245,
                    246,
                    247);

            this.toolStripStandard.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.toolStripStandard.GripStyle =
                System.Windows.Forms.ToolStripGripStyle.Hidden;

            this.toolStripStandard.ImageScalingSize =
                new System.Drawing.Size(
                    20,
                    20);

            this.toolStripStandard.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.btnOpenLayer,
                    this.btnPostGIS,
                    this.sep1,

                    this.btnPan,
                    this.btnZoomIn,
                    this.btnZoomOut,
                    this.btnFullExtent,
                    this.sep2,

                    this.btnInfo,
                    this.btnQueryLocation,
                    this.btnQueryAttribute,
                    this.btnClearSelectionToolbar,
                    this.sep3,

                    this.btnMeasure,
                    this.btnBuffer,
                    this.sep4,

                    this.btnRefreshToolbar
                });

            this.toolStripStandard.Location =
                new System.Drawing.Point(
                    0,
                    24);

            this.toolStripStandard.Name =
                "toolStripStandard";

            this.toolStripStandard.Padding =
                new System.Windows.Forms.Padding(
                    5,
                    1,
                    5,
                    1);

            this.toolStripStandard.ShowItemToolTips =
                true;

            this.toolStripStandard.Size =
                new System.Drawing.Size(
                    1400,
                    31);

            this.toolStripStandard.TabIndex =
                1;

            ConfigureToolButton(
                this.btnOpenLayer,
                "Mở Shapefile");

            ConfigureToolButton(
                this.btnPostGIS,
                "Kết nối PostGIS");

            ConfigureToolButton(
                this.btnPan,
                "Di chuyển bản đồ");

            ConfigureToolButton(
                this.btnZoomIn,
                "Phóng to");

            ConfigureToolButton(
                this.btnZoomOut,
                "Thu nhỏ");

            ConfigureToolButton(
                this.btnFullExtent,
                "Toàn bộ dữ liệu");

            ConfigureToolButton(
                this.btnInfo,
                "Thông tin đối tượng");

            ConfigureToolButton(
                this.btnQueryLocation,
                "Truy vấn không gian");

            ConfigureToolButton(
                this.btnQueryAttribute,
                "Truy vấn thuộc tính");

            ConfigureToolButton(
                this.btnClearSelectionToolbar,
                "Bỏ chọn tất cả");

            ConfigureToolButton(
                this.btnMeasure,
                "Đo khoảng cách");

            ConfigureToolButton(
                this.btnBuffer,
                "Tạo vùng đệm");

            ConfigureToolButton(
                this.btnRefreshToolbar,
                "Làm mới bản đồ");

            // =====================================================
            // CONTROL LOGIC ẨN
            // =====================================================
            this.btnClearMeasure.Name =
                "btnClearMeasure";

            this.btnClearMeasure.Text =
                "Xóa kết quả đo";

            this.toolStripTools.Name =
                "toolStripTools";

            this.toolStripTools.Visible =
                false;

            // =====================================================
            // MAIN SPLIT
            // =====================================================
            this.mainContainer.BackColor =
                System.Drawing.Color.FromArgb(
                    200,
                    200,
                    200);

            this.mainContainer.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.mainContainer.FixedPanel =
                System.Windows.Forms.FixedPanel.Panel1;

            this.mainContainer.Location =
                new System.Drawing.Point(
                    0,
                    55);

            this.mainContainer.Name =
                "mainContainer";

            this.mainContainer.Panel1MinSize =
                220;

            this.mainContainer.Panel2MinSize =
                600;

            this.mainContainer.Size =
                new System.Drawing.Size(
                    1400,
                    722);

            this.mainContainer.SplitterDistance =
                260;

            this.mainContainer.SplitterWidth =
                4;

            this.mainContainer.TabIndex =
                2;

            // =====================================================
            // DANH SÁCH LỚP
            // =====================================================
            this.mainContainer.Panel1.BackColor =
                System.Drawing.Color.White;

            this.mainContainer.Panel1.Controls.Add(
                this.treeLayers);

            this.mainContainer.Panel1.Controls.Add(
                this.panelTocCommands);

            this.mainContainer.Panel1.Controls.Add(
                this.panelTocHeader);

            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------
            this.panelTocHeader.BackColor =
                System.Drawing.Color.FromArgb(
                    240,
                    242,
                    245);

            this.panelTocHeader.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelTocHeader.Controls.Add(
                this.lblTocTitle);

            this.panelTocHeader.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.panelTocHeader.Height =
                34;

            this.panelTocHeader.Name =
                "panelTocHeader";

            this.lblTocTitle.AutoSize =
                true;

            this.lblTocTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblTocTitle.Location =
                new System.Drawing.Point(
                    8,
                    8);

            this.lblTocTitle.Name =
                "lblTocTitle";

            this.lblTocTitle.Text =
                "DANH SÁCH LỚP";

            // -----------------------------------------------------
            // COMMAND BAR
            // -----------------------------------------------------
            this.panelTocCommands.BackColor =
                System.Drawing.Color.FromArgb(
                    250,
                    250,
                    250);

            this.panelTocCommands.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelTocCommands.Controls.Add(
                this.btnAddLayer);

            this.panelTocCommands.Controls.Add(
                this.btnRemoveLayer);

            this.panelTocCommands.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.panelTocCommands.Height =
                42;

            this.panelTocCommands.Name =
                "panelTocCommands";

            this.btnAddLayer.FlatStyle =
                System.Windows.Forms.FlatStyle.System;

            this.btnAddLayer.Location =
                new System.Drawing.Point(
                    7,
                    7);

            this.btnAddLayer.Name =
                "btnAddLayer";

            this.btnAddLayer.Size =
                new System.Drawing.Size(
                    112,
                    27);

            this.btnAddLayer.Text =
                "Thêm lớp...";

            this.btnRemoveLayer.FlatStyle =
                System.Windows.Forms.FlatStyle.System;

            this.btnRemoveLayer.Location =
                new System.Drawing.Point(
                    125,
                    7);

            this.btnRemoveLayer.Name =
                "btnRemoveLayer";

            this.btnRemoveLayer.Size =
                new System.Drawing.Size(
                    112,
                    27);

            this.btnRemoveLayer.Text =
                "Xóa lớp";

            // -----------------------------------------------------
            // TREE
            // -----------------------------------------------------
            this.treeLayers.BackColor =
                System.Drawing.Color.White;

            this.treeLayers.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.treeLayers.CheckBoxes =
                true;

            this.treeLayers.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.treeLayers.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.treeLayers.FullRowSelect =
                true;

            this.treeLayers.HideSelection =
                false;

            this.treeLayers.Indent =
                19;

            this.treeLayers.ItemHeight =
                24;

            this.treeLayers.Name =
                "treeLayers";

            this.treeLayers.ShowLines =
                true;

            this.treeLayers.ShowPlusMinus =
                true;

            this.treeLayers.ShowRootLines =
                true;

            // =====================================================
            // MAP WORKSPACE
            // =====================================================
            this.mainContainer.Panel2.BackColor =
                System.Drawing.Color.White;

            this.mainContainer.Panel2.Controls.Add(
                this.panelMapWorkspace);

            this.panelMapWorkspace.BackColor =
                System.Drawing.Color.White;

            this.panelMapWorkspace.Controls.Add(
                this.panelMap);

            this.panelMapWorkspace.Controls.Add(
                this.panelLayerBar);

            this.panelMapWorkspace.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelMapWorkspace.Name =
                "panelMapWorkspace";

            // =====================================================
            // CURRENT LAYER BAR
            // =====================================================
            this.panelLayerBar.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.panelLayerBar.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelLayerBar.Controls.Add(
                this.lblCurrentLayer);

            this.panelLayerBar.Controls.Add(
                this.cboCurrentLayer);

            this.panelLayerBar.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.panelLayerBar.Height =
                38;

            this.panelLayerBar.Name =
                "panelLayerBar";

            this.lblCurrentLayer.AutoSize =
                true;

            this.lblCurrentLayer.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.lblCurrentLayer.Location =
                new System.Drawing.Point(
                    10,
                    10);

            this.lblCurrentLayer.Name =
                "lblCurrentLayer";

            this.lblCurrentLayer.Text =
                "Lớp hiện tại:";

            this.cboCurrentLayer.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboCurrentLayer.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.cboCurrentLayer.Location =
                new System.Drawing.Point(
                    88,
                    7);

            this.cboCurrentLayer.Name =
                "cboCurrentLayer";

            this.cboCurrentLayer.Size =
                new System.Drawing.Size(
                    310,
                    23);

            // =====================================================
            // MAP
            // =====================================================
            this.panelMap.BackColor =
                System.Drawing.Color.White;

            this.panelMap.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelMap.Controls.Add(
                this.axMap2);

            this.panelMap.Controls.Add(
                this.lblMapPlaceholder);

            this.panelMap.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelMap.Name =
                "panelMap";

            this.axMap2.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.axMap2.Enabled =
                true;

            this.axMap2.Name =
                "axMap2";

            this.axMap2.OcxState =
                ((System.Windows.Forms.AxHost.State)
                (resources.GetObject(
                    "axMap2.OcxState")));

            this.axMap2.TabIndex =
                0;

            this.lblMapPlaceholder.BackColor =
                System.Drawing.Color.White;

            this.lblMapPlaceholder.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblMapPlaceholder.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    11F);

            this.lblMapPlaceholder.ForeColor =
                System.Drawing.Color.Gray;

            this.lblMapPlaceholder.Name =
                "lblMapPlaceholder";

            this.lblMapPlaceholder.Text =
                "Đang khởi tạo bản đồ...";

            this.lblMapPlaceholder.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            // =====================================================
            // HIDDEN QUERY CONTROLS
            // =====================================================
            this.txtLayerName.Name =
                "txtLayerName";

            this.txtLayerName.Visible =
                false;

            this.txtObjectId.Name =
                "txtObjectId";

            this.txtObjectId.Visible =
                false;

            this.txtXY.Name =
                "txtXY";

            this.txtXY.Visible =
                false;

            this.cboAttributes.Name =
                "cboAttributes";

            this.cboAttributes.Visible =
                false;

            this.txtDescription.Multiline =
                true;

            this.txtDescription.Name =
                "txtDescription";

            this.txtDescription.Visible =
                false;

            // =====================================================
            // STATUS BAR
            // =====================================================
            this.statusStrip1.BackColor =
                System.Drawing.Color.FromArgb(
                    245,
                    246,
                    247);

            this.statusStrip1.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    8.5F);

            this.statusStrip1.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.lblStatus,
                    this.lblStatusLayer,
                    this.lblStatusCoordinates,
                    this.lblStatusProjection
                });

            this.statusStrip1.Location =
                new System.Drawing.Point(
                    0,
                    777);

            this.statusStrip1.Name =
                "statusStrip1";

            this.statusStrip1.Size =
                new System.Drawing.Size(
                    1400,
                    23);

            this.lblStatus.Name =
                "lblStatus";

            this.lblStatus.Spring =
                true;

            this.lblStatus.Text =
                "Sẵn sàng";

            this.lblStatus.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            this.lblStatusLayer.BorderSides =
                System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;

            this.lblStatusLayer.Name =
                "lblStatusLayer";

            this.lblStatusLayer.AutoSize =
                false;

            this.lblStatusLayer.Width =
                180;

            this.lblStatusLayer.Text =
                "Lớp: --";

            this.lblStatusCoordinates.BorderSides =
                System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;

            this.lblStatusCoordinates.Name =
                "lblStatusCoordinates";

            this.lblStatusCoordinates.AutoSize =
                false;

            this.lblStatusCoordinates.Width =
                230;

            this.lblStatusCoordinates.Text =
                "Tọa độ: xem góc phải bản đồ";

            this.lblStatusProjection.BorderSides =
                System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;

            this.lblStatusProjection.Name =
                "lblStatusProjection";

            this.lblStatusProjection.AutoSize =
                false;

            this.lblStatusProjection.Width =
                170;

            this.lblStatusProjection.Text =
                "Hệ tọa độ: EPSG:3857";

            // =====================================================
            // FORM
            // =====================================================
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(
                    7F,
                    15F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.BackColor =
                System.Drawing.SystemColors.Control;

            this.ClientSize =
                new System.Drawing.Size(
                    1400,
                    800);

            this.Controls.Add(
                this.mainContainer);

            this.Controls.Add(
                this.statusStrip1);

            this.Controls.Add(
                this.toolStripStandard);

            this.Controls.Add(
                this.menuStrip1);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.MainMenuStrip =
                this.menuStrip1;

            this.MinimumSize =
                new System.Drawing.Size(
                    1100,
                    680);

            this.Name =
                "Form1";

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text =
                "Hệ quản trị CSDL Không gian - MapWinGIS";

            this.WindowState =
                System.Windows.Forms.FormWindowState.Maximized;

            this.Load +=
                new System.EventHandler(
                    this.Form1_Load);

            // =====================================================
            // RESUME
            // =====================================================
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();

            this.toolStripStandard.ResumeLayout(false);
            this.toolStripStandard.PerformLayout();

            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)
                (this.mainContainer)).EndInit();

            this.mainContainer.ResumeLayout(false);

            this.panelTocHeader.ResumeLayout(false);
            this.panelTocHeader.PerformLayout();

            this.panelTocCommands.ResumeLayout(false);

            this.panelMapWorkspace.ResumeLayout(false);

            this.panelLayerBar.ResumeLayout(false);
            this.panelLayerBar.PerformLayout();

            this.panelMap.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)
                (this.axMap2)).EndInit();

            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // =========================================================
        // TOOLBAR BUTTON STYLE
        // =========================================================
        private void ConfigureToolButton(
            System.Windows.Forms.ToolStripButton button,
            string tooltip)
        {
            button.DisplayStyle =
                System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            button.AutoSize =
                false;

            button.Size =
                new System.Drawing.Size(
                    28,
                    27);

            button.Margin =
                new System.Windows.Forms.Padding(
                    1,
                    1,
                    1,
                    1);

            button.Padding =
                new System.Windows.Forms.Padding(1);

            button.Text =
                tooltip;

            button.ToolTipText =
                tooltip;

            button.AutoToolTip =
                true;
        }

        #endregion

        // =========================================================
        // MENU
        // =========================================================
        private System.Windows.Forms.MenuStrip menuStrip1;

        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuLayer;
        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuQuery;
        private System.Windows.Forms.ToolStripMenuItem menuAnalysis;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;

        // =========================================================
        // TOOLBAR CHÍNH
        // =========================================================
        private System.Windows.Forms.ToolStrip toolStripStandard;

        private System.Windows.Forms.ToolStripButton btnOpenLayer;
        private System.Windows.Forms.ToolStripButton btnPostGIS;

        private System.Windows.Forms.ToolStripSeparator sep1;

        private System.Windows.Forms.ToolStripButton btnPan;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnFullExtent;

        private System.Windows.Forms.ToolStripSeparator sep2;

        private System.Windows.Forms.ToolStripButton btnInfo;
        private System.Windows.Forms.ToolStripButton btnQueryLocation;
        private System.Windows.Forms.ToolStripButton btnQueryAttribute;
        private System.Windows.Forms.ToolStripButton btnClearSelectionToolbar;

        private System.Windows.Forms.ToolStripSeparator sep3;

        private System.Windows.Forms.ToolStripButton btnMeasure;
        private System.Windows.Forms.ToolStripButton btnBuffer;

        private System.Windows.Forms.ToolStripSeparator sep4;

        private System.Windows.Forms.ToolStripButton btnRefreshToolbar;

        // =========================================================
        // CONTROL LOGIC ẨN / TƯƠNG THÍCH CODE CŨ
        // =========================================================
        private System.Windows.Forms.ToolStrip toolStripTools;
        private System.Windows.Forms.ToolStripButton btnClearMeasure;

        // =========================================================
        // MAIN
        // =========================================================
        private System.Windows.Forms.SplitContainer mainContainer;

        // =========================================================
        // DANH SÁCH LỚP
        // =========================================================
        private System.Windows.Forms.Panel panelTocHeader;
        private System.Windows.Forms.Label lblTocTitle;

        private System.Windows.Forms.Panel panelTocCommands;
        private System.Windows.Forms.Button btnAddLayer;
        private System.Windows.Forms.Button btnRemoveLayer;

        private System.Windows.Forms.TreeView treeLayers;

        // =========================================================
        // MAP WORKSPACE
        // =========================================================
        private System.Windows.Forms.Panel panelMapWorkspace;

        private System.Windows.Forms.Panel panelLayerBar;
        private System.Windows.Forms.Label lblCurrentLayer;
        private System.Windows.Forms.ComboBox cboCurrentLayer;

        private System.Windows.Forms.Panel panelMap;
        private AxMapWinGIS.AxMap axMap2;
        private System.Windows.Forms.Label lblMapPlaceholder;

        // =========================================================
        // HIDDEN QUERY CONTROLS
        // =========================================================
        private System.Windows.Forms.TextBox txtLayerName;
        private System.Windows.Forms.TextBox txtObjectId;
        private System.Windows.Forms.TextBox txtXY;
        private System.Windows.Forms.ComboBox cboAttributes;
        private System.Windows.Forms.TextBox txtDescription;

        // =========================================================
        // STATUS
        // =========================================================
        private System.Windows.Forms.StatusStrip statusStrip1;

        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusLayer;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusCoordinates;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusProjection;
    }
}
