using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
	/// <summary>
	/// A tokenconverter that stores a variable in the header of type T
	/// </summary>
	public class VariableTokenConverter<T> : SimpleTokenConverter
	{
		/// <summary>
		/// The value that was created from WriteHeader or ReadHeader
		/// </summary>
		private T savedValue;

		/// <summary>
		/// The lambda function which is actually used for writing the token to bytes
		/// </summary>
		public new Action<JToken, BpJsonWriter, T> Write { get; set; }

		/// <summary>
		/// The lambda function which is actually used for reading the token from bytes
		/// </summary>
		public new Func<BpJsonReader, T, JToken> Read { get; set; }

		/// <summary>
		/// Gets the header value from the given JToken
		/// </summary>
		public Func<JToken, T> GetVariable { get; set; }

		/// <inheritdoc />
		public override void WriteToken(JToken token, BpJsonWriter writer)
		{
			Write(token, writer, savedValue);
		}

		/// <inheritdoc />
		public override JToken ReadToken(BpJsonReader reader)
		{
			return Read(reader, savedValue);
		}

		/// <inheritdoc />
		public override void WriteTokenHeader(JToken rootToken, BpJsonWriter writer)
		{
			savedValue = GetVariable(rootToken);
			writer.Write(savedValue);
		}

		/// <inheritdoc />
		public override void ReadTokenHeader(BpJsonReader reader)
		{
			savedValue = reader.Read<T>();
		}
	}
}
