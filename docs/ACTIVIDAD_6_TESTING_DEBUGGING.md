# Actividad 6 - Testing & Debugging de NEXUS 6.0

## Alcance
Esta rama se creó para organizar las pruebas del proyecto NEXUS 6.0 sin modificar directamente la rama principal. El flujo de integración continua compila el proyecto Windows Forms en configuración Release. La compilación correcta es una prueba automática de integración, pero no reemplaza las pruebas funcionales manuales.

## Proyecto evaluado
- Proyecto: `NEXUS.WindowsForms`
- Archivo: `nexuws repoocitorio/POO/NEXUS.WindowsForms/NEXUS.WindowsForms.csproj`
- Plataforma de publicación prevista: Windows x64
- Rama de trabajo: `testing-debugging`

## Resultado de integración continua (GitHub Actions)
- Workflow ejecutado: `NEXUS - Build, Testing and Release`.
- Resultado: **correcto** en la ejecución `45288447369` (23/11/2026).
- Restauración NuGet: correcta.
- Compilación Release: correcta.
- Publicación autocontenida Windows x64: correcta.
- ZIP de distribución: generado como artefacto `NEXUS-6.0-win-x64`.
- Durante la primera ejecución se detectó una llave de cierre adicional en `NexusProgress.cs` (CS1022); se corrigió y la ejecución posterior finalizó correctamente.
- La Release pública aún no se ha creado porque falta enviar una etiqueta `v*`. Las pruebas manuales confirmadas por el usuario (inicio, guardado/recuperación y recolección de cristales) se registran como aprobadas; las demás siguen pendientes.

## Matriz de pruebas funcionales

| ID | Caso | Procedimiento | Resultado esperado | Estado real |
|---|---|---|---|---|
| T01 | Inicio del juego | Ejecutar NEXUS desde la compilación Release. | Aparece la pantalla inicial sin excepción. | **Aprobada** |
| T02 | Navegación | Abrir cada opción del menú lateral y regresar al panel. | Cada botón abre la sección correcta. | Pendiente de ejecutar |
| T03 | Entrada inválida | Dejar vacío el nombre o introducir valores fuera de rango. | Se valida el dato y se muestra un mensaje controlado. | Pendiente de ejecutar |
| T04 | Guardado local | Guardar una partida, cerrar y abrir de nuevo. | Se recuperan los datos guardados en JSON. | **Aprobada** |
| T05 | Escritura SQL | Guardar una partida con SQL Server disponible y consultar dbo.Partidas. | La fila contiene los valores enviados. | Pendiente de ejecutar |
| T06 | Actualización SQL | Guardar de nuevo el mismo jugador con valores modificados. | Se actualiza el registro esperado, sin duplicados no deseados. | Pendiente de ejecutar |
| T07 | Recuperación SQL | Cerrar el juego y cargar la partida desde SQL, si la función está implementada. | Se recuperan los valores persistidos en SQL. | Pendiente de ejecutar |
| T08 | SQL no disponible | Detener el servicio SQL e intentar guardar. | Se informa el error sin cerrar inesperadamente el juego. | Pendiente de ejecutar |
| T09 | Música | Activar, pausar y cambiar volumen desde el menú. | Los controles responden y el volumen cambia. | Pendiente de ejecutar |
| T10 | Publicación | Ejecutar el paquete publicado en una carpeta limpia, sin Visual Studio. | La aplicación inicia con sus archivos de distribución. | **Aprobada** |

| T11 | Recolección de cristales | Iniciar una partida, desplazarse por el mapa y recoger cristales. | El personaje puede recoger cristales y el progreso se actualiza. | **Aprobada** (confirmación del usuario) |

## Registro de defectos
Para cada fallo registrar: ID de prueba, pasos para reproducir, resultado esperado, resultado obtenido, archivo afectado, corrección aplicada, commit y resultado de la repetición.

| Defecto | Descripción | Corrección | Commit | Reprueba |
|---|---|---|---|---|
| D-01 | Llave de cierre adicional en `NexusProgress.cs` (error CS1022) detectada en la primera compilación CI. | Se eliminó la llave sobrante y se volvió a compilar. | Corrección en la rama `testing-debugging` | **Reprueba CI aprobada** |

## Criterio de cierre
1. Ejecutar la compilación automática de GitHub Actions y revisar que termine correctamente.
2. Realizar las pruebas funcionales pendientes de la matriz en el equipo Windows.
3. Adjuntar capturas auténticas de los resultados.
4. Corregir los defectos encontrados y repetir las pruebas.
5. Integrar esta rama en `main` mediante Pull Request después de revisar los cambios.

> Nota: T01, T04, T10 y T11 se marcaron como aprobadas según la confirmación del usuario. T02, T03 y T05–T09 siguen pendientes de prueba y evidencia.
