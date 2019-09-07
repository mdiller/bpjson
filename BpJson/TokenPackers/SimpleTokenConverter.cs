using System;
using System.Collections.Generic;
using System.Text;
using BpJson.BitPacking;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// 
  /// </summary>
  public class SimpleTokenConverter : BaseTokenConverter
  {
    /// <summary>
    /// The lambda function which is actually used for writing the token to bytes
    /// </summary>
    public Action<JToken, BpJsonWriter> Write { get; set; }

    /// <summary>
    /// The lambda function which is actually used for reading the token from bytes
    /// </summary>
    public Func<BpJsonReader, JToken> Read { get; set; }

    /// <inheritdoc cref="BaseTokenConverter.BpJsonTokenType"/>
    public BpJsonToken BpJsonToken { get; set; }

    /// <inheritdoc cref="BaseTokenConverter.JsonTokenTypes"/>
    public List<JTokenType> JsonTokens { get; set; }

    /// <inheritdoc />
    public override BpJsonToken BpJsonTokenType => BpJsonToken;

    /// <inheritdoc />
    public override List<JTokenType> JsonTokenTypes => JsonTokens;

    /// <inheritdoc />
    public override void WriteToken(JToken token, BpJsonWriter writer)
    {
      Write(token, writer);
    }

    /// <inheritdoc />
    public override JToken ReadToken(BpJsonReader reader)
    {
      return Read(reader);
    }

    /// <summary>
    /// The converter for a null token
    /// </summary>
    public static SimpleTokenConverter NullTokenConverter => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Null,
      JsonTokens = new List<JTokenType> { JTokenType.Null },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Null);
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        return null;
      }
    };

    /// <summary>
    /// The converter for a bool token
    /// </summary>
    public static SimpleTokenConverter BooleanTokenConverter => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Boolean,
      JsonTokens = new List<JTokenType> { JTokenType.Boolean },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Boolean);
        writer.WriteBit(token.Value<bool>());
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        return reader.ReadBit();
      }
    };

    /// <summary>
    /// The converter for an array token
    /// </summary>
    public static SimpleTokenConverter ArrayTokenConverter => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Array,
      JsonTokens = new List<JTokenType> { JTokenType.Array },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Array);
        var array = token as JArray;
        foreach (var item in array)
        {
          writer.WriteToken(item);
        }
        writer.Write(BpJsonToken.EndOfList);
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        var array = new JArray();
        while (true)
        {
          var elementType = reader.Read<BpJsonToken>();
          if (elementType == BpJsonToken.EndOfList)
          {
            return array;
          }
          reader.Rewind(typeof(BpJsonToken));
          array.Add(reader.ReadToken());
        }
      }
    };
  }
}
