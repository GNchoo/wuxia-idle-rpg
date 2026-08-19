"""크로마키(마젠타) 배경을 실제 알파로 바꾼다.

가장자리에서 연결된 배경만 지우므로, 물체 안쪽에 우연히 비슷한 색이 있어도 살아남는다.
반투명 광휘처럼 배경과 섞인 픽셀은 섞인 정도만큼 알파를 낮춘다(경계 계단 방지).
"""
import sys, os
import numpy as np
from PIL import Image
from collections import deque

def strip(path, out=None, tol=70):
    im = Image.open(path).convert("RGB")
    a = np.asarray(im).astype(np.int16)
    h, w, _ = a.shape

    # 모서리에서 배경색 추정
    corners = np.stack([a[2,2], a[2,w-3], a[h-3,2], a[h-3,w-3]]).astype(np.float32)
    bgc = corners.mean(axis=0)
    r,g,b = bgc
    if not (r>120 and b>100 and g<110 and (r-g)>50):
        return None, "배경이 마젠타가 아님 %s" % bgc.astype(int).tolist()

    dist = np.abs(a - bgc.reshape(1,1,3)).max(axis=2)
    near = dist <= tol

    # 가장자리 연결 성분만 배경으로 인정
    bgmask = np.zeros((h,w), bool)
    q = deque()
    for x in range(w):
        for y in (0, h-1):
            if near[y,x] and not bgmask[y,x]: bgmask[y,x]=True; q.append((y,x))
    for y in range(h):
        for x in (0, w-1):
            if near[y,x] and not bgmask[y,x]: bgmask[y,x]=True; q.append((y,x))
    while q:
        y,x = q.popleft()
        for dy,dx in ((1,0),(-1,0),(0,1),(0,-1)):
            ny,nx = y+dy, x+dx
            if 0<=ny<h and 0<=nx<w and near[ny,nx] and not bgmask[ny,nx]:
                bgmask[ny,nx]=True; q.append((ny,nx))

    alpha = np.where(bgmask, 0, 255).astype(np.uint8)

    # 경계 부드럽게: 배경에 가까울수록 알파를 낮춘다 (배경 인접 픽셀만)
    soft = (~bgmask) & (dist < tol*1.6)
    if soft.any():
        frac = np.clip((dist[soft] - tol*0.5) / (tol*1.1), 0, 1)
        alpha[soft] = (frac*255).astype(np.uint8)

    # 마젠타 물이 든 경계 픽셀의 색을 눌러준다 (보라 테두리 방지)
    rgb = np.asarray(im).astype(np.float32)
    edge = soft
    if edge.any():
        # 초록을 기준으로 R,B의 과잉분을 깎는다
        gch = rgb[...,1]
        rgb[...,0] = np.where(edge, np.minimum(rgb[...,0], gch*1.25+40), rgb[...,0])
        rgb[...,2] = np.where(edge, np.minimum(rgb[...,2], gch*1.25+40), rgb[...,2])

    out = out or path
    Image.fromarray(np.dstack([rgb.astype(np.uint8), alpha]), "RGBA").save(out)
    return round(float((alpha==0).sum())*100.0/alpha.size,1), None

if __name__ == "__main__":
    for f in sys.argv[1:]:
        pct, err = strip(f)
        print(os.path.basename(f), "->", ("transparent %s%%" % pct) if err is None else ("SKIP: "+err))
