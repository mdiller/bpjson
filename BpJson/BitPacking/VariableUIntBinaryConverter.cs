using System;
using System.Collections.Generic;
using System.Linq;
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
    /// ctor
    /// </summary>
    /// <param name="sections">The number of bits each section can hold</param>
    public VariableUIntBinaryConverter(int[] sections)
    {
      sizeConverter = ConstrainedUIntBinaryConverter.FromMaxValue((ulong)sections.Length - 1);
      Sections = sections;
      SectionConverters = sections.Select(ConstrainedUIntBinaryConverter.FromBitCount).ToArray();
      SectionStart = new int[Sections.Length];
      var value = 0;
      for (int i = 0; i < Sections.Length; i++)
      {
        SectionStart[i] = value;
        value += (int) Math.Pow(2, Sections[i]);
      }
    }

    /// <summary>
    /// The converter for reading and writing the size value
    /// </summary>
    private ConstrainedUIntBinaryConverter sizeConverter;

    /// <summary>
    /// The number of bits each section can hold
    /// </summary>
    public int[] Sections { get; }

    /// <summary>
    /// The value at which each section starts
    /// </summary>
    public int[] SectionStart { get; }

    /// <summary>
    /// The converters used by each section
    /// </summary>
    public ConstrainedUIntBinaryConverter[] SectionConverters { get; }

    /// <summary>
    /// Indicates a variable bit size
    /// </summary>
    public override int Bits => 0;

    /// <inheritdoc />
    public override void Write(ulong value, BitWriter writer)
    {
      var section = 0;
      while (section < Sections.Length - 1 && (ulong)SectionStart[section + 1] <= value)
      {
        section++;
      }
      sizeConverter.Write((ulong)section, writer);
      SectionConverters[section].Write(value - (ulong)SectionStart[section], writer);
    }

    /// <inheritdoc />
    public override ulong Read(BitReader reader)
    {
      var section = sizeConverter.Read(reader);
      return SectionConverters[section].Read(reader) + (ulong)SectionStart[section];
    }
  }
}
