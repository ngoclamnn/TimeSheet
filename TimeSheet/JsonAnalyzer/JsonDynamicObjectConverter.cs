using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet.JsonAnalyzer
{
    public class JsonDynamicObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JsonDynamicObject) || objectType == typeof(List<JsonDynamicObject>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //reader.DateParseHandling =  DateParseHandling.None;
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadList(reader);
                default:
                    if (IsPrimitiveToken(reader.TokenType))
                    {
                        return reader.Value;
                    }

                    throw new JsonSerializationException(string.Format("Unexpected token when converting ExpandoObject: {0}",CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private bool IsPrimitiveToken(JsonToken tokenType)
        {
            switch (tokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return true;
                default:
                    return false;
            }
        }

        private List<object> ReadList(JsonReader reader)
        {
            var list = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        var v = ReadValue(reader);

                        list.Add(v);
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }

           throw new JsonSerializationException("Unexpected end when reading ExpandoObject.");
        }

        private JsonDynamicObject ReadObject(JsonReader reader)
        {
            dynamic expandoObject = new JsonDynamicObject();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new Exception("Unexpected end when reading ExpandoObject.");
                        }

                        object v = ReadValue(reader);

                        expandoObject.Add(propertyName,v);
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return expandoObject;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading ExpandoObject.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
     
        }

    }
}
