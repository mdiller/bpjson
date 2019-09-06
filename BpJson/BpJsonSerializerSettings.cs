using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BpJson.TokenPackers;
using Newtonsoft.Json.Linq;

namespace BpJson
{
  /// <summary>
  /// Settings for how to serialize to and from BpJson
  /// </summary>
  public class BpJsonSerializerSettings
  {
    /// <summary>
    /// The converters used to read and write tokens
    /// </summary>
    public List<BaseTokenConverter> TokenConverters { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public BpJsonSerializerSettings()
    {
      TokenConverters = new List<BaseTokenConverter>
      {
        SimpleTokenConverter.NullTokenConverter,
        SimpleTokenConverter.BooleanTokenConverter,
        SimpleTokenConverter.NumberTokenConverter,
        SimpleTokenConverter.StringTokenConverter,
        SimpleTokenConverter.ArrayTokenConverter,
        SimpleTokenConverter.ObjectTokenConverter
      };
    }

    /// <summary>
    /// Gets a converter that matches the given token type
    /// </summary>
    /// <param name="tokenType">The token type to match</param>
    /// <returns>The converter</returns>
    public BaseTokenConverter GetConverter(JTokenType tokenType)
    {
      return TokenConverters.First(c => c.JsonTokenTypes.Contains(tokenType));
    }

    /// <summary>
    /// Gets a converter that matches the given token type
    /// </summary>
    /// <param name="tokenType">The token type to match</param>
    /// <returns>The converter</returns>
    public BaseTokenConverter GetConverter(BpJsonToken tokenType)
    {
      return TokenConverters.First(c => c.BpJsonTokenType == tokenType);
    }
  }
}
