using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MapWinGIS_TOI
{
    // =============================================================
    // ĐIỂM TẠM KHI SỐ HÓA TRÊN MAP
    //
    // Tọa độ hiện tại của MapWinGIS là EPSG:32648.
    // Đơn vị tọa độ: mét.
    // =============================================================
    public sealed class EditingVertex
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public EditingVertex(
            double x,
            double y)
        {
            X = x;
            Y = y;
        }
    }

    // =============================================================
    // EDITING SERVICE - HÀ NỘI ONLY / EPSG:32648
    //
    // Nhiệm vụ:
    // - Kiểm tra số đỉnh tối thiểu;
    // - Kiểm tra tọa độ hợp lệ;
    // - Loại bỏ đỉnh trùng liên tiếp;
    // - Tạo WKT Point / LineString / Polygon;
    // - Tự khép Polygon đúng chuẩn WKT;
    // - Không truy cập CSDL;
    // - Không phụ thuộc giao diện WinForms.
    //
    // Tọa độ đầu vào đã là EPSG:32648.
    // =============================================================
    public static class EditingService
    {
        // Sai số rất nhỏ để so sánh hai tọa độ trên mặt phẳng mét.
        private const double COORDINATE_EPSILON =
            0.000001;

        // =========================================================
        // LOẠI HÌNH HỌC CỦA TABLE
        // =========================================================
        public static string GetGeometryKind(
            string tableName)
        {
            if (string.Equals(
                tableName,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "POINT";
            }

            if (string.Equals(
                tableName,
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "LINESTRING";
            }

            if (string.Equals(
                tableName,
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "POLYGON";
            }

            return "";
        }

        // =========================================================
        // SỐ ĐỈNH TỐI THIỂU
        // =========================================================
        public static int GetMinimumVertexCount(
            string tableName)
        {
            string kind =
                GetGeometryKind(
                    tableName);

            if (kind == "POINT")
                return 1;

            if (kind == "LINESTRING")
                return 2;

            if (kind == "POLYGON")
                return 3;

            return int.MaxValue;
        }

        // =========================================================
        // HƯỚNG DẪN NGƯỜI DÙNG
        // =========================================================
        public static string GetInstruction(
            string tableName)
        {
            string kind =
                GetGeometryKind(
                    tableName);

            if (kind == "POINT")
            {
                return
                    "Điểm đơn vị được tạo tự động từ xã/phường đã chọn; " +
                    "không số hóa Point thủ công.";
            }

            if (kind == "LINESTRING")
            {
                return
                    "Bấm chuột trái để thêm các đỉnh của tuyến. " +
                    "Chuột phải để kết thúc và lưu. Nhấn Esc để hủy.";
            }

            if (kind == "POLYGON")
            {
                return
                    "Bấm chuột trái để thêm các đỉnh của vùng. " +
                    "Chuột phải để khép vùng và lưu. Nhấn Esc để hủy.";
            }

            return
                "Không hỗ trợ loại dữ liệu này.";
        }

        // =========================================================
        // KIỂM TRA CÓ THỂ HOÀN TẤT HAY CHƯA
        // =========================================================
        public static bool CanFinish(
            string tableName,
            IList<EditingVertex> vertices,
            out string error)
        {
            error = "";

            string kind =
                GetGeometryKind(
                    tableName);

            if (string.IsNullOrWhiteSpace(
                kind))
            {
                error =
                    "Lớp dữ liệu không hỗ trợ biên tập hình học.";

                return false;
            }

            if (vertices == null)
            {
                error =
                    "Chưa có đỉnh nào được số hóa.";

                return false;
            }

            List<EditingVertex> normalized =
                NormalizeVertices(
                    vertices);

            for (int i = 0;
                 i < normalized.Count;
                 i++)
            {
                if (!IsFinite(
                    normalized[i].X)
                    ||
                    !IsFinite(
                    normalized[i].Y))
                {
                    error =
                        "Tọa độ đỉnh thứ " +
                        (i + 1) +
                        " không hợp lệ.";

                    return false;
                }
            }

            int required =
                GetMinimumVertexCount(
                    tableName);

            if (normalized.Count <
                required)
            {
                error =
                    "Chưa đủ số đỉnh để hoàn tất. " +
                    "Cần tối thiểu " +
                    required +
                    " đỉnh khác nhau.";

                return false;
            }

            if (kind == "LINESTRING")
            {
                if (AreSamePoint(
                    normalized[0],
                    normalized[
                        normalized.Count - 1]))
                {
                    error =
                        "Điểm đầu và điểm cuối của tuyến không được trùng nhau.";

                    return false;
                }
            }

            if (kind == "POLYGON")
            {
                if (CountDistinctVertices(
                    normalized) < 3)
                {
                    error =
                        "Polygon cần tối thiểu 3 đỉnh khác nhau.";

                    return false;
                }

                if (Math.Abs(
                    CalculateSignedArea(
                        normalized)) <=
                    COORDINATE_EPSILON)
                {
                    error =
                        "Các đỉnh Polygon đang nằm gần như trên một đường thẳng. " +
                        "Hãy chọn lại vùng.";

                    return false;
                }
            }

            return true;
        }

        // =========================================================
        // TẠO WKT
        //
        // WKT tạo ra sử dụng tọa độ EPSG:32648.
        // SRID được PostGISGeometryRepository gán khi lưu.
        // =========================================================
        public static string BuildWkt(
            string tableName,
            IList<EditingVertex> vertices)
        {
            string error;

            if (!CanFinish(
                tableName,
                vertices,
                out error))
            {
                throw new InvalidOperationException(
                    error);
            }

            string kind =
                GetGeometryKind(
                    tableName);

            List<EditingVertex> normalized =
                NormalizeVertices(
                    vertices);

            if (kind == "POINT")
            {
                return BuildPointWkt(
                    normalized[0]);
            }

            if (kind == "LINESTRING")
            {
                return BuildLineStringWkt(
                    normalized);
            }

            if (kind == "POLYGON")
            {
                return BuildPolygonWkt(
                    normalized);
            }

            throw new InvalidOperationException(
                "Không xác định được loại hình học.");
        }

        // =========================================================
        // POINT
        //
        // Giữ lại để tương thích với code cũ.
        // Luồng hiện tại không dùng người dùng tự số hóa Point đơn vị.
        // =========================================================
        private static string BuildPointWkt(
            EditingVertex vertex)
        {
            return
                "POINT(" +
                FormatNumber(
                    vertex.X) +
                " " +
                FormatNumber(
                    vertex.Y) +
                ")";
        }

        // =========================================================
        // LINESTRING
        // =========================================================
        private static string BuildLineStringWkt(
            IList<EditingVertex> vertices)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.Append(
                "LINESTRING(");

            for (int i = 0;
                 i < vertices.Count;
                 i++)
            {
                if (i > 0)
                {
                    builder.Append(
                        ",");
                }

                AppendCoordinate(
                    builder,
                    vertices[i]);
            }

            builder.Append(
                ")");

            return
                builder.ToString();
        }

        // =========================================================
        // POLYGON
        //
        // - Tự khép vòng bằng đỉnh đầu;
        // - Nếu đỉnh cuối đã trùng đỉnh đầu thì không thêm lần nữa.
        // =========================================================
        private static string BuildPolygonWkt(
            IList<EditingVertex> vertices)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.Append(
                "POLYGON((");

            for (int i = 0;
                 i < vertices.Count;
                 i++)
            {
                if (i > 0)
                {
                    builder.Append(
                        ",");
                }

                AppendCoordinate(
                    builder,
                    vertices[i]);
            }

            if (!AreSamePoint(
                vertices[0],
                vertices[
                    vertices.Count - 1]))
            {
                builder.Append(
                    ",");

                AppendCoordinate(
                    builder,
                    vertices[0]);
            }

            builder.Append(
                "))");

            return
                builder.ToString();
        }

        // =========================================================
        // CHUẨN HÓA DANH SÁCH ĐỈNH
        //
        // Bỏ các đỉnh trùng liên tiếp do người dùng click hai lần
        // gần như cùng một vị trí.
        // =========================================================
        private static List<EditingVertex> NormalizeVertices(
            IList<EditingVertex> vertices)
        {
            List<EditingVertex> result =
                new List<EditingVertex>();

            if (vertices == null)
            {
                return result;
            }

            for (int i = 0;
                 i < vertices.Count;
                 i++)
            {
                EditingVertex current =
                    vertices[i];

                if (current == null)
                {
                    continue;
                }

                if (result.Count == 0)
                {
                    result.Add(
                        current);

                    continue;
                }

                EditingVertex previous =
                    result[
                        result.Count - 1];

                if (!AreSamePoint(
                    previous,
                    current))
                {
                    result.Add(
                        current);
                }
            }

            return result;
        }

        // =========================================================
        // ĐẾM SỐ ĐỈNH KHÁC NHAU
        // =========================================================
        private static int CountDistinctVertices(
            IList<EditingVertex> vertices)
        {
            if (vertices == null)
            {
                return 0;
            }

            List<EditingVertex> distinct =
                new List<EditingVertex>();

            foreach (EditingVertex vertex
                in vertices)
            {
                bool exists =
                    false;

                foreach (EditingVertex item
                    in distinct)
                {
                    if (AreSamePoint(
                        vertex,
                        item))
                    {
                        exists =
                            true;

                        break;
                    }
                }

                if (!exists)
                {
                    distinct.Add(
                        vertex);
                }
            }

            return
                distinct.Count;
        }

        // =========================================================
        // DIỆN TÍCH CÓ DẤU - SHOELACE
        //
        // Chỉ dùng để phát hiện Polygon suy biến.
        // =========================================================
        private static double CalculateSignedArea(
            IList<EditingVertex> vertices)
        {
            if (vertices == null ||
                vertices.Count < 3)
            {
                return 0.0;
            }

            double sum =
                0.0;

            for (int i = 0;
                 i < vertices.Count;
                 i++)
            {
                int j =
                    (i + 1) %
                    vertices.Count;

                sum +=
                    vertices[i].X *
                    vertices[j].Y;

                sum -=
                    vertices[j].X *
                    vertices[i].Y;
            }

            return
                sum / 2.0;
        }

        // =========================================================
        // SO SÁNH HAI ĐIỂM
        // =========================================================
        private static bool AreSamePoint(
            EditingVertex a,
            EditingVertex b)
        {
            if (a == null ||
                b == null)
            {
                return false;
            }

            return
                Math.Abs(
                    a.X - b.X) <=
                COORDINATE_EPSILON
                &&
                Math.Abs(
                    a.Y - b.Y) <=
                COORDINATE_EPSILON;
        }

        // =========================================================
        // KIỂM TRA SỐ HỮU HẠN
        // =========================================================
        private static bool IsFinite(
            double value)
        {
            return
                !double.IsNaN(
                    value)
                &&
                !double.IsInfinity(
                    value);
        }

        // =========================================================
        // GHI MỘT CẶP TỌA ĐỘ VÀO WKT
        // =========================================================
        private static void AppendCoordinate(
            StringBuilder builder,
            EditingVertex vertex)
        {
            builder.Append(
                FormatNumber(
                    vertex.X));

            builder.Append(
                " ");

            builder.Append(
                FormatNumber(
                    vertex.Y));
        }

        // =========================================================
        // FORMAT SỐ AN TOÀN CHO WKT
        // =========================================================
        private static string FormatNumber(
            double value)
        {
            return
                value.ToString(
                    "0.###############",
                    CultureInfo.InvariantCulture);
        }
    }
}