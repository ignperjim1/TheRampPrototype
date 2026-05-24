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

**Estado actual del proyecto:**
- Proyecto creado desde el template oficial **3D (URP) Core** de Unity Hub → URP ya viene configurado (URPAsset asignado en Graphics y Quality, materiales del sample en URP/Lit).
- Pack Kenney **mini-skate** ya importado en `Assets/ThirdParty/Kenney/` por el usuario.
- Cinemachine: **pendiente añadir** (package oficial de Unity, decisión tomada de usarlo).
- Input: **legacy** (`Input.GetMouseButton` + delta manual), decisión tomada.
- Resto de estructura `_Project/` (Art, Audio, Prefabs, Scenes, Scripts, Settings, VFX) → pendiente de crear.

**How to apply:** Antes de añadir packages, materiales nuevos o cambiar render pipeline, confirmar con el usuario; afecta a la estructura del proyecto y al setup del repo.
