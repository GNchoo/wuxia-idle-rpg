using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// xAI(Grok Imagine)로 장비 아이콘을 생성해 Resources/EquipIcons 에 넣는다.
    ///
    /// 키는 이 스크립트에 절대 적지 않는다. 환경변수 XAI_API_KEY 만 읽는다.
    ///   PowerShell(영구): [Environment]::SetEnvironmentVariable("XAI_API_KEY","...","User")
    ///   설정 후 Unity를 재시작해야 에디터 프로세스가 값을 물려받는다.
    ///
    /// 두 가지를 1차 시험에서 배웠다:
    ///  1) 'transparent background'를 요구하면 모델이 알파를 주는 게 아니라
    ///     체커보드를 '그림으로' 그린다 → 프롬프트에서 크로마키 단색을 요구하고
    ///     여기서 그 색을 지워 알파를 만든다.
    ///  2) 동기 대기(Thread.Sleep)로 돌리면 에디터가 통째로 멈춘다(60장이면 30분+)
    ///     → EditorApplication.update 에서 한 장씩 진행하는 비동기 큐로 바꿨다.
    /// </summary>
    public static class XaiIconGenerator
    {
        const string Endpoint = "https://api.x.ai/v1/images/generations";
        const string Model = "grok-imagine-image-2.0";
        const string PromptFile = "docs/equip-icon-prompts.json";
        const string OutDir = "Assets/_Project/Resources/EquipIcons";
        const string WearDir = "Assets/_Project/Resources/WearParts";   // 착용 무기(옆모습, 이펙트 없음)
        const string ArmorDir = "Assets/_Project/Resources/ArmorParts"; // 착용 방어구(부위별 조각)
        const string SkillDir = "Assets/_Project/Resources/SkillIcons"; // 스킬 아이콘 (트리별)
        const string ArtifactDir = "Assets/_Project/Resources/ArtifactIcons"; // 유물 아이콘
        const string BiomeDir = "Assets/_Project/Resources/TplArt/Biomes";   // 챕터 배경 (덮어쓰기)
        const string BossDir = "Assets/_Project/Resources/TplArt/Bosses";    // 챕터 보스 (덮어쓰기)

        [Serializable]
        class PromptItem
        {
            public string id;
            public string name;
            public string prompt;
            public string kind;        // "icon" | "wear" | "armor" (없으면 단일 이미지)
            public string[] targets;   // 시트를 잘라 저장할 파일명들 (왼→오)
            public int width;          // 0이면 1280 — 배경류만 2048 권장 (로컬 백엔드 전용)
            public int height;         // 0이면 720
        }
        [Serializable] class PromptFileDoc { public string styleIcon; public PromptItem[] items; }

        static readonly Queue<PromptItem> _queue = new Queue<PromptItem>();
        static UnityWebRequest _req;
        static PromptItem _cur;
        static int _total, _done, _fail;
        static bool _running;

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 시트 1장", priority = 99)]
        public static void GenerateOneSample() => Enqueue(new[] { "sheet_icon_s0_0" });  // 검 1-5티어 시트

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 시트 3장", priority = 100)]
        public static void GenerateSample() => Enqueue(new[] { "sheet_icon_s0_0", "sheet_icon_s0_1", "sheet_wear_k0" });

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 전체 시트", priority = 101)]
        public static void GenerateAll()
        {
            if (!EditorUtility.DisplayDialog("무협 아트 전체 생성",
                    "docs/equip-icon-prompts.json 의 모든 시트를 다시 요청합니다.\n" +
                    "시트 한 장에서 5~6개를 잘라냅니다.\n" +
                    "계정에 실제 요금이 청구됩니다.\n계속할까요?",
                    "생성", "취소")) return;
            Enqueue(null);
        }

        /// <summary>방어구 9부위 — 몸통·어깨·손·발·허리·목·소매·바지허리·바지다리.</summary>
        public static readonly string[] ArmorSheets =
        {
            "sheet_armor_body", "sheet_armor_shoulder", "sheet_armor_glove",
            "sheet_armor_shoes", "sheet_armor_belt", "sheet_armor_neck",
            "sheet_armor_sleeve", "sheet_armor_pants", "sheet_armor_pantsleg",
        };

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 방어구 9시트", priority = 103)]
        public static void GenerateArmor()
        {
            if (!EditorUtility.DisplayDialog("무협 방어구 9시트 생성",
                    "xAI에 9회 요청합니다.\n시트마다 6티어를 잘라내므로 결과는 54조각입니다.\n" +
                    "계정에 실제 요금이 청구됩니다.\n계속할까요?",
                    "생성", "취소")) return;
            Enqueue(ArmorSheets);
        }

        /// <summary>스킬 아이콘 7시트 — 정파/사파/마도 각 2장 + 융합 1장 = 28종.</summary>
        public static readonly string[] SkillSheets =
        {
            "sheet_skill_hero_a", "sheet_skill_hero_b",
            "sheet_skill_bow_a", "sheet_skill_bow_b",
            "sheet_skill_mage_a", "sheet_skill_mage_b",
            "sheet_skill_hidden",
        };

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 스킬 아이콘 7시트", priority = 104)]
        public static void GenerateSkillIcons()
        {
            if (!EditorUtility.DisplayDialog("스킬 아이콘 7시트 생성",
                    "xAI에 7회 요청합니다.\n시트마다 4개를 잘라내므로 결과는 28종입니다.\n" +
                    "계정에 실제 요금이 청구됩니다.\n계속할까요?",
                    "생성", "취소")) return;
            Enqueue(SkillSheets);
        }

        [MenuItem("IdleMvp/아트/무협 아트 생성 (Grok) — 유물 아이콘 2시트", priority = 105)]
        public static void GenerateArtifactIcons()
        {
            if (!EditorUtility.DisplayDialog("유물 아이콘 2시트 생성",
                    "xAI에 2회 요청합니다. 결과는 8종입니다.\n계정에 실제 요금이 청구됩니다.\n계속할까요?",
                    "생성", "취소")) return;
            Enqueue(new[] { "sheet_artifact_a", "sheet_artifact_b" });
        }

        [MenuItem("IdleMvp/아트/무협 아트 생성 중단", priority = 102)]
        public static void Stop()
        {
            _queue.Clear();
            if (_req != null) { _req.Abort(); _req.Dispose(); _req = null; }
            _running = false;
            EditorApplication.update -= Pump;
            EditorUtility.ClearProgressBar();
            Debug.Log("[XaiIconGen] 중단됨");
        }

        /// <summary>onlyIds 가 null 이면 전부. 대화상자 없이 바로 시작한다.</summary>
        public static void Enqueue(string[] onlyIds)
        {
            if (_running) { Debug.LogWarning("[XaiIconGen] 이미 진행 중입니다."); return; }

            if (!UseLocal && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XAI_API_KEY")))
            {
                EditorUtility.DisplayDialog("XAI_API_KEY 없음",
                    "환경변수 XAI_API_KEY 를 설정한 뒤 Unity를 재시작하세요.\n" +
                    "키는 코드나 프로젝트 파일에 저장하지 않습니다.", "확인");
                return;
            }

            string pf = Path.Combine(Directory.GetCurrentDirectory(), PromptFile);
            if (!File.Exists(pf)) { Debug.LogError("[XaiIconGen] 프롬프트 없음: " + PromptFile); return; }

            var doc = JsonUtility.FromJson<PromptFileDoc>(File.ReadAllText(pf, Encoding.UTF8));
            if (doc?.items == null || doc.items.Length == 0) { Debug.LogError("[XaiIconGen] items 비어있음"); return; }

            _queue.Clear();
            foreach (var it in doc.items)
            {
                if (onlyIds == null) { _queue.Enqueue(it); continue; }
                foreach (var id in onlyIds) if (it.id == id) { _queue.Enqueue(it); break; }
            }

            Directory.CreateDirectory(OutDir);
            _total = _queue.Count; _done = 0; _fail = 0; _running = true;
            EditorApplication.update -= Pump;
            EditorApplication.update += Pump;
            Debug.Log("[XaiIconGen] 시작 — " + _total + "장 (에디터는 멈추지 않습니다)");
        }

        /// <summary>에디터 프레임마다 조금씩 진행한다. 요청 하나가 끝나야 다음을 보낸다.</summary>
        static void Pump()
        {
            if (!_running) { EditorApplication.update -= Pump; return; }

            if (_req == null)
            {
                if (_queue.Count == 0)
                {
                    _running = false;
                    EditorApplication.update -= Pump;
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    Debug.Log(string.Format("[XaiIconGen] 완료 — 성공 {0}, 실패 {1} → {2}", _done, _fail, OutDir));
                    return;
                }
                _cur = _queue.Dequeue();
                Send(_cur);
                return;
            }

            EditorUtility.DisplayProgressBar("Grok 아이콘 생성",
                string.Format("{0}/{1}  {2}", _done + _fail + 1, _total, _cur != null ? _cur.name : ""),
                (float)(_done + _fail) / Mathf.Max(1, _total));

            if (!_req.isDone) return;

            try
            {
                if (_req.result != UnityWebRequest.Result.Success)
                {
                    string b = _req.downloadHandler != null ? _req.downloadHandler.text : "";
                    Debug.LogWarning("[XaiIconGen] " + _cur.id + " 실패: " + _req.error + " " +
                                     (b.Length > 200 ? b.Substring(0, 200) : b));
                    _fail++;
                }
                else
                {
                    string err = null;
                    byte[] png;
                    if (UseLocal)
                    {
                        // 로컬 서버는 raw PNG를 준다 — JSON 파싱 불필요
                        png = _req.downloadHandler.data;
                        if (png == null || png.Length < 8 || png[0] != 0x89 || png[1] != (byte)'P')
                        {
                            err = "로컬 서버 응답이 PNG가 아님: " + (_req.downloadHandler.text ?? "").Substring(0, Mathf.Min(160, (_req.downloadHandler.text ?? "").Length));
                            png = null;
                        }
                    }
                    else
                        png = ExtractImage(_req.downloadHandler.text, out err);
                    if (png == null) { Debug.LogWarning("[XaiIconGen] " + _cur.id + " " + err); _fail++; }
                    else
                    {
                        // 배경(biome)은 화면 전체 그림이라 크로마키가 없다 — 지우려 들면 안 된다
                        byte[] cut = _cur.kind == "biome" ? png : (StripChromaKey(png) ?? png);
                        string outRoot = _cur.kind == "wear" ? WearDir
                                       : _cur.kind == "armor" ? ArmorDir
                                       : _cur.kind == "skill" ? SkillDir
                                       : _cur.kind == "artifact" ? ArtifactDir
                                       : _cur.kind == "biome" ? BiomeDir
                                       : _cur.kind == "boss" ? BossDir : OutDir;
                        // 시트면 물체 덩어리별로 잘라 targets 이름으로 저장한다
                        if (_cur.targets != null && _cur.targets.Length > 1)
                        {
                            Directory.CreateDirectory(outRoot);
                            // 분할이 확실할 때만 저장한다. 물체가 겹쳐 한 덩어리로 잡히면
                            // 시트 전체가 첫 파일에 덮어써져 기존 아트를 망친다(실제로 겪음).
                            string why;
                            int saved = SplitSheet(cut, _cur.targets, outRoot, out why);
                            if (saved == _cur.targets.Length) _done++;
                            else
                            {
                                var raw = Path.Combine(outRoot, "_raw_" + _cur.id + ".png");
                                File.WriteAllBytes(raw, cut);
                                Debug.LogWarning("[XaiIconGen] " + _cur.id + " 분할 실패 (" + why +
                                    ") — 저장하지 않고 원본만 남김: " + raw);
                                _fail++;
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(outRoot);
                            File.WriteAllBytes(Path.Combine(outRoot, _cur.id + ".png"), cut);
                            _done++;
                        }
                    }
                }
            }
            finally
            {
                _req.Dispose(); _req = null;
            }
        }

        /// <summary>로컬 Z-Image-Turbo 서버 (tools/imggen/server.py). Grok 대체 —
        /// 비용 0. IMGGEN_USE_XAI=1 환경변수로만 Grok로 되돌린다.</summary>
        const string LocalUrl = "http://127.0.0.1:8009/generate";
        static bool UseLocal => Environment.GetEnvironmentVariable("IMGGEN_USE_XAI") != "1";

        static void Send(PromptItem it)
        {
            if (UseLocal)
            {
                int w = it.width > 0 ? it.width : 1280;
                int h = it.height > 0 ? it.height : 720;
                string lbody = "{\"prompt\":" + Escape(it.prompt) + ",\"width\":" + w + ",\"height\":" + h + ",\"steps\":9}";
                _req = new UnityWebRequest(LocalUrl, "POST");
                _req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(lbody));
                _req.downloadHandler = new DownloadHandlerBuffer();
                _req.SetRequestHeader("Content-Type", "application/json");
                _req.timeout = 300;
                _req.SendWebRequest();
                return;
            }
            string key = Environment.GetEnvironmentVariable("XAI_API_KEY");
            string body = "{\"model\":\"" + Model + "\",\"prompt\":" + Escape(it.prompt) + "}";
            _req = new UnityWebRequest(Endpoint, "POST");
            _req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            _req.downloadHandler = new DownloadHandlerBuffer();
            _req.SetRequestHeader("Content-Type", "application/json");
            _req.SetRequestHeader("Authorization", "Bearer " + key);
            _req.timeout = 180;
            _req.SendWebRequest();
        }

        /// <summary>
        /// 알파가 적용된 시트에서 물체 덩어리를 찾아 왼→오 순으로 잘라 저장한다.
        /// 격자 위치를 가정하지 않으므로 모델이 간격을 안 맞춰도 어긋나지 않는다.
        /// 잘라낸 뒤 정사각 캔버스 중앙에 놓아 아이콘 비율을 통일한다.
        /// </summary>
        /// <summary>
        /// 시트를 names.Length 조각으로 자른다. 다 자를 수 있을 때만 파일을 쓴다.
        ///
        /// '빈 열 찾기'로 덩어리를 세던 방식은 실패한다. 실측한 시트는 물체 사이 여백이
        /// 6px뿐인데 장식 여백을 흡수하려고 둔 gapAllow(=w/200=6)와 같아서, 멀쩡히
        /// 떨어져 있는 옷 5벌이 한 덩어리로 잡혔다. 광휘가 있는 티어는 틈을 아예 메운다.
        /// → 개수를 알고 있으니 등분 위치 근처에서 **가장 잉크가 적은 열**을 골라 자른다.
        /// </summary>
        static int SplitSheet(byte[] png, string[] names, string outRoot, out string why)
        {
            why = "";
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!src.LoadImage(png)) { why = "PNG 디코드 실패"; return 0; }
            int w = src.width, h = src.height;
            var px = src.GetPixels32();
            UnityEngine.Object.DestroyImmediate(src);

            int n = names.Length;
            var ink = new int[w];                          // 열마다 불투명 픽셀 수
            for (int x = 0; x < w; x++)
            {
                int c = 0;
                for (int y = 0; y < h; y++) if (px[y * w + x].a > 32) c++;
                ink[x] = c;
            }

            int x0 = 0; while (x0 < w && ink[x0] == 0) x0++;
            int x1 = w - 1; while (x1 > x0 && ink[x1] == 0) x1--;
            if (x1 - x0 + 1 < n * 8) { why = "내용이 너무 좁다"; return 0; }

            float avg = 0f;
            for (int x = x0; x <= x1; x++) avg += ink[x];
            avg /= Mathf.Max(1, x1 - x0 + 1);

            float slice = (x1 - x0 + 1) / (float)n;
            var cuts = new int[n + 1];
            cuts[0] = x0; cuts[n] = x1 + 1;
            for (int i = 1; i < n; i++)
            {
                int ideal = x0 + Mathf.RoundToInt(slice * i);
                int win = Mathf.Max(4, Mathf.RoundToInt(slice * 0.35f));
                int lo = Mathf.Max(x0 + 1, ideal - win), hi = Mathf.Min(x1 - 1, ideal + win);
                int best = ideal, bestInk = int.MaxValue;
                for (int x = lo; x <= hi; x++) if (ink[x] < bestInk) { bestInk = ink[x]; best = x; }
                // 골짜기가 얕으면 물체가 실제로 겹친 것 — 반쪽짜리를 저장하느니 그만둔다
                if (bestInk > avg * 0.35f)
                {
                    why = "물체가 겹침 (" + i + "번째 경계 잉크 " + bestInk + " / 평균 " + Mathf.RoundToInt(avg) + ")";
                    return 0;
                }
                cuts[i] = best;
            }

            var boxes = new List<Vector4>();             // x0,y0,x1,y1
            for (int i = 0; i < n; i++)
            {
                int sx = cuts[i], ex = cuts[i + 1] - 1;
                // 구간 안에서 실제 잉크가 있는 x·y 범위로 다시 타이트하게 조인다
                while (sx <= ex && ink[sx] == 0) sx++;
                while (ex >= sx && ink[ex] == 0) ex--;
                if (ex < sx) { why = (i + 1) + "번째 칸이 비었다"; return 0; }
                int y0 = h, y1 = -1;
                for (int x = sx; x <= ex; x++)
                    for (int y = 0; y < h; y++)
                        if (px[y * w + x].a > 32) { if (y < y0) y0 = y; if (y > y1) y1 = y; }
                if (y1 < y0) { why = (i + 1) + "번째 칸이 비었다"; return 0; }
                boxes.Add(new Vector4(sx, y0, ex, y1));
            }

            int saved = 0;
            for (int b = 0; b < boxes.Count && b < names.Length; b++)
            {
                int bx0 = (int)boxes[b].x, by0 = (int)boxes[b].y;
                int bx1 = (int)boxes[b].z, by1 = (int)boxes[b].w;
                int cw = bx1 - bx0 + 1, ch = by1 - by0 + 1;
                int side = Mathf.Max(cw, ch) + 16;       // 여백
                var outPx = new Color32[side * side];
                int ox = (side - cw) / 2, oy = (side - ch) / 2;
                for (int y = 0; y < ch; y++)
                    for (int x = 0; x < cw; x++)
                        outPx[(oy + y) * side + (ox + x)] = px[(by0 + y) * w + (bx0 + x)];
                var t = new Texture2D(side, side, TextureFormat.RGBA32, false);
                t.SetPixels32(outPx); t.Apply();
                File.WriteAllBytes(Path.Combine(outRoot, names[b] + ".png"), t.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(t);
                saved++;
            }
            return saved;
        }

        /// <summary>
        /// 크로마키(마젠타) 배경을 알파로 바꾼다.
        /// 가장자리에서 연결된 배경만 지워, 물체 안쪽의 같은 색은 살린다.
        /// 배경이 마젠타가 아니면(모델이 무시했으면) null을 돌려 원본을 그대로 둔다.
        /// </summary>
        static byte[] StripChromaKey(byte[] png)
        {
            // 원본 PNG에 알파가 없으면 LoadImage가 RGB24 텍스처를 만들고,
            // 거기에 a=0을 넣어도 EncodeToPNG에서 알파가 사라진다.
            // → 픽셀만 가져와 RGBA32 텍스처에 새로 담는다.
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!src.LoadImage(png)) return null;
            int w = src.width, h = src.height;
            var px = src.GetPixels32();
            UnityEngine.Object.DestroyImmediate(src);

            // 네 모서리 평균으로 배경색 추정
            Func<int, int, Color32> at = (x, y) => px[y * w + x];
            var c0 = at(2, 2); var c1 = at(w - 3, 2); var c2 = at(2, h - 3); var c3 = at(w - 3, h - 3);
            int br = (c0.r + c1.r + c2.r + c3.r) / 4;
            int bg = (c0.g + c1.g + c2.g + c3.g) / 4;
            int bb = (c0.b + c1.b + c2.b + c3.b) / 4;

            // 마젠타 계열인지 확인 (R,B 높고 G 낮음). 아니면 손대지 않는다.
            bool magenta = br > 120 && bb > 120 && bg < 110 && (br - bg) > 50 && (bb - bg) > 50;
            if (!magenta) return null;

            const int tol = 60;
            var isBg = new bool[w * h];
            var stack = new Stack<int>();
            Action<int> push = idx =>
            {
                if (idx < 0 || idx >= px.Length || isBg[idx]) return;
                var c = px[idx];
                if (Mathf.Abs(c.r - br) > tol || Mathf.Abs(c.g - bg) > tol || Mathf.Abs(c.b - bb) > tol) return;
                isBg[idx] = true; stack.Push(idx);
            };
            for (int x = 0; x < w; x++) { push(x); push((h - 1) * w + x); }
            for (int y = 0; y < h; y++) { push(y * w); push(y * w + w - 1); }
            while (stack.Count > 0)
            {
                int i = stack.Pop(); int x = i % w, y = i / w;
                if (x > 0) push(i - 1);
                if (x < w - 1) push(i + 1);
                if (y > 0) push(i - w);
                if (y < h - 1) push(i + w);
            }

            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                if (isBg[i]) { c.a = 0; px[i] = c; continue; }
                // 배경색이 번진 경계는 알파를 낮추고 마젠타 물을 뺀다(보라 테두리 방지)
                int d = Mathf.Max(Mathf.Abs(c.r - br), Mathf.Max(Mathf.Abs(c.g - bg), Mathf.Abs(c.b - bb)));
                if (d < tol * 1.6f)
                {
                    float f = Mathf.Clamp01((d - tol * 0.5f) / (tol * 1.1f));
                    c.a = (byte)(f * 255f);
                    c.r = (byte)Mathf.Min(c.r, c.g * 1.25f + 40f);
                    c.b = (byte)Mathf.Min(c.b, c.g * 1.25f + 40f);
                    px[i] = c;
                }
            }
            var dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
            dst.SetPixels32(px); dst.Apply();
            var bytes = dst.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(dst);
            return bytes;
        }

        static byte[] ExtractImage(string json, out string error)
        {
            error = null;
            string b64 = Between(json, "\"b64_json\":\"", "\"");
            if (!string.IsNullOrEmpty(b64))
            {
                try { return Convert.FromBase64String(b64); }
                catch (Exception e) { error = "b64 디코드 실패: " + e.Message; return null; }
            }
            string url = Between(json, "\"url\":\"", "\"");
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("\\/", "/");
                using (var dl = UnityWebRequest.Get(url))
                {
                    dl.timeout = 180;
                    var op = dl.SendWebRequest();
                    while (!op.isDone) System.Threading.Thread.Sleep(30);   // 다운로드만 짧게 대기
                    if (dl.result != UnityWebRequest.Result.Success) { error = "다운로드 실패: " + dl.error; return null; }
                    return dl.downloadHandler.data;
                }
            }
            error = "응답에 b64_json/url 없음: " + (json.Length > 200 ? json.Substring(0, 200) : json);
            return null;
        }

        static string Between(string s, string a, string b)
        {
            int i = s.IndexOf(a, StringComparison.Ordinal);
            if (i < 0) return null;
            i += a.Length;
            int j = s.IndexOf(b, i, StringComparison.Ordinal);
            return j < 0 ? null : s.Substring(i, j - i);
        }

        static string Escape(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in s ?? "")
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c == '\n') sb.Append("\\n");
                else if (c == '\r') sb.Append("\\r");
                else if (c < 32) sb.Append(' ');
                else sb.Append(c);
            }
            return sb.Append('"').ToString();
        }
    }
}
