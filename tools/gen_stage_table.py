"""스테이지 표 생성 (200 스테이지 = 20챕터 × 10).

핵심 수정: **킬당 경험치를 적 성장률과 같은 비율로 올린다.**
예전 표는 적 HP가 3,259배 오르는 동안 킬당 경험치가 20.8배만 올라
후반에 레벨업당 13,000킬 이상이 필요했다.

챕터 10번째 스테이지는 보스(타임어택), 나머지는 잡몹 웨이브 + 미니보스.
"""
import json
import os

OUT = "Assets/_Project/Resources/stage-table.json"

STAGES = 200
PER_CHAPTER = 10

GROW = 1.055        # 스테이지당 성장률 (적 HP·공격력·권장 전투력·경험치 공통)
HP0 = 200.0
ATK0 = 8.0
CP0 = 100.0
XP0 = 1.0           # 킬당 경험치 — 레벨 표 BASE(300)와 짝: 레벨업 ≈ 300킬
GOLD0 = 25.0

CLEAR_XP_MUL = 8.0  # 스테이지 클리어 보너스 = 킬당 경험치 × 8
BOSS_HP_MUL = 1.55  # balance-seed.json bossMultiplier와 동일
MIN_CP_RATIO = 0.85
SOFT_CP_RATIO = 1.15

# mapTier(Normal/Hard/Hell)를 실제 난이도로: 적 수치 배수 + 그보다 큰 보상 배수(도전 이유).
TIER_STAT_MUL = [1.0, 1.15, 1.35]
TIER_REWARD_MUL = [1.0, 1.25, 1.5]

# 적 방어력: 감쇠율이 챕터1 20% → 챕터20 40%가 되도록 역산해서 넣는다.
# 감쇠율 m 일 때 def = K * m/(1-m), K = 권장 전투력 × DEF_K.
DEF_K = 0.6
MITIGATION_MIN = 0.20
MITIGATION_MAX = 0.40


def sig(v, digits=4):
    """유효숫자 정리 — 표가 사람 눈에 읽히게."""
    if v < 10:
        return round(v, 2)
    n = len(str(int(v))) - digits
    step = 10 ** max(0, n)
    return round(v / step) * step


def main():
    rows = []
    for i in range(1, STAGES + 1):
        g = GROW ** (i - 1)
        chapter = (i - 1) // PER_CHAPTER + 1
        stage = (i - 1) % PER_CHAPTER + 1
        boss = stage == PER_CHAPTER

        tier = 0 if chapter <= 7 else (1 if chapter <= 14 else 2)
        tsm = TIER_STAT_MUL[tier]
        trm = TIER_REWARD_MUL[tier]
        hp = HP0 * g * (BOSS_HP_MUL if boss else 1.0) * tsm
        atk = ATK0 * g * tsm
        cp = CP0 * g * tsm
        t = (chapter - 1) / max(1, (STAGES // PER_CHAPTER) - 1)
        # 스폰 규칙은 '사냥 모드'에만 쓰인다. 보스 스테이지도 반복 사냥터라 물량을 준다
        # (보스 전투 자체는 돌파 모드가 따로 스폰한다).
        # 넓은 맵(화면의 3배)을 채우려면 물량이 필요하다 — '쓸어버리는 맛'
        # 맵이 화면의 3배라 화면당 밀도 = spawn_n / 3. 6~11마리가 보이도록 잡는다.
        spawn_n = int(round(18 + 16 * t))                         # 18 → 34마리
        spawn_d = round(0.7 - 0.4 * t, 2)                         # 0.7 → 0.3초
        m = MITIGATION_MIN + (MITIGATION_MAX - MITIGATION_MIN) * t
        edef = cp * DEF_K * (m / (1.0 - m))
        xpk = XP0 * g * trm
        rows.append({
            "index": i,
            "chapter": chapter,
            "stage": stage,
            "boss": boss,
            "enemyHp": sig(hp),
            "enemyAtk": sig(atk),
            "enemyDef": sig(edef),
            "recommendedCp": sig(cp),
            "clearGold": int(sig(GOLD0 * g * trm)),
            "clearXp": int(max(1, sig(xpk * CLEAR_XP_MUL))),
            "mapTier": tier,
            "minCp": sig(cp * MIN_CP_RATIO),
            "softCp": sig(cp * SOFT_CP_RATIO),
            "xpPerKill": sig(xpk),
            "mobHpMul": 1.0,
            "spawnCount": spawn_n,
            "spawnDelay": spawn_d,
            "mobPreset": ((stage - 1) // 3) % 3,
            "bossTimeLimit": 60.0,
        })

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump({"stages": rows}, f, ensure_ascii=False, indent=1)

    print("wrote", OUT, len(rows), "stages /", rows[-1]["chapter"], "chapters")
    for i in (1, 10, 50, 100, 150, 200):
        r = rows[i - 1]
        mit = r["enemyDef"] / (r["enemyDef"] + r["recommendedCp"] * DEF_K)
        print("  %3d (%2d-%2d)%s hp=%12s cp=%12s def=%10s 감쇠 %4.1f%% xp/kill=%9s"
              % (i, r["chapter"], r["stage"], "B" if r["boss"] else " ",
                 format(int(r["enemyHp"]), ","), format(int(r["recommendedCp"]), ","),
                 format(int(r["enemyDef"]), ","), mit * 100,
                 format(int(r["xpPerKill"]), ",")))


if __name__ == "__main__":
    main()
