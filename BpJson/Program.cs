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
      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        DateParseHandling = DateParseHandling.None,
      };

      var json = (JToken)JsonConvert.DeserializeObject((File.ReadAllText("TestData/example1.json")));



      //var outBytes = SerializeJson(json);

      //File.WriteAllBytes("out.bpjson", outBytes.ToArray());

      //var outJson = DeserializeJson(outBytes);

      //File.WriteAllText("out.json", outJson.ToString());

      BpJsonWriter.WriteToFile("out.bpjson", json);

      var outJson = BpJsonReader.ReadFromFile("out.bpjson");

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
          writer.Write(BpJsonToken.Object);
          var obj = token as JObject;
          foreach (var prop in obj.Properties())
          {
            writer.Write(BpJsonToken.Object); // placeholder for now. later will use key indexing here
            writer.Write(prop.Name);
            SerializeJsonNode(prop.Value, writer);
          }
          writer.Write(BpJsonToken.EndOfList);
          break;
        case JTokenType.Array:
          writer.Write(BpJsonToken.Array);
          var array = token as JArray;
          foreach (var item in array)
          {
            SerializeJsonNode(item, writer);
          }
          writer.Write(BpJsonToken.EndOfList);
          break;
        case JTokenType.String:
          writer.Write(BpJsonToken.String);
          writer.Write(token.Value<string>());
          break;
        case JTokenType.Date:
          writer.Write(BpJsonToken.String);
          writer.Write(token.Value<string>());
          break;
        case JTokenType.Integer:
          writer.Write(BpJsonToken.Number);
          writer.WriteBit(true);
          writer.Write(token.Value<long>());
          break;
        case JTokenType.Float:
          writer.Write(BpJsonToken.Number);
          writer.WriteBit(false);
          writer.Write(token.Value<double>());
          break;
        case JTokenType.Boolean:
          writer.Write(BpJsonToken.Boolean);
          writer.WriteBit(token.Value<bool>());
          break;
        case JTokenType.Null:
          writer.Write(BpJsonToken.Null);
          break;
        default:
          throw new Exception($"Unexpected JTokenType: {token.Type}");
      }
    }


    public static JToken DeserializeJsonNode(BitReader reader, BpJsonToken? tokenType = null)
    {
      tokenType = tokenType ?? reader.Read<BpJsonToken>();
      switch (tokenType)
      {
        case BpJsonToken.Object:
          var obj = new JObject();
          while (true)
          {
            var elementType = reader.Read<BpJsonToken>();
            if (elementType == BpJsonToken.EndOfList)
            {
              return obj;
            }
            var key = reader.Read<string>();
            obj.Add(key, DeserializeJsonNode(reader));
          }
        case BpJsonToken.Array:
          var array = new JArray();
          while (true)
          {
            var elementType = reader.Read<BpJsonToken>();
            if (elementType == BpJsonToken.EndOfList)
            {
              return array;
            }
            array.Add(DeserializeJsonNode(reader, elementType));
          }
        case BpJsonToken.String:
          return reader.Read<string>();
        case BpJsonToken.Number:
          if (reader.ReadBit())
          {
            return reader.Read<long>();
          }
          else
          {
            return reader.Read<double>();
          }
        case BpJsonToken.Boolean:
          return reader.ReadBit();
        case BpJsonToken.Null:
          return null;
        default:
          throw new Exception($"Unexpected JsonItem: {tokenType}");
      }
    }
  }
}
