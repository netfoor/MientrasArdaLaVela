# Contexto del proyecto (leer antes de cualquier cambio)

> **Estado actual: `ESTADO.md`.** Léelo primero: dice qué está implementado, qué falta
> y las desviaciones conscientes respecto al Documento de Diseño. Resumen al 2026-10-23: el
> juego se puede jugar de principio a fin; **F01–F25 están hechas** y F26 **ya compila**
> (`Builds/Windows/`). Lo que falta es teclear los tres códigos secretos y probar el .exe en un
> segundo equipo. El arte propio se dibuja con los scripts de `tools/arte/` (un script por
> tanda: objetos, camino de pétalos, campo, calle, Marisol); si tocas uno, vuelve a construir
> la escena que lo usa y **mira la captura**: varios defectos de esta tanda sólo se veían ahí.
> No confíes en el Feature Log del Excel para saber el estado: sus hojas de resumen tienen
> valores cacheados sin recalcular.

**Juego:** *Mientras arda la vela* — aventura narrativa 2D top-down (estilo *To the Moon*) ambientada en Día de Muertos.
**Autor:** Fortino Romero Mantilla (UTP). **Motor:** Unity 6 LTS, URP 2D Renderer, C#.
**Proceso:** Huddle (Preproducción → 3 sprints → Beta → Postmortem).

## Fuentes de verdad
- Documento de Diseño: `RomeroMantillaFortino_diseño.docx` (secciones: mecánica, controles, estados, niveles, guion, música).
- Plan de producción: `RomeroMantillaFortino_producción.xlsx` (Feature Log F01–F26, Sprint Backlogs, Buglist).
Si algo del código contradice el Documento de Diseño, gana el documento; avisa antes de cambiar el diseño.

## Arquitectura de juego (Assets/_Proyecto/Scripts, namespace `MientrasArdaLaVela`)
- `Core/GameManager` singleton persistente con `SaveData`; eventos `FragmentCollected`, `EchoResolved`, `MapCompleted`.
- `Core/SaveSystem` JSON en `Application.persistentDataPath/partida.json`.
- `Core/GameInput` acciones del Input System definidas en código (WASD/flechas/stick, E/Espacio/clic, F, H, Tab, Esc).
- `Core/DialogueBridge` único acceso a Yarn Spinner (`Play(node, onComplete)`, `IsBusy`).
- `Core/MemoryMap` configuración por escena (mapId, fragmentPrefix, requiredFragments, nextScene).
- `Player/` PlayerMovement (Rigidbody2D.linearVelocity, 4 direcciones, `Locked`), PlayerInteractor, CandleLight.
- `Interaction/` Interactable (base), DialogueInteractable, DialogueTrigger.
- `Memory/` MemoryFragment, Revealable, OblivionFog, Echo, Memento.
- `UI/` MainMenu, FragmentCounterUI, TizneHintUI.
- `Companion/Tizne` sigue a Ámbar por su estela; Olfatear con H (20 s de espera), `TryUseHint()` para el puzle.
- `Puzzle/MosaicPuzzle` + `MosaicPiece` puzle 4 × 4 de rotación; `Open(imagen, título, mapId, alResolver, alCancelar)`.
- `Core/SceneTransition` fundido a blanco entre recuerdos (`SceneTransition.Go(escena)`), se crea solo.
- Flujo del memento: activateNode → mosaico → `CompleteMap` (autoguarda) → solvedNode → transición a `nextScene`.
- `UI/InterfazGlobal` se crea sola en tiempo de ejecución (no la montan los constructores): pausa (Esc),
  Diario (Tab), Opciones y la vela de progreso. Congela con `Time.timeScale = 0`.
- `Core/Diario` es el texto del Diario: **al construir un recuerdo nuevo, añade ahí sus fragmentos y Ecos**
  (el verificador del recuerdo lo exige). `Core/Ajustes` guarda las opciones en PlayerPrefs.
- `Core/Musica` música por escena con fundido cruzado (`Musica.Tocar`, `Temporal`/`Restaurar` para el
  mosaico, comando `<<musica M08>>`). Piezas en `Audio/Resources/Musica/`, generadas por `tools/arte/musica.py`.
- `Core/ColisionesTilemap` en el Grid: reconstruye los colliders al cargar (B17). Todo constructor nuevo
  debe llamar a `ConstructorEscena.GuardarYRegistrar`, que genera la geometría antes de guardar.
- `Core/Sonidos.Play("S03", vol, tono)` efectos de un disparo desde `Audio/Resources/Audio/`
  (sintetizados con `tools/arte/sonidos*.py`); cuatro fuentes en rueda y tono variable. **Si falta
  el clip no suena nada y no avisa**: por eso `SonidosTests` comprueba que exista todo lo invocado.
  Los niveles de la mezcla están en una sola tabla, en la cabecera de `Sonidos.cs`: si regeneras un
  clip, mide su RMS y rehaz su ganancia.
- `Core/Ambiente` lecho en bucle por escena (S10, la vela del menú, la ofrenda y el epílogo); se
  crea solo, como la música. `Core/SonidoTexto` es el tic por letra (S05): se engancha a la máquina
  de escribir de Yarn con un `ActionMarkupHandler` y lo registra `ConstructorEscena.TicDeTexto`,
  que **todo constructor con diálogo debe llamar** (el del Mapa 0 también).
- `Cinematica/CinematicCommands` comandos de Yarn para cinemáticas en el motor: `mover`, `caminar`, `mirar`,
  `aparecer`, `desaparecer`, `esperar`, `fundido`, `abrir`, `vineta`, `quitar_vineta`,
  `camara`, `camara_punto`, `camara_jugador`, `sonido`, `fragmento`.
- `OblivionFog` modo `Candle`: niebla que se abre al acercar la Luz de vela (Recuerdo 3).
  `Cinematica/CinematicActor` en cada personaje que actúa (id sin espacios, p. ej. `Aurelio69`).

## Herramientas de editor (Scripts/Editor, namespace `MientrasArdaLaVela.EditorTools`)
Las escenas **se generan con código, no se montan a mano**. Si cambias el trazado o el
contenido de una escena, cambia su constructor y vuelve a ejecutarlo; no edites la
escena a mano, o el siguiente que corra el constructor te lo pisa.

- `Mapa0Builder` / `MainMenuBuilder` construyen cada escena (idempotentes, con Undo).
- `ConstructorEscena` son las piezas comunes para los recuerdos (Ámbar, Tizne, cámara, NPC, Eco,
  niebla, memento, HUD, mosaico, diálogo). `Recuerdo1Builder` la usa; el Mapa 0 conserva su
  constructor propio porque ya está verificado. Ojo: `Tizne`, `Memento` y `MemoryMap` son también
  tipos, así que desde otro constructor se llaman como `ConstructorEscena.Tizne(...)`.
- `Mapa0Verificador` / `MainMenuVerificador` / `AmbarAnimatorVerificador` comprueban el
  resultado contra `LEEME_SETUP.md`. **Si tocas una escena, corre su verificador.**
- `ConstructorEscena.Objeto("frag_lata.png")` carga el arte propio de `Art/Objetos` (lo dibuja
  `tools/arte/objetos_f25.py`) y avisa si falta, cayendo en un cuadro de color. **Ningún
  fragmento, memento ni rombo debe volver a quedarse con el relleno.**
- `ConstructorEscena.TileObjeto("camino_denso.png")` convierte una pieza de `Art/Objetos` en un
  Tile sin collider: así se pinta con arte propio una capa de decoración. Lo usa el camino de
  pétalos del Mapa 0. Si redibujas esos tiles, acuérdate de que el Mapa 0 y el epílogo son de
  noche: con poca luz, los tonos apagados desaparecen sobre la duela.
- `Build.Windows` / `Build.Linux` compilan tomando las escenas de Build Profiles; fallan si la
  primera no es MainMenu. No cambies `productName`: la ruta de la partida guardada se forma con él.
- `AmbarAnimator`, `HojaLPC` (corta hojas LPC), `HojaTizne` (corta el perro), `TilesetLPC` (recorta tiles),
  `PixelArtImporter`, `PlaceholderArt`, `ProyectoSetup`.
- `Mapa0Captura` / `MainMenuCaptura` renderizan las escenas a PNG: sirven para revisar
  sin abrir el editor.

Todo cuelga del menú **Mientras arda la vela**.

## Cómo comprobar tu trabajo
```bash
./tools/unity.sh todo            # verificadores + pruebas EditMode
./tools/unity.sh capturar        # renderiza las escenas a PNG y míralas
./tools/unity.sh build-windows   # compila la entrega (F26); build-linux para probar aquí
```
Requiere el editor **cerrado**: Unity no admite dos instancias sobre el mismo proyecto.
No des por bueno un cambio de escena sin correr el verificador o mirar la captura.

## Convenciones
- IDs de fragmentos con prefijo del mapa: `M0_`, `R1_`, `R2_`, `R3_`, `R4_`. Nodos Yarn: `<MAPA>_<Tipo>_<Nombre>`.
- Unity 6: usar `linearVelocity`, `FindFirstObjectByType`, Input System (nunca `UnityEngine.Input`).
- Pixel art 32 px por unidad; resolución de referencia 640×360.
- Comentarios y textos en español. Sin combate ni armas.
- No mencionar herramientas de asistencia en textos del juego, créditos ni documentos de entrega.
- Cada cambio debe indicar qué tarea del Sprint Backlog (FeatureID) avanza.

## Trampas conocidas de este proyecto
- **Reload Domain está desactivado**: los campos `static` sobreviven entre partidas en el
  editor. Todo estático necesita reinicio explícito (`RuntimeInitializeOnLoadMethod` u
  `OnDestroy`). Ya mordió cuatro veces.
- **`Undo.SetTransformParent` conserva la escala de MUNDO.** Bajo un Canvas eso infla la
  escala del hijo. Normaliza `localScale` después de emparentar.
- **En Yarn Spinner 3.x `StartDialogue` es asíncrono** y `IsDialogueRunning` tarda en
  ponerse a true; no lo uses para bloquear al jugador en el mismo cuadro.
- **El MainMenu copia `CREDITOS.txt` dentro de la escena.** Si tocas los créditos, corre
  `./tools/unity.sh construir-menu` o el verificador del menú fallará (pasó el 2026-09-27).
- **Unity vacía `Temp/` al arrancar**: no guardes ahí nada que quieras conservar.
- Registra cada defecto que encuentres en la buglist del sprint en curso (`Docs/BUGLIST_SPRINT1.md`
  para el 1) con archivo y línea, y cópialo a la hoja Buglist del Excel.
- **La E es a la vez Interactuar y "girar a la derecha" en el puzle.** El puzle ignora la entrada
  del cuadro en que se abre; conserva esa guarda si tocas el flujo del memento.
- Imágenes de mosaico: 256 × 256 (64 px de dibujo × 4) en `Art/Mosaicos/`, cada cuarto con algo
  asimétrico o la pieza no se puede orientar. El script que dibujó la del pan está en `tools/arte/`.
