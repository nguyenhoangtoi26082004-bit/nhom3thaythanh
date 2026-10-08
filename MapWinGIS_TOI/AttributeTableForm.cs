using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // BẢNG THUỘC TÍNH PRO
    //
    // ĐỀ TÀI:
    // "Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    //  các đơn vị quân đội trên địa bàn Hà Nội"
    //
    // Mục tiêu UI/UX:
    // - Bảng thuộc tính giống một GIS Desktop chuyên dụng;
    // - Tìm nhanh + lọc chính xác;
    // - Đồng bộ dòng đang chọn với đối tượng trên bản đồ;
    // - Có panel xem nhanh thuộc tính;
    // - CRUD chỉ bật cho 3 lớp nghiệp vụ;
    // - lớp nền hành chính chỉ xem/tra cứu;
    // - System.Drawing.Point đơn vị được hiểu là System.Drawing.Point đại diện hành chính.
    // =============================================================
    public partial class AttributeTableForm : Form
    {
        private readonly AxMapWinGIS.AxMap _map;
        private readonly int _layerHandle;
        private readonly string _layerKey;
        private readonly string _layerDisplayName;

        private readonly PostGISRepository _repository;
        private readonly Action<string> _refreshLayerCallback;

        private readonly bool _isManagedPostGISLayer;

        private DataTable _sourceTable;
        private DataView _dataView;

        private bool _loading;
        private bool _updatingFilters;

        // =========================================================
        // YÊU CẦU CHUYỂN SANG MODULE EDITING
        // =========================================================
        public string RequestedSpatialCreateTable
        {
            get;
            private set;
        }

        // =========================================================
        // ITEM HIỂN THỊ TRONG COMBO LỌC
        // =========================================================
        private sealed class FilterFieldItem
        {
            public string FieldName
            {
                get;
                set;
            }

            public string DisplayName
            {
                get;
                set;
            }

            public override string ToString()
            {
                return DisplayName ?? "";
            }
        }

        private sealed class FilterValueItem
        {
            public string RawValue
            {
                get;
                set;
            }

            public string DisplayValue
            {
                get;
                set;
            }

            public override string ToString()
            {
                return DisplayValue ?? "";
            }
        }

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public AttributeTableForm(
            AxMapWinGIS.AxMap map,
            int layerHandle,
            string layerKey,
            string layerDisplayName,
            Action<string> refreshLayerCallback)
        {
            InitializeComponentSafe();

            _map =
                map;

            _layerHandle =
                layerHandle;

            _layerKey =
                layerKey ?? "";

            _layerDisplayName =
                string.IsNullOrWhiteSpace(
                    layerDisplayName)
                    ? _layerKey
                    : layerDisplayName;

            _refreshLayerCallback =
                refreshLayerCallback;

            RequestedSpatialCreateTable =
                "";

            _repository =
                new PostGISRepository();

            _isManagedPostGISLayer =
                _repository.IsManagedTable(
                    _layerKey);

            Text =
                "Bảng thuộc tính - " +
                _layerDisplayName;

            lblLayerName.Text =
                _layerDisplayName;

            lblModeBadge.Text =
                _isManagedPostGISLayer
                    ? "QUẢN LÝ"
                    : "CHỈ XEM";

            Load -=
                AttributeTableForm_Load;

            Load +=
                AttributeTableForm_Load;

            FormClosed -=
                AttributeTableForm_FormClosed;

            FormClosed +=
                AttributeTableForm_FormClosed;

            Shown +=
                AttributeTableForm_Shown;

            gridAttributes.CellFormatting -=
                gridAttributes_CellFormatting;

            gridAttributes.CellFormatting +=
                gridAttributes_CellFormatting;
        }

        // =========================================================
        // KHỞI TẠO GIAO DIỆN AN TOÀN
        //
        // Không gọi AttributeTableForm.Designer.InitializeComponent()
        // để loại bỏ hoàn toàn lỗi SplitContainer:
        // "SplitterDistance must be between Panel1MinSize and
        //  Width - Panel2MinSize".
        //
        // Toàn bộ form được dựng bằng TableLayoutPanel nên co giãn
        // ổn định ở mọi độ phân giải và DPI.
        // =========================================================
        private void InitializeComponentSafe()
        {
            SuspendLayout();

            // -----------------------------------------------------
            // FORM
            // -----------------------------------------------------
            Text =
                "Bảng thuộc tính";

            StartPosition =
                FormStartPosition.CenterParent;

            Width =
                1180;

            Height =
                720;

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
                    9F,
                    FontStyle.Regular);

            KeyPreview =
                true;

            KeyDown +=
                AttributeTableForm_KeyDown;


            // -----------------------------------------------------
            // CÁC CONTROL MÀ LOGIC AttributeTable.cs ĐANG DÙNG
            // -----------------------------------------------------
            lblLayerName =
                new System.Windows.Forms.Label();

            lblModeBadge =
                new System.Windows.Forms.Label();

            lblHeaderCount =
                new System.Windows.Forms.Label();

            txtSearch =
                new TextBox();

            btnClearSearch =
                new Button();

            cboFilterField =
                new ComboBox();

            cboFilterValue =
                new ComboBox();

            btnClearFilters =
                new Button();

            gridAttributes =
                new DataGridView();

            lblSelectedTitle =
                new System.Windows.Forms.Label();

            txtSelectedDetails =
                new TextBox();

            lblRecordCount =
                new ToolStripStatusLabel();

            lblStatus =
                new ToolStripStatusLabel();

            btnAdd =
                new Button();

            btnEdit =
                new Button();

            btnDelete =
                new Button();

            btnSelectOnMap =
                new Button();

            btnZoomToRecord =
                new Button();

            btnClearSelection =
                new Button();

            Button btnRefresh =
                new Button();

            Button btnClose =
                new Button();


            // -----------------------------------------------------
            // ROOT LAYOUT
            // -----------------------------------------------------
            TableLayoutPanel root =
                new TableLayoutPanel();

            root.Dock =
                DockStyle.Fill;

            root.Padding =
                new Padding(
                    14);

            root.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            root.ColumnCount =
                1;

            root.RowCount =
                5;

            root.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    58F));

            root.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    78F));

            root.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            root.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    52F));

            root.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    30F));


            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------
            Panel header =
                new Panel();

            header.Dock =
                DockStyle.Fill;

            header.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            header.Padding =
                new Padding(
                    12,
                    8,
                    12,
                    6);

            lblLayerName.AutoSize =
                true;

            lblLayerName.Text =
                "Lớp dữ liệu";

            lblLayerName.Font =
                new Font(
                    "Segoe UI",
                    13F,
                    FontStyle.Bold);

            lblLayerName.ForeColor =
                Color.White;

            lblLayerName.Location =
                new System.Drawing.Point(
                    12,
                    8);

            lblModeBadge.AutoSize =
                true;

            lblModeBadge.Text =
                "CHỈ XEM";

            lblModeBadge.Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold);

            lblModeBadge.ForeColor =
                Color.FromArgb(
                    110,
                    231,
                    183);

            lblModeBadge.BackColor =
                Color.FromArgb(
                    6,
                    78,
                    59);

            lblModeBadge.Padding =
                new Padding(
                    7,
                    3,
                    7,
                    3);

            lblModeBadge.Location =
                new System.Drawing.Point(
                    300,
                    9);

            lblHeaderCount.AutoSize =
                true;

            lblHeaderCount.Text =
                "0 bản ghi";

            lblHeaderCount.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            lblHeaderCount.Location =
                new System.Drawing.Point(
                    14,
                    36);

            header.Controls.Add(
                lblLayerName);

            header.Controls.Add(
                lblModeBadge);

            header.Controls.Add(
                lblHeaderCount);


            // -----------------------------------------------------
            // FILTER BAR
            // -----------------------------------------------------
            TableLayoutPanel filters =
                new TableLayoutPanel();

            filters.Dock =
                DockStyle.Fill;

            filters.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            filters.Padding =
                new Padding(
                    10,
                    8,
                    10,
                    8);

            filters.ColumnCount =
                6;

            filters.RowCount =
                2;

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    92F));

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    45F));

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    100F));

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    27F));

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    28F));

            filters.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    110F));

            filters.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    28F));

            filters.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    28F));

            System.Windows.Forms.Label lblSearch =
                CreateSafeCaptionLabel(
                    "Tìm kiếm:");

            filters.Controls.Add(
                lblSearch,
                0,
                0);

            txtSearch.Dock =
                DockStyle.Fill;

            txtSearch.BorderStyle =
                BorderStyle.FixedSingle;

            txtSearch.BackColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            txtSearch.ForeColor =
                Color.White;

            txtSearch.TextChanged +=
                txtSearch_TextChanged;

            filters.Controls.Add(
                txtSearch,
                1,
                0);

            ConfigureSafeButton(
                btnClearSearch,
                "Xóa tìm",
                false);

            btnClearSearch.Click +=
                btnClearSearch_Click;

            filters.Controls.Add(
                btnClearSearch,
                2,
                0);

            System.Windows.Forms.Label lblFilter =
                CreateSafeCaptionLabel(
                    "Lọc theo:");

            filters.Controls.Add(
                lblFilter,
                0,
                1);

            cboFilterField.Dock =
                DockStyle.Fill;

            cboFilterField.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboFilterField.BackColor =
                Color.White;

            cboFilterField.SelectedIndexChanged +=
                cboFilterField_SelectedIndexChanged;

            filters.Controls.Add(
                cboFilterField,
                1,
                1);

            cboFilterValue.Dock =
                DockStyle.Fill;

            cboFilterValue.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboFilterValue.BackColor =
                Color.White;

            cboFilterValue.SelectedIndexChanged +=
                cboFilterValue_SelectedIndexChanged;

            filters.Controls.Add(
                cboFilterValue,
                3,
                1);

            ConfigureSafeButton(
                btnClearFilters,
                "Xóa bộ lọc",
                false);

            btnClearFilters.Click +=
                btnClearFilters_Click;

            filters.Controls.Add(
                btnClearFilters,
                5,
                1);


            // -----------------------------------------------------
            // NỘI DUNG - KHÔNG DÙNG SPLITCONTAINER
            // -----------------------------------------------------
            TableLayoutPanel content =
                new TableLayoutPanel();

            content.Dock =
                DockStyle.Fill;

            content.Margin =
                new Padding(
                    0,
                    8,
                    0,
                    8);

            content.ColumnCount =
                2;

            content.RowCount =
                1;

            content.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    72F));

            content.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    28F));

            // GRID
            gridAttributes.Dock =
                DockStyle.Fill;

            gridAttributes.BackgroundColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            gridAttributes.BorderStyle =
                BorderStyle.FixedSingle;

            gridAttributes.AllowUserToAddRows =
                false;

            gridAttributes.AllowUserToDeleteRows =
                false;

            gridAttributes.AllowUserToResizeRows =
                false;

            gridAttributes.ReadOnly =
                true;

            gridAttributes.MultiSelect =
                false;

            gridAttributes.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            gridAttributes.RowHeadersVisible =
                false;

            gridAttributes.AutoGenerateColumns =
                true;

            gridAttributes.EnableHeadersVisualStyles =
                false;

            gridAttributes.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            gridAttributes.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            gridAttributes.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            gridAttributes.DefaultCellStyle.BackColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            gridAttributes.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    226,
                    232,
                    240);

            gridAttributes.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    4,
                    120,
                    87);

            gridAttributes.DefaultCellStyle.SelectionForeColor =
                Color.White;

            gridAttributes.GridColor =
                Color.FromArgb(
                    51,
                    65,
                    85);

            gridAttributes.CellClick +=
                gridAttributes_CellClick;

            gridAttributes.CellDoubleClick +=
                gridAttributes_CellDoubleClick;

            gridAttributes.SelectionChanged +=
                gridAttributes_SelectionChanged;

            content.Controls.Add(
                gridAttributes,
                0,
                0);

            // DETAIL
            Panel detailPanel =
                new Panel();

            detailPanel.Dock =
                DockStyle.Fill;

            detailPanel.Margin =
                new Padding(
                    8,
                    0,
                    0,
                    0);

            detailPanel.Padding =
                new Padding(
                    10);

            detailPanel.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            lblSelectedTitle.Dock =
                DockStyle.Top;

            lblSelectedTitle.Height =
                34;

            lblSelectedTitle.Text =
                "CHƯA CHỌN ĐỐI TƯỢNG";

            lblSelectedTitle.Font =
                new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Bold);

            lblSelectedTitle.ForeColor =
                Color.FromArgb(
                    110,
                    231,
                    183);

            txtSelectedDetails.Dock =
                DockStyle.Fill;

            txtSelectedDetails.Multiline =
                true;

            txtSelectedDetails.ReadOnly =
                true;

            txtSelectedDetails.ScrollBars =
                ScrollBars.Vertical;

            txtSelectedDetails.BorderStyle =
                BorderStyle.None;

            txtSelectedDetails.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            txtSelectedDetails.ForeColor =
                Color.FromArgb(
                    203,
                    213,
                    225);

            detailPanel.Controls.Add(
                txtSelectedDetails);

            detailPanel.Controls.Add(
                lblSelectedTitle);

            content.Controls.Add(
                detailPanel,
                1,
                0);


            // -----------------------------------------------------
            // ACTION BAR
            // -----------------------------------------------------
            FlowLayoutPanel actions =
                new FlowLayoutPanel();

            actions.Dock =
                DockStyle.Fill;

            actions.FlowDirection =
                FlowDirection.LeftToRight;

            actions.WrapContents =
                false;

            actions.AutoScroll =
                true;

            actions.Padding =
                new Padding(
                    0,
                    8,
                    0,
                    5);

            actions.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            ConfigureSafeButton(
                btnAdd,
                "Thêm",
                true);

            ConfigureSafeButton(
                btnEdit,
                "Sửa",
                false);

            ConfigureSafeButton(
                btnDelete,
                "Xóa",
                false);

            ConfigureSafeButton(
                btnSelectOnMap,
                "Chọn trên bản đồ",
                false);

            ConfigureSafeButton(
                btnZoomToRecord,
                "Thu phóng tới đối tượng",
                false);

            ConfigureSafeButton(
                btnClearSelection,
                "Bỏ chọn",
                false);

            ConfigureSafeButton(
                btnRefresh,
                "Làm mới",
                false);

            ConfigureSafeButton(
                btnClose,
                "Đóng",
                false);

            btnAdd.Click +=
                btnAdd_Click;

            btnEdit.Click +=
                btnEdit_Click;

            btnDelete.Click +=
                btnDelete_Click;

            btnSelectOnMap.Click +=
                btnSelectOnMap_Click;

            btnZoomToRecord.Click +=
                btnZoomToRecord_Click;

            btnClearSelection.Click +=
                btnClearSelection_Click;

            btnRefresh.Click +=
                btnRefresh_Click;

            btnClose.Click +=
                btnClose_Click;

            actions.Controls.Add(
                btnAdd);

            actions.Controls.Add(
                btnEdit);

            actions.Controls.Add(
                btnDelete);

            actions.Controls.Add(
                btnSelectOnMap);

            actions.Controls.Add(
                btnZoomToRecord);

            actions.Controls.Add(
                btnClearSelection);

            actions.Controls.Add(
                btnRefresh);

            actions.Controls.Add(
                btnClose);


            // -----------------------------------------------------
            // STATUS
            //
            // lblRecordCount và lblStatus trong Designer hiện tại là
            // ToolStripStatusLabel, vì vậy phải đặt chúng trong
            // StatusStrip thay vì Panel.Controls.
            // -----------------------------------------------------
            StatusStrip status =
                new StatusStrip();

            status.Dock =
                DockStyle.Fill;

            status.SizingGrip =
                false;

            status.BackColor =
                Color.FromArgb(
                    2,
                    6,
                    23);

            status.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            status.Padding =
                new Padding(
                    8,
                    0,
                    8,
                    0);

            lblRecordCount.AutoSize =
                true;

            lblRecordCount.Text =
                "0 bản ghi";

            lblRecordCount.ForeColor =
                Color.FromArgb(
                    110,
                    231,
                    183);

            lblRecordCount.Margin =
                new Padding(
                    0,
                    3,
                    18,
                    2);

            lblStatus.AutoSize =
                true;

            lblStatus.Spring =
                true;

            lblStatus.Text =
                "Sẵn sàng.";

            lblStatus.TextAlign =
                ContentAlignment.MiddleLeft;

            lblStatus.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            status.Items.Add(
                lblRecordCount);

            status.Items.Add(
                lblStatus);


            // -----------------------------------------------------
            // ADD ROOT
            // -----------------------------------------------------
            root.Controls.Add(
                header,
                0,
                0);

            root.Controls.Add(
                filters,
                0,
                1);

            root.Controls.Add(
                content,
                0,
                2);

            root.Controls.Add(
                actions,
                0,
                3);

            root.Controls.Add(
                status,
                0,
                4);

            Controls.Clear();

            Controls.Add(
                root);

            ResumeLayout(
                true);

            PerformLayout();
        }


        private System.Windows.Forms.Label CreateSafeCaptionLabel(
            string text)
        {
            System.Windows.Forms.Label label =
                new System.Windows.Forms.Label();

            label.Dock =
                DockStyle.Fill;

            label.Text =
                text;

            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.ForeColor =
                Color.FromArgb(
                    203,
                    213,
                    225);

            return label;
        }


        private void ConfigureSafeButton(
            Button button,
            string text,
            bool accent)
        {
            if (button == null)
            {
                return;
            }

            button.Text =
                text;

            button.AutoSize =
                true;

            button.Height =
                30;

            button.Padding =
                new Padding(
                    10,
                    0,
                    10,
                    0);

            button.Margin =
                new Padding(
                    0,
                    0,
                    8,
                    0);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    71,
                    85,
                    105);

            button.BackColor =
                accent
                    ? Color.FromArgb(
                        5,
                        150,
                        105)
                    : Color.FromArgb(
                        30,
                        41,
                        59);

            button.ForeColor =
                Color.White;

            button.Cursor =
                Cursors.Hand;
        }


        // =========================================================
        // LOAD
        // =========================================================
        private void AttributeTableForm_Load(
            object sender,
            EventArgs e)
        {
            btnAdd.Enabled =
                _isManagedPostGISLayer;

            btnEdit.Enabled =
                false;

            btnDelete.Enabled =
                false;

            btnSelectOnMap.Enabled =
                false;

            btnZoomToRecord.Enabled =
                false;

            lblStatus.Text =
                _isManagedPostGISLayer
                    ? "Bảng quản lý dữ liệu nghiệp vụ."
                    : "Bảng tra cứu dữ liệu nền GIS.";

            ReloadAttributeTable();
        }

        private void AttributeTableForm_Shown(
            object sender,
            EventArgs e)
        {
            try
            {
                txtSearch.Focus();
            }
            catch
            {
            }
        }

        // =========================================================
        // ĐỌC DỮ LIỆU
        // =========================================================
        private void ReloadAttributeTable()
        {
            if (_loading)
                return;

            _loading = true;

            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                lblStatus.Text =
                    "Đang đọc dữ liệu...";

                DisposeTableObjects();

                if (_isManagedPostGISLayer)
                {
                    _sourceTable =
                        _repository.GetTableData(
                            _layerKey);
                }
                else
                {
                    Shapefile sf =
                        GetLayerShapefile();

                    if (sf == null)
                    {
                        MessageBox.Show(
                            "Lớp dữ liệu không còn tồn tại trên bản đồ.",
                            "Không thể mở bảng thuộc tính",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        Close();
                        return;
                    }

                    _sourceTable =
                        AttributeTableService
                            .BuildDataTable(
                                sf);
                }

                if (_sourceTable == null)
                {
                    _sourceTable =
                        new DataTable();
                }

                _sourceTable.CaseSensitive =
                    false;

                _dataView =
                    new DataView(
                        _sourceTable);

                gridAttributes.AutoGenerateColumns =
                    true;

                gridAttributes.DataSource =
                    _dataView;

                ConfigureGridColumns();
                PopulateFilterFields();

                ApplySearchFilter();

                try
                {
                    gridAttributes.ClearSelection();
                }
                catch
                {
                }

                UpdateSelectionPanel();
                UpdateActionButtonState();
                UpdateRecordCount();

                int totalRows =
                    _sourceTable.Rows.Count;

                if (totalRows == 0)
                {
                    lblStatus.Text =
                        _isManagedPostGISLayer
                            ? "Lớp \"" +
                              _layerDisplayName +
                              "\" hiện chưa có bản ghi. Bấm Thêm để tạo dữ liệu mới."
                            : "Lớp \"" +
                              _layerDisplayName +
                              "\" hiện không có đối tượng thuộc tính.";
                }
                else
                {
                    lblStatus.Text =
                        "Đã tải " +
                        totalRows +
                        " đối tượng | " +
                        _layerDisplayName +
                        ".";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể đọc bảng thuộc tính.\n\n" +
                    ex.Message,
                    "Lỗi bảng thuộc tính",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Đọc bảng thuộc tính thất bại.";
            }
            finally
            {
                Cursor.Current =
                    oldCursor;

                _loading =
                    false;
            }
        }

        // =========================================================
        // GRID
        // =========================================================
        private void ConfigureGridColumns()
        {
            foreach (DataGridViewColumn column
                in gridAttributes.Columns)
            {
                string fieldName =
                    column.DataPropertyName;

                if (AttributeTableService
                    .IsTechnicalColumn(
                        fieldName))
                {
                    column.Visible =
                        false;

                    continue;
                }

                // Nếu đã có tên người dùng đọc được thì ưu tiên ẩn
                // mã xã/phường để bảng gọn hơn.
                if (string.Equals(
                    fieldName,
                    "ma_xa_phuong",
                    StringComparison.OrdinalIgnoreCase)
                    &&
                    _sourceTable != null
                    &&
                    _sourceTable.Columns.Contains(
                        "ten_xa_phuong"))
                {
                    column.Visible =
                        false;

                    continue;
                }

                column.HeaderText =
                    AttributeTableService
                        .GetFriendlyFieldName(
                            fieldName);

                column.SortMode =
                    DataGridViewColumnSortMode.Automatic;

                column.Width =
                    AttributeTableService
                        .GetPreferredColumnWidth(
                            fieldName);

                column.MinimumWidth =
                    60;
            }

            gridAttributes.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;

            gridAttributes.RowTemplate.Height =
                28;
        }

        private void gridAttributes_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0
                ||
                e.ColumnIndex >=
                gridAttributes.Columns.Count)
            {
                return;
            }

            DataGridViewColumn column =
                gridAttributes.Columns[
                    e.ColumnIndex];

            string fieldName =
                column.DataPropertyName;

            string formatted =
                AttributeTableService
                    .GetFriendlyValue(
                        fieldName,
                        e.Value);

            if (formatted !=
                (e.Value == null
                    ? ""
                    : e.Value.ToString()))
            {
                e.Value =
                    formatted;

                e.FormattingApplied =
                    true;
            }
        }

        // =========================================================
        // BỘ LỌC PRO
        // =========================================================
        private void PopulateFilterFields()
        {
            _updatingFilters =
                true;

            try
            {
                cboFilterField.Items.Clear();
                cboFilterValue.Items.Clear();

                cboFilterField.Items.Add(
                    new FilterFieldItem
                    {
                        FieldName = "",
                        DisplayName = "-- Không lọc --"
                    });

                string[] fields =
                    AttributeTableService
                        .GetPreferredFilterFields(
                            _sourceTable);

                foreach (string fieldName
                    in fields)
                {
                    cboFilterField.Items.Add(
                        new FilterFieldItem
                        {
                            FieldName = fieldName,
                            DisplayName =
                                AttributeTableService
                                    .GetFriendlyFieldName(
                                        fieldName)
                        });
                }

                cboFilterField.SelectedIndex =
                    0;

                cboFilterValue.Items.Add(
                    new FilterValueItem
                    {
                        RawValue = "",
                        DisplayValue = "-- Tất cả --"
                    });

                cboFilterValue.SelectedIndex =
                    0;

                cboFilterValue.Enabled =
                    false;
            }
            finally
            {
                _updatingFilters =
                    false;
            }
        }

        private void cboFilterField_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_updatingFilters)
                return;

            PopulateFilterValues();
            ApplySearchFilter();
        }

        private void PopulateFilterValues()
        {
            _updatingFilters =
                true;

            try
            {
                cboFilterValue.Items.Clear();

                cboFilterValue.Items.Add(
                    new FilterValueItem
                    {
                        RawValue = "",
                        DisplayValue = "-- Tất cả --"
                    });

                FilterFieldItem fieldItem =
                    cboFilterField.SelectedItem
                    as FilterFieldItem;

                if (fieldItem == null
                    ||
                    string.IsNullOrWhiteSpace(
                        fieldItem.FieldName)
                    ||
                    _sourceTable == null
                    ||
                    !_sourceTable.Columns.Contains(
                        fieldItem.FieldName))
                {
                    cboFilterValue.SelectedIndex =
                        0;

                    cboFilterValue.Enabled =
                        false;

                    return;
                }

                SortedDictionary<string, string> unique =
                    new SortedDictionary<string, string>(
                        StringComparer.CurrentCultureIgnoreCase);

                foreach (DataRow row
                    in _sourceTable.Rows)
                {
                    object value =
                        row[fieldItem.FieldName];

                    if (value == null
                        ||
                        value == DBNull.Value)
                    {
                        continue;
                    }

                    string raw =
                        value.ToString();

                    if (string.IsNullOrWhiteSpace(
                        raw))
                    {
                        continue;
                    }

                    string display =
                        AttributeTableService
                            .GetFriendlyValue(
                                fieldItem.FieldName,
                                value);

                    if (!unique.ContainsKey(
                        raw))
                    {
                        unique.Add(
                            raw,
                            display);
                    }
                }

                foreach (KeyValuePair<string, string> item
                    in unique)
                {
                    cboFilterValue.Items.Add(
                        new FilterValueItem
                        {
                            RawValue = item.Key,
                            DisplayValue = item.Value
                        });
                }

                cboFilterValue.SelectedIndex =
                    0;

                cboFilterValue.Enabled =
                    cboFilterValue.Items.Count > 1;
            }
            finally
            {
                _updatingFilters =
                    false;
            }
        }

        private void cboFilterValue_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_updatingFilters)
                return;

            ApplySearchFilter();
        }

        private void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            if (_loading)
                return;

            ApplySearchFilter();
        }

        private void btnClearSearch_Click(
            object sender,
            EventArgs e)
        {
            txtSearch.Clear();
            txtSearch.Focus();
        }

        private void btnClearFilters_Click(
            object sender,
            EventArgs e)
        {
            _updatingFilters =
                true;

            try
            {
                if (cboFilterField.Items.Count > 0)
                {
                    cboFilterField.SelectedIndex =
                        0;
                }

                cboFilterValue.Items.Clear();

                cboFilterValue.Items.Add(
                    new FilterValueItem
                    {
                        RawValue = "",
                        DisplayValue = "-- Tất cả --"
                    });

                cboFilterValue.SelectedIndex =
                    0;

                cboFilterValue.Enabled =
                    false;
            }
            finally
            {
                _updatingFilters =
                    false;
            }

            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (_dataView == null
                ||
                _sourceTable == null)
            {
                return;
            }

            string filterField =
                "";

            string filterValue =
                "";

            FilterFieldItem fieldItem =
                cboFilterField.SelectedItem
                as FilterFieldItem;

            if (fieldItem != null)
            {
                filterField =
                    fieldItem.FieldName ?? "";
            }

            FilterValueItem valueItem =
                cboFilterValue.SelectedItem
                as FilterValueItem;

            if (valueItem != null)
            {
                filterValue =
                    valueItem.RawValue ?? "";
            }

            try
            {
                _dataView.RowFilter =
                    AttributeTableService
                        .BuildCombinedFilterExpression(
                            _sourceTable,
                            txtSearch.Text,
                            filterField,
                            filterValue);
            }
            catch
            {
                _dataView.RowFilter =
                    "";
            }

            try
            {
                gridAttributes.ClearSelection();
            }
            catch
            {
            }

            UpdateRecordCount();
            UpdateSelectionPanel();
            UpdateActionButtonState();
        }

        // =========================================================
        // GRID -> MAP / PANEL CHI TIẾT
        // =========================================================
        private void gridAttributes_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            UpdateSelectionPanel();
            UpdateActionButtonState();

            // Đồng bộ chọn trên map nhưng không tự zoom.
            SelectCurrentRecord(
                false,
                false);
        }

        private void gridAttributes_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            SelectCurrentRecord(
                true,
                true);
        }

        private void gridAttributes_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (_loading)
                return;

            UpdateSelectionPanel();
            UpdateActionButtonState();
        }

        private void btnSelectOnMap_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentRecord(
                false,
                true);
        }

        private void btnZoomToRecord_Click(
            object sender,
            EventArgs e)
        {
            SelectCurrentRecord(
                true,
                true);
        }

        private bool SelectCurrentRecord(
            bool zoomToRecord,
            bool showMessageWhenMissing)
        {
            DataRowView rowView =
                GetCurrentSelectedRowView();

            if (rowView == null)
            {
                if (showMessageWhenMissing)
                {
                    MessageBox.Show(
                        "Hãy chọn một dòng trong bảng.",
                        "Chưa chọn đối tượng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return false;
            }

            bool success;

            if (_isManagedPostGISLayer)
            {
                int databaseId =
                    AttributeTableService
                        .GetDatabaseId(
                            rowView);

                if (databaseId < 0)
                {
                    if (showMessageWhenMissing)
                    {
                        MessageBox.Show(
                            "Không xác định được ID của đối tượng.",
                            "Lỗi dữ liệu",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }

                    return false;
                }

                success =
                    AttributeTableService
                        .SelectShapeByDatabaseId(
                            _map,
                            _layerHandle,
                            databaseId,
                            zoomToRecord);
            }
            else
            {
                int shapeIndex =
                    AttributeTableService
                        .GetShapeIndex(
                            rowView);

                success =
                    AttributeTableService
                        .SelectShape(
                            _map,
                            _layerHandle,
                            shapeIndex,
                            zoomToRecord);
            }

            if (!success)
            {
                if (showMessageWhenMissing)
                {
                    MessageBox.Show(
                        _isManagedPostGISLayer
                            ? "Bản ghi chưa có geometry trên bản đồ hoặc lớp chưa được làm mới."
                            : "Không thể xác định geometry tương ứng trên bản đồ.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return false;
            }

            lblStatus.Text =
                zoomToRecord
                    ? "Đã chọn và thu phóng tới đối tượng."
                    : "Đã đồng bộ đối tượng đang chọn với bản đồ.";

            return true;
        }

        private void UpdateSelectionPanel()
        {
            DataRowView rowView =
                GetCurrentSelectedRowView();

            if (rowView == null
                ||
                rowView.Row == null)
            {
                lblSelectedTitle.Text =
                    "CHƯA CHỌN ĐỐI TƯỢNG";

                txtSelectedDetails.Text =
                    "Chọn một dòng trong bảng để xem nhanh thuộc tính.\r\n\r\n" +
                    "Double-click một dòng để thu phóng tới đối tượng trên bản đồ.";

                return;
            }

            string objectName =
                GetCurrentObjectDisplayName();

            lblSelectedTitle.Text =
                string.IsNullOrWhiteSpace(
                    objectName)
                    ? "ĐỐI TƯỢNG ĐANG CHỌN"
                    : objectName;

            StringBuilder sb =
                new StringBuilder();

            foreach (DataColumn column
                in rowView.Row.Table.Columns)
            {
                if (AttributeTableService
                    .IsTechnicalColumn(
                        column.ColumnName))
                {
                    continue;
                }

                object value =
                    rowView[column.ColumnName];

                string display =
                    AttributeTableService
                        .GetFriendlyValue(
                            column.ColumnName,
                            value);

                if (string.IsNullOrWhiteSpace(
                    display))
                {
                    continue;
                }

                sb.Append(
                    AttributeTableService
                        .GetFriendlyFieldName(
                            column.ColumnName));

                sb.Append(
                    ": ");

                sb.AppendLine(
                    display);
            }

            if (_layerKey.Equals(
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine();
                sb.AppendLine(
                    "Ghi chú GIS: System.Drawing.Point trên bản đồ là điểm đại diện hành chính theo xã/phường.");
            }

            txtSelectedDetails.Text =
                sb.Length > 0
                    ? sb.ToString()
                    : "Không có thuộc tính hiển thị.";
        }

        // =========================================================
        // CRUD
        // =========================================================
        private void btnAdd_Click(
            object sender,
            EventArgs e)
        {
            if (!_isManagedPostGISLayer)
                return;

            RequestedSpatialCreateTable =
                _layerKey;

            lblStatus.Text =
                "Chuyển sang quy trình tạo mới dữ liệu không gian.";

            DialogResult =
                DialogResult.Retry;

            Close();
        }

        private void btnEdit_Click(
            object sender,
            EventArgs e)
        {
            if (!_isManagedPostGISLayer)
                return;

            int id =
                GetCurrentDatabaseId();

            if (id < 0)
            {
                MessageBox.Show(
                    "Hãy chọn một đối tượng cần sửa.",
                    "Chưa chọn dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (EditObjectForm form =
                new EditObjectForm(
                    _repository,
                    _layerKey,
                    id))
            {
                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    RefreshLayerAfterDatabaseChange();
                    ReloadAttributeTable();

                    lblStatus.Text =
                        "Đã cập nhật " +
                        _layerDisplayName +
                        " | ID = " +
                        id +
                        ".";
                }
            }
        }

        private void btnDelete_Click(
            object sender,
            EventArgs e)
        {
            if (!_isManagedPostGISLayer)
                return;

            int id =
                GetCurrentDatabaseId();

            if (id < 0)
            {
                MessageBox.Show(
                    "Hãy chọn một đối tượng cần xóa.",
                    "Chưa chọn dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string objectName =
                GetCurrentObjectDisplayName();

            string message =
                "Anh có chắc muốn xóa ";

            if (!string.IsNullOrWhiteSpace(
                objectName))
            {
                message +=
                    "\"" +
                    objectName +
                    "\"";
            }
            else
            {
                message +=
                    "đối tượng ID = " +
                    id;
            }

            message +=
                " khỏi cơ sở dữ liệu?\n\n" +
                "Thuộc tính và geometry GIS liên kết của đối tượng cũng sẽ bị xóa.";

            DialogResult confirm =
                MessageBox.Show(
                    message,
                    "Xác nhận xóa đối tượng",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (confirm !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                _repository.DeleteRecord(
                    _layerKey,
                    id);

                RefreshLayerAfterDatabaseChange();
                ReloadAttributeTable();

                lblStatus.Text =
                    "Đã xóa đối tượng khỏi cơ sở dữ liệu.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể xóa đối tượng.\n\n" +
                    ex.Message,
                    "Lỗi xóa dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // BỎ CHỌN / LÀM MỚI / ĐÓNG
        // =========================================================
        private void btnClearSelection_Click(
            object sender,
            EventArgs e)
        {
            AttributeTableService.ClearSelection(
                _map,
                _layerHandle);

            try
            {
                gridAttributes.ClearSelection();
            }
            catch
            {
            }

            UpdateSelectionPanel();
            UpdateActionButtonState();

            lblStatus.Text =
                "Đã bỏ chọn đối tượng.";
        }

        private void btnRefresh_Click(
            object sender,
            EventArgs e)
        {
            ReloadAttributeTable();

            lblStatus.Text =
                "Đã làm mới dữ liệu.";
        }

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        // =========================================================
        // PHÍM TẮT
        // =========================================================
        private void AttributeTableForm_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Control &&
                e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.F5)
            {
                ReloadAttributeTable();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.Escape)
            {
                btnClearSelection_Click(
                    sender,
                    EventArgs.Empty);

                e.SuppressKeyPress =
                    true;
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private DataRowView GetCurrentSelectedRowView()
        {
            if (gridAttributes.SelectedRows.Count <= 0)
                return null;

            DataGridViewRow row =
                gridAttributes.SelectedRows[0];

            return
                row.DataBoundItem
                as DataRowView;
        }

        private int GetCurrentDatabaseId()
        {
            return
                AttributeTableService
                    .GetDatabaseId(
                        GetCurrentSelectedRowView());
        }

        private string GetCurrentObjectDisplayName()
        {
            DataRowView rowView =
                GetCurrentSelectedRowView();

            if (rowView == null
                ||
                rowView.Row == null)
            {
                return "";
            }

            string[] candidateColumns =
            {
                "ten_don_vi",
                "ten_tuyen",
                "ten_khu_vuc",
                "ten_xa_phuong",
                "ten_hanh_chinh",
                "ma_don_vi",
                "ma_tuyen",
                "ma_khu_vuc"
            };

            foreach (string columnName
                in candidateColumns)
            {
                if (!rowView.Row.Table.Columns.Contains(
                    columnName))
                {
                    continue;
                }

                object value =
                    rowView[columnName];

                if (value != null
                    &&
                    value != DBNull.Value
                    &&
                    !string.IsNullOrWhiteSpace(
                        value.ToString()))
                {
                    return value.ToString();
                }
            }

            return "";
        }

        private void RefreshLayerAfterDatabaseChange()
        {
            if (_refreshLayerCallback == null)
                return;

            try
            {
                _refreshLayerCallback(
                    _layerKey);
            }
            catch
            {
                // Bảng vẫn dùng được nếu refresh map gặp lỗi.
            }
        }

        private void UpdateActionButtonState()
        {
            bool hasSelection =
                GetCurrentSelectedRowView() !=
                null;

            btnAdd.Enabled =
                _isManagedPostGISLayer;

            btnEdit.Enabled =
                _isManagedPostGISLayer &&
                hasSelection;

            btnDelete.Enabled =
                _isManagedPostGISLayer &&
                hasSelection;

            btnSelectOnMap.Enabled =
                hasSelection;

            btnZoomToRecord.Enabled =
                hasSelection;

            btnClearSelection.Enabled =
                hasSelection;
        }

        private void UpdateRecordCount()
        {
            int total =
                _sourceTable != null
                    ? _sourceTable.Rows.Count
                    : 0;

            int visible =
                _dataView != null
                    ? _dataView.Count
                    : 0;

            lblRecordCount.Text =
                "Hiển thị: " +
                visible +
                " / " +
                total;

            lblHeaderCount.Text =
                total +
                (_isManagedPostGISLayer
                    ? " bản ghi"
                    : " đối tượng");
        }

        private Shapefile GetLayerShapefile()
        {
            if (_map == null
                ||
                _layerHandle < 0)
            {
                return null;
            }

            try
            {
                return
                    _map.get_Shapefile(
                        _layerHandle);
            }
            catch
            {
                return null;
            }
        }

        private void DisposeTableObjects()
        {
            gridAttributes.DataSource =
                null;

            if (_dataView != null)
            {
                _dataView.Dispose();
                _dataView = null;
            }

            if (_sourceTable != null)
            {
                _sourceTable.Dispose();
                _sourceTable = null;
            }
        }

        private void AttributeTableForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            DisposeTableObjects();
        }
    }
}