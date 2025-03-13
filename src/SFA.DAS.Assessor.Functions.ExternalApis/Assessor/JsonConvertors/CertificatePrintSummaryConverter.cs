using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.JsonConvertors
{
    public class CertificatePrintSummaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CertificatePrintSummaryBase);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            string type = jsonObject["type"]?.ToString();

            CertificatePrintSummaryBase target;

            if (type == "Standard")
            {
                target = new CertificatePrintSummary();
            }
            else if (type == "Framework")
            {
                target = new FrameworkCertificatePrintSummary();
            }
            else
            {
                throw new JsonSerializationException($"Unknown certificate type: {type}");
            }

            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
