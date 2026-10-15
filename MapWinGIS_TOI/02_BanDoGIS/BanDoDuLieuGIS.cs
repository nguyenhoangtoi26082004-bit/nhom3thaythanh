
using System;
using System.Drawing;
using System.Windows.Forms;
using MapWinGIS;
using DrawingPoint = System.Drawing.Point;

namespace MapWinGIS_TOI
{
    // ============================================================
    // BẢN ĐỒ WEB GIS - GIAI ĐOẠN 4.2
    //
    // Chức năng:
    // - Bật/tắt 50 điểm đơn vị Web mẫu.
    // - Bật/tắt nhãn mã đơn vị.
    // - Bật/tắt 30 ranh giới địa bàn mô phỏng.
    // - Bật/tắt nhãn địa bàn.
    // - Thu phóng độc lập theo từng lớp.
    //
    // Nguồn: HQTCSDL, schema web_mau.
    // CRS: EPSG:32648 (WGS 84 / UTM Zone 48N).
    //
    // Không ghi dữ liệu vào PostgreSQL.
    // Không thay đổi lớp GIS chính thức trong ql_don_vi.
    // ============================================================
    public partial class Form1
    {
        private const string WEB_DIEM_KEY =
            "web_mau_don_vi";

        private const string WEB_DIA_BAN_KEY =
            "web_mau_quan_huyen";

        private Button _banDoWebButton;
        private ContextMenuStrip _banDoWebMenu;

        private ToolStripMenuItem _webNhanDiemItem;
        private ToolStripMenuItem _webNhanDiaBanItem;

        // ========================================================
        // 1. KHỞI TẠO MENU QUẢN LÝ LỚP WEB
        // ========================================================
        private void InitBanDoWebGIS()
        {
            if (panelLayerBar == null ||
                _banDoWebButton != null)
            {
                return;
            }

            _banDoWebButton = new Button
            {
                Name = "btnBanDoWebMau",
                Text = "Các lớp Web mẫu ▼",
                Size = new Size(165, 29),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Right
            };

            _banDoWebButton.FlatAppearance.BorderSize = 0;

            _banDoWebMenu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                ShowImageMargin = false,
                Font = new Font("Segoe UI", 9F)
            };

            ThemTieuDeMenu("ĐIỂM HỒ SƠ WEB MẪU");

            ThemLenhMenu(
                "Hiện 50 điểm hồ sơ",
                delegate
                {
                    HienThiWebMauTrenBanDo();

                    DongBoNutLopWeb(
                        WEB_DIEM_KEY,
                        true);
                });

            ThemLenhMenu(
                "Ẩn 50 điểm hồ sơ",
                delegate
                {
                    AnLopWebGis();
                });

            _webNhanDiemItem = new ToolStripMenuItem(
                "Hiện nhãn mã hồ sơ")
            {
                CheckOnClick = true
            };

            _webNhanDiemItem.CheckedChanged += delegate
            {
                BatTatNhanWebGis(
                    _webNhanDiemItem.Checked);
            };

            _banDoWebMenu.Items.Add(
                _webNhanDiemItem);

            ThemLenhMenu(
                "Thu phóng tới 50 điểm",
                delegate
                {
                    ThuPhongWebGis();
                });

            _banDoWebMenu.Items.Add(
                new ToolStripSeparator());

            ThemTieuDeMenu("RANH GIỚI ĐỊA BÀN MÔ PHỎNG");

            ThemLenhMenu(
                "Hiện 30 ranh giới địa bàn",
                delegate
                {
                    HienThiDiaBanWebGis();
                });

            ThemLenhMenu(
                "Ẩn 30 ranh giới địa bàn",
                delegate
                {
                    AnDiaBanWebGis();
                });

            _webNhanDiaBanItem =
                new ToolStripMenuItem(
                    "Hiện nhãn tên địa bàn")
                {
                    CheckOnClick = true
                };

            _webNhanDiaBanItem.CheckedChanged += delegate
            {
                BatTatNhanDiaBanWebGis(
                    _webNhanDiaBanItem.Checked);
            };

            _banDoWebMenu.Items.Add(
                _webNhanDiaBanItem);

            ThemLenhMenu(
                "Thu phóng tới 30 địa bàn",
                delegate
                {
                    ThuPhongDiaBanWebGis();
                });

            _banDoWebMenu.Items.Add(
                new ToolStripSeparator());

            ToolStripMenuItem note =
                new ToolStripMenuItem(
                    "Dữ liệu mô phỏng — chưa xác minh");

            note.Enabled = false;

            _banDoWebMenu.Items.Add(note);

            _banDoWebButton.Click += delegate
            {
                if (_banDoWebMenu != null &&
                    !_banDoWebMenu.IsDisposed)
                {
                    _banDoWebMenu.Show(
                        _banDoWebButton,
                        new DrawingPoint(
                            0,
                            _banDoWebButton.Height));
                }
            };

            panelLayerBar.Controls.Add(
                _banDoWebButton);

            panelLayerBar.Resize += delegate
            {
                CanChinhNutWebGis();
            };

            CanChinhNutWebGis();

            _banDoWebButton.BringToFront();

            Disposed += delegate
            {
                if (_banDoWebMenu != null)
                {
                    _banDoWebMenu.Dispose();
                    _banDoWebMenu = null;
                }
            };
        }

        private void ThemTieuDeMenu(string text)
        {
            ToolStripMenuItem title =
                new ToolStripMenuItem(text)
                {
                    Enabled = false,
                    ForeColor = Color.FromArgb(
                        110, 231, 183)
                };

            _banDoWebMenu.Items.Add(title);
        }

        private void ThemLenhMenu(
            string text,
            EventHandler handler)
        {
            ToolStripMenuItem item =
                new ToolStripMenuItem(text);

            item.Click += handler;

            _banDoWebMenu.Items.Add(item);
        }

        private void CanChinhNutWebGis()
        {
            if (panelLayerBar == null ||
                _banDoWebButton == null ||
                _banDoWebButton.IsDisposed)
            {
                return;
            }

            _banDoWebButton.Left = Math.Max(
                0,
                panelLayerBar.ClientSize.Width -
                _banDoWebButton.Width - 12);

            _banDoWebButton.Top = 5;
        }

        // ========================================================
        // 2. TÌM LỚP ĐÃ NẠP AN TOÀN
        // ========================================================
        private int LayHandleLopWeb(
            string key,
            string tenKiemTra)
        {
            if (axMap2 == null)
                return -1;

            int handle;

            if (!_loadedLayers.TryGetValue(
                key,
                out handle))
            {
                return -1;
            }

            try
            {
                string name =
                    axMap2.get_LayerName(handle);

                if (!string.IsNullOrEmpty(name) &&
                    name.IndexOf(
                        tenKiemTra,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return handle;
                }
            }
            catch
            {
            }

            _loadedLayers.Remove(key);

            return -1;
        }

        private int LayHandleLopWebGis()
        {
            return LayHandleLopWeb(
                WEB_DIEM_KEY,
                "WEB MẪU - Điểm đơn vị");
        }

        private int LayHandleDiaBanWebGis()
        {
            return LayHandleLopWeb(
                WEB_DIA_BAN_KEY,
                "WEB MẪU - Ranh giới");
        }

        // ========================================================
        // 3. ĐỒNG BỘ CÂY QUẢN LÝ LỚP
        // ========================================================
        private void DongBoNutLopWeb(
            string key,
            bool hien)
        {
            if (treeLayers == null)
                return;

            try
            {
                TreeNode[] nodes =
                    treeLayers.Nodes.Find(
                        key,
                        true);

                if (nodes.Length > 0)
                {
                    SetLayerNodeChecked(
                        nodes[0],
                        hien);
                }
            }
            catch
            {
                // Đồng bộ cây lỗi không làm hỏng bản đồ.
            }
        }

        // ========================================================
        // 4. QUẢN LÝ 50 ĐIỂM HỒ SƠ
        // ========================================================
        private void AnLopWebGis()
        {
            int handle = LayHandleLopWebGis();

            if (handle < 0)
                return;

            try
            {
                axMap2.set_LayerVisible(
                    handle,
                    false);

                DongBoNutLopWeb(
                    WEB_DIEM_KEY,
                    false);

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã ẩn lớp 50 điểm Web mẫu.";
                }
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Ẩn điểm Web mẫu",
                    ex);
            }
        }

        private void ThuPhongWebGis()
        {
            int handle = LayHandleLopWebGis();

            if (handle < 0)
            {
                HienThiWebMauTrenBanDo();

                handle = LayHandleLopWebGis();

                if (handle < 0)
                    return;
            }

            try
            {
                AnTongQuan();

                axMap2.set_LayerVisible(
                    handle,
                    true);

                DongBoNutLopWeb(
                    WEB_DIEM_KEY,
                    true);

                axMap2.ZoomToLayer(handle);
                axMap2.Redraw();
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Thu phóng 50 điểm mẫu",
                    ex);
            }
        }

        private void BatTatNhanWebGis(bool hien)
        {
            int handle = LayHandleLopWebGis();

            if (handle < 0 && hien)
            {
                HienThiWebMauTrenBanDo();

                handle = LayHandleLopWebGis();
            }

            if (handle < 0)
                return;

            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(handle);

                if (sf == null ||
                    sf.Labels == null)
                {
                    return;
                }

                if (hien)
                {
                    sf.Labels.Clear();

                    int count = sf.Labels.Generate(
                        "[ma_don_vi]",
                        tkLabelPositioning.lpCentroid,
                        true);

                    DinhDangNhanWeb(sf);

                    sf.Labels.Visible = count > 0;
                }
                else
                {
                    sf.Labels.Visible = false;
                }

                axMap2.Redraw();
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Nhãn mã hồ sơ",
                    ex);
            }
        }

        // ========================================================
        // 5. NẠP 30 RANH GIỚI ĐỊA BÀN MẪU
        // ========================================================
        private void HienThiDiaBanWebGis()
        {
            if (axMap2 == null)
                return;

            int existing = LayHandleDiaBanWebGis();

            if (existing >= 0)
            {
                try
                {
                    AnTongQuan();

                    axMap2.set_LayerVisible(
                        existing,
                        true);

                    DongBoNutLopWeb(
                        WEB_DIA_BAN_KEY,
                        true);

                    axMap2.Redraw();

                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Đang hiển thị ranh giới " +
                            "30 địa bàn mô phỏng.";
                    }

                    return;
                }
                catch
                {
                    _loadedLayers.Remove(
                        WEB_DIA_BAN_KEY);
                }
            }

            int handle = -1;

            bool oldReproject =
                _globalSettings.ReprojectLayersOnAdding;

            bool oldMismatch =
                _globalSettings.AllowProjectionMismatch;

            bool oldNoProjection =
                _globalSettings.AllowLayersWithoutProjections;

            try
            {
                AnTongQuan();

                EnsureLocalMapProjection32648();

                // Giữ SQL đơn giản để OGR nhận diện
                // cột geom và mã định danh integer.
                const string sql = @"
SELECT
    row_number() OVER (
        ORDER BY ma_dia_ban
    )::integer AS id,
    ma_dia_ban,
    ten_dia_ban,
    loai_dia_ban,
    dien_tich_km2,
    geom
FROM web_mau.v_quan_huyen_ban_do
WHERE geom IS NOT NULL
  AND NOT ST_IsEmpty(geom)
  AND ST_IsValid(geom)
  AND ST_SRID(geom) = 32648";

                try
                {
                    _globalSettings.ReprojectLayersOnAdding =
                        false;

                    _globalSettings.AllowProjectionMismatch =
                        true;

                    _globalSettings.AllowLayersWithoutProjections =
                        true;

                    handle = axMap2.AddLayerFromDatabase(
                        PgConnectionString,
                        sql,
                        true);
                }
                finally
                {
                    _globalSettings.ReprojectLayersOnAdding =
                        oldReproject;

                    _globalSettings.AllowProjectionMismatch =
                        oldMismatch;

                    _globalSettings.AllowLayersWithoutProjections =
                        oldNoProjection;
                }

                if (handle < 0)
                {
                    string gdalError = "";

                    try
                    {
                        gdalError =
                            _globalSettings.GdalLastErrorMsg;
                    }
                    catch
                    {
                    }

                    throw new InvalidOperationException(
                        "MapWinGIS không nạp được " +
                        "ranh giới địa bàn.\n" +
                        gdalError);
                }

                const string layerName =
                    "WEB MẪU - Ranh giới 30 địa bàn " +
                    "(mô phỏng)";

                axMap2.set_LayerName(
                    handle,
                    layerName);

                _loadedLayers[
                    WEB_DIA_BAN_KEY] = handle;

                SetLoadedLayerProjection32648(handle);

                // Chỉ vẽ đường ranh giới để
                // không che mất ảnh vệ tinh và điểm GIS.
                Shapefile sf =
                    axMap2.get_Shapefile(handle);

                if (sf != null)
                {
                    sf.DefaultDrawingOptions.FillVisible =
                        false;

                    sf.DefaultDrawingOptions.LineColor =
                        unchecked(
                            (uint)ColorTranslator.ToOle(
                                Color.FromArgb(
                                    56, 189, 248)));

                    sf.DefaultDrawingOptions.LineWidth =
                        1.3f;

                    if (sf.Labels != null)
                    {
                        sf.Labels.Visible = false;
                    }
                }

                axMap2.set_LayerVisible(
                    handle,
                    true);

                ThemLopDiaBanVaoCay();

                try
                {
                    RefreshLayerCombo();
                }
                catch
                {
                }

                // Thu phóng lúc nạp đầu tiên.
                axMap2.ZoomToLayer(handle);
                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã nạp 30 ranh giới địa bàn " +
                        "mô phỏng từ web_mau.";
                }

                // Đồng bộ nhãn nếu người dùng
                // đã bật chức năng trước đó.
                if (_webNhanDiaBanItem != null &&
                    _webNhanDiaBanItem.Checked)
                {
                    BatTatNhanDiaBanWebGis(true);
                }
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Nạp ranh giới Web mẫu",
                    ex);
            }
        }

        // ========================================================
        // 6. ĐƯA RANH GIỚI VÀO CÂY LỚP GIS
        // ========================================================
        private void ThemLopDiaBanVaoCay()
        {
            if (treeLayers == null)
                return;

            TreeNode[] existing =
                treeLayers.Nodes.Find(
                    WEB_DIA_BAN_KEY,
                    true);

            if (existing.Length > 0)
            {
                SetLayerNodeChecked(
                    existing[0],
                    true);

                return;
            }

            TreeNode node = new TreeNode(
                "WEB MẪU - 30 ranh giới mô phỏng");

            node.Name = WEB_DIA_BAN_KEY;

            // Theo cơ chế quản lý lớp hiện tại,
            // FILE nghĩa là lớp đã nạp trực tiếp.
            // Dữ liệu gốc vẫn nằm trong PostGIS.
            node.Tag = "FILE";

            treeLayers.Nodes.Add(node);

            SetLayerNodeChecked(
                node,
                true);
        }

        // ========================================================
        // 7. ẨN LỚP 30 ĐỊA BÀN
        // ========================================================
        private void AnDiaBanWebGis()
        {
            int handle = LayHandleDiaBanWebGis();

            if (handle < 0)
                return;

            try
            {
                axMap2.set_LayerVisible(
                    handle,
                    false);

                DongBoNutLopWeb(
                    WEB_DIA_BAN_KEY,
                    false);

                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã ẩn ranh giới địa bàn Web mẫu.";
                }
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Ẩn ranh giới địa bàn",
                    ex);
            }
        }

        // ========================================================
        // 8. THU PHÓNG TỚI ĐỊA BÀN
        // ========================================================
        private void ThuPhongDiaBanWebGis()
        {
            int handle = LayHandleDiaBanWebGis();

            if (handle < 0)
            {
                HienThiDiaBanWebGis();

                return;
            }

            try
            {
                AnTongQuan();

                axMap2.set_LayerVisible(
                    handle,
                    true);

                DongBoNutLopWeb(
                    WEB_DIA_BAN_KEY,
                    true);

                axMap2.ZoomToLayer(handle);
                axMap2.Redraw();

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Đã thu phóng tới " +
                        "30 địa bàn Web mẫu.";
                }
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Thu phóng địa bàn",
                    ex);
            }
        }

        // ========================================================
        // 9. BẬT / TẮT NHÃN 30 ĐỊA BÀN
        // ========================================================
        private void BatTatNhanDiaBanWebGis(bool hien)
        {
            int handle = LayHandleDiaBanWebGis();

            if (handle < 0 && hien)
            {
                HienThiDiaBanWebGis();

                handle = LayHandleDiaBanWebGis();
            }

            if (handle < 0)
                return;

            try
            {
                Shapefile sf =
                    axMap2.get_Shapefile(handle);

                if (sf == null ||
                    sf.Labels == null)
                {
                    return;
                }

                if (hien)
                {
                    sf.Labels.Clear();

                    int count = sf.Labels.Generate(
                        "[ten_dia_ban]",
                        tkLabelPositioning.lpCentroid,
                        true);

                    DinhDangNhanWeb(sf);

                    sf.Labels.Visible = count > 0;
                }
                else
                {
                    sf.Labels.Visible = false;
                }

                axMap2.Redraw();
            }
            catch (Exception ex)
            {
                ThongBaoLoiWeb(
                    "Nhãn địa bàn",
                    ex);
            }
        }

        // ========================================================
        // 10. ĐỊNH DẠNG NHÃN GIS GIỐNG WEB
        // ========================================================
        private void DinhDangNhanWeb(Shapefile sf)
        {
            if (sf == null ||
                sf.Labels == null)
            {
                return;
            }

            sf.Labels.FontName = "Segoe UI";
            sf.Labels.FontSize = 8;
            sf.Labels.FontBold = true;

            sf.Labels.FontColor =
                unchecked(
                    (uint)ColorTranslator.ToOle(
                        Color.White));

            sf.Labels.HaloVisible = true;

            sf.Labels.HaloColor =
                unchecked(
                    (uint)ColorTranslator.ToOle(
                        Color.FromArgb(15, 23, 42)));

            sf.Labels.HaloSize = 2;

            sf.Labels.FrameVisible = false;

            sf.Labels.AvoidCollisions = true;

            sf.Labels.CollisionBuffer = 8;
        }

        // ========================================================
        // 11. THÔNG BÁO LỖI
        // ========================================================
        private void ThongBaoLoiWeb(
            string chucNang,
            Exception ex)
        {
            if (lblStatus != null)
            {
                lblStatus.Text =
                    "Lỗi lớp Web mẫu: " + chucNang;
            }

            MessageBox.Show(
                this,
                "Chức năng: " + chucNang +
                "\n\nChi tiết:\n" + ex.Message,
                "Bản đồ Web GIS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
