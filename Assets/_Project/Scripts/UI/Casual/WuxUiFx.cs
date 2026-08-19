using System.Collections;
using IdleMvp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Casual
{
    /// <summary>
    /// 무협 창 연출. 한지 창이 펼쳐지듯 열리고, 보상 자리에 기운이 튄다.
    ///
    /// 이펙트 아트는 새로 굽지 않고 전투용 시퀀스(GrowArt/Fx/*)를 색만 바꿔 쓴다 —
    /// 같은 붓결이라 전투와 UI가 따로 놀지 않는다.
    /// </summary>
    public class WuxUiFx : MonoBehaviour
    {
        static WuxUiFx _inst;

        static WuxUiFx Inst
        {
            get
            {
                if (_inst == null)
                {
                    var go = new GameObject("WuxUiFx");
                    DontDestroyOnLoad(go);
                    _inst = go.AddComponent<WuxUiFx>();
                }
                return _inst;
            }
        }

        /// <summary>창이 펼쳐지는 연출. 같은 창을 다시 열면 처음부터 다시 편다.</summary>
        public static void PlayOpen(RectTransform sheet)
        {
            if (sheet == null || !sheet.gameObject.activeInHierarchy) return;
            var cg = sheet.GetComponent<CanvasGroup>();
            if (cg == null) cg = sheet.gameObject.AddComponent<CanvasGroup>();
            Inst.StopCoroutine("OpenCo");
            Inst.StartCoroutine(Inst.OpenCo(sheet, cg));
        }

        IEnumerator OpenCo(RectTransform sheet, CanvasGroup cg)
        {
            const float dur = 0.20f;
            float t = 0f;
            while (t < dur && sheet != null)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                float e = 1f - (1f - u) * (1f - u);       // 빠르게 붙었다 느리게 멎는다
                // 세로로 먼저 펴지는 두루마리 느낌 — 가로는 거의 그대로 둔다
                sheet.localScale = new Vector3(Mathf.Lerp(0.97f, 1f, e), Mathf.Lerp(0.86f, 1f, e), 1f);
                cg.alpha = e;
                yield return null;
            }
            if (sheet != null) sheet.localScale = Vector3.one;
            if (cg != null) cg.alpha = 1f;
            InkifyPaperText(sheet);
        }

        static readonly Color Ink = new Color(0.26f, 0.16f, 0.08f);

        /// <summary>
        /// 한지 위에 얹힌 밝은 글씨를 먹빛으로 바꾼다.
        ///
        /// 창들이 원래 어두운 나무판 배경이라 글씨가 크림색이었다. 바탕이 종이로 바뀌면서
        /// 그대로 두면 안 읽힌다. 버튼 안 글씨는 제 판때기 위에 있으니 건드리지 않는다.
        /// </summary>
        public static void InkifyPaperText(RectTransform sheet)
        {
            if (sheet == null) return;
            foreach (var t in sheet.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                // 버튼 라벨이라도 '판때기'가 실제로 그려져 있을 때만 밝은 글씨를 남긴다.
                // 카드처럼 판 없이 종이 위에 바로 얹힌 버튼은 밝으면 안 읽힌다.
                var sel = t.GetComponentInParent<Selectable>();
                if (sel != null)
                {
                    var plate = sel.GetComponent<Image>();
                    bool hasPlate = plate != null && plate.enabled && plate.sprite != null
                        && plate.color.a > 0.2f;
                    if (hasPlate) continue;
                }
                var c = t.color;
                float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
                if (lum > 0.70f && c.a > 0.1f)
                    t.color = new Color(Ink.r, Ink.g, Ink.b, c.a);
            }
        }

        /// <summary>지정한 자리에 기운이 한 번 튄다 (보상 획득·강화 성공 등).</summary>
        public static void Sparkle(RectTransform at, Color tint, float sizeMul = 1f)
        {
            if (at == null) return;
            var frames = GrowArt.FxSequence("HitBurst");
            if (frames == null || frames.Length == 0) return;
            Inst.StartCoroutine(Inst.SparkleCo(at, tint, sizeMul, frames));
        }

        IEnumerator SparkleCo(RectTransform at, Color tint, float sizeMul, Sprite[] frames)
        {
            var go = new GameObject("Sparkle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(at, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = at.rect.size * 1.1f * sizeMul;
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = tint;

            const float fps = 24f;
            for (int i = 0; i < frames.Length; i++)
            {
                if (img == null) yield break;
                img.sprite = frames[i];
                // 뒤로 갈수록 옅어져 잔광처럼 사라진다
                float k = 1f - i / (float)Mathf.Max(1, frames.Length - 1);
                img.color = new Color(tint.r, tint.g, tint.b, tint.a * Mathf.Clamp01(0.35f + k * 0.65f));
                yield return new WaitForSecondsRealtime(1f / fps);
            }
            if (go != null) Destroy(go);
        }
    }
}
