using System;
using System.Collections.Generic;
using System.Text;

namespace BpJson.BitPacking
{
	/// <summary>
	/// Identifies the BinaryConverter used for serializing this class to bytes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class BinaryConverterAttribute : Attribute
	{
		/// <summary>
		/// Links the class to the given converter
		/// </summary>
		/// <param name="converter">The converter used to serialize this class to bytes</param>
		public BinaryConverterAttribute(Type t)
		{
			Converter = (BinaryConverter)Activator.CreateInstance(t);
		}

		/// <summary>
		/// The converter used to serialize this class to bytes
		/// </summary>
		public BinaryConverter Converter { get; }
	}
}
