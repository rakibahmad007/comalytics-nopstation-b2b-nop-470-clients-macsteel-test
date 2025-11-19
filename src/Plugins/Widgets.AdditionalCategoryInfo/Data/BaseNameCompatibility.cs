using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames =>
            new() { { typeof(AdditionalCategoryInfoData), "AdditionalCategoryInfo" } };

        public Dictionary<(Type, string), string> ColumnName => new() { };
    }
}
