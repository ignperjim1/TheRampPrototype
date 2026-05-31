# Audio procedural

Todos los SFX del prototipo se generan en runtime con `AudioClip.Create` y síntesis a mano (`Assets/_Project/Scripts/Audio/SfxSynth.cs`). **Sin assets de audio externos.**

## Por qué

El brief técnico dice "único asset visual permitido: pack Kenney mini-skate" + "prohibido Asset Store". La lectura sobre audio externo es ambigua (Kenney también tiene packs de audio en OpenGameArt, no Asset Store, pero "único asset" puede leerse estricto). Audio procedural elimina la duda — todo el SFX es código propio.

Bonus: muestra capacidad técnica (DSP / síntesis básica en Unity) y encaja con el estilo arcade del prototipo (cámara fija, bloom, chromatic, trucos cosméticos) — un audio retro 8/16-bit refuerza el lenguaje en lugar de chocar.

## Arquitectura

- **`SfxSynth.cs`**: clase estática con métodos puros que devuelven `AudioClip`. Cada método genera un `float[]` sample-a-sample con la onda + envelope deseada y lo empaqueta en un `AudioClip` con `AudioClip.Create` + `SetData`.
- **`SkaterAudio.cs`**: MonoBehaviour que en `Awake` instancia todos los clips llamando a los helpers de `SfxSynth`. Tiene:
  - 1 `AudioSource` dedicado al loop de rolling.
  - 1 `AudioSource` dedicado al jump swoosh (para poder cortarlo cuando arranca un trick).
  - Pool de N `AudioSource` (`oneShotVoices`, default 6) round-robin para el resto de one-shots.
  - `pitchVariance` aleatoria por disparo (0.06 default) para que repeticiones no suenen idénticas.
  - Todo `spatialBlend = 0` (2D).

## SFX implementados

| Evento | Clip | Síntesis |
|---|---|---|
| Rolling loop (Rolling state) | `RollingLoop` | Ruido blanco filtrado con lowpass de un polo (`lp += cutoff*(noise-lp)`) + sine 60 Hz de rumble. `CrossfadeForLoop` para que el loop sea seamless. Volumen lerpeado con `MoveTowards` según estado. |
| Despegue (Rolling → Airborne) | `JumpSwoosh` | Sweep sine 220→900 Hz, 0.30s, mezcla con ruido blanco. Envelope AR. Filtrado por `hasTouchedGround` y `IsRespawning` para no sonar en el spawn ni en el respawn. |
| Aterrizaje (Airborne → Rolling) | `LandImpact` | Sweep sine 140→55 Hz + click de ruido en los primeros 20ms. Exp decay. |
| `OnTrickStarted` | `TrickSwoosh` | Sweep triángulo 700→1500 Hz, 0.18s. Mezcla con ruido. Corta el `JumpSwoosh` si está sonando (override). |
| `OnCleanLanding` | `SuccessJingle` | Arpegio cuadrada C5–E5–G5–C6 (523-659-784-1046 Hz), 80ms por nota. Estilo 8-bit. |
| `OnTrickFailed` | `FailThud` | Boom sine 120→35 Hz (con sqrt para que decay no sea lineal) + crack de ruido en los primeros 40ms + cola de ruido lowpassed. Normalizado a 0.95 peak. Antes de reproducirse corta todas las voces para que no se solape con un jingle en vuelo. |
| `ScoreManager.OnScoreChanged` (delta > 0) | `ScoreClick` | Sine 1400→1100 Hz, 60ms. AR + exp decay. Suscripción aparte a `ScoreManager`, no a TrickPerformer. |

## Decisiones de diseño audio

- **Jump y land salen de transiciones de estado** (Rolling↔Airborne), no de eventos del TrickPerformer. El audio reacciona al estado real del skater, no a la intención del jugador.
- **No hay sonido separado por `OnTrickCompleted`**. El feedback auditivo "lo hiciste bien" llega al aterrizar (`SuccessJingle` en `OnCleanLanding`). Esto evita capas de audio mientras estás en aire haciendo el trick.
- **Jump swoosh tiene su propio AudioSource dedicado** para que el inicio de un trick pueda llamar `jumpSource.Stop()` y cortarlo en seco. Sin esto, jump y trick se solaparían en los frames inmediatos al despegue.
- **Pitch variance pequeña** (Range 0-0.3). Más alto distorsionaría la identidad del sonido sintetizado.

## Refs técnicas para mantenimiento

- `AudioClip.Create(name, lengthSamples, channels, frequency, ...)`. Llenar con `clip.SetData(float[], 0)`.
- Sample rate típico: 44100 Hz → `lengthSamples = duration * 44100`.
- Onda sinusoidal: `Mathf.Sin(2f * Mathf.PI * freq * t / sampleRate)`.
- Onda cuadrada: `Mathf.Sign(Mathf.Sin(...))` o `phase < 0.5f ? 1f : -1f`.
- Onda triangular: `1f - 4f * Mathf.Abs(phase - 0.5f)`.
- Ruido blanco: `UnityEngine.Random.Range(-1f, 1f)` por sample.
- Lowpass de un polo: `lp += cutoff * (noise - lp)` por sample.
- ADSR simplificado (AR): attack lineal 0→1 durante una fracción del clip, luego decay lineal a 0.
- Exp decay: `Mathf.Exp(-k * t)` con `t` normalizado.
- Loop seamless: crossfade del último tramo con el inicio (`CrossfadeForLoop`).

Ver también: [Arquitectura de eventos del trick](trick_events.md).
