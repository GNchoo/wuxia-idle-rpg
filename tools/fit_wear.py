"""착용용 스프라이트를 SP1 리그 규격에 맞춘다.

**규격은 상상하지 않고 캐릭터가 이미 들고 있는 파츠에서 실측한 값을 쓴다.**
(원본 21종을 뽑아 잰 값 — tools/sp1-wear-reference.md 에 근거를 남겼다)

| 항목 | 실측값 |
|---|---|
| 방향 | 주축 **-30°** = 날이 좌상단, 자루가 우하단 |
| 캔버스 | 알파 타이트 박스 (85x57 ~ 206x140), 정사각 아님 |
| 피벗 | 정중앙 (0.5, 0.5) |
| PPU | 100 |
| 외곽선 | 순검정, 긴 변의 약 1/48 두께 |

생성물은 세로로 서 있으므로 회전이 필요하다. 세워진 무기(끝이 위)를
**+60° (반시계)** 돌리면 끝이 좌상단으로 가서 원본과 같은 축이 된다.

  fit_wear.py <디렉터리> [회전각] [긴변px] [외곽선px]

인자를 안 주면 파일명으로 무기 종류를 알아내 종류별 실측 크기를 쓴다.
"""
import sys, os, glob
from PIL import Image, ImageFilter

# 원본 실측: 단검류 85~120, 검·도 136~173, 창·곤 201~206, 완갑류 68~89
KIND_SIZE = {
    "sword":    150,   # 검  (원본 0007=145, 0014=173)
    "saber":    150,   # 도
    "polearm":  205,   # 창·곤 (원본 0004=201, 0019=206)
    "small":    105,   # 기병 (원본 0001=85, 0017=106)
    "gauntlet":  90,   # 권갑 (원본 0008=68, 0012=89)
    # 방어구 — tools/sp1-armor-reference.md 실측
    # 투구: 원본은 145x144 로 거의 정사각인데 AI는 옆으로 퍼지게 그린다.
    # 170으로 뽑았더니 화면에서 폭이 원본의 1.18배(머리보다 넓다) → 145로 맞춘다.
    "hat":      145,
    "armor":     76,   # 몸통 조각 (원본 60~78, 중앙 76x71)
    "shoulder":  40,   # 어깨 패드 (원본 27~44)
    "glove":     38,
    "shoes":     36,
    "belt":      60,
    "neck":      52,
    "sleeve":    43,   # 소매 ArmorArm (원본 30~43, 중앙 41x41)
    "pants":     60,   # 바지 허리 Pants_Pants (원본 60x25)
    "pantsleg":  36,   # 바지 다리 Pants_L/R (원본 30~36)
}

# 방어구는 '긴 변'이 아니라 **박스**로 맞춘다. 긴 변만 맞추면 세로로 긴 그림이
# 몸보다 길쭉해진다(실측: 조끼가 몸통보다 높고 좁게 붙었다).
# 값은 원본 파츠 실측 + 덮을 몸 파츠와의 비교로 잡았다.
#   투구 세로 80 — 원본 투구는 얼굴에 눈구멍이 뚫려 있어 머리를 다 덮어도 되지만
#   우리 그림은 통짜라 다 덮으면 눈이 사라진다. 눈 위에서 끝나게 낮춘다.
KIND_BOX = {
    "hat":      (145, 80),
    "armor":    (72, 60),   # 몸통이 길면 다리·발을 가린다
    "shoulder": (40, 40),
    # 소매·장갑이 크면 팔 전체를 삼켜 손이 없어 보인다(유저 지적) — 손끝이 보이게 축소
    "glove":    (31, 32),
    "shoes":    (36, 36),
    "belt":     (60, 44),
    "neck":     (52, 34),
    "sleeve":   (34, 30),
    "pants":    (60, 34),
    "pantsleg": (36, 33),
    # S4 플레이어 전용 얼굴·머리 (원본: Face 125x117, HairTop ~125x83, HairBack ~54x61)
    "face":     (125, 117),
    "hairtop":  (128, 100),
    # 뒷머리는 원본 목덜미 조각 규격에 맞춘다 — 크게 붙이면 머리 위로 솟는다(실측)
    "hairback": (84, 82),
}

# 무기는 세워서 뽑으니 눕혀야 하고(+60°), 방어구는 이미 착용 방향이라 안 돌린다.
KIND_ROT = dict((k, 0) for k in
                ("hat", "armor", "shoulder", "glove", "shoes", "belt", "neck",
                 "sleeve", "pants", "pantsleg", "face", "hairtop", "hairback"))

# 무기 id → 종류. weapons.json 의 kind 와 같은 묶음이다.
KIND_OF = {}
for _n in ("wood_sword", "steel_sword", "mythril_blade", "dragon_blade",
           "crimson_saber", "knight_claymore"):
    KIND_OF["w_" + _n] = "sword"
for _n in ("oak_staff", "flame_staff", "frost_staff", "arcane_staff",
           "thunder_rod", "void_wand"):
    KIND_OF["w_" + _n] = "saber"
for _n in ("hunter_bow", "maple_bow", "storm_bow", "phoenix_bow",
           "composite_bow", "sniper_bow"):
    KIND_OF["w_" + _n] = "polearm"
for _n in ("bronze_claw", "shadow_claw", "venom_claw", "assassin_claw",
           "iron_knuckle", "blood_fist"):
    KIND_OF["w_" + _n] = "small"
for _n in ("gauntlet_cloth", "gauntlet_leather", "gauntlet_iron",
           "gauntlet_steel", "gauntlet_cold", "gauntlet_dark"):
    KIND_OF["w_" + _n] = "gauntlet"

# 방어구는 파일명 앞부분이 곧 부위다 (hat_t1, shoulder_t3 ...)
# 바지만 한 벌이 두 종류라 접미사로 갈린다: pants_t1(허리) / pants_t1@leg(다리)
_ARMOR_PARTS = ("hat", "armor", "shoulder", "glove", "shoes", "belt", "neck",
                "sleeve", "pants", "face", "hairtop", "hairback")


def kind_of(name):
    if name in KIND_OF:
        return KIND_OF[name]
    head = name.split("@", 1)[0].split("_", 1)[0]
    if head == "pants" and "@leg" in name:
        return "pantsleg"
    return head if head in _ARMOR_PARTS else ""


def outline(im, width, color=(0, 0, 0, 255)):
    """알파 실루엣 바깥에 검정 테두리를 두른다.

    원본 파츠는 전부 순검정 외곽선을 두르고 있어서, 없으면 우리 것만
    붕 떠 보인다(실측: 경계 픽셀 100%가 (0,0,0)).
    """
    if width <= 0:
        return im
    pad = width + 2
    big = Image.new("RGBA", (im.width + pad * 2, im.height + pad * 2), (0, 0, 0, 0))
    big.paste(im, (pad, pad))
    a = big.getchannel("A")
    # MaxFilter 로 알파를 부풀려 실루엣을 키운다 (커널은 홀수)
    grown = a.filter(ImageFilter.MaxFilter(width * 2 + 1))
    ring = Image.new("RGBA", big.size, color)
    ring.putalpha(grown)
    ring.alpha_composite(big)
    return ring


def edge_darkness(im):
    """실루엣 경계 픽셀 중 검정에 가까운 비율. 이미 외곽선이 그려져 있는지 본다."""
    w, h = im.size
    px = im.load()
    dark = total = 0
    for y in range(h):
        for x in range(w):
            if px[x, y][3] < 32:
                continue
            if all(0 <= x + dx < w and 0 <= y + dy < h and px[x + dx, y + dy][3] >= 32
                   for dx in (-1, 0, 1) for dy in (-1, 0, 1)):
                continue                      # 안쪽 픽셀
            total += 1
            if sum(px[x, y][:3]) < 150:
                dark += 1
    return dark / float(total) if total else 0.0


SRC_CACHE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "_wear_src")

# 가로세로를 따로 늘려 원본 박스에 맞추되, 이 배율 이상은 찌그러뜨리지 않는다.
# AI는 사람 비율로 그리고 SP1 파츠는 등신이 낮아, 조금 눌러 주면 몸에 붙는다.
MAX_SQUASH = 1.35


def _pristine(path):
    """처음 본 파일은 tools/_wear_src 에 원본으로 보관하고, 이후엔 항상 거기서 시작한다.

    안 그러면 fit 을 두 번 돌릴 때마다 축소가 겹쳐 그림이 점점 뭉개진다.
    (Unity가 스캔하지 않는 곳에 둬야 해서 Assets 밖이다)
    """
    if not os.path.isdir(SRC_CACHE):
        os.makedirs(SRC_CACHE)
    keep = os.path.join(SRC_CACHE, os.path.basename(path))
    if not os.path.exists(keep):
        Image.open(path).convert("RGBA").save(keep)
    return keep


def _box_scale(w, h, bw, bh):
    """(bw,bh) 박스를 넘지 않게 맞춘다. 비율이 맞지 않으면 MAX_SQUASH 까지만 눌러 채운다."""
    k = min(float(bw) / w, float(bh) / h)
    kx = min(float(bw) / w, k * MAX_SQUASH)
    ky = min(float(bh) / h, k * MAX_SQUASH)
    return max(1, int(round(w * kx))), max(1, int(round(h * ky)))


def fit(path, rotate=0, size=150, out=None, outline_px=None, box=None):
    im = Image.open(_pristine(path)).convert("RGBA")
    if rotate:
        im = im.rotate(rotate, expand=True, resample=Image.BICUBIC)
    bbox = im.getbbox()                       # 알파 기준 타이트 크롭
    if bbox:
        im = im.crop(bbox)
    w, h = im.size
    if box:
        nw, nh = _box_scale(w, h, box[0], box[1])
    else:
        scale = float(size) / max(w, h)
        nw, nh = max(1, int(w * scale)), max(1, int(h * scale))
    im = im.resize((nw, nh), Image.LANCZOS)
    size = max(nw, nh)
    if outline_px is None:
        # 프롬프트가 먹어서 이미 검정 테두리가 있으면 덧그리지 않는다 (두 겹이 된다)
        outline_px = 0 if edge_darkness(im) > 0.6 else max(1, int(round(size / 48.0)))
    im = outline(im, outline_px)
    bbox = im.getbbox()                       # 테두리까지 포함해 다시 타이트하게
    if bbox:
        im = im.crop(bbox)
    # 정사각 캔버스를 두지 않는다 — 원본도 타이트 박스이고,
    # pivot(0.5,0.5)은 어차피 그 박스의 중심이다.
    im.save(out or path)
    return im.size


if __name__ == "__main__":
    d = sys.argv[1]
    forced_rot = float(sys.argv[2]) if len(sys.argv) > 2 else None
    forced = int(sys.argv[3]) if len(sys.argv) > 3 else 0
    oline = int(sys.argv[4]) if len(sys.argv) > 4 else None
    for f in sorted(glob.glob(os.path.join(d, "*.png"))):
        name = os.path.splitext(os.path.basename(f))[0]
        if name.startswith("_raw_"):
            continue
        kind = kind_of(name)
        size = forced or KIND_SIZE.get(kind, 150)
        rot = forced_rot if forced_rot is not None else KIND_ROT.get(kind, 60)
        box = None if forced else KIND_BOX.get(kind)
        print(name, "->", fit(f, rot, size, outline_px=oline, box=box))
