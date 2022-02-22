using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Models;

[ExcludeFromCodeCoverage]
public record MongoDbSettings
{
    //
    // Résumé :
    //     The connection string for the MongoDb server.
    public string? ConnectionString { get; init; }
    //
    // Résumé :
    //     The name of the MongoDb database where the identity data will be stored.
    public string? DatabaseName { get; init; }
}