using System.Collections.Generic;
using Microverse.Data;

namespace Microverse.Services
{
    public class UiTextCatalog
    {
        private readonly Dictionary<string, string> spanishTexts = new Dictionary<string, string>
        {
            { "nav.home", "Inicio" },
            { "nav.categories", "Categorias" },
            { "nav.scan", "Escanear RA" },
            { "nav.learn", "Aprender" },
            { "nav.profile", "Perfil" },
            { "common.search", "Buscar" },
            { "common.menu", "Menu" },
            { "common.details", "Detalles" },
            { "home.hero", "Explora el mundo microscopico\nen 3D y Realidad Aumentada" },
            { "home.feature.ar.title", "Visor RA" },
            { "home.feature.ar.subtitle", "Ver en tu espacio" },
            { "home.feature.library.title", "Biblioteca 3D" },
            { "home.feature.library.subtitle", "Explorar en 3D" },
            { "home.feature.quiz.title", "Quiz" },
            { "home.feature.quiz.subtitle", "Prueba conocimiento" },
            { "home.feature.favorites.title", "Favoritos" },
            { "home.feature.favorites.subtitle", "Modelos guardados" },
            { "home.search.placeholder", "Buscar por nombre comun o cientifico" },
            { "home.filter.all", "Todos" },
            { "home.filter.cells", "Celulas" },
            { "home.filter.protozoans", "Protozoos" },
            { "home.filter.viruses", "Virus" },
            { "home.filter.bacteria", "Bacterias" },
            { "home.section.life", "Explora vida en miniatura" },
            { "model.favorite", "Favorito" },
            { "model.view_ar", "Ver en RA" },
            { "detail.back.types", "Tipos de celulas" },
            { "detail.categories", "Categorias" },
            { "detail.swipe", "Desliza para explorar" },
            { "detail.view_3d", "Ver en 3D" },
            { "detail.view_ar", "Ver en RA" },
            { "detail.about_prefix", "Acerca de " },
            { "placeholder.categories.title", "Categorias" },
            { "placeholder.learn.title", "Manual de uso" },
            { "placeholder.profile.title", "Creditos" },
            { "placeholder.ar.title", "Modulo RA" },
            { "placeholder.categories.body", "Aqui se organizara el catalogo por clasificacion taxonomica cuando el backend entregue filtros." },
            { "placeholder.learn.body", "Espacio reservado para el manual integrado y la guia rapida de despliegue RA." },
            { "placeholder.profile.body", "Seccion preparada para logos ULS, LIITEC, Dra. Cassia Yano y colaboradores." },
            { "placeholder.ar.body", "La vista RA queda lista para conectarse luego con marcadores fisicos, organelos tocables y descarga dinamica de modelos." }
        };

        private readonly Dictionary<MicroverseLanguage, Dictionary<string, string>> translatedTexts =
            new Dictionary<MicroverseLanguage, Dictionary<string, string>>();

        public IEnumerable<string> Keys
        {
            get { return spanishTexts.Keys; }
        }

        public string Get(string key, MicroverseLanguage language)
        {
            if (language != MicroverseLanguage.Spanish &&
                translatedTexts.TryGetValue(language, out Dictionary<string, string> translations) &&
                translations.TryGetValue(key, out string translated))
            {
                return translated;
            }

            return spanishTexts.TryGetValue(key, out string spanish) ? spanish : key;
        }

        public string GetSpanish(string key)
        {
            return spanishTexts.TryGetValue(key, out string spanish) ? spanish : key;
        }

        public void Set(MicroverseLanguage language, string key, string value)
        {
            if (!translatedTexts.TryGetValue(language, out Dictionary<string, string> translations))
            {
                translations = new Dictionary<string, string>();
                translatedTexts[language] = translations;
            }

            translations[key] = value;
        }
    }
}
