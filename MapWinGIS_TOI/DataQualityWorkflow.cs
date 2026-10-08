using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // DATAQUALITYWORKFLOW.CS
    //
    // Nối module kiểm tra CSDL vào Form1 và bản đồ MapWinGIS.
    // =============================================================
    public partial class Form1
    {
        // =========================================================
        // MỞ MODULE KIỂM TRA CSDL
        // =========================================================
        private void OpenDataQualityModule()
        {
            try
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang mở kiểm tra chất lượng CSDL...";
                }

                DataQualityService service =
                    new DataQualityService();

                using (DataQualityForm form =
                    new DataQualityForm(
                        service,
                        ZoomToDataQualityIssue))
                {
                    form.ShowDialog(
                        this);
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã đóng kiểm tra chất lượng CSDL.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không mở được module kiểm tra CSDL.\n\n" +
                    ex.Message,
                    "Lỗi kiểm tra CSDL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        // =========================================================
        // THU PHÓNG TỚI BẢN GHI LỖI
        // =========================================================
        private void ZoomToDataQualityIssue(
            DataQualityIssue issue)
        {
            if (issue == null ||
                !issue.ObjectId.HasValue ||
                string.IsNullOrWhiteSpace(
                    issue.LayerKey))
            {
                return;
            }

            // Lazy-load đúng lớp cần thiết.
            if (!EnsureLayerReady(
                issue.LayerKey,
                true))
            {
                MessageBox.Show(
                    "Không thể nạp lớp \"" +
                    issue.LayerName +
                    "\" lên bản đồ.",
                    "Không thể thu phóng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int handle =
                GetLayerHandleSafe(
                    issue.LayerKey);

            if (handle < 0)
            {
                return;
            }

            bool selected =
                AttributeTableService
                    .SelectShapeByDatabaseId(
                        axMap2,
                        handle,
                        issue.ObjectId.Value,
                        true);

            if (!selected)
            {
                MessageBox.Show(
                    "Đã tìm thấy lỗi trong CSDL nhưng chưa xác định được " +
                    "geometry tương ứng trên lớp bản đồ.\n\n" +
                    "Có thể bản ghi chưa có geometry hoặc lớp cần được làm mới.",
                    "Không tìm thấy geometry",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            SelectWorkingLayer(
                issue.LayerKey);

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Kiểm tra CSDL | " +
                    issue.LayerName +
                    " | ID " +
                    issue.ObjectId.Value;
            }
        }
    }
}