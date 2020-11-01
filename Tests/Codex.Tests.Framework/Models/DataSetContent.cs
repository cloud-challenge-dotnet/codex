﻿using MongoDB.Bson;
using System.Collections.Generic;

namespace Codex.Tests.Framework.Models
{
    public record DataSetContent
    {
        public string? CollectionName { get; set; }

        public string? Data { get; set; }
    }
}
