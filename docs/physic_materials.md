# Physic Materials del skate

El skate desliza bien por las rampas de Kenney usando un **Physic Material** custom con fricción casi nula, aplicado al Box Collider del Skater.

**Asset:** `Assets/_Project/Settings/Skate_Physics.physicMaterial`

| Parámetro | Valor |
|---|---|
| Dynamic Friction | 0.05 |
| Static Friction | 0.05 |
| Bounciness | 0 |
| Friction Combine | **Minimum** |
| Bounce Combine | Minimum |

## Por qué

Los Mesh Colliders no-Convex de los FBX de Kenney (half-pipe, etc.) son curvas facetadas en triángulos. Con el PhysicMaterial Default (Dynamic Friction 0.6) el skate se queda enganchado en cada faceta y no baja por la rampa. Bajar la fricción al mínimo y usar `Friction Combine: Minimum` resuelve el deslizamiento.

## Cómo aplicarlo

- Si añades un nuevo collider al Skater o a una superficie por la que tenga que deslizar (rampa, suelo), asigna `Skate_Physics` en el campo `Material` del Collider.
- Con `Friction Combine: Minimum` basta con tenerlo en uno de los dos colliders en contacto, pero por claridad asignarlo también en las rampas evita dudas.
- Si el skate empieza a atravesar geometría a velocidad alta, subir `Collision Detection` del Rigidbody de `Discrete` a `Continuous` — no es problema de fricción sino de CCD.
