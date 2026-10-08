using System;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // ZOOMSLIDER.CS - THANH ZOOM DỌC NHỎ GỌN
    //
    // - Kéo lên: phóng to.
    // - Kéo xuống: thu nhỏ.
    // - Nút + / - thay đổi 1 mức zoom.
    // - ProfessionalUI.cs tự đặt panel này ngay dưới toolbar.
    // =============================================================
    public partial class Form1
    {
        private Panel _zoomSliderPanel;
        private TrackBar _zoomTrackBar;
        private Button _btnZoomSliderPlus;
        private Button _btnZoomSliderMinus;

        private bool _zoomSliderInternalChange = false;

        private const int ZOOM_SLIDER_MIN = 5;
        private const int ZOOM_SLIDER_MAX = 20;


        // =========================================================
        // KHỞI TẠO
        // =========================================================
        private void InitVerticalZoomSlider()
        {
            if (axMap2 == null ||
                IsDisposed ||
                Disposing)
            {
                return;
            }

            try
            {
                if (_zoomSliderPanel != null &&
                    !_zoomSliderPanel.IsDisposed)
                {
                    EnsureZoomSliderParent();
                    SyncVerticalZoomSlider();
                    _zoomSliderPanel.BringToFront();
                    return;
                }

                Control mapParent =
                    panelMap != null
                        ? (Control)panelMap
                        : axMap2.Parent;

                if (mapParent == null)
                {
                    return;
                }

                // -------------------------------------------------
                // PANEL
                // -------------------------------------------------
                _zoomSliderPanel =
                    new Panel();

                _zoomSliderPanel.Name =
                    "pnlVerticalZoomSlider";

                _zoomSliderPanel.Size =
                    new Size(
                        34,
                        160);

                // Vị trí dự phòng; ProfessionalUI sẽ căn lại.
                _zoomSliderPanel.Location =
                    new Point(
                        8,
                        270);

                _zoomSliderPanel.Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left;

                _zoomSliderPanel.BackColor =
                    Color.FromArgb(
                        14,
                        27,
                        48);

                _zoomSliderPanel.Padding =
                    new Padding(4);

                _zoomSliderPanel.Margin =
                    new Padding(0);


                // -------------------------------------------------
                // NÚT +
                // -------------------------------------------------
                _btnZoomSliderPlus =
                    new Button();

                _btnZoomSliderPlus.Name =
                    "btnZoomSliderPlus";

                _btnZoomSliderPlus.Text =
                    "+";

                _btnZoomSliderPlus.Size =
                    new Size(
                        26,
                        22);

                _btnZoomSliderPlus.Location =
                    new Point(
                        4,
                        3);

                _btnZoomSliderPlus.FlatStyle =
                    FlatStyle.Flat;

                _btnZoomSliderPlus.FlatAppearance.BorderSize =
                    0;

                _btnZoomSliderPlus.FlatAppearance.MouseOverBackColor =
                    Color.FromArgb(
                        30,
                        64,
                        95);

                _btnZoomSliderPlus.FlatAppearance.MouseDownBackColor =
                    Color.FromArgb(
                        22,
                        78,
                        99);

                _btnZoomSliderPlus.BackColor =
                    Color.FromArgb(
                        14,
                        27,
                        48);

                _btnZoomSliderPlus.ForeColor =
                    Color.FromArgb(
                        52,
                        211,
                        153);

                _btnZoomSliderPlus.Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold);

                _btnZoomSliderPlus.TextAlign =
                    ContentAlignment.MiddleCenter;

                _btnZoomSliderPlus.Cursor =
                    Cursors.Hand;

                _btnZoomSliderPlus.TabStop =
                    false;

                _btnZoomSliderPlus.Click +=
                    btnZoomSliderPlus_Click;


                // -------------------------------------------------
                // TRACKBAR
                // -------------------------------------------------
                _zoomTrackBar =
                    new TrackBar();

                _zoomTrackBar.Name =
                    "trkMapZoom";

                _zoomTrackBar.Orientation =
                    Orientation.Vertical;

                _zoomTrackBar.Minimum =
                    ZOOM_SLIDER_MIN;

                _zoomTrackBar.Maximum =
                    ZOOM_SLIDER_MAX;

                _zoomTrackBar.SmallChange =
                    1;

                _zoomTrackBar.LargeChange =
                    1;

                _zoomTrackBar.TickFrequency =
                    1;

                _zoomTrackBar.TickStyle =
                    TickStyle.None;

                _zoomTrackBar.Size =
                    new Size(
                        26,
                        108);

                _zoomTrackBar.Location =
                    new Point(
                        4,
                        25);

                _zoomTrackBar.BackColor =
                    Color.FromArgb(
                        14,
                        27,
                        48);

                _zoomTrackBar.TabStop =
                    false;

                _zoomTrackBar.Value =
                    ZoomToSliderValue(
                        GetSafeCurrentZoom());

                _zoomTrackBar.ValueChanged +=
                    zoomTrackBar_ValueChanged;


                // -------------------------------------------------
                // NÚT -
                // -------------------------------------------------
                _btnZoomSliderMinus =
                    new Button();

                _btnZoomSliderMinus.Name =
                    "btnZoomSliderMinus";

                _btnZoomSliderMinus.Text =
                    "−";

                _btnZoomSliderMinus.Size =
                    new Size(
                        26,
                        22);

                _btnZoomSliderMinus.Location =
                    new Point(
                        4,
                        135);

                _btnZoomSliderMinus.FlatStyle =
                    FlatStyle.Flat;

                _btnZoomSliderMinus.FlatAppearance.BorderSize =
                    0;

                _btnZoomSliderMinus.FlatAppearance.MouseOverBackColor =
                    Color.FromArgb(
                        30,
                        64,
                        95);

                _btnZoomSliderMinus.FlatAppearance.MouseDownBackColor =
                    Color.FromArgb(
                        22,
                        78,
                        99);

                _btnZoomSliderMinus.BackColor =
                    Color.FromArgb(
                        14,
                        27,
                        48);

                _btnZoomSliderMinus.ForeColor =
                    Color.FromArgb(
                        34,
                        211,
                        238);

                _btnZoomSliderMinus.Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold);

                _btnZoomSliderMinus.TextAlign =
                    ContentAlignment.MiddleCenter;

                _btnZoomSliderMinus.Cursor =
                    Cursors.Hand;

                _btnZoomSliderMinus.TabStop =
                    false;

                _btnZoomSliderMinus.Click +=
                    btnZoomSliderMinus_Click;


                // -------------------------------------------------
                // GẮN VÀO MAP
                // -------------------------------------------------
                _zoomSliderPanel.Controls.Add(
                    _btnZoomSliderPlus);

                _zoomSliderPanel.Controls.Add(
                    _zoomTrackBar);

                _zoomSliderPanel.Controls.Add(
                    _btnZoomSliderMinus);

                mapParent.Controls.Add(
                    _zoomSliderPanel);

                _zoomSliderPanel.BringToFront();

                try
                {
                    mapParent.Controls.SetChildIndex(
                        _zoomSliderPanel,
                        0);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "InitVerticalZoomSlider ERROR: " +
                    ex);
            }
        }


        // =========================================================
        // BẢO ĐẢM PANEL ĐÚNG PARENT
        // =========================================================
        private void EnsureZoomSliderParent()
        {
            if (_zoomSliderPanel == null ||
                _zoomSliderPanel.IsDisposed)
            {
                return;
            }

            Control desiredParent =
                panelMap != null
                    ? (Control)panelMap
                    : axMap2 == null
                        ? null
                        : axMap2.Parent;

            if (desiredParent == null)
            {
                return;
            }

            try
            {
                if (_zoomSliderPanel.Parent !=
                    desiredParent)
                {
                    if (_zoomSliderPanel.Parent != null)
                    {
                        _zoomSliderPanel.Parent.Controls.Remove(
                            _zoomSliderPanel);
                    }

                    desiredParent.Controls.Add(
                        _zoomSliderPanel);
                }

                _zoomSliderPanel.BringToFront();

                desiredParent.Controls.SetChildIndex(
                    _zoomSliderPanel,
                    0);
            }
            catch
            {
            }
        }


        // =========================================================
        // LẤY ZOOM HIỆN TẠI
        // =========================================================
        private int GetSafeCurrentZoom()
        {
            int zoom = 10;

            if (axMap2 == null)
            {
                return zoom;
            }

            try
            {
                zoom =
                    axMap2.CurrentZoom;
            }
            catch
            {
                zoom = 10;
            }

            if (zoom <
                ZOOM_SLIDER_MIN)
            {
                zoom =
                    ZOOM_SLIDER_MIN;
            }

            if (zoom >
                ZOOM_SLIDER_MAX)
            {
                zoom =
                    ZOOM_SLIDER_MAX;
            }

            return zoom;
        }


        // =========================================================
        // ZOOM -> VALUE TRACKBAR
        // =========================================================
        private int ZoomToSliderValue(
            int zoom)
        {
            if (zoom <
                ZOOM_SLIDER_MIN)
            {
                zoom =
                    ZOOM_SLIDER_MIN;
            }

            if (zoom >
                ZOOM_SLIDER_MAX)
            {
                zoom =
                    ZOOM_SLIDER_MAX;
            }

            return
                ZOOM_SLIDER_MIN +
                ZOOM_SLIDER_MAX -
                zoom;
        }


        // =========================================================
        // VALUE TRACKBAR -> ZOOM
        // =========================================================
        private int SliderValueToZoom(
            int sliderValue)
        {
            int zoom =
                ZOOM_SLIDER_MIN +
                ZOOM_SLIDER_MAX -
                sliderValue;

            if (zoom <
                ZOOM_SLIDER_MIN)
            {
                zoom =
                    ZOOM_SLIDER_MIN;
            }

            if (zoom >
                ZOOM_SLIDER_MAX)
            {
                zoom =
                    ZOOM_SLIDER_MAX;
            }

            return zoom;
        }


        // =========================================================
        // KÉO THANH ZOOM
        // =========================================================
        private void zoomTrackBar_ValueChanged(
            object sender,
            EventArgs e)
        {
            if (_zoomSliderInternalChange ||
                axMap2 == null ||
                _zoomTrackBar == null)
            {
                return;
            }

            try
            {
                int targetZoom =
                    SliderValueToZoom(
                        _zoomTrackBar.Value);

                if (axMap2.CurrentZoom !=
                    targetZoom)
                {
                    axMap2.CurrentZoom =
                        targetZoom;

                    axMap2.Redraw();
                }

                axMap2.Focus();
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "zoomTrackBar_ValueChanged ERROR: " +
                    ex.Message);
            }
        }


        // =========================================================
        // +
        // =========================================================
        private void btnZoomSliderPlus_Click(
            object sender,
            EventArgs e)
        {
            if (_zoomTrackBar == null)
            {
                return;
            }

            if (_zoomTrackBar.Value >
                _zoomTrackBar.Minimum)
            {
                _zoomTrackBar.Value -=
                    1;
            }
        }


        // =========================================================
        // -
        // =========================================================
        private void btnZoomSliderMinus_Click(
            object sender,
            EventArgs e)
        {
            if (_zoomTrackBar == null)
            {
                return;
            }

            if (_zoomTrackBar.Value <
                _zoomTrackBar.Maximum)
            {
                _zoomTrackBar.Value +=
                    1;
            }
        }


        // =========================================================
        // ĐỒNG BỘ SLIDER VỚI MAP
        // =========================================================
        private void SyncVerticalZoomSlider()
        {
            if (_zoomTrackBar == null ||
                _zoomTrackBar.IsDisposed ||
                axMap2 == null)
            {
                return;
            }

            try
            {
                int sliderValue =
                    ZoomToSliderValue(
                        GetSafeCurrentZoom());

                if (sliderValue <
                    _zoomTrackBar.Minimum)
                {
                    sliderValue =
                        _zoomTrackBar.Minimum;
                }

                if (sliderValue >
                    _zoomTrackBar.Maximum)
                {
                    sliderValue =
                        _zoomTrackBar.Maximum;
                }

                _zoomSliderInternalChange =
                    true;

                if (_zoomTrackBar.Value !=
                    sliderValue)
                {
                    _zoomTrackBar.Value =
                        sliderValue;
                }
            }
            catch
            {
            }
            finally
            {
                _zoomSliderInternalChange =
                    false;
            }
        }
    }
}