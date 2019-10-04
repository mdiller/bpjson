using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BpJson.BitPacking;
using BpJson.TokenPackers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BpJson
{
  class Program
  {
    public static void Main(string[] args)
    {
      var settings = new JsonSerializerSettings
      {
        DateParseHandling = DateParseHandling.None,
        FloatParseHandling = FloatParseHandling.Decimal
      };

      var json = (JToken)JsonConvert.DeserializeObject(File.ReadAllText("TestData/example1.json"), settings);

      Console.WriteLine("Writing...");

      BpJsonWriter.WriteToFile("out.bpjson", json);

      Console.WriteLine("Reading...");

      var outJson = BpJsonReader.ReadFromFile("out.bpjson");

      File.WriteAllText("out.json", outJson.ToString());
    }
  }
}
