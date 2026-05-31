# The Ramp — Prototipo

Realizado por Ignacio Pérez-Aguirre Jiménez.
Duración: 1 semana - aprox. 20-25 horas.
Prototipo de skate sobre una rampa grande, inspirado en el juego de Steam *The Ramp*. El skate va de un lado a otro de forma automática y salta solo en los bordes; el jugador solo se concentra en hacer swipes en el aire para ejecutar trucos con buen timing.
Este prototipo ha sido realizado con ayuda de Claude Code como pair programmer.

Requisitos:
Prueba técnica de 2 semanas. Foco: UX, game feel y "juicy".
- Sin assets de la Asset Store.
- Sin plugins externos. Solo packages oficiales de Unity.
- Único asset visual: pack Kenney *mini-skate*.
- Control con ratón (simulando dedo).
- Skate sin personaje, una rampa, movimiento y salto automáticos.

----

## Desglose de actividad

Día	    Actividad (tiempos aproximados)
24/05	Sesión nocturna corta — setup y planning (1h)
25/05	1 sesión de tarde — skater + rampas (4h)
27/05	2 sesiones (tarde + noche) — tricks, cámara, primer UI (5h+3h)
28/05	1 sesión de tarde — juicy visual + post-process (4h)
30/05	1 sesión de tarde — audio procedural (2h)
31/05	Sesión de mañana: polish + README (3h)

----

## Cómo jugar

- El skate se mueve solo entre las dos rampas (oscila como un péndulo).
- Salta de forma automática al llegar al borde de cada rampa.
- En el aire, arrastra el ratón desde el punto donde lo pulsas hacia una de las 4 direcciones (arriba, abajo, izquierda, derecha) para hacer un truco.
- El truco solo puntúa si completas la rotación antes de aterrizar. Si caes a medias → wipeout, ragdoll, respawn, y pierdes el score acumulado (la puntuación máxima se mantiene con PlayerPrefs).
- Puedes encadenar varios trucos en un mismo salto.

---

## Controles

Botón izquierdo del ratón pulsado + arrastrar en una dirección mientras el skate está en el aire.

| Swipe | Truco | Puntos |
|-------|-------|--------|
| ↑ Arriba | Impossible | 150 |
| ↓ Abajo | Kickflip | 100 |
| ← Izquierda | Pop Shove-it | 80 |
| → Derecha | Tre Flip | 220 |

El `BEST` score persiste entre runs (`PlayerPrefs`).

---

## Decisiones técnicas relevantes

- URP con post-processing (Bloom + Vignette dinámica por estado + Chromatic Aberration y Lens Distortion en aterrizaje limpio).
- Cinemachine descartado tras probarlo. Nos quedamos con cámara fija propia (`CameraFeedback.cs`) con shake + dolly-zoom hacia el skate al iniciar truco.
- Físicas tipo péndulo: el skate usa un Rigidbody con velocidad inicial única + un `SpeedMaintainer` que reinyecta velocidad al cruzar el centro del suelo. Sin lógica de movimiento ni de salto scripteada — todo es física.
- Tricks como rotación cosmética: la coroutine de truco rota un hijo `SkateModel`, nunca el Rigidbody raíz, así no interfiere con el péndulo.
- Sistema de eventos: `TrickPerformer` expone 4 eventos (`OnTrickStarted`, `OnTrickCompleted`, `OnTrickFailed`, `OnCleanLanding`) consumidos por cámara, audio, HUD, score y feedback visual.
- Audio 100% procedural: todos los SFX se sintetizan en runtime con `AudioClip.Create` (`SfxSynth.cs`). Sin assets de audio externos.
- Reflection puntual en `PostProcessFeedback` para subir el clamp de `ChromaticAberration.intensity` por encima de 1 en el aterrizaje tras un truco completado. Documentado en el código.
- InputBuffering tanto a la hora de saltar de la rampa como a la hora de encadenar trucos.

---

## Estructura del repo

```
Assets/
├── _Project/             ← todo el código y assets del prototipo
│   ├── Art/              ← materiales, VFX
│   ├── Audio/            ← (vacío: el audio es procedural)
│   ├── Prefabs/          ← Skater, rampa, suelo, UI
│   ├── Scenes/Main.unity ← escena única
│   ├── Scripts/
│   │   ├── Audio/        ← SkaterAudio, SfxSynth
│   │   ├── Camera/       ← CameraFeedback
│   │   ├── Feedback/     ← Hitstop, LandingVFX, PostProcessFeedback
│   │   ├── Game/         ← ScoreManager, SkaterSpawner
│   │   ├── Skater/       ← SkaterController, SpeedMaintainer
│   │   ├── Tricks/       ← TrickPerformer, SwipeDetector, TrickDefinition (SO), SwipeDirection
│   │   └── UI/           ← HUD, TrickPopup
│   ├── Scriptables/      ← TrickDefinition assets (Up/Down/Left/Right)
│   └── Settings/         ← URP profiles + Physic Material del skate
├── ThirdParty/
│   └── kenney_mini-skate/ ← único asset visual permitido por el brief
docs/                     ← decisiones de diseño y arquitectura detalladas
```

---

## Documentación técnica detallada

Las decisiones de diseño (por qué péndulo y no salto scripteado, por qué cámara fija, por qué audio procedural, por qué eventos en vez de managers...) están explicadas en [docs/](docs/). Cada documento incluye iteraciones descartadas, trade-offs y notas para mantenimiento.
