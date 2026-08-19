"""표정 샘플 파이프라인 도우미.

analyze  : 기본 얼굴(0012)에서 연결 성분을 나눠 입 획을 찾는다
compose  : 입 획을 지우고 AI 생성 입 패치를 그 자리에 합성해 변형 4종 저장
"""
import sys, os
from PIL import Image

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "_face_src")


def components(im):
    w, h = im.size
    px = im.load()
    seen = [[False] * h for _ in range(w)]
    comps = []
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
            comps.append(pts)
    return comps


def analyze():
    im = Image.open(os.path.join(BASE, "face_base_0012.png")).convert("RGBA")
    for pts in sorted(components(im), key=len, reverse=True):
        xs = [p[0] for p in pts]
        ys = [p[1] for p in pts]
        print(f"comp size={len(pts)} bbox=({min(xs)},{min(ys)})-({max(xs)},{max(ys)})")


def compose(patch_dir, center):
    """SP1 기본 얼굴(0012)엔 입이 없다 — 윤곽+문양 위 하단 중앙에 입 패치를 얹기만 한다.
    center = (x,y) 입 중심 (기본 얼굴 좌표계 125x117)."""
    base = Image.open(os.path.join(BASE, "face_base_0012.png")).convert("RGBA")
    mcx, mcy = center

    names = sorted(f for f in os.listdir(patch_dir) if f.startswith("mouth_e") and f.endswith(".png"))
    out_i = 1
    for n in names:
        patch = Image.open(os.path.join(patch_dir, n)).convert("RGBA")
        bbox = patch.getbbox()
        if not bbox:
            continue
        patch = patch.crop(bbox)
        # 입 폭 상한 — 얼굴(125px) 대비 과대 방지. O 입(e4)은 볼 문양과 겹쳐 더 작게.
        cap = 20 if "e4" in n else 30
        if patch.width > cap:
            k = float(cap) / patch.width
            patch = patch.resize((cap, max(2, int(patch.height * k))), Image.LANCZOS)
        out = base.copy()
        out.alpha_composite(patch, (mcx - patch.width // 2, mcy - patch.height // 2))
        dst = os.path.join(BASE, f"face_expr{out_i}.png")
        out.save(dst)
        print("saved", dst, "patch", n, patch.size)
        out_i += 1


if __name__ == "__main__":
    if sys.argv[1] == "analyze":
        analyze()
    else:
        compose(sys.argv[2], tuple(int(v) for v in sys.argv[3].split(",")))
