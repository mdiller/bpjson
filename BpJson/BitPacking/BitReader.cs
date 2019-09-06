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
  /// Used for reading an array of objects from an array of bytes. See <see cref="BinaryConverter"/> for more info on convertable types
  /// </summary>
  public class BitReader
  {
    /// <summary>
    /// The data being read
    /// </summary>
    private List<byte> Data { get; }

    /// <summary>
    /// The bit offset into the current byte
    /// </summary>
    private int Offset { get; set; }

    /// <summary>
    /// The index of the current byte
    /// </summary>
    private int Index { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="bytes">The bytes to read</param>
    public BitReader(IEnumerable<byte> bytes)
    {
      Data = bytes.ToList();
      Offset = 0;
      Index = 0;
    }

    /// <summary>
    /// The byte we are currently reading from
    /// </summary>
    private byte CurrentByte => Data[Index];

    /// <summary>
    /// Checks to make sure that the index points to a byte that exists
    /// </summary>
    private void VerifyIndex()
    {
      if (Data.Count <= Index)
      {
        throw new Exception("Out of range: Tried to read bitblock data when there is none left in buffer");
      }
    }

    /// <summary>
    /// Resets the reading to start again at the beginning of the block of bytes
    /// </summary>
    public void ResetReading()
    {
      Index = 0;
      Offset = 0;
    }

    /// <summary>
    /// Reads a byte from the block
    /// </summary>
    /// <returns>The byte</returns>
    public byte ReadByte()
    {
      byte value = 0x00;
      if (Offset == 0)
      {
        VerifyIndex();
        value = CurrentByte;
        Index++;
      }
      else
      {
        VerifyIndex();
        value |= (byte)(CurrentByte >> Offset);
        Index++;
        VerifyIndex();
        value |= (byte)(CurrentByte << (8 - Offset));
      }
      return value;
    }

    /// <summary>
    /// Reads a number of bytes from the block
    /// </summary>
    /// <param name="numBytes">The number of bytes to read</param>
    /// <returns>The list of bytes read</returns>
    public IEnumerable<byte> ReadBytes(int numBytes)
    {
      List<byte> bytes = new List<byte>();
      for (int i = 0; i < numBytes; i++)
      {
        bytes.Add(ReadByte());
      }
      return bytes;
    }

    /// <summary>
    /// Reads a single bit from the input buffer
    /// </summary>
    /// <returns>True if the bit is 1, False if the bit is 0</returns>
    public bool ReadBit()
    {
      if (Offset == 0)
      {
        VerifyIndex();
      }

      byte mask = (byte) (1 << Offset);
      bool value = (CurrentByte & mask) != 0;

      Offset++;
      if (Offset > 7)
      {
        Offset = 0;
        Index++;
      }
      return value;
    }

    /// <summary>
    /// Reads an object of a given type from the block
    /// </summary>
    /// <returns>The object read from the block</returns>
    public T Read<T>()
    {
      return (T)ReadObject(typeof(T));
    }

    /// <summary>
    /// Reads an object of a given type from the block
    /// </summary>
    /// <param name="type">The type to read</param>
    /// <returns>The object read from the block</returns>
    public object ReadObject(Type type)
    {
      return BinaryConverter.ReadObject(this, type);
    }

    /// <summary>
    /// Rewind the given number of bits so that we can read them again
    /// </summary>
    /// <param name="bits">The number of bits to rewind</param>
    public void Rewind(int bits)
    {
      while (bits > Offset)
      {
        bits -= 8;
        Index--;
      }
      Offset -= bits;
      VerifyIndex();
    }

    /// <summary>
    /// Rewind the amount of bits it takes to read one of these types
    /// </summary>
    /// <param name="type">The type of item we are rewinding</param>
    public void Rewind(Type type)
    {
      var converter = BinaryConverter.GetConverter(type);
      if (converter.Bits == 0)
      {
        throw new ArgumentException("Cant rewind a variable number of bits");
      }
      Rewind(converter.Bits);
    }

    /// <summary>
    /// Reads an object to a block using the given converter
    /// </summary>
    /// <param name="converter">The converter used to read the object</param>
    /// <returns>The object read from the block</returns>
    public object ReadObject(BinaryConverter converter)
    {
      return converter.ReadGeneric(this);
    }
  }
}
