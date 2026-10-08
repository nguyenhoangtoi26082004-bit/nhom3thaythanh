using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;
using AxMapWinGIS;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        // =========================================================
        // MODULE BIÊN TẬP HÌNH HỌC - HÀ NỘI ONLY
        //
        // CRS toàn hệ thống: EPSG:32648.
        //
        // Luồng:
        // 1. Đơn vị quân đội:
        //    - nhập thuộc tính;
        //    - chọn xã/phường;
        //    - tự gán Point đại diện hành chính bằng ST_PointOnSurface();
        //    - KHÔNG cho người dùng tự chấm vị trí cơ sở.
        //
        // 2. Tuyến đường / Khu vực:
        //    - nhập thuộc tính nhưng CHƯA lưu CSDL;
        //    - bấm "Tiếp tục vẽ";
        //    - số hóa trực tiếp trên MapWinGIS;
        //    - chỉ khi geometry hợp lệ và click phải hoàn tất
        //      thì thuộc tính + geometry mới được lưu.
        //
        // Điều khiển khi số hóa:
        // - Line    : click trái thêm đỉnh, click phải -> hoàn tất & lưu.
        // - Polygon : click trái thêm đỉnh, click phải -> khép + lưu.
        // - Esc     : hủy toàn bộ dữ liệu tạm, CSDL không phát sinh record.
        // =========================================================

        private const short EDIT_MOUSE_LEFT =
            1;

        private const short EDIT_MOUSE_RIGHT =
            2;

        private bool _editingToolsInitialized =
            false;

        private bool _isDigitizing =
            false;

        private string _editingTableName =
            "";
        private Dictionary<string, object>
            _pendingEditingValues =
                new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase);

        private readonly List<EditingVertex>
            _editingVertices =
                new List<EditingVertex>();

        private int _editingDrawingHandle =
            -1;

        private readonly PostGISGeometryRepository
            _geometryRepository =
                new PostGISGeometryRepository();

        // =========================================================
        // KHỞI TẠO
        //
        // GỌI SAU InitMainMenu().
        // =========================================================
        private void InitEditingTools()
        {
            if (_editingToolsInitialized)
                return;

            if (axMap2 == null)
                return;

            try
            {
                // ---------------------------------------------
                // MapWinGIS ActiveX phải bật SendMouseDown.
                // ---------------------------------------------
                axMap2.SendMouseDown =
                    true;

                axMap2.MouseDownEvent -=
                    axMap2_Editing_MouseDownEvent;

                axMap2.MouseDownEvent +=
                    axMap2_Editing_MouseDownEvent;

                // ---------------------------------------------
                // ESC để hủy.
                // ---------------------------------------------
                KeyPreview =
                    true;

                KeyDown -=
                    Form1_Editing_KeyDown;

                KeyDown +=
                    Form1_Editing_KeyDown;

                // ---------------------------------------------
                // Bật menu Chỉnh sửa.
                // ---------------------------------------------
                ConfigureEditingMenu();

                _editingToolsInitialized =
                    true;
            }
            catch (Exception ex)
            {
                _editingToolsInitialized =
                    false;

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không khởi tạo được công cụ biên tập: " +
                        ex.Message;
                }
            }
        }

        // =========================================================
        // MENU CHỈNH SỬA
        // =========================================================
        private void ConfigureEditingMenu()
        {
            if (menuEdit == null)
                return;

            menuEdit.Text =
                "Quản lý";

            menuEdit.Enabled =
                true;

            menuEdit.Visible =
                true;

            menuEdit.DropDownItems.Clear();

            // -----------------------------------------------------
            // NHÓM THÊM MỚI
            // -----------------------------------------------------
            ToolStripMenuItem menuCreate =
                new ToolStripMenuItem(
                    "Thêm mới");

            AddMenuItem(
                menuCreate,
                "Thêm hồ sơ đơn vị quân đội...",
                delegate
                {
                    StartCreateSpatialObject(
                        "don_vi_quan_doi");
                });

            AddMenuItem(
                menuCreate,
                "Tuyến đường đơn vị...",
                delegate
                {
                    StartCreateSpatialObject(
                        "tuyen_duong_don_vi");
                });

            AddMenuItem(
                menuCreate,
                "Khu vực đơn vị...",
                delegate
                {
                    StartCreateSpatialObject(
                        "khu_vuc_don_vi");
                });

            menuEdit.DropDownItems.Add(
                menuCreate);

            menuEdit.DropDownItems.Add(
                new ToolStripSeparator());

            // Sửa / xóa được thực hiện trong bảng thuộc tính,
            // tránh tạo nhiều luồng CRUD khác nhau.
            AddMenuItem(
                menuEdit,
                "Quản lý lớp đang chọn...",
                delegate
                {
                    OpenSelectedLayerAttributeTable();
                });

            menuEdit.DropDownItems.Add(
                new ToolStripSeparator());

            AddMenuItem(
                menuEdit,
                "Hủy thao tác đang vẽ",
                delegate
                {
                    CancelSpatialEditing(
                        true,
                        true);
                },
                Keys.Escape);
        }

        // =========================================================
        // BẮT ĐẦU THÊM ĐỐI TƯỢNG
        //
        // Đơn vị quân đội:
        // - không số hóa thủ công Point;
        // - Point lấy từ xã/phường đã chọn.
        //
        // Tuyến / khu vực:
        // - tiếp tục số hóa trực tiếp.
        // =========================================================
        private void StartCreateSpatialObject(
            string tableName)
        {
            if (_isDigitizing)
            {
                MessageBox.Show(
                    "Đang có một thao tác vẽ chưa hoàn tất.\n\n" +
                    "Hãy hoàn tất hoặc nhấn Esc để hủy trước.",
                    "Đang biên tập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool isUnit =
                string.Equals(
                    tableName,
                    "don_vi_quan_doi",
                    StringComparison.OrdinalIgnoreCase);

            if (!isUnit &&
                string.IsNullOrWhiteSpace(
                    EditingService.GetGeometryKind(
                        tableName)))
            {
                MessageBox.Show(
                    "Lớp dữ liệu này không hỗ trợ biên tập hình học.",
                    "Không hỗ trợ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            PostGISRepository repository =
                new PostGISRepository();

            // -----------------------------------------------------
            // ĐƠN VỊ:
            // vẫn lưu hồ sơ rồi tạo Point đại diện hành chính tự động.
            // Không cho chấm vị trí cơ sở thủ công.
            // -----------------------------------------------------
            if (isUnit)
            {
                int savedRecordId =
                    -1;

                using (EditObjectForm form =
                    new EditObjectForm(
                        repository,
                        tableName,
                        null,
                        false))
                {
                    if (form.ShowDialog(this) !=
                        DialogResult.OK)
                    {
                        return;
                    }

                    if (form.SavedRecordId <= 0)
                    {
                        MessageBox.Show(
                            "Không xác định được ID bản ghi vừa tạo.",
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    savedRecordId =
                        form.SavedRecordId;
                }

                try
                {
                    AssignUnitAdministrativeRepresentativePoint(
                        savedRecordId);

                    RefreshPostGISLayerAfterEdit(
                        "don_vi_quan_doi");

                    if (cboCurrentLayer != null &&
                        cboCurrentLayer.Items.Contains(
                            "don_vi_quan_doi"))
                    {
                        cboCurrentLayer.SelectedItem =
                            "don_vi_quan_doi";
                    }

                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Đã thêm đơn vị ID = " +
                            savedRecordId +
                            " và tạo Point đại diện hành chính.";
                    }

                    MessageBox.Show(
                        "Đã thêm hồ sơ đơn vị thành công.\n\n" +
                        "Point trên bản đồ là điểm đại diện hành chính " +
                        "của xã/phường đã chọn.",
                        "Đã thêm đơn vị",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    try
                    {
                        repository.DeleteRecord(
                            "don_vi_quan_doi",
                            savedRecordId);
                    }
                    catch
                    {
                    }

                    MessageBox.Show(
                        "Không thể tạo Point đại diện hành chính cho đơn vị.\n\n" +
                        ex.Message +
                        "\n\nBản ghi vừa thêm đã được hủy.",
                        "Lỗi tạo Point đại diện",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return;
            }

            // -----------------------------------------------------
            // TUYẾN / KHU VỰC - BƯỚC 1:
            //
            // Form chỉ kiểm tra và trả thuộc tính tạm.
            // CHƯA INSERT vào PostgreSQL.
            // -----------------------------------------------------
            Dictionary<string, object> pendingValues =
                null;

            using (EditObjectForm form =
                new EditObjectForm(
                    repository,
                    tableName,
                    null,
                    true))
            {
                if (form.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                if (form.PendingValues == null ||
                    form.PendingValues.Count == 0)
                {
                    MessageBox.Show(
                        "Không lấy được thông tin thuộc tính để chuyển sang bước vẽ.",
                        "Thiếu dữ liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                pendingValues =
                    new Dictionary<string, object>(
                        form.PendingValues,
                        StringComparer.OrdinalIgnoreCase);
            }

            // -----------------------------------------------------
            // BƯỚC 2:
            // bắt đầu số hóa. Vẫn CHƯA có bản ghi trong CSDL.
            // -----------------------------------------------------
            _editingTableName =
                tableName;
            _pendingEditingValues =
                pendingValues;

            _editingVertices.Clear();

            _isDigitizing =
                true;

            ClearEditingDrawing();

            try
            {
                axMap2.CursorMode =
                    tkCursorMode.cmNone;

                axMap2.MapCursor =
                    tkCursor.crsrCross;
            }
            catch
            {
            }

            string displayName =
                GetPostGISDisplayName(
                    tableName);

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Bước 2/2 - CHƯA LƯU | " +
                    displayName +
                    ": " +
                    EditingService.GetInstruction(
                        tableName) +
                    " | Click phải để hoàn tất & lưu | Esc để hủy";
            }

            MessageBox.Show(
                "BƯỚC 2/2 - VẼ HÌNH HỌC\n\n" +
                "Dữ liệu CHƯA được lưu vào CSDL.\n\n" +
                "• Click trái: thêm đỉnh.\n" +
                "• Click phải: kết thúc, kiểm tra geometry và LƯU.\n" +
                "• Esc: hủy toàn bộ thao tác.\n\n" +
                "Chỉ khi vẽ hợp lệ thì thuộc tính và geometry mới được lưu.",
                "Tiếp tục vẽ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // GÁN POINT ĐẠI DIỆN HÀNH CHÍNH CHO ĐƠN VỊ
        //
        // Point được lấy từ polygon xã/phường đã chọn bằng:
        // ST_PointOnSurface(xp.geom)
        //
        // Không sử dụng tọa độ cơ sở chính xác.
        // =========================================================
        private void AssignUnitAdministrativeRepresentativePoint(
            int unitId)
        {
            if (unitId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "unitId");
            }

            NpgsqlConnectionStringBuilder builder =
                new NpgsqlConnectionStringBuilder();

            builder.Host =
                PG_HOST;

            builder.Port =
                Convert.ToInt32(
                    PG_PORT);

            builder.Database =
                PG_DB;

            builder.Username =
                PG_USER;

            builder.Password =
                PG_PASS;

            builder.Timeout =
                5;

            builder.CommandTimeout =
                20;

            string sql =
                "UPDATE " +
                "\"ql_don_vi\".\"don_vi_quan_doi\" dv " +
                "SET geom = ST_PointOnSurface(xp.geom), " +
                "    ngay_cap_nhat = CURRENT_TIMESTAMP " +
                "FROM \"ql_don_vi\".\"dm_xa_phuong\" xp " +
                "WHERE dv.id = @id " +
                "  AND xp.id = dv.xa_phuong_id " +
                "  AND xp.geom IS NOT NULL;";

            using (NpgsqlConnection connection =
                new NpgsqlConnection(
                    builder.ConnectionString))
            using (NpgsqlCommand command =
                new NpgsqlCommand(
                    sql,
                    connection))
            {
                command.Parameters.Add(
                    "@id",
                    NpgsqlDbType.Bigint).Value =
                    Convert.ToInt64(
                        unitId);

                connection.Open();

                int affected =
                    command.ExecuteNonQuery();

                if (affected != 1)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy xã/phường có geometry hợp lệ " +
                        "cho đơn vị vừa tạo.");
                }
            }
        }

        // =========================================================
        // MOUSE DOWN TRÊN MAP
        // =========================================================
        private void axMap2_Editing_MouseDownEvent(
            object sender,
            _DMapEvents_MouseDownEvent e)
        {
            if (!_isDigitizing)
                return;

            // ---------------------------------------------
            // CLICK TRÁI -> THÊM ĐỈNH
            // ---------------------------------------------
            if (e.button ==
                EDIT_MOUSE_LEFT)
            {
                AddEditingVertex(
                    e.x,
                    e.y);

                return;
            }

            // ---------------------------------------------
            // CLICK PHẢI -> HOÀN TẤT LINE/POLYGON
            // Đơn vị quân đội không đi vào chế độ số hóa.
            // ---------------------------------------------
            if (e.button ==
                EDIT_MOUSE_RIGHT)
            {
                string kind =
                    EditingService.GetGeometryKind(
                        _editingTableName);

                if (kind == "POINT")
                    return;

                FinishSpatialEditing();
            }
        }

        // =========================================================
        // THÊM ĐỈNH
        // =========================================================
        private void AddEditingVertex(
            int pixelX,
            int pixelY)
        {
            double mapX =
                0.0;

            double mapY =
                0.0;

            try
            {
                axMap2.PixelToProj(
                    pixelX,
                    pixelY,
                    ref mapX,
                    ref mapY);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không chuyển được tọa độ chuột sang tọa độ bản đồ.\n\n" +
                    ex.Message,
                    "Lỗi tọa độ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _editingVertices.Add(
                new EditingVertex(
                    mapX,
                    mapY));

            string kind =
                EditingService.GetGeometryKind(
                    _editingTableName);

            // Luồng hiện tại chỉ số hóa LineString / Polygon.
            // Giữ guard POINT để tránh thao tác ngoài dự kiến.
            if (kind == "POINT")
            {
                MessageBox.Show(
                    "Điểm đơn vị được tạo tự động từ xã/phường, " +
                    "không số hóa thủ công.",
                    "Không số hóa Point",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            RenderEditingDraft();

            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Đang vẽ " +
                    GetPostGISDisplayName(
                        _editingTableName) +
                    " | Số đỉnh: " +
                    _editingVertices.Count +
                    " | CHƯA LƯU | Click phải: hoàn tất & lưu | Esc: hủy";
            }
        }

        // =========================================================
        // VẼ TẠM CÁC ĐỈNH / ĐƯỜNG / VÙNG
        // =========================================================
        private void RenderEditingDraft()
        {
            ClearEditingDrawing();

            if (_editingVertices.Count == 0)
                return;

            try
            {
                _editingDrawingHandle =
                    axMap2.NewDrawing(
                        tkDrawReferenceList
                            .dlSpatiallyReferencedList);

                Utils utils =
                    new Utils();

                uint pointColor =
                    utils.ColorByName(
                        tkMapColor.Red);

                uint lineColor =
                    utils.ColorByName(
                        tkMapColor.Blue);

                uint polygonColor =
                    utils.ColorByName(
                        tkMapColor.Yellow);

                // ---------------------------------------------
                // Vẽ từng đỉnh.
                // ---------------------------------------------
                foreach (EditingVertex vertex
                    in _editingVertices)
                {
                    axMap2.DrawPointEx(
                        _editingDrawingHandle,
                        vertex.X,
                        vertex.Y,
                        9,
                        pointColor,
                        255);
                }

                // ---------------------------------------------
                // Vẽ các đoạn nối.
                // ---------------------------------------------
                for (int i = 1;
                     i < _editingVertices.Count;
                     i++)
                {
                    EditingVertex a =
                        _editingVertices[i - 1];

                    EditingVertex b =
                        _editingVertices[i];

                    axMap2.DrawLineEx(
                        _editingDrawingHandle,
                        a.X,
                        a.Y,
                        b.X,
                        b.Y,
                        3,
                        lineColor,
                        255);
                }

                // ---------------------------------------------
                // Polygon từ 3 đỉnh trở lên:
                // hiển thị vùng tô trong suốt.
                // ---------------------------------------------
                string kind =
                    EditingService.GetGeometryKind(
                        _editingTableName);

                if (kind == "POLYGON" &&
                    _editingVertices.Count >= 3)
                {
                    double[] x =
                        new double[
                            _editingVertices.Count];

                    double[] y =
                        new double[
                            _editingVertices.Count];

                    for (int i = 0;
                         i < _editingVertices.Count;
                         i++)
                    {
                        x[i] =
                            _editingVertices[i].X;

                        y[i] =
                            _editingVertices[i].Y;
                    }

                    object xObject =
                        x;

                    object yObject =
                        y;

                    axMap2.DrawPolygonEx(
                        _editingDrawingHandle,
                        ref xObject,
                        ref yObject,
                        _editingVertices.Count,
                        polygonColor,
                        true,
                        70);
                }

                axMap2.Redraw();
            }
            catch
            {
                // Preview lỗi không làm mất dữ liệu đỉnh.
            }
        }

        // =========================================================
        // HOÀN TẤT VÀ LƯU POSTGIS - EPSG:32648
        // =========================================================
        private void FinishSpatialEditing()
        {
            if (!_isDigitizing)
                return;

            string error;

            if (!EditingService.CanFinish(
                _editingTableName,
                _editingVertices,
                out error))
            {
                MessageBox.Show(
                    error +
                    "\n\nDữ liệu chưa được lưu. Hãy tiếp tục vẽ cho hợp lệ.",
                    "Chưa thể lưu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (_pendingEditingValues == null ||
                _pendingEditingValues.Count == 0)
            {
                MessageBox.Show(
                    "Không còn dữ liệu thuộc tính tạm để lưu.\n\n" +
                    "Hãy hủy thao tác và tạo lại đối tượng.",
                    "Thiếu dữ liệu tạm",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            Cursor oldCursor =
                Cursor.Current;

            int insertedId =
                -1;

            try
            {
                Cursor.Current =
                    Cursors.WaitCursor;

                string completedTable =
                    _editingTableName;

                string wkt =
                    EditingService.BuildWkt(
                        completedTable,
                        _editingVertices);

                PostGISRepository repository =
                    new PostGISRepository();

                // -------------------------------------------------
                // CHỈ TỚI ĐÂY MỚI INSERT THUỘC TÍNH.
                // Geometry đã được kiểm tra hợp lệ trước đó.
                // -------------------------------------------------
                insertedId =
                    repository.InsertRecord(
                        completedTable,
                        _pendingEditingValues);

                if (insertedId <= 0)
                {
                    throw new InvalidOperationException(
                        "Không tạo được bản ghi thuộc tính mới.");
                }

                // -------------------------------------------------
                // LƯU GEOMETRY.
                // Nếu lỗi, catch bên dưới sẽ xóa record vừa INSERT.
                // -------------------------------------------------
                _geometryRepository.SaveGeometry(
                    completedTable,
                    insertedId,
                    wkt);
                int completedId =
                    insertedId;

                ResetEditingState();

                RefreshPostGISLayerAfterEdit(
                    completedTable);

                if (cboCurrentLayer != null &&
                    cboCurrentLayer.Items.Contains(
                        completedTable))
                {
                    cboCurrentLayer.SelectedItem =
                        completedTable;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã lưu hoàn chỉnh " +
                        GetPostGISDisplayName(
                            completedTable) +
                        " | ID = " +
                        completedId +
                        " | EPSG:32648.";
                }

                MessageBox.Show(
                    "ĐÃ LƯU THÀNH CÔNG\n\n" +
                    "Thuộc tính và geometry đã được lưu cùng một đối tượng.\n" +
                    "Geometry: EPSG:32648.",
                    "Hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Nếu đã INSERT nhưng geometry chưa lưu được,
                // xóa lại record để CSDL không có dữ liệu rác geom = NULL.
                if (insertedId > 0 &&
                    !string.IsNullOrWhiteSpace(
                        _editingTableName))
                {
                    try
                    {
                        PostGISRepository cleanupRepository =
                            new PostGISRepository();

                        cleanupRepository.DeleteRecord(
                            _editingTableName,
                            insertedId);
                    }
                    catch
                    {
                    }
                }
                MessageBox.Show(
                    "CHƯA LƯU ĐƯỢC ĐỐI TƯỢNG\n\n" +
                    ex.Message +
                    "\n\nKhông có bản ghi chưa hoàn chỉnh được giữ lại trong CSDL. " +
                    "Các đỉnh đang vẽ vẫn còn để anh kiểm tra và thử lưu lại.",
                    "Lỗi lưu dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Lưu thất bại | Dữ liệu CHƯA được ghi hoàn chỉnh vào CSDL.";
                }
            }
            finally
            {
                Cursor.Current =
                    oldCursor;
            }
        }

        // =========================================================
        // ESC
        // =========================================================
        private void Form1_Editing_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (!_isDigitizing)
                return;

            if (e.KeyCode ==
                Keys.Escape)
            {
                e.Handled =
                    true;

                e.SuppressKeyPress =
                    true;

                CancelSpatialEditing(
                    true,
                    true);
            }
        }

        // =========================================================
        // HỦY BIÊN TẬP
        //
        // Luồng hiện tại của Tuyến/Khu vực chỉ INSERT sau khi
        // geometry hợp lệ, vì vậy khi nhấn Esc chưa có bản ghi tạm
        // trong CSDL cần xóa. Tham số deletePendingRecord được giữ
        // lại để tương thích các lời gọi hiện có.
        // =========================================================
        private void CancelSpatialEditing(
            bool deletePendingRecord,
            bool showMessage)
        {
            if (!_isDigitizing)
            {
                if (showMessage &&
                    lblStatus != null)
                {
                    lblStatus.Text =
                        "Không có thao tác vẽ nào đang thực hiện.";
                }

                return;
            }

            string oldTable =
                _editingTableName;

            // Tuyến/khu vực mới chưa được INSERT trong lúc vẽ,
            // nên Esc chỉ cần xóa dữ liệu tạm trên RAM.
            ResetEditingState();

            if (showMessage)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã hủy thao tác. Không có dữ liệu mới được lưu vào CSDL.";
                }

                MessageBox.Show(
                    "Đã hủy thao tác vẽ.\n\n" +
                    "Thông tin nhập và geometry tạm đã bị bỏ. " +
                    "CSDL không phát sinh bản ghi mới.",
                    "Đã hủy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (!string.IsNullOrWhiteSpace(
                oldTable))
            {
                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // RESET TRẠNG THÁI
        // =========================================================
        private void ResetEditingState()
        {
            ClearEditingDrawing();

            _editingVertices.Clear();

            _editingTableName =
                "";
            _pendingEditingValues =
                new Dictionary<string, object>(
                    StringComparer.OrdinalIgnoreCase);

            _isDigitizing =
                false;

            try
            {
                axMap2.CursorMode =
                    tkCursorMode.cmPan;

                axMap2.MapCursor =
                    tkCursor.crsrMapDefault;
            }
            catch
            {
            }
        }

        // =========================================================
        // XÓA DRAWING PREVIEW
        // =========================================================
        private void ClearEditingDrawing()
        {
            if (axMap2 == null)
                return;

            if (_editingDrawingHandle >= 0)
            {
                try
                {
                    axMap2.ClearDrawing(
                        _editingDrawingHandle);
                }
                catch
                {
                }

                _editingDrawingHandle =
                    -1;
            }
        }

        // =========================================================
        // DỪNG MODULE KHI ĐÓNG FORM
        // =========================================================
        private void StopEditingTools()
        {
            try
            {
                if (_isDigitizing)
                {
                    CancelSpatialEditing(
                        true,
                        false);
                }

                if (axMap2 != null)
                {
                    axMap2.MouseDownEvent -=
                        axMap2_Editing_MouseDownEvent;
                }

                KeyDown -=
                    Form1_Editing_KeyDown;
            }
            catch
            {
            }

            _editingToolsInitialized =
                false;
        }
    }
}