using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // ============================================================
    // NAP GIS THAM CHIEU THEO YEU CAU - V4.0
    //
    // Khong override OnShown(): tranh loi CS0111.
    // Khong tao thanh cong cu trung voi "Cac lop GIS".
    // Khong dung INSERT, UPDATE, DELETE.
    // ============================================================
    public partial class Form1
    {
        private bool _napGISV3DaDangKy;
        private bool _napGISV3DangChay;
        private string _napGISV3KetQuaCuoi =
            "Chưa thực hiện nạp lớp GIS tham chiếu.";

        // ========================================================
        // 1. DANG KY PHIM TAT - KHONG TU NAP KHI DOI TRANG
        // ========================================================
        private void KhoiTaoBanDoGISTuDongNapDuLieu()
        {
            if (_napGISV3DaDangKy)
                return;

            _napGISV3DaDangKy = true;
            KeyPreview = true;

            // Quan trong: Khong gan Click cho "Ban do GIS".
            // Click cua menu chi chuyen trang, khong tai du lieu.
            KeyDown -= Form1_NapGISV3_KeyDown;
            KeyDown += Form1_NapGISV3_KeyDown;
        }

        private void Form1_NapGISV3_KeyDown(
            object sender, KeyEventArgs e)
        {
            if (e == null || !e.Control || !e.Shift ||
                e.KeyCode != Keys.G)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (IsDisposed || Disposing || _napGISV3DangChay)
                return;

            // Chu dong tai moi khi nguoi dung yeu cau.
            NapLopGISV3(true);
        }

        // ========================================================
        // 3. KIEM TRA HANDLE TRONG MAPWINGIS
        // Khong lay NumLayers > 1 lam bang chung du lop diem.
        // ========================================================
        private bool CoLopGISV3(
            string key, string dauHieuTenLop)
        {
            if (axMap2 == null || axMap2.IsDisposed)
                return false;

            int cachedHandle;
            if (_loadedLayers.TryGetValue(key, out cachedHandle))
            {
                try
                {
                    for (int i = 0; i < axMap2.NumLayers; i++)
                    {
                        int realHandle = axMap2.get_LayerHandle(i);
                        if (realHandle != cachedHandle)
                            continue;

                        string actualName =
                            axMap2.get_LayerName(realHandle);

                        if (!string.IsNullOrWhiteSpace(actualName) &&
                            actualName.IndexOf(
                                dauHieuTenLop,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[GIS] Kiem tra handle: " + ex);
                }

                // Chi go khoi bo nho quan ly; khong xoa lop GIS.
                _loadedLayers.Remove(key);
            }

            if (string.IsNullOrWhiteSpace(dauHieuTenLop))
                return false;

            try
            {
                for (int i = 0; i < axMap2.NumLayers; i++)
                {
                    int handle = axMap2.get_LayerHandle(i);
                    if (handle < 0)
                        continue;

                    string name = axMap2.get_LayerName(handle) ?? "";
                    if (name.IndexOf(
                            dauHieuTenLop,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _loadedLayers[key] = handle;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GIS] Kiem tra ten lop: " + ex);
            }

            return false;
        }

        // ========================================================
        // 4. NAP CAC LOP THAM CHIEU VA XAC NHAN KET QUA
        // ========================================================
        private void NapLopGISV3(bool yeuCauBaoCao)
        {
            if (_napGISV3DangChay || axMap2 == null ||
                axMap2.IsDisposed)
                return;

            _napGISV3DangChay = true;
            Cursor oldCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            StringBuilder errors = new StringBuilder();
            int beforeCount = axMap2.NumLayers;

            try
            {
                // Khong goi HienTrangBanDo() o day.
                // Nguoi dung phai mo Ban do GIS truoc khi nhan Ctrl+Shift+G.
                // Tranh chuyen trang/truyen ActiveX lap lai.

                bool hasAreas = CoLopGISV3(
                    "web_mau_quan_huyen",
                    "Địa bàn tham chiếu");

                bool hasPoints = CoLopGISV3(
                    "web_mau_don_vi",
                    "WEB MẪU - Điểm");

                if (!hasAreas)
                {
                    try
                    {
                        HienThiDiaBanWebGis();
                    }
                    catch (Exception ex)
                    {
                        errors.AppendLine(
                            "Địa bàn: " + ex.Message);
                    }
                }

                if (!hasPoints)
                {
                    try
                    {
                        HienThiWebMauTrenBanDo();
                    }
                    catch (Exception ex)
                    {
                        errors.AppendLine(
                            "Điểm đơn vị: " + ex.Message);
                    }
                }

                // Cac ham nap lop da tu ve lai MapWinGIS.
                // Khong goi EnsureSatelliteBasemapBelowPostGIS()
                // hoac Redraw() lan nua trong cung mot thao tac.

                bool areasNow = CoLopGISV3(
                    "web_mau_quan_huyen",
                    "Địa bàn tham chiếu");

                bool pointsNow = CoLopGISV3(
                    "web_mau_don_vi",
                    "WEB MẪU - Điểm");

                int afterCount = axMap2.NumLayers;

                StringBuilder report = new StringBuilder();
                report.AppendLine(
                    "KẾT QUẢ KIỂM TRA LỚP BẢN ĐỒ GIS");
                report.AppendLine();
                report.AppendLine(
                    "Lớp điểm tham chiếu: " +
                    (pointsNow ? "Đã phát hiện" : "Chưa phát hiện"));
                report.AppendLine(
                    "Lớp địa bàn tham chiếu: " +
                    (areasNow ? "Đã phát hiện" : "Chưa phát hiện"));
                report.AppendLine(
                    "Tổng số lớp trước / sau: " +
                    beforeCount + " / " + afterCount);
                report.AppendLine("Hệ tọa độ dự án: EPSG:32648");
                report.AppendLine();
                report.AppendLine(
                    "Lưu ý: Dữ liệu tham chiếu chưa được " +
                    "xác minh vị trí thực địa.");

                if (errors.Length > 0)
                {
                    report.AppendLine();
                    report.AppendLine("LỖI KHI NẠP:");
                    report.Append(errors.ToString());
                }

                if (!pointsNow || !areasNow)
                {
                    report.AppendLine();
                    report.AppendLine("CÁC LỚP ĐANG CÓ:");
                    try
                    {
                        for (int i = 0; i < axMap2.NumLayers; i++)
                        {
                            int handle = axMap2.get_LayerHandle(i);
                            if (handle < 0) continue;
                            report.AppendLine("- " +
                                axMap2.get_LayerName(handle));
                        }
                    }
                    catch (Exception ex)
                    {
                        report.AppendLine(
                            "Không đọc được danh sách lớp: " +
                            ex.Message);
                    }
                }

                _napGISV3KetQuaCuoi = report.ToString();

                if (lblStatus != null)
                {
                    lblStatus.Text = pointsNow && areasNow
                        ? "Đã kiểm tra lớp điểm và địa bàn GIS tham chiếu. " +
                          "Các vị trí chưa xác minh."
                        : "Chưa nạp đủ hai lớp tham chiếu. " +
                          "Nhấn Ctrl+Shift+G để xem báo cáo.";
                }

                Debug.WriteLine("[GIS] " + _napGISV3KetQuaCuoi);

                // Tu dong: chi hien popup khi co loi.
                // Thu cong: luon hien bao cao chi tiet.
                if (yeuCauBaoCao ||
                    (!pointsNow || !areasNow || errors.Length > 0))
                {
                    MessageBox.Show(
                        this,
                        _napGISV3KetQuaCuoi,
                        "Kiểm tra dữ liệu bản đồ GIS",
                        MessageBoxButtons.OK,
                        pointsNow && areasNow && errors.Length == 0
                            ? MessageBoxIcon.Information
                            : MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _napGISV3KetQuaCuoi = ex.ToString();
                Debug.WriteLine("[GIS] " + ex);
                MessageBox.Show(
                    this,
                    "Không thể nạp dữ liệu GIS.\n\n" + ex.Message,
                    "Lỗi dữ liệu bản đồ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = oldCursor;
                _napGISV3DangChay = false;
            }
        }
    }
}