#!/usr/bin/env python3
"""Draws assets/logo.png: the store image. Needs Pillow and Liberation Sans Bold."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

W, H = 1280, 720
MINT = (117, 255, 199)
AMBER = (251, 177, 57)
INK = (11, 16, 24)
TEXT = (242, 237, 228)
FONT = "/usr/share/fonts/liberation/LiberationSans-Bold.ttf"

img = Image.new("RGB", (W, H), INK)
d = ImageDraw.Draw(img)

# horizon glow, then a moon limb
for y in range(H):
    t = y / H
    d.line([(0, y), (W, y)], fill=(int(11 + 14 * t), int(16 + 22 * t), int(24 + 30 * t)))
d.ellipse([-400, 520, W + 400, 2000], fill=(30, 38, 50))
d.ellipse([-400, 530, W + 400, 2000], fill=(20, 27, 38))

# compass ribbon with ticks and two markers, the nearest one in amber
d.rounded_rectangle([140, 200, W - 140, 290], radius=14, fill=(18, 26, 38), outline=(58, 74, 96), width=2)
for i, x in enumerate(range(170, W - 160, 36)):
    d.line([(x, 205), (x, 220 if i % 4 else 238)], fill=(88, 106, 130), width=2)
def pin(cx, cy, size, colour):
    d.polygon([(cx, cy + size), (cx - size * 0.75, cy - size * 0.6), (cx + size * 0.75, cy - size * 0.6)], fill=colour)
pin(430, 245, 26, MINT)
pin(900, 245, 26, MINT)
pin(650, 245, 34, AMBER)
f = ImageFont.truetype(FONT, 16)
for cx, label in ((430, "412 m"), (650, "198 m"), (900, "731 m")):
    w = d.textlength(label, font=f)
    d.text((cx - w / 2, 300), label, font=f, fill=(199, 212, 230))

big = ImageFont.truetype(FONT, 96)
small = ImageFont.truetype(FONT, 34)
title = "Moon Sweepers QoL"
d.text(((W - d.textlength(title, font=big)) / 2, 400), title, font=big, fill=TEXT)
sub = "Landmark Scanner  -  an upgrade in the Suit branch"
d.text(((W - d.textlength(sub, font=small)) / 2, 530), sub, font=small, fill=MINT)

img.save(Path(__file__).with_name("logo.png"))
