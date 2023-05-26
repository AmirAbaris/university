using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Settings;

public interface IMongoDbSettings
{
    string? ConnectionString { get; init; }
    string? DatabaseName { get; init; }
}
