using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public class ToolsForm : Form
    {
        private readonly Form1 _mainForm;

        private FlowLayoutPanel pnlTools;
        private Button btnInfo;
        private Button btnQueryLocation;
        private Button btnQueryAttribute;
        private Button btnMeasure;
        private Button btnClearMeasure;
        private Button btnBuffer;

        public ToolsForm(Form1 mainForm)
        {
            if (mainForm == null)
                throw new ArgumentNullException("mainForm");

            _mainForm = mainForm;
            InitializeToolsForm();
        }

        private void InitializeToolsForm()
        {
            Text = "Công cụ GIS";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(650, 500);

            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 90;
            pnlHeader.BackColor = Color.FromArgb(35, 82, 124);

            Label lblTitle = new Label();
            lblTitle.Text = "CÔNG CỤ GIS";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(25, 16);

            Label lblSubTitle = new Label();
            lblSubTitle.Text =
                "Các công cụ truy vấn và xử lý dữ liệu không gian";
            lblSubTitle.ForeColor = Color.FromArgb(220, 230, 240);
            lblSubTitle.AutoSize = true;
            lblSubTitle.Location = new Point(27, 54);

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubTitle);

            pnlTools = new FlowLayoutPanel();
            pnlTools.Dock = DockStyle.Fill;
            pnlTools.Padding = new Padding(22);
            pnlTools.FlowDirection = FlowDirection.LeftToRight;
            pnlTools.WrapContents = true;
            pnlTools.AutoScroll = true;
            pnlTools.BackColor = Color.FromArgb(245, 247, 250);

            btnInfo = CreateToolButton(
                "Thông tin đối tượng",
                "Xem thuộc tính của đối tượng trên bản đồ.");

            btnQueryLocation = CreateToolButton(
                "Truy vấn vị trí",
                "Nhận diện đối tượng trên lớp hiện tại.");

            btnQueryAttribute = CreateToolButton(
                "Truy vấn thuộc tính",
                "Tìm đối tượng theo biểu thức thuộc tính.");

            btnMeasure = CreateToolButton(
                "Đo khoảng cách",
                "Đo khoảng cách trực tiếp trên bản đồ.");

            btnClearMeasure = CreateToolButton(
                "Xóa kết quả đo",
                "Xóa toàn bộ kết quả đo hiện tại.");

            btnBuffer = CreateToolButton(
                "Tạo vùng đệm",
                "Tạo Buffer cho lớp hoặc đối tượng đã chọn.");

            btnInfo.Click += btnInfo_Click;
            btnQueryLocation.Click += btnQueryLocation_Click;
            btnQueryAttribute.Click += btnQueryAttribute_Click;
            btnMeasure.Click += btnMeasure_Click;
            btnClearMeasure.Click += btnClearMeasure_Click;
            btnBuffer.Click += btnBuffer_Click;

            pnlTools.Controls.Add(btnInfo);
            pnlTools.Controls.Add(btnQueryLocation);
            pnlTools.Controls.Add(btnQueryAttribute);
            pnlTools.Controls.Add(btnMeasure);
            pnlTools.Controls.Add(btnClearMeasure);
            pnlTools.Controls.Add(btnBuffer);

            Panel pnlFooter = new Panel();
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Height = 60;
            pnlFooter.BackColor = Color.White;

            Button btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Size = new Size(100, 32);
            btnClose.Location =
                new Point(ClientSize.Width - 125, 14);
            btnClose.Anchor =
                AnchorStyles.Right | AnchorStyles.Top;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.White;
            btnClose.FlatAppearance.BorderColor =
                Color.FromArgb(190, 195, 200);
            btnClose.Click += delegate { Close(); };

            pnlFooter.Controls.Add(btnClose);

            Controls.Add(pnlTools);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }

        private Button CreateToolButton(
            string title,
            string description)
        {
            Button button = new Button();

            button.Size = new Size(285, 96);
            button.Margin = new Padding(8);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(35, 50, 65);
            button.FlatAppearance.BorderColor =
                Color.FromArgb(210, 215, 220);
            button.FlatAppearance.BorderSize = 1;
            button.Font =
                new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(15, 6, 10, 6);
            button.Text =
                title +
                Environment.NewLine +
                Environment.NewLine +
                description;
            button.Cursor = Cursors.Hand;

            button.MouseEnter += delegate
            {
                button.BackColor =
                    Color.FromArgb(235, 243, 250);
            };

            button.MouseLeave += delegate
            {
                button.BackColor = Color.White;
            };

            return button;
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            Hide();
            _mainForm.ActivateInfoTool();
            Close();
        }

        private void btnQueryLocation_Click(object sender, EventArgs e)
        {
            Hide();
            _mainForm.ActivateQueryLocationTool();
            Close();
        }

        private void btnQueryAttribute_Click(object sender, EventArgs e)
        {
            Hide();
            _mainForm.ActivateQueryAttributeTool();
            Close();
        }

        private void btnMeasure_Click(object sender, EventArgs e)
        {
            Hide();
            _mainForm.ActivateMeasureTool();
            Close();
        }

        private void btnClearMeasure_Click(object sender, EventArgs e)
        {
            _mainForm.ClearMeasureTool();
        }

        private void btnBuffer_Click(object sender, EventArgs e)
        {
            Hide();
            _mainForm.ActivateBufferTool();
            Close();
        }
    }
}
