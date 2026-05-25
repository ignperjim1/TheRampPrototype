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

**Estado actual del proyecto (2026-05-25):**
- Proyecto creado desde el template oficial **3D (URP) Core** de Unity Hub → URP 14.0.12 configurado.
- Pack Kenney **mini-skate** importado en `Assets/ThirdParty/kenney_mini-skate/`.
- Cinemachine **2.9.7 ya instalado** en `Packages/manifest.json`.
- Input: **legacy** (`Input.GetMouseButton` + delta manual), decisión tomada.
- Estructura `_Project/` **ya creada** con todas las subcarpetas del plan (Art/{Materials,VFX}, Audio/{Music,SFX}, Prefabs/{Skater,Level,UI}, Scenes, Scripts/{Audio,Camera,Feedback,Game,Skater,Tricks,UI}, Settings).
- `Assets/_Project/Scenes/Main.unity` existe (contenido sin verificar aún).
- `Assets/_Project/Scripts/` vacía de `.cs` — el código del Día 2+ está por escribir.

**How to apply:** Antes de añadir packages, materiales nuevos o cambiar render pipeline, confirmar con el usuario; afecta a la estructura del proyecto y al setup del repo.
