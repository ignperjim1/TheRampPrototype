---
name: project-movement-design
description: Decisiones de diseño del movimiento del skate — modelo péndulo físico, no on-rails ni salto explícito
metadata:
  type: project
---

El skate se comporta como un **péndulo físico** entre las dos rampas, no como un personaje con salto explícito.

**Diseño actual (validado en sesión 2026-05-25):**
- Rigidbody con gravedad estándar, `FreezeRotation` + `FreezePositionX` siempre activos. Sin cambios dinámicos de constraints.
- Velocidad inicial única (`initialVelocity`) en `Start()`. No se vuelve a forzar velocity en código (excepto el SpeedMaintainer).
- `lastGroundedTime` se inicializa a `float.NegativeInfinity` en `Awake` para evitar que el coyote time (0.1s) reporte Rolling falsamente durante el spawn — el skate aparece en aire encima de la rampa, y sin esto el primer FixedUpdate hace `Time.time - 0 <= 0.1` (TRUE), marcando Rolling antes de haber tocado suelo.
- Las rampas, suelo y el `Skate_Physics` (fricción 0) hacen el resto: el skate oscila naturalmente subiendo, parándose, bajando, cruzando el suelo, subiendo la otra rampa.
- **`SpeedMaintainer`**: trigger collider en el centro del suelo que restaura `velocity.z` a un target fijo manteniendo el signo. Compensa la pérdida de energía por las facetas del Mesh Collider de las rampas. Sin él, el péndulo decae.
- **Colliders extendidos en lo alto de las rampas**: paredes invisibles que prolongan artificialmente la superficie por la que el skate desliza, para que visualmente "despegue" pero físicamente siga la guía. Garantiza que la trayectoria del salto sea predecible.

**Por qué péndulo y no salto explícito:**
- Probamos primero con triggers de salto (`JumpTrigger` con impulso Y) y boost (`AccelTrigger` con velocity Z). Ambos descartados: rompían la sensación de continuidad y eran sensibles a tuneo.
- El brief dice "movimiento lateral automático, salto automático" — el péndulo cumple ambos sin código de gameplay, solo física.
- Permite que el jugador se concentre **exclusivamente en swipes para tricks**, sin pensar en momentum ni timing de salto.

**Para los tricks (Día 3) — NO desactivar el collider del skate durante el aire:**
- El collider está en el Rigidbody root, que mantiene la física del péndulo.
- Los tricks deben ser **rotaciones cosméticas del modelo visual hijo**, no del transform físico. Estructura recomendada:
  ```
  Skater (Rigidbody + BoxCollider + SkaterController)
  └── SkateModel (Mesh)
  ```
  El truco rota `SkateModel`, nunca `Skater`.

**How to apply:**
- Si surge un bug donde el péndulo decae, primero subir el `targetSpeed` del `SpeedMaintainer` un pelín por encima del `initialVelocity` (sobrecompensa el frenado).
- Si el péndulo se sale del setup (skate vuela demasiado en la cresta), revisar los colliders extendidos de las rampas — deben formar una guía continua.
- No reintroducir lógica de `OnTakeoff`/`OnLanded` con cambios de velocity. La detección de estado (`Rolling`/`Airborne`) sí es útil para validar swipes solo en aire en el Día 3.

Ver también [[project-physic-materials]] para fricción y [[project-brief-tecnico]] para restricciones.
