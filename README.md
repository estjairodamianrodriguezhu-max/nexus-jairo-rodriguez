# NEXUS — Proyecto de Programación II

NEXUS es un proyecto académico desarrollado en **C#** con enfoque en programación orientada a objetos. Este repositorio reúne el código fuente, la documentación técnica y los materiales de trabajo del equipo.

## Objetivos del repositorio

- Mantener el código organizado y fácil de entender.
- Documentar la arquitectura, las decisiones técnicas y las instrucciones de ejecución.
- Facilitar el trabajo colaborativo mediante ramas y revisiones.
- Registrar el avance por hitos y tareas.

## Requisitos

- Visual Studio 2022 o posterior.
- .NET SDK compatible con la solución del proyecto.
- Git para control de versiones.

> Antes de ejecutar el proyecto, confirma la versión de .NET indicada en el archivo del proyecto (`.csproj`) o en la solución (`.sln`).

## Cómo empezar

1. Clona el repositorio.
2. Abre la solución `.sln` en Visual Studio.
3. Restaura los paquetes NuGet si Visual Studio lo solicita.
4. Compila la solución y ejecuta el proyecto de inicio.
5. Consulta la carpeta [`docs/`](docs/) para conocer la estructura y el plan de trabajo.

## Organización propuesta

```text
nexus-jairo-rodriguez/
├── docs/                 # Arquitectura, guías y planificación
├── src/                  # Código fuente de la aplicación (si aplica)
├── tests/                # Pruebas automatizadas (si aplica)
├── README.md             # Presentación y puesta en marcha
└── CONTRIBUTING.md       # Normas de colaboración
```

La estructura real debe respetar los archivos existentes. Si el código todavía está en la raíz, muévelo a `src/` solo después de comprobar las referencias de la solución en Visual Studio.

## Forma de trabajo con Git

- `main`: versión estable.
- `develop`: integración del trabajo del equipo, si el equipo decide usarla.
- `feature/nombre-tarea`: nuevas funcionalidades.
- `fix/nombre-problema`: correcciones.

Trabaja en una rama propia y abre un Pull Request antes de integrar cambios importantes en `main`.

## Documentación

- [Estructura del proyecto](docs/ESTRUCTURA.md)
- [Plan de trabajo](docs/PLAN_TRABAJO.md)
- [Normas para contribuir](CONTRIBUTING.md)

## Equipo

Actualiza esta sección con los nombres y responsabilidades acordados por el equipo.

## Estado

Proyecto académico en desarrollo. La funcionalidad implementada y los requisitos exactos deben reflejarse en la documentación a medida que avance el trabajo.