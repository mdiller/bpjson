using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BpJson.BitPacking;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
  /// <summary>
  /// Represents a map of unique strings
  /// </summary>
  [BinaryConverter(typeof(StringsMapBinaryConverter))]
  public class StringsMap
  {
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="strings">The unique list of strings</param>
    /// <param name="maxByteSize">The maximum byte size for storing each string</param>
    public StringsMap(List<string> strings, ulong maxByteSize)
    {
      Strings = strings;
      MaxByteSize = maxByteSize;
      IndexConverter = ConstrainedUIntBinaryConverter.FromMaxValue((ulong)Strings.Count);
    }

    /// <summary>
    /// The unique list of strings
    /// </summary>
    public List<string> Strings { get; }

    /// <summary>
    /// Gets the maximum size for a string in this map
    /// </summary>
    public ulong MaxByteSize { get; }

    /// <summary>
    /// The converter used to write the index value of a string in the map
    /// </summary>
    private ConstrainedUIntBinaryConverter IndexConverter { get; }

    /// <summary>
    /// Writes a string from the map to the bitwriter 
    /// </summary>
    /// <param name="str">The string to write</param>
    /// <param name="writer">The writer to write to</param>
    public void WriteString(string str, BitWriter writer)
    {
      var value = Strings.IndexOf(str) + 1;
      IndexConverter.Write((ulong)value, writer);
    }

    /// <summary>
    /// Reads a string from the map
    /// </summary>
    /// <param name="reader">The reader to read from</param>
    /// <returns>The read key, or null if this is the end of the object</returns>
    public string ReadString(BitReader reader)
    {
      var value = IndexConverter.Read(reader);
      return value == 0 ? null : Strings[(int)value - 1];
    }

    /// <summary>
    /// Writes the value indicating end of object
    /// </summary>
    /// <param name="writer">The writer to write to</param>
    public void WriteEndOfObject(BitWriter writer)
    {
      IndexConverter.Write(0, writer);
    }

    /// <summary>
    /// Builds a keymap from the given JToken
    /// </summary>
    /// <param name="token">The token to build from</param>
    /// <returns>The constructed keymap</returns>
    private static IEnumerable<string> KeysFromJToken(JToken token)
    {
      var result = new List<string>();
      if (token is JObject obj)
      {
        foreach (var prop in obj.Properties())
        {
          result.Add(prop.Name);
          result.AddRange(KeysFromJToken(prop.Value));
        }
      }
      else if (token is JArray array)
      {
        foreach (var element in array)
        {
          result.AddRange(KeysFromJToken(element));
        }
      }
      return result;
    }

    /// <summary>
    /// Builds a keymap from a jtoken
    /// </summary>
    /// <param name="rootToken">The root token to build from</param>
    /// <returns>The key map</returns>
    public static StringsMap BuildFromJTokenKeys(JToken rootToken)
    {
      var keys = KeysFromJToken(rootToken).Distinct().ToList();
      var maxByteSize = StringBinaryConverter.FindMaxByteSize(keys);
      return new StringsMap(keys, maxByteSize);
    }
  }

  /// <summary>
  /// The binary converter used to encode a stringkeymap
  /// </summary>
  public class StringsMapBinaryConverter : BinaryConverter<StringsMap>
  {
    /// <summary>
    /// Indicates a variable bit size
    /// </summary>
    public override int Bits => 0;

    /// <inheritdoc />
    public override void Write(StringsMap value, BitWriter writer)
    {
      writer.Write(value.MaxByteSize);
      var listConverter = new ListBinaryConverter<string>(new StringBinaryConverter(value.MaxByteSize));
      listConverter.Write(value.Strings, writer);
    }

    /// <inheritdoc />
    public override StringsMap Read(BitReader reader)
    {
      var maxByteSize = reader.Read<ulong>();
      var listConverter = new ListBinaryConverter<string>(new StringBinaryConverter(maxByteSize));
      var keys = listConverter.Read(reader);
      return new StringsMap(keys, maxByteSize);
    }
  }
}
