using System.Collections.ObjectModel;
using System.Globalization;
using AlternativeCameraMod.Language.Ini;


namespace AlternativeCameraMod.Language;

internal class LanguageConfig
{
   private readonly string _langCode;
   private readonly IniFile? _ini;
   private readonly LanguageConfig? _fallbackLanguage;
   private const string DefaultLang = "en";
   private readonly Dictionary<string, string> _runtimeCache = new();


   private LanguageConfig(string langCode, IniFile? ini)
   {
      _langCode = langCode;
      _ini = ini;
      if (_langCode != DefaultLang)
      {
         _fallbackLanguage = Load(DefaultLang);
      }
   }


   public static ReadOnlyCollection<string> GetAvailableLanguages()
   {
      var langDir = GetLanguageDir();
      var langIniFiles = Directory.GetFiles(langDir, "lang.*.ini");
      var langs = new List<string>();
      foreach (var iniFile in langIniFiles)
      {
         var parts = iniFile.Split('.');
         if (parts.Length == 3)
         {
            langs.Add(parts[1].Trim().ToLower());
         }
      }

      return new ReadOnlyCollection<string>(langs);
   }


   public static LanguageConfig Load(string? langCode = null)
   {
      var langDir = GetLanguageDir();

      string lc;
      if (langCode == null || "default".Equals(langCode.Trim(), StringComparison.InvariantCultureIgnoreCase))
      {
         lc = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
      }
      else
      {
         lc = langCode.Trim();
      }

      var filePath = Path.Combine(langDir, "lang." + lc + ".ini");
      if (!File.Exists(filePath))
      {
         // if culture-specific file not found, try english as default, otherwise empty, which defaults to hardcoded text
         if (!String.Equals(DefaultLang, lc, StringComparison.OrdinalIgnoreCase))
         {
            return Load(DefaultLang);
         }

         return new LanguageConfig(lc, null);
      }

      try
      {
         var ini = IniFile.Open(filePath, true);
         var lconf = new LanguageConfig(lc, ini);
         return lconf;
      }
      catch
      {
         return new LanguageConfig(lc, null);
      }
   }


   private static string GetLanguageDir()
   {
      var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var file = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var langDir = Path.Combine(dir, file + "-assets", "Languages");
      if (!Directory.Exists(langDir))
      {
         Directory.CreateDirectory(langDir);
      }

      return langDir;
   }


   public void Save()
   {
      if (_ini != null && _ini.IsDirty)
      {
         _ini.Save();
      }
   }


   public string LanguageCode
   {
      get { return _langCode; }
   }


   public string GetText(string sectionId, string phraseId, string defaultText, params object[] args)
   {
      if (_ini == null) return defaultText;
      if (!_runtimeCache.TryGetValue(sectionId + "_" + phraseId, out var fmtTxt))
      {
         var defVal = "\"" + defaultText + "\"";
         var sec = _ini.GetSection(sectionId);
         var txt = sec.GetValue(phraseId, _fallbackLanguage?.GetFormat(sectionId, phraseId) ?? defVal);
         fmtTxt = txt.Replace("\\n", Environment.NewLine);

         int pos1 = fmtTxt.IndexOf('"', 0);
         int pos2 = fmtTxt.LastIndexOf('"');
         if (pos1 < 0)
            pos1 = 0;
         else
            pos1++;

         if (pos2 < 0)
            pos2 = fmtTxt.Length;
         
         fmtTxt = fmtTxt.Substring(pos1, pos2-pos1);
         // escape brackets
         fmtTxt = fmtTxt.Replace("{", "{{");
         fmtTxt = fmtTxt.Replace("}", "}}");
         fmtTxt = fmtTxt.Replace("{{0}}", "{0}"); // currently we only have text with single placeholder, so this is effective
         
         _runtimeCache[sectionId + "_" + phraseId] = fmtTxt;
      }
      
      try
      {
         fmtTxt = String.Format(fmtTxt, args);
      }
      catch (FormatException)
      {
         var argStr = " [" + String.Join(", ", args) + "]";
         fmtTxt += argStr;
      }

      return fmtTxt;
   }


   private string? GetFormat(string sectionId, string phraseId)
   {
      var sec = _ini.GetSection(sectionId);
      var txt = sec.GetValue(phraseId, "");
      if (txt == "")
      {
         return null;
      }
      return txt;
   }


   public int GetIntNum(string sectionId, string valueId, int defaultValue)
   {
      var str = GetText(sectionId, valueId, defaultValue.ToString());
      if (!Int32.TryParse(str, out var val))
      {
         return defaultValue;
      }

      return val;
   }
}
