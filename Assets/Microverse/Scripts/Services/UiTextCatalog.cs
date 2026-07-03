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
            { "nav.learn", "Learn" },
            { "nav.profile", "Credits" },
            { "common.search", "Search" },
            { "common.menu", "Menu" },
            { "common.details", "Details" },
            { "common.close", "Close" },
            { "loading.catalog", "Loading catalog..." },
            { "home.hero", "Explore the microscopic world\nin 3D and Augmented Reality" },
            { "home.feature.ar.title", "AR Viewer" },
            { "home.feature.library.title", "3D Library" },
            { "home.feature.favorites.title", "Favorites" },
            { "model.download", "Download" },
            { "model.downloading", "Downloading {0}%" },
            { "home.search.placeholder", "Search by common or scientific name" },
            { "home.filter.all", "All" },
            { "home.filter.more", "More" },
            { "home.filter.cells", "Cells" },
            { "home.filter.protozoans", "Protozoans" },
            { "home.filter.viruses", "Viruses" },
            { "home.filter.bacteria", "Bacteria" },
            { "home.empty.favorites", "No favorite models yet" },
            { "home.empty.library", "No new models to download" },
            { "home.empty.ar", "No local AR models available" },
            { "detail.back.types", "Types of Cells" },
            { "detail.categories", "Categories" },
            { "detail.swipe", "Swipe to explore" },
            { "detail.view_3d", "View in 3D" },
            { "detail.view_ar", "View in AR" },
            { "detail.about_prefix", "About " },
            { "credits.developers", "Developers" },
            { "credits.academic_collaboration", "Academic collaboration" },
            { "credits.institutions", "Collaborating institutions" },
            { "ar.instruction_model", "Use 1 finger to rotate the model\nUse 2 fingers to resize" },
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
            { "nav.learn", "Aprender" },
            { "nav.profile", "Creditos" },
            { "common.search", "Buscar" },
            { "common.menu", "Menu" },
            { "common.details", "Detalles" },
            { "common.close", "Cerrar" },
            { "loading.catalog", "Cargando catalogo..." },
            { "home.hero", "Explora el mundo microscopico\nen 3D y Realidad Aumentada" },
            { "home.feature.ar.title", "Visor RA" },
            { "home.feature.library.title", "Biblioteca 3D" },
            { "home.feature.favorites.title", "Favoritos" },
            { "model.download", "Descargar" },
            { "model.downloading", "Descargando {0}%" },
            { "home.search.placeholder", "Buscar por nombre comun o cientifico" },
            { "home.filter.all", "Todos" },
            { "home.filter.more", "Mas" },
            { "home.filter.cells", "Celulas" },
            { "home.filter.protozoans", "Protozoos" },
            { "home.filter.viruses", "Virus" },
            { "home.filter.bacteria", "Bacterias" },
            { "home.empty.favorites", "Aun no hay modelos favoritos" },
            { "home.empty.library", "No hay modelos nuevos para descargar" },
            { "home.empty.ar", "No hay modelos locales para RA" },
            { "detail.back.types", "Tipos de celulas" },
            { "detail.categories", "Categorias" },
            { "detail.swipe", "Desliza para explorar" },
            { "detail.view_3d", "Ver en 3D" },
            { "detail.view_ar", "Ver en RA" },
            { "detail.about_prefix", "Acerca de " },
            { "credits.developers", "Desarrolladores" },
            { "credits.academic_collaboration", "Colaboracion academica" },
            { "credits.institutions", "Instituciones colaboradoras" },
            { "ar.instruction_model", "Usa 1 dedo para rotar el modelo\nUsa 2 dedos para cambiar el tamano" },
            { "placeholder.categories.title", "Categorias" },
            { "placeholder.learn.title", "Manual de uso" },
            { "placeholder.profile.title", "Creditos" },
            { "placeholder.ar.title", "Modulo RA" },
            { "placeholder.categories.body", "Aqui se organizara el catalogo por clasificacion taxonomica cuando el backend entregue filtros." },
            { "placeholder.learn.body", "Espacio reservado para el manual integrado y la guia rapida de despliegue RA." },
            { "placeholder.profile.body", "Seccion preparada para logos ULS, LIITEC, Dra. Cassia Yano y colaboradores." },
            { "placeholder.ar.body", "La vista RA queda lista para conectarse luego con marcadores fisicos, organelos tocables y descarga dinamica de modelos." }
        };

        private readonly Dictionary<string, string> portugueseTexts = new Dictionary<string, string>
        {
            { "nav.home", "Inicio" },
            { "nav.categories", "Categorias" },
            { "nav.learn", "Aprender" },
            { "nav.profile", "Creditos" },
            { "common.search", "Buscar" },
            { "common.menu", "Menu" },
            { "common.details", "Detalhes" },
            { "common.close", "Fechar" },
            { "loading.catalog", "Carregando catalogo..." },
            { "home.hero", "Explore o mundo microscopico\nem 3D e Realidade Aumentada" },
            { "home.feature.ar.title", "Visualizador RA" },
            { "home.feature.library.title", "Biblioteca 3D" },
            { "home.feature.favorites.title", "Favoritos" },
            { "model.download", "Baixar" },
            { "model.downloading", "Baixando {0}%" },
            { "home.search.placeholder", "Buscar por nome comum ou cientifico" },
            { "home.filter.all", "Todos" },
            { "home.filter.more", "Mais" },
            { "home.filter.cells", "Celulas" },
            { "home.filter.protozoans", "Protozoarios" },
            { "home.filter.viruses", "Virus" },
            { "home.filter.bacteria", "Bacterias" },
            { "home.empty.favorites", "Ainda nao ha modelos favoritos" },
            { "home.empty.library", "Nao ha novos modelos para baixar" },
            { "home.empty.ar", "Nao ha modelos locais para RA" },
            { "detail.back.types", "Tipos de celulas" },
            { "detail.categories", "Categorias" },
            { "detail.swipe", "Deslize para explorar" },
            { "detail.view_3d", "Ver em 3D" },
            { "detail.view_ar", "Ver em RA" },
            { "detail.about_prefix", "Sobre " },
            { "credits.developers", "Desenvolvedores" },
            { "credits.academic_collaboration", "Colaboracao academica" },
            { "credits.institutions", "Instituicoes colaboradoras" },
            { "ar.instruction_model", "Use 1 dedo para rotar o modelo\nUse 2 dedos para redimensionar" },
            { "placeholder.categories.title", "Categorias" },
            { "placeholder.learn.title", "Manual do usuario" },
            { "placeholder.profile.title", "Creditos" },
            { "placeholder.ar.title", "Modulo RA" },
            { "placeholder.categories.body", "O catalogo sera organizado por classificacao taxonomica quando o backend entregar filtros." },
            { "placeholder.learn.body", "Espaco reservado para o manual integrado e o guia rapido de implantacao RA." },
            { "placeholder.profile.body", "Secao preparada para logos ULS, LIITEC, Dra. Cassia Yano e colaboradores." },
            { "placeholder.ar.body", "A vista RA fica pronta para conectar depois com marcadores fisicos, organelos tocaveis e download dinamico de modelos." }
        };

        private readonly Dictionary<MicroverseLanguage, Dictionary<string, string>> translatedTexts =
            new Dictionary<MicroverseLanguage, Dictionary<string, string>>();

        public UiTextCatalog()
        {
            translatedTexts[MicroverseLanguage.Spanish] = new Dictionary<string, string>(spanishTexts);
            translatedTexts[MicroverseLanguage.Portuguese] = new Dictionary<string, string>(portugueseTexts);
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
