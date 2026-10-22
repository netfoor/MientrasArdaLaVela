# Montón de leña (32x32), arte original.
from PIL import Image, ImageDraw
im=Image.new('RGBA',(32,32),(0,0,0,0)); d=ImageDraw.Draw(im)
troncos=[(4,20),(13,20),(22,20),(8,13),(17,13),(12,6)]
for x,y in troncos:
    d.ellipse([x,y,x+8,y+8],fill=(120,72,40,255),outline=(60,36,20,255))
    d.ellipse([x+2,y+2,x+6,y+6],fill=(196,150,96,255)); d.point((x+4,y+4),fill=(150,100,60,255))
d.line([(2,29),(30,29)],fill=(40,28,20,255))
im.save('lena.png'); print('ok')
