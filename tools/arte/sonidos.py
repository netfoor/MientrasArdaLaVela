# Efectos sintetizados para "Mientras arda la vela" (arte sonoro original del proyecto).
import numpy as np, wave
SR=44100
def env(n, a=0.005, r=0.3):
    t=np.arange(n)/SR; e=np.minimum(1, t/a) * np.exp(-t/r); return e
def tono(f, dur, r=0.3, a=0.005, arm=(1,0.35,0.12)):
    n=int(SR*dur); t=np.arange(n)/SR
    s=sum(amp*np.sin(2*np.pi*f*(k+1)*t) for k,amp in enumerate(arm))
    return s*env(n,a,r)
def mezcla(partes, dur):
    out=np.zeros(int(SR*dur))
    for inicio, señal in partes:
        i=int(SR*inicio); out[i:i+len(señal)]+=señal[:len(out)-i]
    return out
def reverb(x, d=0.09, g=0.35, veces=5):
    out=x.copy()
    for k in range(1,veces+1):
        i=int(SR*d*k); out[i:]+=x[:len(x)-i]*(g**k)
    return out
def guardar(nombre, x, vol=0.8):
    x=x/np.max(np.abs(x))*vol
    with wave.open(nombre,'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x*32767).astype(np.int16).tobytes())
rng=np.random.default_rng(3)
# S02 interacción: clic suave
n=int(SR*0.08); s=tono(1400,0.08,r=0.015,arm=(1,0.2))+0.3*tono(700,0.08,r=0.02,arm=(1,))
guardar('S02.wav', s, 0.4)
# S03 fragmento: destello cristalino ascendente (arpegio de campanitas)
notas=[1046.5,1318.5,1568.0,2093.0]
x=mezcla([(i*0.07, tono(f,1.2,r=0.45,arm=(1,0.0,0.25,0.0,0.08))) for i,f in enumerate(notas)],1.6)
x+=0.15*mezcla([(0.28, tono(3136,0.9,r=0.3,arm=(1,)))],1.6)
guardar('S03.wav', reverb(x), 0.6)
# S04 niebla disipada: soplido de ruido filtrado que sube y se desvanece
n=int(SR*1.4); t=np.arange(n)/SR
ruido=rng.standard_normal(n); ruido=np.convolve(ruido, np.ones(40)/40, mode='same')
e=np.sin(np.pi*np.minimum(t/1.4,1))**2
guardar('S04.wav', ruido*e, 0.35)
# S06 rotar pieza: clic de madera
n=int(SR*0.12); t=np.arange(n)/SR
golpe=np.sin(2*np.pi*320*t)*np.exp(-t/0.018)+0.6*np.sin(2*np.pi*760*t)*np.exp(-t/0.01)+0.3*rng.standard_normal(n)*np.exp(-t/0.004)
guardar('S06.wav', golpe, 0.5)
# S07 mosaico completo: acorde brillante (do mayor) y campanilla
acorde=sum(tono(f,2.8,r=1.1,a=0.02,arm=(1,0.3,0.1)) for f in (523.25,659.25,783.99,1046.5))
x=mezcla([(0,acorde),(0.35,0.5*tono(2093,1.8,r=0.7,arm=(1,0,0.3))),(0.55,0.35*tono(2637,1.5,r=0.6,arm=(1,)))],3.2)
guardar('S07.wav', reverb(x,0.11,0.3), 0.7)
# S08 transición: la vela se aviva con eco (ruido cálido creciente + brillo)
n=int(SR*1.6); t=np.arange(n)/SR
fuego=np.convolve(rng.standard_normal(n), np.ones(90)/90, mode='same')*np.minimum(1,t/0.9)*np.exp(-np.maximum(0,t-1.0)/0.25)
brillo=mezcla([(0.5, tono(1568,1.1,r=0.5,arm=(1,0.2))),(0.62, tono(2349,0.9,r=0.4,arm=(1,)))],1.6)
guardar('S08.wav', reverb(fuego*0.8+0.25*brillo,0.12,0.3), 0.55)
print('ok')
# S11 fuego: crepitar del horno (ruido grave + chasquidos)
n=int(SR*2.5); t=np.arange(n)/SR
base=np.convolve(rng.standard_normal(n), np.ones(200)/200, mode='same')*3
chas=np.zeros(n)
for _ in range(45):
    i=rng.integers(0,n-2000); L=rng.integers(200,900)
    chas[i:i+L]+=rng.standard_normal(L)*np.exp(-np.arange(L)/(L/5))*rng.uniform(0.3,1)
x=(base+chas)*np.minimum(1,t/0.3)*np.minimum(1,(2.5-t)/0.4)
guardar('S11.wav', x, 0.5)
# S15 latido: dos golpes graves (revelación)
def golpe(d):
    n=int(SR*d); t=np.arange(n)/SR
    return np.sin(2*np.pi*55*t)*np.exp(-t/0.09)+0.5*np.sin(2*np.pi*90*t)*np.exp(-t/0.05)
x=mezcla([(0,golpe(0.4)),(0.28,0.75*golpe(0.4)),(1.0,golpe(0.4)),(1.28,0.75*golpe(0.4))],1.9)
guardar('S15.wav', x, 0.8)
print('ok2')
