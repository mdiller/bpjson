using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace BpJson.BitPacking
{
	/// <summary>
	/// Extension methods useful when bitpacking
	/// </summary>
	public static class BitPackingExtensions
	{
    /// <summary>
    /// Gzipped an array of bytes
    /// </summary>
    /// <param name="data">The bytes to gzip</param>
    /// <returns>The gzipped array of bytes</returns>
    public static List<byte> Gzip(this IEnumerable<byte> data)
    {
      byte[] raw = data.ToArray();
      using (MemoryStream memory = new MemoryStream())
      {
        using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
        {
          gzip.Write(raw, 0, raw.Length);
        }
        return memory.ToArray().ToList();
      }
    }

    /// <summary>
    /// Unzips a gzipped array of bytes
    /// </summary>
    /// <param name="data">The bytes to unzip</param>
    /// <returns>The unzipped bytes</returns>
    public static List<byte> UnGzip(this IEnumerable<byte> data)
    {
      using (MemoryStream inputStream = new MemoryStream(data.ToArray()))
      {
        using (MemoryStream memory = new MemoryStream())
        {
          using (GZipStream gzip = new GZipStream(inputStream, CompressionMode.Decompress))
          {
            gzip.CopyTo(memory);
          }
          return memory.ToArray().ToList();
        }
      }
    }
  }
}
