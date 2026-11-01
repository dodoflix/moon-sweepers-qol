#!/usr/bin/env python3
"""Draw assets/logo.png (the README and Nexus page image) and assets/banner.png (the 1300x372 page header).

Chunky rounded lettering on a flat slate plate, after the game's own wordmark. Drawn at
twice the size and scaled down once, so edges are antialiased without a blur pass. Needs
Pillow; Fredoka (OFL) ships in assets/fonts.
"""

import os

from PIL import Image, ImageDraw, ImageFont

S = 2

BG = (42, 50, 61)
PLATE = (21, 26, 33)
WHITE = (238, 240, 243)
ORANGE = (242, 140, 40)
CHIP = (57, 68, 82)
CHIP_TEXT = (216, 222, 230)

CHIPS = ("Scanner", "Belts", "Machines", "Robots")

HERE = os.path.dirname(os.path.abspath(__file__))


def font(size, weight):
    f = ImageFont.truetype(os.path.join(HERE, "fonts", "Fredoka.ttf"), round(size * S))
    f.set_variation_by_axes([weight, 100])
    return f


def centred(d, cx, cy, text, f, fill, outline=0, plate=PLATE):
    """Text centred on (cx, cy) in 1x coordinates, with an optional outline plate."""
    d.text((cx * S, cy * S), text, font=f, fill=fill, anchor="mm", stroke_width=outline * S, stroke_fill=plate)


def chips(d, cy, height, size, gap):
    """A row of pills, each one a plain label."""
    f = font(size, 500)
    pad = height * 0.7
    widths = [d.textlength(label, font=f) / S + 2 * pad for label in CHIPS]
    x = (d.im.size[0] / S - (sum(widths) + gap * (len(CHIPS) - 1))) / 2
    for label, w in zip(CHIPS, widths):
        d.rounded_rectangle([x * S, (cy - height / 2) * S, (x + w) * S, (cy + height / 2) * S], radius=height / 2 * S, fill=CHIP)
        centred(d, x + w / 2, cy, label, f, CHIP_TEXT)
        x += w + gap


def lockup(w, h, kicker_y, title_y, title_size, outline, chip_y, chip_h, chip_size, chip_gap):
    img = Image.new("RGB", (w * S, h * S), BG)
    d = ImageDraw.Draw(img)
    centred(d, w / 2, kicker_y, "MOON SWEEPERS", font(title_size * 0.3, 600), ORANGE)
    centred(d, w / 2, title_y, "Quality of Life", font(title_size, 700), WHITE, outline)
    if chip_y:
        chips(d, chip_y, chip_h, chip_size, chip_gap)
    return img.resize((w, h), Image.LANCZOS)


def main():
    lockup(1280, 720, 258, 355, 150, 8, 495, 64, 30, 18).save(os.path.join(HERE, "logo.png"))
    # Nexus draws the mod title and breadcrumbs over the bottom of the header, so the lettering
    # stays in the top half and there are no chips here.
    lockup(1300, 372, 72, 140, 112, 7, None, 0, 0, 0).save(os.path.join(HERE, "banner.png"))


main()
