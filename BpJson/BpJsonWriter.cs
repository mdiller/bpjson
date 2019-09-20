using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BpJson.BitPacking;
using BpJson.TokenPackers;
using Newtonsoft.Json.Linq;

namespace BpJson
{
  /// <summary>
  /// Writes a JToken to bpjson
  /// </summary>
  public class BpJsonWriter : BitWriter
  {
    /// <summary>
    /// The serializer settings to tell us how to write tokens
    /// </summary>
    public BpJsonSerializerSettings Settings { get; set; }


    /// <summary>
    /// The serializer settings to tell us how to write tokens
    /// </summary>
    public BpLogger Logger { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public BpJsonWriter()
    {
      Settings = new BpJsonSerializerSettings();
      Logger = new BpLogger();
    }

    /// <summary>
    /// Writes the header (initializes the tokenconverters if applicable)
    /// </summary>
    public void WriteHeader(JToken rootToken)
    {
      Settings.TokenConverters.ForEach(c =>
      {
        Logger.StartWriteHeader(c.BpJsonTokenType, this);
        c.WriteTokenHeader(rootToken, this);
        Logger.EndWriteHeader(c.BpJsonTokenType, this);
      });
    }

    /// <summary>
    /// Writes a json token
    /// </summary>
    public void WriteToken(JToken token)
    {
      var converter = Settings.GetConverter(token.Type);
      Logger.StartWriteToken(converter.BpJsonTokenType, this);
      converter.WriteToken(token, this);
      Logger.EndWriteToken(converter.BpJsonTokenType, this);
    }

    /// <summary>
    /// Writes the given token to bytes
    /// </summary>
    /// <param name="token">The token to write</param>
    /// <param name="gzip">Whether or not to gzip the bytes</param>
    /// <returns>The written bytes</returns>
    public static List<byte> Convert(JToken token, bool gzip = false)
    {
      var writer = new BpJsonWriter();
      writer.WriteHeader(token);
      writer.WriteToken(token);
      writer.Logger.Print();
      return gzip ? writer.Data.Gzip() : writer.Data;
    }

    /// <summary>
    /// Writes the given token to a file
    /// </summary>
    /// <param name="path">The name of the file to write to</param>
    /// <param name="token">The token to write</param>
    public static void WriteToFile(string path, JToken token)
    {
      File.WriteAllBytes(path, Convert(token, true).ToArray());
    }
  }
}
