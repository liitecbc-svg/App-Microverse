using System.Collections.Generic;
using Microverse.Data;

namespace Microverse.Services
{
    public class UiTextCatalog
    {
        private readonly Dictionary<string, string> sourceTexts = new Dictionary<string, string>
        {
            { "nav.home", "Home" },
            { "nav.categories", "Categories" },
            { "nav.scan", "Scan AR" },
            { "nav.learn", "Learn" },
            { "nav.profile", "Profile" },
            { "common.search", "Search" },
            { "common.menu", "Menu" },
            { "common.details", "Details" },
            { "home.hero", "Explore the microscopic world\nin 3D and Augmented Reality" },
            { "home.feature.ar.title", "AR Viewer" },
            { "home.feature.ar.subtitle", "View in your space" },
            { "home.feature.library.title", "3D Library" },
            { "home.feature.library.subtitle", "Explore in 3D" },
            { "home.feature.quiz.title", "Quiz" },
            { "home.feature.quiz.subtitle", "Test knowledge" },
            { "home.feature.favorites.title", "Favorites" },
            { "home.feature.favorites.subtitle", "Saved models" },
            { "home.search.placeholder", "Search by common or scientific name" },
            { "home.filter.all", "All" },
            { "home.filter.cells", "Cells" },
            { "home.filter.protozoans", "Protozoans" },
            { "home.filter.viruses", "Viruses" },
            { "home.filter.bacteria", "Bacteria" },
            { "home.section.life", "Explore life in miniature" },
            { "model.favorite", "Favorite" },
            { "model.view_ar", "View in AR" },
            { "detail.back.types", "Types of Cells" },
            { "detail.categories", "Categories" },
            { "detail.swipe", "Swipe to explore" },
            { "detail.view_3d", "View in 3D" },
            { "detail.view_ar", "View in AR" },
            { "detail.about_prefix", "About " },
            { "placeholder.categories.title", "Categories" },
            { "placeholder.learn.title", "User Manual" },
            { "placeholder.profile.title", "Credits" },
            { "placeholder.ar.title", "AR Module" },
            { "placeholder.categories.body", "The catalog will be organized by taxonomic classification when backend filters are connected." },
            { "placeholder.learn.body", "Reserved space for the integrated manual and quick AR deployment guide." },
            { "placeholder.profile.body", "Prepared section for ULS, LIITEC, Dra. Cassia Yano, and collaborator logos." },
            { "placeholder.ar.body", "The AR view is ready to connect later with physical markers, touchable organelles, and dynamic model downloads." }
        };

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

        public UiTextCatalog()
        {
            translatedTexts[MicroverseLanguage.Spanish] = new Dictionary<string, string>(spanishTexts);
        }

        public IEnumerable<string> Keys
        {
            get { return sourceTexts.Keys; }
        }

        public string Get(string key, MicroverseLanguage language)
        {
            if (language != MicroverseLanguage.English &&
                translatedTexts.TryGetValue(language, out Dictionary<string, string> translations) &&
                translations.TryGetValue(key, out string translated))
            {
                return translated;
            }

            return sourceTexts.TryGetValue(key, out string source) ? source : key;
        }

        public string GetSource(string key)
        {
            return sourceTexts.TryGetValue(key, out string source) ? source : key;
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
