using System;
using System.Collections.Generic;
using System.Data;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // =============================================================
    // ATTRIBUTE TABLE SERVICE
    //
    // ĐỀ TÀI:
    // "Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
    //  các đơn vị quân đội trên địa bàn Hà Nội"
    //
    // Nhiệm vụ:
    // - Đọc thuộc tính từ lớp MapWinGIS/Shapefile;
    // - Chuẩn hóa tên trường tiếng Việt;
    // - Tìm kiếm nhanh trong DataTable;
    // - Liên kết dòng thuộc tính với đối tượng trên bản đồ;
    // - Hỗ trợ đủ Point / LineString / Polygon / MultiPolygon:
    //      don_vi_quan_doi
    //      tuyen_duong_don_vi
    //      khu_vuc_don_vi
    //      dia_gioi_hanh_chinh
    //      xa_phuong_ha_noi
    //
    // Mô hình hành chính hiện hành:
    // Thành phố Hà Nội -> Xã/Phường.
    // Không dùng Quận/Huyện trong dữ liệu nghiệp vụ.
    // =============================================================
    public static class AttributeTableService
    {
        public const string ShapeIndexColumn =
            "__shape_index";

        // =========================================================
        // ĐỌC BẢNG THUỘC TÍNH TỪ LAYER MAPWINGIS
        //
        // Dùng cho:
        // - Shapefile cục bộ;
        // - lớp PostGIS không quản lý CRUD trực tiếp,
        //   ví dụ ranh giới Hà Nội và 126 xã/phường.
        // =========================================================
        public static DataTable BuildDataTable(
            Shapefile shapefile)
        {
            DataTable table =
                new DataTable();

            table.Columns.Add(
                new DataColumn(
                    ShapeIndexColumn,
                    typeof(int)));

            if (shapefile == null)
                return table;

            for (int fieldIndex = 0;
                 fieldIndex < shapefile.NumFields;
                 fieldIndex++)
            {
                Field field =
                    shapefile.get_Field(
                        fieldIndex);

                string fieldName =
                    field != null
                        ? field.Name
                        : "field_" + fieldIndex;

                if (string.IsNullOrWhiteSpace(
                    fieldName))
                {
                    fieldName =
                        "field_" + fieldIndex;
                }

                fieldName =
                    MakeUniqueColumnName(
                        table,
                        fieldName);

                table.Columns.Add(
                    new DataColumn(
                        fieldName,
                        typeof(string)));
            }

            for (int shapeIndex = 0;
                 shapeIndex < shapefile.NumShapes;
                 shapeIndex++)
            {
                DataRow row =
                    table.NewRow();

                row[ShapeIndexColumn] =
                    shapeIndex;

                for (int fieldIndex = 0;
                     fieldIndex < shapefile.NumFields;
                     fieldIndex++)
                {
                    object value = null;

                    try
                    {
                        value =
                            shapefile.get_CellValue(
                                fieldIndex,
                                shapeIndex);
                    }
                    catch
                    {
                    }

                    row[fieldIndex + 1] =
                        value == null ||
                        value == DBNull.Value
                            ? ""
                            : value.ToString();
                }

                table.Rows.Add(
                    row);
            }

            return table;
        }

        // =========================================================
        // TÊN HIỂN THỊ TIẾNG VIỆT
        // =========================================================
        public static string GetFriendlyFieldName(
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(
                fieldName))
            {
                return "";
            }

            switch (fieldName.ToLowerInvariant())
            {
                // -------------------------------------------------
                // KHÓA / TRƯỜNG CHUNG
                // -------------------------------------------------
                case "id":
                    return "ID";

                case "ngay_tao":
                    return "Ngày tạo";

                case "ngay_cap_nhat":
                    return "Ngày cập nhật";

                case "mo_ta":
                    return "Mô tả";

                case "ghi_chu":
                    return "Ghi chú";

                case "tinh_thanh_pho":
                    return "Tỉnh/Thành phố";

                // -------------------------------------------------
                // ĐƠN VỊ QUÂN ĐỘI
                // -------------------------------------------------
                case "ma_don_vi":
                    return "Mã đơn vị";

                case "ten_don_vi":
                    return "Tên đơn vị";

                case "cap_don_vi_id":
                    return "ID cấp đơn vị";

                case "ma_cap":
                    return "Mã cấp";

                case "ten_cap":
                    return "Cấp đơn vị";

                case "loai_don_vi_id":
                    return "ID loại đơn vị";

                case "ma_loai":
                    return "Mã loại";

                case "ten_loai":
                    return "Loại đơn vị";

                case "don_vi_cap_tren_id":
                    return "ID đơn vị cấp trên";

                case "ten_don_vi_cap_tren":
                    return "Đơn vị cấp trên";

                case "xa_phuong_id":
                    return "ID xã/phường";

                case "ma_xa_phuong":
                    return "Mã xã/phường";

                case "ten_xa_phuong":
                    return "Xã/Phường";

                case "loai_xa_phuong":
                    return "Loại xã/phường";

                case "dia_chi_hien_hanh":
                    return "Địa chỉ hiện hành";

                case "ky_hieu_bien_so":
                    return "Ký hiệu";

                case "thong_tin_lien_he":
                    return "Thông tin liên hệ";

                case "nguon_tham_khao":
                    return "Nguồn tham khảo";

                case "ngay_xac_minh":
                    return "Ngày xác minh";

                case "muc_do_xac_minh":
                    return "Mức độ xác minh";

                case "trang_thai_quan_ly":
                    return "Trạng thái quản lý";

                case "trang_thai_geometry":
                    return "Trạng thái hình học";

                // -------------------------------------------------
                // TUYẾN ĐƯỜNG ĐƠN VỊ
                // -------------------------------------------------
                case "don_vi_id":
                    return "ID đơn vị";

                case "ma_tuyen":
                    return "Mã tuyến";

                case "ten_tuyen":
                    return "Tên tuyến";

                case "loai_tuyen":
                    return "Loại tuyến";

                case "cap_duong":
                    return "Cấp đường";

                case "be_rong_m":
                    return "Bề rộng (m)";

                case "chieu_dai_m":
                    return "Chiều dài (m)";

                // -------------------------------------------------
                // KHU VỰC ĐƠN VỊ
                // -------------------------------------------------
                case "ma_khu_vuc":
                    return "Mã khu vực";

                case "ten_khu_vuc":
                    return "Tên khu vực";

                case "loai_khu_vuc":
                    return "Loại khu vực";

                case "dien_tich_m2":
                    return "Diện tích (m²)";

                case "chu_vi_m":
                    return "Chu vi (m)";

                // -------------------------------------------------
                // RANH GIỚI THÀNH PHỐ HÀ NỘI
                // -------------------------------------------------
                case "ma_hanh_chinh":
                    return "Mã hành chính";

                case "ten_hanh_chinh":
                    return "Tên hành chính";

                case "cap_hanh_chinh":
                    return "Cấp hành chính";

                // -------------------------------------------------
                // DỮ LIỆU XÃ / PHƯỜNG
                // -------------------------------------------------
                case "loai":
                    return "Loại xã/phường";

                case "ten_hien_thi":
                    return "Tên hiển thị";

                case "ma_danh_muc":
                    return "Mã danh mục";

                default:
                    return fieldName;
            }
        }

        // =========================================================
        // CHUYỂN MÃ TRẠNG THÁI THÀNH TIẾNG VIỆT
        // =========================================================
        public static string GetFriendlyStatus(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string status =
                value.ToString();

            if (string.Equals(
                status,
                "dang_quan_ly",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Đang quản lý";
            }

            if (string.Equals(
                status,
                "tam_ngung",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Tạm ngừng";
            }

            if (string.Equals(
                status,
                "khong_con_quan_ly",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Không còn quản lý";
            }

            return status;
        }


        // =========================================================
        // CHUYỂN LOẠI XÃ / PHƯỜNG THÀNH TIẾNG VIỆT
        // =========================================================
        public static string GetFriendlyAdministrativeType(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string code =
                value.ToString();

            if (string.Equals(
                code,
                "phuong",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Phường";
            }

            if (string.Equals(
                code,
                "xa",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Xã";
            }

            if (string.Equals(
                code,
                "thanh_pho",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Thành phố";
            }

            return code;
        }

        // =========================================================
        // CHUYỂN MỨC ĐỘ XÁC MINH THÀNH TÊN DỄ ĐỌC
        // =========================================================
        public static string GetFriendlyVerificationLevel(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string code =
                value.ToString();

            if (string.Equals(
                code,
                "tham_chieu_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Tham chiếu hành chính";
            }

            return code;
        }

        // =========================================================
        // CHUYỂN MÃ LOẠI TUYẾN THÀNH TIẾNG VIỆT
        // =========================================================
        public static string GetFriendlyRouteType(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string code = value.ToString();

            if (string.Equals(
                code,
                "duong_vao",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Đường vào đơn vị";
            }

            if (string.Equals(
                code,
                "duong_tuan_tra",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Đường tuần tra";
            }

            if (string.Equals(
                code,
                "khac",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khác";
            }

            return code;
        }

        // =========================================================
        // CHUYỂN MÃ LOẠI KHU VỰC THÀNH TIẾNG VIỆT
        // =========================================================
        public static string GetFriendlyAreaType(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string code = value.ToString();

            if (string.Equals(
                code,
                "ranh_gioi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Ranh giới đơn vị";
            }

            if (string.Equals(
                code,
                "khu_chuc_nang",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khu chức năng";
            }

            if (string.Equals(
                code,
                "khu_ky_thuat",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khu kỹ thuật";
            }

            if (string.Equals(
                code,
                "khu_hau_can",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khu hậu cần";
            }

            if (string.Equals(
                code,
                "khac",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khác";
            }

            return code;
        }

        // =========================================================
        // CHỌN SHAPE THEO SHAPE INDEX
        // =========================================================
        public static bool SelectShape(
            AxMapWinGIS.AxMap map,
            int layerHandle,
            int shapeIndex,
            bool zoomToShape)
        {
            if (map == null ||
                layerHandle < 0 ||
                shapeIndex < 0)
            {
                return false;
            }

            try
            {
                Shapefile sf =
                    map.get_Shapefile(
                        layerHandle);

                if (sf == null ||
                    shapeIndex >= sf.NumShapes)
                {
                    return false;
                }

                sf.SelectNone();

                sf.set_ShapeSelected(
                    shapeIndex,
                    true);

                if (zoomToShape)
                {
                    map.ZoomToSelected(
                        layerHandle);
                }

                map.Redraw();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // CHỌN SHAPE THEO ID DATABASE
        //
        // Dùng cho bảng đơn vị quân đội đọc trực tiếp từ PostGIS.
        // =========================================================
        public static bool SelectShapeByDatabaseId(
            AxMapWinGIS.AxMap map,
            int layerHandle,
            int databaseId,
            bool zoomToShape)
        {
            if (map == null ||
                layerHandle < 0)
            {
                return false;
            }

            try
            {
                Shapefile sf =
                    map.get_Shapefile(
                        layerHandle);

                if (sf == null)
                    return false;

                int idFieldIndex =
                    FindFieldIndex(
                        sf,
                        "id");

                if (idFieldIndex < 0)
                    return false;

                int targetShapeIndex =
                    -1;

                for (int i = 0;
                     i < sf.NumShapes;
                     i++)
                {
                    object value =
                        sf.get_CellValue(
                            idFieldIndex,
                            i);

                    if (value == null ||
                        value == DBNull.Value)
                    {
                        continue;
                    }

                    long id;

                    if (long.TryParse(
                        value.ToString(),
                        out id)
                        &&
                        id == Convert.ToInt64(
                            databaseId))
                    {
                        targetShapeIndex =
                            i;

                        break;
                    }
                }

                if (targetShapeIndex < 0)
                    return false;

                return SelectShape(
                    map,
                    layerHandle,
                    targetShapeIndex,
                    zoomToShape);
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // BỎ CHỌN TRÊN MỘT LỚP
        // =========================================================
        public static void ClearSelection(
            AxMapWinGIS.AxMap map,
            int layerHandle)
        {
            if (map == null ||
                layerHandle < 0)
            {
                return;
            }

            try
            {
                Shapefile sf =
                    map.get_Shapefile(
                        layerHandle);

                if (sf == null)
                    return;

                sf.SelectNone();
                map.Redraw();
            }
            catch
            {
            }
        }

        // =========================================================
        // LẤY SHAPE INDEX TỪ DATAROW
        // =========================================================
        public static int GetShapeIndex(
            DataRowView rowView)
        {
            if (rowView == null ||
                rowView.Row == null ||
                !rowView.Row.Table.Columns.Contains(
                    ShapeIndexColumn))
            {
                return -1;
            }

            try
            {
                return Convert.ToInt32(
                    rowView[ShapeIndexColumn]);
            }
            catch
            {
                return -1;
            }
        }

        // =========================================================
        // LẤY ID DATABASE
        // =========================================================
        public static int GetDatabaseId(
            DataRowView rowView)
        {
            if (rowView == null ||
                rowView.Row == null ||
                !rowView.Row.Table.Columns.Contains(
                    "id"))
            {
                return -1;
            }

            try
            {
                return Convert.ToInt32(
                    rowView["id"]);
            }
            catch
            {
                return -1;
            }
        }

        // =========================================================
        // TẠO BIỂU THỨC TÌM NHANH
        //
        // Tìm trên toàn bộ cột đang có trong bảng.
        // =========================================================
        public static string BuildFilterExpression(
            DataTable table,
            string keyword)
        {
            if (table == null ||
                string.IsNullOrWhiteSpace(
                    keyword))
            {
                return "";
            }

            string safeKeyword =
                keyword
                    .Trim()
                    .Replace(
                        "'",
                        "''")
                    .Replace(
                        "[",
                        "[[]")
                    .Replace(
                        "%",
                        "[%]")
                    .Replace(
                        "*",
                        "[*]");

            string expression =
                "";

            foreach (DataColumn column
                in table.Columns)
            {
                if (column.ColumnName ==
                    ShapeIndexColumn)
                {
                    continue;
                }

                string safeColumnName =
                    column.ColumnName.Replace(
                        "]",
                        "]]");

                string columnExpression;

                if (column.DataType ==
                    typeof(string))
                {
                    columnExpression =
                        "[" +
                        safeColumnName +
                        "]";
                }
                else
                {
                    columnExpression =
                        "Convert([" +
                        safeColumnName +
                        "], 'System.String')";
                }

                string part =
                    columnExpression +
                    " LIKE '%" +
                    safeKeyword +
                    "%'";

                if (expression.Length > 0)
                    expression += " OR ";

                expression += part;
            }

            return expression;
        }

        // =========================================================
        // PRO - LẤY GIÁ TRỊ HIỂN THỊ THỐNG NHẤT
        // =========================================================
        public static string GetFriendlyValue(
            string fieldName,
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "";
            }

            string name =
                fieldName ?? "";

            if (string.Equals(
                name,
                "trang_thai_quan_ly",
                StringComparison.OrdinalIgnoreCase))
            {
                return GetFriendlyStatus(
                    value);
            }

            if (string.Equals(
                name,
                "loai_xa_phuong",
                StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "loai",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "cap_hanh_chinh",
                    StringComparison.OrdinalIgnoreCase))
            {
                return GetFriendlyAdministrativeType(
                    value);
            }

            if (string.Equals(
                name,
                "muc_do_xac_minh",
                StringComparison.OrdinalIgnoreCase))
            {
                return GetFriendlyVerificationLevel(
                    value);
            }

            if (string.Equals(
                name,
                "loai_tuyen",
                StringComparison.OrdinalIgnoreCase))
            {
                return GetFriendlyRouteType(
                    value);
            }

            if (string.Equals(
                name,
                "loai_khu_vuc",
                StringComparison.OrdinalIgnoreCase))
            {
                return GetFriendlyAreaType(
                    value);
            }

            if (string.Equals(
                name,
                "ngay_tao",
                StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "ngay_cap_nhat",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "ngay_xac_minh",
                    StringComparison.OrdinalIgnoreCase))
            {
                DateTime dateValue;

                if (DateTime.TryParse(
                    value.ToString(),
                    out dateValue))
                {
                    if (string.Equals(
                        name,
                        "ngay_xac_minh",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return dateValue.ToString(
                            "dd/MM/yyyy");
                    }

                    return dateValue.ToString(
                        "dd/MM/yyyy HH:mm");
                }
            }

            if (string.Equals(
                name,
                "be_rong_m",
                StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "chieu_dai_m",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "chu_vi_m",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    name,
                    "dien_tich_m2",
                    StringComparison.OrdinalIgnoreCase))
            {
                double number;

                if (double.TryParse(
                    value.ToString(),
                    out number))
                {
                    return number.ToString(
                        "#,##0.##");
                }
            }

            return value.ToString();
        }

        // =========================================================
        // PRO - CỘT KỸ THUẬT KHÔNG NÊN CHIẾM CHỖ TRÊN GRID
        //
        // Dữ liệu vẫn còn trong DataTable và vẫn dùng được cho
        // CRUD/liên kết bản đồ; chỉ ẩn khỏi màn hình.
        // =========================================================
        public static bool IsTechnicalColumn(
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(
                fieldName))
            {
                return false;
            }

            string name =
                fieldName.ToLowerInvariant();

            return
                name == ShapeIndexColumn.ToLowerInvariant()
                ||
                name == "cap_don_vi_id"
                ||
                name == "loai_don_vi_id"
                ||
                name == "don_vi_cap_tren_id"
                ||
                name == "xa_phuong_id"
                ||
                name == "don_vi_id";
        }

        // =========================================================
        // PRO - CỘT PHÙ HỢP ĐỂ LỌC CHÍNH XÁC
        //
        // Chỉ lấy các trường chuỗi có ý nghĩa nghiệp vụ, tránh đưa
        // mô tả/ghi chú/địa chỉ dài vào ComboBox lọc.
        // =========================================================
        public static bool IsPreferredFilterField(
            DataColumn column)
        {
            if (column == null ||
                column.DataType != typeof(string))
            {
                return false;
            }

            string name =
                column.ColumnName.ToLowerInvariant();

            if (name == ShapeIndexColumn.ToLowerInvariant()
                ||
                name == "mo_ta"
                ||
                name == "ghi_chu"
                ||
                name == "thong_tin_lien_he"
                ||
                name == "nguon_tham_khao"
                ||
                name == "dia_chi_hien_hanh")
            {
                return false;
            }

            return true;
        }

        // =========================================================
        // PRO - DANH SÁCH CỘT DÙNG CHO BỘ LỌC
        // =========================================================
        public static string[] GetPreferredFilterFields(
            DataTable table)
        {
            if (table == null)
            {
                return new string[0];
            }

            List<string> result =
                new List<string>();

            foreach (DataColumn column
                in table.Columns)
            {
                if (IsPreferredFilterField(
                    column))
                {
                    result.Add(
                        column.ColumnName);
                }
            }

            return result.ToArray();
        }

        // =========================================================
        // PRO - TẠO BỘ LỌC KẾT HỢP
        //
        // keyword: tìm nhanh trên toàn bảng
        // exactField/exactValue: lọc chính xác theo một trường
        // =========================================================
        public static string BuildCombinedFilterExpression(
            DataTable table,
            string keyword,
            string exactField,
            string exactValue)
        {
            if (table == null)
            {
                return "";
            }

            string searchExpression =
                BuildFilterExpression(
                    table,
                    keyword);

            string exactExpression =
                BuildExactFilterExpression(
                    table,
                    exactField,
                    exactValue);

            if (string.IsNullOrWhiteSpace(
                searchExpression))
            {
                return exactExpression;
            }

            if (string.IsNullOrWhiteSpace(
                exactExpression))
            {
                return searchExpression;
            }

            return
                "(" +
                searchExpression +
                ") AND (" +
                exactExpression +
                ")";
        }

        // =========================================================
        // PRO - LỌC CHÍNH XÁC THEO GIÁ TRỊ CHUỖI
        // =========================================================
        public static string BuildExactFilterExpression(
            DataTable table,
            string fieldName,
            string rawValue)
        {
            if (table == null
                ||
                string.IsNullOrWhiteSpace(
                    fieldName)
                ||
                string.IsNullOrWhiteSpace(
                    rawValue)
                ||
                !table.Columns.Contains(
                    fieldName))
            {
                return "";
            }

            DataColumn column =
                table.Columns[fieldName];

            string safeColumnName =
                column.ColumnName.Replace(
                    "]",
                    "]]");

            string safeValue =
                rawValue
                    .Replace(
                        "'",
                        "''");

            if (column.DataType ==
                typeof(string))
            {
                return
                    "[" +
                    safeColumnName +
                    "] = '" +
                    safeValue +
                    "'";
            }

            return
                "Convert([" +
                safeColumnName +
                "], 'System.String') = '" +
                safeValue +
                "'";
        }

        // =========================================================
        // PRO - ĐỘ RỘNG CỘT THEO NGHIỆP VỤ
        // =========================================================
        public static int GetPreferredColumnWidth(
            string fieldName)
        {
            string name =
                (fieldName ?? "")
                    .ToLowerInvariant();

            if (name == "id")
                return 65;

            if (name == "ma_don_vi"
                ||
                name == "ma_tuyen"
                ||
                name == "ma_khu_vuc"
                ||
                name == "ma_xa_phuong"
                ||
                name == "ma_hanh_chinh")
            {
                return 120;
            }

            if (name == "ten_don_vi"
                ||
                name == "ten_don_vi_cap_tren"
                ||
                name == "ten_tuyen"
                ||
                name == "ten_khu_vuc")
            {
                return 220;
            }

            if (name == "ten_xa_phuong"
                ||
                name == "ten_hanh_chinh"
                ||
                name == "ten_cap"
                ||
                name == "ten_loai")
            {
                return 170;
            }

            if (name == "dia_chi_hien_hanh")
                return 250;

            if (name == "mo_ta"
                ||
                name == "ghi_chu"
                ||
                name == "nguon_tham_khao"
                ||
                name == "thong_tin_lien_he")
            {
                return 230;
            }

            if (name == "trang_thai_quan_ly"
                ||
                name == "muc_do_xac_minh")
            {
                return 155;
            }

            if (name == "ngay_tao"
                ||
                name == "ngay_cap_nhat"
                ||
                name == "ngay_xac_minh")
            {
                return 145;
            }

            return 135;
        }

        // =========================================================
        // TÌM FIELD INDEX
        // =========================================================
        private static int FindFieldIndex(
            Shapefile sf,
            string fieldName)
        {
            if (sf == null)
                return -1;

            for (int i = 0;
                 i < sf.NumFields;
                 i++)
            {
                Field field =
                    sf.get_Field(i);

                if (field != null &&
                    string.Equals(
                        field.Name,
                        fieldName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        // =========================================================
        // TRÁNH TRÙNG TÊN CỘT
        // =========================================================
        private static string MakeUniqueColumnName(
            DataTable table,
            string requestedName)
        {
            string name =
                requestedName;

            int suffix = 1;

            while (table.Columns.Contains(
                name))
            {
                name =
                    requestedName +
                    "_" +
                    suffix;

                suffix++;
            }

            return name;
        }
    }
}