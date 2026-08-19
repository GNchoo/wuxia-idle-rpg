"""시트에서 물체를 잘라낸다 — 세로 투영(column projection) 방식.

연결 성분으로 자르면 검의 '칼날'과 '손잡이'가 끊긴 곳에서 두 조각이 되어
반쪽짜리가 저장된다. 시트는 물체가 가로로 나열되므로, 알파가 있는 열을 찾아
x구간으로 나누면 한 물체의 위아래 조각이 함께 묶인다.
"""
import sys, os
import numpy as np
from PIL import Image

def split(path, outdir, names=None, gap_div=200, min_w_div=60, pad=10):
    im = Image.open(path).convert("RGBA")
    a = np.asarray(im.getchannel("A"))
    h, w = a.shape
    colhas = (a > 32).any(axis=0)

    gap_allow = max(2, w // gap_div)
    min_w = max(8, w // min_w_div)
    boxes, x = [], 0
    while x < w:
        while x < w and not colhas[x]: x += 1
        if x >= w: break
        sx = x; ex = x; gap = 0
        while x < w:
            if colhas[x]: ex = x; gap = 0
            else:
                gap += 1
                if gap > gap_allow: break
            x += 1
        if ex - sx + 1 < min_w: continue
        band = a[:, sx:ex+1]
        rows = np.where((band > 32).any(axis=1))[0]
        if len(rows) == 0: continue
        boxes.append((sx, int(rows[0]), ex, int(rows[-1])))

    os.makedirs(outdir, exist_ok=True)
    out = []
    for i, (x0, y0, x1, y1) in enumerate(boxes):
        cx0 = max(0, x0-pad); cy0 = max(0, y0-pad)
        cx1 = min(w, x1+1+pad); cy1 = min(h, y1+1+pad)
        crop = im.crop((cx0, cy0, cx1, cy1))
        side = max(crop.width, crop.height)
        sq = Image.new("RGBA", (side, side), (0,0,0,0))
        sq.paste(crop, ((side-crop.width)//2, (side-crop.height)//2))
        nm = names[i] if names and i < len(names) else "part%02d" % (i+1)
        sq.save(os.path.join(outdir, nm + ".png"))
        out.append((nm, crop.size))
    return out

if __name__ == "__main__":
    src, outdir = sys.argv[1], sys.argv[2]
    names = sys.argv[3].split(",") if len(sys.argv) > 3 else None
    res = split(src, outdir, names)
    print("조각 %d개" % len(res))
    for nm, size in res: print(" ", nm, size)
