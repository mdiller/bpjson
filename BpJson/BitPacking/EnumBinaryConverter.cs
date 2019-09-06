using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BpJson.BitPacking
{
  /// <summary>
  /// A converter that convert the given enum to and from bits
  /// </summary>
  /// <typeparam name="T">The enum type to convert</typeparam>
  public class EnumBinaryConverter<T> : BinaryConverter<T> where T : struct, IConvertible
  {
    /// <summary>
    /// ctor
    /// </summary>
    public EnumBinaryConverter()
    {
      if (!typeof(T).IsEnum)
      {
        throw new ArgumentException($"The given type '{typeof(T).Name}' must be an enum");
      }
      var maxCountAttribute = typeof(T).GetCustomAttributes(typeof(EnumMaxCount), true).FirstOrDefault() as EnumMaxCount;
      if (maxCountAttribute == null)
      {
        throw new ArgumentException($"The given enum '{typeof(T).Name}' has not specified an {nameof(EnumMaxCount)} attribute");
      }
      if (maxCountAttribute.Count < Enum.GetNames(typeof(T)).Length)
      {
        throw new ArgumentException($"The given enum '{typeof(T).Name}' has specified a max value count that is smaller than it's actual value count");
      }
      internalConverter = new ConstrainedUIntBinaryConverter((int)(maxCountAttribute.Count - 1));
    }

    private readonly ConstrainedUIntBinaryConverter internalConverter;

    /// <summary>
    /// The length in bits that will be used to contain this type. 0 if length is variable.
    /// </summary>
    public override int Bits => internalConverter.Bits;

    /// <inheritdoc />
    public override void Write(T value, BitWriter writer)
    {
      internalConverter.Write(Convert.ToInt32(value), writer);
    }

    /// <inheritdoc />
    public override T Read(BitReader reader)
    {
      return (T)Enum.ToObject(typeof(T), internalConverter.Read(reader));
    }
  }
}
