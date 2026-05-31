# Movimiento del skate: péndulo físico

El skate se comporta como un **péndulo físico** entre las dos rampas, no como un personaje con salto explícito scripteado.

## Diseño actual

- `Rigidbody` con gravedad estándar, `FreezeRotation` + `FreezePositionX` siempre activos. Sin cambios dinámicos de constraints (excepto durante el ragdoll del fail).
- **Velocidad inicial única** (`initialVelocity`) aplicada en `Start()`. No se vuelve a forzar `velocity` en código (excepto el `SpeedMaintainer`).
- `lastGroundedTime` se inicializa a `float.NegativeInfinity` en `Awake` para evitar que el coyote time (0.1s) reporte Rolling falsamente durante el spawn — el skate aparece en aire encima de la rampa, y sin esto el primer FixedUpdate haría `Time.time - 0 <= 0.1` (TRUE), marcando Rolling antes de haber tocado suelo.
- Las rampas, el suelo y el [Skate_Physics](physic_materials.md) (fricción 0.05) hacen el resto: el skate oscila naturalmente subiendo, parándose, bajando, cruzando el suelo, subiendo la otra rampa.
- **`SpeedMaintainer`**: trigger collider en el centro del suelo que restaura `velocity.z` a un target fijo manteniendo el signo. Compensa la pérdida de energía por las facetas del Mesh Collider de las rampas. Sin él, el péndulo decae con cada pasada.
- **Colliders extendidos en lo alto de las rampas**: paredes invisibles que prolongan artificialmente la superficie por la que el skate desliza, para que visualmente "despegue" pero físicamente siga la guía. Garantiza que la trayectoria del salto sea predecible.

## Por qué péndulo y no salto explícito

Iteraciones descartadas antes de llegar al péndulo:

- `JumpTrigger` con `AddForce` vertical en el borde de las rampas.
- `AccelTrigger` con boost de `velocity.z` en el suelo.

Ambos rompían la continuidad y eran sensibles a tuneo (cambiar el impulso desbalanceaba todo el ritmo).

El brief dice "movimiento lateral automático, salto automático". El péndulo cumple ambos **sin código de gameplay, solo física**. Permite que el jugador se concentre exclusivamente en swipes para tricks, sin pensar en momentum ni timing de salto.

## Estructura del Skater

```
Skater (Rigidbody + BoxCollider + SkaterController + TrickPerformer + SwipeDetector + LandingVFX)
└── SkateModel (Mesh)
```

Los tricks rotan **`SkateModel`** (modelo visual hijo), nunca el `Skater` raíz. El collider y el Rigidbody quedan en el padre, así la física del péndulo no se ve afectada por la rotación cosmética del truco.

## Notas para mantenimiento

- Si el péndulo decae, subir el `targetSpeed` del `SpeedMaintainer` un pelín por encima del `initialVelocity` (sobrecompensa el frenado por fricción de las facetas).
- Si el skate vuela demasiado en la cresta, revisar los colliders extendidos de las rampas — deben formar una guía continua.
- No reintroducir lógica de `OnTakeoff`/`OnLanded` con cambios de `velocity`. La detección de estado (`Rolling`/`Airborne`) sí es útil para los consumers que validan tricks o reaccionan al aterrizaje.

Ver también: [Physic Materials](physic_materials.md), [Arquitectura de eventos del trick](trick_events.md).
