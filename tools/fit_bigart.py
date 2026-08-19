"""Q5 배경·보스 후처리 — 생성물을 기존 에셋과 같은 규격으로 맞춘다.

배경(Biome01~10): 원본이 전부 1080x607. 생성물이 다른 비율이면 중앙 크롭 후 리사이즈.
보스(Boss*):      원본 표본 699x670. 알파 타이트 크롭 후 긴 변 700으로.
                  (필드 배치 크기는 sizePx가 정하지만, PPU가 같아야 체급이 같다)

  python tools/fit_bigart.py
"""
import glob
import os
import time

from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..",
                    "Assets", "_Project", "Resources", "TplArt")
FRESH_SEC = 3 * 3600          # 최근 생성분만 손댄다 — 기존 서양 아트는 건드리지 않는다

BIOME_W, BIOME_H = 1080, 607
BOSS_LONG = 700


def fresh(path):
    return time.time() - os.path.getmtime(path) < FRESH_SEC


def fit_biome(path):
    im = Image.open(path).convert("RGB")
    tw, th = BIOME_W, BIOME_H
    k = max(tw / im.width, th / im.height)
    im = im.resize((round(im.width * k), round(im.height * k)), Image.LANCZOS)
    x = (im.width - tw) // 2
    # 배경은 지평선이 아래쪽 — 세로가 남으면 아래를 살리고 위(하늘)를 자른다
    y = im.height - th
    im = im.crop((x, y, x + tw, y + th))
    im.save(path)
    return im.size


def fit_boss(path):
    im = Image.open(path).convert("RGBA")
    box = im.getbbox()
    if box:
        im = im.crop(box)
    k = BOSS_LONG / max(im.size)
    if k < 1:
        im = im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.LANCZOS)
    im.save(path)
    return im.size


if __name__ == "__main__":
    for f in sorted(glob.glob(os.path.join(ROOT, "Biomes", "Biome*.png"))):
        if fresh(f):
            print(os.path.basename(f), "->", fit_biome(f))
    for f in sorted(glob.glob(os.path.join(ROOT, "Bosses", "Boss*.png"))):
        if fresh(f):
            print(os.path.basename(f), "->", fit_boss(f))
