using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    public enum ModalSize { Small, Medium, Large }

    /// <summary>
    /// Standard modal: header (title + close) / scrollable content stack / footer action bar.
    /// Dual modals also expose LeftRail for Maple Idle-style preview/stat pane.
    /// Content widgets are added to Content (a VStack inside a ScrollRect on the right).
    /// </summary>
    public class ModalView
    {
        public GameObject Go;
        public TMP_Text Title;
        public RectTransform Content;
        public RectTransform Footer;
        /// <summary>Left preview/stat rail (CreateDual only). Null for single-pane modals.</summary>
        public RectTransform LeftRail;
        /// <summary>Called every time the modal opens or global state refreshes while open.</summary>
        public System.Action Refresh;
        UiModalAnimator _anim;

        public bool Active => Go != null && Go.activeSelf;
        public bool IsDual => LeftRail != null;

        internal ModalView(GameObject go, TMP_Text title, RectTransform content, RectTransform footer,
            UiModalAnimator anim, RectTransform leftRail = null)
        {
            Go = go;
            Title = title;
            Content = content;
            Footer = footer;
            LeftRail = leftRail;
            _anim = anim;
        }

        internal void PlayOpen() => _anim?.PlayOpen();

        /// <summary>Remove all dynamic content rows (for rebuild-style modals).</summary>
        public void ClearContent()
        {
            for (int i = Content.childCount - 1; i >= 0; i--)
                Object.Destroy(Content.GetChild(i).gameObject);
        }
    }

    /// <summary>Dim + modal host with open/close animation and dim-click close.</summary>
    public class MapleModalHost
    {
        public GameObject Dim { get; }
        public Transform Root { get; }
        /// <summary>Raised whenever all modals close (dim click, close button, code).</summary>
        public System.Action OnClosed;
        ModalView _open;

        public MapleModalHost(Transform canvasRoot)
        {
            Dim = UiKit.Img(canvasRoot, "Dim", UiKit.DimColor).gameObject;
            var dimRt = Dim.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;
            Dim.GetComponent<Image>().raycastTarget = true;
            var dimBtn = Dim.AddComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(() => Close());
            Dim.SetActive(false);

            var rootGo = new GameObject("Modals", typeof(RectTransform));
            rootGo.transform.SetParent(canvasRoot, false);
            var rt = rootGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Root = rootGo.transform;
        }

        public ModalView Create(string name, string title, ModalSize size = ModalSize.Medium, bool footer = true)
        {
            return CreateInternal(name, title, size, footer, dual: false, leftWidth: 0f);
        }

        /// <summary>
        /// Maple Idle dual pane: left preview/stat rail + right titled detail (scroll + footer).
        /// </summary>
        public ModalView CreateDual(string name, string title, bool footer = true, float leftWidth = 400f)
        {
            return CreateInternal(name, title, ModalSize.Large, footer, dual: true, leftWidth: leftWidth);
        }

        ModalView CreateInternal(string name, string title, ModalSize size, bool footer, bool dual, float leftWidth)
        {
            var canvasRt = Root.parent as RectTransform;
            float cw = canvasRt != null ? canvasRt.rect.width : 1920f;
            float ch = canvasRt != null ? canvasRt.rect.height : 1080f;
            if (cw < 100f) cw = 1920f;
            if (ch < 100f) ch = 1080f;

            Image panel;
            if (dual)
            {
                // Maple Idle docked style: left preview panel + right management panel,
                // game field stays visible between them.
                panel = UiKit.Img(Root, name, new Color(0f, 0f, 0f, 0f));
                panel.raycastTarget = false;
                var frt = panel.rectTransform;
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.offsetMin = Vector2.zero;
                frt.offsetMax = Vector2.zero;
            }
            else
            {
                panel = MapleUiTheme.Panel(Root, name);
                var rt = panel.rectTransform;
                switch (size)
                {
                    // 통짜 창 일러스트 비율(1191×962≈1.24)을 존중해야 프레임이 안 뭉개진다
                    case ModalSize.Small:
                    {
                        float h0 = Mathf.Clamp(ch * 0.74f, 580f, 740f);
                        UiKit.CenterSize(rt, Mathf.Min(cw * 0.58f, h0 * 1.24f), h0);
                        break;
                    }
                    case ModalSize.Large:
                    {
                        float h0 = Mathf.Clamp(ch * 0.95f, 880f, 1020f);
                        UiKit.CenterSize(rt, Mathf.Min(cw * 0.92f, h0 * 1.38f), h0);
                        break;
                    }
                    default:
                    {
                        float h0 = Mathf.Clamp(ch * 0.88f, 760f, 940f);
                        UiKit.CenterSize(rt, Mathf.Min(cw * 0.80f, h0 * 1.30f), h0);
                        break;
                    }
                }
                panel.raycastTarget = true;
            }

            Transform rightParent = panel.transform;
            RectTransform leftRail = null;

            if (dual)
            {
                // Left docked panel (character preview / stats)
                var leftPanel = MapleUiTheme.Panel(panel.transform, name + "Left");
                leftPanel.raycastTarget = true;
                // Clear top HUD (player chip + currency row ≈ 120) and bottom nav (≈ 110).
                var lprt = leftPanel.rectTransform;
                lprt.anchorMin = new Vector2(0f, 0f);
                lprt.anchorMax = new Vector2(0f, 1f);
                lprt.pivot = new Vector2(0f, 0.5f);
                lprt.sizeDelta = new Vector2(leftWidth, -272f);
                lprt.anchoredPosition = new Vector2(12f, -26f);

                // Scroll + mask so long left rails never paint outside the pane.
                var viewport = UiKit.Img(leftPanel.transform, "LeftViewport", new Color(0f, 0f, 0f, 0.01f));
                viewport.raycastTarget = true;
                UiKit.Fill(viewport.rectTransform, MapleUiTheme.WindowFrameInset);
                if (viewport.gameObject.GetComponent<RectMask2D>() == null)
                    viewport.gameObject.AddComponent<RectMask2D>();

                leftRail = UiKit.VStack(viewport.transform, "LeftRail", UiKit.Space2,
                    UiKit.Space3, UiKit.Space3, UiKit.Space2, 24);
                var lrRt = leftRail.GetComponent<RectTransform>();
                lrRt.anchorMin = new Vector2(0f, 1f);
                lrRt.anchorMax = new Vector2(1f, 1f);
                lrRt.pivot = new Vector2(0.5f, 1f);
                lrRt.anchoredPosition = Vector2.zero;
                lrRt.sizeDelta = new Vector2(0f, 0f);
                var leftFitter = leftRail.gameObject.GetComponent<ContentSizeFitter>() ?? leftRail.gameObject.AddComponent<ContentSizeFitter>();
                leftFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                leftFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var leftScroll = leftPanel.gameObject.GetComponent<ScrollRect>() ?? leftPanel.gameObject.AddComponent<ScrollRect>();
                leftScroll.viewport = viewport.rectTransform;
                leftScroll.content = leftRail;
                leftScroll.horizontal = false;
                leftScroll.vertical = true;
                leftScroll.movementType = ScrollRect.MovementType.Clamped;
                leftScroll.scrollSensitivity = 28f;
                leftScroll.inertia = true;

                // Right docked panel (management/detail — header/scroll/footer live here)
                var rightPanel = MapleUiTheme.Panel(panel.transform, name + "Right");
                rightPanel.raycastTarget = true;
                var rprt = rightPanel.rectTransform;
                rprt.anchorMin = new Vector2(1f, 0f);
                rprt.anchorMax = new Vector2(1f, 1f);
                rprt.pivot = new Vector2(1f, 0.5f);
                rprt.sizeDelta = new Vector2(Mathf.Clamp(cw - leftWidth - 560f, 620f, 860f), -128f);
                rprt.anchoredPosition = new Vector2(-12f, 46f);
                rightParent = rightPanel.transform;
            }

            const float headerH = 64f;
            float footerH = footer ? 96f : 0f;
            // Keep header/content inside frame stroke (kit 9-slice or procedural rim).
            float edge = MapleUiTheme.WindowFrameInset;
            // Single-pane Popup_Bg has large round corners — title bar must sit inside the flat edge.
            // Both single-pane and docked-dual panels use round-cornered Popup_Bg now.
            float headerSide = MapleUiTheme.WindowHeaderSideInset;
            float headerTop = Mathf.Max(edge, 18f);
            // 통짜 창 일러스트: 제목은 그림 속 진홍 판(상단 ~8~17% 지점) 위에, 콘텐츠는
            // 나무 프레임 안쪽(좌우 ~7.5%, 하단 ~9%)으로 넣는다.
            bool wuxIll = !dual && CasualArt.C("wux_window_large") != null;
            float panelH = 900f, panelW = 1200f;
            if (!dual)
            {
                var prt0 = panel.rectTransform;
                panelW = prt0.sizeDelta.x > 100f ? prt0.sizeDelta.x : panelW;
                panelH = prt0.sizeDelta.y > 100f ? prt0.sizeDelta.y : panelH;
            }
            if (wuxIll)
            {
                headerTop = panelH * 0.105f;   // 그림 속 진홍 판 위에 제목이 앉도록
                headerSide = panelW * 0.16f;
                edge = panelW * 0.085f;
            }
            // Extra gap so grid tiles are not sliced by footer / frame.
            float scrollBottom = footer ? footerH + 28f : 20f;
            if (wuxIll) scrollBottom += panelH * 0.055f;

            // Header bar — 통짜 창 일러스트엔 타이틀 판이 이미 그려져 있다 → 배경 없이 글씨만 올린다.
            var header = UiKit.Img(rightParent, "Header", UiKit.HeaderDark);
            bool wuxWindow = !dual && CasualArt.C("wux_window_large") != null;
            if (wuxWindow)
            {
                header.sprite = null;
                header.color = new Color(0f, 0f, 0f, 0f);
                header.raycastTarget = false;
            }
            else if (CasualArt.PopupNavy != null)
            {
                header.sprite = CasualArt.PopupNavy;
                header.color = Color.white;
            }
            else
            {
                header.sprite = MapleLightTheme.RoundedSprite(10);
                header.color = new Color(0.10f, 0.11f, 0.14f, 1f);
            }
            header.type = Image.Type.Sliced;
            var hr = header.rectTransform;
            hr.anchorMin = new Vector2(0f, 1f);
            hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.offsetMin = new Vector2(headerSide, -headerTop - headerH);
            hr.offsetMax = new Vector2(-headerSide, -headerTop);

            var titleT = UiKit.TmpLabel(header.transform, "Title", title, UiKit.TmpHeader, new Color(0.95f, 0.90f, 0.78f, 1f),   // 진홍 판자 위 크림 제목
                bold: true, TextAlignmentOptions.Center);
            var tr = titleT.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(56f, 6f);
            tr.offsetMax = new Vector2(-56f, -6f);

            Button close;
            var closeSprite = GrowArt.IconClose;
            if (closeSprite != null)
            {
                var closeImg = UiKit.Sprite(header.transform, "Close", closeSprite);
                close = closeImg.gameObject.AddComponent<Button>();
                close.targetGraphic = closeImg;
                closeImg.raycastTarget = true;
                UiKit.Press(close);
                close.onClick.AddListener(() => Close());
            }
            else
            {
                close = MapleUiTheme.SecondaryButton(header.transform, "Close", "✕", () => Close(), UiKit.FontH2);
            }
            var cr = close.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(1f, 0.5f);
            cr.anchorMax = new Vector2(1f, 0.5f);
            cr.pivot = new Vector2(1f, 0.5f);
            cr.sizeDelta = new Vector2(46f, 46f);
            cr.anchoredPosition = new Vector2(-12f, 0f);

            // Scrollable content
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(rightParent, false);
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(edge + 4f, scrollBottom);
            srt.offsetMax = new Vector2(-(edge + 4f), -(headerTop + headerH + 12f));

            var viewportImg = UiKit.Img(scrollGo.transform, "Viewport", new Color(1f, 1f, 1f, 0.01f));
            var vrt = viewportImg.rectTransform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            // 스크롤할 때 위아래가 칼같이 잘리면 보기 나쁘다.
            // RectMask2D.softness로 가장자리를 부드럽게 페이드시킨다.
            var vpMask = viewportImg.gameObject.AddComponent<RectMask2D>();
            vpMask.softness = new Vector2Int(0, 28);
            viewportImg.raycastTarget = true;

            var content = UiKit.VStack(viewportImg.transform, "Content", UiKit.Space2,
                UiKit.Space3, UiKit.Space3, UiKit.Space3, 32);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = vrt;
            scroll.content = crt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            // Footer
            RectTransform footerRt = null;
            if (footer)
            {
                footerRt = UiKit.HStack(rightParent, "Footer", UiKit.Space2,
                    UiKit.Space4, UiKit.Space4, UiKit.Space2, UiKit.Space2, TextAnchor.MiddleRight);
                var frt = footerRt.GetComponent<RectTransform>();
                frt.anchorMin = new Vector2(0f, 0f);
                frt.anchorMax = new Vector2(1f, 0f);
                frt.pivot = new Vector2(0.5f, 0f);
                frt.offsetMin = new Vector2(edge, 8f);
                frt.offsetMax = new Vector2(-edge, footerH);
                var line = UiKit.Img(rightParent, "FooterLine", new Color(0f, 0f, 0f, 0.18f));
                var lrt = line.rectTransform;
                lrt.anchorMin = new Vector2(0f, 0f);
                lrt.anchorMax = new Vector2(1f, 0f);
                lrt.offsetMin = new Vector2(edge + 8f, footerH);
                lrt.offsetMax = new Vector2(-(edge + 8f), footerH + 2f);
                line.raycastTarget = false;
            }

            var anim = panel.gameObject.AddComponent<UiModalAnimator>();
            panel.gameObject.SetActive(false);
            return new ModalView(panel.gameObject, titleT, content, footerRt, anim, leftRail);
        }

        public void Open(ModalView modal)
        {
            CloseAllExcept(null);
            Dim.SetActive(true);
            if (modal == null) return;
            IdleMvp.Core.AudioService.Open();
            modal.Go.SetActive(true);
            modal.Go.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            UiKit.RelayoutAllFillGrids(modal.Go.transform);
            modal.Refresh?.Invoke();
            Canvas.ForceUpdateCanvases();
            UiKit.RelayoutAllFillGrids(modal.Go.transform);
            modal.PlayOpen();
            _open = modal;
        }

        public ModalView OpenModal => _open != null && _open.Active ? _open : null;

        public void RefreshOpen()
        {
            var m = OpenModal;
            if (m == null) return;
            Canvas.ForceUpdateCanvases();
            UiKit.RelayoutAllFillGrids(m.Go.transform);
            m.Refresh?.Invoke();
            Canvas.ForceUpdateCanvases();
            UiKit.RelayoutAllFillGrids(m.Go.transform);
        }

        public void Close(bool hideDim = true)
        {
            bool wasOpen = _open != null && _open.Active;
            CloseAllExcept(null);
            _open = null;
            if (hideDim)
            {
                Dim.SetActive(false);
                OnClosed?.Invoke();
            }
            if (wasOpen) IdleMvp.Core.AudioService.Close();
        }

        void CloseAllExcept(GameObject keep)
        {
            foreach (Transform c in Root)
                if (c.gameObject != keep)
                    c.gameObject.SetActive(false);
        }
    }
}
