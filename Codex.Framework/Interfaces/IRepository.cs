using System.Threading.Tasks;

namespace Codex.Core.Interfaces
{   
    public interface IRepository<TDocument>
    {
        Task<bool> ExistsByIdAsync(string id);

        Task<TDocument> FindOneAsync(string id);

        Task<TDocument> InsertAsync(TDocument tenant);
    }
}
