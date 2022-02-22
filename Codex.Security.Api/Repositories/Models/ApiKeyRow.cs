using Codex.Core.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Codex.Security.Api.Repositories.Models;

public record ApiKeyRow
{
    public ApiKeyRow()
        => (Id, Name, Roles) = (null, new(), new());

    public ApiKeyRow(string? id, TranslationDataRow name, List<string> roles)
        => (Id, Name, Roles) = (id, name, roles);

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string? Id { get; init; }

    public TranslationDataRow Name { get; init; }

    public List<string> Roles { get; init; }
}