---
name: project-audio-procedural
description: Decisión de audio procedural con Unity puro (sin assets externos) + tabla de SFX a implementar
metadata:
  type: project
---

**Decisión (2026-05-27):** implementar SFX procedurales con `AudioClip.Create` y síntesis a mano. Sin assets de audio externos.

**Why:** el brief técnico dice "único asset visual permitido: pack Kenney mini-skate" + "prohibido Asset Store". La lectura sobre audio externo es ambigua (Kenney audio pack está en su web/OpenGameArt, no Asset Store, pero "único asset" puede leerse estricto). Audio procedural elimina la duda — todo el SFX es código propio. Bonus: muestra capacidad técnica (DSP / síntesis en Unity) que pesa en una selección. Encaja con el estilo arcade del prototipo (cámara fija, bloom, chromatic, trucos cosméticos) — un audio retro 8/16-bit refuerza el lenguaje en lugar de chocar.

**How to apply:** crear un `AudioFeedback` (o `SfxManager`) en `Scripts/Audio/` que se suscriba a los eventos de TrickPerformer ([[project-trick-events]]: OnTrickStarted, OnTrickCompleted, OnTrickFailed, OnCleanLanding) y al estado del SkaterController para el roll loop modulado por velocity. Cada SFX se genera con un método helper que crea un `AudioClip` procedural y lo reproduce con `AudioSource.PlayOneShot`. ADSR envelope sobre todas las ondas — sin envelope suenan a pitido seco de Game Boy malo.

**Tabla de SFX a implementar (redactada el 2026-05-27):**

| Evento | Tipo de onda | Frecuencia / Duración | Envelope | Notas |
|---|---|---|---|---|
| Roll loop | Ruido marrón (integral acumulativa de ruido blanco con clamp) | Loop continuo ~0.5-2s | Amplitud modulada por `|rb.velocity.z|` del skater | `AudioSource.loop = true`. Volumen lerp en Update según velocity |
| Trick start (swipe) | Sweep sine/triángulo (chirp descendente) | 800 Hz → 200 Hz en 0.15s | Attack 5 ms, Release 100 ms | Coincide con OnTrickStarted |
| Trick completed | Arpegio cuadrada | C5–E5–G5 (523-659-783 Hz), 80 ms por nota | Attack 5 ms, Release 30 ms por nota | Estilo 8-bit retro. Coincide con OnTrickCompleted |
| Clean landing | Cuadrada grave + ruido pasabaja | 80-120 Hz (cuadrada) + noise decay 200 ms | Attack 1 ms, Release 200 ms | Mezcla de capas. Coincide con OnCleanLanding (y con hitstop + chispas + chromatic pulse). El pulso se ve durante el freeze del hitstop |
| Trick failed | Sweep descendente + ruido caótico | 600 Hz → 80 Hz en 0.3s + noise burst | Attack 10 ms, Release 300 ms | Tono "fallo" arcade. Coincide con OnTrickFailed (antes del ragdoll) |
| Score popup | Click sinusoidal | 800 Hz, 50 ms | Attack 1 ms, Release 40 ms | Suscribir a ScoreManager.OnScoreChanged |

**Refs técnicas:**
- `AudioClip.Create(name, lengthSamples, channels, frequency, ...)`. Llenar con `clip.SetData(float[], 0)`.
- Sample rate típico: 44100 Hz → `lengthSamples = duration * 44100`.
- Onda sinusoidal: `Mathf.Sin(2f * Mathf.PI * freq * t / sampleRate)`.
- Onda cuadrada: `Mathf.Sign(Mathf.Sin(...))`.
- Onda triangular: integrar la cuadrada o usar `Mathf.PingPong`.
- Ruido blanco: `UnityEngine.Random.Range(-1f, 1f)` por sample.
- Ruido marrón: acumulador con clamp — `brown = Mathf.Clamp(brown + Random.Range(-0.1f, 0.1f), -1f, 1f)` por sample. Es ruido blanco integrado, mucho más grave que el blanco crudo.
- ADSR: multiplicar la amplitud sample-a-sample por una curva con cuatro tramos (attack lineal 0→1, decay lineal 1→sustain, sustain constante, release lineal sustain→0).
- Para arpegio: concatenar varios bloques de samples, cada bloque con su propia envelope.

Ver también [[project-trick-events]] (eventos a consumir) y [[project-brief-tecnico]] (restricciones).
