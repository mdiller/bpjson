using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BpJson
{
  /// <summary>
  /// Logs information about the bitpacking process, for feedback that lets me further refine it.
  /// This will likely be turned of in a client-facing version, as this is mostly only useful to me as a developer
  /// </summary>
  public class BpLogger
  {
    /// <summary>
    /// ctor
    /// </summary>
    public BpLogger()
    {
      BitCounts = new Dictionary<BpJsonToken, int>();
      HeaderBitCounts = new Dictionary<BpJsonToken, int>();
      TokenCounts = new Dictionary<BpJsonToken, int>();
      BitStack = new Stack<int>();
      ConsumedStack = new Stack<int>();
      ConsumedStack.Push(0);
    }

    private Stack<int> BitStack { get; }

    private Stack<int> ConsumedStack { get; }

    private Dictionary<BpJsonToken, int> HeaderBitCounts { get; }

    private Dictionary<BpJsonToken, int> BitCounts { get; }

    private Dictionary<BpJsonToken, int> TokenCounts { get; }

    /// <summary>
    /// Logs the start of writing a header
    /// </summary>
    /// <param name="token">The tokentype who's header is being written</param>
    /// <param name="writer">The bitwriter</param>
    public void StartWriteHeader(BpJsonToken token, BpJsonWriter writer)
    {
      BitStack.Push(writer.BitPosition);
    }

    /// <summary>
    /// Logs the end of writing a header
    /// </summary>
    /// <param name="token">The tokentype who's header is being written</param>
    /// <param name="writer">The bitwriter</param>
    public void EndWriteHeader(BpJsonToken token, BpJsonWriter writer)
    {
      HeaderBitCounts[token] = writer.BitPosition - BitStack.Pop();
    }

    /// <summary>
    /// Logs the start of writing a token
    /// </summary>
    /// <param name="token">The tokentype that is being written</param>
    /// <param name="writer">The bitwriter</param>
    public void StartWriteToken(BpJsonToken token, BpJsonWriter writer)
    {
      TokenCounts[token] = (TokenCounts.ContainsKey(token) ? TokenCounts[token] : 0) + 1;
      BitStack.Push(writer.BitPosition);
      ConsumedStack.Push(0);
    }

    /// <summary>
    /// Logs the end of writing a token
    /// </summary>
    /// <param name="token">The tokentype that is being written</param>
    /// <param name="writer">The bitwriter</param>
    public void EndWriteToken(BpJsonToken token, BpJsonWriter writer)
    {
      var childConsumed = ConsumedStack.Pop();
      var count = (writer.BitPosition - BitStack.Pop()) - childConsumed;
      BitCounts[token] = (BitCounts.ContainsKey(token) ? BitCounts[token] : 0) + (int)count;
      ConsumedStack.Push(ConsumedStack.Pop() + childConsumed + count);
    }

    /// <summary>
    /// Prints the result of the logging to the console
    /// </summary>
    public void Print()
    {
      Console.WriteLine("Headers:");
      foreach (var kv in HeaderBitCounts)
      {
        if (kv.Value != 0)
        {
          Console.WriteLine($"{kv.Key,-10} {kv.Value:n0} bits");
        }
      }
      Console.WriteLine("\nTokens:");
      foreach (var kv in BitCounts)
      {
        Console.WriteLine($"{kv.Key,-10} {kv.Value:n0} bits");
      }
      Console.WriteLine("\nTokens (excluding BpJsonTokens):");
      foreach (var kv in BitCounts)
      {
        Console.WriteLine($"{kv.Key,-10} {kv.Value - (TokenCounts[kv.Key] * 3):n0} bits");
      }
      Console.WriteLine("\n\nToken Counts:");
      foreach (var kv in TokenCounts)
      {
        Console.WriteLine($"{kv.Key,-10} {kv.Value:n0}");
      }

      var tokenTotal = TokenCounts.Values.Sum() * 3;
      Console.WriteLine($"\nTotal used by BpJsonTokens: {tokenTotal:n0} bits");

      var total = HeaderBitCounts.Values.Sum() + BitCounts.Values.Sum();
      Console.WriteLine($"\nTotal: {total:n0} bits");
    }
  }
}
