using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Comalytics.DomainFilter.Domains;

namespace Nop.Plugin.Comalytics.DomainFilter.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(Domain), "CP_Domain" }
    };

    public Dictionary<(Type, string), string> ColumnName => new()
    {

    };
}
