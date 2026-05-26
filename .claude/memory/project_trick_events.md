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
- **`ScoreManager`**: `OnTrickCompleted` → suma `trick.score` y emite `OnScoreChanged(newScore, delta)`. `OnTrickFailed` → resetea score a 0 (con `delta = -scoreAnterior`).
- **`HUD`**: `OnTrickStarted` → instancia popup blanco con `NOMBRE +score`. `OnTrickCompleted` → `popup.PlaySuccess()` (verde sube y desvanece). `OnTrickFailed` → `popup.PlayFail()` (rojo cae con tilt). También suscrito a `ScoreManager.OnScoreChanged` para actualizar el texto del score con pop.
- **`SkaterController`**: `OnTrickFailed` → `RagdollAndRespawn`: quita constraints del Rigidbody, AddTorque random para volar, desactiva `trickPerformer.enabled` para que swipes durante el ragdoll no se cuelen, espera `respawnDelay` (default 2s) y restaura posición/rotation/constraints capturadas en `Awake`.

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
