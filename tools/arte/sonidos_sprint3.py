# Efectos del Sprint 3 (arte sonoro original del proyecto): S12, S13 y S16.
import numpy as np, wave
SR=44100
def guardar(nombre, x, vol=0.8):
    x=x/np.max(np.abs(x))*vol
    with wave.open(nombre,'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x*32767).astype(np.int16).tobytes())
def campana(f, dur, r):
    # parciales inarmónicos típicos de una campana de bronce
    t=np.arange(int(SR*dur))/SR
    parc=[(0.5,0.6,1.3),(1.0,1.0,1.0),(1.19,0.5,0.8),(1.5,0.35,0.6),(2.0,0.4,0.5),(2.74,0.2,0.35),(3.0,0.15,0.3)]
    return sum(a*np.sin(2*np.pi*f*k*t)*np.exp(-t/(r*d)) for k,a,d in parc)*np.minimum(1,t/0.003)
def mezcla(partes, dur):
    out=np.zeros(int(SR*dur))
    for i0,s in partes:
        i=int(SR*i0); out[i:i+len(s)]+=s[:len(out)-i]
    return out
def reverb(x, d=0.11, g=0.4, veces=7):
    out=x.copy()
    for k in range(1,veces+1):
        i=int(SR*d*k); out[i:]+=x[:len(x)-i]*(g**k)
    return out
def lejano(x, n=18):
    return np.convolve(x, np.ones(n)/n, mode='same')     # pasa-bajos: suena a distancia
# S12 campanas lejanas del pueblo (logro desbloqueado)
x=mezcla([(0,campana(392,3.5,1.2)),(0.55,0.8*campana(330,3.2,1.1)),(1.1,0.7*campana(392,2.6,1.0))],4.2)
guardar('S12.wav', reverb(lejano(x),0.13,0.45), 0.55)
# S13 campanilla de puerta (código secreto aceptado)
t=np.arange(int(SR*1.2))/SR
x=mezcla([(0,campana(1760,1.1,0.35)),(0.09,0.8*campana(2093,1.0,0.3)),(0.18,0.6*campana(1760,0.9,0.25))],1.3)
guardar('S13.wav', reverb(x,0.07,0.25,4), 0.6)
# S16 pétalo: pulso suave de marimba, dos notas
def marimba(f,dur):
    t=np.arange(int(SR*dur))/SR
    return (np.sin(2*np.pi*f*t)+0.25*np.sin(2*np.pi*f*4*t)*np.exp(-t/0.03))*np.exp(-t/0.22)*np.minimum(1,t/0.002)
x=mezcla([(0,marimba(784,0.6)),(0.08,0.8*marimba(1175,0.6))],0.8)
guardar('S16.wav', reverb(x,0.08,0.25,3), 0.45)
print('ok')
