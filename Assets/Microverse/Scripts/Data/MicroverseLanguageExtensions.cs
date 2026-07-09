/**
 * MicroverseLanguageExtensions.cs
 *
 * Agrega utilidades para convertir los idiomas internos de Microverse a codigos compatibles con servicios externos.
 *
 * Main responsibilities:
 * - Mapear idiomas a codigos ISO usados por ML Kit.
 * - Mantener la conversion de idioma en un unico lugar.
 * - Facilitar llamadas a servicios de traduccion automatica.
 *
 * Related elements:
 * - MicroverseLanguage
 * - AndroidMlKitTranslationService
 * - TranslationRequest
 */
namespace Microverse.Data
{
    public static class MicroverseLanguageExtensions
    {
        public static string ToLanguageCode(this MicroverseLanguage language)
        {
            switch (language)
            {
                case MicroverseLanguage.English:
                    return "en";
                case MicroverseLanguage.Portuguese:
                    return "pt";
                default:
                    return "es";
            }
        }
    }
}
