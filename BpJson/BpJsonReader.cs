using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BpJson.BitPacking;
using Newtonsoft.Json.Linq;

namespace BpJson
{
  /// <summary>
  /// Reads bpjson into a JToken
  /// </summary>
  public class BpJsonReader : BitReader
  {
    /// <summary>
    /// The serializer settings to tell us how to read tokens
    /// </summary>
    public BpJsonSerializerSettings Settings { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public BpJsonReader(IEnumerable<byte> bytes) : base(bytes)
    {
      Settings = new BpJsonSerializerSettings();
    }

    /// <summary>
    /// Reads the header (initializes the tokenconverters if applicable)
    /// </summary>
    public void ReadHeader()
    {
      Settings.TokenConverters.ForEach(c => c.ReadTokenHeader(this));
    }

    /// <summary>
    /// Reads a json token
    /// </summary>
    public JToken ReadToken()
    {
      var tokenType = Read<BpJsonToken>();
      Rewind(typeof(BpJsonToken));
      return Settings.GetConverter(tokenType).ReadToken(this);
    }

    /// <summary>
    /// Reads the given bytes to jtoken
    /// </summary>
    /// <param name="data">The data to read in</param>
    /// <param name="gzipped">Whether or not the data has been gzipped, and needs to be unzipped</param>
    /// <returns>The constructed JToken</returns>
    public static JToken Convert(IEnumerable<byte> data, bool gzipped = false)
    {
      if (gzipped)
      {
        data = data.UnGzip();
      }
      var reader = new BpJsonReader(data);
      reader.ReadHeader();
      return reader.ReadToken();
    }

    /// <summary>
    /// Reads the jtoken from the given bpjson file
    /// </summary>
    /// <param name="path">The path to the bpjson file</param>
    /// <returns>The constructed JToken</returns>
    public static JToken ReadFromFile(string path)
    {
      return Convert(File.ReadAllBytes(path), true);
    }
  }
}
