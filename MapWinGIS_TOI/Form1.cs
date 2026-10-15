using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MapWinGIS;

namespace MapWinGIS_TOI
{
    // ============================================================
    // FORM1.CS - KHOI TAO HE THONG CSDL GIS HA NOI
    //
    // Cong nghe:
    // - C# Windows Forms
    // - MapWinGIS ActiveX
    // - PostgreSQL / PostGIS
    // - He toa do EPSG:32648
    //
    // Phan he:
    // 1. Giao dien chinh
    // 2. Ban do GIS
    // 3. Tra cuu don vi
    // 4. Quan ly du lieu
    // 5. Phan tich khong gian
    // 6. Tong quan va thong ke
    // 7. Du lieu ho so tham chieu
    //
    // LUU Y:
    // - Form1.cs chi chua khoi tao he thong.
    // - Phan he ban do rieng chua cac ham nap diem GIS.
    // - Khong khai bao lai InitBanDoWebGIS() trong file nay.
    // - Khong khai bao lai HienThiDiaBanWebGis().
    // ============================================================

    public partial class Form1 : Form
    {
        // ========================================================
        // 1. BIEN DUNG CHUNG TOAN UNG DUNG
        // ========================================================

        // Danh sach cac lop da duoc nap vao MapWinGIS.
        // Duoc su dung boi:
        // - LayerManager.cs
        // - Map.cs
        // - PostGIS.cs
        // - Cac phan he du lieu va ban do da tich hop
        private readonly Dictionary<string, int>
            _loadedLayers =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

        // Cau hinh chung cho MapWinGIS.
        private readonly GlobalSettings
            _globalSettings =
                new GlobalSettings();

        // Kiem soat khoi tao, tranh goi trung.
        private bool _applicationInitialized = false;

        private bool _applicationInitializing = false;

        // ========================================================
        // 2. CONSTRUCTOR
        // ========================================================

        public Form1()
        {
            InitializeComponent();

            // Khong dang ky lai Form1_Load.
            // Su kien nay da duoc gan trong Form1.Designer.cs.

            // Dam bao nut PostGIS khong bi dang ky
            // cung mot handler nhieu lan.
            if (btnPostGIS != null)
            {
                btnPostGIS.Click -= btnPostGIS_Click;

                btnPostGIS.Click += btnPostGIS_Click;
            }

            // Su kien truoc khi dong Form.
            FormClosing -= Form1_FormClosing;

            FormClosing += Form1_FormClosing;

            // Su kien sau khi Form da dong.
            FormClosed -= Form1_FormClosed;

            FormClosed += Form1_FormClosed;

            // Phim tat du phong quan ly lop GIS: Ctrl + Shift + L.
            // Khong tao them ham OnShown() de tranh trung voi
            // cac file partial class Form1 khac.
            KeyPreview = true;
            KeyDown -= Form1_PhimTatQuanLyLopGIS;
            KeyDown += Form1_PhimTatQuanLyLopGIS;
        }

        // ========================================================
        // 3. KHOI TAO TOAN BO UNG DUNG
        // ========================================================

        private void Form1_Load(
            object sender,
            EventArgs e)
        {
            if (_applicationInitialized ||
                _applicationInitializing)
            {
                return;
            }

            _applicationInitializing = true;

            string buocDangThucHien =
                "Bắt đầu khởi tạo hệ thống";

            try
            {
                // ================================================
                // BUOC 01 - THIET LAP BO CUC
                // ================================================

                buocDangThucHien =
                    "01 - Thiết lập bố cục";

                InitializeLayout();

                // ================================================
                // BUOC 02 - CAU HINH MAPWINGIS
                // ================================================

                buocDangThucHien =
                    "02 - Cấu hình MapWinGIS";

                InitializeMapWinGISSettings();

                // ================================================
                // BUOC 03 - DIEU HUONG BAN DO
                // ================================================

                buocDangThucHien =
                    "03 - Khởi tạo điều hướng";

                InitNavigationTools();

                // ================================================
                // BUOC 04 - KHOI TAO BAN DO HA NOI
                // ================================================

                buocDangThucHien =
                    "04 - Khởi tạo bản đồ Hà Nội";

                // Giu ten ham de tuong thich voi Map.cs.
                // Ung dung van su dung nen ve tinh cuc bo.
                InitOpenStreetMap();

                // ================================================
                // BUOC 05 - QUAN LY CAC LOP GIS
                // ================================================

                buocDangThucHien =
                    "05 - Quản lý lớp GIS";

                InitLayerManager();

                // Dam bao nen ve tinh duoc dat
                // ben duoi cac lop du lieu GIS.
                EnsureSatelliteBasemapBelowPostGIS();

                // ================================================
                // BUOC 06 - CAC CONG CU GIS
                // ================================================

                buocDangThucHien =
                    "06 - Công cụ truy vấn";

                InitQueryTools();

                buocDangThucHien =
                    "07 - Công cụ đo đạc";

                InitMeasureTools();

                // ================================================
                // BUOC 07 - MENU NGHIEP VU
                // ================================================

                buocDangThucHien =
                    "08 - Khởi tạo menu chính";

                InitMainMenu();

                buocDangThucHien =
                    "09 - Công cụ nền vệ tinh";

                InitSatelliteBasemapTools();

                // ================================================
                // BUOC 08 - CAC PHAN HE QUAN LY
                // ================================================

                buocDangThucHien =
                    "10 - Chỉnh sửa dữ liệu";

                InitEditingTools();

                buocDangThucHien =
                    "11 - Tra cứu đơn vị";

                InitUnitSearchTools();

                buocDangThucHien =
                    "12 - Phân tích không gian";

                InitSpatialQueryTools();

                // ================================================
                // BUOC 09 - GIAO DIEN CHINH
                // ================================================

                buocDangThucHien =
                    "13 - Xây dựng giao diện chính";

                ApplyProfessionalUI();

                // ================================================
                // BUOC 10 - TONG QUAN
                // ================================================

                // Phai khoi tao SAU ApplyProfessionalUI
                // de sidebar va vung noi dung da ton tai.

                buocDangThucHien =
                    "14 - Khởi tạo Tổng quan";

                InitTongQuanModule();

                // ================================================
                // BUOC 11 - DU LIEU HO SO THAM CHIEU
                // ================================================

                buocDangThucHien =
                    "15 - Khởi tạo dữ liệu hồ sơ tham chiếu";

                try
                {
                    // Khoi tao phan he tham khao ho so don vi
                    // va dia ban hanh chinh da nhap vao PostGIS.
                    KhoiTaoDuLieuWebMau();
                }
                catch (Exception duLieuEx)
                {
                    GhiLoiKhoiTaoForm1(
                        "Khởi tạo dữ liệu tham chiếu",
                        duLieuEx);
                }

                // ================================================
                // BUOC 12 - LOP DU LIEU BAN DO GIS
                // ================================================

                // Ham InitBanDoWebGIS() duoc dinh nghia
                // trong phan he ban do GIS hien tai.
                //
                // Khong copy noi dung ham do vao Form1.cs.

                buocDangThucHien =
                    "16 - Khởi tạo lớp bản đồ GIS";

                try
                {
                    InitBanDoWebGIS();
                }
                catch (Exception lopGISEx)
                {
                    GhiLoiKhoiTaoForm1(
                        "Khởi tạo lớp bản đồ GIS",
                        lopGISEx);
                }

                // ================================================
                // BUOC 13 - THANH THU PHONG DOC
                // ================================================

                buocDangThucHien =
                    "17 - Thanh thu phóng";

                InitVerticalZoomSlider();

                // ================================================
                // BUOC 14 - THANH TRANG THAI
                // ================================================

                buocDangThucHien =
                    "18 - Thanh trạng thái";

                InitStatusBar();

                // ================================================
                // BUOC 15 - GIAO DIEN HE THONG VA DIEU KHIEN LOP
                // ================================================

                buocDangThucHien =
                    "19 - Hoàn thiện giao diện hệ thống";

                // Cac Control giao dien va thanh dieu huong
                // phai san sang truoc khi dang ky bang lop.
                ApDungGiaoDienHeThong();

                // Bang dieu khien lop la chuc nang phu tro.
                // Neu gap loi, khong dung ban do va Tong quan.
                try
                {
                    KhoiTaoBangDieuKhienLopGIS();
                }
                catch (Exception loiBangLop)
                {
                    GhiLoiKhoiTaoForm1(
                        "Khởi tạo bảng điều khiển lớp GIS",
                        loiBangLop);
                }

                // ================================================
                // BUOC 15.1 - DANG KY NAP DU LIEU BAN DO GIS V3
                // ================================================

                // Phai dang ky SAU ApDungGiaoDienHeThong()
                // de bat duoc su kien cua nut Ban do GIS.
                // Ham nay chi DANG KY su kien; viec nap
                // du lieu dien ra khi chon Ban do GIS.
                buocDangThucHien =
                    "19.1 - Đăng ký dữ liệu bản đồ GIS";

                try
                {
                    KhoiTaoBanDoGISTuDongNapDuLieu();
                }
                catch (Exception loiNapBanDo)
                {
                    GhiLoiKhoiTaoForm1(
                        "Đăng ký tự nạp dữ liệu bản đồ GIS",
                        loiNapBanDo);
                }

                // ================================================
                // BUOC 16 - DONG BO TRANG THAI BAN DO
                // ================================================
                buocDangThucHien =
                    "20 - Đồng bộ danh sách lớp";

                RefreshLayerCombo();

                buocDangThucHien =
                    "21 - Cập nhật lớp hiện tại";

                UpdateCurrentLayerStatus();

                // ================================================
                // BUOC 17 - CON TRO BAN DO MAC DINH
                // ================================================

                buocDangThucHien =
                    "22 - Cấu hình con trỏ bản đồ";

                if (axMap2 != null)
                {
                    axMap2.CursorMode =
                        tkCursorMode.cmPan;
                }

                // ================================================
                // BUOC 18 - HE TOA DO
                // ================================================

                if (lblStatusProjection != null)
                {
                    lblStatusProjection.Text =
                        "CRS: EPSG:32648 | mét";
                }

                // ================================================
                // BUOC 19 - THONG BAO SAN SANG
                // ================================================

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Sẵn sàng | Hà Nội | " +
                        "EPSG:32648 | " +
                        "PostGIS | " +
                        "Dữ liệu minh họa chưa xác minh";
                }

                // Chi danh dau hoan tat
                // sau khi cac buoc da chay xong.
                _applicationInitialized = true;
            }
            catch (Exception ex)
            {
                _applicationInitialized = false;

                GhiLoiKhoiTaoForm1(
                    buocDangThucHien,
                    ex);

                if (lblStatus != null)
                {
                    lblStatus.Text =
                        "Lỗi khởi tạo tại: " +
                        buocDangThucHien;
                }

                MessageBox.Show(
                    this,
                    "Không thể khởi tạo chương trình.\n\n" +
                    "Bước gặp lỗi:\n" +
                    buocDangThucHien +
                    "\n\nChi tiết:\n" +
                    ex.Message,
                    "Lỗi khởi tạo Form1",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _applicationInitializing = false;
            }
        }

        // ========================================================
        // PHIM TAT DU PHONG MO BANG QUAN LY CAC LOP GIS
        // ========================================================

        private void Form1_PhimTatQuanLyLopGIS(
            object sender,
            KeyEventArgs e)
        {
            if (e == null || !e.Control || !e.Shift ||
                e.KeyCode != Keys.L)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (!_applicationInitialized || IsDisposed || Disposing)
                return;

            try
            {
                MoBangDieuKhienLopGIS();
            }
            catch (Exception ex)
            {
                GhiLoiKhoiTaoForm1(
                    "Mở bảng điều khiển lớp GIS",
                    ex);

                MessageBox.Show(
                    this,
                    "Chưa thể mở bảng điều khiển lớp GIS.\n\n" +
                    ex.Message,
                    "Quản lý lớp GIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ========================================================
        // 4. GHI NHAT KY LOI KHOI TAO
        // ========================================================

        private void GhiLoiKhoiTaoForm1(
            string buoc,
            Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
                    "FORM1 | " +
                    buoc +
                    " | " +
                    ex.ToString());
            }
            catch
            {
                // Khong de loi ghi nhat ky
                // lam dung qua trinh khoi tao.
            }
        }

        // ========================================================
        // 5. THIET LAP BO CUC CHINH
        // ========================================================

        private void InitializeLayout()
        {
            if (mainContainer != null)
            {
                const int tocWidth = 275;

                mainContainer.FixedPanel =
                    FixedPanel.Panel1;

                mainContainer.IsSplitterFixed = false;

                if (mainContainer.Width >
                    tocWidth + 500)
                {
                    mainContainer.SplitterDistance =
                        tocWidth;
                }
            }

            if (axMap2 != null)
            {
                axMap2.Dock =
                    DockStyle.Fill;
            }

            if (lblMapPlaceholder != null)
            {
                lblMapPlaceholder.Visible =
                    false;
            }

            if (panelLayerBar != null)
            {
                panelLayerBar.Visible =
                    true;
            }
        }

        // ========================================================
        // 6. CAU HINH MAPWINGIS
        // ========================================================

        private void InitializeMapWinGISSettings()
        {
            // Khong tu dong chuyen doi he toa do.
            _globalSettings.ReprojectLayersOnAdding =
                false;

            // Khong cho phep lop khac he toa do
            // duoc them mac dinh.
            _globalSettings.AllowProjectionMismatch =
                false;

            // Khong tu dong them lop thieu CRS.
            _globalSettings.AllowLayersWithoutProjections =
                false;
        }

        // ========================================================
        // 7. XU LY KHI DONG CHUONG TRINH
        // ========================================================

        private void Form1_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (e != null && e.Cancel)
            {
                return;
            }

            // ------------------------------------------
            // Dung cong cu chinh sua.
            // ------------------------------------------

            try
            {
                StopEditingTools();
            }
            catch (Exception ex)
            {
                GhiLoiKhoiTaoForm1(
                    "Dừng chỉnh sửa",
                    ex);
            }

            // ------------------------------------------
            // Dung thanh trang thai.
            // ------------------------------------------

            try
            {
                StopStatusBar();
            }
            catch (Exception ex)
            {
                GhiLoiKhoiTaoForm1(
                    "Dừng thanh trạng thái",
                    ex);
            }

            // ------------------------------------------
            // Dung cac tien trinh nen ban do.
            // ------------------------------------------

            try
            {
                StopOpenStreetMapSafely();
            }
            catch (Exception ex)
            {
                GhiLoiKhoiTaoForm1(
                    "Dừng bản đồ nền",
                    ex);
            }

            if (axMap2 == null)
            {
                return;
            }

            // ------------------------------------------
            // Xoa ket qua do.
            // ------------------------------------------

            try
            {
                if (axMap2.Measuring != null)
                {
                    axMap2.Measuring.Clear();
                }
            }
            catch
            {
            }

            // ------------------------------------------
            // Dua chuot ve che do di chuyen.
            // ------------------------------------------

            try
            {
                axMap2.CursorMode =
                    tkCursorMode.cmPan;
            }
            catch
            {
            }

            // ------------------------------------------
            // Go lop GIS khoi MapWinGIS.
            //
            // KHONG xoa bang PostGIS.
            // ------------------------------------------

            try
            {
                axMap2.RemoveAllLayers();
            }
            catch
            {
            }
        }

        // ========================================================
        // 8. GIAI PHONG TAI NGUYEN SAU KHI DONG FORM
        // ========================================================

        private void Form1_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            try
            {
                // Xoa danh sach handle cac lop da tai.
                _loadedLayers.Clear();

                // Giai phong menu quan ly lop.
                if (_layerContextMenu != null)
                {
                    _layerContextMenu.Dispose();

                    _layerContextMenu = null;
                }
            }
            catch (Exception ex)
            {
                GhiLoiKhoiTaoForm1(
                    "Giải phóng tài nguyên",
                    ex);
            }
            finally
            {
                _applicationInitialized = false;

                _applicationInitializing = false;
            }
        }
    }
}