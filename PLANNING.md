# The Ramp — Prototipo (planning)

Referencia del sprint: estructura de carpetas, decisiones y calendario.

---

## Estructura de carpetas propuesta

```
Assets/
├── _Project/                       ← guion bajo para que salga arriba en el Project window
│   ├── Art/
│   │   ├── Materials/              ← materiales URP del skate, rampa, suelo, etc.
│   │   └── VFX/                    ← ParticleSystems (humo ruedas, polvo aterrizaje…)
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/                    ← roll, jump, land, trick-success, fail, swoosh
│   ├── Prefabs/
│   │   ├── Skater/
│   │   ├── Level/
│   │   └── UI/
│   ├── Scenes/
│   │   └── Main.unity              ← una sola escena, no necesitamos Bootstrap
│   ├── Scripts/
│   │   ├── Skater/                 ← movimiento lateral, salto, rotación física
│   │   ├── Tricks/                 ← SwipeDetector, TrickDefinition (SO), TrickEvaluator
│   │   ├── Camera/                 ← extensiones/control de Cinemachine (shake, dutch)
│   │   ├── Feedback/               ← Juicy: hitstop, shake, time-scale, flashes
│   │   ├── UI/                     ← HUD, score, timing del swipe
│   │   ├── Audio/                  ← AudioManager simple (pool de SFX)
│   │   └── Game/                   ← GameManager, score, estado de partida
│   └── Settings/                   ← URP Renderer, Volume profile, Input axes si toca
└── ThirdParty/
    └── Kenney/                     ← mover aquí el pack mini-skate (FBX + Textures + license)
```

### Decisiones detrás

- **`_Project/` + `ThirdParty/`**: separa lo tuyo de assets de terceros — el revisor ve de un vistazo qué has hecho tú vs. qué venía con el pack.
- **Scripts por feature, no por tipo**: `Skater/`, `Tricks/`, `Feedback/`… se lee mejor que `Controllers/`, `Models/`, `Managers/`. Y para un prototipo de game feel, "Feedback" como carpeta propia transmite intención.
- **`TrickDefinition` como ScriptableObject**: añadir/balancear trucos sin tocar código. Clave si quieres iterar rápido en game feel.
- **Una sola escena**: no merece la pena un Bootstrap para un prototipo de una pantalla.

---

## Plan de sprint (2 semanas naturales)

### Semana 1 — Vertical slice jugable

| Día | Objetivo |
|-----|----------|
| 1   | Setup: instalar URP + Cinemachine, migrar materiales del pack, mover Kenney, estructura de carpetas, escena con rampa + suelo |
| 2   | `SkaterController`: movimiento lateral automático ida/vuelta + salto automático al borde de la rampa. Física básica con Rigidbody |
| 3   | `SwipeDetector` con mouse + 1 truco funcional (ej. kickflip = swipe abajo) con rotación visual del skate |
| 4   | Cámara Cinemachine: virtual cam en suelo, otra en aire, blend automático, noise de shake en aterrizaje |
| 5   | Aterrizaje éxito/fallo según ángulo, score básico, HUD mínimo (score + indicador de ventana de swipe) |

### Semana 2 — Game feel y juicy

| Día   | Objetivo |
|-------|----------|
| 6-7   | 2-3 trucos completos con swipes distintos (ej. kickflip, 360 shove-it, indy grab). Balanceo de ventanas de timing |
| 8     | Juicy visual: hitstop al aterrizar truco, screen shake, partículas (humo ruedas, polvo aterrizaje, "estrellas" en truco perfecto) |
| 9     | Audio: rolling loop, jump swoosh, land impact, success/fail jingles. Mezcla y volumen |
| 10    | UX: feedback visual del timing del swipe (indicador en pantalla), preview del truco, easing en todo (curves) |
| 11    | Post-processing URP: bloom sutil, vignette en aire, chromatic aberration en truco perfecto |
| 12    | Polish + balance: ajustar tiempos, sensibilidad swipe, dificultad |
| 13    | Buffer / fixes / edge cases |
| 14    | Build de prueba, limpieza, borrar `Library`, zip y entrega |

---

## Fuera de scope deliberadamente

Para no morir en el sprint:

- **Personaje encima del skate** — el brief dice "skate sin personaje".
- **Variedad de rampas o niveles** — el brief dice "una rampa grande".
- **Sistema de combos, multiplicadores, progresión.**
- **Menús** — pantalla de juego directa, máximo botón "restart".
