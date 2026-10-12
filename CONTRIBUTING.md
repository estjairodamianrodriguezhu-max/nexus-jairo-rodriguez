# Guía de colaboración

## Antes de empezar

1. Actualiza tu copia local de la rama de integración acordada.
2. Crea una rama para una sola tarea.
3. Revisa los archivos que vas a modificar para evitar conflictos.

## Nombres de ramas

- `feature/descripcion-corta`: funcionalidad nueva.
- `fix/descripcion-corta`: corrección.
- `docs/descripcion-corta`: documentación.
- `refactor/descripcion-corta`: reorganización interna sin cambiar el comportamiento esperado.

## Mensajes de commit

Usa mensajes cortos que expliquen el cambio:

- `feat: agregar validación de usuario`
- `fix: corregir cálculo de horario`
- `docs: actualizar instrucciones`
- `refactor: separar lógica de presentación`

## Pull Requests

Antes de solicitar revisión:

- [ ] El cambio tiene un objetivo claro.
- [ ] La solución compila.
- [ ] Las pruebas relevantes pasan, si existen.
- [ ] La documentación está actualizada.
- [ ] No se incluyeron secretos, contraseñas ni archivos generados innecesarios.

Describe qué cambiaste, por qué lo hiciste y cómo se puede comprobar. No integres directamente cambios grandes a `main` sin revisión del equipo.

## Trabajo en equipo

- Comunica los archivos que estás modificando.
- No sobrescribas cambios de otra persona sin hablarlo primero.
- Mantén los Pull Requests pequeños y fáciles de revisar.
- Si una decisión afecta la arquitectura, documenta el motivo en `docs/`.