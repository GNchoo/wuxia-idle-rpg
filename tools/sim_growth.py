"""성장 곡선 시뮬레이터.

스테이지 표(stage-table.json)와 레벨 표(level-xp.json)를 읽어
 - 구간별 레벨업에 필요한 킬 수
 - 스테이지 진행 대비 권장 전투력 성장률
 - 레벨 상한까지의 누적 경험치
를 뽑아 밸런스 계수를 확정하는 데 쓴다.

사용: python tools/sim_growth.py [스테이지표] [레벨표]
"""
import json
import sys

RES = "Assets/_Project/Resources/"


def load(stage_path, level_path):
    stages = json.load(open(stage_path, encoding="utf-8"))["stages"]
    levels = json.load(open(level_path, encoding="utf-8"))
    need = {e["level"]: e["requiredXP"] for e in levels}
    return stages, need


def need_at(need, lv):
    """표 밖은 마지막 값에서 같은 비율로 이어 붙인다 (LevelXpTable과 동일 규칙)."""
    if lv in need:
        return need[lv]
    top = max(need)
    return round(need[top] * 1.12 ** max(1, lv - top))


def stage_for_level(stages, need, lv):
    """설계 계약: 권장 전투력 게이트 때문에 레벨과 스테이지는 나란히 간다고 본다.

    (예전 모델은 '모든 스테이지를 한 번씩 클리어한 누적 경험치'로 역산했는데,
     실제로는 최고 스테이지를 반복 사냥하므로 레벨을 과대평가했다.)
    """
    return min(lv, stages[-1]["index"])


def report(stages, need, label):
    print("=" * 72)
    print(label)
    print("=" * 72)
    first, last = stages[0], stages[-1]
    n = len(stages)
    print("스테이지 %d개 · 챕터 %d개" % (n, max(s["chapter"] for s in stages)))
    print("적 HP     %8.0f → %12.0f  (%.1f배, 스테이지당 %.4f)"
          % (first["enemyHp"], last["enemyHp"],
             last["enemyHp"] / first["enemyHp"],
             (last["enemyHp"] / first["enemyHp"]) ** (1 / (n - 1))))
    print("킬당 XP   %8.1f → %12.1f  (%.1f배, 스테이지당 %.4f)"
          % (first["xpPerKill"], last["xpPerKill"],
             last["xpPerKill"] / first["xpPerKill"],
             (last["xpPerKill"] / first["xpPerKill"]) ** (1 / (n - 1))))
    top = max(need)
    print("레벨 표    1~%d · Lv%d 필요 XP %s" % (top, top, format(need[top], ",")))
    print()
    print(" 레벨 | 도달 스테이지 |   필요 XP    | 킬당 XP |  필요 킬 수")
    print("-" * 66)
    worst = 0
    for lv in (2, 10, 20, 30, 40, 50, 60, 80, 100, 120, 150, 180, 200):
        if lv > top + 40:
            break
        idx = stage_for_level(stages, need, lv)
        s = stages[idx - 1]
        kills = need_at(need, lv) / s["xpPerKill"]
        worst = max(worst, kills)
        print("%5d | %13d | %12s | %7.1f | %11.0f"
              % (lv, idx, format(need_at(need, lv), ","), s["xpPerKill"], kills))
    print("-" * 66)
    print("최대 필요 킬 수: %.0f  (목표 200~400)" % worst)
    print()


if __name__ == "__main__":
    sp = sys.argv[1] if len(sys.argv) > 1 else RES + "stage-table.json"
    lp = sys.argv[2] if len(sys.argv) > 2 else RES + "level-xp.json"
    st, nd = load(sp, lp)
    report(st, nd, "%s + %s" % (sp.split("/")[-1], lp.split("/")[-1]))
