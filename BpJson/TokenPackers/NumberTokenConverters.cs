using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// Represents the various kinds of strategies for converting number json tokens
  /// </summary>
  public static class NumberTokenConverters
  {
    /// <summary>
    /// A simple converter which converts longs directly to bytes, and doubles directly to bytes.
    /// Has some issues with doubles being read and written slightly differently
    /// </summary>
    public static SimpleTokenConverter Simple => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Number,
      JsonTokens = new List<JTokenType> { JTokenType.Integer, JTokenType.Float },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Number);
        if (token.Type == JTokenType.Integer)
        {
          writer.WriteBit(true);
          writer.Write(token.Value<long>());
        }
        else
        {
          writer.WriteBit(false);
          writer.Write(token.Value<double>());
        }
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        if (reader.ReadBit())
        {
          return reader.Read<long>();
        }
        else
        {
          return reader.Read<double>();
        }
      }
    };


    /// <summary>
    /// A simple converter which simply encodes the values as strings
    /// </summary>
    public static SimpleTokenConverter AsString => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Number,
      JsonTokens = new List<JTokenType> { JTokenType.Integer, JTokenType.Float },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Number);
        writer.Write(token.Value<string>());
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        return JToken.Parse(reader.Read<string>());
      }
    };
  }
}
