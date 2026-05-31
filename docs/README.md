# Documentación técnica

Decisiones de diseño y arquitectura del prototipo. Cada documento explica **qué se decidió y por qué**, incluyendo alternativas descartadas y notas para mantenimiento.

| Documento | Tema |
|---|---|
| [movement_design.md](movement_design.md) | Por qué el skate se mueve como péndulo físico y no con salto scripteado. Iteraciones descartadas (JumpTrigger, AccelTrigger). |
| [physic_materials.md](physic_materials.md) | Setup del `Skate_Physics` (fricción 0.05, Friction Combine Minimum) para que el skate deslice por los Mesh Colliders facetados de Kenney. |
| [trick_events.md](trick_events.md) | Sistema de 4 eventos en `TrickPerformer` (Started/Completed/Failed/CleanLanding), quién consume cada uno, inyección de dependencias vía `SkaterSpawner`, encadenado mid-coroutine de combos, filtros `hasTouchedGround`. |
| [camera_feedback.md](camera_feedback.md) | Cámara fija propia (Cinemachine descartado), shake + dolly-zoom hacia el skate al iniciar trick, shake fuerte al aterrizar limpio. |
| [audio_procedural.md](audio_procedural.md) | SFX 100% sintetizados con `AudioClip.Create`. Sin audio externo. Decisiones por SFX. |

Para visión general del juego y cómo jugarlo, ver el [README principal](../README.md) del repositorio.
