using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{
  /// <summary>
  /// Represents how to save/load a list from bytes
  /// </summary>
  /// <typeparam name="T">The type of the elements in the list</typeparam>
  public class ListBinaryConverter<T> : BinaryConverter<List<T>>
  {
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="elementConverter">The converter to use for each element. null indicates use the default converter</param>
    public ListBinaryConverter(BinaryConverter<T> elementConverter = null)
    {
      ElementConverter = elementConverter ?? (BinaryConverter<T>)GetConverter(typeof(T));
    }

    /// <summary>
    /// The binary converter to use for each element
    /// </summary>
    public BinaryConverter<T> ElementConverter { get; protected set; }

    /// <summary>
    /// Variable bit size
    /// </summary>
    public override int Bits => 0;

    /// <inheritdoc />
    public override void Write(List<T> value, BitWriter writer)
    {
      writer.Write((uint)value.Count);
      value.ForEach(element => ElementConverter.Write(element, writer));
    }

    /// <inheritdoc />
    public override List<T> Read(BitReader reader)
    {
      var count = reader.Read<uint>();
      var result = new List<T>();
      for (int i = 0; i < count; i++)
      {
        result.Add(ElementConverter.Read(reader));
      }
      return result;
    }
  }
}
