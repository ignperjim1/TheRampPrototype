---
name: feedback-memoria-en-proyecto
description: Toda la memoria persistente debe guardarse dentro del proyecto en .claude/memory/, no en el perfil personal del usuario
metadata:
  type: feedback
---

Guardar memoria SIEMPRE en `<raíz del proyecto>\.claude\memory\` (dentro del repo del proyecto), nunca en `C:\Users\Nacho\.claude\projects\...`. Ruta actual del proyecto: `C:\Viva Games\TheRampPrototype\.claude\memory\`.

**Why:** Durante el sprint de 2 semanas el usuario trabaja desde varios equipos distintos y necesita que el contexto viaje con el proyecto (zip, sync, etc.). Si la memoria vive en el perfil personal de una máquina, se pierde al cambiar de equipo.

**How to apply:** Cualquier `MEMORY.md` o archivo de memoria nuevo va a `.claude/memory/` del proyecto. La carpeta `.claude/` está fuera de `Assets/` así que Unity no la importa, y al estar dentro del repo se sincroniza junto con el código. Aplica también a futuros proyectos si el usuario lo indica.
