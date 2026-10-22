# Banderitas de papel picado, 32x32, con el cordel arriba. Arte original del proyecto.
from PIL import Image, ImageDraw
colores={'rosa':(232,74,120),'naranja':(250,160,40),'verde':(80,180,110),'azul':(70,140,230),'morado':(160,90,210)}
for nombre,c in colores.items():
    im=Image.new('RGBA',(32,32),(0,0,0,0)); d=ImageDraw.Draw(im)
    d.line([(0,3),(31,3)],fill=(60,45,40,255))
    osc=tuple(int(v*0.75) for v in c)
    d.polygon([(4,4),(27,4),(27,22),(24,25),(21,22),(18,25),(15,22),(12,25),(9,22),(6,25),(4,22)],fill=c+(255,))
    # calado: calaverita y flores
    d.ellipse([12,8,19,15],fill=(0,0,0,0)); d.rectangle([14,15,17,17],fill=(0,0,0,0))
    d.point((14,11),fill=c+(255,)); d.point((17,11),fill=c+(255,))
    for (x,y) in [(7,8),(24,8),(7,18),(24,18)]: d.rectangle([x-1,y-1,x+1,y+1],fill=(0,0,0,0))
    d.line([(4,4),(4,22)],fill=osc+(255,)); 
    im.save(f'papel_picado_{nombre}.png')
print('ok')
