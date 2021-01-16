using Codex.Models.Exceptions;

namespace Codex.Web.Services.Models.Exceptions
{
    public class NotFoundException : FunctionnalException
    {
        public NotFoundException(string entityId, string message) : base(message, code: "NOT_FOUND")
        {
            EntityId = entityId;
        }

        public string? EntityId { get; init; }
    }
}
