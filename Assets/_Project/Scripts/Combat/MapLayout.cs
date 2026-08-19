using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// 테마별 발판 배치표.
    ///
    /// 좌표는 맵 절반폭에 대한 비율(-1..1)이라 해상도나 맵 폭이 바뀌어도 그대로 쓴다.
    /// 참고 자료(횡스크롤 방치형 맵 5종)에서 뽑은 배치 원형 5가지:
    ///   0 산개형   — 넓은 지면 위에 크기가 제각각인 발판이 좌우로 어긋나게 흩어진다
    ///   1 양끝절벽 — 좌우에 높은 대지, 가운데는 낮은 다리 하나로만 이어진다
    ///   2 규칙층   — 폭이 거의 같은 얇은 선반이 층마다 반복된다 (수직 구조물 내부)
    ///   3 계단유적 — 위로 갈수록 좁아지는 대칭 계단
    ///   4 2단대공간 — 천장과 지면 두 덩어리, 사이에는 작은 징검다리만
    ///
    /// 층에 발판이 없으면 그 층엔 몹도 안 나온다 — 테마마다 실제로 쓰는 층 수가 달라진다.
    /// </summary>
    public static class MapLayout
    {
        /// <summary>[테마][층] = {중심, 절반폭} 쌍의 나열. 빈 배열이면 그 층엔 발판이 없다.</summary>
        static readonly float[][][] Data =
        {
            // 0 죽림·마을 (Ch1~4) — 산개형
            new[]
            {
                new[] { 0f, 1f },
                new[] { -0.62f, 0.20f,  -0.06f, 0.13f,  0.56f, 0.24f },
                new[] { -0.34f, 0.15f,   0.32f, 0.19f },
                new[] {  0.03f, 0.11f },
            },
            // 1 설산 관문 (Ch5~8) — 양끝절벽
            new[]
            {
                new[] { 0f, 1f },
                new[] { -0.76f, 0.21f,   0.00f, 0.28f,  0.76f, 0.21f },
                // 가운데 두 개는 다리 위로 솟은 돌기둥 꼭대기 — 중앙이 허전하지 않게
                new[] { -0.85f, 0.14f,  -0.23f, 0.07f,  0.25f, 0.07f,  0.85f, 0.14f },
                new[] { -0.88f, 0.09f,   0.88f, 0.10f },
            },
            // 2 석굴 (Ch9~12) — 규칙층
            new[]
            {
                new[] { 0f, 1f },
                new[] { -0.06f, 0.86f },
                new[] {  0.06f, 0.86f },
                new[] {  0.00f, 0.78f },
            },
            // 3 황야 유적 (Ch13~16) — 계단유적
            new[]
            {
                new[] { 0f, 1f },
                new[] { 0f, 0.68f },
                new[] { 0f, 0.44f },
                new[] { 0f, 0.21f },
            },
            // 4 마교 궁전 (Ch17~20) — 2단대공간
            new[]
            {
                new[] { 0f, 1f },
                new[] { -0.44f, 0.09f,   0.44f, 0.09f },
                new float[0],
                new[] { 0f, 0.93f },
            },
        };

        public static readonly string[] ThemeNames = { "죽림", "설산", "석굴", "유적", "마교" };

        /// <summary>발판 두께(필드 px). 얇은 선반과 두꺼운 대지가 테마를 구분한다.</summary>
        static readonly float[] ThickPx = { 36f, 52f, 26f, 46f, 58f };

        /// <summary>발판 색조. 전용 아트가 나오기 전까지 잔도 아트를 이 색으로 물들여 쓴다.</summary>
        static readonly Color[] Tints =
        {
            new Color(1.00f, 1.00f, 1.00f),   // 죽림 — 원색(흙·풀)
            new Color(0.82f, 0.88f, 1.00f),   // 설산 — 푸른 한기
            new Color(0.70f, 0.76f, 0.72f),   // 석굴 — 눅눅한 회녹
            new Color(1.00f, 0.92f, 0.72f),   // 유적 — 마른 사암
            new Color(0.66f, 0.56f, 0.70f),   // 마교 — 흑자색
        };

        public static int ThemeCount => Data.Length;
        public static int ThemeOf(int chapter) => Mathf.Clamp((chapter - 1) / 4, 0, Data.Length - 1);
        public static float Thickness(int theme) => ThickPx[Mathf.Clamp(theme, 0, ThickPx.Length - 1)];
        public static Color Tint(int theme) => Tints[Mathf.Clamp(theme, 0, Tints.Length - 1)];

        static float[] Row(int theme, int floor)
        {
            var t = Data[Mathf.Clamp(theme, 0, Data.Length - 1)];
            return floor >= 0 && floor < t.Length ? t[floor] : new float[0];
        }

        /// <summary>이 층에 놓인 발판 개수. 0이면 층 자체가 없는 테마다.</summary>
        public static int Count(int theme, int floor) => Row(theme, floor).Length / 2;

        /// <summary>
        /// 같은 테마 안에서도 챕터가 깊어질수록 발판이 조금씩 밀리고 좁아진다 —
        /// "설산 → 깊은 설산" 같은 심화 변형을 아트 없이 배치만으로 낸다.
        /// 시드가 아니라 식이라 매 프레임 같은 값이 나온다.
        /// </summary>
        public static void Bounds(int theme, int chapter, int floor, int index, float mapHalfW,
            out float minX, out float maxX)
        {
            var row = Row(theme, floor);
            int i = Mathf.Clamp(index, 0, Mathf.Max(0, row.Length / 2 - 1)) * 2;
            if (row.Length < 2) { minX = -mapHalfW; maxX = mapHalfW; return; }

            float depth = ((chapter - 1) % 4) * 0.25f;             // 테마 내 진행도 0~0.75
            float c = row[i] + Mathf.Sin((i + 1) * 2.3f + depth * 6.283f) * 0.05f;
            float hw = row[i + 1] * (1f - depth * 0.14f);          // 심처일수록 발 디딜 곳이 좁다
            if (row[i + 1] >= 0.999f) { c = 0f; hw = 1f; }         // 지면은 항상 맵 전체

            minX = (c - hw) * mapHalfW;
            maxX = (c + hw) * mapHalfW;
        }

        /// <summary>
        /// x 지점에 발 디딜 발판이 있으면 그 발판의 좌우 끝을 준다.
        /// 몹은 이 범위 밖으로 못 나가고, 히어로는 범위를 벗어나면 아래로 떨어진다.
        /// </summary>
        public static bool BoundsAt(int theme, int chapter, int floor, float x, float mapHalfW,
            out float minX, out float maxX)
        {
            int n = Count(theme, floor);
            for (int k = 0; k < n; k++)
            {
                Bounds(theme, chapter, floor, k, mapHalfW, out minX, out maxX);
                if (x >= minX && x <= maxX) return true;
            }
            minX = -mapHalfW; maxX = mapHalfW;
            return false;
        }

        /// <summary>발판이 하나라도 있는 층 중에서 slot 번째를 고른다 (없는 층은 건너뛴다).</summary>
        public static int PickFloor(int theme, int slot)
        {
            int floors = Data[Mathf.Clamp(theme, 0, Data.Length - 1)].Length;
            int tries = floors;
            int f = ((slot % floors) + floors) % floors;
            while (tries-- > 0 && Count(theme, f) == 0) f = (f + 1) % floors;
            return Count(theme, f) > 0 ? f : 0;
        }
    }
}
