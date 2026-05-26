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

**Estado actual del proyecto (2026-05-26, fin de sesión):**
- Proyecto creado desde el template oficial **3D (URP) Core** de Unity Hub → URP 14.0.12 configurado.
- Pack Kenney **mini-skate** importado en `Assets/ThirdParty/kenney_mini-skate/`.
- Cinemachine **descartado** (estaba 2.9.7, ya quitado del manifest). Cámara fija + `CameraFeedback` — ver [[project-camera-feedback]].
- Input: **legacy** (`Input.GetMouseButton` + delta manual), decisión tomada.
- Estructura `_Project/` ya creada con todas las subcarpetas del plan.
- **Días 1-5 del [[planning]] hechos**: setup, movimiento péndulo (`SkaterController` + `SpeedMaintainer`), tricks encadenables (`SwipeDetector` + `TrickPerformer` + 4 `TrickDefinition` SOs + chain buffer 0.1s), cámara fija con shake+dolly-zoom al iniciar trick, evaluación clean/fail, `ScoreManager`, HUD con `TrickPopup` (verde sube/rojo cae) y score con pop. Ragdoll+respawn al fallar (`SkaterController.RagdollAndRespawn`).
- **Arquitectura de eventos** del flujo de trick: ver [[project-trick-events]].
- **Siguiente: Día 6-7** — balancear los 4 trucos existentes (timings, score, axes). Pendiente menor: pulir feel del ragdoll (intensidad de torque, posiblemente bounciness temporal durante caída).

**Próxima sesión cambia de equipo (2026-05-27):**
- Al abrir el repo en la otra máquina, el `Library/` de Unity se regenera solo al abrir el proyecto (no se sincroniza por `.gitignore`). Primer arranque será lento (~minutos) mientras Unity reimporta assets.
- Verificar que el `.gitignore` está respetando lo que debe — no debería haber `Library/`, `Temp/`, `Logs/`, `UserSettings/` en el commit.
- Antes de empezar a trabajar en la nueva máquina: `git pull` para asegurar que tienes lo último.

**How to apply:** Antes de añadir packages, materiales nuevos o cambiar render pipeline, confirmar con el usuario; afecta a la estructura del proyecto y al setup del repo.
