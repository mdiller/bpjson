using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpJson.BitPacking
{
  /// <summary>
  /// Used for serializing an array of objects to an array of bytes. See <see cref="BinaryConverter"/> for more info on convertable types
  /// </summary>
  public class BitWriter
  {
    /// <summary>
    /// The bit offset that our 'cursor' is currently pointing to in the last byte
    /// </summary>
    protected int Offset { get; set; }

    /// <summary>
    /// The current list of bytes
    /// </summary>
    public List<byte> Data { get; }

    /// <summary>
    /// ctor
    /// </summary>
    public BitWriter()
    {
      Data = new List<byte>();
      Offset = 0;
    }

    /// <summary>
    /// The current number of written bits
    /// </summary>
    public int BitPosition => (Data.Count * 8) - (Offset == 0 ? 0 : 8 - Offset);

    /// <summary>
    /// The last byte in data, which is the one we will be writing to
    /// </summary>
    private byte LastByte
    {
      get => Data[Data.Count - 1];
      set => Data[Data.Count - 1] = value;
    }

    /// <summary>
    /// Writes a byte to the block
    /// </summary>
    /// <param name="b">the byte to write</param>
    public void WriteByte(byte b)
    {
      if (Offset == 0)
      {
        Data.Add(b);
      }
      else
      {
        LastByte |= (byte)(b << Offset);
        Data.Add(0x00);
        LastByte |= (byte)(b >> (8 - Offset));
      }
    }

    /// <summary>
    /// Writes a list of bytes to the block in order
    /// </summary>
    /// <param name="bytes">the list of bytes to write</param>
    public void WriteBytes(IEnumerable<byte> bytes)
    {
      foreach (var b in bytes)
      {
        WriteByte(b);
      }
    }

    /// <summary>
    /// Writes a bit to the block
    /// </summary>
    /// <param name="bit">the bit to write (true if 1, false if 0)</param>
    public void WriteBit(bool bit)
    {
      if (Offset == 0)
      {
        Data.Add(0x00);
      }

      LastByte |= (byte)((bit ? 1 : 0) << Offset);

      Offset++;
      if (Offset > 7)
      {
        Offset = 0;
      }
    }

    /// <summary>
    /// Writes the given object to the block, if it is a convertable type.
    /// </summary>
    /// <returns>The object to write to the block</returns>
    public void Write<T>(T obj)
    {
      WriteObject(obj, typeof(T));
    }

    /// <summary>
    /// Writes the given object to the block, if it is a convertable type.
    /// </summary>
    /// <param name="obj">The object to write</param>
    /// <param name="type">The type of the object to write (this must be supplied because we could pass in null in the obj field)</param>
    public void WriteObject(object obj, Type type)
    {
      BinaryConverter.WriteObject(obj, this, type);
    }

    /// <summary>
    /// Writes the given object to the block, given a converter
    /// </summary>
    /// <param name="obj">The object to write</param>
    /// <param name="converter">The converter used to write the object</param>
    public void WriteObject(object obj, BinaryConverter converter)
    {
      converter.WriteGeneric(obj, this);
    }
  }
}
