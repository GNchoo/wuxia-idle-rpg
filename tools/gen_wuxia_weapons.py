"""무협 무기 생성 → Hippo 규격 변환 (V7).

로컬 Z-Image 서버(8009)로 티어별 무기를 뽑아 Hippo 무기 스프라이트 규격으로 맞춘다:
  캔버스 256x512, 칼끝 위, 아트 중앙 x=128, 피벗(0.5, 0.25)=손잡이(아트 하단 18% 지점),
  2H 아트 높이 ~268px / 1H ~200px (SamuraiSword1/Katana1 실측).

출력: Assets/_Project/Resources/WuxiaWeapons/k{kind}_t{tier:02}.png
임포트 설정(피벗·PPU)은 Unity 에디터 메뉴 'IdleMvp/아트/무협 무기 임포트 설정'이 박는다.
"""
import io
import json
import os
import sys
import urllib.request

from PIL import Image

SERVER = "http://127.0.0.1:8009/generate"
OUT = "Assets/_Project/Resources/WuxiaWeapons"

STYLE = ("one single weapon only, perfectly vertical, blade tip pointing straight up, "
         "grip at the bottom, side view profile, centered. "
         "ART STYLE: chunky simplified 2D cartoon game sprite, VERY thick uniform pure black "
         "outline around the whole silhouette and every interior part, flat cel shading with "
         "only two or three solid tones per material, hard edges, no gradients, no texture "
         "noise, bold readable shapes, mobile game item art. "
         "BACKGROUND: entire background is one single flat solid magenta color #FF00FF, "
         "no shadow, no glow spilling onto background, no floor, no frame.")

# 검(kind 0) 10티어 — 소박함 → 화려함 도파민 커브 (무협 톤)
SWORDS = [
    "a plain wooden practice sword (mokgeom), simple straight wooden blade, cloth-wrapped handle",
    "a basic iron straight sword with a small round guard, dull gray blade, leather-wrapped grip",
    "a steel Chinese jian straight sword, small winged bronze guard, blue silk tassel on pommel",
    "a fine blue-steel jian sword, elegant bronze cloud-pattern guard, azure silk tassel, subtle shine",
    "a heavy two-handed dao saber, broad curved steel blade, red tassel, dark iron ring pommel",
    "a two-handed jian greatsword with silver dragon-head guard, jade inlay on the hilt, white tassel",
    "a two-handed crimson-red blade with gold phoenix engraving on the blade, ornate gold guard",
    "a two-handed dark violet blade with glowing purple runes along the edge, silver serpent guard",
    "a two-handed azure ice-crystal blade, frost mist on the edge, sapphire dragon guard, glowing pale blue",
    "a two-handed radiant golden dragon sword, blazing golden flame aura on the blade, dragon coiled around the hilt, legendary divine weapon",
]

# 아트 배치 파라미터 (Hippo 실측)
CANVAS = (256, 512)
PIVOT_Y = 384          # top-down px (0.5, 0.25)
GRIP_FRAC = 0.18       # 손잡이 = 아트 하단에서 18%
ART_H_1H = 200
ART_H_2H = 268


def generate(prompt, seed, w=640, h=1024):
    body = json.dumps({"prompt": prompt, "width": w, "height": h, "seed": seed}).encode()
    req = urllib.request.Request(SERVER, data=body, headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=600) as r:
        data = r.read()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("not a png: " + data[:200].decode(errors="replace"))
    return Image.open(io.BytesIO(data)).convert("RGBA")


def chroma_remove(im, key=(255, 0, 255), tol=120):
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            # 마젠타 거리: R·B 높고 G 낮은 픽셀
            d = abs(r - key[0]) + abs(g - key[1]) + abs(b - key[2])
            if d < tol:
                px[x, y] = (0, 0, 0, 0)
    return im


def despeckle(im, min_px=120):
    """고립 섬 제거 — 배경 잔여물이 캔버스 구석에 뜨는 것 방지."""
    px = im.load()
    w, h = im.size
    seen = [[False] * h for _ in range(w)]
    for sx in range(w):
        for sy in range(h):
            if seen[sx][sy] or px[sx, sy][3] <= 16:
                continue
            stack = [(sx, sy)]
            seen[sx][sy] = True
            pts = []
            while stack:
                x, y = stack.pop()
                pts.append((x, y))
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and not seen[nx][ny] and px[nx, ny][3] > 16:
                        seen[nx][ny] = True
                        stack.append((nx, ny))
            if len(pts) < min_px:
                for x, y in pts:
                    px[x, y] = (0, 0, 0, 0)
    return im


def to_hippo(im, art_h):
    """타이트 크롭 → 목표 높이로 스케일 → 256x512, 손잡이가 피벗(384)에 오게 배치."""
    bbox = im.getbbox()
    if bbox is None:
        raise RuntimeError("empty image after chroma removal")
    art = im.crop(bbox)
    scale = art_h / art.height
    art = art.resize((max(1, round(art.width * scale)), art_h), Image.LANCZOS)
    canvas = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
    bottom = round(PIVOT_Y + GRIP_FRAC * art_h)
    canvas.alpha_composite(art, (128 - art.width // 2, bottom - art_h))
    return canvas


def main():
    only = sys.argv[1] if len(sys.argv) > 1 else None   # 예: t07 하나만 재생성
    os.makedirs(OUT, exist_ok=True)
    for i, desc in enumerate(SWORDS):
        tier = i + 1
        name = "k0_t%02d" % tier
        if only and only not in name:
            continue
        art_h = ART_H_2H if tier >= 5 else ART_H_1H
        print("[gen]", name, "...", flush=True)
        im = generate(desc + ". " + STYLE, seed=4200 + tier)
        im = chroma_remove(im)
        im = despeckle(im)
        im = to_hippo(im, art_h)
        im.save(os.path.join(OUT, name + ".png"))
        print("[ok]", name, flush=True)
    print("done")


if __name__ == "__main__":
    main()
