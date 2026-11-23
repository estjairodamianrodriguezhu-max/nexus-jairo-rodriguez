# Actividad 5 - Publicación ejecutable de NEXUS 6.0

## Objetivo
Generar una distribución de NEXUS.WindowsForms para Windows x64, probarla fuera de Visual Studio y publicarla en GitHub Releases.

## Automatización incorporada
El workflow `.github/workflows/nexus-build-release.yml` restaura dependencias, compila en Release y publica la aplicación para Windows x64 como paquete autocontenido. El ZIP queda disponible como artefacto de GitHub Actions. Cuando se envía una etiqueta con formato `v*`, el workflow crea una GitHub Release y adjunta el ZIP.

## Cómo generar una versión
1. Confirmar que la compilación de la rama elegida termina correctamente.
2. Crear y enviar una etiqueta de versión, por ejemplo:

```bash
git tag v1.0.0
git push origin v1.0.0
```

3. Abrir la pestaña Actions del repositorio y comprobar que el workflow finalice correctamente.
4. Abrir Releases y descargar `NEXUS-6.0-win-x64.zip`.
5. Extraerlo en una carpeta distinta del proyecto y ejecutar el archivo `.exe`.
6. Probar el inicio, navegación, guardado local y demás funciones que dependan del entorno.

## Consideraciones
- La publicación autocontenida incluye el runtime de .NET necesario, pero no instala SQL Server. Las funciones SQL requieren que el equipo tenga acceso a una instancia configurada y que la cadena de conexión sea válida.
- La primera ejecución debe probarse en una carpeta limpia.
- No afirmar que la Release está publicada hasta que exista una etiqueta, el workflow termine y el ZIP aparezca adjunto en Releases.

## Evidencias para el informe
- URL de la GitHub Release.
- Captura de GitHub Actions con estado correcto.
- Captura del ZIP descargado.
- Captura del juego ejecutándose desde la carpeta publicada, sin Visual Studio.
