using System;
using System.Collections.Generic;
using System.IO;
using BpJson.BitPacking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BpJson
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        DateParseHandling = DateParseHandling.None
      };

      var json = (JToken)JsonConvert.DeserializeObject((File.ReadAllText("TestData/example1.json")));



      var outBytes = SerializeJson(json);

      File.WriteAllBytes("out.bpjson", outBytes.ToArray());

      var outJson = DeserializeJson(outBytes);

      File.WriteAllText("out.json", outJson.ToString());
    }

    public static List<byte> SerializeJson(JToken token)
    {
      var writer = new BitWriter();
      // do preliminary reading / checking
      // write header
      // call recursive function to write bytes
      SerializeJsonNode(token, writer);
      return writer.Data;
    }

    public static JToken DeserializeJson(List<byte> data)
    {
      var reader = new BitReader(data);
      return DeserializeJsonNode(reader);
    }

    public static void SerializeJsonNode(JToken token, BitWriter writer)
    {
      switch (token.Type)
      {
        case JTokenType.Object:
          writer.Write(JsonItem.Object);
          var obj = token as JObject;
          foreach (var prop in obj.Properties())
          {
            writer.Write(JsonItem.Object); // placeholder for now. later will use key indexing here
            writer.Write(prop.Name);
            SerializeJsonNode(prop.Value, writer);
          }
          writer.Write(JsonItem.EndOfItem);
          break;
        case JTokenType.Array:
          writer.Write(JsonItem.Array);
          var array = token as JArray;
          foreach (var item in array)
          {
            SerializeJsonNode(item, writer);
          }
          writer.Write(JsonItem.EndOfItem);
          break;
        case JTokenType.String:
          writer.Write(JsonItem.String);
          writer.Write(token.Value<string>());
          break;
        case JTokenType.Date:
          writer.Write(JsonItem.String);
          writer.Write(token.Value<string>());
          break;
        case JTokenType.Integer:
          writer.Write(JsonItem.Number);
          writer.WriteBit(true);
          writer.Write(token.Value<long>());
          break;
        case JTokenType.Float:
          writer.Write(JsonItem.Number);
          writer.WriteBit(false);
          writer.Write(token.Value<double>());
          break;
        case JTokenType.Boolean:
          writer.Write(JsonItem.Boolean);
          writer.WriteBit(token.Value<bool>());
          break;
        case JTokenType.Null:
          writer.Write(JsonItem.Null);
          break;
        default:
          throw new Exception($"Unexpected JTokenType: {token.Type}");
      }
    }


    public static JToken DeserializeJsonNode(BitReader reader, JsonItem? itemType = null)
    {
      itemType = itemType ?? reader.Read<JsonItem>();
      switch (itemType)
      {
        case JsonItem.Object:
          var obj = new JObject();
          while (true)
          {
            var elementType = reader.Read<JsonItem>();
            if (elementType == JsonItem.EndOfItem)
            {
              return obj;
            }
            var key = reader.Read<string>();
            obj.Add(key, DeserializeJsonNode(reader));
          }
        case JsonItem.Array:
          var array = new JArray();
          while (true)
          {
            var elementType = reader.Read<JsonItem>();
            if (elementType == JsonItem.EndOfItem)
            {
              return array;
            }
            array.Add(DeserializeJsonNode(reader, elementType));
          }
        case JsonItem.String:
          return reader.Read<string>();
        case JsonItem.Number:
          if (reader.ReadBit())
          {
            return reader.Read<long>();
          }
          else
          {
            return reader.Read<double>();
          }
        case JsonItem.Boolean:
          return reader.ReadBit();
        case JsonItem.Null:
          return null;
        default:
          throw new Exception($"Unexpected JsonItem: {itemType}");
      }
    }
  }
}
