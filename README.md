# Microverse Backend - Integración con Supabase

Este proyecto integra **Supabase** como backend en la aplicación Unity para cargar dinámicamente el catálogo de microorganismos y células en tiempo real.

---

## Archivos de Configuración y Backend

Si deseas realizar modificaciones en la base de datos o en la conexión, aquí tienes los archivos clave:

### 1. Variables de Entorno (Recomendado)
* **Ruta:** `.env` (en la raíz del proyecto)
* **Propósito:** Almacena las credenciales locales de Supabase fuera del control de versiones.
* **Estructura:**
  ```env
  SUPABASE_URL=https://********************
  SUPABASE_KEY=****************************
  ```
  *(Nota: Este archivo está en el `.gitignore` por defecto para evitar fugas de seguridad).*

### 2. Configuración de Credenciales de Respaldo (Builds)
* **Ruta:** `Assets/Resources/supabase_config.json`
* **Propósito:** Configuración de respaldo leída por Unity fuera del editor (para compilaciones finalizadas/builds).
* **Estructura:**
  ```json
  {
    "supabaseUrl": "https://your-project.supabase.co",
    "supabaseKey": "your-anon-key-here"
  }
  ```
  *(Nota: Este archivo también ha sido agregado al `.gitignore` para seguridad).*

### 2. Esquema de Base de Datos (SQL para Supabase)
* **Ruta:** `supabase_schema.sql` (en la raíz del proyecto)
* **Propósito:** Contiene las sentencias SQL para crear las tablas e insertar los datos iniciales.
* **Tablas Creadas:**
  - `categorias`: Identifica los grupos de modelos (ej. Bacterias, Virus).
  - `modelos_3d`: Almacena la información de cada microorganismo.
* **Cómo usarlo:** Copia el contenido de este archivo y ejecútalo en la consola de Supabase (**SQL Editor > New Query > Run**).

---

## Estructura del Código C# (Qué editar en Unity)

Si necesitas realizar modificaciones al comportamiento del catálogo en Unity, edita estos archivos:

1. **[SupabaseModelCatalogService.cs](file:///home/brax/Documentos/App-Microverse-main/Assets/Microverse/Scripts/Services/SupabaseModelCatalogService.cs)**
   * **Propósito:** Realiza las consultas HTTP (`UnityWebRequest`) a Supabase, procesa las respuestas JSON y mapea los datos a objetos C# de Unity.
   * **Qué editar aquí:**
     * Las reglas para deducir si un modelo es alargado (`is_elongated`) según su nombre o especie.
     * El generador procedimental de colores armónicos si la base de datos no especifica un color hexadecimal (`primary_color` / `secondary_color`).

2. **[MicroverseApp.cs](file:///home/brax/Documentos/App-Microverse-main/Assets/Microverse/Scripts/UI/MicroverseApp.cs)**
   * **Propósito:** Inicializa el catálogo al arrancar la aplicación (`Awake()`).
   * **Qué editar aquí:**
     * El diseño y texto de la pantalla de carga inicial (`ShowLoadingScreen()`).
     * Las acciones que se realizan cuando ocurre un fallo y se activa el catálogo local (`LocalModelCatalogService`) como fallback.

3. **[IModelCatalogService.cs](file:///home/brax/Documentos/App-Microverse-main/Assets/Microverse/Scripts/Services/IModelCatalogService.cs)**
   * **Propósito:** Define el contrato/interfaz del catálogo.

4. **[LocalModelCatalogService.cs](file:///home/brax/Documentos/App-Microverse-main/Assets/Microverse/Scripts/Services/LocalModelCatalogService.cs)**
   * **Propósito:** Catálogo estático local. Útil como respaldo (fallback) offline o si deseas seguir probando de manera local sin conexión de red.

---

## Cómo agregar o modificar Modelos en Supabase

El mapeador de C# está diseñado para ser flexible. Puedes agregar campos estéticos opcionales en la tabla `modelos_3d` si deseas tener control total desde el panel de Supabase:

* **`subtitle` (text):** Subtítulo o tipo de célula (ej: *Célula eucariota*).
* **`scientific_name` (text):** Nombre científico (ej: *Amoeba proteus*).
* **`primary_color` y `secondary_color` (varchar):** Color en formato HEX (ej: `#00A0FF` o `#C03DFF`). Esto determina cómo se pinta la célula proceduralmente en la UI.
* **`visual_seed` (integer):** Semilla numérica que genera variaciones en los organelos celulares de la vista previa.
* **`is_elongated` (boolean):** Define si la forma celular se dibuja circular o elíptica.

*Si dejas estos campos vacíos o en NULL en tu panel de Supabase, la aplicación deducirá los colores y las formas de manera automática y armónica basados en el nombre y categoría.*
