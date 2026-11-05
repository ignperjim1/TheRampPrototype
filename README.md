# The Ramp — Prototype

Made by Ignacio Pérez-Aguirre Jiménez.
Time: 1 week, about 20–25 hours. Unity 2022.3.

A skateboarding prototype on a big ramp, inspired by the Steam game *The Ramp*. The board rides back and forth on its own and jumps by itself at the edges, so the player only has to focus on swiping in the air to land tricks with good timing.

Brief:
Two-week technical test. Focus: UX, game feel and "juice".
- No Asset Store assets.
- No external plugins. Official Unity packages only.
- Only visual asset: the Kenney *mini-skate* pack.
- Mouse control (simulating a finger).
- A board with no rider, one ramp, automatic movement and jumping.

----

## Time log

| Day | Activity (approximate times) |
|-----|------------------------------|
| 24 May | Short night session: setup and planning (1h) |
| 25 May | One afternoon session: skater and ramps (4h) |
| 27 May | Two sessions (afternoon and night): tricks, camera, first UI (5h + 3h) |
| 28 May | One afternoon session: visual juice and post-processing (4h) |
| 30 May | One afternoon session: procedural audio (2h) |
| 31 May | Morning session: polish and README (3h) |

----

## How to play

- The board moves on its own between the two ramps (it swings like a pendulum).
- It jumps automatically when it reaches the edge of each ramp.
- In the air, press the mouse and drag from that point in one of 4 directions (up, down, left, right) to do a trick.
- A trick only scores if its rotation finishes before landing. If you land halfway through a trick → wipeout, ragdoll, respawn, and you lose the score you built up (the best score is kept with PlayerPrefs).
- You can chain several tricks in a single jump.

---

## Controls

Hold the left mouse button and drag in a direction while the board is in the air.

| Swipe | Trick | Points |
|-------|-------|--------|
| ↑ Up | Impossible | 150 |
| ↓ Down | Kickflip | 100 |
| ← Left | Pop Shove-it | 80 |
| → Right | Tre Flip | 220 |

The `BEST` score persists between runs (`PlayerPrefs`).

---

## Key technical decisions

- URP with post-processing (Bloom, a Vignette that changes with the game state, and Chromatic Aberration and Lens Distortion on a clean landing).
- Cinemachine was dropped after trying it. I kept my own fixed camera (`CameraFeedback.cs`) with shake and a dolly zoom towards the board when a trick starts.
- Pendulum physics: the board uses a Rigidbody with a single initial velocity, plus a `SpeedMaintainer` that adds speed back each time it crosses the centre of the floor. There is no scripted movement or jump logic: it is all physics.
- Tricks are a cosmetic rotation: the trick coroutine rotates a child `SkateModel`, never the root Rigidbody, so it doesn't interfere with the pendulum.
- Event system: `TrickPerformer` exposes 4 events (`OnTrickStarted`, `OnTrickCompleted`, `OnTrickFailed`, `OnCleanLanding`) that the camera, audio, HUD, score and visual feedback listen to.
- 100% procedural audio: every sound effect is synthesised at runtime with `AudioClip.Create` (`SfxSynth.cs`). No external audio assets.
- One use of reflection in `PostProcessFeedback`, to raise the clamp of `ChromaticAberration.intensity` above 1 on the landing after a completed trick. It is documented in the code.
- Input buffering, both for the jump off the ramp and for chaining tricks.

---

## Repo structure

```
Assets/
├── _Project/             ← all the prototype's code and assets
│   ├── Art/              ← materials, VFX
│   ├── Audio/            ← (empty: the audio is procedural)
│   ├── Prefabs/          ← Skater, ramp, floor, UI
│   ├── Scenes/Main.unity ← single scene
│   ├── Scripts/
│   │   ├── Audio/        ← SkaterAudio, SfxSynth
│   │   ├── Camera/       ← CameraFeedback
│   │   ├── Feedback/     ← Hitstop, LandingVFX, PostProcessFeedback
│   │   ├── Game/         ← ScoreManager, SkaterSpawner
│   │   ├── Skater/       ← SkaterController, SpeedMaintainer
│   │   ├── Tricks/       ← TrickPerformer, SwipeDetector, TrickDefinition (SO), SwipeDirection
│   │   └── UI/           ← HUD, TrickPopup
│   ├── Scriptables/      ← TrickDefinition assets (Up/Down/Left/Right)
│   └── Settings/         ← URP profiles and the board's Physic Material
├── ThirdParty/
│   └── kenney_mini-skate/ ← the only visual asset the brief allowed
docs/                     ← detailed design and architecture decisions
```

---

## Detailed technical docs

The design decisions (why a pendulum and not a scripted jump, why a fixed camera, why procedural audio, why events instead of managers...) are explained in [docs/](docs/). Each document covers the iterations I discarded, the trade-offs and notes for maintenance. The docs are written in Spanish.
