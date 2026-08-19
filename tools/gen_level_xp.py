"""레벨 경험치 표 생성 (1~200).

설계 의도
 - 1~100: 킬당 경험치와 **같은 비율**로 오른다 → 레벨업에 필요한 킬 수가 일정하게 유지된다.
 - 101~200: 경험치 요구만 더 가파르게 오른다 → 레벨업이 점점 느려지고,
   성장의 주도권이 **경지·환생·장비**로 넘어간다. (유저 확정 방향)

필요 경험치는 PlayerGrowth.CurrentXp가 int라 21억을 넘으면 안 된다.
아래 계수는 Lv200에서 약 5.8천만으로, 여유 있게 들어온다.
"""
import json
import os

OUT = "Assets/_Project/Resources/level-xp.json"

MAX_LEVEL = 200
BASE = 300          # Lv1 → Lv2 필요 경험치
GROW_EARLY = 1.055  # 1~100 (스테이지 성장률과 동일)
GROW_LATE = 1.070   # 101~200 (레벨업이 점점 느려진다)
PIVOT = 100


def required_xp(level):
    if level <= PIVOT:
        return BASE * GROW_EARLY ** (level - 1)
    at_pivot = BASE * GROW_EARLY ** (PIVOT - 1)
    return at_pivot * GROW_LATE ** (level - PIVOT)


def main():
    rows = []
    for lv in range(1, MAX_LEVEL + 1):
        xp = required_xp(lv)
        # 보기 좋게 유효숫자 3자리로 반올림 (초반은 정수 그대로)
        if xp >= 1000:
            digits = len(str(int(xp))) - 3
            step = 10 ** max(0, digits)
            xp = round(xp / step) * step
        rows.append({"level": lv, "requiredXP": int(round(xp))})

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8") as f:
        json.dump(rows, f, ensure_ascii=False, indent=1)

    print("wrote", OUT, len(rows), "levels")
    for lv in (1, 10, 30, 60, 100, 140, 200):
        print("  Lv%-4d %15s" % (lv, format(rows[lv - 1]["requiredXP"], ",")))
    total = sum(r["requiredXP"] for r in rows)
    print("  누적(1→200) %s" % format(total, ","))
    print("  int 한계(2,147,483,647) 대비 최대값:",
          format(max(r["requiredXP"] for r in rows), ","))


if __name__ == "__main__":
    main()
