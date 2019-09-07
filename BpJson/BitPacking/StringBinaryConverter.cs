using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BpJson.BitPacking
{
  /// <summary>
  /// Stores a string in binary, up to the max byte length specified
  /// </summary>
  public class StringBinaryConverter : BinaryConverter<string>
  {
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="maxByteLength">The maximum encoded byte size this converter can store</param>
    public StringBinaryConverter(ulong maxByteLength)
    {
      MaxByteLength = maxByteLength;
    }

    /// <summary>
    /// The text encoding that the binaryconverters use
    /// </summary>
    public static readonly Encoding TextEncoding = Encoding.UTF8;

    /// <summary>
    /// The maximum number of bytes that this converter can store
    /// </summary>
    public ulong MaxByteLength { get; }

    /// <summary>
    /// The value above the max byte length, which if read, indicates that the string is null
    /// </summary>
    public ulong NullIndicator => MaxByteLength + 1;

    /// <summary>
    /// The converter for reading and writing the size value
    /// </summary>
    public BinaryConverter<ulong> SizeConverter => ConstrainedUIntBinaryConverter.FromMaxValue(NullIndicator);

    /// <summary>
    /// Indicates a variable bit size
    /// </summary>
    public override int Bits => 0;

    /// <inheritdoc />
    public override void Write(string value, BitWriter writer)
    {
      if (value == null)
      {
        // Maxvalue indicates null
        SizeConverter.Write(NullIndicator, writer);
      }
      else
      {
        var data = TextEncoding.GetBytes(value);
        if ((ulong)data.Length > MaxByteLength)
        {
          throw new ArgumentException("String too long to be written to bytes using this converter");
        }
        // write byte count
        SizeConverter.Write((ulong)data.Length, writer);
        // write the encoded bytes
        writer.WriteBytes(data);
      }
    }

    /// <inheritdoc />
    public override string Read(BitReader reader)
    {
      var byteCount = SizeConverter.Read(reader);
      if (byteCount == NullIndicator)
      {
        return null;
      }
      return TextEncoding.GetString(reader.ReadBytes((int)byteCount).ToArray());
    }

    /// <summary>
    /// Gets the max byte size needed to store the largest of the given strings
    /// </summary>
    /// <param name="strings">The strings that will need to be stored</param>
    /// <returns>The largest byte size that will be needed</returns>
    public static ulong FindMaxByteSize(List<string> strings)
    {
      return (ulong)strings.Select(s => TextEncoding.GetByteCount(s ?? "")).Max();
    }
  }
}
