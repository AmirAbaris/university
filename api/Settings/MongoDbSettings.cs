using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Settings;

public class MongoDbSettings : IMongoDbSettings
{
    public string? ConnectionString { get; init; }
    public string? DatabaseName { get; init; }
}
