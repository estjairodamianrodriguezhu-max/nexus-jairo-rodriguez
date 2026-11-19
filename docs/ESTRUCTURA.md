# Estructura del proyecto

## Propósito

Mantener el proyecto NEXUS organizado por responsabilidad, sin mezclar código, documentación y pruebas.

## Estructura recomendada

```text
nexus-jairo-rodriguez/
├── docs/
│   ├── ESTRUCTURA.md
│   ├── PLAN_TRABAJO.md
│   ├── arquitectura.md
│   └── decisiones-tecnicas.md
├── src/
│   └── NEXUS/
│       ├── Domain/          # Entidades y reglas del negocio
│       ├── Application/     # Casos de uso y servicios
│       ├── Infrastructure/  # Persistencia e integraciones, si aplica
│       └── Presentation/    # Interfaz de usuario
├── tests/
│   └── NEXUS.Tests/
├── README.md
└── CONTRIBUTING.md
```

Esta es una propuesta, no una orden para mover archivos automáticamente. Primero identifica la solución `.sln`, los proyectos `.csproj`, los recursos y las rutas relativas que usan. Mover proyectos sin actualizar esas referencias puede romper la compilación.

## Reglas de organización

1. Un archivo debe tener una responsabilidad clara.
2. Usa nombres descriptivos y consistentes; evita nombres como `Clase1`, `PruebaFinal2` o `nuevo`.
3. Mantén las clases relacionadas en carpetas coherentes con su función.
4. No subas contraseñas, claves, tokens ni archivos locales de configuración con secretos.
5. Guarda manuales, diagramas y decisiones técnicas en `docs/`.
6. Agrega pruebas para las reglas importantes del sistema.

## Antes de reorganizar carpetas

- [ ] Identificar todos los archivos `.sln` y `.csproj`.
- [ ] Confirmar la versión de .NET y los paquetes NuGet.
- [ ] Revisar imágenes, recursos y archivos de configuración.
- [ ] Crear una rama para la reorganización.
- [ ] Mover un grupo pequeño de archivos y compilar.
- [ ] Ejecutar las pruebas antes de integrar los cambios.