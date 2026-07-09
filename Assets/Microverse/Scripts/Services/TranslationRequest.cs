/**
 * TranslationRequest.cs
 *
 * Representa una solicitud individual de traduccion para procesar textos por lote.
 *
 * Main responsibilities:
 * - Guardar texto fuente y codigos de idioma.
 * - Transportar datos hacia ITranslationService.
 * - Permitir cachear traducciones por combinacion idioma-texto.
 *
 * Related elements:
 * - ITranslationService
 * - AndroidMlKitTranslationService
 * - UiTextCatalog
 */
namespace Microverse.Services
{
    public readonly struct TranslationRequest
    {
        public readonly string Text;
        public readonly string SourceLanguage;
        public readonly string TargetLanguage;

        public TranslationRequest(string text, string sourceLanguage, string targetLanguage)
        {
            Text = text;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
        }
    }
}
