---
name: project-physic-materials
description: Setup de Physic Materials para que el skate resbale bien por los Mesh Colliders curvos de Kenney
metadata:
  type: project
---

El skate desliza bien por las rampas de Kenney usando un **Physic Material** custom con fricción casi nula, aplicado al Box Collider del Skater.

**Asset:** `Assets/_Project/Settings/Skate_Physics.physicMaterial`
- Dynamic Friction: 0.05
- Static Friction: 0.05
- Bounciness: 0
- Friction Combine: **Minimum**
- Bounce Combine: Minimum

**Why:** Los Mesh Colliders no-Convex de los FBX de Kenney (half-pipe, etc.) son curvas facetadas en triángulos. Con el PhysicMaterial Default (Dynamic Friction 0.6) el skate se queda enganchado en cada faceta y no baja por la rampa. Bajar la fricción al mínimo y usar `Friction Combine: Minimum` resuelve el deslizamiento — validado en sesión 2026-05-25.

**How to apply:**
- Si añades un nuevo collider al Skater o a una superficie por la que tenga que deslizar (rampa, suelo), asigna `Skate_Physics` en el campo `Material` del Collider.
- Con `Friction Combine: Minimum` basta con tenerlo en uno de los dos colliders en contacto, pero por claridad asígnalo también en las rampas si surge alguna duda.
- Si el skate empieza a atravesar geometría a velocidad alta, sube `Collision Detection` del Rigidbody de `Discrete` a `Continuous` — no es problema de fricción sino de CCD.

Ver también [[project-brief-tecnico]] para restricciones generales.
