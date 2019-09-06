using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{
	/// <summary>
	/// The generic version of the BinaryConverter class
	/// </summary>
	/// <typeparam name="T">The type of object that this converter can serialize to bytes</typeparam>
	public class SimpleBinaryConverter<T> : BinaryConverter
	{
		/// <inheritdoc />
		public override Type Type => typeof(T);

		/// <summary>
		/// The lambda function which is actually used for writing the object to bytes
		/// </summary>
		public Action<T, BitWriter> Write { get; set; }

		/// <summary>
		/// The lambda function which is actually used for reading the object from bytes
		/// </summary>
		public Func<BitReader, T> Read { get; set; }

		/// <summary>
		/// Settable version of <see cref="Bits"/>
		/// </summary>
		public int BitCount { get; set; }

		/// <summary>
		/// The length in bits that will be used to contain this type. 0 if length is variable.
		/// </summary>
		public override int Bits => BitCount;

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
