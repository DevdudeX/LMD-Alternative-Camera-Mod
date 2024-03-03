namespace AlternativeCameraMod.Language.Ini;

internal class Ini
{
   private readonly Dictionary<string, IniSection> _sections =
      new Dictionary<string, IniSection>(StringComparer.CurrentCultureIgnoreCase);

   private bool _allowKeylessEntries;
   private bool _allowEolComments;


   public Ini()
   {
      // nothing here; just to document that public ctor is available
   }


   public void LoadFromText(string text)
   {
      using (var s = new StringReader(text))
      {
         LoadFromReader(s);
      }
   }


   public virtual bool AllowKeylessEntries
   {
      get { return _allowKeylessEntries; }
      set { _allowKeylessEntries = value; }
   }


   public virtual bool AllowEolComments
   {
      get { return _allowEolComments; }
      set { _allowEolComments = value; }
   }


   public IniSection this[string sectionName]
   {
      get { return GetSection(sectionName); }
   }


   protected Dictionary<string, IniSection> Sections
   {
      get { return _sections; }
   }


   private string ParseSectionName(string line)
   {
      if (!line.StartsWith("[", StringComparison.Ordinal))
         return null;
      if (!line.EndsWith("]", StringComparison.Ordinal))
         return null;
      if (line.Length < 3)
         return null;
      return line.Substring(1, line.Length - 2);
   }


   public void LoadFromIni(Ini otherIni)
   {
      if (otherIni == null)
      {
         throw new ArgumentNullException(nameof(otherIni));
      }

      if (otherIni == this)
      {
         throw new InvalidOperationException("Can not load from itself");
      }

      _sections.Clear();
      foreach (var section in otherIni._sections)
      {
         var newSec = new IniSection(section.Key, this);
         _sections.Add(section.Key, newSec);

         foreach (var kvp in section.Value.Keys)
         {
            newSec.Add(kvp, new IniEntry(section.Value[kvp]));
         }
      }
   }


   public void MergeIni(Ini otherIni)
   {
      if (otherIni == null)
      {
         throw new ArgumentNullException(nameof(otherIni));
      }

      if (otherIni == this)
      {
         throw new InvalidOperationException("Can not load from itself");
      }

      foreach (var section in otherIni._sections)
      {
         if (!_sections.TryGetValue(section.Key, out var newSec))
         {
            newSec = new IniSection(section.Key, this);
            _sections.Add(section.Key, newSec);
         }

         foreach (var kvp in section.Value.Keys)
         {
            newSec.Add(kvp, new IniEntry(section.Value[kvp]));
         }
      }
   }


   protected void LoadFromReader(TextReader sr)
   {
      _sections.Clear();

      IniSection currentSection = null;
      string line;
      string key = null;
      IniEntry val = null;
      while ((line = sr.ReadLine()) != null)
      {
         line = line.Trim();

         // Check for section names
         string sectionName = ParseSectionName(line);
         if (sectionName != null)
         {
            // Only first occurrence of a section is loaded
            if (_sections.ContainsKey(sectionName))
            {
               currentSection = null;
            }
            else
            {
               currentSection = new IniSection(sectionName, this);
               _sections.Add(sectionName, currentSection);
            }
         }
         else if (currentSection != null)
         {
            // Check for key+value pair
            if (currentSection.ParseKeyValuePair(line, ref key, ref val))
            {
               // Only first occurrence of a key is loaded
               if (!currentSection.ContainsKey(key))
               {
                  currentSection.Add(key, val);
               }
               else
               {
                  // AddWarning("Section '{0}' contains duplicate key '{1}'; only first occurrence loaded",
                  //    currentSection.Name,
                  //    key);
               }
            }
         }
      }
   }

   public virtual bool ContainsSection(string sectionName)
   {
      return _sections.ContainsKey(sectionName);
   }


   protected internal virtual void OnModified()
   {
   }


   public bool IsDirty
   {
      get
      {
         foreach (var sec in _sections.Values)
         {
            if (sec.IsDirty)
            {
               return true;
            }
         }

         return false;
      }
   }


   internal void MarkClean()
   {
      foreach (var sec in _sections.Values)
      {
         sec.MarkClean();
      }
   }


   public virtual IniSection GetSection(string sectionName)
   {
      // Check if the section exists
      if (!_sections.TryGetValue(sectionName, out var section))
      {
         section = new IniSection(sectionName, this);
         _sections.Add(sectionName, section);
         OnModified();
      }

      return section;
   }



   public string GetValue(string sectionName, string key, string defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public bool GetValue(string sectionName, string key, bool defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public int GetValue(string sectionName, string key, int defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public long GetValue(string sectionName, string key, long defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public double GetValue(string sectionName, string key, double defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public byte[] GetValue(string sectionName, string key, byte[] defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public DateTime GetValue(string sectionName, string key, DateTime defaultValue)
   {
      return GetSection(sectionName).GetValue(key, defaultValue);
   }


   public void SetValue(string sectionName, string key, string value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, bool value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, int value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, long value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, double value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, byte[] value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void SetValue(string sectionName, string key, DateTime value)
   {
      GetSection(sectionName).SetValue(key, value);
   }


   public void LoadSectionFrom(string sectionName, string keyValueString)
   {
      var section = GetSection(sectionName);
      section.LoadFrom(keyValueString);
   }
}