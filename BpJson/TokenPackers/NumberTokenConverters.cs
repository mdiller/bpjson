using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using BpJson.BitPacking;
using Newtonsoft.Json;
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

    private static Regex floatPattern = new Regex("(-?)(\\d*)\\.(\\d*)");
    private static VariableUIntBinaryConverter variableIntConverter = new VariableUIntBinaryConverter();

    /// <summary>
    /// Uses Variable sized integers for both ints and floats
    /// </summary>
    public static SimpleTokenConverter VariableNumbers => new SimpleTokenConverter
    {
      BpJsonToken = BpJsonToken.Number,
      JsonTokens = new List<JTokenType> { JTokenType.Integer, JTokenType.Float },
      Write = (token, writer) =>
      {
        writer.Write(BpJsonToken.Number);
        writer.WriteBit(token.Type == JTokenType.Integer);
        if (token.Type == JTokenType.Integer)
        {
          var value = token.Value<long>();
          writer.WriteBit(value > 0);
          variableIntConverter.Write((ulong)Math.Abs(value), writer);
        }
        else
        {
          var value = token.Value<string>();
          var match = floatPattern.Match(value);
          writer.WriteBit(match.Groups[1].Value != "-");
          var val1 = match.Groups[2].Value;
          variableIntConverter.Write(ulong.Parse(val1 != "" ? val1 : "0"), writer);
          var val2 = match.Groups[3].Value;
          val2 = val2.ReverseChars();
          variableIntConverter.Write(ulong.Parse(val2 != "" ? val2 : "0"), writer);
        }
      },
      Read = reader =>
      {
        reader.Read<BpJsonToken>();
        var isInt = reader.ReadBit();
        var signBit = reader.ReadBit();

        if (isInt)
        {
          var value = (long)variableIntConverter.Read(reader);
          return signBit ? value : 0 - value;
        }
        else
        {
          var signChar = signBit ? "" : "-";
          var val1 = variableIntConverter.Read(reader);
          var val2Backwards = variableIntConverter.Read(reader);
          var val2 = val2Backwards.ToString().ReverseChars();
          using (var w = new JTokenWriter())
          {
            w.WriteRaw($"{signChar}{val1}.{val2}");
            return w.CurrentToken;
          }
        }
      }
    };
  }
}
