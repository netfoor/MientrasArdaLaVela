# Contexto del proyecto (leer antes de cualquier cambio)

> **Estado actual: `ESTADO.md`.** Léelo primero: dice qué está implementado, qué falta
> y las desviaciones conscientes respecto al Documento de Diseño. El Sprint 1 (F01–F10)
> está hecho y jugado; el Sprint 2 no se ha empezado.

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
- `UI/` MainMenu, FragmentCounterUI.

## Herramientas de editor (Scripts/Editor, namespace `MientrasArdaLaVela.EditorTools`)
Las escenas **se generan con código, no se montan a mano**. Si cambias el trazado o el
contenido de una escena, cambia su constructor y vuelve a ejecutarlo; no edites la
escena a mano, o el siguiente que corra el constructor te lo pisa.

- `Mapa0Builder` / `MainMenuBuilder` construyen cada escena (idempotentes, con Undo).
- `Mapa0Verificador` / `MainMenuVerificador` / `AmbarAnimatorVerificador` comprueban el
  resultado contra `LEEME_SETUP.md`. **Si tocas una escena, corre su verificador.**
- `AmbarAnimator`, `HojaLPC` (corta hojas LPC), `TilesetLPC` (recorta tiles),
  `PixelArtImporter`, `PlaceholderArt`, `ProyectoSetup`.
- `Mapa0Captura` / `MainMenuCaptura` renderizan las escenas a PNG: sirven para revisar
  sin abrir el editor.

Todo cuelga del menú **Mientras arda la vela**.

## Cómo comprobar tu trabajo
```bash
./tools/unity.sh todo      # verificadores + pruebas EditMode
./tools/unity.sh capturar  # renderiza las escenas a PNG y míralas
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
- **Unity vacía `Temp/` al arrancar**: no guardes ahí nada que quieras conservar.
- Registra cada defecto que encuentres en `Docs/BUGLIST_SPRINT1.md` con archivo y línea.
