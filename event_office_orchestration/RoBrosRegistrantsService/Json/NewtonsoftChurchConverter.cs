using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoBrosRegistrantsService.Models;

namespace RoBrosRegistrantsService.Json
{
    public class NewtonsoftChurchConverter : JsonConverter<Church>
    {
        public override Church? ReadJson(JsonReader reader, Type objectType, Church? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var name = (string?)reader.Value;
                return new Church { Name = name ?? string.Empty };
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                var jo = JObject.Load(reader);
                return jo.ToObject<Church>(serializer);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, Church? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // serialize full object
            serializer.Serialize(writer, value);
        }
    }
}
