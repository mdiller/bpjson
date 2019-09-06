using System;
using System.Collections.Generic;
using System.Text;
using BpJson.BitPacking;
using Newtonsoft.Json.Linq;

namespace BpJson.TokenPackers
{
	/// <summary>
	/// 
	/// </summary>
	public class SimpleTokenConverter : BaseTokenConverter
	{
		/// <summary>
		/// The lambda function which is actually used for writing the token to bytes
		/// </summary>
		public Action<JToken, BpJsonWriter> Write { get; set; }

		/// <summary>
		/// The lambda function which is actually used for reading the token from bytes
		/// </summary>
		public Func<BpJsonReader, JToken> Read { get; set; }

		/// <inheritdoc cref="BaseTokenConverter.BpJsonTokenType"/>
		public BpJsonToken BpJsonToken { get; set; }

		/// <inheritdoc cref="BaseTokenConverter.JsonTokenTypes"/>
		public List<JTokenType> JsonTokens { get; set; }

		/// <inheritdoc />
		public override BpJsonToken BpJsonTokenType => BpJsonToken;

		/// <inheritdoc />
		public override List<JTokenType> JsonTokenTypes => JsonTokens;

		/// <inheritdoc />
		public override void WriteToken(JToken token, BpJsonWriter writer)
		{
			Write(token, writer);
		}

		/// <inheritdoc />
		public override JToken ReadToken(BpJsonReader reader)
		{
			return Read(reader);
		}

		/// <summary>
		/// The converter for a null token
		/// </summary>
		public static SimpleTokenConverter NullTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.Null,
			JsonTokens = new List<JTokenType> { JTokenType.Null },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.Null);
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				return null;
			}
		};

		/// <summary>
		/// The converter for a bool token
		/// </summary>
		public static SimpleTokenConverter BooleanTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.Boolean,
			JsonTokens = new List<JTokenType> { JTokenType.Boolean },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.Boolean);
				writer.WriteBit(token.Value<bool>());
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				return reader.ReadBit();
			}
		};

		/// <summary>
		/// The converter for an array token
		/// </summary>
		public static SimpleTokenConverter ArrayTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.Array,
			JsonTokens = new List<JTokenType> { JTokenType.Array },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.Array);
				var array = token as JArray;
				foreach (var item in array)
				{
					writer.WriteToken(item);
				}
				writer.Write(BpJsonToken.EndOfItem);
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				var array = new JArray();
				while (true)
				{
					var elementType = reader.Read<BpJsonToken>();
					if (elementType == BpJsonToken.EndOfItem)
					{
						return array;
					}
					reader.Rewind(typeof(BpJsonToken));
					array.Add(reader.ReadToken());
				}
			}
		};

		// The following converters will be rewritten in multiple more efficent ways

		/// <summary>
		/// The converter for a string token
		/// </summary>
		public static SimpleTokenConverter ObjectTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.Object,
			JsonTokens = new List<JTokenType> { JTokenType.Object },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.Object);
				var obj = token as JObject;
				foreach (var prop in obj.Properties())
				{
					writer.Write(BpJsonToken.Object);
					writer.Write(prop.Name);
					writer.WriteToken(prop.Value);
				}
				writer.Write(BpJsonToken.EndOfItem);
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				var obj = new JObject();
				while (true)
				{
					var elementType = reader.Read<BpJsonToken>();
					if (elementType == BpJsonToken.EndOfItem)
					{
						return obj;
					}
					var key = reader.Read<string>();
					obj.Add(key, reader.ReadToken());
				}
			}
		};

		/// <summary>
		/// The converter for a string token
		/// </summary>
		public static SimpleTokenConverter StringTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.String,
			JsonTokens = new List<JTokenType> { JTokenType.String, JTokenType.Date },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.String);
				writer.Write(token.Value<string>());
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				return reader.Read<string>();
			}
		};

		/// <summary>
		/// The converter for a number token
		/// </summary>
		public static SimpleTokenConverter NumberTokenConverter => new SimpleTokenConverter
		{
			BpJsonToken = BpJsonToken.Number,
			JsonTokens = new List<JTokenType> { JTokenType.Integer, JTokenType.Float },
			Write = (token, writer) =>
			{
				writer.Write(BpJsonToken.Number);
				if (token.Type == JTokenType.Integer)
				{
					writer.WriteBit(true);
					writer.Write(token.Value<long>());
				}
				else
				{
					writer.WriteBit(false);
					writer.Write(token.Value<double>());
				}
			},
			Read = reader =>
			{
				reader.Read<BpJsonToken>();
				if (reader.ReadBit())
				{
					return reader.Read<long>();
				}
				else
				{
					return reader.Read<double>();
				}
			}
		};
	}
}
