<div align="center">
<img src="assets/capri.png" alt="Capri" width="200">

# Capri en español

Traducción comunitaria de Capri para Windows, basada en **v0.7.8**.

El personaje, el diseño y las animaciones son los del [proyecto original de Lachy](https://github.com/lachydotmcg/capri-desktop).
</div>

Capri vive en tu escritorio, se sienta en tus ventanas, pasea por tus pantallas y conversa contigo mediante texto y micrófono. Esta edición adapta su interfaz al español. Es un proyecto no oficial, sin suscripción añadida.

## Qué incluye

- Panel, ajustes básicos y avanzados, menús, controles de voz, accesibilidad e instalador en español.
- Presentación inicial y respuestas automáticas habituales traducidas. Reconoce «me llamo», «saltar», «parar» y pronombres en español.
- Español como idioma de conversación en instalaciones nuevas. Las preferencias existentes se conservan.
- Opción **Español · voz local gratuita**, con Piper Sharvard y su voz femenina. No necesita claves, servidor ni una grabación personal.
- [Ayuda en español](docs/es/ayuda.html) incluida en la aplicación y disponible sin conexión.
- Catálogos de traducción editables y comprobaciones de espacio para varias escalas de pantalla.

La traducción de interfaz de esta edición es para **Windows**. Las versiones de macOS y Linux conservan la interfaz original. Los nombres de modelos, rutas, teclas, identificadores de API y mensajes técnicos recibidos de otros programas mantienen su forma original.

## Instalar y usar sin pagar

1. Ejecuta el instalador de esta edición, `Capri-Setup.exe`.
2. En el panel, elige **NORMAL → Preparar su IA** para descargar el modelo recomendado. No necesitas una API de pago.
3. En **VOZ → LOCAL INCLUIDA**, abre el selector y elige **Español · voz local gratuita**. Si el motor de voz aún se está descargando, espera a que termine y repite la selección. La voz española se descarga una vez, se comprueba mediante SHA-256 y queda en tu PC. Al activarse aparece en LOCAL PROPIA.
4. Pulsa **Probar** y selecciona tu micrófono. En **Ajustes → Idioma**, comprueba que figure Español.

Escribe con **Ctrl + Alt + T**, habla con **Ctrl + Alt + V** y silencia el micrófono con **Ctrl + Alt + N**. Haz clic derecho en Capri para moverla, sentarla o subirla al cursor. Puedes cambiar estos atajos en Más ajustes.

La voz HD original requiere una grabación que tengas derecho a utilizar; no hace falta para usar la voz española incluida como opción. Las APIs externas son opcionales y pueden tener costes y condiciones propios.

La voz original **La suya (Her own)** conserva su acento inglés. Elegir Español como idioma de conversación no cambia ese acento. Para pronunciación peninsular usa la opción española Piper es_ES, con la voz femenina 1.

## Actualizar desde Capri original

Cierra Capri desde su menú y ejecuta el instalador traducido. Conserva la carpeta de datos, los modelos y los ajustes. Las actualizaciones automáticas de binarios originales se mantienen manuales en esta edición para conservar la traducción; consulta las versiones publicadas de la edición española antes de actualizar.

## Construir y comprobar

Se utiliza el compilador de .NET Framework 4.8 de Windows y `Windows.winmd` del SDK de Windows. No se necesita una suscripción ni un servicio de traducción.

```powershell
./ci/build-windows.ps1 -OutputDirectory out/windows -SpanishTest
```

Si el SDK no está instalado en la ubicación habitual, pasa `-WindowsMetadata RUTA\Windows.winmd`. Las referencias oficiales también están disponibles en el paquete NuGet de Microsoft Windows SDK CPP. No se incluyen herramientas ni modelos descargados en este repositorio.

`-SpanishTest` comprueba los catálogos, la presentación, los identificadores conservados y el espacio de los textos a varias escalas; deja capturas y resultados en `out/windows/spanish`. La prueba general original sigue disponible con `-SmokeTest`.

En GitHub las comprobaciones se ejecutan una sola vez por cambio de la propuesta y también al actualizar `main`. Windows dispone de 15 minutos para la comprobación general. La medición de rendimiento de ventanas transparentes se reserva para un escritorio interactivo: en GitHub se registra explícitamente como `LIMIT`, mientras que las comprobaciones de fotogramas, interpolación, intervalos y ajustes siguen ejecutándose. La medición completa permanece disponible al usar `-SmokeTest` en un PC.

**Limitación observada al validar v0.7.8:** la prueba general original falla en este equipo al reconocer los números de su imagen de prueba de OCR. Se reprodujo con la versión original sin traducir y con esta edición. Ese fallo no se ha ocultado ni modificado. Las comprobaciones específicas de la traducción se ejecutan por separado. Los mensajes generados por proveedores externos y las tareas complejas requieren pruebas de uso continuadas.

## Colaborar con la traducción

Edita `locales/es-ES.tsv`: cada línea contiene el texto original, una tabulación y su traducción. Conserva los espacios de las claves en `es-ES.fragments.tsv`, utilizados en frases con nombres o cifras. No traduzcas identificadores de API, rutas ni nombres de modelos. El proceso de construcción rechaza claves duplicadas.

Al avisar de un texto sin traducir, indica la pantalla, el texto exacto y la versión. No incluyas claves de API ni conversaciones privadas. Conservamos el código y los créditos originales; las mejoras de traducción pueden proponerse también al proyecto de Capri.

## Créditos y licencia

Required Notice: Copyright 2026 Lachy (https://github.com/lachydotmcg)

Required Notice: Anyone who uses Capri's code, art, animations or any of its other assets must credit the Capri project (https://github.com/lachydotmcg/capri-desktop). Capri's art and animations are by her artist, who prefers to stay private. Parts made by others keep their own licences, listed in its README.

Capri es un proyecto de fans no oficial, sin afiliación ni respaldo de CD PROJEKT RED, los titulares del anime ni sus actores de voz. La traducción comunitaria tampoco es una versión oficial de Lachy.

Se conserva [LICENSE.md](LICENSE.md), **PolyForm Noncommercial 1.0.0**, con sus avisos obligatorios. Consulta [README.en.md](README.en.md) para la documentación y los créditos completos de los componentes originales, incluidos Share Tech Mono, Kokoro, Parakeet, sherpa-onnx y Qwen3-TTS.

Voz española: [Piper Sharvard es_ES](https://huggingface.co/rhasspy/piper-voices/tree/main/es/es_ES/sharvard/medium), convertida por [sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx/releases/tag/tts-models). Su [ficha](https://huggingface.co/rhasspy/piper-voices/blob/main/es/es_ES/sharvard/medium/MODEL_CARD) atribuye el [conjunto de datos Sharvard](https://datashare.ed.ac.uk/handle/10283/574) bajo [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). El modelo se descarga de su fuente original; no forma parte del código del repositorio.
