# Microverse Play Store release

## Estado de publicacion

El proyecto queda configurado para Android con:

- Package name estable: `com.microverse.app`
- Version name inicial: `1.0.0`
- Version code inicial: `1`
- Escena incluida: `Assets/Scenes/SampleScene.unity`
- Target SDK: Android 15 / API 35
- Min SDK: Android 8 / API 26
- Arquitecturas: ARMv7 y ARM64
- Salida recomendada: Android App Bundle (`.aab`)

El package name es permanente despues de publicar en Google Play. Si la ficha ya existe con otro package name, cambialo antes del primer upload.

## Build manual desde Unity

1. Abre el proyecto en Unity.
2. Ejecuta `Microverse > Android > Configure Play Store Settings`.
3. Incrementa `versionCode` con `Microverse > Android > Bump Version Code` antes de cada actualizacion.
4. En `File > Build Profiles`, selecciona Android y genera un App Bundle.

## Build por linea de comandos

Define las credenciales de firma como variables de entorno:

```powershell
$env:MICROVERSE_KEYSTORE_PATH="C:\ruta\microverse-release.jks"
$env:MICROVERSE_KEYSTORE_PASS="password-del-keystore"
$env:MICROVERSE_KEY_ALIAS="microverse"
$env:MICROVERSE_KEY_PASS="password-del-alias"
$env:MICROVERSE_VERSION_NAME="1.0.1"
$env:MICROVERSE_VERSION_CODE="2"
```

Ejecuta Unity en batch mode:

```powershell
Unity.exe -batchmode -quit -projectPath . -executeMethod Microverse.Editor.PlayStoreBuild.BuildPlayStoreRelease
```

El archivo se genera en `Builds/Android/`.

## Supabase y `.env`

El `.env` no debe subirse a Git y no debe ir como archivo dentro de la app. En este proyecto el editor sincroniza `SUPABASE_URL` y `SUPABASE_KEY` hacia `Assets/Resources/supabase_config.json`, que Unity si incluye en la build.

Eso esta bien solo si `SUPABASE_KEY` es la llave publica anon/publishable y Supabase tiene Row Level Security configurado. No pongas nunca `service_role`, `sb_secret_`, tokens privados, claves de storage administrativas ni credenciales de usuario en la app.
