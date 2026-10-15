using System;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        public void ActivateInfoTool()
        {
            btnInfo_Click(
                this,
                EventArgs.Empty);
        }

        public void ActivateQueryLocationTool()
        {
            btnQueryLocation_Click(
                this,
                EventArgs.Empty);
        }

        public void ActivateQueryAttributeTool()
        {
            btnQueryAttribute_Click(
                this,
                EventArgs.Empty);
        }

        public void ActivateMeasureTool()
        {
            btnMeasure_Click(
                this,
                EventArgs.Empty);
        }

        public void ClearMeasureTool()
        {
            btnClearMeasure_Click(
                this,
                EventArgs.Empty);
        }

        public void ActivateBufferTool()
        {
            btnBuffer_Click(
                this,
                EventArgs.Empty);
        }
    }
}
