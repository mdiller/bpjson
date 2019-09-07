using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// Represents the various kinds of strategies for converting object json tokens
  /// </summary>
  public static class ObjectTokenConverters
  {
    /// <summary>
    /// The most simple converter. Encodes the keys directly as strings in place
    /// </summary>
    public static SimpleTokenConverter Simple => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Object,
      JsonTokens = new List<JTokenType> { JTokenType.Object },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Object);
        var obj = token as JObject;
        foreach (var prop in obj.Properties())
        {
          writer.Write(BpJsonToken.Object);
          writer.Write(prop.Name);
          writer.WriteToken(prop.Value);
        }
        writer.Write(BpJsonToken.EndOfList);
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        var obj = new JObject();
        while (true)
        {
          var elementType = reader.Read<BpJsonToken>();
          if (elementType == BpJsonToken.EndOfList)
          {
            return obj;
          }
          var key = reader.Read<string>();
          obj.Add(key, reader.ReadToken());
        }
      }
    };

    /// <summary>
    /// Creates a map of keys that is stored at the beginning of the file
    /// </summary>
    public static BaseTokenConverter KeyMapping => new VariableTokenConverter<StringsMap>
    {
      BpJsonToken = BpJsonToken.Object,
      JsonTokens = new List<JTokenType> { JTokenType.Object },
      GetVariable = StringsMap.BuildFromJTokenKeys,
      Write = (token, writer, keyMap) =>
      {
        writer.Write(BpJsonToken.Object);
        var obj = token as JObject;
        foreach (var prop in obj.Properties())
        {
          keyMap.WriteString(prop.Name, writer);
          writer.WriteToken(prop.Value);
        }
        keyMap.WriteEndOfObject(writer);
      },
      Read = (reader, keyMap) =>
      {
        reader.Read<BpJsonToken>();
        var obj = new JObject();
        while (true)
        {
          var key = keyMap.ReadString(reader);
          if (key == null)
          {
            return obj;
          }
          obj.Add(key, reader.ReadToken());
        }
      }
    };
  }
}
