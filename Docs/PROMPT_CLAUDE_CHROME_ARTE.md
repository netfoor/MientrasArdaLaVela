# Prompt para Claude en Chrome — Descarga de arte abierto (F01)

> Tarea que **no** se puede hacer desde la terminal: el generador LPC y OpenGameArt necesitan navegador
> (elegir capas en una UI, aceptar licencias y copiar el texto de créditos que genera la página).
> Copia y pega TODO el bloque de abajo en Claude para Chrome.

---

Necesito que me ayudes a reunir arte abierto (pixel art, licencias CC0 / CC-BY / OGA-BY / GPL) para un
videojuego 2D top-down de estilo LPC. Trabaja con cuidado y **no inventes licencias**: copia siempre el
texto literal de créditos que muestra cada sitio.

## Parte A — Personajes con el Universal LPC Spritesheet Character Generator

Abre la versión web del *Universal LPC Spritesheet Character Generator*
(repositorio LiberatedPixelCup/Universal-LPC-Spritesheet-Character-Generator en GitHub; tiene GitHub Pages).

Para **cada** personaje de la tabla: arma las capas indicadas, exporta el PNG completo de la hoja de sprites
con el botón de descarga, y **copia el texto completo de créditos/licencias** que la página muestra debajo
del lienzo. Solo necesito la animación **walk** (la pose 0 de cada fila sirve de reposo), pero si el
generador exporta la hoja universal completa está bien: la recorto yo en Unity.

| Archivo a guardar | Personaje | Cuerpo | Cabello / rostro | Ropa |
|---|---|---|---|---|
| `ambar_walk.png` | Ámbar, 19 años | Femenino, piel morena | Trenza larga negra | Blusa o vestido + capa/chal morado (rebozo) + sandalias |
| `aurelio_espiritu_walk.png` | Don Aurelio, espíritu, 79 | Masculino, complexión robusta | Cabello canoso corto + bigote blanco | Camisa clara + delantal + sombrero si existe |
| `aurelio_69_walk.png` | Aurelio 69 | Masculino | Canoso | Delantal de panadero |
| `aurelio_42_walk.png` | Aurelio 42 | Masculino | Cabello negro | Traje (boda) |
| `aurelio_20_walk.png` | Aurelio 20 | Masculino | Cabello negro | Delantal de panadero |
| `aurelio_9_walk.png` | Aurelio 9 | Cuerpo **child** | Cabello negro | Ropa sencilla de campo |
| `cuco_12_walk.png` | Refugio "Cuco", 12 | Cuerpo **child**, delgado | Cabello negro despeinado | Camisa y pantalón sencillos |
| `cuco_23_walk.png` | Cuco, 23 | Masculino delgado | Cabello negro despeinado | Camisa y pantalón sencillos |
| `cuco_77_walk.png` | Cuco, 77 | Masculino delgado | Canoso | Camisa y pantalón sencillos |
| `rosa_40_walk.png` | Rosa, 40 | Femenino | Trenza recogida | Vestido azul |
| `lucia_9_walk.png` | Lucía, 9 | Cuerpo **child** | Cabello corto | Overol |
| `lucia_19_walk.png` | Lucía, 19 | Femenino | Cabello corto | Sudadera |
| `ernesto_walk.png` | Don Ernesto | Masculino | Bigote negro | Delantal + gorro de panadero |

Si alguna capa exacta no existe en el generador, elige la más parecida y **anótalo** en tu reporte final.

## Parte B — Animal y tilesets en OpenGameArt

Busca en OpenGameArt.org y descarga el mejor resultado de cada punto. **Prioriza CC0; si no, CC-BY / OGA-BY.**
Evita cualquier cosa que sea "solo uso no comercial" o sin licencia clara.

1. **Perro estilo LPC** (para el xoloitzcuintle "Tizne"): busca `LPC dog` y `LPC animals`.
   Guardar como `tizne_dog.png` (lo recoloreo a negro yo).
2. **Interior de casa, estilo LPC 32 px**: busca `LPC interior`, `LPC house insides`, `LPC tile atlas`.
   Necesito suelo, paredes, puertas y muebles de cocina/sala. Guardar en una carpeta `tileset_interior/`.
3. **Flores y pétalos (cempasúchil / naranja)**: busca `LPC flowers`. Guardar en `tileset_flores/`.
4. **Velas, veladoras u objetos de altar** si encuentras algo LPC/32 px. Opcional, guardar en `tileset_altar/`.

## Qué necesito de vuelta

1. Los archivos descargados (déjalos en la carpeta de descargas y dime la ruta exacta de cada uno).
2. Un bloque de texto listo para pegar en `CREDITOS.txt`, con **una entrada por recurso** en este formato:

```
--- <nombre del recurso> ---
Archivo(s): <nombre de archivo>
Autor(es): <tal como aparece en la página>
Licencia: <CC0 / CC-BY 4.0 / OGA-BY 3.0 / GPL 3.0 ...>
Fuente: <URL directa a la página del recurso>
Créditos literales del generador/página:
<pega aquí el texto tal cual>
```

3. Una lista de cualquier cosa que NO encontraste o que sustituiste por algo distinto.

No modifiques ningún archivo de mi computadora fuera de la carpeta de descargas.
