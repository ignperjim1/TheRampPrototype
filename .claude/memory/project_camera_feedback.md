---
name: project-camera-feedback
description: Cámara fija (sin Cinemachine) + CameraFeedback con shake y dolly-zoom al iniciar trick
metadata:
  type: project
---

La cámara del prototipo es **fija, una sola Camera nativa de Unity** (Cinemachine descartado). Todo el feedback de "juicy" durante el trick lo gestiona `CameraFeedback` en `Assets/_Project/Scripts/Camera/CameraFeedback.cs`.

**Why:** Se probó primero con `CinemachineVirtualCamera` cambiando prioridades entre ground/air/static (`CameraDirector.cs`, ya borrado). Los blends entre vcams eran inestables, daban tropiezos visuales y no aportaban a un encuadre tipo The Ramp. The Ramp usa cámara fija lateral, y eso encaja mucho mejor con un prototipo donde el foco debe estar en la rotación del trick, no en cortes de cámara.

**Arquitectura actual:**
- `TrickPerformer.cs` expone `event Action<TrickDefinition> OnTrickStarted`, lanzado al inicio de `PerformTrick`.
- `CameraFeedback` se suscribe a ese evento vía referencia serializada al `TrickPerformer`.
- Shake y zoom corren en coroutines independientes; cada una mantiene un offset (`currentShakeOffset`, `currentZoomOffset`) y un `LateUpdate` los combina sobre `basePosition` capturada en `Awake`. No se pisan entre sí.
- Shake: 2 muestras de `Mathf.PerlinNoise` desfasadas (X e Y) para movimiento orgánico, modulado por `shakeFalloff` (AnimationCurve).
- Zoom: combina cambio de FOV (`zoomDeltaFOV`) + desplazamiento hacia un `focusTarget` opcional (`zoomMoveDistance`) modulado por `zoomShape` (AnimationCurve tipo peak-and-return). Si `focusTarget` está vacío, solo cambia FOV.

**Valores validados (sesión 2026-05-26) — efecto "dolly-zoom" suave hacia el skate:**
- `Focus Target`: GameObject del Skate.
- `Zoom Delta FOV`: −2 (zoom-in muy sutil).
- `Zoom Move Distance`: 2 (la cámara se acerca 2 unidades world hacia el skate durante el peak).
- `Zoom Duration`: 0.45s.
- Shake con defaults (amplitude 0.15, freq 25, duration 0.25s).

La combinación FOV ligeramente negativo + acercamiento físico al sujeto evita la sensación rara del zoom-in puro (que hace zoom "al vacío" en medio de la pista) y queda más cinematográfico.

**How to apply:**
- Para añadir un segundo trigger más fuerte al aterrizar limpio (Día 5 del [[planning]]): llamar `cameraFeedback.PlayFeedback()` desde el `SkaterController` o un futuro `LandingEvaluator`. El método ya es público y reutilizable. Si se quiere distinto magnitud, exponer un segundo set de params o usar dos componentes separados.
- Si surge bug donde la cámara se queda desplazada tras un trick, verificar que ninguna otra coroutine escribe a `transform.position` durante o después del feedback — `LateUpdate` siempre debe ser el último en tocarla.
- `basePosition` se captura en `transform.position` (world). Si la cámara cuelga de un padre que se mueve, romper a `localPosition` + cálculos en parent space.
- Cinemachine sigue en `Packages/manifest.json` por inercia; cuando se confirme que ninguna escena tiene `CinemachineBrain` ni vcams, se puede quitar del manifest sin riesgo.

Ver también [[project-movement-design]] (péndulo físico, fuente del evento) y [[project-brief-tecnico]].
