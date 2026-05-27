---
name: project-trick-events
description: Arquitectura de eventos del ciclo de trick (TrickPerformer emite, varios sistemas reaccionan)
metadata:
  type: project
---

Todo el flujo de tricks (inicio, éxito, fallo, aterrizaje) viaja como eventos `Action` desde `TrickPerformer.cs`. Los sistemas consumidores se suscriben en `OnEnable`/`OnDisable` con referencias serializadas. Sin singletons, sin mensajes globales.

**Eventos expuestos por `TrickPerformer`:**

| Evento | Firma | Cuándo se emite |
|--------|-------|-----------------|
| `OnTrickStarted` | `Action<TrickDefinition>` | Inicio de la coroutine `PerformTrick` (al consumirse un swipe válido en air window). |
| `OnTrickCompleted` | `Action<TrickDefinition>` | La coroutine de rotación termina **en aire** (rotación completa, sin haber tocado suelo). Inmediato, no espera al aterrizaje. |
| `OnTrickFailed` | `Action<TrickDefinition>` | Aterrizaste mid-coroutine (state pasó a Rolling antes de completar). Emitido desde dentro de la coroutine antes del `yield break`. |
| `OnCleanLanding` | `Action` (sin params) | Transición Airborne→Rolling detectada en Update, si hubo al menos un `OnTrickCompleted` desde el último aterrizaje. Una sola vez por aterrizaje aunque hayas encadenado varios. |

**Quién escucha qué:**

- **`CameraFeedback`** (cámara): `OnTrickStarted` → shake corto + dolly-zoom hacia el skate. `OnCleanLanding` → shake fuerte sin zoom (variant `landingShake*` en el inspector).
- **`Hitstop`** (Scripts/Feedback): `OnCleanLanding` → `Time.timeScale = freezeScale` (default 0) durante `duration` segundos realtime (default 0.08s), luego restaura a 1. Restaura también en `OnDisable` si la coroutine está activa para evitar que el juego quede congelado si se desactiva el componente mid-hitstop.
- **`LandingVFX`** (Scripts/Feedback): `OnCleanLanding` → reproduce un array de `ParticleSystem` (`landingBursts`, p.ej. chispas + polvo configurados como Play-On-Awake=false con burst en Emission) y activa un `TrailRenderer` (`landingTrail`) durante `trailDuration` segundos (default 1s). El trail se inicializa con `emitting = false` en Awake. Sin VFX en aire por decisión de diseño (2026-05-27) — el feedback en aire queda solo en CameraFeedback (shake + dolly-zoom).
- **`PostProcessFeedback`** (Scripts/Feedback): controla el Volume de URP (`SampleSceneProfile.asset`). En `Awake` **clona el sharedProfile** con `Instantiate(volume.sharedProfile)` y lo asigna a `volume.profile` — sin esto, las modificaciones en runtime ensucian el asset en disco (en Editor, `volume.profile` devuelve `sharedProfile` directamente, no una copia). Mantiene la referencia en `runtimeProfile` y la destruye en `OnDestroy`. En `Update` lerpea la intensity del Vignette entre `vignetteGrounded` (0.15) y `vignetteAirborne` (0.45) según `SkaterController.CurrentState`, **con dos filtros**: (1) ignora Airborne si `IsRespawning` (durante el ragdoll el skate vuela pero no queremos vignette de "trick"); (2) usa un flag local `hasTouchedGround` para descartar el primer Airborne post-spawn — el skate spawnea sobre la rampa en el aire y cae a ella, eso no es un trick. `hasTouchedGround` arranca en false, pasa a true al primer Rolling detectado, y se resetea a false durante `IsRespawning` para reaplicar el mismo filtro post-respawn. En `OnCleanLanding` lanza un pulso de ChromaticAberration (peak 0.8 → fade en 0.4s). Vignette y ChromaticAberration deben existir como overrides en el profile.

**`SkaterController` expone**: `State CurrentState`, `bool IsRespawning` (true entre `OnTrickFailed` y el final del respawn) — consumido por PostProcessFeedback para distinguir aire-haciendo-trick de aire-en-ragdoll.
- **`ScoreManager`**: `OnTrickCompleted` → suma `trick.score` y emite `OnScoreChanged(newScore, delta)`. `OnTrickFailed` → resetea score a 0 (con `delta = -scoreAnterior`).
- **`HUD`**: `OnTrickStarted` → instancia popup blanco con `NOMBRE +score`. `OnTrickCompleted` → `popup.PlaySuccess()` (verde sube y desvanece). `OnTrickFailed` → `popup.PlayFail()` (rojo cae con tilt). También suscrito a `ScoreManager.OnScoreChanged` para actualizar el texto del score con pop.
- **`SkaterController`**: `OnTrickFailed` → `RagdollAndRespawn`: quita constraints del Rigidbody, aplica `AddForce` (Vector3.up * upward + random insideUnitSphere con Y forzada positiva) + `AddTorque` random, desactiva `trickPerformer.enabled` Y `speedMaintainer.enabled` (sin esto último el skate vuelve al patrón de péndulo al cruzar el trigger del SpeedMaintainer), espera `respawnDelay` (default 2s) y restaura posición/rotation/constraints capturadas en `Awake`. Parámetros tuneables: `ragdollSpinTorque`, `ragdollLaunchUpward`, `ragdollLaunchRandom`.

**Encadenado de tricks (chain buffer):**
- `isPerforming = true` bloquea la ejecución de un segundo trick simultáneo, pero `HandleSwipe` **sí guarda** el swipe en `bufferedSwipe` aunque haya un trick activo.
- Al final de `PerformTrick` (después de `OnTrickCompleted`), se llama `TryChainFromBuffer()`.
- `TryChainFromBuffer` requiere: buffer reciente (≤ `chainBufferDuration`, default 0.1s) + sigue en aire + `IsInAirWindow()` (es decir, `t - airborneSince ≤ windowEnd`). Si pasa todas, lanza el siguiente trick directamente.
- `windowEnd` actúa como override duro: aunque el swipe esté buffereado, si la ventana de aire expiró, el chain se descarta.

**Edge cases a tener en cuenta:**
- Durante `RagdollAndRespawn`, `TrickPerformer` está deshabilitado. Los swipes que llegan mientras tanto al `SwipeDetector` no llegan al `TrickPerformer` (porque `OnEnable`/`OnDisable` lo desuscriben del `SwipeDetector` al desactivar el componente). Al reactivarse, la suscripción se restaura.
- `pendingCleanLanding` puede quedar `true` si te desactivan el TrickPerformer entre `OnTrickCompleted` y la transición a Rolling. En la práctica el respawn ocurre con el skate aterrizando en spawn → `OnCleanLanding` se dispararía al respawnar, lo que provocaría un shake fuerte indeseado. Si se observa este bug, resetear `hasPendingCleanLanding = false` al final del respawn (o exponer un `ResetState()` público en TrickPerformer).
- Popup activo durante el ragdoll: el `HUD.activePopup` ya recibió `PlayFail` y se está animando independientemente; sobrevive al respawn sin problemas.

Ver también [[project-camera-feedback]] (consumidor cámara), [[project-movement-design]] (péndulo físico que provee el state Rolling/Airborne).
