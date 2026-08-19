using System.Collections.Generic;
using UnityEngine;

namespace IdleMvp.UI
{
    /// <summary>
    /// 동료 아트의 단일 진실 공급원.
    ///
    /// 예전엔 필드에 소환된 동료는 하드코딩 프리셋("Bandit Cutthroat") 리그를 쓰고,
    /// 동료창 카드는 전혀 다른 TplArt 수채화 일러스트를 썼다. 그래서 뽑은 동료와
    /// 화면에 나오는 동료가 서로 딴판이었다. 이제 양쪽 다 여기를 거친다.
    ///
    /// 초상화는 프리셋 리그를 한 번 렌더링해 Sprite로 캐시한다(프리셋 9종뿐이라 비용 고정).
    /// </summary>
    public static class CompanionArt
    {
        /// <summary>Resources/CharPresets 에 실제로 존재하는 리그들. 약→강 순.</summary>
        static readonly string[] Presets =
        {
            "Peasant", "Goblin", "Bandit Bowman", "Bandit Cutthroat",
            "Raider", "Warrior", "Orc Warrior", "Orc Brute", "Berserker",
        };

        const int PortraitW = 160;
        const int PortraitH = 200;

        static readonly Dictionary<string, Sprite> _portraits = new Dictionary<string, Sprite>(16);

        static int StableHash(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int h = 17;
            for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
            return h & 0x7fffffff;
        }

        /// <summary>
        /// 동료 → 프리셋. 이름이 같으면 항상 같은 프리셋이 나온다(뽑을 때마다 바뀌면 안 된다).
        /// 등급이 높을수록 위압감 있는 리그 쪽으로 치우친다.
        /// </summary>
        /// <summary>
        /// 동료 이름 → 프리셋 고정 배정. 해시 + (idx/2 + rarity) 방식은 분산이 뭉개져서
        /// 6명 중 4명이 똑같이 "Raider"로 나왔다(이름만 다르고 그림은 같은 상태).
        /// 프리셋이 9종뿐이라 16동료를 전부 다르게는 못 만들지만, 같은 프리셋이라도
        /// TintFor()가 동료별 고유 색을 입혀 구분된다.
        /// </summary>
        static readonly Dictionary<string, string> PresetByName = new Dictionary<string, string>
        {
            { "무사 동료",     "Warrior"          },
            { "도사 동료",     "Peasant"          },
            { "궁사 동료",     "Bandit Bowman"    },
            { "살수 동료",     "Bandit Cutthroat" },
            { "호법 동료",     "Orc Warrior"      },
            { "혈귀 동료",     "Berserker"        },
            { "화공술사 동료", "Raider"           },
            { "빙공술사 동료", "Goblin"           },
            { "약사 동료",     "Peasant"          },
            { "포쾌 동료",     "Bandit Bowman"    },
            { "광전사 동료",   "Orc Brute"        },
            { "무당 동료",     "Goblin"           },
            { "자객 동료",     "Bandit Cutthroat" },
            { "수문장 동료",   "Orc Warrior"      },
            { "단약사 동료",   "Peasant"          },
            { "용린족 동료",   "Berserker"        },
        };

        public static string PresetFor(string companionName, int rarity)
        {
            string hit;
            if (!string.IsNullOrEmpty(companionName) && PresetByName.TryGetValue(companionName, out hit))
                return hit;
            // 표에 없는 이름(구버전 세이브 등)만 해시로 흩는다
            return Presets[StableHash(companionName) % Presets.Length];
        }

        /// <summary>
        /// 동료별 고유 색. 프리셋이 겹쳐도 옷 색이 달라 한눈에 구분된다.
        /// 이름 해시를 색상환에 고르게 뿌린다.
        /// </summary>
        public static Color TintFor(string companionName)
        {
            if (string.IsNullOrEmpty(companionName)) return Color.white;
            int h = StableHash(companionName);
            float hue = (h % 360) / 360f;
            return Color.HSVToRGB(hue, 0.55f, 0.95f);
        }

        /// <summary>프리셋 리그를 렌더링한 초상화. 프리셋+색 조합당 1회만 렌더링하고 캐시한다.</summary>
        public static Sprite Portrait(string preset)
        {
            return Portrait(preset, Color.white);
        }

        public static Sprite Portrait(string preset, Color tint)
        {
            if (string.IsNullOrEmpty(preset)) return null;
            // 같은 프리셋이라도 색·복장 티어가 다르면 다른 초상화다
            string key = preset + "#" + ColorUtility.ToHtmlStringRGB(tint) + "#" + _portraitRarity;
            Sprite cached;
            if (_portraits.TryGetValue(key, out cached)) return cached;

            var made = Render(preset, tint);
            _portraits[key] = made;   // 실패(null)도 캐시해 매번 재시도하지 않는다
            return made;
        }

        public static Sprite PortraitFor(string companionName, int rarity)
        {
            // 등급이 복장 티어를 정하므로 캐시 키에 포함돼야 한다 (Portrait key에 색+등급)
            _portraitRarity = rarity;
            return Portrait(PresetFor(companionName, rarity), TintFor(companionName));
        }

        static int _portraitRarity;

        /// <summary>
        /// 옷/갑옷 계열 파츠만 동료 고유색으로 물들인다.
        /// 프리셋이 9종뿐이라 색이 없으면 이름만 다르고 그림은 같아 보인다.
        /// 피부·눈은 건드리지 않는다(사람이 아닌 색이 되어 버린다).
        /// </summary>
        static Sprite Render(string preset, Color tint)
        {
            var prefab = Resources.Load<GameObject>("CharPresets/" + preset);
            if (prefab == null) return null;

            // 화면 밖 먼 곳에서 렌더링한다 (실제 전투 필드와 겹치지 않게)
            var basePos = new Vector3(5000f, 5000f, 0f);
            GameObject rig = null;
            GameObject camGo = null;
            RenderTexture rt = null;
            var prevActive = RenderTexture.active;

            try
            {
                rig = Object.Instantiate(prefab);
                rig.name = "PortraitRig_" + preset;
                rig.transform.position = basePos;
                // 동료 고유색은 갑옷·투구 렌더러에만 (피부·눈은 사람 색 유지)
                var ch = rig.GetComponentInChildren<Assets.HeroEditor.Common.Scripts.CharacterScripts.Character>();
                if (ch != null && tint != Color.white)
                {
                    foreach (var r in ch.ArmorRenderers) if (r != null) r.color = tint;
                    if (ch.HelmetRenderer != null) ch.HelmetRenderer.color = tint;
                }

                rt = new RenderTexture(PortraitW, PortraitH, 16);
                camGo = new GameObject("PortraitCam_" + preset);
                camGo.transform.position = basePos + new Vector3(0f, 0.85f, -10f);
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 1.35f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                cam.cullingMask = ~0;
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(PortraitW, PortraitH, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, PortraitW, PortraitH), 0, 0);
                tex.Apply();
                tex.name = "Portrait_" + preset;

                return Sprite.Create(tex, new Rect(0, 0, PortraitW, PortraitH),
                    new Vector2(0.5f, 0.5f), 100f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[CompanionArt] 초상화 렌더 실패 " + preset + ": " + e.Message);
                return null;
            }
            finally
            {
                RenderTexture.active = prevActive;
                // ⚠️ Destroy는 프레임 끝 처리다. 같은 프레임에 여러 초상화를 렌더링하면
                // 이전 리그가 같은 자리에 아직 살아 있어 전부 겹쳐 찍힌다(실제로 동료창
                // 카드가 뒤로 갈수록 분홍 덩어리로 뭉개졌다). 즉시 파괴로 바꾼다.
                if (rig != null) Object.DestroyImmediate(rig);
                if (camGo != null) Object.DestroyImmediate(camGo);
                if (rt != null) { rt.Release(); Object.DestroyImmediate(rt); }
            }
        }
    }
}
