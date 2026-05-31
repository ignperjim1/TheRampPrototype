# Arquitectura de eventos del trick

Todo el flujo de tricks (inicio, éxito, fallo, aterrizaje) viaja como eventos `Action` desde `TrickPerformer.cs`. Los sistemas consumidores se suscriben en `OnEnable`/`OnDisable` con referencias inyectadas por `SkaterSpawner`. **Sin singletons, sin `FindObjectOfType`, sin mensajes globales.**

## Eventos expuestos por `TrickPerformer`

| Evento | Firma | Cuándo se emite |
|--------|-------|-----------------|
| `OnTrickStarted` | `Action<TrickDefinition>` | Inicio de la coroutine `PerformTrick` (al consumirse un swipe válido dentro de la air window). |
| `OnTrickCompleted` | `Action<TrickDefinition>` | La coroutine de rotación termina **en aire** (rotación completa, sin haber tocado suelo). Inmediato, no espera al aterrizaje. |
| `OnTrickFailed` | `Action<TrickDefinition>` | Aterrizaste mid-coroutine (`CurrentState` pasó a `Rolling` antes de completar). Emitido desde dentro de la coroutine antes del `yield break`. |
| `OnCleanLanding` | `Action` (sin params) | Transición Airborne→Rolling detectada en `Update`, si hubo al menos un `OnTrickCompleted` desde el último aterrizaje. Una sola vez por aterrizaje aunque hayas encadenado varios. |

## Quién escucha qué

- **`CameraFeedback`** — `OnTrickStarted`: shake corto + dolly-zoom hacia el skate. `OnCleanLanding`: shake fuerte sin zoom. Ver [Cámara y feedback](camera_feedback.md).
- **`Hitstop`** — `OnCleanLanding`: `Time.timeScale = freezeScale` (default 0) durante `duration` segundos realtime (default 0.08s), luego restaura a 1. Restaura también en `OnDisable` si la coroutine está activa para evitar dejar el juego congelado.
- **`LandingVFX`** — `OnCleanLanding`: reproduce un array de `ParticleSystem` (`landingBursts`: chispas + polvo configurados con burst en Emission) y activa varios `TrailRenderer` (`landingTrails`) durante `trailDuration` segundos. Sin VFX en aire por decisión de diseño — el feedback en aire queda solo en CameraFeedback (shake + dolly-zoom).
- **`PostProcessFeedback`** — controla el Volume de URP. En `Awake` **clona el sharedProfile** (`Instantiate(volume.sharedProfile)`) y lo asigna a `volume.profile`. Sin esto, las modificaciones en runtime ensucian el asset en disco (en Editor, `volume.profile` devuelve `sharedProfile` directamente). Mantiene la referencia en `runtimeProfile` y la destruye en `OnDestroy`. En `Update` lerpea la intensity del Vignette entre `vignetteGrounded` (0.15) y `vignetteAirborne` (0.45) según `SkaterController.CurrentState`, con dos filtros: ignora Airborne si `IsRespawning`, y usa un flag local `hasTouchedGround` para descartar el primer Airborne post-spawn. En `OnCleanLanding` lanza un pulso combinado de **ChromaticAberration** (peak por encima de 1 vía override del clamp de URP usando reflection) + **LensDistortion**.
- **`ScoreManager`** — `OnTrickCompleted`: suma `trick.score` y emite `OnScoreChanged(newScore, delta)`. Si supera `HighScore`, lo persiste con `PlayerPrefs` y emite `OnHighScoreChanged(newHigh)`. `OnTrickFailed`: resetea score actual a 0 (con `delta = -scoreAnterior`).
- **`HUD`** — `OnTrickStarted`: instancia popup blanco con `NOMBRE +score`. `OnTrickCompleted`: `popup.PlaySuccess()` (verde sube y desvanece). `OnTrickFailed`: `popup.PlayFail()` (rojo cae con tilt). También suscrito a `ScoreManager.OnScoreChanged` y `OnHighScoreChanged` para actualizar score y BEST con pop.
- **`SkaterAudio`** — `OnTrickStarted`: corta el `jumpSource` (override del swoosh de salto) y reproduce `TrickSwoosh`. `OnCleanLanding`: `SuccessJingle`. `OnTrickFailed`: corta todas las voces one-shot y reproduce `FailThud`. Además se suscribe a `ScoreManager.OnScoreChanged` para reproducir un click corto si `delta > 0`.
- **`SkaterController`** — `OnTrickFailed`: `RagdollAndRespawn`: quita constraints del Rigidbody, aplica `AddForce` (`Vector3.up * ragdollLaunchUpward + insideUnitSphere * ragdollLaunchRandom`) + `AddTorque` random, desactiva `trickPerformer.enabled` Y `speedMaintainer.enabled` (sin esto último el SpeedMaintainer secuestra al skate de vuelta al péndulo al cruzar el trigger del suelo), espera `respawnDelay` (default 2s) y restaura posición/rotation/constraints capturadas en `Awake`.

`SkaterController` expone `State CurrentState` y `bool IsRespawning` (true entre `OnTrickFailed` y el final del respawn) — consumidos por PostProcessFeedback y SkaterAudio para distinguir aire-haciendo-trick de aire-en-ragdoll.

## Inyección de dependencias

Los componentes consumers viven en escena con sus refs serializadas vacías. El skater (Skate.prefab) se instancia en runtime por `SkaterSpawner`, que tras crear la instancia llama a los métodos `SetTrickPerformer(...)` / `SetSkater(...)` / `SetFocusTarget(...)` / `SetSpeedMaintainer(...)` de cada consumer. Esos setters usan el patrón:

```csharp
public void SetTrickPerformer(TrickPerformer tp) {
    enabled = false;   // OnDisable: desuscribe del viejo (null al arranque)
    trickPerformer = tp;
    enabled = true;    // OnEnable: suscribe al nuevo
}
```

Así no hay riesgo de doble suscripción ni de leaks.

## Encadenado de tricks (chain mid-coroutine)

- `isPerforming = true` bloquea ejecución simultánea, pero `HandleSwipe` **sí guarda** el swipe en `bufferedSwipe` aunque haya un trick activo.
- El chain NO espera a que termine el trick. Dentro de `PerformTrick`, en cuanto el progreso normalizado `k >= chainAfter` (default 0.7) y `HasChainableBuffer()` es true, la coroutine hace `break` antes de tiempo y lanza el siguiente trick. Encadena el combo sin esperar al 100% de la rotación.
- El board **no vuelve a plano** entre tricks encadenados: el siguiente trick arranca desde la `localRotation` actual del modelo, así el combo fluye continuo.
- **`comboBase`**: rotación del modelo capturada en el frame de despegue. Solo se restaura al **completar sin chain** (con un settle suave de `settleDuration`) o al **fallar** (snap inmediato, da igual porque entra ragdoll).
- `HasChainableBuffer()` exige: buffer presente + reciente (≤ `swipeBufferDuration`, default 0.3s) + `IsInAirWindow()` + que el swipe resuelva a un trick válido.
- `windowEnd` actúa como override duro: aunque haya swipe buffereado, si la ventana de aire expiró, el chain se descarta.

## Filtro `hasTouchedGround`

El skate spawnea sobre la rampa **en el aire** (cae a ella). Esa caída no debe permitir triggear tricks. Lo mismo tras el respawn de un fail.

Para evitarlo, `IsInAirWindow()` chequea un flag `hasTouchedGround` que arranca en `false` y solo pasa a `true` cuando `CurrentState` se observa como `Rolling`. Durante `IsRespawning` se resetea a `false`. Adicionalmente:

- `SkaterController.CurrentState` arranca por default como **`Airborne`** (no `Rolling`) para evitar un race condition: si el primer `Update` del TrickPerformer corre antes del primer `FixedUpdate` del skater, ve `Airborne` por default y no marca el flag falsamente como tocado.
- `TrickPerformer.OnEnable` resetea `hasTouchedGround = false` y `bufferedSwipe = null`. Esto es necesario porque durante el respawn el `SkaterController` desactiva el TrickPerformer (para ignorar swipes mid-ragdoll), y al reactivarlo el flag se quedaría con su valor anterior si no se reseteara.

Ver también: [Cámara y feedback](camera_feedback.md), [Movimiento (péndulo físico)](movement_design.md), [Audio procedural](audio_procedural.md).
