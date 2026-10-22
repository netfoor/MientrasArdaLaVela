# Buglist — Sprint 3

| BugID | Descripción | Descripción técnica (archivo/línea) | FeatureID | Severidad | Estado |
|---|---|---|---|---|---|
| B17 | Se atraviesan paredes (reportado en el Recuerdo 3; probablemente en todos los mapas construidos en batchmode). | El `CompositeCollider2D` de *Paredes* se guardaba con `m_CompositePaths` vacío: la geometría nunca se generaba al construir la escena desde la línea de comandos. Ahora `ConstructorEscena.GuardarYRegistrar` y `Mapa0Builder` llaman a `ColisionesTilemap.Regenerar` antes de guardar, el Grid lleva el componente `ColisionesTilemap` (reconstruye los colliders al cargar) y los verificadores exigen que el composite tenga contornos. | F19 | Alta | Corregido (sin confirmar) |
