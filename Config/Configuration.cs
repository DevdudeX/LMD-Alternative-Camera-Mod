using AlternativeCameraMod.Language;
using Il2CppSystem.Text;


namespace AlternativeCameraMod.Config;

internal class Configuration
{
   public const string ConfigFilePath = "UserData/AlternativeCameraWithPhotoMode.ini";
   public const string GameColor = "#E9AC4F";
   public const string GameColor2 = "#8FBC0D";

   private readonly Dictionary<string, ModSettingsCategory> _categories = new();
   private readonly string _filePath;


   public Configuration(LanguageConfig lng)
   {
      if (lng == null)
      {
         throw new ArgumentNullException(nameof(lng));
      }

      FileInfo filePath = new FileInfo(ConfigFilePath);
      _filePath = filePath.FullName;
#if DEBUG
      File.Delete(_filePath); // to refresh config
#endif

      _categories.Add(nameof(CommonSettings), new CommonSettings(ConfigFilePath, lng));
      _categories.Add(nameof(CameraSettings), new CameraSettings(ConfigFilePath, lng));
      _categories.Add(nameof(UISettings), new UISettings(ConfigFilePath, lng));
      _categories.Add(nameof(ControllerSettings), new ControllerSettings(ConfigFilePath, lng));
      _categories.Add(nameof(MouseSettings), new MouseSettings(ConfigFilePath, lng));
      _categories.Add(nameof(KeyboardSettings), new KeyboardSettings(ConfigFilePath, lng));
      _categories.Add(nameof(PhotoModeSettings), new PhotoModeSettings(ConfigFilePath, lng));
   }


   public void Save()
   {
      foreach (var category in _categories.Values)
      {
         category.Save();
      }
      Beautify();
   }


   private void Beautify()
   {
      try
      {
         var text = File.ReadAllText(_filePath, System.Text.Encoding.UTF8);
         // cleanup line endings
         text = text.Replace(Environment.NewLine, "\n");
         text = text.Replace("\r", "\n");
         var lines = text.Split("\n");

         StringBuilder output = new StringBuilder(lines.Sum(l => l.Length));
         bool lastIsEntry = false;
         for (int i = 0; i < lines.Length; i++)
         {
            var line = lines[i];
            if (String.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("[")) // category
            {
               if (i > 0)
               {
                  output.AppendLine().AppendLine(); // two lines spacing before category
               }
               output.AppendLine(line);
               lastIsEntry = false;
            }
            else if (line.StartsWith("#")) // comment
            {
               var trimLine = line.Trim();
               if (trimLine == "#") // not an empty comment
               {
                  output.AppendLine(); // omit empty #
               }
               else
               {
                  if (lastIsEntry)
                  {
                     output.AppendLine(); // one line spacing
                  }
                  output.AppendLine(line);
                  lastIsEntry = false;
               }
            }
            else // entry
            {
               output.AppendLine(line);
               lastIsEntry = true;
            }
         }

         File.WriteAllText(_filePath, output.ToString(), System.Text.Encoding.UTF8);
      }
      catch
      {
         // ignore
      }
   }


   public string? Validate(LanguageConfig language)
   {
      var sb = new StringBuilder();
      foreach (var category in _categories.Values)
      {
         var err = category.Validate(language);
         if (!String.IsNullOrEmpty(err))
         {
            sb.AppendLine(err);
         }
      }

      return sb.ToString();
   }


   public CommonSettings Common
   {
      get { return (CommonSettings)_categories[nameof(CommonSettings)]; }
   }


   public UISettings UI
   {
      get { return (UISettings)_categories[nameof(UISettings)]; }
   }


   public ControllerSettings Controller
   {
      get { return (ControllerSettings)_categories[nameof(ControllerSettings)]; }
   }


   public KeyboardSettings Keyboard
   {
      get { return (KeyboardSettings)_categories[nameof(KeyboardSettings)]; }
   }


   public PhotoModeSettings PhotoMode
   {
      get { return (PhotoModeSettings)_categories[nameof(PhotoModeSettings)]; }
   }


   public MouseSettings Mouse
   {
      get { return (MouseSettings)_categories[nameof(MouseSettings)]; }
   }


   public CameraSettings Camera
   {
      get { return (CameraSettings)_categories[nameof(CameraSettings)]; }
   }
}