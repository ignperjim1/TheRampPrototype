---
name: project-brief-tecnico
description: Restricciones técnicas duras del prototipo y assets disponibles
metadata:
  type: project
---

**Restricciones duras (no negociables del brief):**
- Unity **2022.3.62f2** exacto (LTS).
- Prohibido usar assets de la Asset Store.
- Prohibido usar plugins externos (no DOTween, no Cinemachine de terceros, etc. — los packages oficiales de Unity sí cuentan como Unity, no como plugin externo, pero conviene confirmar con el usuario antes de añadir cualquiera).
- Único asset visual permitido: pack Kenney "mini-skate" (https://opengameart.org/content/mini-skate) — ya importado en `Assets/Models/KenneyModels/` y `Assets/kenney_mini-skate/`.
- Control: ratón (el revisor juega en editor).

**Ubicación del proyecto:** `C:\Viva Games\TheRampPrototype\` (NO `c:\Unity\TheRampPrototype\` — ese fue un primer intento con Built-in que se descartó).

**Estado actual del proyecto (2026-05-27, fin de sesión):**
- Proyecto creado desde el template oficial **3D (URP) Core** de Unity Hub → URP 14.0.12 configurado.
- Pack Kenney **mini-skate** importado en `Assets/ThirdParty/kenney_mini-skate/`.
- Cinemachine **descartado** (ya quitado del manifest). Cámara fija + `CameraFeedback` — ver [[project-camera-feedback]].
- Input: **legacy** (`Input.GetMouseButton` + delta manual).
- Estructura `_Project/` con todas las subcarpetas del plan.
- **Días 1-8 + 11 del [[planning]] hechos**:
  - Días 1-5: setup, movimiento péndulo (`SkaterController` + `SpeedMaintainer`), tricks encadenables (`SwipeDetector` + `TrickPerformer` + 4 `TrickDefinition` SOs + chain buffer 0.1s), cámara fija con shake+dolly-zoom al iniciar trick, evaluación clean/fail, `ScoreManager`, HUD con `TrickPopup`.
  - Días 6-7: 4 trucos balanceados (Up/Down/Left/Right), popup ajustado a layout vertical (nombre + score en dos líneas).
  - Día 8 (esta sesión): juicy visual completo — `Hitstop` (0.08s freeze al clean landing), `LandingVFX` (burst de chispas+polvo + TrailRenderer 1s post-aterrizaje). Ragdoll mejorado con `AddForce` (Vector3.up * upward + random insideUnitSphere con Y forzada positiva) + desactivación de `SpeedMaintainer` durante respawn para que no secuestre al skate de vuelta al péndulo.
  - Día 11 (esta sesión): post-processing — `PostProcessFeedback` con Vignette dinámica por estado (con dos filtros: ignora Airborne durante `IsRespawning` y durante el primer descenso post-spawn vía `hasTouchedGround`) + pulso de ChromaticAberration en clean landing. Bloom y Tonemapping configurados en el Volume Profile.
- **Arquitectura de eventos** del flujo de trick: ver [[project-trick-events]] (Hitstop, LandingVFX, PostProcessFeedback documentados).
- **Sesión 2026-05-27 cerrada con cambios sin commitear** (a verificar al abrir en el nuevo PC): `SkaterController.cs`, `TrickPopup.cs`, `TrickPopup.prefab` + nuevos `Scripts/Feedback/Hitstop.cs`, `LandingVFX.cs`, `PostProcessFeedback.cs`. Si el usuario commiteó+pusheó antes de cambiar de PC, todo arriba; si no, esos cambios se quedaron en el PC anterior.
- **Siguiente: Día 9 (audio)** — decisión tomada de implementar **audio procedural** sin assets externos. Tabla de SFX redactada en [[project-audio-procedural]]. Después quedan UX del swipe (Día 10), polish/balance (Día 12), buffer (Día 13), build final (Día 14).

**Heads-up al abrir en el nuevo PC:**
- `git pull` primero para traer los cambios pusheados.
- `Library/` se regenera al abrir Unity (no se sincroniza). Primer arranque lento (~minutos) reimportando assets.
- Verificar que los componentes nuevos (Hitstop, LandingVFX, PostProcessFeedback) están en escena con sus refs asignadas — si el usuario configuró los PS/Trail en el PC anterior, las refs viven en la escena y deberían venir con el `.unity` commiteado.
- Si no commiteó la configuración de los PS/Trail antes de cambiar, hay que rehacerla siguiendo los valores de [[project-trick-events]] y el chat anterior (Hemisphere radius 0.35 para sparks, Cone radius 0.3 para dust, TrailRenderer width 0.2→0 color blanco→cyan).

**How to apply:** Antes de añadir packages, materiales nuevos o cambiar render pipeline, confirmar con el usuario; afecta a la estructura del proyecto y al setup del repo.
