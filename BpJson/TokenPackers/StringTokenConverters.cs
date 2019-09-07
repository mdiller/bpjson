using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// Represents the various kinds of strategies for converting string json tokens
  /// </summary>
  public static class StringTokenConverters
  {
    /// <summary>
    /// The most simple converter. Directly encodes the strings in place with the default uint for the length
    /// </summary>
    public static SimpleTokenConverter Simple => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.String,
      JsonTokens = new List<JTokenType> { JTokenType.String, JTokenType.Date },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.String);
        writer.Write(token.Value<string>());
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        return reader.Read<string>();
      }
    };
  }
}
