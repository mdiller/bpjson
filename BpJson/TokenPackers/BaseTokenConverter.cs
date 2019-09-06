using System;
using System.Collections.Generic;
using System.Text;
using BpJson.BitPacking;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// A class that can pack and unpack json data for a specific token type
  /// </summary>
  public abstract class BaseTokenConverter
  {
    /// <summary>
    /// The type of bp token that this converter can read/write
    /// </summary>
    public abstract BpJsonToken BpJsonTokenType { get; }

    /// <summary>
    /// The type of json tokens that this converter can read/write
    /// </summary>
    public abstract List<JTokenType> JsonTokenTypes { get; }

    /// <summary>
    /// Writes the header data if this converter uses it, and initializes the converter for this object
    /// </summary>
    /// <param name="rootToken">The root json object that is to be written</param>
    /// <param name="writer">The writer to write to</param>
    public virtual void WriteTokenHeader(JToken rootToken, BpJsonWriter writer) { }

    /// <summary>
    /// Reads the header data if this converter uses it, and initializes the converter for reading this object
    /// </summary>
    /// <param name="reader">The bitreader used for reading this object</param>
    public virtual void ReadTokenHeader(BpJsonReader reader) { }

    /// <summary>
    /// Writes the given token to the bitwriter
    /// </summary>
    /// <param name="token">The token to write</param>
    /// <param name="writer">The writer to use</param>
    public abstract void WriteToken(JToken token, BpJsonWriter writer);

    /// <summary>
    /// Reads the token from the bitreader
    /// </summary>
    /// <param name="reader">The reader to read from</param>
    /// <returns>The read token</returns>
    public abstract JToken ReadToken(BpJsonReader reader);
  }
}
