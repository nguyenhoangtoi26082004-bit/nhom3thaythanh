using System;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        private bool _unitSearchToolsInitialized =
            false;

        private ToolStripMenuItem
            _menuSearchUnit;

        private void InitUnitSearchTools()
        {
            if (_unitSearchToolsInitialized)
                return;

            if (menuQuery == null)
                return;

            _menuSearchUnit =
                new ToolStripMenuItem();

            _menuSearchUnit.Name =
                "menuSearchUnit";

            _menuSearchUnit.Text =
                "Tra cứu đơn vị...";

            _menuSearchUnit.ShortcutKeys =
                Keys.Control | Keys.F;

            _menuSearchUnit.ShowShortcutKeys =
                true;

            _menuSearchUnit.Click +=
                menuSearchUnit_Click;

            menuQuery.DropDownItems.Insert(
                0,
                _menuSearchUnit);

            menuQuery.DropDownItems.Insert(
                1,
                new ToolStripSeparator());

            _unitSearchToolsInitialized =
                true;
        }

        private void menuSearchUnit_Click(
            object sender,
            EventArgs e)
        {
            OpenUnitSearchForm();
        }

        private void OpenUnitSearchForm()
        {
            int layerHandle =
                -1;

            _loadedLayers.TryGetValue(
                "don_vi_quan_doi",
                out layerHandle);

            using (UnitSearchForm form =
                new UnitSearchForm(
                    axMap2,
                    layerHandle))
            {
                form.ShowDialog(this);
            }
        }
    }
}