using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{ 
  /// <summary>
  /// Represents how to save/load an unsigned integer to/from bytes (values given are ints but theyre assumed to all be >= 0)
  /// </summary>
  public class ConstrainedUIntBinaryConverter : BinaryConverter<ulong>
  {
    /// <summary>
    /// ctor
    /// </summary>
    private ConstrainedUIntBinaryConverter(int bitCount)
    {
      BitCount = bitCount;
    }

    /// <summary>
    /// The maximum length of a string
    /// </summary>
    public int BitCount { get; }

    /// <summary>
    /// The length in bits that will be used to contain this type. 0 if length is variable.
    /// </summary>
    public override int Bits => BitCount;

    /// <inheritdoc />
    public override void Write(ulong value, BitWriter writer)
    {
      for (int i = 0; i < Bits; i++)
      {
        writer.WriteBit((value & ((ulong)1 << i)) != 0);
      }
    }

    /// <inheritdoc />
    public override ulong Read(BitReader reader)
    {
      ulong v = 0;
      for (var i = 0; i < Bits; i++)
      {
        // set bit then shift over
        if (reader.ReadBit())
        {
          v |= (ulong)1 << i;
        }
      }
      return v;
    }

    /// <summary>
    /// Gets the number of bits needed to represent the given max size
    /// </summary>
    /// <param name="maxValue">The maximum value needing to be stored</param>
    /// <returns>The bits needed to store that amount</returns>
    public static int MaxValueToBitCount(ulong maxValue)
    {
      return maxValue == 0 ? 0 : (int)Math.Log(maxValue, 2) + 1;
    }

    /// <summary>
    /// Creates a bitconverter where the passed in value is the number of bits we have available to us
    /// </summary>
    /// <param name="bitCount">The number of bits we are to write to</param>
    /// <returns>The bitconverter</returns>
    public static ConstrainedUIntBinaryConverter FromBitCount(int bitCount)
    {
      return new ConstrainedUIntBinaryConverter(bitCount);
    }

    /// <summary>
    /// Creates a bitconverter where the passed in value is the max value that will need to be represented
    /// </summary>
    /// <param name="maxValue">The max value that we are able to store</param>
    /// <returns>The bitconverter</returns>
    public static ConstrainedUIntBinaryConverter FromMaxValue(ulong maxValue)
    {
      return new ConstrainedUIntBinaryConverter(MaxValueToBitCount(maxValue));
    }
  }
}
