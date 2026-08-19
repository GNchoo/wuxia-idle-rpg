"""AI가 '투명해 보이도록' 그려 넣은 체커보드 배경을 실제 알파로 바꾼다.

xAI 이미지 API에는 투명 옵션이 없어서 결과가 RGB(알파 없음)로 온다.
배경은 밝은 회색/흰색 두 톤이 번갈아 찍힌 균일한 격자라, 모서리에서 배경 톤을
샘플링해 그 두 톤에 가까운 픽셀만 지우면 물체는 남고 배경만 투명해진다.
"""
import sys, os
from PIL import Image
import numpy as np

def strip(path, out=None, tol=16, edge_feather=True):
    im = Image.open(path).convert("RGB")
    a = np.asarray(im).astype(np.int16)
    h, w, _ = a.shape

    # 네 모서리 16x16에서 배경 톤 후보를 모은다 (체커 두 색)
    patches = np.concatenate([
        a[0:16, 0:16].reshape(-1, 3), a[0:16, w-16:w].reshape(-1, 3),
        a[h-16:h, 0:16].reshape(-1, 3), a[h-16:h, w-16:w].reshape(-1, 3),
    ])
    # 밝기로 두 그룹(밝은칸/어두운칸) 분리
    lum = patches.mean(axis=1)
    thr = (lum.min() + lum.max()) / 2.0
    tones = []
    for grp in (patches[lum <= thr], patches[lum > thr]):
        if len(grp): tones.append(grp.mean(axis=0))
    if not tones:
        return None

    # 배경 톤과의 최소 거리
    dist = np.full((h, w), 1e9, dtype=np.float32)
    for t in tones:
        d = np.abs(a - t.reshape(1, 1, 3)).max(axis=2)
        dist = np.minimum(dist, d)

    alpha = np.where(dist <= tol, 0, 255).astype(np.uint8)

    # 물체 안쪽에 배경색과 우연히 같은 점이 뚫리는 걸 막는다:
    # 가장자리에서 연결된 영역만 배경으로 인정 (flood fill)
    from collections import deque
    bg = np.zeros((h, w), dtype=bool)
    q = deque()
    for x in range(w):
        for y in (0, h - 1):
            if alpha[y, x] == 0 and not bg[y, x]: bg[y, x] = True; q.append((y, x))
    for y in range(h):
        for x in (0, w - 1):
            if alpha[y, x] == 0 and not bg[y, x]: bg[y, x] = True; q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((1,0),(-1,0),(0,1),(0,-1)):
            ny, nx = y+dy, x+dx
            if 0 <= ny < h and 0 <= nx < w and not bg[ny, nx] and alpha[ny, nx] == 0:
                bg[ny, nx] = True; q.append((ny, nx))
    alpha = np.where(bg, 0, 255).astype(np.uint8)

    rgba = np.dstack([np.asarray(im).astype(np.uint8), alpha])
    res = Image.fromarray(rgba, "RGBA")
    out = out or path
    res.save(out)
    pct = round(float((alpha == 0).sum()) * 100.0 / alpha.size, 1)
    return pct

if __name__ == "__main__":
    files = sys.argv[1:]
    for f in files:
        pct = strip(f)
        print(os.path.basename(f), "-> transparent", pct, "%")
