namespace Codex.Core.Models
{
    public record MongoDbSettings
    {
        //
        // Résumé :
        //     The connection string for the MongoDb server.
        public string? ConnectionString { get; set; }
        //
        // Résumé :
        //     The name of the MongoDb database where the identity data will be stored.
        public string? DatabaseName { get; set; }
    }
}
