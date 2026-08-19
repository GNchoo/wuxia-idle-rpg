"""A-포즈 캐릭터를 페이퍼돌 파츠로 절단한다 (v2 — 중복+메꿈 방식).

v1(배타 분할)의 교훈: 직선 절단선이 옷을 관통해 이음선이 보이고, 회전하면
구멍이 드러났다. v2는
  1) 팔·부츠를 관절캡까지 '중복 포함'해 자르고 (정지 상태 = 원본과 동일)
  2) 몸통에서 소매 아래쪽만 지운 뒤 도포색으로 메꾼다 (회전 노출 시 천으로 보임)
  3) 머리는 몸통에서 아무것도 지우지 않는다 (목 위 중복만)

각 파츠는 타이트 크롭 + manifest.json(오프셋·피벗)로 기록 — HeroTallBuilder가 조립.
"""
import json
import os
from PIL import Image, ImageDraw

SRC = "tools/_face_src/hero_apose_cut.png"
OUT = "Assets/_Project/Resources/HeroTall"

# 파츠 추출 폴리곤 (관절캡 포함 — 원본 832x1408 좌표, 격자 실측)
REGIONS = {
    "head":  [(295, 55), (600, 55), (600, 335), (455, 318), (430, 310), (390, 306), (355, 300), (330, 286)],
    "armR":  [(255, 345), (345, 340), (348, 462), (308, 470), (303, 560), (298, 605), (238, 662), (222, 765), (145, 765), (145, 480)],
    "armL":  [(540, 395), (600, 380), (700, 430), (700, 780), (560, 780), (535, 520), (523, 480)],
    "bootR": [(130, 1128), (365, 1138), (372, 1300), (330, 1348), (40, 1348), (40, 1300), (128, 1262)],
    "bootL": [(495, 1140), (598, 1148), (606, 1292), (600, 1362), (495, 1362)],
}
# 몸통에서 '지우는' 영역 (관절캡 아래 소매·부츠) — 팔은 도포색으로 메꾼다
TORSO_REMOVE = {
    "armR":  ([(250, 455), (335, 462), (306, 472), (300, 560), (295, 608), (235, 663), (218, 765), (145, 765), (145, 500)], True),
    "armL":  ([(532, 472), (700, 472), (700, 780), (560, 780), (537, 522)], True),
    "bootR": ([(130, 1168), (362, 1168), (372, 1300), (330, 1348), (40, 1348), (40, 1300), (128, 1262)], False),
    "bootL": ([(495, 1168), (600, 1168), (606, 1292), (600, 1362), (495, 1362)], False),
}
# 회전 피벗 (원본 좌표계) — 어깨캡 중심·목·부츠 상단·허리
PIVOTS = {
    "head":  (415, 300),
    "armR":  (308, 392),
    "armL":  (580, 425),
    "bootR": (250, 1150),
    "bootL": (548, 1155),
    "torso": (430, 900),
}
# 그리기 순서 (뒤→앞): 부츠 → 몸통 → 머리 → 팔(중복 캡이 몸통 어깨를 덮는다)
ORDER = {"bootR": 0, "bootL": 1, "torso": 2, "head": 3, "armL": 4, "armR": 5}

ROBE_FILL_SAMPLE = (300, 600)   # 도포 남색 샘플 지점


def main():
    im = Image.open(SRC).convert("RGBA")
    os.makedirs(OUT, exist_ok=True)
    manifest = {"canvas": [im.width, im.height], "parts": {}}

    fill = im.getpixel(ROBE_FILL_SAMPLE)

    # 1) 파츠 추출 (원본에서 그대로 — 중복 허용)
    for name, poly in REGIONS.items():
        mask = Image.new("L", im.size, 0)
        ImageDraw.Draw(mask).polygon(poly, fill=255)
        part = Image.new("RGBA", im.size, (0, 0, 0, 0))
        part.paste(im, (0, 0), mask)
        bbox = part.getbbox()
        part.crop(bbox).save(os.path.join(OUT, name + ".png"))
        px, py = PIVOTS[name]
        manifest["parts"][name] = {
            "offset": [bbox[0], bbox[1]],
            "size": [bbox[2] - bbox[0], bbox[3] - bbox[1]],
            "pivot": [px, py], "order": ORDER[name],
        }

    # 2) 몸통: 소매 아래·부츠 제거, 소매 자리는 도포색 메꿈
    torso = im.copy()
    for name, (poly, do_fill) in TORSO_REMOVE.items():
        mask = Image.new("L", im.size, 0)
        ImageDraw.Draw(mask).polygon(poly, fill=255)
        if do_fill:
            # 원래 불투명했던 픽셀만 도포색으로 (배경까지 칠하면 실루엣이 커진다)
            solid = Image.new("RGBA", im.size, fill)
            alpha = im.getchannel("A").point(lambda a: 255 if a > 32 else 0)
            both = Image.new("L", im.size, 0)
            both.paste(alpha, (0, 0), mask)
            torso.paste(solid, (0, 0), both)
        else:
            clear = Image.new("RGBA", im.size, (0, 0, 0, 0))
            torso.paste(clear, (0, 0), mask)

    # 절단 잔여물 청소: 몸통에서 300px 미만의 고립 섬 제거 (스윙 시 부유 조각으로 보였다)
    px = torso.load()
    w, h = torso.size
    seen = [[False] * h for _ in range(w)]
    for sx in range(w):
        for sy in range(h):
            if seen[sx][sy] or px[sx, sy][3] <= 32:
                continue
            stack = [(sx, sy)]
            seen[sx][sy] = True
            pts = []
            while stack:
                x, y = stack.pop()
                pts.append((x, y))
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and not seen[nx][ny] and px[nx, ny][3] > 32:
                        seen[nx][ny] = True
                        stack.append((nx, ny))
            if len(pts) < 300:
                for x, y in pts:
                    px[x, y] = (0, 0, 0, 0)

    bbox = torso.getbbox()
    torso.crop(bbox).save(os.path.join(OUT, "torso.png"))
    px, py = PIVOTS["torso"]
    manifest["parts"]["torso"] = {
        "offset": [bbox[0], bbox[1]],
        "size": [bbox[2] - bbox[0], bbox[3] - bbox[1]],
        "pivot": [px, py], "order": ORDER["torso"],
    }

    with open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=1)
    print("parts:", list(manifest["parts"]))


if __name__ == "__main__":
    main()
