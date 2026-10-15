using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    public partial class Form1
    {
        // =========================================================
        // POSTGIS - HÀ NỘI ONLY
        //
        // Đề tài:
        // Xây cơ sở dữ liệu GIS tra cứu thông tin và quản lý
        // các đơn vị quân đội trên địa bàn Hà Nội.
        //
        // Kiến trúc bản đồ:
        // 1. Ranh giới Thành phố Hà Nội -> MultiPolygon
        // 2. 126 xã/phường mới          -> MultiPolygon
        // 3. Khu vực đơn vị             -> Polygon
        // 4. Tuyến đường đơn vị          -> LineString
        // 5. Đơn vị quân đội             -> Point hiển thị
        //    Ưu tiên: geom_hien_thi (nếu có)
        //    Dự phòng: geom đại diện hành chính
        //
        // Tất cả dùng EPSG:32648.
        // Không dùng tile online.
        // Không biến đổi CRS trong lúc tải layer.
        // =========================================================

        // =========================================================
        // CẤU HÌNH POSTGIS
        // =========================================================
        private const string PG_HOST = "localhost";
        private const string PG_PORT = "5433";
        private const string PG_DB = "HQTCSDL";
        private const string PG_USER = "postgres";

        // Giữ nguyên theo cấu hình hiện tại của dự án.
        // Nếu PostgreSQL trên máy đổi mật khẩu thì sửa tại đây.
        private const string PG_PASS = "123456";

        private const string PG_SCHEMA = "ql_don_vi";

        // Dữ liệu + bản đồ cùng một CRS.
        private const int DATA_SRID = 32648;
        private const int MAP_SRID = 32648;

        private const string POSTGIS_NODE_TAG = "POSTGIS";

        // =========================================================
        // KEY LOGIC CỦA CÁC LỚP
        //
        // Giữ key don_vi_quan_doi/tuyen_duong_don_vi/
        // khu_vuc_don_vi để các module CRUD, tra cứu, chọn đối tượng
        // hiện tại tiếp tục hoạt động.
        // =========================================================
        private readonly string[] _postgisLoadOrder =
        {
            // Add trước = nằm dưới.
            "dia_gioi_hanh_chinh",
            "xa_phuong_ha_noi",
            "khu_vuc_don_vi",
            "tuyen_duong_don_vi",
            "don_vi_quan_doi"
        };

        private readonly string[] _postgisTables =
        {
            // Thứ tự hiện trong TreeView.
            "dia_gioi_hanh_chinh",
            "xa_phuong_ha_noi",
            "don_vi_quan_doi",
            "tuyen_duong_don_vi",
            "khu_vuc_don_vi"
        };

        private readonly string[] _legacyPostgisTables =
        {
            "cong",
            "duong_noi_bo",
            "cong_trinh",
            "khu_vuc",
            "doanh_trai"
        };

        // =========================================================
        // GDAL/OGR CONNECTION STRING
        // =========================================================
        private string PgConnectionString
        {
            get
            {
                return
                    "PG:" +
                    "host=" + PG_HOST + " " +
                    "port=" + PG_PORT + " " +
                    "dbname=" + PG_DB + " " +
                    "user=" + PG_USER + " " +
                    "password=" + PG_PASS;
            }
        }

        // =========================================================
        // NÚT POSTGIS
        // =========================================================
        private void btnPostGIS_Click(
            object sender,
            EventArgs e)
        {
            if (btnPostGIS == null ||
                !btnPostGIS.Enabled)
            {
                return;
            }

            btnPostGIS.Enabled = false;

            OgrDatasource datasource = null;

            try
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang nạp dữ liệu GIS Hà Nội từ PostGIS...";
                }

                // -------------------------------------------------
                // 1. Map và dữ liệu cùng EPSG:32648.
                // -------------------------------------------------
                EnsureLocalMapProjection32648();

                // Tiles luôn tắt.
                try
                {
                    axMap2.Tiles.Visible = false;
                    axMap2.Tiles.UseServer = false;
                }
                catch
                {
                }

                // -------------------------------------------------
                // 2. Mở datasource.
                // -------------------------------------------------
                datasource =
                    new OgrDatasource();

                if (!datasource.Open(
                    PgConnectionString))
                {
                    MessageBox.Show(
                        "Không thể kết nối PostgreSQL/PostGIS.\n\n" +
                        "Máy chủ: " + PG_HOST +
                        "\nCổng: " + PG_PORT +
                        "\nCơ sở dữ liệu: " + PG_DB +
                        "\nTài khoản: " + PG_USER +
                        "\nSchema: " + PG_SCHEMA +
                        "\n\nGDAL:\n" +
                        datasource.GdalLastErrorMsg,
                        "Lỗi kết nối PostGIS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Kết nối PostGIS thất bại.";
                    }

                    return;
                }

                // -------------------------------------------------
                // 3. Datasource đã mở thành công.
                // Các VIEW được tải trực tiếp bằng SQL bên dưới.
                // -------------------------------------------------

                // -------------------------------------------------
                // 4. Dọn lớp PostGIS cũ.
                // -------------------------------------------------
                RemoveOldPostGISLayers();
                RemoveOldPostGISNodes();

                int loadedCount = 0;

                List<string> failedTables =
                    new List<string>();

                // -------------------------------------------------
                // 5. Load 5 lớp theo thứ tự vẽ.
                // -------------------------------------------------
                foreach (string layerKey
                    in _postgisLoadOrder)
                {
                    // Danh sách OGR chỉ dùng để chẩn đoán.
                    // Với VIEW PostGIS, một số bản GDAL/OGR có thể
                    // không liệt kê đầy đủ trong LayerCount. Vì vậy
                    // luôn thử tải trực tiếp bằng SQL thay vì bỏ qua.
                    bool loaded =
                        LoadPostGISLayer(
                            layerKey,
                            false);

                    if (loaded)
                    {
                        loadedCount++;

                        ApplyInitialPostGISVisibility(
                            layerKey);
                    }
                    else
                    {
                        failedTables.Add(
                            GetPostGISDisplayName(
                                layerKey));
                    }
                }

                // -------------------------------------------------
                // 6. Tạo node TreeView.
                // -------------------------------------------------
                foreach (string layerKey
                    in _postgisTables)
                {
                    TreeNode node =
                        new TreeNode();

                    node.Name =
                        layerKey;

                    node.Text =
                        GetPostGISDisplayName(
                            layerKey);

                    node.Tag =
                        POSTGIS_NODE_TAG;

                    treeLayers.Nodes.Add(
                        node);

                    bool visible =
                        IsLoadedPostGISLayerVisible(
                            layerKey);

                    SetLayerNodeChecked(
                        node,
                        visible);
                }

                // -------------------------------------------------
                // 7. BẢO TOÀN BASEMAP VỆ TINH
                //
                // Các lớp PostGIS được thêm sau basemap. Luôn bảo đảm
                // raster vệ tinh vẫn bật và nằm dưới cùng để polygon
                // hành chính không thay thế/che mất nền.
                // -------------------------------------------------
                EnsureSatelliteBasemapBelowPostGIS();

                // -------------------------------------------------
                // 8. Đồng bộ UI.
                // -------------------------------------------------
                RefreshLayerCombo();
                UpdateCurrentLayerStatus();

                // -------------------------------------------------
                // 9. Vẽ và zoom đúng phạm vi Hà Nội.
                // -------------------------------------------------
                try
                {
                    axMap2.Redraw();
                }
                catch
                {
                }

                if (loadedCount > 0)
                {
                    try
                    {
                        axMap2.ZoomToMaxVisibleExtents();
                    }
                    catch
                    {
                    }
                }

                // -------------------------------------------------
                // 10. Trạng thái.
                // -------------------------------------------------
                if (lblStatus != null)
                {
                    if (failedTables.Count == 0)
                    {
                        lblStatus.Text =
                            "Đã tải " +
                            loadedCount +
                            "/5 lớp GIS Hà Nội | EPSG:" +
                            MAP_SRID +
                            " | Nền vệ tinh cục bộ";
                    }
                    else
                    {
                        lblStatus.Text =
                            "Đã tải " +
                            loadedCount +
                            "/5 lớp. Không tải được: " +
                            string.Join(
                                ", ",
                                failedTables.ToArray());
                    }
                }

                if (loadedCount == 0)
                {
                    MessageBox.Show(
                        "Đã kết nối PostGIS nhưng chưa tải được lớp GIS nào.\n\n" +
                        "CSDL hiện tại cần có:\n" +
                        "• ql_don_vi.v_ban_do_ha_noi\n" +
                        "• ql_don_vi.v_ban_do_xa_phuong\n" +
                        "• ql_don_vi.v_ho_so_don_vi\n" +
                        "• ql_don_vi.tuyen_duong_don_vi\n" +
                        "• ql_don_vi.khu_vuc_don_vi\n\n" +
                        "CRS yêu cầu: EPSG:32648.",
                        "Không có lớp được tải",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (COMException ex)
            {
                MessageBox.Show(
                    "MapWinGIS gặp lỗi COM khi đọc PostGIS.\n\n" +
                    ex.Message,
                    "Lỗi COM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "MapWinGIS gặp lỗi COM.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi khi đọc dữ liệu PostGIS.\n\n" +
                    ex.Message,
                    "Lỗi PostGIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Có lỗi khi đọc PostGIS.";
                }
            }
            finally
            {
                if (datasource != null)
                {
                    try
                    {
                        datasource.Close();
                    }
                    catch
                    {
                    }
                }

                if (btnPostGIS != null)
                {
                    btnPostGIS.Enabled =
                        true;
                }
            }
        }

        // =========================================================
        // OBJECT THẬT TRONG POSTGIS CỦA MỖI LOGICAL KEY
        // =========================================================
        private string GetPostGISSourceObjectName(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return "";
            }

            if (layerKey.Equals(
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return "v_ban_do_ha_noi";
            }

            if (layerKey.Equals(
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "v_ban_do_xa_phuong";
            }

            if (layerKey.Equals(
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "v_ho_so_don_vi";
            }

            return layerKey;
        }

        // =========================================================
        // LẤY DANH SÁCH TABLE/VIEW TỪ POSTGIS
        // =========================================================
        private HashSet<string> GetAvailablePostGISTables(
            OgrDatasource datasource)
        {
            HashSet<string> tables =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            if (datasource == null)
                return tables;

            try
            {
                int count =
                    datasource.LayerCount;

                for (int i = 0;
                     i < count;
                     i++)
                {
                    try
                    {
                        string name =
                            datasource.GetLayerName(
                                i);

                        if (string.IsNullOrWhiteSpace(
                            name))
                        {
                            continue;
                        }

                        name =
                            name.Trim();

                        int dotIndex =
                            name.LastIndexOf('.');

                        if (dotIndex >= 0 &&
                            dotIndex < name.Length - 1)
                        {
                            name =
                                name.Substring(
                                    dotIndex + 1);
                        }

                        name =
                            name.Trim('"');

                        tables.Add(
                            name);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
                // Nếu OGR không liệt kê được thì LoadPostGISLayer
                // vẫn có thể được gọi trực tiếp.
            }

            return tables;
        }

        // =========================================================
        // LOAD MỘT LỚP POSTGIS
        // =========================================================
        private bool LoadPostGISLayer(
            string layerKey,
            bool redraw)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return false;
            }

            if (!IsManagedPostGISTable(
                layerKey))
            {
                return false;
            }

            try
            {
                EnsureLocalMapProjection32648();

                // ---------------------------------------------
                // Đã load -> bật lại.
                // ---------------------------------------------
                int existingHandle;

                if (_loadedLayers.TryGetValue(
                    layerKey,
                    out existingHandle))
                {
                    try
                    {
                        axMap2.set_LayerVisible(
                            existingHandle,
                            true);

                        if (redraw)
                        {
                            axMap2.Redraw();
                        }

                        return true;
                    }
                    catch
                    {
                        _loadedLayers.Remove(
                            layerKey);
                    }
                }

                string sql =
                    BuildPostGISLayerSql(
                        layerKey);

                if (string.IsNullOrWhiteSpace(
                    sql))
                {
                    return false;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đang tải lớp: " +
                        GetPostGISDisplayName(
                            layerKey) +
                        "...";

                    if (lblStatus.GetCurrentParent() != null)
                    {
                        lblStatus.GetCurrentParent()
                            .Refresh();
                    }
                }

                bool oldReproject =
                    _globalSettings.ReprojectLayersOnAdding;

                bool oldAllowMismatch =
                    _globalSettings.AllowProjectionMismatch;

                bool oldAllowNoProjection =
                    _globalSettings.AllowLayersWithoutProjections;

                int handle = -1;

                try
                {
                    // Map và layer đều là 32648:
                    // không reproject khi add.
                    _globalSettings.ReprojectLayersOnAdding =
                        false;

                    // AddLayerFromDatabase() với câu SQL có JOIN/
                    // biểu thức geometry đôi khi làm OGR không trả metadata
                    // projection dù tọa độ thực tế vẫn là EPSG:32648.
                    //
                    // Chỉ nới kiểm tra trong đúng lúc AddLayerFromDatabase;
                    // ngay sau đó các GlobalSettings được khôi phục và
                    // SetLoadedLayerProjection32648() gán metadata 32648
                    // cho Shapefile đã nạp.
                    _globalSettings.AllowProjectionMismatch =
                        true;

                    _globalSettings.AllowLayersWithoutProjections =
                        true;

                    handle =
                        axMap2.AddLayerFromDatabase(
                            PgConnectionString,
                            sql,
                            true);
                }
                finally
                {
                    _globalSettings.ReprojectLayersOnAdding =
                        oldReproject;

                    _globalSettings.AllowProjectionMismatch =
                        oldAllowMismatch;

                    _globalSettings.AllowLayersWithoutProjections =
                        oldAllowNoProjection;
                }

                if (handle < 0)
                {
                    ShowPostGISLayerError(
                        layerKey);

                    return false;
                }

                axMap2.set_LayerName(
                    handle,
                    GetPostGISDisplayName(
                        layerKey));

                _loadedLayers[layerKey] =
                    handle;

                axMap2.set_LayerVisible(
                    handle,
                    true);

                SetLoadedLayerProjection32648(
                    handle);

                ApplyPostGISLayerStyle(
                    layerKey,
                    handle);

                if (redraw)
                {
                    try
                    {
                        axMap2.Redraw();
                    }
                    catch
                    {
                    }
                }

                return true;
            }
            catch (COMException ex)
            {
                MessageBox.Show(
                    "MapWinGIS gặp lỗi COM khi tải lớp:\n\n" +
                    GetPostGISDisplayName(
                        layerKey) +
                    "\n\n" +
                    ex.Message,
                    "Lỗi COM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tải được lớp:\n" +
                    GetPostGISDisplayName(
                        layerKey) +
                    "\n\n" +
                    ex.Message,
                    "Lỗi PostGIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        // =========================================================
        // GÁN EPSG:32648 CHO LAYER ĐÃ LOAD
        // =========================================================
        private void SetLoadedLayerProjection32648(
            int handle)
        {
            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(
                        handle);

                if (sf == null ||
                    sf.GeoProjection == null)
                {
                    return;
                }

                sf.GeoProjection.ImportFromEPSG(
                    MAP_SRID);
            }
            catch
            {
                // Geometry SQL đã là EPSG:32648.
            }
        }

        // =========================================================
        // THÔNG BÁO LỖI LAYER
        // =========================================================
        private void ShowPostGISLayerError(
            string layerKey)
        {
            string mapError = "";
            string gdalError = "";

            try
            {
                mapError =
                    axMap2.get_ErrorMsg(
                        axMap2.LastErrorCode);
            }
            catch
            {
            }

            try
            {
                gdalError =
                    _globalSettings.GdalLastErrorMsg;
            }
            catch
            {
            }

            MessageBox.Show(
                "Không tải được lớp: " +
                GetPostGISDisplayName(
                    layerKey) +
                "\n\nNguồn PostGIS: " +
                PG_SCHEMA +
                "." +
                GetPostGISSourceObjectName(
                    layerKey) +
                "\n\nMapWinGIS:\n" +
                mapError +
                "\n\nGDAL:\n" +
                gdalError +
                "\n\nMap CRS: EPSG:" +
                MAP_SRID,
                "Lỗi tải PostGIS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        // =========================================================
        // SQL 5 LỚP GIS HÀ NỘI
        // =========================================================
        private string BuildPostGISLayerSql(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return null;
            }

            // =====================================================
            // 1. RANH GIỚI THÀNH PHỐ HÀ NỘI
            // =====================================================
            if (layerKey.Equals(
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "id, " +
                    "ma_hanh_chinh, " +
                    "ten_hanh_chinh, " +
                    "cap_hanh_chinh, " +
                    "tinh_thanh_pho, " +
                    BuildGeometry32648Sql(
                        "geom",
                        "MultiPolygon") +
                    " AS geom " +
                    "FROM " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "v_ban_do_ha_noi") +
                    " " +
                    "WHERE geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(geom)";
            }

            // =====================================================
            // 2. 126 XÃ/PHƯỜNG MỚI
            // =====================================================
            if (layerKey.Equals(
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "id, " +
                    "ma_xa_phuong, " +
                    "ten_xa_phuong, " +
                    "loai_xa_phuong, " +
                    "tinh_thanh_pho, " +
                    BuildGeometry32648Sql(
                        "geom",
                        "MultiPolygon") +
                    " AS geom " +
                    "FROM " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "v_ban_do_xa_phuong") +
                    " " +
                    "WHERE geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(geom)";
            }

            // =====================================================
            // 3. KHU VỰC ĐƠN VỊ
            // =====================================================
            if (layerKey.Equals(
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "kv.id, " +
                    "kv.don_vi_id, " +
                    "dv.ma_don_vi, " +
                    "dv.ten_don_vi, " +
                    "kv.ma_khu_vuc, " +
                    "kv.ten_khu_vuc, " +
                    "kv.loai_khu_vuc, " +
                    "ROUND(COALESCE(ST_Area(kv.geom),0)::numeric,2) AS dien_tich_m2, " +
                    "ROUND(COALESCE(ST_Perimeter(kv.geom),0)::numeric,2) AS chu_vi_m, " +
                    "kv.mo_ta, " +
                    "kv.ghi_chu, " +
                    "kv.trang_thai_quan_ly, " +
                    "kv.ngay_tao, " +
                    "kv.ngay_cap_nhat, " +
                    BuildGeometry32648Sql(
                        "kv.geom",
                        "Polygon") +
                    " AS geom " +
                    "FROM " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "khu_vuc_don_vi") +
                    " kv " +
                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "don_vi_quan_doi") +
                    " dv ON dv.id=kv.don_vi_id " +
                    "WHERE kv.geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(kv.geom)";
            }

            // =====================================================
            // 4. TUYẾN ĐƯỜNG ĐƠN VỊ
            // =====================================================
            if (layerKey.Equals(
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "td.id, " +
                    "td.don_vi_id, " +
                    "dv.ma_don_vi, " +
                    "dv.ten_don_vi, " +
                    "td.ma_tuyen, " +
                    "td.ten_tuyen, " +
                    "td.loai_tuyen, " +
                    "td.cap_duong, " +
                    "td.be_rong_m, " +
                    "ROUND(COALESCE(ST_Length(td.geom),0)::numeric,2) AS chieu_dai_m, " +
                    "td.mo_ta, " +
                    "td.ghi_chu, " +
                    "td.trang_thai_quan_ly, " +
                    "td.ngay_tao, " +
                    "td.ngay_cap_nhat, " +
                    BuildGeometry32648Sql(
                        "td.geom",
                        "LineString") +
                    " AS geom " +
                    "FROM " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "tuyen_duong_don_vi") +
                    " td " +
                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "don_vi_quan_doi") +
                    " dv ON dv.id=td.don_vi_id " +
                    "WHERE td.geom IS NOT NULL " +
                    "AND NOT ST_IsEmpty(td.geom)";
            }

            // =====================================================
            // 5. 43 ĐƠN VỊ - POINT HIỂN THỊ
            //
            // Quy tắc:
            // - Nếu don_vi_quan_doi.geom_hien_thi có dữ liệu:
            //     dùng geom_hien_thi để vẽ.
            // - Nếu chưa có:
            //     dùng hs.geom (điểm đại diện hành chính) làm dự phòng.
            //
            // Không ghi đè geom gốc.
            // =====================================================
            if (layerKey.Equals(
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "SELECT " +
                    "hs.id, " +
                    "hs.ma_don_vi, " +
                    "hs.ten_don_vi, " +
                    "hs.ten_viet_tat, " +
                    "hs.ky_hieu_bien_so, " +
                    "hs.ten_cap, " +
                    "hs.ten_loai, " +
                    "hs.ten_don_vi_cap_tren, " +
                    "hs.ma_xa_phuong, " +
                    "hs.ten_xa_phuong, " +
                    "hs.loai_xa_phuong, " +
                    "hs.tinh_thanh_pho, " +
                    "hs.dia_chi_hien_hanh, " +
                    "hs.nguon_tham_khao, " +
                    "hs.ngay_xac_minh, " +
                    "hs.muc_do_xac_minh, " +
                    "hs.mo_ta, " +
                    "hs.ghi_chu, " +
                    "hs.trang_thai_quan_ly, " +

                    // Chất lượng dữ liệu hồ sơ.
                    "CASE " +
                    "WHEN hs.id = 42 THEN 'can_ra_soat_pham_vi' " +
                    "WHEN POSITION('http' IN LOWER(COALESCE(hs.nguon_tham_khao,''))) > 0 " +
                    "THEN 'da_doi_chieu_cong_khai' " +
                    "ELSE 'tham_chieu_hanh_chinh' " +
                    "END AS chat_luong_du_lieu, " +

                    // Nhãn địa chỉ.
                    "CASE " +
                    "WHEN hs.id = 42 THEN 'Địa bàn hiện lưu' " +
                    "WHEN POSITION('http' IN LOWER(COALESCE(hs.nguon_tham_khao,''))) > 0 " +
                    "THEN 'Địa chỉ công khai' " +
                    "ELSE 'Địa bàn tham chiếu' " +
                    "END AS nhan_dia_chi_hien_thi, " +

                    // Cảnh báo phạm vi.
                    "CASE " +
                    "WHEN hs.id = 42 " +
                    "THEN 'Cần rà soát phạm vi; không sử dụng bản ghi này để khẳng định vị trí hiện hành trên địa bàn Hà Nội.' " +
                    "ELSE NULL " +
                    "END AS canh_bao_du_lieu, " +

                    // Cho giao diện biết điểm đang vẽ lấy từ đâu.
                    "CASE " +
                    "WHEN dv.geom_hien_thi IS NOT NULL " +
                    "AND NOT ST_IsEmpty(dv.geom_hien_thi) " +
                    "THEN 'diem_hien_thi' " +
                    "ELSE 'diem_dai_dien_hanh_chinh' " +
                    "END AS loai_vi_tri_gis, " +

                    "CASE " +
                    "WHEN dv.geom_hien_thi IS NOT NULL " +
                    "AND NOT ST_IsEmpty(dv.geom_hien_thi) " +
                    "THEN 'Điểm hiển thị đã được gán riêng cho hồ sơ đơn vị.' " +
                    "ELSE 'Điểm đại diện hành chính theo xã/phường.' " +
                    "END AS mo_ta_vi_tri_gis, " +

                    // Geometry thực sự gửi sang MapWinGIS.
                    BuildGeometry32648Sql(
                        "COALESCE(dv.geom_hien_thi, hs.geom)",
                        "Point") +
                    " AS geom " +

                    "FROM " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "v_ho_so_don_vi") +
                    " hs " +

                    "LEFT JOIN " +
                    QuoteQualifiedTable(
                        PG_SCHEMA,
                        "don_vi_quan_doi") +
                    " dv ON dv.id = hs.id " +

                    "WHERE COALESCE(dv.geom_hien_thi, hs.geom) IS NOT NULL " +
                    "AND NOT ST_IsEmpty(COALESCE(dv.geom_hien_thi, hs.geom))";
            }

            return null;
        }

        // =========================================================
        // GEOMETRY EPSG:32648
        //
        // CSDL hiện tại đã được chuẩn hóa hoàn toàn về EPSG:32648.
        // Không ST_Transform / ST_SetSRID trong lúc tải layer.
        //
        // geometryExpression có thể là:
        //   geom
        //   kv.geom
        //   COALESCE(dv.geom_hien_thi, hs.geom)
        //
        // Vì vậy helper này nhận biểu thức SQL geometry, không chỉ tên cột.
        // =========================================================
        private string BuildGeometry32648Sql(
            string geometryExpression,
            string geometryType)
        {
            if (string.IsNullOrWhiteSpace(
                geometryExpression))
            {
                geometryExpression =
                    "geom";
            }

            if (string.IsNullOrWhiteSpace(
                geometryType))
            {
                throw new ArgumentException(
                    "Geometry type không được để trống.",
                    "geometryType");
            }

            return
                "ST_Force2D(" +
                geometryExpression +
                ")::geometry(" +
                geometryType +
                ", " +
                MAP_SRID +
                ")";
        }

        // =========================================================
        // QUOTE SCHEMA + OBJECT
        // =========================================================
        private string QuoteQualifiedTable(
            string schema,
            string table)
        {
            string safeSchema =
                (schema ?? "")
                .Replace(
                    "\"",
                    "\"\"");

            string safeTable =
                (table ?? "")
                .Replace(
                    "\"",
                    "\"\"");

            return
                "\"" +
                safeSchema +
                "\".\"" +
                safeTable +
                "\"";
        }

        // =========================================================
        // STYLE 5 LỚP HÀ NỘI
        // =========================================================
        private void ApplyPostGISLayerStyle(
            string layerKey,
            int handle)
        {
            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(
                        handle);

                if (sf == null)
                    return;

                Utils utils =
                    new Utils();

                // -------------------------------------------------
                // ĐƠN VỊ - POINT
                // -------------------------------------------------
                if (layerKey.Equals(
                    "don_vi_quan_doi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sf.DefaultDrawingOptions.PointSize =
                        13;

                    sf.DefaultDrawingOptions.FillColor =
                        utils.ColorByName(
                            tkMapColor.Red);

                    sf.DefaultDrawingOptions.LineColor =
                        utils.ColorByName(
                            tkMapColor.Black);

                    sf.DefaultDrawingOptions.LineWidth =
                        1.5f;

                    return;
                }

                // -------------------------------------------------
                // TUYẾN ĐƯỜNG
                // -------------------------------------------------
                if (layerKey.Equals(
                    "tuyen_duong_don_vi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sf.DefaultDrawingOptions.LineColor =
                        utils.ColorByName(
                            tkMapColor.Blue);

                    sf.DefaultDrawingOptions.LineWidth =
                        3.0f;

                    return;
                }

                // -------------------------------------------------
                // KHU VỰC
                // -------------------------------------------------
                if (layerKey.Equals(
                    "khu_vuc_don_vi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sf.DefaultDrawingOptions.FillColor =
                        utils.ColorByName(
                            tkMapColor.LightYellow);

                    // MapWinGIS: 0 = trong suốt, 255 = đục hoàn toàn.
                    // Giữ khu vực ở mức bán trong suốt để còn thấy basemap.
                    sf.DefaultDrawingOptions.FillTransparency =
                        85;

                    sf.DefaultDrawingOptions.LineColor =
                        utils.ColorByName(
                            tkMapColor.Red);

                    sf.DefaultDrawingOptions.LineWidth =
                        2.0f;

                    return;
                }

                // -------------------------------------------------
                // 126 XÃ/PHƯỜNG
                // -------------------------------------------------
                if (layerKey.Equals(
                    "xa_phuong_ha_noi",
                    StringComparison.OrdinalIgnoreCase))
                {
                    // Không tô mặt để giữ nguyên ảnh vệ tinh.
                    sf.DefaultDrawingOptions.FillVisible =
                        false;

                    // Xanh lam-ngọc dịu, rõ trên vệ tinh nhưng không chói.
                    uint xaBoundaryColor =
                        unchecked(
                            (uint)ColorTranslator.ToOle(
                                Color.FromArgb(
                                    150,
                                    218,
                                    225)));

                    sf.DefaultDrawingOptions.LineColor =
                        xaBoundaryColor;

                    sf.DefaultDrawingOptions.LineWidth =
                        0.75f;

                    ConfigureXaPhuongLabels(
                        sf);

                    return;
                }

                // -------------------------------------------------
                // RANH GIỚI THÀNH PHỐ HÀ NỘI
                // -------------------------------------------------
                if (layerKey.Equals(
                    "dia_gioi_hanh_chinh",
                    StringComparison.OrdinalIgnoreCase))
                {
                    // Chỉ vẽ đường bao Thành phố Hà Nội.
                    // Không tô vùng để không che basemap vệ tinh.
                    sf.DefaultDrawingOptions.FillVisible =
                        false;

                    sf.DefaultDrawingOptions.LineColor =
                        utils.ColorByName(
                            tkMapColor.DarkBlue);

                    sf.DefaultDrawingOptions.LineWidth =
                        2.2f;
                }
            }
            catch
            {
                // Style lỗi không làm hỏng layer.
            }
        }

        // =========================================================
        // NHÃN TÊN XÃ / PHƯỜNG
        //
        // View v_ban_do_xa_phuong đã trả cột ten_xa_phuong.
        // Labels.Generate dùng biểu thức [ten_xa_phuong].
        // =========================================================
        private void ConfigureXaPhuongLabels(
            Shapefile sf)
        {
            if (sf == null ||
                sf.Labels == null)
            {
                return;
            }

            try
            {
                sf.Labels.Clear();

                int generated =
                    sf.Labels.Generate(
                        "[ten_xa_phuong]",
                        tkLabelPositioning.lpCentroid,
                        true);

                sf.Labels.Visible =
                    generated > 0;

                // -------------------------------------------------
                // KIỂU CHỮ GỌN - KHÔNG KHUNG HỘP
                // -------------------------------------------------
                sf.Labels.FontName =
                    "Segoe UI Semibold";

                sf.Labels.FontSize =
                    8;

                sf.Labels.FontBold =
                    false;

                // Chữ trắng ngà + viền xanh đen, dễ đọc trên nền vệ tinh.
                sf.Labels.FontColor =
                    unchecked(
                        (uint)ColorTranslator.ToOle(
                            Color.FromArgb(
                                242,
                                246,
                                248)));

                sf.Labels.HaloVisible =
                    true;

                sf.Labels.HaloColor =
                    unchecked(
                        (uint)ColorTranslator.ToOle(
                            Color.FromArgb(
                                25,
                                48,
                                58)));

                sf.Labels.HaloSize =
                    2;

                // Tắt hoàn toàn khung chữ hình chữ nhật.
                sf.Labels.FrameVisible =
                    false;

                // -------------------------------------------------
                // GIẢM CHỒNG CHỮ
                // -------------------------------------------------
                sf.Labels.AvoidCollisions =
                    true;

                sf.Labels.CollisionBuffer =
                    14;

                sf.Labels.RemoveDuplicates =
                    true;

                sf.Labels.AutoOffset =
                    false;

                sf.Labels.OffsetX =
                    0;

                sf.Labels.OffsetY =
                    0;

                // Không ghi nhãn những polygon quá nhỏ trên màn hình.
                sf.Labels.MinDrawingSize =
                    30;

                // -------------------------------------------------
                // CHỈ HIỆN TÊN KHI PHÓNG ĐỦ GẦN
                //
                // Ở toàn Hà Nội sẽ không bị kín chữ.
                // Chỉ hiện khi phóng tương đối gần để toàn cảnh không kín chữ.
                // -------------------------------------------------
                sf.Labels.DynamicVisibility =
                    true;

                sf.Labels.MinVisibleScale =
                    0;

                sf.Labels.MaxVisibleScale =
                    180000;

                sf.Labels.ScaleLabels =
                    false;
            }
            catch (Exception ex)
            {
                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Không tạo được nhãn xã/phường: " +
                        ex.Message;
                }
            }
        }

        // =========================================================
        // BẢO ĐẢM BASEMAP VỆ TINH LUÔN NẰM DƯỚI POSTGIS
        //
        // Map.cs quản lý:
        //   _basemapLayerHandle
        //   ShowSatelliteBasemap()
        //
        // Vì Form1 là partial class, PostGIS.cs có thể dùng trực tiếp
        // các thành viên private này.
        // =========================================================
        private void EnsureSatelliteBasemapBelowPostGIS()
        {
            if (axMap2 == null)
            {
                return;
            }

            // Chưa có basemap -> thử nạp nền vệ tinh.
            if (_basemapLayerHandle < 0)
            {
                try
                {
                    ShowSatelliteBasemap();
                }
                catch (Exception ex)
                {
                    CrashLogger.Write(
                        "EnsureSatelliteBasemapBelowPostGIS LOAD ERROR: " +
                        ex.Message);
                }

                return;
            }

            try
            {
                axMap2.set_LayerVisible(
                    _basemapLayerHandle,
                    true);

                axMap2.MoveLayerBottom(
                    _basemapLayerHandle);
            }
            catch (Exception ex)
            {
                CrashLogger.Write(
                    "EnsureSatelliteBasemapBelowPostGIS ERROR: " +
                    ex.Message);

                // Handle cũ không còn hợp lệ: đánh dấu mất basemap
                // rồi nạp lại từ GeoTIFF cục bộ.
                _basemapLayerHandle = -1;
                _basemapImage = null;

                try
                {
                    ShowSatelliteBasemap();
                }
                catch (Exception reloadEx)
                {
                    CrashLogger.Write(
                        "EnsureSatelliteBasemapBelowPostGIS RELOAD ERROR: " +
                        reloadEx.Message);
                }
            }
        }


        // =========================================================
        // VISIBILITY BAN ĐẦU CỦA 5 LỚP
        // =========================================================
        private void ApplyInitialPostGISVisibility(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return;
            }

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                return;
            }

            bool visible =
                !string.Equals(
                    layerKey,
                    "tuyen_duong_don_vi",
                    StringComparison.OrdinalIgnoreCase)
                &&
                !string.Equals(
                    layerKey,
                    "khu_vuc_don_vi",
                    StringComparison.OrdinalIgnoreCase);

            try
            {
                axMap2.set_LayerVisible(
                    handle,
                    visible);
            }
            catch
            {
            }
        }

        private bool IsLoadedPostGISLayerVisible(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return false;
            }

            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                return false;
            }

            try
            {
                return axMap2.get_LayerVisible(
                    handle);
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // XÓA POSTGIS LAYER CŨ KHỎI MAP
        // =========================================================
        private void RemoveOldPostGISLayers()
        {
            foreach (string layerKey
                in _postgisTables)
            {
                RemoveLoadedLayerByKey(
                    layerKey);
            }

            foreach (string tableName
                in _legacyPostgisTables)
            {
                RemoveLoadedLayerByKey(
                    tableName);
            }
        }

        // =========================================================
        // XÓA MỘT LAYER THEO KEY
        // =========================================================
        private void RemoveLoadedLayerByKey(
            string layerKey)
        {
            int handle;

            if (!_loadedLayers.TryGetValue(
                layerKey,
                out handle))
            {
                return;
            }

            try
            {
                axMap2.RemoveLayer(
                    handle);
            }
            catch
            {
            }

            _loadedLayers.Remove(
                layerKey);
        }

        // =========================================================
        // XÓA NODE POSTGIS CŨ
        // =========================================================
        private void RemoveOldPostGISNodes()
        {
            if (treeLayers == null)
                return;

            bool oldState =
                _layerCheckInternalChange;

            _layerCheckInternalChange =
                true;

            treeLayers.BeginUpdate();

            try
            {
                RemovePostGISNodesRecursive(
                    treeLayers.Nodes);
            }
            finally
            {
                treeLayers.EndUpdate();

                _layerCheckInternalChange =
                    oldState;
            }
        }

        // =========================================================
        // XÓA NODE POSTGIS ĐỆ QUY
        // =========================================================
        private void RemovePostGISNodesRecursive(
            TreeNodeCollection nodes)
        {
            if (nodes == null)
                return;

            for (int i = nodes.Count - 1;
                 i >= 0;
                 i--)
            {
                TreeNode node =
                    nodes[i];

                if (node == null)
                    continue;

                if (node.Nodes.Count > 0)
                {
                    RemovePostGISNodesRecursive(
                        node.Nodes);
                }

                bool isPostGIS =
                    node.Tag != null &&
                    string.Equals(
                        node.Tag.ToString(),
                        POSTGIS_NODE_TAG,
                        StringComparison.OrdinalIgnoreCase);

                string layerKey =
                    !string.IsNullOrWhiteSpace(
                        node.Name)
                        ? node.Name
                        : node.Text;

                if (isPostGIS ||
                    IsManagedPostGISTable(
                        layerKey) ||
                    IsLegacyPostGISTable(
                        layerKey))
                {
                    nodes.RemoveAt(
                        i);

                    continue;
                }

                if (node.Nodes.Count == 0 &&
                    (
                        string.Equals(
                            node.Text,
                            "Dữ liệu PostGIS",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        string.Equals(
                            node.Text,
                            "DỮ LIỆU GIS - HÀ NỘI",
                            StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    nodes.RemoveAt(
                        i);
                }
            }
        }

        // =========================================================
        // LAYER KEY CÓ THUỘC HỆ THỐNG HAY KHÔNG
        // =========================================================
        private bool IsManagedPostGISTable(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return false;
            }

            foreach (string name
                in _postgisTables)
            {
                if (string.Equals(
                    name,
                    layerKey,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // TABLE CŨ
        // =========================================================
        private bool IsLegacyPostGISTable(
            string tableName)
        {
            if (string.IsNullOrWhiteSpace(
                tableName))
            {
                return false;
            }

            foreach (string name
                in _legacyPostgisTables)
            {
                if (string.Equals(
                    name,
                    tableName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // TÊN HIỂN THỊ
        // =========================================================
        private string GetPostGISDisplayName(
            string layerKey)
        {
            if (string.IsNullOrWhiteSpace(
                layerKey))
            {
                return "";
            }

            if (layerKey.Equals(
                "dia_gioi_hanh_chinh",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Ranh giới Thành phố Hà Nội";
            }

            if (layerKey.Equals(
                "xa_phuong_ha_noi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Xã, phường Hà Nội";
            }

            if (layerKey.Equals(
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Đơn vị quân đội";
            }

            if (layerKey.Equals(
                "tuyen_duong_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Tuyến đường đơn vị";
            }

            if (layerKey.Equals(
                "khu_vuc_don_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Khu vực đơn vị";
            }

            return layerKey;
        }
    }
}