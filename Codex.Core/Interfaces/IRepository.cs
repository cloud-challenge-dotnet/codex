using System.Threading.Tasks;

namespace Codex.Core.Interfaces
{   
    public interface IRepository<TDocument, TId>
    {
        Task<bool> ExistsByIdAsync(TId id);

        Task<TDocument?> FindOneAsync(TId id);

        Task<TDocument> InsertAsync(TDocument document);
    }
}
