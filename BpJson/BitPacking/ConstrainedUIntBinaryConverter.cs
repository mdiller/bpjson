using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{ 
	/// <summary>
	/// Represents how to save/load an unsigned integer to/from bytes (values given are ints but theyre assumed to all be >= 0)
	/// </summary>
	public class ConstrainedUIntBinaryConverter : BinaryConverter<int>
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ConstrainedUIntBinaryConverter(int maxSize)
		{
			maxSize = maxSize < 0 ? 0 : maxSize;
			MaxSize = (uint)maxSize;
		}

		/// <summary>
		/// The maximum length of a string
		/// </summary>
		public uint MaxSize { get; }

		/// <summary>
		/// The length in bits that will be used to contain this type. 0 if length is variable.
		/// </summary>
		public override int Bits => MaxSize == 0 ? 0 : (int)Math.Log(MaxSize, 2) + 1;

		/// <inheritdoc />
		public override void Write(int value, BitWriter writer)
		{
			uint v = (uint)value;
			for (int i = 0; i < Bits; i++)
			{
				writer.WriteBit((v & (1 << i)) != 0);
			}
		}

		/// <inheritdoc />
		public override int Read(BitReader reader)
		{
			var v = 0;
			for (int i = 0; i < Bits; i++)
			{
				// set bit then shift over
				if (reader.ReadBit())
				{
					v |= 1 << i;
				}
			}
			return v;
		}
	}
}
