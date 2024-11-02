using System.Globalization;
using System.Text;


namespace LMSR_AlternativeCameraMod.Language.Ini;

[Serializable]
internal sealed class IniSection
{
   private readonly Ini _ini;
   private readonly Dictionary<string, IniEntry> _values = new(StringComparer.CurrentCultureIgnoreCase);
   private bool _isNew;
   private readonly Dictionary<string, IniEntry> _initialValues = new(StringComparer.CurrentCultureIgnoreCase);

   private readonly string _name;


   internal IniSection(string name, Ini ini)
   {
      _name = name;
      _ini = ini;
      _isNew = true;
   }


   public string Name
   {
      get { return _name; }
   }


   internal int NoKeyCounter { get; set; }


   public string this[string valueName]
   {
      get { return _values[valueName].Value; }
   }


   public string GetComment(string key)
   {
      return _values[key].Comment;
   }


   public void SetComment(string key, string comment)
   {
      _values[key].Comment = comment;
   }


   public string GetValue(string key, string defaultValue)
   {
      // Check if the key exists
      if (!_values.TryGetValue(key, out var val))
      {
         return defaultValue;
      }
      
      return val.Value;
   }


   public bool GetValue(string key, bool defaultValue)
   {
      string stringValue = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));

      if (bool.TryParse(stringValue, out var value))
      {
         return value;
      }

      if (int.TryParse(stringValue, out var intValue))
      {
         return intValue != 0;
      }

      return defaultValue;
   }


   public int GetValue(string key, int defaultValue)
   {
      string stringValue = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
      if (int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
      {
         return value;
      }
      return defaultValue;
   }


   public long GetValue(string key, long defaultValue)
   {
      string stringValue = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
      if (long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
      {
         return value;
      }
      return defaultValue;
   }


   public double GetValue(string key, double defaultValue)
   {
      string stringValue = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
      if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
      {
         return value;
      }
      return defaultValue;
   }


   public byte[] GetValue(string key, byte[] defaultValue)
   {
      string stringValue = GetValue(key, EncodeByteArray(defaultValue));
      try
      {
         return DecodeByteArray(stringValue);
      }
      catch (FormatException)
      {
         return defaultValue;
      }
   }


   public DateTime GetValue(string key, DateTime defaultValue)
   {
      string stringValue = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
      if (DateTime.TryParse(
             stringValue,
             CultureInfo.InvariantCulture,
             DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal,
             out var value))
         return value;
      return defaultValue;
   }


   public void SetValue(string key, string val)
   {
      _values.TryGetValue(key, out var curVal);
      bool modified = curVal == null || curVal.Value != val;
      if (modified)
      {
         _initialValues.TryGetValue(key, out var initVal);
         bool revertedToOriginalVal = initVal != null && initVal.Value == val;

         if (revertedToOriginalVal)
         {
            _initialValues.Remove(key);
         }
         else if (!_initialValues.ContainsKey(key))
         {
            _initialValues[key] = curVal;
         }

         _ini.OnModified();
      }

      _values.Remove(key); // to allow key upper/lower case changes 
      if (curVal == null)
      {
         curVal = new IniEntry(val);
      }
      else
      {
         curVal.Value = val;
      }

      _values[key] = curVal;
   }


   public void SetValue(string key, bool val)
   {
      SetValue(key, val ? "1" : "0");
   }


   public void SetValue(string key, int val)
   {
      SetValue(key, val.ToString(CultureInfo.InvariantCulture));
   }


   public void SetValue(string key, long val)
   {
      SetValue(key, val.ToString(CultureInfo.InvariantCulture));
   }


   public void SetValue(string key, double val)
   {
      SetValue(key, val.ToString(CultureInfo.InvariantCulture));
   }


   public void SetValue(string key, byte[] val)
   {
      SetValue(key, EncodeByteArray(val));
   }


   public void SetValue(string key, DateTime val)
   {
      SetValue(key, val.ToString(CultureInfo.InvariantCulture));
   }


   private string EncodeByteArray(byte[] val)
   {
      if (val == null)
      {
         return null;
      }

      var sb = new StringBuilder();
      foreach (byte b in val)
      {
         string hex = Convert.ToString(b, 16);
         int l = hex.Length;
         if (l > 2)
         {
            sb.Append(hex.Substring(l - 2, 2));
         }
         else
         {
            if (l < 2)
            {
               sb.Append("0");
            }
            sb.Append(hex);
         }
      }
      return sb.ToString();
   }


   private byte[] DecodeByteArray(string val)
   {
      if (val == null)
         return null;

      int l = val.Length;
      if (l < 2)
         return new byte[] { };

      l /= 2;
      var result = new byte[l];
      for (int i = 0; i < l; i++)
      {
         result[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
      }
      return result;
   }


   public bool ContainsKey(string key)
   {
      return _values.ContainsKey(key);
   }


   public void Add(string key, IniEntry val)
   {
      _values.Add(key, val);
   }


   public bool TryGetValue(string key, out IniEntry val)
   {
      return _values.TryGetValue(key, out val);
   }


   public IEnumerable<string> Keys
   {
      get { return _values.Keys; }
   }


   public IEnumerable<string> Values
   {
      get { return _values.Values.Select(v => v.Value); }
   }


   public bool IsDirty
   {
      get { return _isNew || _initialValues.Count > 0; }
   }


   internal string GetWriteValue(string key, bool allowEolComments)
   {
      var v = _values[key];
      return v.ToString(allowEolComments);
   }


   public override string ToString()
   {
      return _name;
   }


   public void DiscardChanges()
   {
      foreach (var value in _initialValues)
      {
         _values[value.Key] = value.Value;
      }

      MarkClean();
   }


   internal void MarkClean()
   {
      _isNew = false;
      _initialValues.Clear();
   }


   internal void LoadFrom(string keyValueString)
   {
      using (var s = new StringReader(keyValueString))
      {
         LoadFromReader(s);
      }
   }


   private void LoadFromReader(TextReader sr)
   {
      string line;
      string key = null;
      IniEntry val = null;
      while ((line = sr.ReadLine()) != null)
      {
         line = line.Trim();
         if (ParseKeyValuePair(line, ref key, ref val))
         {
            // Only first occurrence of a key is loaded
            if (!ContainsKey(key))
            {
               Add(key, val);
            }
         }
      }
   }


   internal bool ParseKeyValuePair(string curLine, ref string key, ref IniEntry val)
   {
      if (string.IsNullOrEmpty(curLine))
      {
         return false;
      }

      if (IsCommentLine(curLine))
      {
         return false;
      }

      string lineContent = RemoveEndOfLineComment(curLine, out var comment);
      int splitPos = lineContent.IndexOf('=');

      if (splitPos >= 0)
      {
         key = lineContent.Substring(0, splitPos).Trim();
         string valStr = lineContent.Substring(splitPos + 1).Trim();
         val = new IniEntry(valStr, comment);
      }

      // No '=', or '=' but no key
      if (_ini.AllowKeylessEntries)
      {
         if (splitPos < 0 || string.IsNullOrEmpty(key))
         {
            key = NoKeyCounter.ToString(CultureInfo.CurrentCulture);
            while (ContainsKey(key))
            {
               NoKeyCounter++;
               key = NoKeyCounter.ToString(CultureInfo.CurrentCulture);
            }

            NoKeyCounter++;
         }

         if (splitPos < 0)
         {
            val = new IniEntry(lineContent);
         }
      }

      return key != null;
   }


   private string RemoveEndOfLineComment(string curLine, out string comment)
   {
      comment = string.Empty;

      if (!_ini.AllowEolComments)
      {
         return curLine;
      }

      int commentSplitPos = curLine.IndexOf("//", StringComparison.OrdinalIgnoreCase);
      string lineContent = curLine;
      if (commentSplitPos >= 0)
      {
         lineContent = curLine.Substring(0, commentSplitPos);
         comment = curLine.Substring(commentSplitPos + 2);
      }
      return lineContent;
   }


   private bool IsCommentLine(string curLine)
   {
      return curLine.StartsWith("#", StringComparison.Ordinal)
             || curLine.StartsWith("//", StringComparison.Ordinal)
             || curLine.StartsWith(";", StringComparison.Ordinal);
   }

}