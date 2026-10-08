using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AxMapWinGIS;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        private int _unitClickDownX = -1;
        private int _unitClickDownY = -1;
        private const int UNIT_CLICK_TOLERANCE_PX = 14;

        private void InitQueryTools()
        {
            btnInfo.Click -= btnInfo_Click;
            btnInfo.Click += btnInfo_Click;

            btnQueryLocation.Click -= btnQueryLocation_Click;
            btnQueryLocation.Click += btnQueryLocation_Click;

            btnQueryAttribute.Click -= btnQueryAttribute_Click;
            btnQueryAttribute.Click += btnQueryAttribute_Click;

            axMap2.ShapeIdentified -= axMap2_ShapeIdentified;
            axMap2.ShapeIdentified += axMap2_ShapeIdentified;

            // -----------------------------------------------------
            // CLICK TRỰC TIẾP VÀO ĐIỂM ĐƠN VỊ
            //
            // Không phụ thuộc ShapeIdentified của MapWinGIS.
            // Người dùng chỉ cần click một chấm đỏ khi đang ở chế độ
            // Pan hoặc Identify; hệ thống tự tìm Point gần nhất.
            // -----------------------------------------------------
            try
            {
                axMap2.SendMouseDown = true;
                axMap2.SendMouseUp = true;

                axMap2.MouseDownEvent -= axMap2_UnitInfoMouseDown;
                axMap2.MouseDownEvent += axMap2_UnitInfoMouseDown;

                axMap2.MouseUpEvent -= axMap2_UnitInfoMouseUp;
                axMap2.MouseUpEvent += axMap2_UnitInfoMouseUp;
            }
            catch
            {
            }
        }

        private void RefreshLayerCombo()
        {
            string current =
                cboCurrentLayer.SelectedItem as string;

            cboCurrentLayer.BeginUpdate();

            try
            {
                cboCurrentLayer.Items.Clear();

                foreach (string key in _loadedLayers.Keys)
                    cboCurrentLayer.Items.Add(key);

                if (!string.IsNullOrWhiteSpace(current) &&
                    cboCurrentLayer.Items.Contains(current))
                {
                    cboCurrentLayer.SelectedItem = current;
                }
                else if (cboCurrentLayer.Items.Count > 0)
                {
                    cboCurrentLayer.SelectedIndex = 0;
                }
            }
            finally
            {
                cboCurrentLayer.EndUpdate();
            }
        }

        // =========================================================
        // CLICK TRỰC TIẾP VÀO ĐIỂM ĐƠN VỊ
        // =========================================================
        private void axMap2_UnitInfoMouseDown(
            object sender,
            _DMapEvents_MouseDownEvent e)
        {
            if (e == null ||
                e.button != 1)
            {
                return;
            }

            _unitClickDownX =
                e.x;

            _unitClickDownY =
                e.y;
        }


        private void axMap2_UnitInfoMouseUp(
            object sender,
            _DMapEvents_MouseUpEvent e)
        {
            if (e == null ||
                e.button != 1)
            {
                return;
            }

            // Nếu người dùng đang kéo bản đồ thì không coi là click.
            if (_unitClickDownX >= 0 &&
                _unitClickDownY >= 0)
            {
                int dx =
                    Math.Abs(
                        e.x -
                        _unitClickDownX);

                int dy =
                    Math.Abs(
                        e.y -
                        _unitClickDownY);

                _unitClickDownX =
                    -1;

                _unitClickDownY =
                    -1;

                if (dx > 4 ||
                    dy > 4)
                {
                    return;
                }
            }

            // Chỉ bắt click khi đang Pan hoặc Identify.
            try
            {
                tkCursorMode mode =
                    axMap2.CursorMode;

                if (mode != tkCursorMode.cmPan &&
                    mode != tkCursorMode.cmIdentify)
                {
                    return;
                }
            }
            catch
            {
            }

            TryShowUnitInfoAtPixel(
                e.x,
                e.y);
        }


        // =========================================================
        // TÌM POINT ĐƠN VỊ GẦN VỊ TRÍ CLICK
        // =========================================================
        private bool TryShowUnitInfoAtPixel(
            int pixelX,
            int pixelY)
        {
            if (axMap2 == null ||
                _loadedLayers == null ||
                !_loadedLayers.ContainsKey(
                    "don_vi_quan_doi"))
            {
                return false;
            }

            int handle =
                _loadedLayers[
                    "don_vi_quan_doi"];

            Shapefile sf = null;

            try
            {
                sf =
                    axMap2.get_Shapefile(
                        handle);
            }
            catch
            {
            }

            if (sf == null ||
                sf.NumShapes <= 0)
            {
                return false;
            }

            try
            {
                // -------------------------------------------------
                // Tạo vùng chọn theo pixel, vì tolerance theo mét
                // thay đổi theo mức zoom.
                // -------------------------------------------------
                double x1 = 0.0;
                double y1 = 0.0;
                double x2 = 0.0;
                double y2 = 0.0;

                axMap2.PixelToProj(
                    pixelX -
                    UNIT_CLICK_TOLERANCE_PX,
                    pixelY -
                    UNIT_CLICK_TOLERANCE_PX,
                    ref x1,
                    ref y1);

                axMap2.PixelToProj(
                    pixelX +
                    UNIT_CLICK_TOLERANCE_PX,
                    pixelY +
                    UNIT_CLICK_TOLERANCE_PX,
                    ref x2,
                    ref y2);

                MapWinGIS.Extents ext =
                    new MapWinGIS.Extents();

                ext.SetBounds(
                    Math.Min(
                        x1,
                        x2),
                    Math.Min(
                        y1,
                        y2),
                    0.0,
                    Math.Max(
                        x1,
                        x2),
                    Math.Max(
                        y1,
                        y2),
                    0.0);

                object result =
                    null;

                bool found =
                    sf.SelectShapes(
                        ext,
                        0.0,
                        SelectMode.INTERSECTION,
                        ref result);

                if (!found ||
                    result == null)
                {
                    return false;
                }

                Array indices =
                    result as Array;

                if (indices == null ||
                    indices.Length == 0)
                {
                    return false;
                }

                int bestShapeIndex =
                    -1;

                double bestDistance =
                    double.MaxValue;

                foreach (object item
                    in indices)
                {
                    int shapeIndex;

                    try
                    {
                        shapeIndex =
                            Convert.ToInt32(
                                item);
                    }
                    catch
                    {
                        continue;
                    }

                    if (shapeIndex < 0 ||
                        shapeIndex >=
                        sf.NumShapes)
                    {
                        continue;
                    }

                    MapWinGIS.Shape shp =
                        sf.get_Shape(
                            shapeIndex);

                    if (shp == null)
                    {
                        continue;
                    }

                    double projX = 0.0;
                    double projY = 0.0;

                    // MapWinGIS 5.3 trong project này không expose
                    // thuộc tính NumPoints trên Shape. Với lớp đơn vị
                    // là Point, chỉ cần thử đọc trực tiếp đỉnh số 0.
                    if (!shp.get_XY(
                        0,
                        ref projX,
                        ref projY))
                    {
                        continue;
                    }

                    double screenX = 0.0;
                    double screenY = 0.0;

                    axMap2.ProjToPixel(
                        projX,
                        projY,
                        ref screenX,
                        ref screenY);

                    double dx =
                        screenX -
                        pixelX;

                    double dy =
                        screenY -
                        pixelY;

                    double distance =
                        Math.Sqrt(
                            dx * dx +
                            dy * dy);

                    if (distance <
                        bestDistance)
                    {
                        bestDistance =
                            distance;

                        bestShapeIndex =
                            shapeIndex;
                    }
                }

                if (bestShapeIndex < 0 ||
                    bestDistance >
                    UNIT_CLICK_TOLERANCE_PX +
                    4)
                {
                    return false;
                }

                ShowUnitInfoFromShape(
                    handle,
                    sf,
                    bestShapeIndex);

                return true;
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "TryShowUnitInfoAtPixel ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // =========================================================
        // HIỂN THỊ THÔNG TIN 1 POINT ĐƠN VỊ
        // =========================================================
        private void ShowUnitInfoFromShape(
            int layerHandle,
            Shapefile sf,
            int shapeIndex)
        {
            if (sf == null ||
                shapeIndex < 0 ||
                shapeIndex >= sf.NumShapes)
            {
                return;
            }

            Dictionary<string, string> attributes =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int fieldIndex = 0;
                fieldIndex < sf.NumFields;
                fieldIndex++)
            {
                try
                {
                    Field field =
                        sf.get_Field(
                            fieldIndex);

                    if (field == null)
                    {
                        continue;
                    }

                    string fieldName =
                        field.Name ?? "";

                    object raw =
                        sf.get_CellValue(
                            fieldIndex,
                            shapeIndex);

                    string value =
                        raw == null ||
                        raw == DBNull.Value
                            ? ""
                            : raw.ToString();

                    if (!attributes.ContainsKey(
                        fieldName))
                    {
                        attributes.Add(
                            fieldName,
                            value);
                    }
                }
                catch
                {
                }
            }

            double pointX = 0.0;
            double pointY = 0.0;

            try
            {
                MapWinGIS.Shape shp =
                    sf.get_Shape(
                        shapeIndex);

                if (shp != null)
                {
                    // Không dùng NumPoints vì COM wrapper MapWinGIS 5.3
                    // của project hiện tại không cung cấp property này.
                    shp.get_XY(
                        0,
                        ref pointX,
                        ref pointY);
                }
            }
            catch
            {
            }

            // -----------------------------------------------------
            // Chọn sáng đúng điểm vừa click.
            // -----------------------------------------------------
            try
            {
                sf.SelectNone();

                sf.set_ShapeSelected(
                    shapeIndex,
                    true);

                axMap2.Redraw();
            }
            catch
            {
            }

            // -----------------------------------------------------
            // Đưa lớp đơn vị thành lớp làm việc hiện tại.
            // -----------------------------------------------------
            try
            {
                SelectWorkingLayer(
                    "don_vi_quan_doi");
            }
            catch
            {
            }

            string layerName =
                "Đơn vị quân đội";

            try
            {
                string currentName =
                    axMap2.get_LayerName(
                        layerHandle);

                if (!string.IsNullOrWhiteSpace(
                    currentName))
                {
                    layerName =
                        currentName;
                }
            }
            catch
            {
            }

            // -----------------------------------------------------
            // Hiện thẻ thông tin ở bên phải bản đồ.
            // -----------------------------------------------------
            ShowObjectInfoCard(
                "don_vi_quan_doi",
                layerName,
                shapeIndex,
                pointX,
                pointY,
                attributes);

            if (lblStatus != null)
            {
                string tenDonVi = "";

                attributes.TryGetValue(
                    "ten_don_vi",
                    out tenDonVi);

                lblStatus.Text =
                    !string.IsNullOrWhiteSpace(
                        tenDonVi)
                        ? "Đã chọn đơn vị: " +
                          tenDonVi
                        : "Đã chọn một đơn vị quân đội.";
            }
        }


        // =========================================================
        // THÔNG TIN ĐỐI TƯỢNG
        //
        // Ưu tiên tuyệt đối lớp Đơn vị quân đội.
        // Mục tiêu: khi người dùng bấm biểu tượng "i" rồi bấm chấm đỏ,
        // MapWinGIS phải nhận đúng Point đơn vị, không nhận polygon
        // ranh giới hành chính ở phía dưới.
        // =========================================================
        private void btnInfo_Click(
            object sender,
            EventArgs e)
        {
            RefreshLayerCombo();

            if (_loadedLayers == null ||
                _loadedLayers.Count == 0)
            {
                MessageBox.Show(
                    "Chưa có lớp dữ liệu trên bản đồ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string layerKey = null;

            if (_loadedLayers.ContainsKey(
                "don_vi_quan_doi"))
            {
                layerKey =
                    "don_vi_quan_doi";

                try
                {
                    SelectWorkingLayer(
                        layerKey);
                }
                catch
                {
                    if (cboCurrentLayer != null &&
                        cboCurrentLayer.Items.Contains(
                            layerKey))
                    {
                        cboCurrentLayer.SelectedItem =
                            layerKey;
                    }
                }
            }
            else
            {
                layerKey =
                    cboCurrentLayer != null
                        ? cboCurrentLayer.SelectedItem as string
                        : null;

                if (string.IsNullOrWhiteSpace(
                        layerKey)
                    ||
                    !_loadedLayers.ContainsKey(
                        layerKey))
                {
                    foreach (string key
                        in _loadedLayers.Keys)
                    {
                        layerKey =
                            key;

                        break;
                    }
                }
            }

            int targetHandle =
                -1;

            if (!string.IsNullOrWhiteSpace(
                    layerKey)
                &&
                _loadedLayers.ContainsKey(
                    layerKey))
            {
                targetHandle =
                    _loadedLayers[layerKey];
            }

            foreach (KeyValuePair<string, int> kv
                in _loadedLayers)
            {
                try
                {
                    Shapefile sf =
                        axMap2.get_Shapefile(
                            kv.Value);

                    if (sf != null)
                    {
                        sf.Identifiable =
                            kv.Value ==
                            targetHandle;
                    }
                }
                catch
                {
                }
            }

            try
            {
                axMap2.Identifier.IdentifierMode =
                    tkIdentifierMode.imAllLayers;

                axMap2.CursorMode =
                    tkCursorMode.cmIdentify;

                axMap2.Focus();
            }
            catch
            {
            }

            if (lblStatus != null)
            {
                string displayName =
                    string.IsNullOrWhiteSpace(
                        layerKey)
                        ? "lớp hiện tại"
                        : GetPostGISDisplayName(
                            layerKey);

                lblStatus.Text =
                    "Thông tin đối tượng | " +
                    displayName +
                    " | Bấm vào chấm đỏ để xem hồ sơ.";
            }
        }


        private void btnQueryLocation_Click(object sender, EventArgs e)
        {
            RefreshLayerCombo();

            string layerKey =
                cboCurrentLayer.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(layerKey) ||
                !_loadedLayers.ContainsKey(layerKey))
            {
                MessageBox.Show(
                    "Chưa chọn lớp ở ô \"Lớp hiện tại\".",
                    "Chưa chọn lớp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            int targetHandle =
                _loadedLayers[layerKey];

            foreach (var kv in _loadedLayers)
            {
                try
                {
                    Shapefile sf =
                        axMap2.get_Shapefile(kv.Value);

                    if (sf != null)
                    {
                        sf.Identifiable =
                            kv.Value == targetHandle;
                    }
                }
                catch
                {
                }
            }

            axMap2.Identifier.IdentifierMode =
                tkIdentifierMode.imAllLayers;

            axMap2.CursorMode =
                tkCursorMode.cmIdentify;

            lblStatus.Text =
                "Truy vấn vị trí trên lớp \"" +
                layerKey +
                "\": bấm vào đối tượng.";
        }

        // =========================================================
        // ĐÃ NHẬN DIỆN ĐỐI TƯỢNG
        // =========================================================
        private void axMap2_ShapeIdentified(
            object sender,
            _DMapEvents_ShapeIdentifiedEvent e)
        {
            if (e == null ||
                e.layerHandle < 0 ||
                e.shapeIndex < 0)
            {
                return;
            }

            Shapefile sf = null;

            try
            {
                sf =
                    axMap2.get_Shapefile(
                        e.layerHandle);
            }
            catch
            {
            }

            if (sf == null ||
                e.shapeIndex >= sf.NumShapes)
            {
                return;
            }

            string layerName = "";

            try
            {
                layerName =
                    axMap2.get_LayerName(
                        e.layerHandle);
            }
            catch
            {
            }

            string layerKey =
                GetLayerKeyByHandle(
                    e.layerHandle);

            Dictionary<string, string> attributes =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            StringBuilder sbAll =
                new StringBuilder();

            // -----------------------------------------------------
            // Đọc toàn bộ thuộc tính từ Shapefile/OGR.
            // -----------------------------------------------------
            for (int f = 0;
                f < sf.NumFields;
                f++)
            {
                try
                {
                    Field field =
                        sf.get_Field(f);

                    if (field == null)
                    {
                        continue;
                    }

                    string fieldName =
                        field.Name ?? "";

                    object rawValue =
                        sf.get_CellValue(
                            f,
                            e.shapeIndex);

                    string value =
                        rawValue == null ||
                        rawValue == DBNull.Value
                            ? ""
                            : rawValue.ToString();

                    if (!attributes.ContainsKey(
                        fieldName))
                    {
                        attributes.Add(
                            fieldName,
                            value);
                    }

                    sbAll.AppendLine(
                        fieldName +
                        " = " +
                        value);
                }
                catch
                {
                }
            }

            // -----------------------------------------------------
            // Chọn sáng đối tượng vừa bấm.
            // -----------------------------------------------------
            try
            {
                sf.SelectNone();

                sf.set_ShapeSelected(
                    e.shapeIndex,
                    true);

                axMap2.Redraw();
            }
            catch
            {
                try
                {
                    sf.ShapeSelected[
                        e.shapeIndex] =
                        true;

                    axMap2.Redraw();
                }
                catch
                {
                }
            }

            // -----------------------------------------------------
            // Giữ cập nhật các ô cũ để không phá code tương thích.
            // Các ô này có thể đang ẩn trong giao diện mới.
            // -----------------------------------------------------
            try
            {
                if (txtLayerName != null)
                {
                    txtLayerName.Text =
                        layerName ?? "";
                }

                if (txtObjectId != null)
                {
                    txtObjectId.Text =
                        GetAttributeValue(
                            attributes,
                            "id",
                            e.shapeIndex.ToString());
                }

                if (txtXY != null)
                {
                    txtXY.Text =
                        e.pointX.ToString(
                            "F3") +
                        " ; " +
                        e.pointY.ToString(
                            "F3");
                }

                if (cboAttributes != null)
                {
                    cboAttributes.Items.Clear();

                    foreach (KeyValuePair<string, string> kv
                        in attributes)
                    {
                        cboAttributes.Items.Add(
                            kv.Key +
                            " = " +
                            kv.Value);
                    }

                    if (cboAttributes.Items.Count > 0)
                    {
                        cboAttributes.SelectedIndex =
                            0;
                    }
                }

                if (txtDescription != null)
                {
                    txtDescription.Text =
                        sbAll.ToString();
                }
            }
            catch
            {
            }

            // -----------------------------------------------------
            // HIỂN THỊ THẺ THÔNG TIN MỚI TRÊN BẢN ĐỒ.
            // Đây là phần thay cho panel truy vấn cũ đã bị ẩn.
            // -----------------------------------------------------
            ShowObjectInfoCard(
                layerKey,
                layerName,
                e.shapeIndex,
                e.pointX,
                e.pointY,
                attributes);

            if (lblStatus != null)
            {
                string displayName =
                    !string.IsNullOrWhiteSpace(
                        layerKey)
                        ? GetPostGISDisplayName(
                            layerKey)
                        : layerName;

                lblStatus.Text =
                    "Đã chọn đối tượng trên lớp " +
                    displayName +
                    ".";
            }
        }


        // =========================================================
        // LẤY KEY LỚP THEO HANDLE
        // =========================================================
        private string GetLayerKeyByHandle(
            int layerHandle)
        {
            if (_loadedLayers == null)
            {
                return "";
            }

            foreach (KeyValuePair<string, int> kv
                in _loadedLayers)
            {
                if (kv.Value ==
                    layerHandle)
                {
                    return kv.Key;
                }
            }

            return "";
        }


        // =========================================================
        // LẤY GIÁ TRỊ THUỘC TÍNH KHÔNG PHÂN BIỆT HOA/THƯỜNG
        // =========================================================
        private string GetAttributeValue(
            Dictionary<string, string> attributes,
            string fieldName,
            string fallback)
        {
            if (attributes == null ||
                string.IsNullOrWhiteSpace(
                    fieldName))
            {
                return fallback ?? "";
            }

            string value;

            if (attributes.TryGetValue(
                fieldName,
                out value))
            {
                return value ?? "";
            }

            return fallback ?? "";
        }


        // =========================================================
        // BẢNG THUỘC TÍNH - LUỒNG MƯỢT
        //
        // Không yêu cầu người dùng phải chọn "Lớp hiện tại" trước.
        // Nếu chưa có lớp làm việc, hệ thống tự dùng Đơn vị quân đội.
        // =========================================================
        private void btnQueryAttribute_Click(
            object sender,
            EventArgs e)
        {
            OpenAttributeTableSmooth();
        }

        private string ShowQueryInputDialog(
            string title,
            string prompt)
        {
            using (Form form = new Form())
            using (System.Windows.Forms.Label label = new System.Windows.Forms.Label())
            using (TextBox textBox = new TextBox())
            using (Button buttonOk = new Button())
            using (Button buttonCancel = new Button())
            {
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.ClientSize = new Size(460, 160);

                label.Text = prompt;
                label.SetBounds(12, 12, 436, 60);
                label.AutoSize = false;

                textBox.SetBounds(12, 76, 436, 24);

                buttonOk.Text = "OK";
                buttonOk.DialogResult = DialogResult.OK;
                buttonOk.SetBounds(292, 118, 75, 28);

                buttonCancel.Text = "Hủy";
                buttonCancel.DialogResult = DialogResult.Cancel;
                buttonCancel.SetBounds(373, 118, 75, 28);

                form.Controls.AddRange(
                    new Control[]
                    {
                        label,
                        textBox,
                        buttonOk,
                        buttonCancel
                    });

                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog(this) == DialogResult.OK
                    ? textBox.Text
                    : null;
            }
        }
    }
}