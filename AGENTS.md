# Contexto del proyecto (leer antes de cualquier cambio)

**Juego:** *Mientras arda la vela* — aventura narrativa 2D top-down (estilo *To the Moon*) ambientada en Día de Muertos.
**Autor:** Fortino Romero Mantilla (UTP). **Motor:** Unity 6 LTS, URP 2D Renderer, C#.
**Proceso:** Huddle (Preproducción → 3 sprints → Beta → Postmortem).

## Fuentes de verdad
- Documento de Diseño: `RomeroMantillaFortino_diseño.docx` (secciones: mecánica, controles, estados, niveles, guion, música).
- Plan de producción: `RomeroMantillaFortino_producción.xlsx` (Feature Log F01–F26, Sprint Backlogs, Buglist).
Si algo del código contradice el Documento de Diseño, gana el documento; avisa antes de cambiar el diseño.

## Arquitectura existente (Assets/_Proyecto/Scripts, namespace `MientrasArdaLaVela`)
- `Core/GameManager` singleton persistente con `SaveData`; eventos `FragmentCollected`, `EchoResolved`, `MapCompleted`.
- `Core/SaveSystem` JSON en `Application.persistentDataPath/partida.json`.
- `Core/GameInput` acciones del Input System definidas en código (WASD/flechas/stick, E/Espacio/clic, F, H, Tab, Esc).
- `Core/DialogueBridge` único acceso a Yarn Spinner (`Play(node, onComplete)`, `IsBusy`).
- `Core/MemoryMap` configuración por escena (mapId, fragmentPrefix, requiredFragments, nextScene).
- `Player/` PlayerMovement (Rigidbody2D.linearVelocity, 4 direcciones, `Locked`), PlayerInteractor, CandleLight.
- `Interaction/` Interactable (base), DialogueInteractable, DialogueTrigger.
- `Memory/` MemoryFragment, Revealable, OblivionFog, Echo, Memento.
- `UI/` MainMenu, FragmentCounterUI.

## Convenciones
- IDs de fragmentos con prefijo del mapa: `M0_`, `R1_`, `R2_`, `R3_`, `R4_`. Nodos Yarn: `<MAPA>_<Tipo>_<Nombre>`.
- Unity 6: usar `linearVelocity`, `FindFirstObjectByType`, Input System (nunca `UnityEngine.Input`).
- Pixel art 32 px por unidad; resolución de referencia 640×360.
- Comentarios y textos en español. Sin combate ni armas.
- No mencionar herramientas de asistencia en textos del juego, créditos ni documentos de entrega.
- Cada cambio debe indicar qué tarea del Sprint Backlog (FeatureID) avanza.
