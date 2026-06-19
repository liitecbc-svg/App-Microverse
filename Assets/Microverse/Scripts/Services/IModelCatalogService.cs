using System;
using System.Collections.Generic;
using Microverse.Data;

namespace Microverse.Services
{
    public interface IModelCatalogService
    {
        IReadOnlyList<BiologicalModel> GetModels();
        void LoadModels(Action<IReadOnlyList<BiologicalModel>> onComplete, Action<string> onError);
        IReadOnlyList<string> GetCategories();
    }
}
