using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BpJson.BitPacking
{
  /// <summary>
  /// Stores a integer in an efficient way so larger sizes do not take up as much space
  /// </summary>
  /// <remarks>
  /// In the first 3 bits, a value x is encoded where 2^x is the number of bits that follow which are used to represent the actual value.
  /// This means that the smaller the value is, the less space it will take up
  /// This allows for 0 and 1 to be encoded in 4 bits, while still allowing for a max value of 2^64 - 1 to be encoded in 67 bits.
  /// </remarks>
  public class VariableUIntBinaryConverter : BinaryConverter<ulong>
  {
    /// <summary>
    /// The converter for reading and writing the size value
    /// </summary>
    private ConstrainedUIntBinaryConverter sizeConverter =
      ConstrainedUIntBinaryConverter.FromMaxValue(6);

    /// <summary>
    /// Indicates a variable bit size
    /// </summary>
    public override int Bits => 0;

    /// <inheritdoc />
    public override void Write(ulong value, BitWriter writer)
    {
      var bitCount = ConstrainedUIntBinaryConverter.MaxValueToBitCount(value);
      bitCount = bitCount == 0 ? 1 : bitCount; // bitcount starts at 1
      var bitCountExponent = Math.Ceiling(Math.Log(bitCount, 2));
      sizeConverter.Write((ulong)bitCountExponent, writer);
      bitCount = (int)Math.Pow(2, bitCountExponent); // gotta go up to the next available one
      var converter = ConstrainedUIntBinaryConverter.FromBitCount(bitCount);
      converter.Write(value, writer);
    }

    /// <inheritdoc />
    public override ulong Read(BitReader reader)
    {
      var bitCountExponent = sizeConverter.Read(reader);
      var bitCount = (int)Math.Pow(2, bitCountExponent);
      var converter = ConstrainedUIntBinaryConverter.FromBitCount(bitCount);
      return converter.Read(reader);
    }
  }
}
