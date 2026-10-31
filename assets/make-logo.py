#!/usr/bin/env python3
"""Draw assets/logo.png: the Nexus Mods page image.

Same lockup as the Core Keeper mod: kicker, title, divider, one framed tile per
feature. Drawn at twice the size and scaled down once, so edges are antialiased
without a blur pass. Needs Pillow; Fira Sans (OFL) ships in assets/fonts.
"""

import math
import os
import random

from PIL import Image, ImageDraw, ImageFont

W, H = 1280, 720
S = 2

TOP = (9, 13, 21)
BOTTOM = (23, 34, 49)
MOON = (20, 29, 41)
MOON_RIM = (47, 64, 86)
CRATER = (16, 24, 34)
MINT = (117, 255, 199)
MINT_DIM = (62, 140, 112)
AMBER = (251, 177, 57)
TEXT = (242, 237, 228)
DIM = (140, 156, 176)
FRAME = (58, 76, 100)
FRAME_LIT = (92, 116, 146)
TILE_FILL = (15, 23, 34)
INK = (8, 12, 18)

HERE = os.path.dirname(os.path.abspath(__file__))


def font(size, bold):
    name = "FiraSans-Bold.ttf" if bold else "FiraSans-Medium.ttf"
    return ImageFont.truetype(os.path.join(HERE, "fonts", name), size * S)


def spaced(d, cx, y, text, f, fill, gap):
    """Centred text with extra letter spacing, in 1x coordinates."""
    widths = [d.textlength(ch, font=f) for ch in text]
    total = sum(widths) + gap * S * (len(text) - 1)
    x = cx * S - total / 2
    for ch, w in zip(text, widths):
        d.text((x, y * S), ch, font=f, fill=fill)
        x += w + gap * S


def box(x0, y0, x1, y1):
    return [x0 * S, y0 * S, x1 * S, y1 * S]


def background(w=W, h=H, keep=(100, 140, W - 100, 640)):
    img = Image.new("RGB", (w * S, h * S))
    d = ImageDraw.Draw(img)
    for y in range(h * S):
        t = y / (h * S)
        d.line([(0, y), (w * S, y)], fill=tuple(int(a + (b - a) * t) for a, b in zip(TOP, BOTTOM)))

    # Lunar surface at the bottom edge, with a few craters.
    top = h - 80
    d.ellipse(box(-500, top, w + 500, top + 1260), fill=MOON, outline=MOON_RIM, width=2 * S)
    for x, y, rx, ry in ((180, 50, 70, 12), (470, 72, 48, 8), (900, 60, 90, 14), (1130, 76, 40, 7)):
        if x < w:
            d.ellipse(box(x - rx, top + y - ry, x + rx, top + y + ry), fill=CRATER)

    # Stars, kept off the lockup.
    rng = random.Random(7)
    for _ in range(70):
        x, y = rng.randrange(8, w - 8), rng.randrange(8, h - 100)
        if keep[0] < x < keep[2] and keep[1] < y < keep[3]:
            continue
        r = rng.choice((1, 1, 1, 2))
        colour = rng.choice(((90, 104, 126), (90, 104, 126), (150, 168, 190), MINT_DIM, (170, 120, 50)))
        d.ellipse(box(x - r, y - r, x + r, y + r), fill=colour)
    return img


def tile(d, x, y, size):
    d.rounded_rectangle(box(x - 2, y - 2, x + size + 2, y + size + 2), radius=18 * S, fill=INK)
    d.rounded_rectangle(box(x, y, x + size, y + size), radius=16 * S, fill=TILE_FILL, outline=FRAME, width=2 * S)
    d.line([((x + 18) * S, (y + 1) * S), ((x + size - 18) * S, (y + 1) * S)], fill=FRAME_LIT, width=S)


def scanner(d, cx, cy):
    """Compass ribbon with the nearest landmark pinned in amber."""
    d.rounded_rectangle(box(cx - 52, cy - 22, cx + 52, cy + 14), radius=7 * S, fill=(21, 31, 45), outline=FRAME, width=S)
    for i, x in enumerate(range(cx - 44, cx + 48, 11)):
        d.line([(x * S, (cy - 18) * S), (x * S, (cy - (6 if i % 2 else 11)) * S)], fill=(88, 106, 130), width=S)
    d.polygon([(cx * S, (cy + 24) * S), ((cx - 11) * S, (cy - 2) * S), ((cx + 11) * S, (cy - 2) * S)], fill=AMBER)
    d.polygon([((cx - 36) * S, (cy + 6) * S), ((cx - 29) * S, (cy - 6) * S), ((cx - 22) * S, (cy + 6) * S)], fill=MINT)
    d.polygon([((cx + 22) * S, (cy + 6) * S), ((cx + 29) * S, (cy - 6) * S), ((cx + 36) * S, (cy + 6) * S)], fill=MINT)


def belt(d, cx, cy):
    """Conveyor with a crate riding it."""
    d.rounded_rectangle(box(cx - 50, cy + 2, cx + 50, cy + 26), radius=12 * S, fill=(26, 40, 56), outline=MINT, width=2 * S)
    for x in range(cx - 38, cx + 42, 12):
        d.line([(x * S, (cy + 7) * S), (x * S, (cy + 21) * S)], fill=MINT_DIM, width=2 * S)
    for x in (cx - 38, cx + 38):
        d.ellipse(box(x - 7, cy + 7, x + 7, cy + 21), fill=TILE_FILL, outline=MINT, width=2 * S)
    d.rounded_rectangle(box(cx - 14, cy - 24, cx + 14, cy), radius=3 * S, fill=AMBER)
    d.line([((cx - 14) * S, (cy - 12) * S), ((cx + 14) * S, (cy - 12) * S)], fill=(160, 100, 20), width=2 * S)
    d.polygon([((cx + 24) * S, (cy - 8) * S), ((cx + 40) * S, (cy - 8) * S), ((cx + 40) * S, (cy - 14) * S), ((cx + 52) * S, (cy - 4) * S), ((cx + 40) * S, (cy + 6 - 2) * S), ((cx + 40) * S, (cy - 0) * S), ((cx + 24) * S, (cy - 0) * S)], fill=MINT_DIM)


def machine(d, cx, cy):
    """Gear for the presses and chambers."""
    teeth, outer, inner = 10, 44, 34
    pts = []
    for i in range(teeth):
        a = 2 * math.pi * i / teeth
        for da, r in ((-0.20, inner), (-0.12, outer), (0.12, outer), (0.20, inner)):
            pts.append(((cx + math.cos(a + da) * r) * S, (cy + math.sin(a + da) * r) * S))
    d.polygon(pts, fill=MINT)
    d.ellipse(box(cx - 24, cy - 24, cx + 24, cy + 24), fill=TILE_FILL)
    d.ellipse(box(cx - 14, cy - 14, cx + 14, cy + 14), fill=AMBER)
    d.ellipse(box(cx - 6, cy - 6, cx + 6, cy + 6), fill=TILE_FILL)


def robot(d, cx, cy):
    """Robot vacuum seen from above."""
    d.ellipse(box(cx - 46, cy - 46, cx + 46, cy + 46), fill=(26, 40, 56), outline=MINT, width=3 * S)
    d.ellipse(box(cx - 30, cy - 30, cx + 30, cy + 30), outline=MINT_DIM, width=2 * S)
    d.ellipse(box(cx - 12, cy - 12, cx + 12, cy + 12), fill=AMBER)
    for a in (-0.9, 0.9):
        x, y = cx + math.sin(a) * 40, cy - math.cos(a) * 40
        d.line([(cx + math.sin(a) * 30) * S, (cy - math.cos(a) * 30) * S, x * S, y * S], fill=MINT, width=3 * S)
        d.ellipse(box(x - 5, y - 5, x + 5, y + 5), fill=MINT)


def main():
    img = background()
    d = ImageDraw.Draw(img)

    spaced(d, W / 2, 160, "MOON SWEEPERS", font(30, False), DIM, 14)
    spaced(d, W / 2, 196, "QUALITY OF LIFE", font(104, True), TEXT, 7)

    # Divider with an amber diamond in the middle.
    y = 372
    d.line([(290 * S, y * S), (610 * S, y * S)], fill=MINT, width=2 * S)
    d.line([(670 * S, y * S), (990 * S, y * S)], fill=MINT, width=2 * S)
    d.polygon([(640 * S, (y - 10) * S), (650 * S, y * S), (640 * S, (y + 10) * S), (630 * S, y * S)], fill=AMBER)

    size, gap = 150, 36
    x0 = (W - (4 * size + 3 * gap)) // 2
    cap = font(20, False)
    for i, (icon, label) in enumerate(((scanner, "SCANNER"), (belt, "BELTS"), (machine, "MACHINES"), (robot, "ROBOTS"))):
        x = x0 + i * (size + gap)
        tile(d, x, 420, size)
        icon(d, x + size // 2, 420 + size // 2)
        spaced(d, x + size / 2, 590, label, cap, DIM, 5)

    img.resize((W, H), Image.LANCZOS).save(os.path.join(HERE, "logo.png"))


def banner():
    """The 1300x372 header strip: title on the left, the tiles on the right."""
    bw, bh = 1300, 372
    img = background(bw, bh, keep=(40, 60, bw - 40, 300))
    d = ImageDraw.Draw(img)

    spaced(d, 355, 119, "MOON SWEEPERS", font(24, False), DIM, 12)
    spaced(d, 355, 151, "QUALITY OF LIFE", font(66, True), TEXT, 4)

    size, gap = 124, 18
    x0 = bw - 40 - (4 * size + 3 * gap)
    cap = font(15, False)
    for i, (icon, label) in enumerate(((scanner, "SCANNER"), (belt, "BELTS"), (machine, "MACHINES"), (robot, "ROBOTS"))):
        x = x0 + i * (size + gap)
        tile(d, x, 108, size)
        icon(d, x + size // 2, 108 + size // 2)
        spaced(d, x + size / 2, 246, label, cap, DIM, 3)

    img.resize((bw, bh), Image.LANCZOS).save(os.path.join(HERE, "banner.png"))


main()
banner()
