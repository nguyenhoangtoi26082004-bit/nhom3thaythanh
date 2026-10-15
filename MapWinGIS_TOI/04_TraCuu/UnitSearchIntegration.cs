
using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ============================================================
    // UNIT SEARCH INTEGRATION - GOOGLE AI STUDIO V6
    //
    // Menu Tra cứu và Ctrl + F
    // sử dụng chung màn hình Tra cứu Web.
    //
    // Không mở UnitSearchForm của ql_don_vi.
    // Không tạo truy vấn hoặc chức năng trùng lặp.
    // ============================================================
    public partial class Form1
    {
        private bool _unitSearchToolsInitialized = false;

        private ToolStripMenuItem _menuSearchUnit;

        // ========================================================
        // 1. KHỞI TẠO MENU TRA CỨU
        // ========================================================
        private void InitUnitSearchTools()
        {
            if (_unitSearchToolsInitialized)
                return;

            if (menuQuery == null)
                return;

            _menuSearchUnit = new ToolStripMenuItem();

            _menuSearchUnit.Name = "menuSearchUnit";
            _menuSearchUnit.Text = "Tra cứu đơn vị Web...";
            _menuSearchUnit.ShortcutKeys =
                Keys.Control | Keys.F;
            _menuSearchUnit.ShowShortcutKeys = true;

            _menuSearchUnit.Click +=
                menuSearchUnit_Click;

            menuQuery.DropDownItems.Insert(
                0,
                _menuSearchUnit);

            menuQuery.DropDownItems.Insert(
                1,
                new ToolStripSeparator());

            _unitSearchToolsInitialized = true;
        }

        // ========================================================
        // 2. XỬ LÝ MENU / CTRL + F
        // ========================================================
        private void menuSearchUnit_Click(
            object sender,
            EventArgs e)
        {
            OpenUnitSearchForm();
        }

        // ========================================================
        // 3. GIỮ TÊN HÀM CŨ ĐỂ TƯƠNG THÍCH
        //
        // Những mô-đun khác nếu gọi OpenUnitSearchForm()
        // vẫn hoạt động, nhưng dùng nguồn web_mau.
        // ========================================================
        private void OpenUnitSearchForm()
        {
            OpenUnitSearchSmooth("");
        }
    }
}
