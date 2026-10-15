namespace MapWinGIS_TOI
{
    partial class EditObjectForm
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
            this.layoutRoot =
                new System.Windows.Forms.TableLayoutPanel();

            this.panelHeader =
                new System.Windows.Forms.Panel();

            this.lblTitle =
                new System.Windows.Forms.Label();

            this.lblSubtitle =
                new System.Windows.Forms.Label();

            this.lblStepBadge =
                new System.Windows.Forms.Label();

            this.panelFields =
                new System.Windows.Forms.Panel();

            this.panelCommands =
                new System.Windows.Forms.Panel();

            this.layoutFooter =
                new System.Windows.Forms.TableLayoutPanel();

            this.lblCommandHint =
                new System.Windows.Forms.Label();

            this.flowButtons =
                new System.Windows.Forms.FlowLayoutPanel();

            this.btnSave =
                new System.Windows.Forms.Button();

            this.btnCancel =
                new System.Windows.Forms.Button();

            this.layoutRoot.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelCommands.SuspendLayout();
            this.layoutFooter.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();

            // =====================================================
            // ROOT LAYOUT
            // =====================================================
            this.layoutRoot.ColumnCount =
                1;

            this.layoutRoot.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.layoutRoot.RowCount =
                3;

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
                    96F));

            this.layoutRoot.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.layoutRoot.Margin =
                new System.Windows.Forms.Padding(0);

            this.layoutRoot.Name =
                "layoutRoot";

            this.layoutRoot.Padding =
                new System.Windows.Forms.Padding(0);

            this.layoutRoot.Controls.Add(
                this.panelHeader,
                0,
                0);

            this.layoutRoot.Controls.Add(
                this.panelFields,
                0,
                1);

            this.layoutRoot.Controls.Add(
                this.panelCommands,
                0,
                2);

            // =====================================================
            // HEADER
            // =====================================================
            this.panelHeader.BackColor =
                System.Drawing.Color.FromArgb(
                    35,
                    68,
                    101);

            this.panelHeader.Controls.Add(
                this.lblTitle);

            this.panelHeader.Controls.Add(
                this.lblSubtitle);

            this.panelHeader.Controls.Add(
                this.lblStepBadge);

            this.panelHeader.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelHeader.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelHeader.Name =
                "panelHeader";

            // TITLE
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
                    12);

            this.lblTitle.Name =
                "lblTitle";

            this.lblTitle.Text =
                "CHỈNH SỬA ĐỐI TƯỢNG";

            // SUBTITLE
            this.lblSubtitle.AutoEllipsis =
                true;

            this.lblSubtitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.lblSubtitle.ForeColor =
                System.Drawing.Color.Gainsboro;

            this.lblSubtitle.Location =
                new System.Drawing.Point(
                    19,
                    48);

            this.lblSubtitle.Name =
                "lblSubtitle";

            this.lblSubtitle.Size =
                new System.Drawing.Size(
                    560,
                    28);

            this.lblSubtitle.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblSubtitle.Text =
                "Thông tin thuộc tính";

            // STEP BADGE
            this.lblStepBadge.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblStepBadge.BackColor =
                System.Drawing.Color.FromArgb(
                    52,
                    101,
                    150);

            this.lblStepBadge.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblStepBadge.ForeColor =
                System.Drawing.Color.White;

            this.lblStepBadge.Location =
                new System.Drawing.Point(
                    590,
                    16);

            this.lblStepBadge.Name =
                "lblStepBadge";

            this.lblStepBadge.Size =
                new System.Drawing.Size(
                    110,
                    28);

            this.lblStepBadge.Text =
                "BƯỚC 1/2";

            this.lblStepBadge.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            // =====================================================
            // FIELDS
            // =====================================================
            this.panelFields.AutoScroll =
                true;

            this.panelFields.BackColor =
                System.Drawing.Color.White;

            this.panelFields.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelFields.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelFields.Name =
                "panelFields";

            this.panelFields.Padding =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    0,
                    12);

            // =====================================================
            // FOOTER
            // =====================================================
            this.panelCommands.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    249,
                    250);

            this.panelCommands.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.panelCommands.Controls.Add(
                this.layoutFooter);

            this.panelCommands.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.panelCommands.Margin =
                new System.Windows.Forms.Padding(0);

            this.panelCommands.Name =
                "panelCommands";

            // Footer table: hint trái, nút phải.
            this.layoutFooter.ColumnCount =
                2;

            this.layoutFooter.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.layoutFooter.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    280F));

            this.layoutFooter.RowCount =
                1;

            this.layoutFooter.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.layoutFooter.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.layoutFooter.Margin =
                new System.Windows.Forms.Padding(0);

            this.layoutFooter.Padding =
                new System.Windows.Forms.Padding(
                    12,
                    10,
                    12,
                    10);

            this.layoutFooter.Name =
                "layoutFooter";

            this.layoutFooter.Controls.Add(
                this.lblCommandHint,
                0,
                0);

            this.layoutFooter.Controls.Add(
                this.flowButtons,
                1,
                0);

            // HINT
            this.lblCommandHint.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblCommandHint.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.lblCommandHint.ForeColor =
                System.Drawing.Color.FromArgb(
                    70,
                    70,
                    70);

            this.lblCommandHint.Margin =
                new System.Windows.Forms.Padding(
                    4,
                    4,
                    8,
                    4);

            this.lblCommandHint.Name =
                "lblCommandHint";

            this.lblCommandHint.Text =
                "Kiểm tra thông tin trước khi tiếp tục.";

            this.lblCommandHint.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            // BUTTON FLOW
            this.flowButtons.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.flowButtons.FlowDirection =
                System.Windows.Forms.FlowDirection.RightToLeft;

            this.flowButtons.Margin =
                new System.Windows.Forms.Padding(0);

            this.flowButtons.Name =
                "flowButtons";

            this.flowButtons.Padding =
                new System.Windows.Forms.Padding(
                    0,
                    10,
                    0,
                    0);

            this.flowButtons.WrapContents =
                false;

            this.flowButtons.Controls.Add(
                this.btnSave);

            this.flowButtons.Controls.Add(
                this.btnCancel);

            // PRIMARY BUTTON
            this.btnSave.BackColor =
                System.Drawing.Color.FromArgb(
                    35,
                    68,
                    101);

            this.btnSave.FlatAppearance.BorderSize =
                0;

            this.btnSave.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnSave.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnSave.ForeColor =
                System.Drawing.Color.White;

            this.btnSave.Margin =
                new System.Windows.Forms.Padding(
                    8,
                    0,
                    0,
                    0);

            this.btnSave.Name =
                "btnSave";

            this.btnSave.Size =
                new System.Drawing.Size(
                    145,
                    40);

            this.btnSave.TabIndex =
                0;

            this.btnSave.Text =
                "Lưu";

            this.btnSave.UseVisualStyleBackColor =
                false;

            this.btnSave.Click +=
                new System.EventHandler(
                    this.btnSave_Click);

            // CANCEL BUTTON
            this.btnCancel.BackColor =
                System.Drawing.Color.White;

            this.btnCancel.FlatStyle =
                System.Windows.Forms.FlatStyle.System;

            this.btnCancel.Margin =
                new System.Windows.Forms.Padding(
                    8,
                    0,
                    0,
                    0);

            this.btnCancel.Name =
                "btnCancel";

            this.btnCancel.Size =
                new System.Drawing.Size(
                    100,
                    40);

            this.btnCancel.TabIndex =
                1;

            this.btnCancel.Text =
                "Hủy";

            this.btnCancel.UseVisualStyleBackColor =
                true;

            this.btnCancel.Click +=
                new System.EventHandler(
                    this.btnCancel_Click);

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
                    720,
                    720);

            this.Controls.Add(
                this.layoutRoot);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            // Cho phép resize để không mất nút ở Windows Scale 125/150%.
            this.FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.Sizable;

            this.MaximizeBox =
                false;

            this.MinimizeBox =
                false;

            this.MinimumSize =
                new System.Drawing.Size(
                    680,
                    620);

            this.Name =
                "EditObjectForm";

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterParent;

            this.Text =
                "Chỉnh sửa đối tượng";

            this.layoutRoot.ResumeLayout(false);

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();

            this.panelCommands.ResumeLayout(false);

            this.layoutFooter.ResumeLayout(false);

            this.flowButtons.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutRoot;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStepBadge;

        private System.Windows.Forms.Panel panelFields;

        private System.Windows.Forms.Panel panelCommands;
        private System.Windows.Forms.TableLayoutPanel layoutFooter;
        private System.Windows.Forms.Label lblCommandHint;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}