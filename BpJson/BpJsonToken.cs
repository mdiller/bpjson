using System;
using System.Collections.Generic;
using System.Text;
using BpJson.BitPacking;

namespace BpJson
{
  /// <summary>
  /// The different kinds of items that can be stored in json
  /// </summary>
  [EnumMaxCount(8)]
  public enum BpJsonToken
  {
    /// <summary>
    /// Marks the end of an Object or Array
    /// </summary>
    EndOfList,

    /// <summary>
    /// Marks the beginning of a json object
    /// </summary>
    Object,

    /// <summary>
    /// Marks the beginning of a json array
    /// </summary>
    Array,

    /// <summary>
    /// Marks the beginning of a string
    /// </summary>
    String,

    /// <summary>
    /// Marks the beginning of a number
    /// </summary>
    Number,

    /// <summary>
    /// Marks the beginning of a boolean
    /// </summary>
    Boolean,

    /// <summary>
    /// Marks a null
    /// </summary>
    Null
  }
}
