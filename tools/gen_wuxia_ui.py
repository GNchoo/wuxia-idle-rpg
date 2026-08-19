"""무협풍 UI 아트 킷 생성 (W1).

로컬 Z-Image로 패널·버튼·프레임·헤더를 뽑는다. 톤: 먹빛·한지·주홍 옻칠·금박.
패널류는 불투명 통짜(9-slice 테두리는 임포터에서), 버튼·프레임은 마젠타 크로마키 제거.

출력: Assets/_Project/Resources/WuxiaUi/<id>.png
"""
import io
import json
import os
import sys
import urllib.request

from PIL import Image

SERVER = "http://127.0.0.1:8009/generate"
OUT = "Assets/_Project/Resources/WuxiaUi"

STYLE = ("clean 2D mobile game UI asset, crisp edges, flat cel shading with subtle depth, "
         "no text, no letters, no watermark, symmetric, centered composition")

CHROMA_BG = ("entire background is one single flat solid magenta color #FF00FF, "
             "no shadow on background, no glow spilling onto background")

# (id, w, h, chroma?, prompt)
# 고정 팔레트 — 모든 에셋 공통 (요소별 색이 제각각이면 화면에서 싸운다)
# v2 (유저 확정): 어두운 호두나무·진홍 기각 → '연한 모래빛 나무 + 크림 양피지' 밝은 톤.
PALETTE = ("STRICT LIMITED PALETTE: light warm tan wood, pale golden oak planks, "
           "aged cream parchment paper, soft medium brown wood accents, muted antique "
           "gold. bright soft warm sun-bleached tones overall. NO dark walnut, NO deep "
           "dark brown filling large areas, NO deep crimson, NO blue, NO purple, "
           "NO green, NO neon colors. ")

ITEMS = [
    ("window_large", 1600, 1008, True,
     PALETTE + "a complete hand-illustrated game UI window: a big hanging wooden notice board "
     "suspended by two thick ropes from the top corners, built from overlapping light tan "
     "wooden planks with nails and carved edges, a wide medium warm brown wooden title plank "
     "protruding at the top center (EMPTY, no text), decorative vine leaves on the frame, "
     "large EMPTY flat light wood plank area filling the inside for content, "
     "slightly irregular hand-crafted silhouette, whole window as one single object. "
     "no text anywhere. " + CHROMA_BG),
    ("window_popup", 1008, 768, True,
     PALETTE + "a complete hand-illustrated small game UI popup window: a hanging wooden board "
     "suspended by ropes, overlapping light tan wooden planks with nails, a small medium "
     "warm brown wooden title plank protruding at the top center (EMPTY, no text), "
     "EMPTY flat light wood area inside for content, hand-crafted irregular silhouette, "
     "one single object, no text anywhere. " + CHROMA_BG),
    ("wood_board", 1024, 1024, False,
     PALETTE + "a game UI window background made entirely of light warm tan wooden planks, bright and soft, "
     "like a big hanging wooden notice board, thick carved wooden plank frame border around all "
     "four edges in slightly deeper warm brown, horizontal light wood plank texture filling the "
     "center, warm and weathered, uniform border width. " + STYLE),
    ("paper_sheet", 512, 512, True,
     PALETTE + "a single sheet of aged cream parchment paper with slightly torn irregular "
     "edges, like a poster pinned on a board, subtle stains and folds, empty center. "
     + STYLE + ". " + CHROMA_BG),
    ("panel_hanji", 512, 512, False,
     PALETTE + "a square UI panel of aged cream parchment paper pinned on a light oak wooden "
     "board, thin medium brown wooden frame border, uniform border on all four sides, subtle "
     "paper texture, empty center. " + STYLE),
    ("panel_dark", 512, 512, False,
     PALETTE + "a square UI panel of medium warm tan wood planks with a thin antique gold trim "
     "border, uniform border on all four sides, subtle wood grain, empty center. " + STYLE),
    ("btn_primary", 512, 192, True,
     PALETTE + "a wide rounded rectangle UI button carved from warm terracotta brown wood "
     "plank, weathered edges, slight bevel, empty center. " + STYLE + ". " + CHROMA_BG),
    ("btn_secondary", 512, 192, True,
     PALETTE + "a wide rounded rectangle UI button carved from light tan oak wood plank with "
     "a thin brown outline, weathered edges, slight bevel, empty center. "
     + STYLE + ". " + CHROMA_BG),
    ("header_cloud", 1024, 256, True,
     PALETTE + "a wide horizontal title banner of medium warm brown wood plank, weathered, "
     "slightly irregular edges like an old sign board, symmetric, empty center for title. "
     + STYLE + ". " + CHROMA_BG),
    ("frame_gold", 512, 512, True,
     PALETTE + "a square ornate frame border only, antique brass with subtle oriental corner "
     "engraving, hollow transparent center, uniform border width. " + STYLE + ". " + CHROMA_BG),
    ("tab_on", 384, 128, True,
     PALETTE + "a wide horizontal UI tab of light warm cream wood plank, selected bright state, "
     "carved edges. " + STYLE + ". " + CHROMA_BG),
    ("tab_off", 384, 128, True,
     PALETTE + "a wide horizontal UI tab of medium muted tan wood plank, unselected dim "
     "state, carved edges. " + STYLE + ". " + CHROMA_BG),
    ("bar_bg", 512, 96, True,
     PALETTE + "a long horizontal empty progress bar track carved into light tan wood, "
     "recessed groove, rounded ends, thin gold rim. " + STYLE + ". " + CHROMA_BG),
    ("bar_fill", 512, 96, True,
     PALETTE + "a long horizontal progress bar fill of warm cream gold gradient, rounded ends, "
     "subtle shine. " + STYLE + ". " + CHROMA_BG),
    ("row_dark", 384, 128, True,
     PALETTE + "a wide horizontal strip of aged cream parchment paper with a thin brown "
     "ornamental line border, like one row of an old ledger, flat and subtle. "
     + STYLE + ". " + CHROMA_BG),
    ("chip_gold", 320, 128, True,
     PALETTE + "a small rounded UI label chip of medium warm brown wood with thin gold "
     "border, empty center. " + STYLE + ". " + CHROMA_BG),
    ("slot_frame", 320, 320, True,
     PALETTE + "a square item slot frame border only, pale grayscale carved wood with subtle "
     "corner ornaments, thick uniform border, hollow transparent center, neutral gray tones "
     "for tinting. " + STYLE + ". " + CHROMA_BG),
    ("slot_empty", 320, 320, True,
     PALETTE + "a square empty item slot recessed into light tan wood, subtle inner shadow, "
     "thin gold rim. " + STYLE + ". " + CHROMA_BG),
    # 풀스크린 화면(2560×1440)용 통짜 창 일러스트 — 화면 전체가 '나무로 짠 창'.
    # 테두리=조각 대들보+황동 장석, 내부=균일한 어두운 판자 벽(콘텐츠가 어디 놓여도 안 어긋남).
    # 주의: PALETTE의 'parchment paper' 단어가 내부에 양피지를 그려 넣는다(2회 재현)
    # → 이 항목만 종이 언급이 없는 전용 팔레트를 쓴다.
    # v2: 유저 피드백 — 직사각형 꽉 채움이 아니라 '진짜 나무판을 짠 것 같은'
    # 구불구불한 비정형 실루엣 + 상단 돌출 제목판. 크로마로 실루엣 보존.
    ("screen_wood", 1600, 896, True,
     "STRICT LIMITED PALETTE: light warm tan wood, pale golden oak planks, soft medium "
     "brown wood accents, muted antique gold. bright soft sun-bleached warm tones. "
     "NO dark walnut, NO deep crimson, NO blue, NO purple, NO green, NO neon colors. "
     "a complete hand-illustrated game UI window, one single object: a huge hand-crafted "
     "wooden notice board nailed together from many separate light tan oak planks of "
     "clearly DIFFERENT lengths, the ends of individual planks STICKING OUT past the "
     "border at different distances so the outline is strongly jagged and bumpy, "
     "the outer silhouette is OBVIOUSLY NOT a rectangle: chunky hand-chopped rounded "
     "corners, cracked and chipped plank tips, a thick top beam that extends much wider "
     "than the board on both sides, a wide medium warm brown wooden title plank "
     "protruding upward from the top center (EMPTY plank, no text), muted gold "
     "fittings and round nail heads, the whole inside fully covered with soft uniform "
     "flat light tan wooden planks, completely EMPTY interior: NO paper, NO parchment, "
     "NO objects inside, hand-illustrated wuxia style, no text anywhere, no letters. "
     + CHROMA_BG),
    # ---- 무공 서고 (스킬 = 책장) ----
    # 책장: 단칸(한 줄) 개방형 — 칸이 여러 개면 책 위치와 어긋난다(구역 어긋남 교훈).
    # 책·책장은 '정면 직교 시점' 강제 — 원근이 섞이면 세트로 안 보인다 (유저 기각 1회)
    ("bookshelf", 1600, 512, True,
     PALETTE + "a long antique light oak wooden bookshelf viewed straight from the front, "
     "perfectly FLAT ORTHOGRAPHIC front view with no perspective, ONE single wide open "
     "compartment, a perfectly straight horizontal shelf floor board at the bottom of the "
     "opening, the inside back wall a slightly deeper warm tan, completely EMPTY inside "
     "(no books, nothing on the shelf), thick carved light wood frame around the opening, "
     "high detail hand-painted 2D game asset, one single object. no text. " + CHROMA_BG),
    ("book_a", 256, 640, True,
     PALETTE + "ONE ancient East Asian stitch-bound book standing upright, FLAT "
     "ORTHOGRAPHIC front view with no perspective, a clean vertical rectangle silhouette, "
     "cover of deep green silk cloth with a thin gold trim frame and a row of stitched "
     "binding threads along the left edge, elegant and high detail hand-painted 2D game "
     "asset, EMPTY center with no text, one object. " + CHROMA_BG),
    ("book_b", 256, 640, True,
     PALETTE + "ONE ancient East Asian stitch-bound book standing upright, FLAT "
     "ORTHOGRAPHIC front view with no perspective, a clean vertical rectangle silhouette, "
     "cover of deep warm brown silk cloth with a thin gold trim frame and a row of "
     "stitched binding threads along the left edge, elegant and high detail hand-painted "
     "2D game asset, EMPTY center with no text, one object. " + CHROMA_BG),
    ("book_c", 256, 640, True,
     PALETTE + "ONE ancient East Asian stitch-bound book standing upright, FLAT "
     "ORTHOGRAPHIC front view with no perspective, a clean vertical rectangle silhouette, "
     "cover of aged cream paper with a thin brown trim frame and a row of stitched "
     "binding threads along the left edge, elegant and high detail hand-painted 2D game "
     "asset, EMPTY center with no text, one object. " + CHROMA_BG),
    ("book_locked", 256, 640, True,
     PALETTE + "ONE ancient East Asian stitch-bound book standing upright, FLAT "
     "ORTHOGRAPHIC front view with no perspective, a clean vertical rectangle silhouette, "
     "pale beige cloth cover tightly wrapped with dark crimson rope crossing in a big X "
     "with a small antique brass lock charm at the center, sealed forbidden book, high "
     "detail hand-painted 2D game asset, EMPTY, no text, one object. " + CHROMA_BG),
    # ---- 7일 출석 걸개 배너 ----
    ("banner_day", 256, 656, True,
     PALETTE + "one vertical hanging banner of soft warm brown silk with thin antique gold "
     "border, hanging from a small light wooden rod with a string loop at the top, a small "
     "tassel at the bottom, EMPTY center, oriental style, flat 2D game UI, one object. "
     + CHROMA_BG),
    ("banner_day_on", 256, 656, True,
     PALETTE + "one vertical hanging scroll of warm cream silk with muted crimson and "
     "gold border, hanging from a small dark wooden rod with a string loop at the top, "
     "a small tassel at the bottom, softly glowing, EMPTY center, oriental style, "
     "flat 2D game UI, one object. " + CHROMA_BG),
    # 다크+금 포인트 UI 세트 (유저 확정: AAA 캐릭터창 문법 — 반투명 먹색+가는 금테)
    ("btn_dark", 512, 176, True,
     "a wide rounded rectangle game UI button of dark charcoal lacquered wood, very dark "
     "warm gray-brown, subtle sheen, ONE thin elegant antique gold border line following "
     "the edge, flat premium minimalist style, completely empty center, no text. "
     + CHROMA_BG),
    ("row_dim", 768, 96, True,
     "a wide slim horizontal game UI stat row bar of dark charcoal, very dark warm gray, "
     "slightly translucent look, a THIN antique gold hairline line along the bottom edge "
     "only, flat premium minimalist style, completely empty, no text. " + CHROMA_BG),
    ("slot_dark", 320, 320, True,
     "a square game UI item slot of very dark charcoal with a thin antique gold border "
     "and a subtle inner shadow, flat premium minimalist style, empty center, no text. "
     + CHROMA_BG),
    # 주의: 어두운 반투명 패널을 크로마로 뽑으면 마젠타가 배어 자줏빛이 된다 → 불투명 풀블리드
    # ---- 화면 전용 배경 (구역·그리드가 그려진 레이아웃 배경, 유저 확정) ----
    # 원칙: 얇은 나무 테두리 + 전체화면 + 구역이 그려져 있고 UI를 그 구역에 맞춰 넣는다.
    # v4 (유저 확정): 전체화면 폐기 → 창 형태 2장(좌 캐릭터판 + 우 메인창), 무협 카툰
    # 세력별 캐릭터판 3종 (전직에 따라 교체) — 공통 프레임: 갈라진 나무 + 삼베 밧줄, 못 없음
    ("win_side_jeong", 560, 1008, False,
     "a CARTOON WUXIA game UI side panel, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a vertical panel framed by CRACKED "
     "SPLIT old walnut wood beams with deep grain cracks and chipped corners, bound at "
     "the corners with coarse HEMP ROPE lashings, absolutely NO metal nails. The UPPER TWO THIRDS of the panel (a large tall scene box) "
     "shows a serene righteous-sect scene: a stone Shaolin "
     "temple courtyard on a misty mountain with pagoda roofs, pine trees and plum "
     "blossom, warm dawn light, and a cracked flat stone slab at the very bottom edge of that scene box for a character to stand on. The LOWER ONE THIRD is an OLD TORN rice-paper sheet with ragged frayed edges, brown "
     "water stains and faded ink smudges, divided into SIX equal horizontal rows by "
     "uneven hand-drawn brush lines, the FIRST row tinted deeper tan as a header. "
     "All areas EMPTY, no text, no letters, no characters, no icons, wuxia mobile game art"),
    ("win_side_sa", 560, 1008, False,
     "a CARTOON WUXIA game UI side panel, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a vertical panel framed by CRACKED "
     "SPLIT old walnut wood beams with deep grain cracks and chipped corners, bound at "
     "the corners with coarse HEMP ROPE lashings, absolutely NO metal nails. The UPPER TWO THIRDS of the panel (a large tall scene box) "
     "shows a rough unorthodox-sect scene: a rugged bandit "
     "camp at a dusty desert canyon gorge with torn banners, broken weapon racks and "
     "campfire smoke at harsh sunset, and a cracked flat stone slab at its bottom edge "
     "at the very bottom edge of that scene box for a character to stand on. The LOWER ONE THIRD is an OLD TORN rice-paper sheet with ragged frayed edges, "
     "brown water stains and faded ink smudges, divided into SIX equal horizontal rows "
     "by uneven hand-drawn brush lines, the FIRST row tinted deeper tan as a header. "
     "All areas EMPTY, no text, no letters, no characters, no icons, wuxia mobile game art"),
    ("win_side_ma", 560, 1008, False,
     "a CARTOON WUXIA game UI side panel, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a vertical panel framed by CRACKED "
     "SPLIT old walnut wood beams with deep grain cracks and chipped corners, bound at "
     "the corners with coarse HEMP ROPE lashings, absolutely NO metal nails. The UPPER TWO THIRDS of the panel (a large tall scene box) "
     "shows a sinister demonic-cult scene: a dark blood-"
     "stained stone shrine with crimson banners, black candles, bone totems and eerie red "
     "mist under a blood moon, and a cracked flat stone slab at its bottom edge around 52 "
     "at the very bottom edge of that scene box for a character to stand on. The LOWER ONE THIRD is an OLD TORN rice-paper sheet with ragged frayed edges, dark "
     "stains and faded ink smudges, divided into SIX equal horizontal rows by uneven "
     "hand-drawn brush lines, the FIRST row tinted deeper tan as a header. All areas "
     "EMPTY, no text, no letters, no characters, no icons, wuxia mobile game art"),
    ("win_char_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI main window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by FOUR separate "
     "CRACKED SPLIT old walnut wood beams with deep grain cracks and chipped edges; the "
     "beams simply CROSS OVER each other at the corners and are held together ONLY by "
     "coarse HEMP ROPE lashings wrapped around the crossings — absolutely NO corner "
     "brackets, NO metal joints, NO square corner plates, NO metal nails, NO bamboo; "
     "the inner "
     "surface is OLD STAINED rice paper with ragged edges, brown tea stains and a faint "
     "ink mountain watermark. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK "
     "hanging from two short hemp ropes across the top from 88 to 99 percent height; "
     "then THREE equal HANGING SCROLLS side by side from 46 to 80 percent height — each "
     "scroll is an aged unrolled paper scroll with a dark wooden rod at its top and bottom, "
     "hanging from a thin hemp cord, its surface EMPTY aged parchment, and a small worn "
     "wooden round button plate drawn INSIDE near the scroll bottom; then ONE WIDE "
     "horizontally unrolled parchment banner from 6 to 42 percent height spanning almost "
     "the full width with wooden rods at its left and right ends and slightly curled "
     "edges, its surface divided into THREE equal horizontal writing rows by faint brush "
     "lines, each row having a tiny worn wooden button plate at its right end. All zones "
     "EMPTY, no text, no letters, no icons, no characters, oriental wuxia mobile game art"),
    # ---- 장비창 (참고작 무기창 구조: 좌 상세 / 우 등급 그리드) ----
    ("win_equip_side", 560, 1008, False,
     "a CARTOON WUXIA game UI side panel, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a vertical panel framed by four "
     "CRACKED SPLIT old walnut wood beams that simply cross at the corners and are held "
     "ONLY by coarse HEMP ROPE lashings — no metal brackets, no nails. The inner surface "
     "is OLD STAINED torn rice paper. Clearly drawn EMPTY zones stacked vertically: a "
     "square item display frame of dark weathered wood at 74 to 94 percent height "
     "centered; a slim torn paper name strip at 66 to 72 percent height; a small aged "
     "paper block at 56 to 64 percent height; then a taller torn parchment area from 26 "
     "to 54 percent height divided into FOUR equal horizontal rows by faint brush lines; "
     "then TWO wide weathered wooden button plates stacked at 13 to 24 percent and 2 to "
     "12 percent height. All zones EMPTY, no text, no letters, no icons, no characters, "
     "oriental wuxia mobile game art"),
    ("win_equip_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI main window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams that cross at the corners and are held ONLY by coarse "
     "HEMP ROPE lashings — no metal brackets, no nails, no bamboo. The inner surface is "
     "OLD STAINED torn rice paper with a faint ink mountain watermark. Clearly drawn "
     "EMPTY zones: a weathered dark wood TITLE PLANK hanging from two short hemp ropes "
     "across the top from 88 to 99 percent height; below it a horizontal row of SIX small "
     "aged wooden tab plates from 78 to 86 percent height, each tab plate COMPLETELY "
     "BLANK with absolutely NO numbers, NO digits, NO glyphs, NO carved marks; "
     "then a large torn parchment "
     "sheet from 16 to 75 percent height containing a clean grid of 5 columns by 4 rows "
     "of empty square cells outlined with faint brown brush lines; then TWO wide "
     "weathered wooden button plates side by side at 3 to 13 percent height. All zones "
     "EMPTY, no text, no letters, no icons, no characters, oriental wuxia mobile game art"),
    # 캐릭터창 통짜 1장 (유저 확정): 두 창을 따로 오려 붙이지 않고 한 이미지로
    ("win_char_full", 1600, 896, False,
     "a CARTOON village NOTICE BOARD for a mobile game UI, with EXACTLY FIVE blank paper sheets "
     "tacked onto it in this precise layout: (1) a TALL portrait sheet on the LEFT from 12 to 78 "
     "percent height and 6 to 32 percent width; (2) a TALL sheet in the CENTER from 12 to 78 percent "
     "height and 35 to 54 percent width, carrying eight faint horizontal ruled lines; (3) on the "
     "RIGHT exactly THREE BROAD CHUNKY vertical sheets of identical size standing side by side, each "
     "about 11 percent of the image width with only a narrow gap between them, all three from 46 to "
     "78 percent height and together spanning 57 to 94 percent width; (4) ONE wide sheet directly "
     "below those three, from 12 to 40 percent height and 57 to 94 percent width. That is five sheets "
     "in total and no others. Every single sheet is completely blank and empty parchment, each "
     "pressed flat against the wood and held by one FLAT ROUND METAL THUMBTACK at its top edge, each "
     "tilted a couple of playful degrees, drawn with chunky torn deckled edges, one big curled "
     "corner, warm-tea stains and a soft cartoon drop shadow. The board behind them is ONE SINGLE "
     "ENORMOUS SLAB of dusty muted brown-mauve timber, one continuous piece of wood, NOT planks, NO "
     "plank seams, with deep chunky grain, long dry cracks, knots and worn rounded chipped edges. "
     "Lying flat across the very top and bolted onto that slab is a thick chunky split-wood "
     "horizontal beam spanning the full width, and nailed flat against the middle of that beam with "
     "four fat round studs is a chunky wooden title plank whose bare face is completely blank and "
     "empty. The beam is bare and unadorned along its whole length. The ENTIRE board is fully visible "
     "inside the frame, complete and uncropped, covering about 88 percent of the picture. Around its "
     "edges peeks a soft, heavily blurred, out-of-focus ORIENTAL WUXIA VILLAGE at warm dusk: curved "
     "tiled rooftops, paper lanterns and misty layered mountains, painted so hazily that the board "
     "stays the clear focus. Style: cel shaded cartoon, clean bold simplified chunky shapes, soft "
     "painterly brush shading, MUTED palette of dusty brown-mauve timber, faded terracotta shadows "
     "and soft oatmeal paper, warm but low saturation, soft diffused light, cozy storybook mood. no "
     "text, no letters, no numbers, no words, no icons, no symbols, no buttons, no characters, no "
     "figures, no drawings."),
    ("char_sheet_hero", 512, 800, True,
     "ONE single sheet of old cream paper NAILED to a wall, portrait orientation, standing alone as one object: softly TORN deckled edges, one curled bottom corner, tea stains and fold creases, a small round metal NAIL head pinning the sheet near its TOP CENTER and a tiny second nail at the top-left corner, and a soft drop shadow falling behind the sheet. Painted ON the sheet: the TOP fifth is left COMPLETELY EMPTY blank paper; the rest is filled by a soft faded ink-wash scene, and in front of that scene ONE FULL-BODY human figure drawn as a FLAT SOLID SILHOUETTE with no facial features and no inner detail, standing upright facing the viewer, the whole body inside the sheet with clear margin above the head and below the feet. cartoon fantasy mobile game art, cel shaded. no text, no letters, no numbers, no UI, no frame, no border. The silhouette is a righteous martial hero in flowing robes holding a straight sword at his side, calm upright stance, dark slate blue-grey. The faded scene behind is a misty mountain temple with pines and stone steps in pale blue-grey ink wash. " + CHROMA_BG),
    ("char_sheet_bowmaster", 512, 800, True,
     "ONE single sheet of old cream paper NAILED to a wall, portrait orientation, standing alone as one object: softly TORN deckled edges, one curled bottom corner, tea stains and fold creases, a small round metal NAIL head pinning the sheet near its TOP CENTER and a tiny second nail at the top-left corner, and a soft drop shadow falling behind the sheet. Painted ON the sheet: the TOP fifth is left COMPLETELY EMPTY blank paper; the rest is filled by a soft faded ink-wash scene, and in front of that scene ONE FULL-BODY human figure drawn as a FLAT SOLID SILHOUETTE with no facial features and no inner detail, standing upright facing the viewer, the whole body inside the sheet with clear margin above the head and below the feet. cartoon fantasy mobile game art, cel shaded. no text, no letters, no numbers, no UI, no frame, no border. The silhouette is a rogue assassin in a hooded short robe with a curved dagger held reversed, low ready stance, dark warm brown. The faded scene behind is a rough cliffside camp with torn banners and dead trees under a dull amber sky. " + CHROMA_BG),
    ("char_sheet_archmage", 512, 800, True,
     "ONE single sheet of old cream paper NAILED to a wall, portrait orientation, standing alone as one object: softly TORN deckled edges, one curled bottom corner, tea stains and fold creases, a small round metal NAIL head pinning the sheet near its TOP CENTER and a tiny second nail at the top-left corner, and a soft drop shadow falling behind the sheet. Painted ON the sheet: the TOP fifth is left COMPLETELY EMPTY blank paper; the rest is filled by a soft faded ink-wash scene, and in front of that scene ONE FULL-BODY human figure drawn as a FLAT SOLID SILHOUETTE with no facial features and no inner detail, standing upright facing the viewer, the whole body inside the sheet with clear margin above the head and below the feet. cartoon fantasy mobile game art, cel shaded. no text, no letters, no numbers, no UI, no frame, no border. The silhouette is a demonic cult sorcerer in a long heavy robe with wide sleeves holding a gnarled staff, imposing wide stance, near-black with a faint crimson rim. The faded scene behind is blood-red fog over a ruined stone altar with hanging chains. " + CHROMA_BG),
    ("win_offline", 1008, 768, False,
     "a CARTOON WUXIA game UI popup window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams crossing at the corners, held ONLY by coarse HEMP ROPE "
     "lashings — no metal brackets, no nails. The inner surface is OLD STAINED torn rice "
     "paper. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK hanging from "
     "two short hemp ropes across the top from 84 to 98 percent height; then TWO large "
     "equal square reward slots side by side from 44 to 76 percent height, each a "
     "recessed aged paper slot with a thin dark wood border, both COMPLETELY EMPTY; then "
     "one long horizontal recessed groove bar from 28 to 38 percent height; then TWO wide "
     "weathered wooden button plates side by side at 6 to 20 percent height. All zones "
     "EMPTY, no text, no letters, no numbers, no icons, oriental wuxia mobile game art"),
    ("plank_label", 512, 192, True,
     "ONE single weathered dark walnut wooden sign plank with cracked grain and chipped "
     "edges, hanging from two short coarse hemp ropes tied at its top corners, no metal "
     "nails, the plank face COMPLETELY EMPTY with no text and no carving, cartoon wuxia "
     "game UI asset, cel shaded with bold outlines. " + CHROMA_BG),
    ("win_list_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI list window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams crossing at the corners, held ONLY by coarse HEMP ROPE "
     "lashings — no metal brackets, no nails. The inner surface is OLD STAINED torn rice "
     "paper. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK hanging from "
     "two short hemp ropes across the top from 88 to 99 percent height; then SEVEN equal "
     "wide horizontal list strips stacked vertically from 16 to 84 percent height, each "
     "strip an aged paper row with a thin dark wood border and a small square emblem "
     "socket at its left end, all strips COMPLETELY EMPTY; then TWO wide weathered wooden "
     "button plates side by side at 3 to 13 percent height. All zones EMPTY, no text, no "
     "letters, no numbers, no icons, oriental wuxia mobile game art"),
    ("win_comp_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI roster window, cel shaded with bold outlines, WEATHERED "
     "and RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams crossing at the corners, held ONLY by coarse HEMP ROPE "
     "lashings — no metal brackets, no nails. The inner surface is OLD STAINED torn rice "
     "paper. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK hanging from "
     "two short hemp ropes across the top from 88 to 99 percent height; then TWELVE equal "
     "TALL PORTRAIT card frames arranged in a grid of 4 columns by 3 rows from 14 to 84 "
     "percent height, each card a vertical aged paper portrait frame with a thin dark "
     "wood border and a small weathered wooden name plate at its bottom edge, all cards "
     "COMPLETELY EMPTY; then one wide weathered wooden info plate at 3 to 12 percent "
     "height. All zones EMPTY, no text, no letters, no numbers, no icons, no portraits, "
     "no characters, oriental wuxia mobile game art"),
    ("win_shop_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI shop window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams crossing at the corners, held ONLY by coarse HEMP ROPE "
     "lashings — no metal brackets, no nails. The inner surface is OLD STAINED torn rice "
     "paper. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK hanging from "
     "two short hemp ropes across the top from 88 to 99 percent height; then SIX equal "
     "market stall product plaques arranged in a grid of 3 columns by 2 rows from 18 to "
     "84 percent height, each plaque an aged paper card with a thin dark wood border and "
     "a small weathered wooden price plate drawn at its bottom edge, all plaques "
     "COMPLETELY EMPTY; then one wide weathered wooden info plate at 4 to 14 percent "
     "height. All zones EMPTY, no text, no letters, no numbers, no icons, no goods, "
     "oriental wuxia mobile game art"),
    ("win_skill_main", 1360, 1008, False,
     "a CARTOON WUXIA game UI main window, cel shaded with bold outlines, WEATHERED and "
     "RUSTIC, filling the entire image edge to edge: a window framed by four CRACKED "
     "SPLIT old walnut wood beams that cross at the corners and are held ONLY by coarse "
     "HEMP ROPE lashings — no metal brackets, no nails. The inner surface is OLD STAINED "
     "torn rice paper. Clearly drawn EMPTY zones: a weathered dark wood TITLE PLANK "
     "hanging from two short hemp ropes across the top from 88 to 99 percent height; "
     "then a large dark wooden BOOKCASE from 18 to 82 percent height and 12 to 88 percent "
     "width, built from cracked planks, with TWO wide horizontal open shelf compartments "
     "separated by one thick plank shelf board in the middle, both compartments "
     "COMPLETELY EMPTY with a slightly darker back wall (no books, nothing inside); "
     "then TWO wide weathered wooden button plates side by side at 4 to 14 percent "
     "height. All zones EMPTY, no text, no letters, no icons, no characters, oriental "
     "wuxia mobile game art"),
    # v3: 메이플키우기 캐릭터창 구조 분석 반영 (전체화면판, 참고용 보관) —
    # 좌=캐릭터 무대+스탯 목록, 우=등급바+3강화카드+특별능력치 그리드+버튼
    ("bg_char", 1600, 896, False,
     "a colorful CARTOON style mobile game character screen BACKGROUND LAYOUT, cel "
     "shaded with bold clean outlines, bright saturated colors, fills the entire image: "
     "a THIN warm brown wood border frame about 20 pixels thick along the outer edges. "
     "Background is a bright jade-teal oriental sky with cartoon clouds and roof tiles "
     "at the top. Clearly drawn EMPTY zones: "
     "on the LEFT from 4 to 36 percent width: the TOP HALF from 48 to 95 percent height "
     "is COMPLETELY EMPTY OPEN SKY with a small floating grass platform sitting at its "
     "very bottom around 48 to 56 percent height (big empty space above the platform for "
     "a character to stand), and BELOW that a cream paper panel from 5 to 44 percent "
     "height with a chunky brown outline divided into SIX equal horizontal rows by thin "
     "brown lines; "
     "on the RIGHT from 40 to 96 percent width: a slim horizontal cream bar at 84 to 92 "
     "percent height, then THREE equal cream cards side by side from 56 to 80 percent "
     "height, then one big cream panel from 16 to 52 percent height clearly divided into "
     "a grid of 2 columns by 3 rows of equal empty cells with visible brown dividing "
     "lines, then a slim empty strip at 4 to 13 percent height. All zones are EMPTY, no "
     "icons, no text, no letters, no characters, playful mobile game art"),
    ("bg_equip", 1600, 896, False,
     "a colorful CARTOON style mobile game inventory screen BACKGROUND LAYOUT, cel "
     "shaded with bold clean outlines, bright saturated colors, fills the entire image: "
     "a THIN carved warm brown wood border frame only about 20 pixels thick along the "
     "outer edges. The interior is a bright jade-teal oriental courtyard wall with "
     "cheerful cartoon clouds and cartoon roof tiles at the very top. Clearly drawn "
     "EMPTY zones: a wide horizontal cream paper tab bar strip across the top from 4 to "
     "62 percent width and 82 to 92 percent height; below it a BIG cream paper panel "
     "from 4 to 62 percent width and 8 to 78 percent height with a chunky brown outline, "
     "and inside it a clean grid of 7 columns by 5 rows of empty square cells drawn with "
     "thin brown lines; on the RIGHT a tall cream paper detail panel from 65 to 96 "
     "percent width and 8 to 92 percent height with a chunky brown outline. All zones "
     "are EMPTY, no icons, no text, no letters, no items, playful mobile game art"),
    # 아이콘 샘플 (스타일 후보 — 승인 후 세트 확장)
    ("ico_sword", 320, 320, True,
     "ONE game item icon: an elegant Chinese jian straight sword with a dark red "
     "tassel, slightly tilted, hand-painted semi-realistic fantasy style with warm "
     "rim light, rich colors, crisp silhouette, high detail AAA mobile game icon, "
     "no background, no text. " + CHROMA_BG),
    ("ico_companion", 320, 320, True,
     "ONE game icon: a mysterious wuxia martial artist silhouette bust wearing a "
     "bamboo hat, dark teal robe, painted semi-realistic fantasy style with warm rim "
     "light, crisp silhouette, high detail AAA mobile game icon, no background, "
     "no text. " + CHROMA_BG),
    ("ico_chest", 320, 320, True,
     "ONE game icon: an ornate dark lacquered oriental treasure chest with antique "
     "gold fittings, slightly open with warm golden glow inside, painted semi-"
     "realistic fantasy style, crisp silhouette, high detail AAA mobile game icon, "
     "no background, no text. " + CHROMA_BG),
    ("panel_dg", 768, 768, False,
     "a square game UI panel filling the ENTIRE image edge to edge: very dark warm "
     "charcoal surface, almost black with a slight warm brown tint, a THIN elegant "
     "antique gold border line just inside the edge with subtle oriental corner "
     "accents, flat premium minimalist AAA style, completely empty center, no text"),
    ("ground_ring", 768, 256, True,
     "a faint elegant antique gold circular ornament ring drawn on the ground seen from "
     "a low angle so it forms a wide flat ellipse, thin oriental line ornament, subtle "
     "soft glow, like a game character standing marker, no text. " + CHROMA_BG),
    # 최종 배경 v3 (유저 확정: 풍경도 기각 — 거의 단색 다크 + 희미한 문양, 참고=AC재이드/오딘)
    ("screen_dark", 1600, 896, False,
     "a premium dark game UI background: near-solid very dark warm charcoal, almost "
     "black with a slight warm brown tint, ONE very faint large thin-line antique gold "
     "circular oriental diagram ornament on the center-left side (barely visible, 8 "
     "percent opacity feel), extremely subtle ink mist at the bottom corners, strong "
     "soft dark vignette at all edges, minimal and clean, mostly uniform dark empty "
     "space for UI content, high-end AAA quality, no text, no letters, fills the "
     "entire image edge to edge"),
    # 분위기 풍경 배경 v2 (유저 확정: 흰 종이 기각, 참고=요령사 캐릭터창 대나무 숲)
    ("screen_scene", 1600, 896, False,
     "a moody atmospheric hand-painted wuxia game background: a dark bamboo forest at "
     "dusk, deep green and teal tones, soft mist between the bamboo stalks, one warm "
     "glowing paper lantern on a wooden post at the middle left, faint stone path at the "
     "bottom, fireflies, the LEFT third more open and dim for a standing character, the "
     "RIGHT two-thirds darker and simpler for UI panels, cinematic soft light, "
     "high detail 2D illustration, no text, no letters, fills the entire image edge to "
     "edge"),
    ("screen_paper", 1600, 896, False,
     "STRICT LIMITED PALETTE: soft warm ivory, pale cream hanji paper, very light warm "
     "gray, faint muted gold accents. NO dark colors, NO saturated colors, NO blue. "
     "an elegant full-screen game UI background of soft warm ivory hanji mulberry paper, "
     "subtle paper fiber texture, very faint ink-wash cloud and mountain line ornaments "
     "near the corners only, a faint large decorative circular seal watermark on the "
     "left side, mostly EMPTY clean space for UI content, flat, high-end minimalist "
     "oriental game UI background, no text, no letters, fills the entire image edge "
     "to edge"),
    ("bg_ink", 1280, 720, False,
     PALETTE + "a soft warm full-screen background: light tan wooden wall with subtle "
     "plank seams and faint vine shadows, gentle warm beige, low contrast, "
     "empty center area for UI content"),
]

def generate(prompt, seed, w, h):
    body = json.dumps({"prompt": prompt, "width": w, "height": h, "seed": seed}).encode()
    req = urllib.request.Request(SERVER, data=body, headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=900) as r:
        data = r.read()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("not a png: " + data[:200].decode(errors="replace"))
    return Image.open(io.BytesIO(data)).convert("RGBA")


def chroma_remove(im, tol=260):
    """마젠타 제거 + 디프린지 — 경계 안티앨리어스 픽셀의 분홍 물듦까지 지운다."""
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            d = abs(r - 255) + g + abs(b - 255)
            if d < tol:
                px[x, y] = (0, 0, 0, 0)
            elif r > g + 60 and b > g + 60:
                # 마젠타 기운이 남은 경계 픽셀: 분홍끼를 빼고 반투명으로
                m = min(r, b) - g
                px[x, y] = (max(0, r - m), g, max(0, b - m), max(0, a - m))
    return im


def main():
    only = sys.argv[1] if len(sys.argv) > 1 else None
    os.makedirs(OUT, exist_ok=True)
    for i, (name, w, h, chroma, prompt) in enumerate(ITEMS):
        if only and only not in name:
            continue
        print("[gen]", name, flush=True)
        im = generate(prompt, seed=7100 + i, w=w, h=h)
        if chroma:
            im = chroma_remove(im)
            bbox = im.getbbox()
            if bbox: im = im.crop(bbox)   # 투명 여백을 남기면 9-slice가 빈 판이 된다
        im.save(os.path.join(OUT, name + ".png"))
        print("[ok]", name, flush=True)
    print("done")


if __name__ == "__main__":
    main()
