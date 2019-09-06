using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BpJson.BitPacking
{
	/// <summary>
	/// The endianness
	/// </summary>
	public enum Endianness
	{
		/// <summary>
		/// Big Endian
		/// </summary>
		BigEndian,

		/// <summary>
		/// Little Endian
		/// </summary>
		LittleEndian
	}

	/// <summary>
	/// The BinaryConverter abstract base class. For serializing objects to and from bits/bytes
	/// </summary>
	public abstract class BinaryConverter
	{
		/// <summary>
		/// A cache of converters for various types
		/// </summary>
		private static readonly ConcurrentDictionary<Type, BinaryConverter> converterCache = new ConcurrentDictionary<Type, BinaryConverter>();

		/// <summary>
		/// The endianness of the bytes being read and written
		/// </summary>
		public static readonly Endianness Endianness = Endianness.LittleEndian;

		/// <summary>
		/// The text encoding that the binaryconverters use
		/// </summary>
		public static readonly Encoding TextEncoding = Encoding.UTF8;

		/// <summary>
		/// Writes the given object to the bitwriter
		/// </summary>
		/// <param name="obj">The object to write</param>
		/// <param name="writer">The writer to use for writing</param>
		/// <param name="type">The type of the object to write</param>
		public static void WriteObject(object obj, BitWriter writer, Type type)
		{
			GetConverter(type).WriteGeneric(obj, writer);
		}

		/// <summary>
		/// Reads an object of the given type from the bitreader
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="type">The type of the object to read</param>
		/// <returns>An object of the desired type</returns>
		public static object ReadObject(BitReader reader, Type type)
		{
			return GetConverter(type).ReadGeneric(reader);
		}

		/// <summary>
		/// Gets the converter corresponding to the given type
		/// </summary>
		/// <param name="t">The type of object that the converter should be able to read/write</param>
		/// <returns>The converter that was found</returns>
		public static BinaryConverter GetConverter(Type t)
		{
			var converter = SimpleConverters.FirstOrDefault(tm => tm.Type == t);
			if (converter == null && converterCache.ContainsKey(t))
			{
				return converterCache[t];
			}
			if (converter == null)
			{
				var attribute = t.GetCustomAttribute<BinaryConverterAttribute>();
				if (attribute != null)
				{
					converter = attribute.Converter;
				}
				else if (t.IsEnum)
				{
					converter = (BinaryConverter)Activator.CreateInstance(typeof(EnumBinaryConverter<>).MakeGenericType(t));
				}

				if (converter == null)
				{
					throw new ArgumentException($"A converter does not exist for the type: {t.Name}");
				}

				// Cache this converter for use later
				converterCache[t] = converter;
				return converter;
			}
			return converter;
		}

		/// <summary>
		/// The type that this converter can serialize/deserialize
		/// </summary>
		public abstract Type Type { get; }

		/// <summary>
		/// A function which when called writes the given object to the BitWriter, assuming the object is of this BinaryConverter's type
		/// </summary>
		/// <param name="value">The value to write the bytes</param>
		/// <param name="writer">The writer to use for writing</param>
		public abstract void WriteGeneric(object value, BitWriter writer);

		/// <summary>
		/// A function which when called reads the given object from the BitReader, assuming the object is of this BinaryConverter's type
		/// </summary>
		/// <param name="reader">The reader for reading bytes</param>
		/// <returns>The object that was read</returns>
		public abstract object ReadGeneric(BitReader reader);

		/// <summary>
		/// The length in bits that will be used to contain this type. 0 if length is variable.
		/// </summary>
		public abstract int Bits { get; }

		/// <summary>
		/// Fixes the endianness of the bytes by reversing their order if they were generated using the wrong endianness
		/// </summary>
		/// <param name="bytes">The bytes to fix</param>
		/// <returns>The fixed ordering of bytes</returns>
		public static IEnumerable<byte> FixEndianness(IEnumerable<byte> bytes)
		{
			if (Endianness == Endianness.LittleEndian != BitConverter.IsLittleEndian)
			{
				return bytes.Reverse();
			}
			return bytes;
		}

		/// <summary>
		/// The list of available converters. If you want another type to be convertable to bits, just add it to this list.
		/// Note that some converters (like individual enum converters) will be cached in this list during runtime
		/// </summary>
		public static List<BinaryConverter> SimpleConverters = new List<BinaryConverter>
		{
			new SimpleBinaryConverter<bool>
			{
				BitCount = 1,
				Write = (v, writer) => writer.WriteBit(v),
				Read = reader => reader.ReadBit()
			},
			new SimpleBinaryConverter<byte>
			{
				BitCount = 8,
				Write = (v, writer) => writer.WriteByte(v),
				Read = reader => reader.ReadByte()
			},
			new SimpleBinaryConverter<int>
			{
				BitCount = 4 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToInt32(FixEndianness(reader.ReadBytes(4)).ToArray(), 0)
			},
			new SimpleBinaryConverter<uint>
			{
				BitCount = 4 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToUInt32(FixEndianness(reader.ReadBytes(4)).ToArray(), 0)
			},
			new SimpleBinaryConverter<long>
			{
				BitCount = 8 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToInt64(FixEndianness(reader.ReadBytes(8)).ToArray(), 0)
			},
			new SimpleBinaryConverter<double>
			{
				BitCount = 8 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToDouble(FixEndianness(reader.ReadBytes(8)).ToArray(), 0)
			},
			new SimpleBinaryConverter<short>
			{
				BitCount = 2 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToInt16(FixEndianness(reader.ReadBytes(2)).ToArray(), 0)
			},
			new SimpleBinaryConverter<ushort>
			{
				BitCount = 2 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v))),
				Read = reader => BitConverter.ToUInt16(FixEndianness(reader.ReadBytes(2)).ToArray(), 0)
			},
			new SimpleBinaryConverter<DateTime>
			{
				BitCount = 8 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v.Ticks))),
				Read = reader => DateTime.FromBinary(BitConverter.ToInt64(FixEndianness(reader.ReadBytes(8)).ToArray(), 0))
			},
			new SimpleBinaryConverter<TimeSpan>
			{
				BitCount = 8 * 8,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(BitConverter.GetBytes(v.Ticks))),
				Read = reader => TimeSpan.FromTicks(BitConverter.ToInt64(FixEndianness(reader.ReadBytes(8)).ToArray(), 0))
			},
			new SimpleBinaryConverter<Guid>
			{
				BitCount = 8 * 16,
				Write = (v, writer) => writer.WriteBytes(FixEndianness(v.ToByteArray())),
				Read = reader => new Guid(reader.ReadBytes(16).ToArray())
			},
			new SimpleBinaryConverter<string>
			{ // This is the variable length version. For the fixed length version, see ConstrainedStringBinaryConverter
				BitCount = 0, // max of (8 * 2) + (8 * 2 * n) bits long, where n is the length of the string
				Write = (v, writer) =>
				{
					if (v == null)
					{
						// Maxvalue indicates null
						writer.WriteBytes(BitConverter.GetBytes(UInt16.MaxValue));
					}
					else
					{
						var data = TextEncoding.GetBytes(v);
						if (data.Length >= UInt16.MaxValue)
						{
							throw new ArgumentException("String too long to be written to bytes");
						}
						// write byte count as a ushort
						writer.WriteBytes(FixEndianness(BitConverter.GetBytes((ushort) data.Length)));
						// write the encoded bytes
						writer.WriteBytes(data);
					}
				},
				Read = reader =>
				{
					// read byte count as a ushort (UInt16.MaxValue indicates null)
					ushort byteCount = BitConverter.ToUInt16(FixEndianness(reader.ReadBytes(2)).ToArray(), 0);
					if (byteCount == UInt16.MaxValue)
					{
						return null;
					}
					return TextEncoding.GetString(reader.ReadBytes(byteCount).ToArray());
				}
			}
		};
	}

	/// <summary>
	/// Represents a BinaryConverter that can convert between bytes and type T
	/// </summary>
	/// <typeparam name="T">The type that this converter converts to and from</typeparam>
	public abstract class BinaryConverter<T> : BinaryConverter
	{
		/// <summary>
		/// A function which when called writes the given object to the BitWriter, assuming the object is of this BinaryConverter's type
		/// </summary>
		/// <param name="value">The value to write the bytes</param>
		/// <param name="writer">The writer to use for writing</param>
		public abstract void Write(T value, BitWriter writer);

		/// <summary>
		/// A function which when called reads the given object from the BitReader, assuming the object is of this BinaryConverter's type
		/// </summary>
		/// <param name="reader">The reader for reading bytes</param>
		/// <returns>The object that was read</returns>
		public abstract T Read(BitReader reader);

		/// <inheritdoc />
		public sealed override Type Type => typeof(T);

		/// <inheritdoc />
		public override void WriteGeneric(object value, BitWriter writer)
		{
			Write((T)value, writer);
		}

		/// <inheritdoc />
		public override object ReadGeneric(BitReader reader)
		{
			return Read(reader);
		}
	}
}
