using System.Threading.Tasks;

namespace Codex.Web.Services.Tools.Interfaces
{
    public interface IHttpManager
    {
        Task<T> GetAsync<T>(string uri);

        Task PostAsync(string uri, object value);

        Task<T> PostAsync<T>(string uri, object value);

        Task PutAsync(string uri, object value);

        Task<T> PutAsync<T>(string uri, object value);

        Task DeleteAsync(string uri);

        Task<T> DeleteAsync<T>(string uri);
    }
}
