using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{
	/// <summary>
	/// Represents an upper limit to the number of values that this enum has.
	/// This is used for serializing the enum to bytes, and assumes that the enum uses the default enum to index serialization
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum)]
	public class EnumMaxCount : Attribute
	{
		/// <summary>
		/// ctor
		/// </summary>
		public EnumMaxCount(uint count)
		{
			Count = count;
		}

		/// <summary>
		/// Represents an upper limit to the number of values that this enum has.
		/// </summary>
		public uint Count { get; protected set; }
	}
}
