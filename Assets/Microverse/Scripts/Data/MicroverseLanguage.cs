/**
 * MicroverseLanguage.cs
 *
 * Define los idiomas soportados por Microverse para la interfaz y el contenido del catalogo.
 *
 * Main responsibilities:
 * - Enumerar las opciones de idioma disponibles.
 * - Dar un tipo comun para servicios de traduccion y vistas.
 * - Evitar strings sueltos al cambiar el idioma activo.
 *
 * Related elements:
 * - MicroverseLanguageExtensions
 * - LocalizedText
 * - UiTextCatalog
 */
namespace Microverse.Data
{
    public enum MicroverseLanguage
    {
        Spanish,
        English,
        Portuguese
    }
}
