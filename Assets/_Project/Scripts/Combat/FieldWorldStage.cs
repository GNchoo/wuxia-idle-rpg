using IdleMvp.UI;
using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// World-space layer behind the overlay HUD: battle background sprite and
    /// field-pixel to world-unit conversion for world actors.
    /// Field coords: x from field bottom-center, y above field bottom (canvas px).
    /// </summary>
    public class FieldWorldStage : MonoBehaviour
    {
        public static FieldWorldStage Instance { get; private set; }

        Camera _cam;
        RectTransform _field;
        SpriteRenderer _bg;

        public static FieldWorldStage Ensure(RectTransform field)
        {
            if (Instance == null)
            {
                var go = new GameObject("FieldWorldStage");
                Instance = go.AddComponent<FieldWorldStage>();
            }
            Instance.Bind(field);
            return Instance;
        }

        void Bind(RectTransform field)
        {
            _field = field;
            _cam = Camera.main;
            if (_cam == null) return;

            // Camera must sit behind the z=0 sprite plane.
            if (_cam.transform.position.z > -1f)
                _cam.transform.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y, -10f);
            _cam.orthographic = true;

            BuildBackground();
        }

        void BuildBackground()
        {
            var sprite = GrowArt.BattleBg;
            if (sprite == null) return;

            if (_bg == null)
            {
                var go = new GameObject("BattleBgWorld");
                go.transform.SetParent(transform, false);
                _bg = go.AddComponent<SpriteRenderer>();
                _bg.sortingOrder = -100;
                // Initial sprite only — SyncChapterBg owns swaps afterwards.
                // (Ensure() fires on every actor spawn; it must not stomp the biome art.)
                _bg.sprite = sprite;
            }
            FitBackground();
        }

        int _chapter = -1;
        SpriteRenderer[] _platformBodies;
        SpriteRenderer[] _platformEdges;
        SpriteRenderer _ground;
        Transform _propsRoot;
        int _propChapter = -1;
        static Sprite _white;

        static Sprite Terrain(string name) => Resources.Load<Sprite>("TerrainArt/" + name);

        /// <summary>
        /// 맵 스크롤 (필드 픽셀). 화면은 그대로 두고 필드 좌표계를 옆으로 밀어
        /// 히어로가 넓은 맵을 돌아다니는 것처럼 보이게 한다.
        /// </summary>
        public float ScrollX { get; private set; }

        public void SetScrollX(float x)
        {
            ScrollX = x;
        }

        void LateUpdate()
        {
            if (_bg == null) return;
            SyncChapterBg();
            FitBackground();
            PositionPlatforms();
            PositionGround();
            EnsureProps();
        }

        static Sprite WhiteSprite()
        {
            if (_white != null) return _white;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _white = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return _white;
        }

        /// <summary>발판 하나가 어느 층·어느 칸인지 — 매 프레임 위치를 다시 계산할 때 쓴다.</summary>
        class PlatformAnchor : MonoBehaviour
        {
            public int Floor;
            public int Index;
        }

        int _platformChapter = -1;

        /// <summary>전투 중인 스테이지 — 사냥/돌파에 따라 다르다. 컨트롤러와 같은 값을 봐야 배치가 안 어긋난다.</summary>
        static int ActiveStage(IdleMvp.Progression.StageProgress sp)
        {
            var c = FieldAutoHuntController.Instance;
            return c != null ? c.ActiveStageIndex() : sp.StageIndex;
        }

        /// <summary>맵 절반 폭(필드 px). 발판 비율 좌표를 실제 좌표로 펴는 기준 — 컨트롤러와 같은 값을 써야 한다.</summary>
        public float MapHalfWidthPx()
        {
            float halfW = _field != null && _field.rect.width > 10f
                ? Mathf.Max(200f, _field.rect.width * 0.5f - 20f) : 420f;
            return halfW * FieldAutoHuntController.MapWidthFactor;
        }

        void EnsurePlatforms()
        {
            int ch = _chapter < 1 ? 1 : _chapter;
            if (_platformBodies != null && _platformChapter == ch) return;
            _platformChapter = ch;

            if (_platformBodies != null)
                foreach (var old in _platformBodies)
                    if (old != null) Destroy(old.gameObject);

            int theme = MapLayout.ThemeOf(ch);
            var walk = Terrain("terrain_walkway");   // U1 잔도 아트 — 없으면 기존 바 폴백
            var tint = MapLayout.Tint(theme);
            var list = new System.Collections.Generic.List<SpriteRenderer>();
            var edges = new System.Collections.Generic.List<SpriteRenderer>();

            // 층 0은 지면 띠가 그리므로 1층부터. 발판이 없는 층은 그냥 건너뛴다.
            for (int floor = 1; floor < FieldAutoHuntController.FloorCount; floor++)
            {
                int n = MapLayout.Count(theme, floor);
                for (int i = 0; i < n; i++)
                {
                    var body = new GameObject("Platform" + floor + "_" + i).AddComponent<SpriteRenderer>();
                    body.transform.SetParent(transform, false);
                    body.sprite = walk != null ? walk : WhiteSprite();
                    body.color = walk != null ? tint : new Color(0.25f * tint.r, 0.20f * tint.g, 0.16f * tint.b, 0.97f);
                    body.sortingOrder = -20;
                    var a = body.gameObject.AddComponent<PlatformAnchor>();
                    a.Floor = floor;
                    a.Index = i;
                    list.Add(body);

                    var edge = new GameObject("Edge").AddComponent<SpriteRenderer>();
                    edge.transform.SetParent(body.transform, false);
                    edge.sprite = WhiteSprite();
                    edge.color = new Color(0.62f * tint.r, 0.50f * tint.g, 0.34f * tint.b, 1f);
                    edge.sortingOrder = -19;
                    edge.gameObject.SetActive(walk == null);   // 아트가 있으면 모서리 바 불필요
                    edges.Add(edge);
                }
            }
            _platformBodies = list.ToArray();
            _platformEdges = edges.ToArray();
        }

        void PositionPlatforms()
        {
            EnsurePlatforms();
            if (_cam == null || _field == null) return;
            float unitsPerPx = UnitsPerFieldPx;
            int theme = MapLayout.ThemeOf(_platformChapter);
            float thick = MapLayout.Thickness(theme);
            float mapHalfW = MapHalfWidthPx();
            for (int i = 0; i < _platformBodies.Length; i++)
            {
                var body = _platformBodies[i];
                if (body == null) continue;
                var a = body.GetComponent<PlatformAnchor>();
                if (a == null) continue;
                int floor = a.Floor;
                float minX, maxX;
                MapLayout.Bounds(theme, _platformChapter, floor, a.Index, mapHalfW, out minX, out maxX);
                float y = FieldAutoHuntController.FloorY(floor) - 10f;
                var center = FieldToWorld(new Vector2((minX + maxX) * 0.5f, y));
                float w = (maxX - minX) * unitsPerPx;
                bool art = body.sprite != null && body.sprite != _white;
                float hBody = (art ? thick : thick * 0.78f) * unitsPerPx;
                body.transform.position = new Vector3(center.x, center.y - hBody * 0.5f, 5f);
                if (art)
                {
                    var s = body.sprite.bounds.size;
                    body.transform.localScale = new Vector3(w / Mathf.Max(0.01f, s.x), hBody / Mathf.Max(0.01f, s.y), 1f);
                }
                else
                {
                    body.transform.localScale = new Vector3(w, hBody, 1f);
                    var edge = _platformEdges[i];
                    edge.transform.localPosition = new Vector3(0f, 0.5f - (3f * unitsPerPx) / Mathf.Max(0.0001f, hBody) * 0.5f, 0f);
                    edge.transform.localScale = new Vector3(1f, (4f * unitsPerPx) / Mathf.Max(0.0001f, hBody), 1f);
                }
            }
        }

        /// <summary>U1 바닥 지형 띠 — 지면 라인 아래를 바이옴 톤 지형으로 채운다.</summary>
        void PositionGround()
        {
            if (_cam == null || _field == null) return;
            var spg = IdleMvp.Progression.StageProgress.Instance;
            var row = IdleMvp.Progression.StageTable.Get(spg != null ? ActiveStage(spg) : 1);
            // 석굴(테마 2)·마교(테마 4) 계열과 상위 티어는 암반 지형, 나머지는 흙·풀 지형
            int theme = row != null ? MapLayout.ThemeOf(row.chapter) : 0;
            bool rocky = theme == 2 || theme == 4 || (row != null && row.mapTier >= 2);
            string name = rocky ? "terrain_ground_cave" : "terrain_ground_forest";
            var sp = Terrain(name);
            if (sp == null) { if (_ground != null) _ground.gameObject.SetActive(false); return; }

            if (_ground == null)
            {
                _ground = new GameObject("GroundStrip").AddComponent<SpriteRenderer>();
                _ground.transform.SetParent(transform, false);
                _ground.sortingOrder = -20;
            }
            _ground.gameObject.SetActive(true);
            if (_ground.sprite != sp) _ground.sprite = sp;

            float unitsPerPx = UnitsPerFieldPx;
            float worldW = _cam.orthographicSize * 2f * _cam.aspect;
            float hStrip = 78f * unitsPerPx;
            var top = FieldToWorld(new Vector2(0f, FieldAutoHuntController.GroundY - 8f));
            var size = _ground.sprite.bounds.size;
            _ground.transform.position = new Vector3(top.x, top.y - hStrip * 0.5f, 5f);
            _ground.transform.localScale = new Vector3(
                worldW / Mathf.Max(0.01f, size.x), hStrip / Mathf.Max(0.01f, size.y), 1f);
        }

        /// <summary>U1 소품 — 층마다 바위·풀·등불 등을 챕터 고정 시드로 흩뿌린다.</summary>
        static readonly string[] PropNames = { "prop_rock", "prop_grass", "prop_lantern", "prop_flag", "prop_bamboo" };

        /// <summary>
        /// 20개 챕터를 5개 테마로 묶고 테마마다 소품 구성을 달리한다.
        /// 죽림 → 설산 → 동굴 → 황야 → 마교. 인덱스는 PropNames.
        /// </summary>
        static readonly int[][] ThemeProps =
        {
            new[] { 4, 4, 1, 2 },   // 죽림·마을 — 대나무·풀·등불
            new[] { 0, 0, 3, 1 },   // 설산 — 바위·깃발
            new[] { 0, 0, 0, 2 },   // 동굴 — 바위·등불
            new[] { 0, 3, 1, 3 },   // 황야 — 바위·깃발·마른 풀
            new[] { 2, 3, 0, 2 },   // 마교 본거지 — 등불·깃발
        };

        void EnsureProps()
        {
            var sp0 = Terrain(PropNames[0]);
            if (sp0 == null) return;   // 소품 아트 미설치 — 조용히 스킵
            int ch = _chapter < 1 ? 1 : _chapter;
            if (_propChapter == ch && _propsRoot != null) { PositionPropsRoot(); return; }
            _propChapter = ch;
            if (_propsRoot != null) Destroy(_propsRoot.gameObject);
            _propsRoot = new GameObject("Props").transform;
            _propsRoot.SetParent(transform, false);

            var rng = new System.Random(ch * 7919);   // 챕터 고정 시드 — 리프레시에도 흔들리지 않게
            var theme = ThemeProps[Mathf.Clamp((ch - 1) / 4, 0, ThemeProps.Length - 1)];
            // 소품은 발판 위에만 올린다 — 발판이 없는 허공에 바위가 떠 있으면 안 된다
            int layout = MapLayout.ThemeOf(ch);
            float mapHalfW = MapHalfWidthPx();
            int mul = Mathf.Max(1, Mathf.RoundToInt(FieldAutoHuntController.MapWidthFactor));
            for (int floor = 0; floor < FieldAutoHuntController.FloorCount; floor++)
            for (int seg = 0; seg < MapLayout.Count(layout, floor); seg++)
            {
                float segMin, segMax;
                MapLayout.Bounds(layout, ch, floor, seg, mapHalfW, out segMin, out segMax);
                // 넓은 발판일수록 많이 — 좁은 징검다리에 소품을 쑤셔 넣지 않는다
                int count = Mathf.Clamp(Mathf.RoundToInt((segMax - segMin) / 260f), 1, 4 * mul);
                for (int k = 0; k < count; k++)
                {
                    var sr = new GameObject("P").AddComponent<SpriteRenderer>();
                    sr.transform.SetParent(_propsRoot, false);
                    sr.sprite = Terrain(PropNames[theme[rng.Next(theme.Length)]]);
                    if (sr.sprite == null) { Destroy(sr.gameObject); continue; }
                    sr.sortingOrder = -18;
                    // 위치·크기는 필드 px 기준으로 기록해 두고 매 프레임 월드로 변환
                    float pad = Mathf.Min(60f, (segMax - segMin) * 0.15f);
                    float fx = Mathf.Lerp(segMin + pad, segMax - pad, (float)rng.NextDouble());
                    sr.gameObject.name = "P_" + floor + "_" + fx.ToString("0");
                    var holder = sr.gameObject.AddComponent<PropAnchor>();
                    holder.Floor = floor;
                    holder.FieldX = fx;
                    holder.HeightPx = 40f + (float)rng.NextDouble() * 22f;
                }
            }
            PositionPropsRoot();
        }

        class PropAnchor : MonoBehaviour
        {
            public int Floor;
            public float FieldX;
            public float HeightPx;
        }

        void PositionPropsRoot()
        {
            if (_propsRoot == null) return;
            float unitsPerPx = UnitsPerFieldPx;
            foreach (Transform t in _propsRoot)
            {
                var a = t.GetComponent<PropAnchor>();
                var sr = t.GetComponent<SpriteRenderer>();
                if (a == null || sr == null || sr.sprite == null) continue;
                float h = a.HeightPx * unitsPerPx;
                float k = h / Mathf.Max(0.01f, sr.sprite.bounds.size.y);
                // 잔도 윗면이 FloorY-10 — 소품 바닥을 거기에 붙인다 (떠 보이지 않게)
                var pos = FieldToWorld(new Vector2(a.FieldX, FieldAutoHuntController.FloorY(a.Floor) - 10f));
                t.position = new Vector3(pos.x, pos.y + h * 0.5f, 6f);
                t.localScale = new Vector3(k, k, 1f);
            }
        }

        /// <summary>Swap background to the current hunting chapter's biome art.</summary>
        void SyncChapterBg()
        {
            var sp = IdleMvp.Progression.StageProgress.Instance;
            if (sp == null) return;
            var row = IdleMvp.Progression.StageTable.Get(ActiveStage(sp));
            int ch = row != null ? row.chapter : 1;
            if (ch == _chapter) return;
            var s = Resources.Load<Sprite>($"TplArt/Biomes/Biome{Mathf.Clamp(ch, 1, 20):00}");
            if (s == null) return; // biome art not ready yet — retry next frame, don't cache
            _chapter = ch;
            _bg.sprite = s;
        }

        void FitBackground()
        {
            if (_cam == null || _bg == null || _bg.sprite == null) return;
            float worldH = _cam.orthographicSize * 2f;
            float worldW = worldH * _cam.aspect;
            var size = _bg.sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f) return;
            // 시차로 배경이 옆으로 밀려도 화면 밖이 드러나지 않게 여유를 두고 키운다
            const float ParallaxMargin = 1.9f;
            float scale = Mathf.Max(worldW / size.x, worldH / size.y) * ParallaxMargin;
            _bg.transform.localScale = new Vector3(scale, scale, 1f);
            var c = _cam.transform.position;
            // 배경은 맵보다 천천히 흘러 원경처럼 보인다 (시차)
            float par = -ScrollX * UnitsPerFieldPx * 0.35f;
            // 확대로 생긴 여유 폭 안에서만 민다 — 넘기면 배경 끝이 보인다
            float slack = Mathf.Max(0f, (size.x * scale - worldW) * 0.5f);
            par = Mathf.Clamp(par, -slack, slack);
            _bg.transform.position = new Vector3(c.x + par, c.y, 10f);
        }

        /// <summary>World position for a field-space point (x from bottom-center, y above bottom).</summary>
        public Vector3 FieldToWorld(Vector2 fieldPos)
        {
            if (_cam == null || _field == null)
                return new Vector3(fieldPos.x * 0.01f, fieldPos.y * 0.01f, 0f);

            var rect = _field.rect;
            var local = new Vector3(rect.center.x + fieldPos.x - ScrollX, rect.yMin + fieldPos.y, 0f);
            // Overlay canvas: TransformPoint yields screen pixels.
            Vector3 screen = _field.TransformPoint(local);
            var world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_cam.transform.position.z));
            world.z = 0f;
            return world;
        }

        /// <summary>World units per field pixel (uniform for orthographic camera).</summary>
        public float UnitsPerFieldPx
        {
            get
            {
                var a = FieldToWorld(Vector2.zero);
                var b = FieldToWorld(new Vector2(100f, 0f));
                float d = Mathf.Abs(b.x - a.x);
                return d > 0.0001f ? d / 100f : 0.01f;
            }
        }
    }
}
