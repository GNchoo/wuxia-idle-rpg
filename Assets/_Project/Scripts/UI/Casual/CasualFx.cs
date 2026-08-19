using System.Collections;
using IdleMvp.UI.Maple;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI.Casual
{
    /// <summary>
    /// 뽑기·강화 연출. 구매 에셋의 글로우/카드 프레임 스프라이트만 써서 만든다
    /// (직접 그린 텍스처 금지 — CasualArt 참조).
    /// </summary>
    public static class CasualFx
    {
        static MonoBehaviour _runner;

        public static void SetRunner(MonoBehaviour runner) => _runner = runner;

        /// <summary>소환 연출: 중앙에서 빛이 확 퍼진다. 10연차는 더 크고 길게.</summary>
        public static void SummonBurst(Transform host, bool big)
        {
            if (host == null || _runner == null) return;
            _runner.StartCoroutine(BurstCo(host, big));
        }

        static IEnumerator BurstCo(Transform host, bool big)
        {
            var glow = MakeGlow(host, big
                ? new Color(1f, 0.86f, 0.35f, 0f)      // 10연차: 금색
                : new Color(0.55f, 0.85f, 1f, 0f));    // 1회: 하늘색
            if (glow == null) yield break;

            float dur = big ? 0.85f : 0.5f;
            float from = big ? 220f : 150f;
            float to = big ? 1150f : 700f;
            float t = 0f;
            var rt = glow.rectTransform;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                float ease = 1f - (1f - u) * (1f - u);            // out-quad
                float d = Mathf.Lerp(from, to, ease);
                rt.sizeDelta = new Vector2(d, d);
                // 앞부분에 확 밝아지고 뒤로 사그라든다
                float a = u < 0.22f ? u / 0.22f : 1f - (u - 0.22f) / 0.78f;
                var c = glow.color; c.a = Mathf.Clamp01(a) * 0.9f; glow.color = c;
                rt.localRotation = Quaternion.Euler(0f, 0f, ease * (big ? 90f : 40f));
                yield return null;
            }
            Object.Destroy(glow.gameObject);
        }

        /// <summary>강화 연출: 짧고 강한 흰 섬광 + 링 확산.</summary>
        public static void EnhanceFlash(Transform host)
        {
            if (host == null || _runner == null) return;
            _runner.StartCoroutine(FlashCo(host));
        }

        static IEnumerator FlashCo(Transform host)
        {
            var ring = MakeGlow(host, new Color(0.75f, 1f, 0.9f, 0f));
            if (ring == null) yield break;
            const float dur = 0.42f;
            float t = 0f;
            var rt = ring.rectTransform;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                float d = Mathf.Lerp(120f, 620f, 1f - (1f - u) * (1f - u));
                rt.sizeDelta = new Vector2(d, d);
                var c = ring.color; c.a = (1f - u) * 0.85f; ring.color = c;
                yield return null;
            }
            Object.Destroy(ring.gameObject);
        }

        /// <summary>강화 실패 — 화면을 붉게 한 번 치고 좌우로 흔든다.</summary>
        public static void FailShake(Transform host)
        {
            if (host == null || _runner == null) return;
            _runner.StartCoroutine(FailCo(host));
        }

        static IEnumerator FailCo(Transform host)
        {
            var ring = MakeGlow(host, new Color(1f, 0.35f, 0.3f, 0f));
            var rt = host as RectTransform;
            Vector2 basePos = rt != null ? rt.anchoredPosition : Vector2.zero;
            const float dur = 0.35f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                if (ring != null)
                {
                    ring.rectTransform.sizeDelta = new Vector2(300f, 300f);
                    var c = ring.color; c.a = (1f - u) * 0.5f; ring.color = c;
                }
                if (rt != null)
                    rt.anchoredPosition = basePos + new Vector2(
                        Mathf.Sin(u * 40f) * 9f * (1f - u), 0f);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = basePos;
            if (ring != null) Object.Destroy(ring.gameObject);
        }

        /// <summary>키트 글로우 스프라이트로 중앙 정렬된 임시 이미지를 만든다.</summary>
        static Image MakeGlow(Transform host, Color tint)
        {
            var sprite = CasualArt.C("Common_Popup_Glow")
                ?? CasualArt.C("CardFrame03_Glow")
                ?? CasualArt.C("CardFrame01_Glow")
                ?? CasualArt.C("Background_ScreenGlow");
            if (sprite == null) return null;

            var go = new GameObject("Fx_Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(host, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.raycastTarget = false;
            img.color = tint;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            go.transform.SetAsLastSibling();
            return img;
        }
    }
}
