# Mientras arda la vela — Guía de montaje del Sprint 1

Desarrollador: Fortino Romero Mantilla · Motor: Unity 6 LTS (2D, URP) · Sprint 1: 19/09/2026 – 09/10/2026

Fuente de verdad: `RomeroMantillaFortino_diseño.docx` (v1.1) y `RomeroMantillaFortino_producción.xlsx`.
Cada tarea de abajo corresponde a una fila del **Sprint 1 Backlog**. Al terminarla, cambia su Estado a
"Terminado" y registra el esfuerzo restante del día.

> ## ✅ ESTE MONTAJE YA ESTÁ HECHO (2026-09-27)
>
> Las secciones 1 a 4 están completas y **automatizadas**: las dos escenas se generan con
> código, no se montan a mano. Este archivo se conserva como **especificación** — es contra
> lo que comprueban los verificadores — pero ya no hay que seguirlo paso a paso.
>
> - Reconstruir: menú **Mientras arda la vela → Construir jerarquía Mapa 0 / Construir MainMenu**
> - Comprobar: `./tools/unity.sh todo`
> - Estado completo y desviaciones: **`ESTADO.md`**
>
> Cada sección lleva abajo una nota de cómo quedó.

---

## 1. Crear el proyecto (F01)

> ✅ **Hecho.** Proyecto creado sobre Unity 6000.6.0f1 con la plantilla Universal 2D,
> Git inicializado y `Assets/_Proyecto` en su sitio. **Falta el `git push`**: no se ha
> dado la URL del repositorio.

1. Unity Hub → *New project* → Unity 6 LTS → plantilla **Universal 2D** → nombre `MientrasArdaLaVela`.
2. Cierra Unity y copia dentro del proyecto la carpeta `Assets/_Proyecto` y el archivo `.gitignore` de este paquete.
3. Inicializa Git y sube a GitHub:
   ```
   git init
   git add .
   git commit -m "Sprint 1: estructura y scripts base"
   git branch -M main
   git remote add origin https://github.com/<tu-usuario>/MientrasArdaLaVela.git
   git push -u origin main
   ```

## 2. Paquetes
> ✅ **Hecho.** Input System 1.19, 2D Tilemap + Extras, uGUI/TMP 2.6, URP 17.6,
> Test Framework y Yarn Spinner 3.2.8 (registro OpenUPM). *Active Input Handling* en
> Input System Package (New). **Cinemachine quedó en 6.6.0, no 3.1.7**: Unity 6.6 la trae
> integrada y tiene prioridad; es la misma línea renumerada y la API coincide.


En *Window → Package Manager* verifica o instala:

| Paquete | Uso |
|---|---|
| Input System | Controles (al instalarlo, acepta reiniciar; en *Project Settings → Player → Active Input Handling* elige **Input System Package (New)** o **Both**) |
| Cinemachine (3.x) | Cámara que sigue a Ámbar |
| 2D Tilemap Editor y 2D Tilemap Extras | Mapas |
| TextMeshPro / uGUI | Textos (en Unity 6 viene con uGUI; importa *TMP Essential Resources* cuando lo pida) |
| Yarn Spinner para Unity | Diálogos. Sigue la guía oficial de instalación en docs.yarnspinner.dev (Package Manager o Asset Store) |

Si la consola muestra errores de compilación antes de instalar Yarn Spinner, es normal: `DialogueBridge.cs` depende de él.

## 3. Ajustes de pixel art
> ✅ **Hecho y automatizado.** `Editor/PixelArtImporter.cs` aplica PPU 32, Point y sin
> compresión a todo lo que entre en `Art/`, más `maxTextureSize 4096` (sin eso Unity
> reescalaría las hojas LPC de 832×3456 y rompería la rejilla de 64 px). Capa
> **Interactable** creada en el índice 8.


- *Project Settings → Graphics / URP*: el renderer del template ya es **2D Renderer** (necesario para Light2D).
- Para cada sprite/tileset importado: **Pixels Per Unit = 32**, **Filter Mode = Point (no filter)**, **Compression = None**.
- *Project Settings → Tags and Layers*: crea la capa **Interactable**.

## 4. Escenas
> ✅ **Hecho.** Las dos escenas existen y están en el Build en el orden correcto
> (MainMenu primero). **No se montaron a mano**: las generan `Editor/Mapa0Builder.cs` y
> `Editor/MainMenuBuilder.cs`, y `Editor/Mapa0Verificador.cs` y `MainMenuVerificador.cs`
> comprueban ~150 detalles de las secciones 4.1 y 4.2 de este archivo.
>
> Dos diferencias respecto a lo escrito abajo:
> - **4.2.2 la luz global está en 0.14.** Bajada el 2026-09-24 porque con los tilesets
>   definitivos, que son claros, a 0.25 no se lee como noche. Ya registrado en el Documento
>   de Diseño v1.2 y corregido abajo.
> - **4.2.5 el prefab de Yarn Spinner ya no se arrastra a mano**: el constructor lo
>   instancia, le asigna el Yarn Project y le añade el `DialogueBridge`.


Crea en `Assets/_Proyecto/Scenes`: `MainMenu` y `Mapa0_Ofrenda`. Agrégalas en *File → Build Profiles → Scene List*
en ese orden (MainMenu primero). Los nombres deben coincidir con `SceneNames.cs`.

### 4.1 MainMenu (F09)
1. Canvas (Screen Space – Overlay, *Canvas Scaler*: Scale With Screen Size 1920×1080) + EventSystem
   (en el EventSystem usa **Input System UI Input Module**).
2. Paneles hijos: `TitlePanel` (título + "Presiona cualquier tecla"), `MenuPanel` (botones Nueva partida, Continuar,
   Opciones, Créditos, Salir), `CreditsPanel` (texto de CREDITOS.txt + botón Volver).
3. Objeto vacío `MainMenu` con el script **MainMenu**; arrastra los paneles, el botón Continuar y Nueva partida como *First Selected*.
4. En cada botón → *OnClick* → objeto MainMenu → `OnNewGame`, `OnContinue`, `OnOptions`, `OnCredits`, `OnQuit`, `OnBack`.
5. Objeto vacío `GameManager` con el script **GameManager**.

### 4.2 Mapa0_Ofrenda (F02–F08, F10)
1. **Grid** con Tilemaps: `Suelo`, `Paredes` (con *Tilemap Collider 2D* + *Composite Collider 2D*, Rigidbody2D Static),
   `Muebles` (con collider), `Decoracion` (sin collider). Zonas: sala con ofrenda, cocina y patio (Figura 8 del documento).
2. **Global Light 2D** con intensidad 0.14 (noche). *(Era 0.25; cambiado en el Documento de Diseño v1.2.)*
3. **Ámbar**: sprite/Animator + `Rigidbody2D` (Dynamic, Gravity 0, Interpolate) + `CapsuleCollider2D` pequeño a los pies +
   **PlayerMovement** + **PlayerInteractor** (Interactable Mask = Interactable; Prompt = un TMP World Space con "[E] Examinar") +
   hijo `Farol` con **Light2D** (Point, color ámbar) y en Ámbar el script **CandleLight** apuntando a esa luz.
   - Animator: parámetros float `MoveX`, `MoveY`, `Speed`; Blend Tree 2D *Idle* y *Walk* con las 4 direcciones;
     transición Idle→Walk si Speed > 0.1.
4. **Cámara**: Main Camera con *Pixel Perfect Camera* (Assets PPU 32, Reference Resolution 640×360) +
   *Cinemachine Brain*; objeto *Cinemachine Camera* siguiendo a Ámbar con *Cinemachine Confiner 2D* (PolygonCollider2D con el borde del mapa).
5. **Dialogue System**: arrastra el prefab de Yarn Spinner (*Dialogue System*), asigna el Yarn Project que contiene
   `Mapa0_Ofrenda.yarn` y agrega al mismo objeto el script **DialogueBridge**.
6. **MemoryMap** (objeto vacío): mapId `M0`, prefijo `M0_`, nombre "La ofrenda — 2025", requeridos 4, siguiente escena `Recuerdo1_Panaderia`.
7. **Fragmentos** (capa Interactable, collider *Is Trigger*), script **MemoryFragment**:

   | Objeto | fragmentId | yarnNode | ¿Oculto? |
   |---|---|---|---|
   | Fotografía en la ofrenda | `M0_foto` | `M0_Frag_Foto` | No |
   | Delantal en la cocina | `M0_delantal` | `M0_Frag_Delantal` | No |
   | Camino de pétalos (patio) | `M0_petalos` | `M0_Frag_Petalos` | Detrás de niebla |
   | Sombrero de palma de niño | `M0_sombrero` | `M0_Frag_Sombrero` | Sí: agrega **Revealable** (revealId `M0_sombrero_rev`) |

8. **Niebla del Olvido** sobre la entrada al patio: sprite gris semitransparente (o Particle System) + BoxCollider2D (no trigger)
   + **OblivionFog** (Unlock By = Fragment, unlockId = `M0_delantal`).
9. **Memento** (pan de muerto de dos piezas en la ofrenda) con **Memento**: lockedNode `M0_Memento_Bloqueado`,
   activateNode `M0_Memento_Activar`, glow = un hijo con brillo.
10. **Lucía** dormida: **DialogueInteractable** con yarnNode `M0_Lucia`.
11. **DialogueTrigger**s: al iniciar (`M0_Prologo`, Play On Start, saveId `M0_prologo`); cerca del sombrero oculto
    (`M0_TutorialVela`, saveId `M0_tut_vela`); frente a la niebla (`M0_TutorialNiebla`, saveId `M0_tut_niebla`).
12. HUD: Canvas con **FragmentCounterUI** (nombre del mapa, "0/4" y 4 imágenes de rombo).

## 5. Prueba de aceptación del Sprint 1 (Pruebas Alfa)
> **Jugada el 2026-09-27: los puntos 1 a 6 pasan** (Fortino llegó a 4/4 fragmentos, lo que
> exige la Luz de vela para el sombrero y la niebla disipada para los pétalos).
> **Falta confirmar el punto 7** (salir y elegir Continuar).
> Los hallazgos están en `Docs/BUGLIST_SPRINT1.md`: 13 registrados, **13 corregidos** (2026-09-27).


- [ ] Desde MainMenu: cualquier tecla → menú; Nueva partida carga la ofrenda y se reproduce el prólogo.
- [ ] Ámbar camina en 4 direcciones con teclado y gamepad, no atraviesa paredes, la cámara no sale del mapa.
- [ ] Aparece "[E] Examinar" junto a los objetos; E, Espacio o clic abren el diálogo; el jugador queda congelado durante el diálogo.
- [ ] Con F el farol crece y revela el sombrero.
- [ ] Al reunir el delantal, la niebla del patio se disipa.
- [ ] El contador llega a 4/4 y el pan de muerto dice "[E] Recordar".
- [ ] Al salir y elegir Continuar, se conservan fragmentos, niebla disipada y mapa.

Todo error encontrado se registra en la hoja **Buglist** (BugID, descripción, descripción técnica, FeatureID, sprint 1).
El archivo de guardado está en `%USERPROFILE%\AppData\LocalLow\<Company>\MientrasArdaLaVela\partida.json`.
