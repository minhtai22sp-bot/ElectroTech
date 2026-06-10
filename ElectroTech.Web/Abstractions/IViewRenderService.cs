using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ElectroTech.Web.Abstractions
{
    public interface IViewRenderService
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, ViewDataDictionary viewDictionary = null);
        Task<string> RenderViewnoModelToStringAsync(string pageName, ViewDataDictionary viewDictionary = null);
    }
}
