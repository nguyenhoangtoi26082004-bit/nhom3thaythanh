using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    // =============================================================
    // OBJECTINFOPANEL.CS
    //
    // Thẻ thông tin nổi trên bản đồ khi người dùng dùng công cụ
    // "Thông tin đối tượng" và bấm vào một đối tượng GIS.
    //
    // Không dùng panel truy vấn cũ trong Designer.
    // =============================================================
    public partial class Form1
    {
        private Panel _objectInfoPanel;
        private System.Windows.Forms.Label _objectInfoTitle;
        private System.Windows.Forms.Label _objectInfoSubtitle;
        private FlowLayoutPanel _objectInfoBody;
        private Button _objectInfoCloseButton;
        private Button _objectInfoPanButton;


        // =========================================================
        // HIỂN THỊ THẺ THÔNG TIN
        // =========================================================
        private void ShowObjectInfoCard(
            string layerKey,
            string layerName,
            int shapeIndex,
            double mapX,
            double mapY,
            Dictionary<string, string> attributes)
        {
            if (panelMap == null)
            {
                return;
            }

            EnsureObjectInfoPanel();

            if (_objectInfoPanel == null)
            {
                return;
            }

            bool isUnit =
                string.Equals(
                    layerKey,
                    "don_vi_quan_doi",
                    StringComparison.OrdinalIgnoreCase);

            string displayLayer =
                !string.IsNullOrWhiteSpace(
                    layerKey)
                    ? GetPostGISDisplayName(
                        layerKey)
                    : layerName;

            if (string.IsNullOrWhiteSpace(
                displayLayer))
            {
                displayLayer =
                    "Đối tượng GIS";
            }

            string objectName =
                ResolveObjectDisplayName(
                    attributes);

            _objectInfoTitle.Text =
                isUnit
                    ? "HỒ SƠ ĐƠN VỊ"
                    : "THÔNG TIN ĐỐI TƯỢNG";

            _objectInfoSubtitle.Text =
                !string.IsNullOrWhiteSpace(
                    objectName)
                    ? objectName
                    : displayLayer;

            _objectInfoBody.SuspendLayout();

            try
            {
                _objectInfoBody.Controls.Clear();

                if (isUnit)
                {
                    BuildUnitProfileInfo(
                        attributes,
                        mapX,
                        mapY);
                }
                else
                {
                    AddObjectInfoSection(
                        "Lớp dữ liệu",
                        displayLayer,
                        true);

                    if (attributes != null)
                    {
                        foreach (string fieldName
                            in GetPreferredInfoFields(
                                layerKey,
                                attributes))
                        {
                            string value;

                            if (!attributes.TryGetValue(
                                fieldName,
                                out value))
                            {
                                continue;
                            }

                            if (ShouldHideInfoField(
                                fieldName,
                                value))
                            {
                                continue;
                            }

                            AddObjectInfoSection(
                                GetFriendlyInfoFieldName(
                                    fieldName),
                                string.IsNullOrWhiteSpace(
                                    value)
                                    ? "—"
                                    : value,
                                false);
                        }
                    }
                }
            }
            finally
            {
                _objectInfoBody.ResumeLayout(
                    true);
            }

            PositionObjectInfoPanel();

            _objectInfoPanel.Visible =
                true;

            _objectInfoPanel.BringToFront();

            panelMap.Controls.SetChildIndex(
                _objectInfoPanel,
                0);
        }


        // =========================================================
        // HỒ SƠ ĐƠN VỊ - HIỂN THỊ THEO NHÓM NGHIỆP VỤ
        // =========================================================
        private void BuildUnitProfileInfo(
            Dictionary<string, string> attributes,
            double mapX,
            double mapY)
        {
            if (attributes == null)
            {
                attributes =
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);
            }

            // -----------------------------------------------------
            // 1. CHẤT LƯỢNG DỮ LIỆU
            // -----------------------------------------------------
            string dataQuality =
                GetInfoValue(
                    attributes,
                    "chat_luong_du_lieu");

            if (string.IsNullOrWhiteSpace(
                dataQuality))
            {
                dataQuality =
                    ResolveUnitDataQuality(
                        attributes);
            }

            string dataWarning =
                GetInfoValue(
                    attributes,
                    "canh_bao_du_lieu");

            AddUnitDataQualityCard(
                dataQuality,
                dataWarning);

            // -----------------------------------------------------
            // 2. THÔNG TIN CHUNG
            // -----------------------------------------------------
            AddObjectInfoGroupHeader(
                "THÔNG TIN CHUNG");

            AddUnitInfoField(
                attributes,
                "Mã hồ sơ",
                "ma_don_vi");

            AddUnitInfoField(
                attributes,
                "Tên đơn vị",
                "ten_don_vi");

            AddUnitInfoField(
                attributes,
                "Tên viết tắt",
                "ten_viet_tat");

            AddUnitInfoField(
                attributes,
                "Ký hiệu tham chiếu",
                "ky_hieu_bien_so");

            // -----------------------------------------------------
            // 3. PHÂN LOẠI
            // -----------------------------------------------------
            AddObjectInfoGroupHeader(
                "PHÂN LOẠI");

            AddUnitInfoField(
                attributes,
                "Cấp đơn vị",
                "ten_cap");

            AddUnitInfoField(
                attributes,
                "Loại đơn vị",
                "ten_loai");

            AddUnitInfoField(
                attributes,
                "Đơn vị cấp trên",
                "ten_don_vi_cap_tren");

            // -----------------------------------------------------
            // 4. ĐỊA BÀN
            // -----------------------------------------------------
            AddObjectInfoGroupHeader(
                "ĐỊA BÀN VÀ VỊ TRÍ GIS");

            string xa =
                GetInfoValue(
                    attributes,
                    "ten_xa_phuong");

            string loaiXa =
                GetInfoValue(
                    attributes,
                    "loai_xa_phuong");

            if (!string.IsNullOrWhiteSpace(
                    xa))
            {
                string diaBan =
                    string.IsNullOrWhiteSpace(
                        loaiXa)
                        ? xa
                        : GetFriendlyAdministrativeValue(
                              loaiXa) +
                          " " +
                          xa;

                AddObjectInfoSection(
                    "Xã/Phường",
                    diaBan,
                    false);
            }

            AddUnitInfoField(
                attributes,
                "Thành phố",
                "tinh_thanh_pho");

            string address =
                GetInfoValue(
                    attributes,
                    "dia_chi_hien_hanh");

            if (!string.IsNullOrWhiteSpace(
                    address)
                &&
                !IsPlaceholderInfoValue(
                    address))
            {
                string addressCaption =
                    GetInfoValue(
                        attributes,
                        "nhan_dia_chi_hien_thi");

                if (string.IsNullOrWhiteSpace(
                    addressCaption))
                {
                    addressCaption =
                        GetUnitAddressCaption(
                            dataQuality);
                }

                AddObjectInfoSection(
                    addressCaption,
                    address,
                    false);
            }

            // -----------------------------------------------------
            // VỊ TRÍ GIS
            //
            // Điểm hiển thị trên bản đồ là điểm đại diện hành chính
            // theo xã/phường đã gán cho đơn vị. Không coi đây là
            // tọa độ chính xác của trụ sở/cổng/cơ sở.
            // -----------------------------------------------------
            string gisPositionLabel =
                BuildUnitRepresentativePositionLabel(
                    xa,
                    loaiXa,
                    dataQuality);

            AddObjectInfoSection(
                "Loại vị trí GIS",
                gisPositionLabel,
                false);

            AddObjectInfoSection(
                "Ý nghĩa điểm bản đồ",
                GetUnitRepresentativePositionNote(
                    dataQuality),
                false);

            // -----------------------------------------------------
            // 5. QUẢN LÝ DỮ LIỆU
            // -----------------------------------------------------
            AddObjectInfoGroupHeader(
                "QUẢN LÝ DỮ LIỆU");

            string status =
                GetInfoValue(
                    attributes,
                    "trang_thai_quan_ly");

            if (!string.IsNullOrWhiteSpace(
                    status))
            {
                AddObjectInfoSection(
                    "Trạng thái quản lý",
                    GetFriendlyStatusValue(
                        status),
                    false);
            }

            string verification =
                GetInfoValue(
                    attributes,
                    "muc_do_xac_minh");

            if (!string.IsNullOrWhiteSpace(
                    verification))
            {
                AddObjectInfoSection(
                    "Mức độ dữ liệu",
                    GetFriendlyVerificationValue(
                        verification),
                    false);
            }

            string verificationDate =
                GetInfoValue(
                    attributes,
                    "ngay_xac_minh");

            if (!string.IsNullOrWhiteSpace(
                    verificationDate))
            {
                AddObjectInfoSection(
                    string.Equals(
                        dataQuality,
                        "da_doi_chieu_cong_khai",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Ngày đối chiếu"
                        : "Ngày cập nhật tham chiếu",
                    verificationDate,
                    false);
            }

            AddUnitInfoField(
                attributes,
                "Nguồn tham khảo",
                "nguon_tham_khao");

            AddUnitInfoField(
                attributes,
                "Ghi chú",
                "ghi_chu");

            // Point chỉ là điểm đại diện phục vụ GIS,
            // không mặc định là tọa độ chính xác của cơ sở.
        }


        // =========================================================
        // MÔ TẢ ĐIỂM ĐẠI DIỆN GIS
        // =========================================================
        private string BuildUnitRepresentativePositionLabel(
            string xa,
            string loaiXa,
            string dataQuality)
        {
            if (string.Equals(
                dataQuality,
                "can_ra_soat_pham_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Điểm đang lưu - cần rà soát phạm vi";
            }

            if (!string.IsNullOrWhiteSpace(
                    xa))
            {
                string adminType =
                    GetFriendlyAdministrativeValue(
                        loaiXa);

                if (string.IsNullOrWhiteSpace(
                    adminType))
                {
                    return
                        "Điểm đại diện hành chính của " +
                        xa;
                }

                return
                    "Điểm đại diện hành chính của " +
                    adminType +
                    " " +
                    xa;
            }

            return
                "Điểm đại diện hành chính";
        }


        private string GetUnitRepresentativePositionNote(
            string dataQuality)
        {
            if (string.Equals(
                dataQuality,
                "can_ra_soat_pham_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Điểm này không dùng để khẳng định vị trí hiện hành của đơn vị trên địa bàn Hà Nội.";
            }

            if (string.Equals(
                dataQuality,
                "da_doi_chieu_cong_khai",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Địa chỉ công khai đã được đối chiếu, nhưng điểm trên bản đồ vẫn chỉ là điểm đại diện theo xã/phường; không phải tọa độ chính xác của trụ sở.";
            }

            return
                "Điểm trên bản đồ chỉ phục vụ tra cứu GIS theo địa bàn; không phải tọa độ chính xác của trụ sở.";
        }


        // =========================================================
        // PHÂN LOẠI CHẤT LƯỢNG DỮ LIỆU ĐƠN VỊ
        // =========================================================
        private string ResolveUnitDataQuality(
            Dictionary<string, string> attributes)
        {
            string id =
                GetInfoValue(
                    attributes,
                    "id");

            if (id == "42")
            {
                return
                    "can_ra_soat_pham_vi";
            }

            string source =
                GetInfoValue(
                    attributes,
                    "nguon_tham_khao");

            if (!string.IsNullOrWhiteSpace(
                    source)
                &&
                (
                    source.IndexOf(
                        "http://",
                        StringComparison.OrdinalIgnoreCase) >= 0
                    ||
                    source.IndexOf(
                        "https://",
                        StringComparison.OrdinalIgnoreCase) >= 0
                ))
            {
                return
                    "da_doi_chieu_cong_khai";
            }

            return
                "tham_chieu_hanh_chinh";
        }


        private string GetUnitAddressCaption(
            string dataQuality)
        {
            if (string.Equals(
                dataQuality,
                "da_doi_chieu_cong_khai",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Địa chỉ công khai";
            }

            if (string.Equals(
                dataQuality,
                "can_ra_soat_pham_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Địa bàn hiện lưu";
            }

            return
                "Địa bàn tham chiếu";
        }


        // =========================================================
        // THẺ TRẠNG THÁI CHẤT LƯỢNG DỮ LIỆU
        // =========================================================
        private void AddUnitDataQualityCard(
            string dataQuality,
            string warning)
        {
            if (_objectInfoBody == null)
            {
                return;
            }

            string title;
            string description;
            Color backgroundColor;
            Color titleColor;

            if (string.Equals(
                dataQuality,
                "da_doi_chieu_cong_khai",
                StringComparison.OrdinalIgnoreCase))
            {
                title =
                    "ĐÃ ĐỐI CHIẾU NGUỒN CÔNG KHAI";

                description =
                    "Địa chỉ đã được đối chiếu từ nguồn công khai. Điểm trên bản đồ vẫn là điểm đại diện hành chính theo xã/phường.";

                backgroundColor =
                    Color.FromArgb(
                        6,
                        78,
                        59);

                titleColor =
                    Color.FromArgb(
                        110,
                        231,
                        183);
            }
            else if (string.Equals(
                dataQuality,
                "can_ra_soat_pham_vi",
                StringComparison.OrdinalIgnoreCase))
            {
                title =
                    "CẦN RÀ SOÁT PHẠM VI";

                description =
                    !string.IsNullOrWhiteSpace(
                        warning)
                        ? warning
                        : "Không sử dụng bản ghi này để khẳng định vị trí hiện hành trên địa bàn Hà Nội.";

                backgroundColor =
                    Color.FromArgb(
                        69,
                        10,
                        10);

                titleColor =
                    Color.FromArgb(
                        252,
                        165,
                        165);
            }
            else
            {
                title =
                    "THAM CHIẾU HÀNH CHÍNH / KHU VỰC";

                description =
                    "Chưa xác minh địa chỉ liên hệ công khai chi tiết; vị trí chỉ dùng cho tra cứu GIS theo địa bàn.";

                backgroundColor =
                    Color.FromArgb(
                        69,
                        45,
                        8);

                titleColor =
                    Color.FromArgb(
                        253,
                        224,
                        71);
            }

            Panel card =
                new Panel();

            card.Width =
                Math.Max(
                    300,
                    _objectInfoBody.ClientSize.Width -
                    28);

            card.Height =
                104;

            card.Margin =
                new Padding(
                    0,
                    0,
                    0,
                    10);

            card.Padding =
                new Padding(
                    12,
                    9,
                    12,
                    9);

            card.BackColor =
                backgroundColor;

            System.Windows.Forms.Label lblTitle =
                new System.Windows.Forms.Label();

            lblTitle.Dock =
                DockStyle.Top;

            lblTitle.Height =
                22;

            lblTitle.Text =
                title;

            lblTitle.ForeColor =
                titleColor;

            lblTitle.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            System.Windows.Forms.Label lblDescription =
                new System.Windows.Forms.Label();

            lblDescription.Dock =
                DockStyle.Fill;

            lblDescription.Text =
                description;

            lblDescription.ForeColor =
                Color.White;

            lblDescription.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Regular);

            lblDescription.TextAlign =
                ContentAlignment.TopLeft;

            card.Controls.Add(
                lblDescription);

            card.Controls.Add(
                lblTitle);

            _objectInfoBody.Controls.Add(
                card);
        }


        private void AddUnitInfoField(
            Dictionary<string, string> attributes,
            string caption,
            string fieldName)
        {
            string value =
                GetInfoValue(
                    attributes,
                    fieldName);

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return;
            }

            if (IsPlaceholderInfoValue(
                value))
            {
                return;
            }

            AddObjectInfoSection(
                caption,
                value,
                false);
        }


        private string GetInfoValue(
            Dictionary<string, string> attributes,
            string fieldName)
        {
            if (attributes == null ||
                string.IsNullOrWhiteSpace(
                    fieldName))
            {
                return "";
            }

            string value;

            if (!attributes.TryGetValue(
                    fieldName,
                    out value))
            {
                return "";
            }

            return
                value == null
                    ? ""
                    : value.Trim();
        }


        private bool IsPlaceholderInfoValue(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return true;
            }

            string normalized =
                value.Trim()
                .ToUpperInvariant();

            return
                normalized == "CHƯA XÁC MINH" ||
                normalized == "CHUA XAC MINH" ||
                normalized == "KHÔNG CÓ" ||
                normalized == "KHONG CO" ||
                normalized == "NULL" ||
                normalized == "N/A";
        }


        private string GetFriendlyStatusValue(
            string value)
        {
            string normalized =
                (value ?? "")
                .Trim()
                .ToLowerInvariant();

            if (normalized ==
                "dang_quan_ly")
            {
                return "Đang quản lý";
            }

            if (normalized ==
                "tam_ngung")
            {
                return "Tạm ngừng";
            }

            if (normalized ==
                "khong_con_quan_ly")
            {
                return "Không còn quản lý";
            }

            return
                value ?? "";
        }


        private string GetFriendlyVerificationValue(
            string value)
        {
            string normalized =
                (value ?? "")
                .Trim()
                .ToLowerInvariant();

            if (normalized ==
                    "da_xac_minh"
                ||
                normalized ==
                    "da_xac_minh_cong_khai")
            {
                return
                    "Đã xác minh công khai";
            }

            if (normalized ==
                    "tham_chieu_hanh_chinh"
                ||
                normalized ==
                    "tham_chieu_theo_khu_vuc"
                ||
                normalized ==
                    "tham_chieu_khu_vuc")
            {
                return
                    "Tham chiếu hành chính / khu vực";
            }

            if (normalized ==
                    "doi_chieu_nghi_quyet")
            {
                return
                    "Đối chiếu theo nguồn hành chính";
            }

            if (normalized ==
                    "can_ra_soat_pham_vi")
            {
                return
                    "Cần rà soát phạm vi";
            }

            if (normalized ==
                    "chua_xac_minh")
            {
                return
                    "Chưa xác minh";
            }

            return
                value ?? "";
        }


        private string GetFriendlyAdministrativeValue(
            string value)
        {
            string normalized =
                (value ?? "")
                .Trim()
                .ToLowerInvariant();

            if (normalized ==
                "phuong")
            {
                return "Phường";
            }

            if (normalized ==
                "xa")
            {
                return "Xã";
            }

            return
                value ?? "";
        }


        private void AddObjectInfoGroupHeader(
            string text)
        {
            if (_objectInfoBody == null)
            {
                return;
            }

            System.Windows.Forms.Label label =
                new System.Windows.Forms.Label();

            label.Width =
                Math.Max(
                    285,
                    _objectInfoBody.ClientSize.Width -
                    28);

            label.Height =
                28;

            label.Margin =
                new Padding(
                    0,
                    8,
                    0,
                    5);

            label.Padding =
                new Padding(
                    4,
                    5,
                    0,
                    0);

            label.Text =
                text ?? "";

            label.ForeColor =
                Color.FromArgb(
                    52,
                    211,
                    153);

            label.Font =
                new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold);

            _objectInfoBody.Controls.Add(
                label);
        }


        // =========================================================
        // TẠO PANEL
        // =========================================================
        private void EnsureObjectInfoPanel()
        {
            if (_objectInfoPanel != null &&
                !_objectInfoPanel.IsDisposed)
            {
                return;
            }

            _objectInfoPanel =
                new Panel();

            _objectInfoPanel.Name =
                "panelThongTinDoiTuong";

            _objectInfoPanel.Size =
                new Size(
                    390,
                    570);

            _objectInfoPanel.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            _objectInfoPanel.BorderStyle =
                BorderStyle.FixedSingle;

            _objectInfoPanel.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------
            Panel header =
                new Panel();

            header.Dock =
                DockStyle.Top;

            header.Height =
                76;

            header.Padding =
                new Padding(
                    14,
                    10,
                    42,
                    8);

            header.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            _objectInfoTitle =
                new System.Windows.Forms.Label();

            _objectInfoTitle.Dock =
                DockStyle.Top;

            _objectInfoTitle.Height =
                25;

            _objectInfoTitle.Text =
                "THÔNG TIN ĐỐI TƯỢNG";

            _objectInfoTitle.ForeColor =
                Color.White;

            _objectInfoTitle.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            _objectInfoSubtitle =
                new System.Windows.Forms.Label();

            _objectInfoSubtitle.Dock =
                DockStyle.Fill;

            _objectInfoSubtitle.Text =
                "";

            _objectInfoSubtitle.ForeColor =
                Color.FromArgb(
                    110,
                    231,
                    183);

            _objectInfoSubtitle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Regular);

            _objectInfoSubtitle.AutoEllipsis =
                true;

            _objectInfoSubtitle.TextAlign =
                ContentAlignment.MiddleLeft;

            _objectInfoCloseButton =
                new Button();

            _objectInfoCloseButton.Text =
                "×";

            _objectInfoCloseButton.Size =
                new Size(
                    30,
                    28);

            _objectInfoCloseButton.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            _objectInfoCloseButton.Location =
                new Point(
                    _objectInfoPanel.Width -
                    38,
                    8);

            _objectInfoCloseButton.FlatStyle =
                FlatStyle.Flat;

            _objectInfoCloseButton.FlatAppearance.BorderSize =
                0;

            _objectInfoCloseButton.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            _objectInfoCloseButton.ForeColor =
                Color.White;

            _objectInfoCloseButton.Font =
                new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold);

            _objectInfoCloseButton.Cursor =
                Cursors.Hand;

            _objectInfoCloseButton.Click +=
                delegate
                {
                    HideObjectInfoCard();
                };

            header.Controls.Add(
                _objectInfoSubtitle);

            header.Controls.Add(
                _objectInfoTitle);

            header.Controls.Add(
                _objectInfoCloseButton);

            // -----------------------------------------------------
            // FOOTER
            // -----------------------------------------------------
            Panel footer =
                new Panel();

            footer.Dock =
                DockStyle.Bottom;

            footer.Height =
                48;

            footer.Padding =
                new Padding(
                    10,
                    8,
                    10,
                    8);

            footer.BackColor =
                Color.FromArgb(
                    17,
                    24,
                    39);

            _objectInfoPanButton =
                new Button();

            _objectInfoPanButton.Text =
                "Quay lại di chuyển bản đồ";

            _objectInfoPanButton.Dock =
                DockStyle.Fill;

            _objectInfoPanButton.FlatStyle =
                FlatStyle.Flat;

            _objectInfoPanButton.FlatAppearance.BorderColor =
                Color.FromArgb(
                    71,
                    85,
                    105);

            _objectInfoPanButton.BackColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            _objectInfoPanButton.ForeColor =
                Color.White;

            _objectInfoPanButton.Cursor =
                Cursors.Hand;

            _objectInfoPanButton.Click +=
                delegate
                {
                    HideObjectInfoCard();

                    ActivatePanMode();

                    if (lblStatus != null)
                    {
                        lblStatus.Text =
                            "Đã chuyển về chế độ di chuyển bản đồ.";
                    }
                };

            footer.Controls.Add(
                _objectInfoPanButton);

            // -----------------------------------------------------
            // BODY
            // -----------------------------------------------------
            _objectInfoBody =
                new FlowLayoutPanel();

            _objectInfoBody.Dock =
                DockStyle.Fill;

            _objectInfoBody.FlowDirection =
                FlowDirection.TopDown;

            _objectInfoBody.WrapContents =
                false;

            _objectInfoBody.AutoScroll =
                true;

            _objectInfoBody.Padding =
                new Padding(
                    10,
                    10,
                    10,
                    10);

            _objectInfoBody.BackColor =
                Color.FromArgb(
                    15,
                    23,
                    42);

            _objectInfoPanel.Controls.Add(
                _objectInfoBody);

            _objectInfoPanel.Controls.Add(
                footer);

            _objectInfoPanel.Controls.Add(
                header);

            panelMap.Controls.Add(
                _objectInfoPanel);

            _objectInfoPanel.Visible =
                false;

            PositionObjectInfoPanel();

            panelMap.Resize -=
                panelMap_ObjectInfo_Resize;

            panelMap.Resize +=
                panelMap_ObjectInfo_Resize;
        }


        // =========================================================
        // THÊM 1 TRƯỜNG THÔNG TIN
        // =========================================================
        private void AddObjectInfoSection(
            string caption,
            string value,
            bool accent)
        {
            if (_objectInfoBody == null)
            {
                return;
            }

            Panel row =
                new Panel();

            row.Width =
                Math.Max(
                    285,
                    _objectInfoBody.ClientSize.Width -
                    28);

            row.Height =
                GetObjectInfoRowHeight(
                    value);

            row.Margin =
                new Padding(
                    0,
                    0,
                    0,
                    8);

            row.Padding =
                new Padding(
                    10,
                    7,
                    10,
                    7);

            row.BackColor =
                accent
                    ? Color.FromArgb(
                        6,
                        78,
                        59)
                    : Color.FromArgb(
                        17,
                        24,
                        39);

            System.Windows.Forms.Label lblCaption =
                new System.Windows.Forms.Label();

            lblCaption.Dock =
                DockStyle.Top;

            lblCaption.Height =
                19;

            lblCaption.Text =
                caption;

            lblCaption.ForeColor =
                Color.FromArgb(
                    148,
                    163,
                    184);

            lblCaption.Font =
                new Font(
                    "Segoe UI",
                    8F,
                    FontStyle.Bold);

            System.Windows.Forms.Label lblValue =
                new System.Windows.Forms.Label();

            lblValue.Dock =
                DockStyle.Fill;

            lblValue.Text =
                value ?? "";

            lblValue.ForeColor =
                Color.White;

            lblValue.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    accent
                        ? FontStyle.Bold
                        : FontStyle.Regular);

            lblValue.AutoEllipsis =
                false;

            lblValue.TextAlign =
                ContentAlignment.TopLeft;

            row.Controls.Add(
                lblValue);

            row.Controls.Add(
                lblCaption);

            _objectInfoBody.Controls.Add(
                row);
        }


        private int GetObjectInfoRowHeight(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return 55;
            }

            int lineCount =
                value.Split(
                    new[] { '\n' },
                    StringSplitOptions.None)
                .Length;

            int approximateCharsPerLine =
                42;

            lineCount +=
                Math.Max(
                    0,
                    value.Length /
                    approximateCharsPerLine);

            return
                Math.Max(
                    55,
                    Math.Min(
                        150,
                        45 +
                        lineCount * 18));
        }


        // =========================================================
        // SẮP XẾP CÁC TRƯỜNG QUAN TRỌNG LÊN TRƯỚC
        // =========================================================
        private List<string> GetPreferredInfoFields(
            string layerKey,
            Dictionary<string, string> attributes)
        {
            List<string> result =
                new List<string>();

            string[] preferred;

            if (string.Equals(
                layerKey,
                "don_vi_quan_doi",
                StringComparison.OrdinalIgnoreCase))
            {
                preferred =
                    new[]
                    {
                        "id",
                        "ma_don_vi",
                        "ten_don_vi",
                        "ten_viet_tat",
                        "ky_hieu_bien_so",
                        "ten_cap",
                        "ten_loai",
                        "ten_don_vi_cap_tren",
                        "ma_xa_phuong",
                        "ten_xa_phuong",
                        "loai_xa_phuong",
                        "tinh_thanh_pho",
                        "dia_chi_hien_hanh",
                        "chat_luong_du_lieu",
                        "nhan_dia_chi_hien_thi",
                        "canh_bao_du_lieu",
                        "muc_do_xac_minh",
                        "ngay_xac_minh",
                        "trang_thai_quan_ly",
                        "nguon_tham_khao",
                        "ghi_chu"
                    };
            }
            else
            {
                preferred =
                    new[]
                    {
                        "id",
                        "ma_tuyen",
                        "ten_tuyen",
                        "ma_khu_vuc",
                        "ten_khu_vuc",
                        "ma_xa_phuong",
                        "ten_xa_phuong",
                        "ten_hanh_chinh",
                        "loai",
                        "trang_thai",
                        "mo_ta"
                    };
            }

            foreach (string name
                in preferred)
            {
                if (attributes.ContainsKey(
                    name)
                    &&
                    !result.Contains(
                        name))
                {
                    result.Add(
                        name);
                }
            }

            foreach (string name
                in attributes.Keys)
            {
                if (!result.Contains(
                    name))
                {
                    result.Add(
                        name);
                }
            }

            return result;
        }


        // =========================================================
        // TÊN HIỂN THỊ THÂN THIỆN
        // =========================================================
        private string GetFriendlyInfoFieldName(
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(
                fieldName))
            {
                return "Thuộc tính";
            }

            Dictionary<string, string> names =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    { "id", "Mã số bản ghi" },
                    { "ma_don_vi", "Mã đơn vị" },
                    { "ten_don_vi", "Tên đơn vị" },
                    { "ten_viet_tat", "Tên viết tắt" },
                    { "ky_hieu", "Ký hiệu" },
                    { "ky_hieu_bien_so", "Ký hiệu / biên số" },
                    { "cap_don_vi", "Cấp đơn vị" },
                    { "ten_cap_don_vi", "Cấp đơn vị" },
                    { "ten_cap", "Cấp đơn vị" },
                    { "loai_don_vi", "Loại đơn vị" },
                    { "ten_loai_don_vi", "Loại đơn vị" },
                    { "ten_loai", "Loại đơn vị" },
                    { "don_vi_cap_tren", "Đơn vị cấp trên" },
                    { "ten_don_vi_cap_tren", "Đơn vị cấp trên" },
                    { "ma_xa_phuong", "Mã xã/phường" },
                    { "ten_xa_phuong", "Xã/phường" },
                    { "loai_xa_phuong", "Loại xã/phường" },
                    { "tinh_thanh_pho", "Tỉnh/Thành phố" },
                    { "dia_chi_hien_hanh", "Địa chỉ hiện hành" },
                    { "chat_luong_du_lieu", "Chất lượng dữ liệu" },
                    { "nhan_dia_chi_hien_thi", "Loại địa chỉ" },
                    { "canh_bao_du_lieu", "Cảnh báo dữ liệu" },
                    { "ngay_xac_minh", "Ngày xác minh" },
                    { "ghi_chu", "Ghi chú" },
                    { "trang_thai", "Trạng thái" },
                    { "trang_thai_quan_ly", "Trạng thái quản lý" },
                    { "muc_do_xac_minh", "Mức độ xác minh" },
                    { "nguon_tham_chieu", "Nguồn tham chiếu" },
                    { "nguon_tham_khao", "Nguồn tham khảo" },
                    { "nguon_du_lieu", "Nguồn dữ liệu" },
                    { "mo_ta", "Mô tả" },
                    { "ma_tuyen", "Mã tuyến" },
                    { "ten_tuyen", "Tên tuyến" },
                    { "ma_khu_vuc", "Mã khu vực" },
                    { "ten_khu_vuc", "Tên khu vực" },
                    { "ten_hanh_chinh", "Tên hành chính" },
                    { "loai", "Loại đối tượng" }
                };

            string value;

            if (names.TryGetValue(
                fieldName,
                out value))
            {
                return value;
            }

            string text =
                fieldName.Replace(
                    "_",
                    " ");

            if (text.Length > 0)
            {
                text =
                    char.ToUpper(
                        text[0]) +
                    text.Substring(1);
            }

            return text;
        }


        // =========================================================
        // ẨN TRƯỜNG KỸ THUẬT KHÔNG CẦN HIỂN THỊ
        // =========================================================
        private bool ShouldHideInfoField(
            string fieldName,
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                fieldName))
            {
                return true;
            }

            string name =
                fieldName.Trim()
                .ToLowerInvariant();

            if (name == "geom" ||
                name == "geometry" ||
                name == "wkb_geometry" ||
                name == "shape" ||
                name == "ogc_fid")
            {
                return true;
            }

            // Bỏ trường nội bộ hoàn toàn trống.
            if (string.IsNullOrWhiteSpace(
                    value)
                &&
                (name.EndsWith("_id") ||
                 name.StartsWith("created_") ||
                 name.StartsWith("updated_")))
            {
                return true;
            }

            return false;
        }


        // =========================================================
        // TÊN ĐỐI TƯỢNG CHÍNH
        // =========================================================
        private string ResolveObjectDisplayName(
            Dictionary<string, string> attributes)
        {
            if (attributes == null)
            {
                return "";
            }

            string[] candidates =
            {
                "ten_don_vi",
                "ten_tuyen",
                "ten_khu_vuc",
                "ten_xa_phuong",
                "ten_hanh_chinh",
                "ma_don_vi",
                "ma_tuyen",
                "ma_khu_vuc"
            };

            foreach (string field
                in candidates)
            {
                string value;

                if (attributes.TryGetValue(
                        field,
                        out value)
                    &&
                    !string.IsNullOrWhiteSpace(
                        value))
                {
                    return value;
                }
            }

            return "";
        }


        // =========================================================
        // VỊ TRÍ PANEL
        // =========================================================
        private void PositionObjectInfoPanel()
        {
            if (_objectInfoPanel == null ||
                panelMap == null)
            {
                return;
            }

            int left =
                Math.Max(
                    8,
                    panelMap.ClientSize.Width -
                    _objectInfoPanel.Width -
                    12);

            _objectInfoPanel.Location =
                new Point(
                    left,
                    12);
        }


        private void panelMap_ObjectInfo_Resize(
            object sender,
            EventArgs e)
        {
            PositionObjectInfoPanel();
        }


        // =========================================================
        // ẨN PANEL
        // =========================================================
        private void HideObjectInfoCard()
        {
            if (_objectInfoPanel != null)
            {
                _objectInfoPanel.Visible =
                    false;
            }
        }
    }
}