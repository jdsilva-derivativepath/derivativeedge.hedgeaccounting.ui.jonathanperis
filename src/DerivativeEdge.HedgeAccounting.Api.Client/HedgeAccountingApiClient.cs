using DerivativeEdge.HedgeAccounting.Api.Client.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeEdge.HedgeAccounting.Api.Client
{
    public partial class HedgeAccountingApiClient
    {
        static partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.Converters.Add(new SafeDateTimeOffsetConverter());
            settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        }
    }
}