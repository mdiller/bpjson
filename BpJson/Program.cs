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
      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        DateParseHandling = DateParseHandling.None,
      };

      var json = (JToken)JsonConvert.DeserializeObject((File.ReadAllText("TestData/example1.json")));

      Console.WriteLine("Writing...");

      BpJsonWriter.WriteToFile("out.bpjson", json);

      Console.WriteLine("Reading...");

      var outJson = BpJsonReader.ReadFromFile("out.bpjson");

      Console.ReadKey();

      File.WriteAllText("out.json", outJson.ToString());
    }
  }
}
