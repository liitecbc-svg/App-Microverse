# Documentacion tecnica de Microverse

## 1. Resumen del proyecto

Microverse es una aplicacion Unity orientada a visualizar modelos biologicos en 3D y en un modo de Realidad Aumentada basado en camara. La app combina contenido incluido en el build, modelos descargados desde Supabase, cache offline, traduccion automatica con ML Kit en Android y una interfaz construida por codigo.

El proyecto esta preparado para Android y usa `Assets/Scenes/SampleScene.unity` como escena de entrada. El arranque real de la aplicacion no depende de objetos prearmados en la escena: `MicroverseBootstrap` crea los componentes base al cargar.

## 2. Entorno y dependencias

- Version de Unity: `6000.4.9f1`.
- UI: `uGUI` y `TextMeshPro`.
- Backend: Supabase via REST.
- Modelos runtime: recursos incluidos en `Assets/Microverse/Resources/Models`, descargas en `Application.persistentDataPath/MicroverseModels` y soporte de glTF/glb mediante `com.unity.cloud.gltfast`.
- Traduccion: puente Android nativo con Google ML Kit Translate.
- Build Android: configuracion automatizada en `Assets/Microverse/Scripts/Editor/PlayStoreBuild.cs`.

Dependencias relevantes declaradas en `Packages/manifest.json`:

- `com.unity.ugui`
- `com.unity.cloud.gltfast`
- `com.unity.ai.inference`
- `com.unity.timeline`
- modulos Unity WebRequest, AndroidJNI, UI, XR y otros modulos base.

## 3. Estructura principal

```text
Assets/
  Microverse/
    Resources/
      AppLogo/
      Instructions/
      ModelPreviews/
      Models/
    Scripts/
      Data/
      Editor/
      Runtime/
      Services/
      UI/
  Plugins/
    Android/
      AndroidManifest.xml
      mainTemplate.gradle
      com/microverse/translation/
  Scenes/
    SampleScene.unity
docs/
  documentacion-tecnica.md
  play-store-release.md
supabase_schema.sql
```

## 4. Arquitectura por capas

### Runtime

`Assets/Microverse/Scripts/Runtime` contiene el arranque y utilidades transversales:

- `MicroverseBootstrap.cs`: crea `UnityMainThreadDispatcher`, asegura un `EventSystem` y agrega `MicroverseApp`.
- `UnityMainThreadDispatcher.cs`: permite que callbacks de Android o tareas asincronas ejecuten acciones en el hilo principal de Unity.

### Data

`Assets/Microverse/Scripts/Data` define los tipos base:

- `BiologicalModel.cs`: entidad principal del catalogo. Contiene nombre, subtitulo, categoria, descripcion, colores, semilla visual, rutas de modelo y preview, y marca `IsBundledModel`.
- `LocalizedText.cs`: texto en espanol, ingles y portugues con fallback.
- `MicroverseLanguage.cs`: enum de idiomas soportados.
- `MicroverseLanguageExtensions.cs`: conversion de idioma interno a codigos `en`, `es`, `pt`.

### Services

`Assets/Microverse/Scripts/Services` contiene integracion de datos, cache, favoritos y traduccion:

- `IModelCatalogService.cs`: contrato para cualquier fuente de catalogo.
- `LocalModelCatalogService.cs`: catalogo incluido en la app.
- `SupabaseModelCatalogService.cs`: carga categorias y modelos desde Supabase.
- `CompositeModelCatalogService.cs`: fusiona local, remoto y descargado.
- `ModelDownloadStore.cs`: descarga modelos y conserva metadatos offline.
- `PreviewImageStore.cs`: descarga y cachea previews.
- `FavoriteModelsStore.cs`: guarda favoritos en `PlayerPrefs`.
- `UiTextCatalog.cs`: textos estaticos de la interfaz.
- `ITranslationService.cs`, `AndroidMlKitTranslationService.cs`, `FallbackTranslationService.cs`, `TranslationRequest.cs` y `MlKitTranslationCallback.cs`: traduccion y fallback.

### UI

`Assets/Microverse/Scripts/UI` construye toda la interfaz por codigo:

- `MicroverseApp.cs`: controlador principal de flujo, catalogo, pantallas, traduccion y AR.
- `HomeScreenView.cs`: catalogo principal con busqueda, filtros, descargas y favoritos.
- `ModelCardView.cs`: tarjeta individual de modelo.
- `CategoriesScreenView.cs`: agrupacion por categorias.
- `DetailScreenView.cs`: ficha de modelo y acceso al visor.
- `BottomNavigationBar.cs`: navegacion inferior.
- `WebCamCameraBackground.cs`: fondo de camara para AR.
- `ModelManipulator.cs`: rotacion y escala del modelo.
- `BiologyVisualFactory.cs`: sprites procedurales y previews.
- `UiFactory.cs`, `RoundedSpriteFactory.cs`, `MicroverseTheme.cs`: helpers visuales compartidos.
- `LongPressTrigger.cs`: pulsacion larga para acciones secundarias.

### Editor

`Assets/Microverse/Scripts/Editor` contiene herramientas solo para el editor:

- `SupabaseConfigSync.cs`: sincroniza `.env` hacia `Assets/Resources/supabase_config.json`.
- `PlayStoreBuild.cs`: aplica configuracion Android y puede generar un AAB.

## 5. Flujo de arranque

1. Unity carga `Assets/Scenes/SampleScene.unity`.
2. `MicroverseBootstrap.StartMicroverse()` se ejecuta con `RuntimeInitializeOnLoadMethod`.
3. Se crea `UnityMainThreadDispatcher`.
4. Si no existe, se crea un `EventSystem`.
5. Se crea un GameObject `MicroverseApp`.
6. `MicroverseApp.Awake()` instancia:
   - `AndroidMlKitTranslationService`
   - `UiTextCatalog`
   - canvas principal
   - `CompositeModelCatalogService`
7. La app muestra pantalla inicial y carga catalogo cuando el flujo lo necesita.

## 6. Flujo de catalogo

La fuente final de modelos es `CompositeModelCatalogService`.

Orden de carga:

1. Carga modelos incluidos con `LocalModelCatalogService`.
2. Agrega modelos descargados previamente desde `ModelDownloadStore`.
3. Intenta cargar modelos remotos con `SupabaseModelCatalogService`.
4. Mezcla modelos remotos, manteniendo protegidos los modelos incluidos cuando corresponda.
5. Vuelve a incorporar descargados para asegurar soporte offline.

Este comportamiento permite que la app siga mostrando contenido aunque Supabase no este disponible. Los modelos incluidos no dependen de internet.

## 7. Modelo de datos

`BiologicalModel` usa estos campos principales:

- `Id`: identificador estable del modelo.
- `Name`, `Subtitle`, `Category`, `Description`: textos localizados.
- `ScientificName`: nombre cientifico.
- `PrimaryColor`, `SecondaryColor`: colores para previews/procedural.
- `VisualSeed`: semilla para generar variaciones visuales.
- `IsElongated`: cambia la forma procedural.
- `ModelFileUrl`: ruta `resource:`, URL remota o archivo local.
- `PreviewUrl`: ruta `resource:`, URL remota o archivo local.
- `IsBundledModel`: indica si el modelo va incluido dentro del build.
- `LoadedPreviewSprite`: cache en memoria de la preview.

## 8. Supabase

El esquema base esta en `supabase_schema.sql`.

Tablas:

- `categorias`
  - `id`
  - `nombre`
  - `fecha_creacion`
- `modelos_3d`
  - `id`
  - `nombre`
  - `archivo_modelo_url`
  - `preview_url`
  - `formato`
  - `tamano_bytes`
  - `descripcion`
  - `categoria_id`
  - `subtitle`
  - `scientific_name`
  - `primary_color`
  - `secondary_color`
  - `visual_seed`
  - `is_elongated`

`SupabaseModelCatalogService` consulta:

```text
/rest/v1/categorias?select=id,nombre
/rest/v1/modelos_3d?select=*
```

Las credenciales se leen en este orden:

1. `.env` en la raiz del proyecto.
2. `Assets/Resources/supabase_config.json`.

El servicio rechaza claves que parezcan privadas, como `service_role`, `secret` o `sb_secret_`.

## 9. Configuracion y secretos

Archivo local recomendado:

```env
SUPABASE_URL=https://tu-proyecto.supabase.co
SUPABASE_KEY=tu-anon-key-publica
```

Puntos importantes:

- `.env` no debe subirse a Git.
- `Assets/Resources/supabase_config.json` se incluye en el build si existe.
- Solo debe usarse una llave publica anon/publishable.
- Las politicas RLS de Supabase deben permitir lectura publica solo de lo necesario.
- Nunca incluir `service_role`, claves administrativas, tokens privados ni credenciales de usuario.

## 10. Funcionamiento offline

La app mantiene tres niveles de disponibilidad:

1. Modelos incluidos en `Assets/Microverse/Resources/Models`.
2. Previews incluidas en `Assets/Microverse/Resources/ModelPreviews`.
3. Modelos descargados y cacheados en `Application.persistentDataPath`.

`ModelDownloadStore` conserva:

- rutas locales en `microverse.downloaded_model_paths`;
- metadatos de catalogo en `microverse.downloaded_model_catalog`;
- archivos fisicos en `MicroverseModels`.

`PreviewImageStore` conserva previews remotas como PNG dentro de `MicroversePreviews`.

Esto permite que un modelo descargado siga visible despues de reiniciar la app, incluso sin conexion, siempre que el archivo local exista.

## 11. UI y navegacion

La UI se construye completamente desde C#, principalmente en `MicroverseApp` y sus vistas auxiliares.

Pantallas principales:

- Inicio: entrada visual de Microverse.
- Catalogo AR: modelos locales o disponibles para visualizar.
- Biblioteca 3D: modelos remotos descargables.
- Favoritos: modelos marcados por el usuario.
- Categorias: filas por categoria con tarjetas horizontales.
- Creditos: colaboradores e instituciones.
- Visor AR/3D: camara, modelo, instrucciones y advertencia de seguridad.

La navegacion inferior se limita a visualizacion, biblioteca y favoritos. Otras pantallas se abren desde acciones internas o menus.

## 12. Descarga de modelos

El flujo de descarga parte desde `HomeScreenView`:

1. La tarjeta llama a `StartModelDownload`.
2. `ModelDownloadStore.DownloadModelRoutine` descarga el archivo remoto.
3. Se guarda en `Application.persistentDataPath/MicroverseModels`.
4. Se descarga la preview si corresponde.
5. Se guardan metadatos y ruta local.
6. La tarjeta actualiza progreso sin reconstruir toda la grilla.

Los modelos descargados se pueden eliminar con pulsacion larga en la tarjeta. Esa accion usa `LongPressTrigger` y `ModelDownloadStore.DeleteDownloadedModel`.

## 13. Visor AR/3D

El visor se controla desde `MicroverseApp.EnterARMode`.

Componentes principales:

- `WebCamCameraBackground`: muestra camara con `WebCamTexture`.
- `ModelManipulator`: permite rotar y escalar el modelo.
- luces y camara Unity configuradas en runtime.
- overlay de instrucciones.
- advertencia de seguridad AR una vez por sesion de app.

Carga de modelo:

1. Primero intenta resolver un modelo incluido o descargado.
2. Si el origen es `.glb` o `.gltf`, intenta usar glTFast por reflexion.
3. Si no puede cargar un modelo real, usa una representacion procedural.

El fallback procedural evita que el visor quede vacio cuando un modelo remoto falla, falta el paquete importador o no hay archivo local valido.

## 14. Camara

`WebCamCameraBackground` solicita permisos en Android/iOS y prueba varias configuraciones:

- 1920x1080 a 30 fps.
- 1280x720 a 30 fps.
- constructor por defecto.
- 640x480 a 30 fps.

Tambien:

- prioriza camaras no frontales;
- descarta nodos de metadata comunes en Linux;
- ajusta rotacion segun `videoRotationAngle`;
- aplica espejo vertical si `videoVerticallyMirrored`;
- escala el `RawImage` para llenar la vista.

Si la camara no inicia, muestra un fondo de simulacion 3D.

## 15. Traduccion

El idioma fuente canonico para textos estaticos y contenido traducible es ingles.

Capas:

- `UiTextCatalog`: textos de interfaz.
- `LocalizedText`: contenido del catalogo.
- `AndroidMlKitTranslationService`: traduccion real en Android.
- `FallbackTranslationService`: devuelve texto original en editor o plataformas no soportadas.
- `MlKitTranslatorBridge.java`: puente Android hacia Google ML Kit.

Flujo:

1. La app prepara modelos offline para los idiomas objetivo.
2. Las solicitudes se agrupan como `TranslationRequest`.
3. Android traduce por lote.
4. Los callbacks vuelven al hilo principal mediante `UnityMainThreadDispatcher`.
5. Los resultados actualizan textos UI y textos de modelos.

## 16. Plugin Android

Archivos clave:

- `Assets/Plugins/Android/AndroidManifest.xml`
- `Assets/Plugins/Android/mainTemplate.gradle`
- `Assets/Plugins/Android/com/microverse/translation/MlKitTranslatorBridge.java`
- `Assets/Plugins/Android/com/microverse/translation/MlKitTranslationCallback.java`
- `Assets/Plugins/Android/com/microverse/translation/MlKitModelDownloadCallback.java`

`mainTemplate.gradle` agrega:

```gradle
implementation 'com.google.mlkit:translate:17.0.3'
```

El manifest declara:

- `android.permission.CAMERA`
- `android.permission.VIBRATE`
- `android:usesCleartextTraffic="false"`
- `android:allowBackup="false"`

## 17. Build Android y Play Store

La configuracion automatizada vive en `PlayStoreBuild.cs`.

Valores esperados:

- Package name: `com.microverse.app`
- Product name: `Microverse`
- Version name base: `1.0.0`
- Min SDK: API 26
- Target SDK: API 35
- Arquitecturas: ARMv7 y ARM64
- Formato recomendado: Android App Bundle (`.aab`)
- Escena incluida: `Assets/Scenes/SampleScene.unity`

Menu de Unity:

- `Microverse > Android > Configure Play Store Settings`
- `Microverse > Android > Bump Version Code`

Build batch:

```powershell
Unity.exe -batchmode -quit -projectPath . -executeMethod Microverse.Editor.PlayStoreBuild.BuildPlayStoreRelease
```

Guia relacionada: `docs/play-store-release.md`.

## 18. Como agregar un modelo incluido

1. Copiar el archivo del modelo a `Assets/Microverse/Resources/Models`.
2. Copiar la preview a `Assets/Microverse/Resources/ModelPreviews`.
3. Agregar una entrada en `LocalModelCatalogService`.
4. Usar rutas:

```text
resource:Models/nombre-del-modelo
resource:ModelPreviews/nombre-preview
```

5. Marcar `IsBundledModel` como `true`.
6. Probar sin internet para confirmar que aparece y abre.

## 19. Como agregar un modelo remoto

1. Subir el archivo `.fbx` a un storage publico o firmado segun la estrategia del backend.
3. Insertar registro en `modelos_3d`.
4. Relacionarlo con `categorias`.
5. Verificar que `archivo_modelo_url` y `preview_url` sean accesibles desde el dispositivo.
6. Abrir la biblioteca en la app y probar descarga.
7. Reiniciar sin internet y validar que el modelo descargado siga disponible.


## 20. Mapa rapido de archivos

| Area | Archivo | Funcion |
| --- | --- | --- |
| Arranque | `Runtime/MicroverseBootstrap.cs` | Crea la app al cargar escena. |
| App principal | `UI/MicroverseApp.cs` | Controla flujo, pantallas, catalogo y AR. |
| Catalogo local | `Services/LocalModelCatalogService.cs` | Modelos incluidos en el build. |
| Catalogo remoto | `Services/SupabaseModelCatalogService.cs` | Consulta Supabase. |
| Catalogo combinado | `Services/CompositeModelCatalogService.cs` | Une local, remoto y descargados. |
| Descargas | `Services/ModelDownloadStore.cs` | Guarda modelos offline. |
| Previews | `Services/PreviewImageStore.cs` | Guarda imagenes offline. |
| Favoritos | `Services/FavoriteModelsStore.cs` | Persiste favoritos. |
| Traduccion | `Services/AndroidMlKitTranslationService.cs` | Puente C# hacia ML Kit. |
| Textos UI | `Services/UiTextCatalog.cs` | Catalogo de strings. |
| Tarjetas | `UI/ModelCardView.cs` | Vista de modelo en grillas. |
| Catalogo | `UI/HomeScreenView.cs` | Busqueda, filtros y descargas. |
| Camara AR | `UI/WebCamCameraBackground.cs` | Fondo de camara. |
| Manipulacion | `UI/ModelManipulator.cs` | Gestos de rotacion y escala. |
| Build Android | `Editor/PlayStoreBuild.cs` | Configuracion y AAB. |
| Supabase config | `Editor/SupabaseConfigSync.cs` | Sincroniza `.env` a Resources. |
