# Personajes con el generador LPC (Sprint 1, tarea F01 "Descargar e importar recursos abiertos")

Generador: *Universal LPC Spritesheet Character Generator* (repositorio LiberatedPixelCup en GitHub; tiene versión web en GitHub Pages).
Por cada personaje: arma las capas, exporta el PNG completo y **copia el texto de créditos/licencias que muestra el generador** a `CREDITOS.txt`.
Guarda los PNG en `Assets/_Proyecto/Art/Personajes/`. Solo necesitamos la animación **walk** (y la pose 0 como reposo).

| Personaje | Cuerpo | Cabello/rostro | Ropa | Notas del Documento de Diseño |
|---|---|---|---|---|
| Ámbar (19 años) | Femenino, piel morena | Trenza larga negra | Blusa/vestido, capa o chal morado (rebozo), sandalias | Farol: se dibuja aparte o se usa la Light2D |
| Don Aurelio (espíritu, 79) | Masculino, complexión robusta | Cabello canoso corto, bigote blanco | Camisa clara, delantal, sombrero (si hay) | En juego se vuelve translúcido con color gris-azulado |
| Aurelio 69 / 42 / 20 / 9 | Mismo personaje con variantes | Canoso → negro; versión niño con cuerpo *child* | Delantal (69, 20), traje (42 boda) | Recuerdos 1 a 4 |
| Refugio "Cuco" (12, 23, 77) | Masculino delgado; *child* para 12 | Cabello negro despeinado; canoso para el epílogo | Camisa y pantalón sencillos | Recuerdos 3 y 4, epílogo |
| Rosa (40) | Femenino | Trenza recogida | Vestido azul | Recuerdo 2 |
| Lucía (9 y 19) | *Child* / femenino | Cabello corto | Overol (9) / sudadera (19) | Mapa 0 y Recuerdo 1 |
| Don Ernesto | Masculino | Bigote negro | Delantal y gorro de panadero | Recuerdo 3 |
| Ecos | Duplicar el sprite del personaje | — | — | Recolorear en gris al 60 % de opacidad desde Unity (SpriteRenderer.color) |

**Tizne (xoloitzcuintle):** el generador no incluye animales; busca en OpenGameArt "LPC dog" o "LPC animals" y recolorea en negro.

**Tilesets (Sprint 1 para la ofrenda; Sprint 2 para la panadería):** busca en OpenGameArt "LPC interior", "LPC house insides",
"LPC tile atlas" y "LPC flowers". Prioriza licencias CC0 o CC-BY; anota autor y licencia en `CREDITOS.txt` al descargar.
