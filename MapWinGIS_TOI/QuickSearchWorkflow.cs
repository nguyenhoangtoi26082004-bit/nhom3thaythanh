using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // QUICKSEARCHWORKFLOW.CS
    //
    // Tra cứu nhanh trên header:
    // - tự lazy-load lớp Đơn vị quân đội;
    // - truyền từ khóa thẳng vào UnitSearchForm;
    // - form mở ra đã có kết quả lọc;
    // - chọn/zoom trên bản đồ dùng được ngay khi layer nạp thành công.
    // =============================================================
    public partial class Form1
    {
        private void OpenUnitSearchSmooth(
            string keyword)
        {
            keyword =
                keyword == null
                    ? ""
                    : keyword.Trim();

            bool layerReady =
                EnsureLayerReady(
                    "don_vi_quan_doi",
                    true);

            int unitLayerHandle =
                layerReady
                    ? GetLayerHandleSafe(
                        "don_vi_quan_doi")
                    : -1;

            try
            {
                using (UnitSearchForm form =
                    new UnitSearchForm(
                        axMap2,
                        unitLayerHandle,
                        keyword))
                {
                    form.ShowDialog(
                        this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không mở được chức năng tra cứu đơn vị.\n\n" +
                    ex.Message,
                    "Lỗi tra cứu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}