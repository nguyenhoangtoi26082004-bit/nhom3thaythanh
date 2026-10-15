using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class EditObjectForm : Form
    {
        private sealed class StatusItem
        {
            public string Code { get; private set; }
            public string Text { get; private set; }

            public StatusItem(
                string code,
                string text)
            {
                Code = code;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private readonly PostGISRepository _repository;
        private readonly PostGISTableDefinition _definition;

        private readonly bool _isNewRecord;
        private readonly int _recordId;
        private readonly bool _geometryWillBeCaptured;

        private readonly Dictionary<string, Control>
            _editors =
                new Dictionary<string, Control>(
                    StringComparer.OrdinalIgnoreCase);

        public bool DataChanged { get; private set; }
        public int SavedRecordId { get; private set; }

        // Thuộc tính tạm dùng cho luồng:
        // Nhập thông tin -> Vẽ geometry -> mới lưu CSDL.
        public Dictionary<string, object>
            PendingValues
        { get; private set; }

        // =========================================================
        // CONSTRUCTOR
        //
        // geometryWillBeCaptured = true khi được gọi từ Editing.cs.
        // =========================================================
        public EditObjectForm(
            PostGISRepository repository,
            string tableName,
            int? recordId,
            bool geometryWillBeCaptured = false)
        {
            InitializeComponent();

            _repository =
                repository;

            _definition =
                _repository.GetDefinition(
                    tableName);

            _isNewRecord =
                !recordId.HasValue;

            _recordId =
                recordId.HasValue
                    ? recordId.Value
                    : -1;

            _geometryWillBeCaptured =
                geometryWillBeCaptured;

            DataChanged =
                false;

            SavedRecordId =
                -1;

            PendingValues =
                new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase);

            Text =
                (_isNewRecord
                    ? "Thêm "
                    : "Sửa ") +
                _definition.DisplayName;

            lblTitle.Text =
                (_isNewRecord
                    ? "THÊM "
                    : "SỬA ") +
                _definition.DisplayName.ToUpperInvariant();

            if (_isNewRecord &&
                string.Equals(
                    _definition.TableName,
                    "don_vi_quan_doi",
                    StringComparison.OrdinalIgnoreCase))
            {
                lblSubtitle.Text =
                    "Nhập hồ sơ đơn vị và chọn đúng xã/phường mới của Hà Nội. " +
                    "Point đại diện hành chính sẽ được tạo tự động theo xã/phường.";
            }
            else if (_isNewRecord &&
                     _geometryWillBeCaptured)
            {
                lblSubtitle.Text =
                    "Bước 1/2: nhập thông tin thuộc tính. " +
                    "Sau đó chọn “Vẽ và lưu” để sang bước 2 và số hóa hình học.";
            }
            else
            {
                lblSubtitle.Text =
                    _isNewRecord
                        ? "Nhập thông tin thuộc tính cho đối tượng mới."
                        : "Chỉnh sửa thông tin thuộc tính của đối tượng.";
            }

            Load +=
                EditObjectForm_Load;

            Shown +=
                EditObjectForm_Shown;
        }

        // =========================================================
        // LOAD
        // =========================================================
        private void EditObjectForm_Load(
            object sender,
            EventArgs e)
        {
            BuildEditors();
            ConfigureCommandArea();

            if (!_isNewRecord)
            {
                LoadExistingValues();
            }
            else
            {
                SetDefaultValues();
            }
        }

        // =========================================================
        // SHOWN
        //
        // Luôn mở form ở đầu danh sách trường, tránh tình trạng
        // WinForms tự cuộn xuống control đang focus ở phía dưới.
        // =========================================================
        private void EditObjectForm_Shown(
            object sender,
            EventArgs e)
        {
            try
            {
                panelFields.AutoScrollPosition =
                    new Point(
                        0,
                        0);

                foreach (PostGISFieldDefinition field
                    in _definition.EditableFields)
                {
                    Control editor;

                    if (_editors.TryGetValue(
                        field.ColumnName,
                        out editor)
                        &&
                        editor != null
                        &&
                        editor.CanFocus)
                    {
                        editor.Focus();
                        break;
                    }
                }

                panelFields.AutoScrollPosition =
                    new Point(
                        0,
                        0);
            }
            catch
            {
            }
        }

        // =========================================================
        // CẤU HÌNH KHU VỰC NÚT LỆNH
        // =========================================================
        private void ConfigureCommandArea()
        {
            if (_isNewRecord &&
                _geometryWillBeCaptured)
            {
                btnSave.Text =
                    "Vẽ và lưu →";

                if (lblStepBadge != null)
                {
                    lblStepBadge.Text =
                        "BƯỚC 1/2";

                    lblStepBadge.Visible =
                        true;
                }

                if (lblCommandHint != null)
                {
                    lblCommandHint.Text =
                        "Nhập đủ thông tin → bấm “Vẽ và lưu” → vẽ trên bản đồ. " +
                        "Chỉ khi hình học hợp lệ và hoàn tất thì dữ liệu mới được lưu.";
                }

                this.AcceptButton =
                    btnSave;

                this.CancelButton =
                    btnCancel;

                return;
            }

            if (_isNewRecord)
            {
                btnSave.Text =
                    "Lưu hồ sơ";

                if (lblStepBadge != null)
                {
                    lblStepBadge.Text =
                        "HỒ SƠ";

                    lblStepBadge.Visible =
                        true;
                }

                if (lblCommandHint != null)
                {
                    lblCommandHint.Text =
                        "Kiểm tra các trường bắt buộc rồi bấm “Lưu hồ sơ”. " +
                        "Nút “Hủy” sẽ đóng form mà không ghi dữ liệu.";
                }

                this.AcceptButton =
                    btnSave;

                this.CancelButton =
                    btnCancel;

                return;
            }

            btnSave.Text =
                "Lưu thay đổi";

            if (lblStepBadge != null)
            {
                lblStepBadge.Text =
                    "CHỈNH SỬA";

                lblStepBadge.Visible =
                    true;
            }

            if (lblCommandHint != null)
            {
                lblCommandHint.Text =
                    "Bấm “Lưu thay đổi” để cập nhật thuộc tính. " +
                    "Bấm “Hủy” để bỏ các thay đổi chưa lưu.";
            }

            this.AcceptButton =
                btnSave;

            this.CancelButton =
                btnCancel;
        }

        // =========================================================
        // TẠO CONTROL ĐỘNG
        // =========================================================
        private void BuildEditors()
        {
            panelFields.SuspendLayout();

            try
            {
                panelFields.Controls.Clear();
                _editors.Clear();
                panelFields.AutoScroll = true;

                int top =
                    14;

                foreach (PostGISFieldDefinition field
                    in _definition.EditableFields)
                {
                    Label label =
                        new Label();

                    label.AutoSize =
                        true;

                    label.Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            field.Required
                                ? FontStyle.Bold
                                : FontStyle.Regular);

                    label.Left =
                        16;

                    label.Top =
                        top + 5;

                    label.Text =
                        field.Label +
                        (field.Required
                            ? " *"
                            : "");

                    panelFields.Controls.Add(
                        label);

                    Control editor =
                        CreateEditor(
                            field);

                    editor.Left =
                        180;

                    editor.Top =
                        top;

                    editor.Width =
                        Math.Max(
                            250,
                            panelFields.ClientSize.Width - 205);

                    editor.Anchor =
                        AnchorStyles.Top
                        | AnchorStyles.Left
                        | AnchorStyles.Right;

                    panelFields.Controls.Add(
                        editor);

                    // Sau khi control đã có Parent/BindingContext của Form,
                    // bảo đảm ComboBox DataSource có một lựa chọn hợp lệ.
                    ComboBox boundCombo =
                        editor as ComboBox;

                    if (boundCombo != null &&
                        boundCombo.DataSource != null &&
                        boundCombo.Items.Count > 0 &&
                        boundCombo.SelectedIndex < 0)
                    {
                        boundCombo.SelectedIndex =
                            0;
                    }

                    _editors[field.ColumnName] =
                        editor;

                    top +=
                        editor.Height + 13;
                }
            }
            finally
            {
                panelFields.ResumeLayout();
            }
        }

        // =========================================================
        // TẠO EDITOR THEO FIELD
        // =========================================================
        private Control CreateEditor(
            PostGISFieldDefinition field)
        {
            string name =
                field.ColumnName;

            // -----------------------------------------------------
            // CẤP ĐƠN VỊ
            // -----------------------------------------------------
            if (name.Equals(
                "cap_don_vi_id",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                BindLookupCombo(
                    combo,
                    _repository.GetCapDonViData(),
                    "id",
                    "ten_cap",
                    "-- Chưa xác định --");

                return combo;
            }

            // -----------------------------------------------------
            // LOẠI ĐƠN VỊ
            // -----------------------------------------------------
            if (name.Equals(
                "loai_don_vi_id",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                BindLookupCombo(
                    combo,
                    _repository.GetLoaiDonViData(),
                    "id",
                    "ten_loai",
                    "-- Chưa xác định --");

                return combo;
            }

            // -----------------------------------------------------
            // ĐƠN VỊ CẤP TRÊN
            // -----------------------------------------------------
            if (name.Equals(
                "don_vi_cap_tren_id",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                DataTable source =
                    _repository.GetDonViCapTrenData(
                        _isNewRecord
                            ? (int?)null
                            : _recordId);

                AddUnitDisplayColumn(
                    source);

                BindLookupCombo(
                    combo,
                    source,
                    "id",
                    "__display",
                    "-- Không có --");

                return combo;
            }

            // -----------------------------------------------------
            // XÃ / PHƯỜNG MỚI CỦA HÀ NỘI
            // -----------------------------------------------------
            if (name.Equals(
                "xa_phuong_id",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                BindLookupCombo(
                    combo,
                    _repository.GetXaPhuongData(),
                    "id",
                    "ten_hien_thi",
                    "-- Chọn xã/phường --");

                return combo;
            }

            // -----------------------------------------------------
            // ĐƠN VỊ SỞ HỮU TUYẾN / KHU VỰC
            // -----------------------------------------------------
            if (name.Equals(
                "don_vi_id",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                DataTable source =
                    _repository.GetDonViData();

                AddUnitDisplayColumn(
                    source);

                BindLookupCombo(
                    combo,
                    source,
                    "id",
                    "__display",
                    "-- Chọn đơn vị --");

                return combo;
            }

            // -----------------------------------------------------
            // LOẠI TUYẾN
            // -----------------------------------------------------
            if (name.Equals(
                "loai_tuyen",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                BindLookupCombo(
                    combo,
                    _repository.GetLoaiTuyenData(),
                    "ma_loai",
                    "ten_loai",
                    "-- Chọn loại tuyến --");

                return combo;
            }

            // -----------------------------------------------------
            // LOẠI KHU VỰC
            // -----------------------------------------------------
            if (name.Equals(
                "loai_khu_vuc",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    CreateLookupCombo();

                BindLookupCombo(
                    combo,
                    _repository.GetLoaiKhuVucData(),
                    "ma_loai",
                    "ten_loai",
                    "-- Chọn loại khu vực --");

                return combo;
            }

            // -----------------------------------------------------
            // TRẠNG THÁI ĐƠN VỊ
            // -----------------------------------------------------
            if (name.Equals(
                "trang_thai_quan_ly",
                StringComparison.OrdinalIgnoreCase))
            {
                ComboBox combo =
                    new ComboBox();

                combo.DropDownStyle =
                    ComboBoxStyle.DropDownList;

                combo.Items.Add(
                    new StatusItem(
                        "dang_quan_ly",
                        "Đang quản lý"));

                combo.Items.Add(
                    new StatusItem(
                        "tam_ngung",
                        "Tạm ngừng"));

                combo.Items.Add(
                    new StatusItem(
                        "khong_con_quan_ly",
                        "Không còn quản lý"));

                combo.SelectedIndex =
                    0;

                return combo;
            }

            // -----------------------------------------------------
            // CÁC TRƯỜNG VĂN BẢN DÀI
            // -----------------------------------------------------
            if (name.Equals(
                "dia_chi_hien_hanh",
                StringComparison.OrdinalIgnoreCase)
                ||
                name.Equals(
                    "thong_tin_lien_he",
                    StringComparison.OrdinalIgnoreCase)
                ||
                name.Equals(
                    "nguon_tham_khao",
                    StringComparison.OrdinalIgnoreCase)
                ||
                name.Equals(
                    "mo_ta",
                    StringComparison.OrdinalIgnoreCase)
                ||
                name.Equals(
                    "ghi_chu",
                    StringComparison.OrdinalIgnoreCase))
            {
                TextBox box =
                    new TextBox();

                box.Multiline =
                    true;

                box.ScrollBars =
                    ScrollBars.Vertical;

                box.Height =
                    name.Equals(
                        "mo_ta",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    name.Equals(
                        "ghi_chu",
                        StringComparison.OrdinalIgnoreCase)
                        ? 76
                        : 58;

                return box;
            }

            // -----------------------------------------------------
            // KÝ HIỆU - TỰ ĐỘNG VIẾT HOA
            // -----------------------------------------------------
            if (name.Equals(
                "ky_hieu_bien_so",
                StringComparison.OrdinalIgnoreCase))
            {
                TextBox box =
                    new TextBox();

                box.CharacterCasing =
                    CharacterCasing.Upper;

                box.MaxLength =
                    50;

                return box;
            }

            // -----------------------------------------------------
            // SỐ THUẦN (không phải khóa ngoại)
            // -----------------------------------------------------
            if (field.DataType ==
                typeof(int))
            {
                NumericUpDown number =
                    new NumericUpDown();

                number.Minimum =
                    0;

                number.Maximum =
                    1000000000;

                number.DecimalPlaces =
                    0;

                number.ThousandsSeparator =
                    true;

                return number;
            }

            if (field.DataType ==
                typeof(double))
            {
                NumericUpDown number =
                    new NumericUpDown();

                number.Minimum =
                    0;

                number.Maximum =
                    1000000000;

                number.DecimalPlaces =
                    2;

                number.ThousandsSeparator =
                    true;

                return number;
            }

            return new TextBox();
        }

        // =========================================================
        // COMBO HELPERS
        // =========================================================
        private ComboBox CreateLookupCombo()
        {
            ComboBox combo =
                new ComboBox();

            combo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            return combo;
        }

        private void BindLookupCombo(
            ComboBox combo,
            DataTable source,
            string valueMember,
            string displayMember,
            string emptyText)
        {
            if (source == null)
            {
                source =
                    new DataTable();
            }

            if (!source.Columns.Contains(
                valueMember))
            {
                source.Columns.Add(
                    valueMember,
                    typeof(string));
            }

            if (!source.Columns.Contains(
                displayMember))
            {
                source.Columns.Add(
                    displayMember,
                    typeof(string));
            }

            DataRow emptyRow =
                source.NewRow();

            emptyRow[valueMember] =
                DBNull.Value;

            emptyRow[displayMember] =
                emptyText;

            source.Rows.InsertAt(
                emptyRow,
                0);

            // Thiết lập BindingContext riêng vì ComboBox được tạo động
            // trước khi được gắn vào panelFields. Nếu ép SelectedIndex = 0
            // quá sớm, Items có thể vẫn chưa được tạo và gây
            // ArgumentOutOfRangeException.
            combo.BindingContext =
                new BindingContext();

            combo.DisplayMember =
                displayMember;

            combo.ValueMember =
                valueMember;

            combo.DataSource =
                source;

            // Chỉ chọn dòng đầu khi binding đã tạo Items.
            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex =
                    0;
            }
        }

        private void AddUnitDisplayColumn(
            DataTable source)
        {
            if (source == null)
                return;

            if (!source.Columns.Contains(
                "__display"))
            {
                source.Columns.Add(
                    "__display",
                    typeof(string));
            }

            foreach (DataRow row
                in source.Rows)
            {
                string code =
                    row.Table.Columns.Contains(
                        "ma_don_vi")
                    &&
                    row["ma_don_vi"] != DBNull.Value
                        ? row["ma_don_vi"].ToString()
                        : "";

                string name =
                    row.Table.Columns.Contains(
                        "ten_don_vi")
                    &&
                    row["ten_don_vi"] != DBNull.Value
                        ? row["ten_don_vi"].ToString()
                        : "";

                row["__display"] =
                    code.Length > 0
                        ? code + " - " + name
                        : name;
            }
        }

        // =========================================================
        // DEFAULT
        // =========================================================
        private void SetDefaultValues()
        {
            Control editor;

            // Trạng thái quản lý mặc định.
            if (_editors.TryGetValue(
                "trang_thai_quan_ly",
                out editor))
            {
                ComboBox combo =
                    editor as ComboBox;

                if (combo != null &&
                    combo.Items.Count > 0)
                {
                    combo.SelectedIndex =
                        0;
                }
            }

            // Các giá trị mặc định cho hồ sơ đơn vị Hà Nội.
            SetTextEditorDefault(
                "dia_chi_hien_hanh",
                "CHƯA XÁC MINH");

            SetTextEditorDefault(
                "thong_tin_lien_he",
                "Nguồn công khai");

            SetTextEditorDefault(
                "nguon_tham_khao",
                "Nguồn công khai");

            SetTextEditorDefault(
                "muc_do_xac_minh",
                "tham_chieu_hanh_chinh");

            SetTextEditorDefault(
                "mo_ta",
                "Không có");

            SetTextEditorDefault(
                "ghi_chu",
                "Không có");
        }

        private void SetTextEditorDefault(
            string fieldName,
            string value)
        {
            Control editor;

            if (!_editors.TryGetValue(
                fieldName,
                out editor))
            {
                return;
            }

            TextBox box =
                editor as TextBox;

            if (box == null ||
                !string.IsNullOrWhiteSpace(
                    box.Text))
            {
                return;
            }

            box.Text =
                value ?? "";
        }

        // =========================================================
        // LOAD RECORD
        // =========================================================
        private void LoadExistingValues()
        {
            Cursor oldCursor =
                Cursor.Current;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                Dictionary<string, object> values =
                    _repository.GetRecord(
                        _definition.TableName,
                        _recordId);

                if (values == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy bản ghi ID = " +
                        _recordId +
                        ".",
                        "Không tìm thấy dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    Close();
                    return;
                }

                foreach (PostGISFieldDefinition field
                    in _definition.EditableFields)
                {
                    object value;

                    if (!values.TryGetValue(
                        field.ColumnName,
                        out value))
                    {
                        continue;
                    }

                    SetEditorValue(
                        field,
                        value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể đọc dữ liệu cần sửa.\n\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Close();
            }
            finally
            {
                Cursor.Current =
                    oldCursor;
            }
        }

        // =========================================================
        // SAVE
        // =========================================================
        private void btnSave_Click(
            object sender,
            EventArgs e)
        {
            Cursor oldCursor =
                Cursor.Current;

            btnSave.Enabled =
                false;

            try
            {
                Dictionary<string, object> values =
                    ReadEditorValues();

                // -------------------------------------------------
                // TUYẾN / KHU VỰC MỚI:
                //
                // KHÔNG ghi xuống CSDL ở bước này.
                // Chỉ giữ thuộc tính tạm để Editing.cs chuyển sang
                // bước vẽ. Chỉ khi geometry hợp lệ và người dùng
                // click phải hoàn tất thì mới INSERT + lưu geometry.
                // -------------------------------------------------
                if (_isNewRecord &&
                    _geometryWillBeCaptured)
                {
                    PendingValues =
                        new Dictionary<string, object>(
                            values,
                            StringComparer.OrdinalIgnoreCase);

                    SavedRecordId =
                        -1;

                    DataChanged =
                        false;

                    DialogResult =
                        DialogResult.OK;

                    Close();
                    return;
                }

                Cursor.Current =
                    Cursors.WaitCursor;

                // -------------------------------------------------
                // ĐƠN VỊ MỚI:
                // vẫn lưu hồ sơ ngay. Point đại diện hành chính sẽ
                // được Editing.cs tạo tự động từ xã/phường.
                // -------------------------------------------------
                if (_isNewRecord)
                {
                    SavedRecordId =
                        _repository.InsertRecord(
                            _definition.TableName,
                            values);

                    DataChanged =
                        true;
                }
                else
                {
                    _repository.UpdateRecord(
                        _definition.TableName,
                        _recordId,
                        values);

                    SavedRecordId =
                        _recordId;

                    DataChanged =
                        true;

                    MessageBox.Show(
                        "Đã cập nhật dữ liệu thành công.",
                        "Đã lưu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tiếp tục.\n\n" +
                    ex.Message,
                    "Kiểm tra dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current =
                    oldCursor;

                btnSave.Enabled =
                    true;
            }
        }

        private void btnCancel_Click(
            object sender,
            EventArgs e)
        {
            DialogResult =
                DialogResult.Cancel;

            Close();
        }

        // =========================================================
        // READ FORM
        // =========================================================
        private Dictionary<string, object>
            ReadEditorValues()
        {
            Dictionary<string, object> values =
                new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (PostGISFieldDefinition field
                in _definition.EditableFields)
            {
                Control editor =
                    _editors[field.ColumnName];

                object value =
                    GetEditorValue(
                        field,
                        editor);

                if (field.Required)
                {
                    if (value == null ||
                        value == DBNull.Value ||
                        string.IsNullOrWhiteSpace(
                            value.ToString()))
                    {
                        editor.Focus();

                        throw new InvalidOperationException(
                            "Vui lòng nhập/chọn \"" +
                            field.Label +
                            "\".");
                    }
                }

                values[field.ColumnName] =
                    value;
            }

            return values;
        }

        // =========================================================
        // GET VALUE
        // =========================================================
        private object GetEditorValue(
            PostGISFieldDefinition field,
            Control editor)
        {
            ComboBox combo =
                editor as ComboBox;

            if (combo != null)
            {
                if (field.ColumnName.Equals(
                    "trang_thai_quan_ly",
                    StringComparison.OrdinalIgnoreCase))
                {
                    StatusItem status =
                        combo.SelectedItem
                        as StatusItem;

                    return
                        status != null
                            ? (object)status.Code
                            : null;
                }

                object selectedValue =
                    combo.SelectedValue;

                if (selectedValue == null ||
                    selectedValue == DBNull.Value)
                {
                    return null;
                }

                // Các khóa ngoại trong schema ql_don_vi là BIGINT.
                if (field.ColumnName.EndsWith(
                    "_id",
                    StringComparison.OrdinalIgnoreCase))
                {
                    long id;

                    if (long.TryParse(
                        selectedValue.ToString(),
                        out id))
                    {
                        return id;
                    }

                    return null;
                }

                // loai_tuyen / loai_khu_vuc.
                return
                    selectedValue.ToString();
            }

            NumericUpDown numeric =
                editor as NumericUpDown;

            if (numeric != null)
            {
                if (field.DataType ==
                    typeof(int))
                {
                    return
                        Convert.ToInt32(
                            numeric.Value);
                }

                if (field.DataType ==
                    typeof(double))
                {
                    return
                        Convert.ToDouble(
                            numeric.Value);
                }
            }

            TextBox textBox =
                editor as TextBox;

            if (textBox != null)
            {
                string text =
                    textBox.Text.Trim();

                return
                    text.Length == 0
                        ? null
                        : (object)text;
            }

            return null;
        }

        // =========================================================
        // SET VALUE
        // =========================================================
        private void SetEditorValue(
            PostGISFieldDefinition field,
            object value)
        {
            Control editor;

            if (!_editors.TryGetValue(
                field.ColumnName,
                out editor))
            {
                return;
            }

            if (value == null ||
                value == DBNull.Value)
            {
                return;
            }

            ComboBox combo =
                editor as ComboBox;

            if (combo != null)
            {
                if (field.ColumnName.Equals(
                    "trang_thai_quan_ly",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string code =
                        value.ToString();

                    for (int i = 0;
                         i < combo.Items.Count;
                         i++)
                    {
                        StatusItem item =
                            combo.Items[i]
                            as StatusItem;

                        if (item != null &&
                            string.Equals(
                                item.Code,
                                code,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            combo.SelectedIndex =
                                i;

                            return;
                        }
                    }

                    return;
                }

                SelectComboValue(
                    combo,
                    value);

                return;
            }

            NumericUpDown numeric =
                editor as NumericUpDown;

            if (numeric != null)
            {
                decimal decimalValue;

                try
                {
                    decimalValue =
                        Convert.ToDecimal(
                            value,
                            CultureInfo.InvariantCulture);
                }
                catch
                {
                    return;
                }

                if (decimalValue <
                    numeric.Minimum)
                {
                    decimalValue =
                        numeric.Minimum;
                }

                if (decimalValue >
                    numeric.Maximum)
                {
                    decimalValue =
                        numeric.Maximum;
                }

                numeric.Value =
                    decimalValue;

                return;
            }

            TextBox textBox =
                editor as TextBox;

            if (textBox != null)
            {
                textBox.Text =
                    value.ToString();
            }
        }

        private void SelectComboValue(
            ComboBox combo,
            object value)
        {
            if (combo == null ||
                value == null ||
                value == DBNull.Value)
            {
                return;
            }

            string target =
                value.ToString();

            for (int i = 0;
                 i < combo.Items.Count;
                 i++)
            {
                DataRowView row =
                    combo.Items[i]
                    as DataRowView;

                if (row == null)
                    continue;

                object itemValue =
                    row.Row[
                        combo.ValueMember];

                if (itemValue == null ||
                    itemValue == DBNull.Value)
                {
                    continue;
                }

                if (string.Equals(
                    itemValue.ToString(),
                    target,
                    StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex =
                        i;

                    return;
                }
            }
        }
    }
}