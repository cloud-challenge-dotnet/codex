using System.Threading.Tasks;

namespace Codex.Core.RazorHelpers.Interfaces
{
    public interface IRazorPartialToStringRenderer
    {
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
    }
}
