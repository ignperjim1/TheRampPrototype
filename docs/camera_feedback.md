# Cámara y feedback visual

La cámara del prototipo es **fija, una sola Camera nativa de Unity**. Cinemachine fue descartado tras probarlo. Todo el feedback "juicy" de la cámara durante el trick lo gestiona `CameraFeedback` en `Assets/_Project/Scripts/Camera/CameraFeedback.cs`.

## Por qué se descartó Cinemachine

Se probó primero con `CinemachineVirtualCamera` cambiando prioridades entre tres vcams (ground / air / static) vía un `CameraDirector`. Los blends entre vcams eran inestables, daban tropiezos visuales y no aportaban a un encuadre tipo *The Ramp* (cámara fija lateral). Para un prototipo donde el foco debe estar en la rotación del trick, no en cortes de cámara, la cámara fija propia funciona mejor.

## Arquitectura

- `TrickPerformer` expone `event Action<TrickDefinition> OnTrickStarted` y `event Action OnCleanLanding`.
- `CameraFeedback` se suscribe a ambos (vía referencia inyectada por `SkaterSpawner`).
- **Shake y zoom corren en coroutines independientes**; cada una mantiene un offset (`currentShakeOffset`, `currentZoomOffset`) y un `LateUpdate` los combina sobre `basePosition` capturada en `Awake`. Las dos contribuciones no se pisan entre sí.
- **Shake**: 2 muestras de `Mathf.PerlinNoise` desfasadas (X e Y) para movimiento orgánico, modulado por `shakeFalloff` (AnimationCurve).
- **Zoom**: combina cambio de FOV (`zoomDeltaFOV`) + desplazamiento hacia un `focusTarget` opcional (`zoomMoveDistance`) modulado por `zoomShape` (AnimationCurve tipo peak-and-return).

## Dos modos de feedback

| Evento | Método | Efecto |
|---|---|---|
| `OnTrickStarted` | `PlayFeedback()` | Shake corto + dolly-zoom hacia el skate |
| `OnCleanLanding` | `PlayLandingFeedback()` | Shake más fuerte sin zoom (variant `landingShake*` en el inspector) |

## Valores que funcionan bien (dolly-zoom hacia el skate)

- `Focus Target`: el GameObject del Skate (asignado vía Spawner)
- `Zoom Delta FOV`: −2 (zoom-in muy sutil)
- `Zoom Move Distance`: 2 (la cámara se acerca 2 unidades world hacia el skate)
- `Zoom Duration`: 0.45s
- Shake con defaults (amplitude 0.15, freq 25, duration 0.25s)

La combinación FOV ligeramente negativo + acercamiento físico al sujeto evita la sensación rara del zoom-in puro (que hace zoom "al vacío" en medio de la pista) y queda más cinematográfico.

## Notas de mantenimiento

- Si la cámara se queda desplazada tras un trick, verificar que ninguna otra coroutine escribe a `transform.position` durante o después del feedback. El `LateUpdate` de este componente debe ser el último en tocar la transform.
- `basePosition` se captura en `transform.position` (world). Si la cámara cuelga de un padre que se mueve, habría que cambiar a `localPosition` + cálculos en parent space.

Ver también: [Arquitectura de eventos del trick](trick_events.md).
