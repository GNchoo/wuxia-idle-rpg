using System.Collections;
using System.Collections.Generic;
using IdleMvp.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.Combat
{
    /// <summary>
    /// Field hit/skill FX from IdleRPG_Assets Spells.
    /// Spell frames are black-backed pixel art meant for additive blending
    /// (same look as the template Lightning/Meteor/Tornado/Skull anims).
    /// </summary>
    public class FieldCombatFx : MonoBehaviour
    {
        public static FieldCombatFx Instance { get; private set; }

        static readonly string[] SkillFolders = { "Lightning", "Meteor", "Ice", "Scream" };
        static readonly Color[] SkillTints =
        {
            Color.white,
            new Color(1f, 0.95f, 0.8f, 1f),
            new Color(0.7f, 0.95f, 1f, 1f),
            new Color(1f, 0.75f, 1f, 1f)
        };
        static readonly float[] SkillScale = { 5.5f, 5f, 5f, 1.15f };
        static readonly float[] SkillFps = { 16f, 18f, 14f, 20f };

        /// <summary>
        /// 세력별 기운 색. 정파=검기(푸른 검광) · 사파=암기(독빛) · 마도=혈마공(핏빛).
        /// 스킬 아트는 공용이지만 색과 잔광이 달라 각기 다른 무공처럼 읽힌다.
        /// </summary>
        public static Color FactionTint()
        {
            switch (IdleMvp.Core.JobProgress.TreeId)
            {
                case "bowmaster": return new Color(0.62f, 1.00f, 0.55f);   // 사파 · 독빛
                case "archmage":  return new Color(1.00f, 0.38f, 0.40f);   // 마도 · 핏빛
                default:          return new Color(0.68f, 0.90f, 1.00f);   // 정파 · 검광
            }
        }

        static Color Blend(Color baseTint, float weight = 0.75f)
        {
            var f = FactionTint();
            return Color.Lerp(baseTint, new Color(baseTint.r * f.r, baseTint.g * f.g,
                baseTint.b * f.b, baseTint.a), weight);
        }

        RectTransform _root;
        AudioSource _audio;
        static Material _additiveMat;
        static Sprite[] _skullFrames;

        // GameObject당 Graphic은 하나만 붙는다. 데미지 숫자(TMP)와 이펙트(Image)가
        // 같은 풀을 쓰면 재사용 시 AddComponent가 실패하므로 종류별로 나눈다.
        readonly Dictionary<string, Queue<GameObject>> _fxPools = new Dictionary<string, Queue<GameObject>>(4);

        Queue<GameObject> PoolFor(string name)
        {
            Queue<GameObject> q;
            if (!_fxPools.TryGetValue(name, out q)) { q = new Queue<GameObject>(32); _fxPools[name] = q; }
            return q;
        }

        GameObject RentGo(string name)
        {
            var pool = PoolFor(name);
            while (pool.Count > 0)
            {
                var g = pool.Dequeue();
                if (g != null) { g.SetActive(true); return g; }
            }
            return new GameObject(name, typeof(RectTransform));
        }

        void ReturnGo(GameObject g)
        {
            if (g == null) return;
            g.SetActive(false);
            var pool = PoolFor(g.name);
            if (pool.Count < 48) pool.Enqueue(g);
            else Destroy(g);
        }

        void Awake()
        {
            Instance = this;
            _audio = gameObject.GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
        }

        public void Bind(RectTransform fieldRoot)
        {
            if (fieldRoot == null) { _root = null; return; }
            // 이펙트는 전용 레이어에 담는다. 맵 스크롤을 이 레이어 하나만 따라가게 해서
            // 모든 이펙트가 액터와 같이 움직인다 — 호출부 8곳을 고치지 않아도 된다.
            var layer = fieldRoot.Find("FxLayer") as RectTransform;
            if (layer == null)
            {
                layer = (RectTransform)new GameObject("FxLayer", typeof(RectTransform)).transform;
                layer.SetParent(fieldRoot, false);
                layer.anchorMin = Vector2.zero;
                layer.anchorMax = Vector2.one;
                layer.offsetMin = Vector2.zero;
                layer.offsetMax = Vector2.zero;
            }
            // 피벗을 필드와 맞춰야 좌표 원점이 같아진다 — 안 맞추면 이펙트가 통째로 어긋난다
            layer.pivot = fieldRoot.pivot;
            layer.SetAsLastSibling();
            _root = layer;
        }

        void LateUpdate()
        {
            if (_root == null) return;
            var stage = FieldWorldStage.Instance;
            float sx = stage != null ? stage.ScrollX : 0f;
            // 필드 좌표는 y를 바닥부터 재는데(FieldToWorld) UI anchoredPosition은 중심 기준이다.
            // 레이어를 바닥까지 내려 두 좌표계를 맞춘다 — 안 그러면 이펙트가 액터보다 한참 위에 뜬다.
            var parent = _root.parent as RectTransform;
            float baseY = parent != null ? parent.rect.yMin : 0f;
            var want = new Vector2(-sx, baseY);
            if ((_root.anchoredPosition - want).sqrMagnitude > 0.01f) _root.anchoredPosition = want;
        }

        // 타격음은 여기서 낸다 — 호출부가 5곳(자동공격/보스강타/광역/반격/스킬)이라
        // 공용 지점 하나에 두는 게 맞다. 다단히트가 같은 프레임에 몰리면 귀가 아프므로
        // 짧은 간격 안의 중복은 삼킨다.
        float _lastHitSfx = -1f;

        public void PlayHit(Vector2 anchoredPos, bool skillPulse = false)
        {
            if (Time.unscaledTime - _lastHitSfx > 0.07f)
            {
                _lastHitSfx = Time.unscaledTime;
                IdleMvp.Core.AudioService.Hit();
            }
            if (_root == null) return;
            var burst = GrowArt.FxSequence("HitBurst");
            if (burst != null && burst.Length > 0)
            {
                float size = NativeSize(burst[0], skillPulse ? 4.2f : 3.2f, 64f, 140f);
                SpawnAnim(burst, anchoredPos, size, skillPulse ? 18f : 16f, Color.white, 0.08f, 900f, additive: true);
                return;
            }

            var sp = skillPulse
                ? GrowArt.First("GrowArt/Fx/SkillPulse", "GrowArt/Fx/Hit1")
                : GrowArt.First("GrowArt/Fx/Hit1", "GrowArt/Fx/Hit2", "GrowArt/Fx/Hit3");
            if (sp == null) return;
            SpawnStatic(sp, anchoredPos, NativeSize(sp, 3.5f, 64f, 120f), 0.28f, Color.white, 880f, true);
        }

        public void PlaySkill(int skillId, Vector2 anchoredPos)
        {
            if (_root == null) return;
            int id = ((skillId % 4) + 4) % 4;

            // Scary Scream: high-res skull sheet from template Skull art
            if (id == 3)
            {
                var skull = GetSkullFrames();
                if (skull != null && skull.Length > 0)
                {
                    float size = NativeSize(skull[Mathf.Min(8, skull.Length - 1)], SkillScale[3], 140f, 280f);
                    SpawnAnim(skull, anchoredPos + new Vector2(0f, 48f), size, SkillFps[3],
                        Blend(SkillTints[3]), 0.05f, 520f, additive: true);
                    return;
                }
            }

            string folder = SkillFolders[id];
            var frames = GrowArt.FxSequence(folder);
            if (frames == null || frames.Length == 0)
            {
                PlayHit(anchoredPos, skillPulse: true);
                return;
            }

            PointFilter(frames);
            float sz = NativeSize(frames[0], SkillScale[id], 90f, 200f);
            // Lightning is tall — prefer height-driven box
            if (id == 0)
                sz = Mathf.Clamp(frames[0].rect.height * SkillScale[id], 110f, 240f);

            SpawnAnim(frames, anchoredPos + new Vector2(0f, id == 0 ? 70f : 40f), sz, SkillFps[id],
                Blend(SkillTints[id]), 0.06f, 620f + id * 50f, additive: true);
            // 세력 기운 파동 — 광역으로 쓸어내는 맛을 준다
            StartCoroutine(AoeRingCo(id, anchoredPos, 210f, FactionTint()));
        }

        /// <summary>
        /// Glowing bolt that actually travels from → to, then fires onArrive.
        /// arcHeight > 0 curves the path (lobbed arrows / meteor drop feel).
        /// </summary>
        public void PlayProjectile(int skillId, Vector2 from, Vector2 to, float duration,
            float arcHeight, System.Action onArrive)
        {
            if (_root == null) { onArrive?.Invoke(); return; }
            StartCoroutine(ProjectileCo(skillId, from, to, Mathf.Max(0.08f, duration), arcHeight, onArrive));
        }

        IEnumerator ProjectileCo(int skillId, Vector2 from, Vector2 to, float duration,
            float arcHeight, System.Action onArrive)
        {
            int id = ((skillId % 4) + 4) % 4;
            var tint = SkillTints[id];

            var go = RentGo("Bolt");
            go.transform.SetParent(_root, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(46f, 16f);
            rt.localRotation = Quaternion.identity;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = IdleMvp.UI.Maple.MapleLightTheme.RoundedSprite(8);
            img.type = Image.Type.Sliced;
            img.color = new Color(tint.r, tint.g, tint.b, 0.95f);
            img.raycastTarget = false;
            img.material = AdditiveMat();

            var trail = rt.childCount > 0 ? rt.GetChild(0).gameObject : new GameObject("Trail", typeof(RectTransform));
            trail.transform.SetParent(go.transform, false);
            var trt = (RectTransform)trail.transform;
            trt.sizeDelta = new Vector2(70f, 8f);
            trt.anchoredPosition = new Vector2(-34f, 0f);
            var timg = trail.GetComponent<Image>();
            if (timg == null) timg = trail.AddComponent<Image>();
            timg.sprite = IdleMvp.UI.Maple.MapleLightTheme.RoundedSprite(4);
            timg.type = Image.Type.Sliced;
            timg.color = new Color(tint.r, tint.g, tint.b, 0.35f);
            timg.raycastTarget = false;
            timg.material = AdditiveMat();

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                var pos = Vector2.Lerp(from, to, u);
                pos.y += Mathf.Sin(u * Mathf.PI) * arcHeight;
                // face travel direction (including arc slope)
                float du = Mathf.Min(1f, u + 0.02f);
                var ahead = Vector2.Lerp(from, to, du);
                ahead.y += Mathf.Sin(du * Mathf.PI) * arcHeight;
                var dir = ahead - pos;
                if (dir.sqrMagnitude > 0.001f)
                    rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                rt.anchoredPosition = pos;
                yield return null;
            }
            ReturnGo(go);
            onArrive?.Invoke();
        }

        /// <summary>Expanding blast ring for AoE skills.</summary>
        public void PlayAoeBlast(int skillId, Vector2 center, float radius)
        {
            if (_root == null) return;
            StartCoroutine(AoeRingCo(skillId, center, radius, null));
        }

        /// <summary>색을 직접 지정하는 판 — 경지 기운처럼 스킬과 무관한 연출에 쓴다.</summary>
        public void PlayAoeBlast(Vector2 center, float radius, Color tint)
        {
            if (_root == null) return;
            StartCoroutine(AoeRingCo(0, center, radius, tint));
        }

        IEnumerator AoeRingCo(int skillId, Vector2 center, float radius, Color? overrideTint)
        {
            int id = ((skillId % 4) + 4) % 4;
            var tint = overrideTint ?? Blend(SkillTints[id]);
            var go = RentGo("AoeRing");
            go.transform.SetParent(_root, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = center;
            rt.localRotation = Quaternion.identity;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            // big-radius rounded sprite reads as a circle when rect is square
            img.sprite = IdleMvp.UI.Maple.MapleLightTheme.RoundedSprite(64);
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            img.material = AdditiveMat();
            const float dur = 0.38f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                float d = Mathf.Lerp(radius * 0.4f, radius * 2f, 1f - (1f - u) * (1f - u));
                rt.sizeDelta = new Vector2(d, d * 0.45f); // 필드 원근감: 납작한 타원
                img.color = new Color(tint.r, tint.g, tint.b, 0.5f * (1f - u));
                yield return null;
            }
            ReturnGo(go);
        }

        static TMPro.TMP_FontAsset _dmgFont;
        /// <summary>Floating damage number (strong = skill/crit styling).</summary>
        public void PopDamage(Vector2 anchoredPos, float amount, bool strong)
        {
            if (_root == null) return;
            if (_dmgFont == null)
                _dmgFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts/UIHangulSDF");
            var go = RentGo("Dmg");
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(220f, 44f);
            rt.anchoredPosition = anchoredPos + new Vector2(Random.Range(-14f, 14f), 0f);
            var t = go.GetComponent<TMPro.TextMeshProUGUI>();
            if (t == null) t = go.AddComponent<TMPro.TextMeshProUGUI>();
            if (_dmgFont != null) t.font = _dmgFont;
            t.text = IdleMvp.UI.Maple.UiKit.Num(amount);
            t.fontSize = strong ? 34f : 24f;
            t.fontStyle = TMPro.FontStyles.Bold;
            t.alignment = TMPro.TextAlignmentOptions.Center;
            t.color = strong ? new Color(1f, 0.82f, 0.25f, 1f) : Color.white;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            StartCoroutine(DmgPopCo(rt, t, strong));
        }

        /// <summary>드랍 획득 문구 — 데미지 숫자와 같은 풀로 짧은 라벨을 띄운다.</summary>
        public void PopLabel(string text, Vector2 anchoredPos, Color color)
        {
            if (_root == null) return;
            if (_dmgFont == null)
                _dmgFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts/UIHangulSDF");
            var go = RentGo("Dmg");
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(260f, 44f);
            rt.anchoredPosition = anchoredPos + new Vector2(Random.Range(-10f, 10f), 0f);
            var t = go.GetComponent<TMPro.TextMeshProUGUI>();
            if (t == null) t = go.AddComponent<TMPro.TextMeshProUGUI>();
            if (_dmgFont != null) t.font = _dmgFont;
            t.text = text;
            t.fontSize = 26f;
            t.fontStyle = TMPro.FontStyles.Bold;
            t.alignment = TMPro.TextAlignmentOptions.Center;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            StartCoroutine(DmgPopCo(rt, t, false));
        }

        Material AdditiveMat()
        {
            if (_additiveMat == null)
            {
                var sh = Shader.Find("IdleMvp/UiAdditive");
                if (sh != null) _additiveMat = new Material(sh);
            }
            return _additiveMat;
        }

        public void PlayCastFlash(int skillId, Vector2 anchoredPos)
        {
            if (_root == null) return;
            int id = ((skillId % 4) + 4) % 4;
            if (id == 3)
            {
                var skull = GetSkullFrames();
                if (skull != null && skull.Length > 4)
                {
                    SpawnStatic(skull[4], anchoredPos + new Vector2(0f, 24f), 72f, 0.2f,
                        Blend(SkillTints[3]), 700f, false);
                    return;
                }
            }

            var frames = GrowArt.FxSequence(SkillFolders[id]);
            if (frames == null || frames.Length == 0)
            {
                PlayHit(anchoredPos, true);
                return;
            }
            int mid = Mathf.Clamp(frames.Length / 2, 0, frames.Length - 1);
            PointFilter(frames);
            SpawnStatic(frames[mid], anchoredPos + new Vector2(0f, 28f),
                NativeSize(frames[mid], 2.8f, 56f, 96f), 0.2f, SkillTints[id], 760f + id * 30f, true);
        }

        void SpawnAnim(Sprite[] frames, Vector2 anchoredPos, float size, float fps, Color tint,
            float settleHold, float beepHz, bool additive)
        {
            var go = new GameObject("SkillFx", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.3f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = anchoredPos + new Vector2(0f, 64f);

            var img = go.GetComponent<Image>();
            img.sprite = frames[0];
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.color = tint;
            if (additive)
            {
                var mat = AdditiveMaterial();
                if (mat != null) img.material = mat;
            }

            Beep(beepHz);
            StartCoroutine(PlayFrames(img, frames, fps, settleHold));
        }

        void SpawnStatic(Sprite sp, Vector2 anchoredPos, float size, float dur, Color tint, float beepHz,
            bool additive)
        {
            var go = new GameObject("Fx", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_root, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = anchoredPos + new Vector2(0f, 72f);

            var img = go.GetComponent<Image>();
            img.sprite = sp;
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.color = tint;
            if (additive)
            {
                var mat = AdditiveMaterial();
                if (mat != null) img.material = mat;
            }

            Beep(beepHz);
            StartCoroutine(FadeAndKill(img, rt, dur));
        }

        IEnumerator PlayFrames(Image img, Sprite[] frames, float fps, float settleHold)
        {
            float frameDur = 1f / Mathf.Max(8f, fps);
            for (int i = 0; i < frames.Length; i++)
            {
                if (img == null) yield break;
                img.sprite = frames[i];
                yield return new WaitForSecondsRealtime(frameDur);
            }

            if (img == null) yield break;
            float t = 0f;
            var c0 = img.color;
            while (t < settleHold)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / Mathf.Max(0.01f, settleHold));
                img.color = new Color(c0.r, c0.g, c0.b, (1f - u) * c0.a);
                yield return null;
            }
            if (img != null) Destroy(img.gameObject);
        }

        IEnumerator DmgPopCo(RectTransform rt, TMPro.TMP_Text t, bool strong)
        {
            float dur = strong ? 0.7f : 0.55f;
            var start = rt.anchoredPosition;
            float el = 0f;
            while (el < dur)
            {
                el += Time.deltaTime;
                float u = Mathf.Clamp01(el / dur);
                rt.anchoredPosition = start + new Vector2(0f, (strong ? 64f : 46f) * (1f - (1f - u) * (1f - u)));
                float sc = strong ? Mathf.Lerp(1.35f, 1f, Mathf.Min(1f, u * 3f)) : 1f;
                rt.localScale = new Vector3(sc, sc, 1f);
                var c = t.color; c.a = u < 0.55f ? 1f : 1f - (u - 0.55f) / 0.45f;
                t.color = c;
                yield return null;
            }
            if (rt != null) ReturnGo(rt.gameObject);
        }

        IEnumerator FadeAndKill(Image img, RectTransform rt, float dur)
        {
            float t = 0f;
            var c0 = img.color;
            var s0 = rt.localScale;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                img.color = new Color(c0.r, c0.g, c0.b, c0.a * (1f - u));
                rt.localScale = s0 * (1f + u * 0.25f);
                yield return null;
            }
            if (img != null) Destroy(img.gameObject);
        }

        static float NativeSize(Sprite sp, float scale, float min, float max)
        {
            if (sp == null) return min;
            float n = Mathf.Max(sp.rect.width, sp.rect.height) * scale;
            return Mathf.Clamp(n, min, max);
        }

        static void PointFilter(Sprite[] frames)
        {
            if (frames == null) return;
            foreach (var s in frames)
            {
                if (s != null && s.texture != null)
                    s.texture.filterMode = FilterMode.Point;
            }
        }

        static Material AdditiveMaterial()
        {
            if (_additiveMat != null) return _additiveMat;
            var shader = Shader.Find("IdleMvp/UiAdditive")
                         ?? Shader.Find("Particles/Additive")
                         ?? Shader.Find("Legacy Shaders/Particles/Additive")
                         ?? Shader.Find("Mobile/Particles/Additive");
            if (shader == null) return null;
            _additiveMat = new Material(shader);
            return _additiveMat;
        }

        /// <summary>Slice IdleRPG Skull spritesheet (18×2) at runtime.</summary>
        static Sprite[] GetSkullFrames()
        {
            if (_skullFrames != null) return _skullFrames;

            var tex = Resources.Load<Texture2D>("GrowArt/Fx/Scream/skull");
            if (tex == null)
            {
                var single = GrowArt.Load("GrowArt/Fx/Scream/skull");
                if (single != null) _skullFrames = new[] { single };
                else _skullFrames = System.Array.Empty<Sprite>();
                return _skullFrames;
            }

            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            const int cols = 18;
            const int rows = 2;
            float fw = tex.width / (float)cols;
            float fh = tex.height / (float)rows;
            var list = new List<Sprite>(cols * rows);
            // Top row first (appear), then bottom row (dissipate) — Unity UV origin bottom-left
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    var rect = new Rect(c * fw, r * fh, fw, fh);
                    list.Add(Sprite.Create(tex, rect, new Vector2(0.5f, 0.35f), 100f));
                }
            }
            _skullFrames = list.ToArray();
            return _skullFrames;
        }

        void Beep(float hz)
        {
            if (_audio == null) return;
            _audio.PlayOneShot(MakeBeep(hz, 0.055f), 0.28f);
        }

        static AudioClip MakeBeep(float freq, float seconds)
        {
            int rate = 22050;
            int samples = Mathf.CeilToInt(rate * seconds);
            var clip = AudioClip.Create("HitBeep", samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float env = 1f - (i / (float)samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)rate)) * env * 0.35f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
