using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // MILITARY THEME
    //
    // Theme giao diện tối dùng chung cho toàn bộ WinForms.
    //
    // Tương thích:
    // - .NET Framework
    // - Windows Forms
    // - MapWinGIS ActiveX
    //
    // Không can thiệp trực tiếp vào AxMap.
    // =============================================================
    internal static class MilitaryTheme
    {
        // =========================================================
        // MÀU CHỦ ĐẠO
        // =========================================================

        public static readonly Color AppBackground =
            Color.FromArgb(
                2,
                6,
                23);

        public static readonly Color Surface =
            Color.FromArgb(
                15,
                23,
                42);

        public static readonly Color Surface2 =
            Color.FromArgb(
                30,
                41,
                59);

        public static readonly Color Surface3 =
            Color.FromArgb(
                51,
                65,
                85);

        public static readonly Color Border =
            Color.FromArgb(
                51,
                65,
                85);

        public static readonly Color TextPrimary =
            Color.FromArgb(
                241,
                245,
                249);

        public static readonly Color TextSecondary =
            Color.FromArgb(
                148,
                163,
                184);

        public static readonly Color Accent =
            Color.FromArgb(
                5,
                150,
                105);

        public static readonly Color AccentHover =
            Color.FromArgb(
                16,
                185,
                129);

        public static readonly Color Cyan =
            Color.FromArgb(
                34,
                211,
                238);

        public static readonly Color Amber =
            Color.FromArgb(
                245,
                158,
                11);

        public static readonly Color Danger =
            Color.FromArgb(
                190,
                24,
                93);

        public static readonly Color DangerDark =
            Color.FromArgb(
                76,
                5,
                25);

        // =========================================================
        // FORM ĐÃ THEME
        // =========================================================

        private static readonly HashSet<Form>
            _themedForms =
                new HashSet<Form>();

        private static bool
            _autoThemeStarted =
                false;

        // =========================================================
        // BẮT ĐẦU TỰ ĐỘNG THEME FORM
        // =========================================================
        public static void StartAutoTheme()
        {
            if (_autoThemeStarted)
                return;

            _autoThemeStarted =
                true;

            Application.Idle -=
                Application_Idle;

            Application.Idle +=
                Application_Idle;
        }

        // =========================================================
        // APPLICATION IDLE
        //
        // QUAN TRỌNG:
        // Không dùng:
        //
        // Application.OpenForms.CopyTo(...)
        //
        // vì FormCollection của .NET Framework hiện tại
        // không hỗ trợ overload đó.
        // =========================================================
        private static void Application_Idle(
            object sender,
            EventArgs e)
        {
            try
            {
                int count =
                    Application.OpenForms.Count;

                if (count <= 0)
                    return;

                Form[] forms =
                    new Form[count];

                // ---------------------------------------------
                // Copy thủ công để tương thích .NET Framework.
                // ---------------------------------------------
                for (int i = 0;
                    i < count;
                    i++)
                {
                    forms[i] =
                        Application.OpenForms[i];
                }

                // ---------------------------------------------
                // Theme từng Form.
                // ---------------------------------------------
                for (int i = 0;
                    i < forms.Length;
                    i++)
                {
                    Form form =
                        forms[i];

                    if (form == null)
                        continue;

                    if (form.IsDisposed)
                        continue;

                    if (_themedForms.Contains(
                        form))
                    {
                        continue;
                    }

                    ApplyToForm(
                        form);
                }
            }
            catch
            {
                // Không để lỗi theme làm crash chương trình.
            }
        }

        // =========================================================
        // ÁP DỤNG THEME CHO FORM
        // =========================================================
        public static void ApplyToForm(
            Form form)
        {
            if (form == null)
                return;

            if (form.IsDisposed)
                return;

            form.SuspendLayout();

            try
            {
                form.BackColor =
                    AppBackground;

                form.ForeColor =
                    TextPrimary;

                form.Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Regular);

                StyleControlTree(
                    form);

                if (!_themedForms.Contains(
                    form))
                {
                    _themedForms.Add(
                        form);
                }
            }
            catch
            {
                // Theme chỉ là giao diện,
                // không được phép làm chương trình dừng.
            }
            finally
            {
                try
                {
                    form.ResumeLayout(
                        true);
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // DUYỆT TOÀN BỘ CONTROL
        // =========================================================
        private static void StyleControlTree(
            Control parent)
        {
            if (parent == null)
                return;

            foreach (Control control
                in parent.Controls)
            {
                if (control == null)
                    continue;

                // ---------------------------------------------
                // Không tác động ActiveX MapWinGIS.
                // ---------------------------------------------
                AxHost axHost =
                    control as AxHost;

                if (axHost != null)
                    continue;

                StyleSingleControl(
                    control);

                if (control.HasChildren)
                {
                    StyleControlTree(
                        control);
                }
            }
        }

        // =========================================================
        // STYLE MỘT CONTROL
        // =========================================================
        private static void StyleSingleControl(
            Control control)
        {
            if (control == null)
                return;

            // =====================================================
            // PANEL
            // =====================================================
            Panel panel =
                control as Panel;

            if (panel != null)
            {
                panel.BackColor =
                    Surface;

                panel.ForeColor =
                    TextPrimary;

                return;
            }

            // =====================================================
            // SPLIT CONTAINER
            // =====================================================
            SplitContainer split =
                control as SplitContainer;

            if (split != null)
            {
                split.BackColor =
                    Border;

                split.Panel1.BackColor =
                    Surface;

                split.Panel2.BackColor =
                    AppBackground;

                return;
            }

            // =====================================================
            // GROUP BOX
            // =====================================================
            GroupBox group =
                control as GroupBox;

            if (group != null)
            {
                group.BackColor =
                    Surface;

                group.ForeColor =
                    TextPrimary;

                return;
            }

            // =====================================================
            // TAB CONTROL
            // =====================================================
            TabControl tabs =
                control as TabControl;

            if (tabs != null)
            {
                tabs.BackColor =
                    Surface;

                tabs.ForeColor =
                    TextPrimary;

                foreach (TabPage page
                    in tabs.TabPages)
                {
                    page.UseVisualStyleBackColor =
                        false;

                    page.BackColor =
                        Surface;

                    page.ForeColor =
                        TextPrimary;
                }

                return;
            }

            // =====================================================
            // LABEL
            // =====================================================
            System.Windows.Forms.Label label =
                control as System.Windows.Forms.Label;

            if (label != null)
            {
                if (label.ForeColor !=
                    Color.White)
                {
                    label.ForeColor =
                        TextPrimary;
                }

                // Không bắt buộc đổi BackColor vì một số label
                // nằm trên header có màu riêng.
                return;
            }

            // =====================================================
            // BUTTON
            // =====================================================
            Button button =
                control as Button;

            if (button != null)
            {
                StyleButton(
                    button);

                return;
            }

            // =====================================================
            // TEXT BOX / RICH TEXT BOX
            // =====================================================
            TextBoxBase textBox =
                control as TextBoxBase;

            if (textBox != null)
            {
                textBox.BackColor =
                    AppBackground;

                textBox.ForeColor =
                    TextPrimary;

                textBox.BorderStyle =
                    BorderStyle.FixedSingle;

                return;
            }

            // =====================================================
            // COMBO BOX
            // =====================================================
            ComboBox combo =
                control as ComboBox;

            if (combo != null)
            {
                combo.BackColor =
                    AppBackground;

                combo.ForeColor =
                    TextPrimary;

                combo.FlatStyle =
                    FlatStyle.Flat;

                return;
            }

            // =====================================================
            // NUMERIC UP DOWN
            // =====================================================
            NumericUpDown numeric =
                control as NumericUpDown;

            if (numeric != null)
            {
                numeric.BackColor =
                    AppBackground;

                numeric.ForeColor =
                    TextPrimary;

                numeric.BorderStyle =
                    BorderStyle.FixedSingle;

                return;
            }

            // =====================================================
            // DATE TIME PICKER
            // =====================================================
            DateTimePicker date =
                control as DateTimePicker;

            if (date != null)
            {
                date.CalendarMonthBackground =
                    AppBackground;

                date.CalendarForeColor =
                    TextPrimary;

                return;
            }

            // =====================================================
            // DATAGRIDVIEW
            // =====================================================
            DataGridView grid =
                control as DataGridView;

            if (grid != null)
            {
                StyleGrid(
                    grid);

                return;
            }

            // =====================================================
            // TREE VIEW
            // =====================================================
            TreeView tree =
                control as TreeView;

            if (tree != null)
            {
                tree.BackColor =
                    Surface;

                tree.ForeColor =
                    TextPrimary;

                tree.BorderStyle =
                    BorderStyle.None;

                tree.LineColor =
                    Surface3;

                tree.HideSelection =
                    false;

                tree.FullRowSelect =
                    true;

                return;
            }

            // =====================================================
            // LIST VIEW
            // =====================================================
            ListView list =
                control as ListView;

            if (list != null)
            {
                list.BackColor =
                    Surface;

                list.ForeColor =
                    TextPrimary;

                list.BorderStyle =
                    BorderStyle.FixedSingle;

                return;
            }

            // =====================================================
            // LIST BOX
            // =====================================================
            ListBox listBox =
                control as ListBox;

            if (listBox != null)
            {
                listBox.BackColor =
                    Surface;

                listBox.ForeColor =
                    TextPrimary;

                listBox.BorderStyle =
                    BorderStyle.FixedSingle;

                return;
            }

            // =====================================================
            // CHECKBOX
            // =====================================================
            CheckBox checkBox =
                control as CheckBox;

            if (checkBox != null)
            {
                checkBox.ForeColor =
                    TextPrimary;

                return;
            }

            // =====================================================
            // RADIO BUTTON
            // =====================================================
            RadioButton radio =
                control as RadioButton;

            if (radio != null)
            {
                radio.ForeColor =
                    TextPrimary;

                return;
            }

            // =====================================================
            // STATUS STRIP
            // =====================================================
            StatusStrip status =
                control as StatusStrip;

            if (status != null)
            {
                status.BackColor =
                    AppBackground;

                status.ForeColor =
                    TextSecondary;

                status.SizingGrip =
                    false;

                foreach (ToolStripItem item
                    in status.Items)
                {
                    item.ForeColor =
                        TextSecondary;
                }

                return;
            }

            // =====================================================
            // MENU STRIP
            // =====================================================
            MenuStrip menu =
                control as MenuStrip;

            if (menu != null)
            {
                menu.BackColor =
                    Surface;

                menu.ForeColor =
                    TextPrimary;

                menu.Renderer =
                    new ToolStripProfessionalRenderer(
                        new MilitaryColorTable());

                return;
            }

            // =====================================================
            // TOOL STRIP
            // =====================================================
            ToolStrip tool =
                control as ToolStrip;

            if (tool != null)
            {
                tool.BackColor =
                    Surface;

                tool.ForeColor =
                    TextPrimary;

                tool.Renderer =
                    new ToolStripProfessionalRenderer(
                        new MilitaryColorTable());

                foreach (ToolStripItem item
                    in tool.Items)
                {
                    item.ForeColor =
                        TextPrimary;
                }

                return;
            }
        }

        // =========================================================
        // STYLE BUTTON
        // =========================================================
        public static void StyleButton(
            Button button)
        {
            if (button == null)
                return;

            string text =
                button.Text == null
                    ? ""
                    : button.Text.Trim();

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.BorderColor =
                Border;

            button.ForeColor =
                TextPrimary;

            button.Cursor =
                Cursors.Hand;

            // -----------------------------------------------------
            // Nút chính.
            // -----------------------------------------------------
            bool primary =
                ContainsAny(
                    text,
                    "Thêm",
                    "Lưu",
                    "Tìm",
                    "Thực hiện",
                    "Chạy",
                    "Kết nối",
                    "Xác nhận",
                    "Tiếp tục");

            // -----------------------------------------------------
            // Nút xóa.
            // -----------------------------------------------------
            bool delete =
                ContainsAny(
                    text,
                    "Xóa",
                    "Xoá");

            if (primary)
            {
                button.BackColor =
                    Accent;

                button.ForeColor =
                    Color.White;

                button.FlatAppearance.MouseOverBackColor =
                    AccentHover;

                return;
            }

            if (delete)
            {
                button.BackColor =
                    DangerDark;

                button.ForeColor =
                    Color.FromArgb(
                        253,
                        164,
                        175);

                button.FlatAppearance.MouseOverBackColor =
                    Danger;

                return;
            }

            // -----------------------------------------------------
            // Nút thường.
            // -----------------------------------------------------
            button.BackColor =
                Surface2;

            button.FlatAppearance.MouseOverBackColor =
                Surface3;
        }

        // =========================================================
        // STYLE DATAGRIDVIEW
        // =========================================================
        public static void StyleGrid(
            DataGridView grid)
        {
            if (grid == null)
                return;

            grid.BackgroundColor =
                AppBackground;

            grid.BorderStyle =
                BorderStyle.None;

            grid.GridColor =
                Border;

            grid.EnableHeadersVisualStyles =
                false;

            grid.RowHeadersVisible =
                false;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.MultiSelect =
                false;

            grid.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;

            grid.RowTemplate.Height =
                30;

            // -----------------------------------------------------
            // Header.
            // -----------------------------------------------------
            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Surface;

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(
                    203,
                    213,
                    225);

            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Surface;

            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                TextPrimary;

            // -----------------------------------------------------
            // Cell.
            // -----------------------------------------------------
            grid.DefaultCellStyle.BackColor =
                AppBackground;

            grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    226,
                    232,
                    240);

            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    6,
                    78,
                    59);

            grid.DefaultCellStyle.SelectionForeColor =
                Color.White;

            // -----------------------------------------------------
            // Alternate rows.
            // -----------------------------------------------------
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(
                    8,
                    15,
                    30);
        }

        // =========================================================
        // KIỂM TRA TEXT BUTTON
        // =========================================================
        private static bool ContainsAny(
            string text,
            params string[] keywords)
        {
            if (text == null)
                return false;

            if (keywords == null)
                return false;

            for (int i = 0;
                i < keywords.Length;
                i++)
            {
                string keyword =
                    keywords[i];

                if (string.IsNullOrWhiteSpace(
                    keyword))
                {
                    continue;
                }

                if (text.IndexOf(
                    keyword,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // TOOLSTRIP COLOR TABLE
        // =========================================================
        private sealed class MilitaryColorTable
            : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin
            {
                get
                {
                    return Surface;
                }
            }

            public override Color ToolStripGradientMiddle
            {
                get
                {
                    return Surface;
                }
            }

            public override Color ToolStripGradientEnd
            {
                get
                {
                    return Surface;
                }
            }

            public override Color MenuStripGradientBegin
            {
                get
                {
                    return Surface;
                }
            }

            public override Color MenuStripGradientEnd
            {
                get
                {
                    return Surface;
                }
            }

            public override Color MenuItemSelected
            {
                get
                {
                    return Surface2;
                }
            }

            public override Color MenuItemBorder
            {
                get
                {
                    return Border;
                }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get
                {
                    return Surface2;
                }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get
                {
                    return Surface2;
                }
            }

            public override Color ImageMarginGradientBegin
            {
                get
                {
                    return Surface;
                }
            }

            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return Surface;
                }
            }

            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return Surface;
                }
            }

            public override Color ToolStripDropDownBackground
            {
                get
                {
                    return Surface;
                }
            }

            public override Color SeparatorDark
            {
                get
                {
                    return Border;
                }
            }

            public override Color SeparatorLight
            {
                get
                {
                    return Border;
                }
            }

            public override Color ButtonSelectedBorder
            {
                get
                {
                    return Accent;
                }
            }

            public override Color ButtonSelectedGradientBegin
            {
                get
                {
                    return Surface3;
                }
            }

            public override Color ButtonSelectedGradientMiddle
            {
                get
                {
                    return Surface3;
                }
            }

            public override Color ButtonSelectedGradientEnd
            {
                get
                {
                    return Surface3;
                }
            }
        }
    }
}