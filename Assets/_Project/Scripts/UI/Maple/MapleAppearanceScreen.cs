using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using IdleMvp.Core;
using IdleMvp.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Maple
{
    /// <summary>
    /// In-game character appearance editor (Hippo rig): live RT preview,
    /// grouped slot browsing with prev/next, color swatches, save/reset.
    /// 항시 커스터마이징은 신체·헤어·얼굴뿐 (벤치마크 동일) — 의상·무기·모자는
    /// 장착한 장비가 결정한다 (HippoLookService.ApplyEquipmentLook).
    /// </summary>
    public class MapleAppearanceScreen
    {
        readonly MapleModalHost _modals;
        readonly System.Action<string> _toast;
        readonly System.Action _refreshAll;

        public ModalView Modal { get; private set; }

        // preview rig
        GameObject _rigGo;
        Character _char;
        Camera _rtCam;
        RenderTexture _rt;
        RawImage _previewImage;

        // 편집 중인 외형 (저장 전까지 실계정에 반영 안 함)
        HippoLookService.HippoAppearance _look;

        // slot browsing
        string _selectedSlot;
        RectTransform _colorRow;
        readonly Dictionary<string, TMP_Text> _slotValueLabels = new Dictionary<string, TMP_Text>();
        readonly Dictionary<string, Button[]> _slotArrows = new Dictionary<string, Button[]>();
        readonly Dictionary<string, Image> _slotRowBg = new Dictionary<string, Image>();

        static readonly Color RowNormal = Color.white;                          // 종이 띠 원색
        static readonly Color RowPicked = new Color(1f, 0.86f, 0.62f, 1f);     // 선택 = 금빛 종이

        static readonly (string title, string[] slots)[] Groups =
        {
            ("신체", new[] { "Skin", "Head", "Eyes", "Mouth" }),
            ("헤어", new[] { "Hair", "Eyebrows", "Beard" }),
        };

        static readonly Dictionary<string, string> SlotKo = new Dictionary<string, string>
        {
            { "Skin", "피부" }, { "Head", "얼굴형" }, { "Eyes", "눈" }, { "Mouth", "입" },
            { "Hair", "머리" }, { "Eyebrows", "눈썹" }, { "Beard", "수염" },
        };

        // Curated palette (hair / skin / eyes friendly).
        static readonly Color[] Palette =
        {
            new Color(0.13f, 0.12f, 0.12f), new Color(0.35f, 0.22f, 0.12f), new Color(0.62f, 0.42f, 0.22f),
            new Color(0.85f, 0.72f, 0.45f), new Color(0.92f, 0.88f, 0.82f), new Color(0.75f, 0.2f, 0.2f),
            new Color(0.9f, 0.45f, 0.2f), new Color(0.95f, 0.8f, 0.3f), new Color(0.3f, 0.6f, 0.3f),
            new Color(0.2f, 0.45f, 0.75f), new Color(0.45f, 0.3f, 0.65f), new Color(0.9f, 0.55f, 0.7f),
            new Color(0.55f, 0.58f, 0.62f), new Color(0.9f, 0.9f, 0.92f),
            new Color(1.0f, 0.87f, 0.73f), new Color(0.87f, 0.68f, 0.52f), new Color(0.62f, 0.44f, 0.32f),
        };

        public MapleAppearanceScreen(MapleModalHost modals, System.Action<string> toast, System.Action refreshAll)
        {
            _modals = modals;
            _toast = toast;
            _refreshAll = refreshAll;
        }

        // ---- 슬롯 → 컬렉션 목록 -----------------------------------------------

        List<string> ItemsFor(string slot)
        {
            var col = _char != null ? _char.SpriteCollection : null;
            if (col == null) return null;
            switch (slot)
            {
                case "Head": return col.Head.Where(i => i.Id.StartsWith("Common.")).Select(i => i.Id).ToList();
                case "Eyes": return col.Eyes.Select(i => i.Id).ToList();
                case "Mouth": return col.Mouth.Select(i => i.Id).ToList();
                case "Hair": return col.Hair.Select(i => i.Id).ToList();
                case "Eyebrows": return col.Eyebrows.Select(i => i.Id).ToList();
                case "Beard": return col.Beard.Select(i => i.Id).ToList();
                default: return null;   // Skin = 색상 전용
            }
        }

        string GetLook(string slot)
        {
            switch (slot)
            {
                case "Head": return _look.head;
                case "Eyes": return _look.eyes;
                case "Mouth": return _look.mouth;
                case "Hair": return _look.hair;
                case "Eyebrows": return _look.eyebrows;
                case "Beard": return _look.beard;
                default: return null;
            }
        }

        void SetLook(string slot, string id)
        {
            switch (slot)
            {
                case "Head": _look.head = id; break;
                case "Eyes": _look.eyes = id; break;
                case "Mouth": _look.mouth = id; break;
                case "Hair": _look.hair = id; break;
                case "Eyebrows": _look.eyebrows = id; break;
                case "Beard": _look.beard = id; break;
            }
        }

        // ---- UI ----------------------------------------------------------------

        public void Build()
        {
            Modal = _modals.Create("Appearance", "외형 꾸미기", ModalSize.Large);
            var c = Modal.Content;

            var row = UiKit.HStack(c, "Main", UiKit.Space3, 0, 0, 0, 0, TextAnchor.UpperLeft);
            // 바깥 모달 스크롤의 Viewport 높이가 600 — 640이면 하단 팔레트가 잘린다
            UiKit.Fix(row, -1f, 590f);

            // ---- left: live preview ----
            var previewCol = UiKit.VStack(row, "PreviewCol", UiKit.Space2, 0, 0, 0, 0, TextAnchor.UpperCenter);
            UiKit.Fix(previewCol, 280f, -1f);   // 통짜 창(비율 고정)에 맞춘 폭
            var frame = UiKit.Img(previewCol, "Frame", FantasyKitSlots.KitPanel);
            FantasyKitSlots.FrameRarity(frame, 2, 260f);
            UiKit.Fix(frame, -1f, 400f);
            var raw = new GameObject("Preview", typeof(RectTransform), typeof(RawImage));
            raw.transform.SetParent(frame.transform, false);
            _previewImage = raw.GetComponent<RawImage>();
            UiKit.Fill(_previewImage.rectTransform, 14f);
            _previewImage.color = Color.white;

            var btnRow = UiKit.HStack(previewCol, "Btns", 8f, 0, 0, 0, 0, TextAnchor.MiddleCenter, true);
            UiKit.Fix(btnRow, -1f, 52f);
            MapleUiTheme.SecondaryButton(btnRow, "Random", "랜덤", Randomize, UiKit.FontCaption);
            MapleUiTheme.SecondaryButton(btnRow, "Default", "기본", ResetToDefault, UiKit.FontCaption);

            // ---- right: slot browser ----
            var editCol = UiKit.VStack(row, "EditCol", UiKit.Space2, 0, 0, 0, 0, TextAnchor.UpperLeft);
            UiKit.Flex(editCol);

            // 색상 팔레트는 '맨 위' 고정 (하단에 두면 모달 높이에 따라 잘렸다)
            var colorHeader = MapleUiTheme.SectionHeader(editCol, "색상 (선택한 부위)");
            var headerLe = UiKit.Fix(colorHeader, -1f, 40f);
            headerLe.flexibleHeight = 0f;
            _colorRow = UiKit.HStack(editCol, "Colors", 6f, 0, 0, 0, 0, TextAnchor.MiddleLeft);
            var colorLe = UiKit.Fix(_colorRow, -1f, 52f);
            colorLe.flexibleHeight = 0f;
            BuildColorSwatches();

            // 슬롯 목록은 자체 스크롤
            var scrollGo = new GameObject("SlotScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(editCol, false);
            var scrollRt = (RectTransform)scrollGo.transform;
            UiKit.Flex(scrollRt);
            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 24f;

            var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            vpGo.transform.SetParent(scrollRt, false);
            var vpRt = (RectTransform)vpGo.transform;
            UiKit.Fill(vpRt);
            vpGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            sr.viewport = vpRt;

            var slotContent = UiKit.VStack(vpRt, "SlotContent", UiKit.Space2, 0, 0, 0, 0, TextAnchor.UpperLeft);
            slotContent.anchorMin = new Vector2(0f, 1f);
            slotContent.anchorMax = new Vector2(1f, 1f);
            slotContent.pivot = new Vector2(0.5f, 1f);
            slotContent.anchoredPosition = Vector2.zero;
            slotContent.sizeDelta = new Vector2(0f, slotContent.sizeDelta.y);
            var fitter = slotContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = slotContent;

            foreach (var (title, slots) in Groups)
            {
                MapleUiTheme.SectionHeader(slotContent, title);
                foreach (var slot in slots)
                    BuildSlotRow(slotContent, slot);
            }

            var save = MapleUiTheme.AccentButton(Modal.Footer, "Save", "저장", SaveAndClose);
            UiKit.Fix(save, 200f, 64f);

            Modal.Refresh = EnsureRig;
        }

        void BuildSlotRow(Transform parent, string slot)
        {
            var rowBg = UiKit.Img(parent, "Row_" + slot, RowNormal);
            rowBg.sprite = CasualArt.RowDark != null ? CasualArt.RowDark
                : CasualArt.CardRound != null ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(10);
            rowBg.type = UnityEngine.UI.Image.Type.Sliced;
            UiKit.Fix(rowBg, -1f, 52f);
            MapleUiTheme.StretchFullWidth(rowBg);
            var h = UiKit.HStack(rowBg.transform, "H", 8f, 12, 12, 6, 6, TextAnchor.MiddleLeft);
            UiKit.Fill(h);

            string ko = SlotKo.TryGetValue(slot, out var k) ? k : slot;
            var name = UiKit.TmpLabel(h, "Name", ko, UiKit.TmpCaption, UiKit.TextInverseDim, bold: true);
            name.enableWordWrapping = false;
            UiKit.Fix(name, 120f, 30f);

            var prev = MapleUiTheme.SecondaryButton(h, "Prev", "◀", () => Cycle(slot, -1), UiKit.FontCaption);
            UiKit.Fix(prev, 48f, 40f);

            var val = UiKit.TmpLabel(h, "Val", "-", UiKit.TmpCaption, FantasyKitSlots.KitTeal,
                bold: true, TextAlignmentOptions.Center);
            val.enableWordWrapping = false;
            UiKit.Flex(val);
            _slotValueLabels[slot] = val;

            var next = MapleUiTheme.SecondaryButton(h, "Next", "▶", () => Cycle(slot, +1), UiKit.FontCaption);
            UiKit.Fix(next, 48f, 40f);

            _slotArrows[slot] = new[] { prev, next };
            _slotRowBg[slot] = rowBg;

            var btn = rowBg.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                _selectedSlot = slot;
                _toast?.Invoke(ko + " 색상 편집 — 아래 색을 고르세요");
                RefreshSlotLabels();
            });
        }

        void BuildColorSwatches()
        {
            foreach (var col in Palette)
            {
                var frame = UiKit.Img(_colorRow, "Sw", new Color(0.88f, 0.92f, 1f, 1f));
                frame.sprite = CasualArt.CardRound != null
                    ? CasualArt.CardRound : MapleLightTheme.RoundedSprite(999);
                frame.type = UnityEngine.UI.Image.Type.Sliced;
                UiKit.Fix(frame, 36f, 36f);

                var sw = UiKit.Img(frame.transform, "Fill", col);
                sw.sprite = frame.sprite;
                sw.type = UnityEngine.UI.Image.Type.Sliced;
                sw.raycastTarget = false;
                UiKit.Fill(sw.rectTransform, 4f);

                var b = frame.gameObject.AddComponent<Button>();
                b.targetGraphic = frame;
                var captured = col;
                b.onClick.AddListener(() => ApplyColor(captured));
            }
        }

        // ---- preview rig ------------------------------------------------------

        void EnsureRig()
        {
            if (_char == null)
            {
                string preset = AppearanceService.PresetForJob(JobProgress.JobId);
                var prefab = Resources.Load<GameObject>("CharPresets/" + preset);
                if (prefab == null) { _toast?.Invoke("프리셋 없음: " + preset); return; }

                _rigGo = Object.Instantiate(prefab);
                _rigGo.name = "AppearancePreviewRig";
                _rigGo.transform.position = new Vector3(500f, 500f, 0f); // far off-field
                _char = _rigGo.GetComponentInChildren<Character>();
                if (_char == null) { _toast?.Invoke("Hippo 리그 아님: " + preset); return; }

                // 저장본에서 시작 (없으면 기본값)
                var cur = HippoLookService.Current;
                _look = cur != null
                    ? JsonUtility.FromJson<HippoLookService.HippoAppearance>(JsonUtility.ToJson(cur))
                    : new HippoLookService.HippoAppearance();
                ApplyPreview();

                _rt = new RenderTexture(512, 768, 16);
                var camGo = new GameObject("AppearanceCam");
                camGo.transform.position = new Vector3(500f, 500.75f, -10f);
                _rtCam = camGo.AddComponent<Camera>();
                _rtCam.orthographic = true;
                _rtCam.orthographicSize = 1.1f;
                _rtCam.clearFlags = CameraClearFlags.SolidColor;
                _rtCam.backgroundColor = new Color(0.13f, 0.15f, 0.2f, 1f);
                _rtCam.targetTexture = _rt;
                _previewImage.texture = _rt;
            }
            RefreshSlotLabels();
        }

        void ApplyPreview()
        {
            if (_char == null) return;
            try
            {
                HippoLookService.ApplyCustomization(_char, _look);
                HippoLookService.ApplyEquipmentLook(_char);   // 장비 외형은 항상 유지
                _char.Initialize();
            }
            catch (System.Exception e) { Debug.LogWarning("[Appearance] preview apply: " + e.Message); }
        }

        public void ReleaseRig()
        {
            if (_rigGo != null) Object.Destroy(_rigGo);
            if (_rtCam != null) Object.Destroy(_rtCam.gameObject);
            if (_rt != null) { _rt.Release(); Object.Destroy(_rt); }
            _rigGo = null; _char = null; _rtCam = null; _rt = null;
        }

        // ---- edits ------------------------------------------------------------

        void Cycle(string slot, int dir)
        {
            if (_char == null) return;
            var items = ItemsFor(slot);
            if (items == null || items.Count == 0)
            {
                _selectedSlot = slot;
                string ko1 = SlotKo.TryGetValue(slot, out var kk) ? kk : slot;
                _toast?.Invoke(ko1 + "은(는) 아래 색상으로 바꾸세요");
                RefreshSlotLabels();
                return;
            }
            string cur = GetLook(slot);
            int idx = items.IndexOf(cur);   // 미설정(-1)에서 ▶ = 0번
            idx = (idx + dir + items.Count + (idx < 0 && dir < 0 ? 1 : 0)) % items.Count;
            SetLook(slot, items[idx]);
            _selectedSlot = slot;
            ApplyPreview();
            RefreshSlotLabels();
            IdleMvp.Core.AudioService.Click();
        }

        void ApplyColor(Color c)
        {
            if (_char == null) return;
            if (string.IsNullOrEmpty(_selectedSlot)) _selectedSlot = "Skin";

            switch (_selectedSlot)
            {
                case "Hair":
                case "Beard":
                case "Eyebrows": _look.hairColor = c; break;
                case "Eyes": _look.eyesColor = c; break;
                case "Skin":
                case "Head": _look.skinColor = c; break;
                default: _toast?.Invoke("이 부위는 색상 변경 불가"); return;
            }
            ApplyPreview();
            RefreshSlotLabels();
            IdleMvp.Core.AudioService.Click();
        }

        void Randomize()
        {
            if (_char == null) return;
            foreach (var (_, slots) in Groups)
                foreach (var slot in slots)
                {
                    var items = ItemsFor(slot);
                    if (items == null || items.Count == 0) continue;
                    // 수염은 절반 확률로 없음 — 전원 수염이면 노안 파티가 된다
                    if (slot == "Beard" && Random.value < 0.5f) { SetLook(slot, ""); continue; }
                    SetLook(slot, items[Random.Range(0, items.Count)]);
                }
            _look.hairColor = Palette[Random.Range(0, Palette.Length)];
            _look.skinColor = Palette[Random.Range(14, Palette.Length)];   // 피부톤 구간
            ApplyPreview();
            RefreshSlotLabels();
        }

        void ResetToDefault()
        {
            if (_char == null) return;
            _look = new HippoLookService.HippoAppearance();
            // 프리셋 기본 파츠로 되돌린다 — 리그 재생성이 가장 확실
            ReleaseRig();
            EnsureRig();
        }

        void SaveAndClose()
        {
            if (_char == null) return;
            HippoLookService.Save(_look);
            Combat.FieldAutoHuntController.Instance?.RefreshHeroAppearance();
            _toast?.Invoke("외형 저장 완료");
            _modals.Close();
            _refreshAll?.Invoke();
        }

        void RefreshSlotLabels()
        {
            if (_char == null) return;
            foreach (var pair in _slotValueLabels)
            {
                string slot = pair.Key;
                var items = ItemsFor(slot);
                int count = items != null ? items.Count : 0;
                string cur = GetLook(slot);

                if (count == 0)
                    pair.Value.text = "색상만 변경 가능";
                else if (string.IsNullOrEmpty(cur))
                    pair.Value.text = "기본  (0/" + count + ")";
                else
                    pair.Value.text = (items.IndexOf(cur) + 1) + " / " + count;

                if (_slotArrows.TryGetValue(slot, out var arrows))
                    for (int i = 0; i < arrows.Length; i++)
                        if (arrows[i] != null) arrows[i].interactable = count > 1;

                if (_slotRowBg.TryGetValue(slot, out var bg) && bg != null)
                    bg.color = slot == _selectedSlot ? RowPicked : RowNormal;
            }
        }
    }
}
