using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace AlternativeCameraMod.Language.Ini;

internal class IniFile : Ini, IDisposable
{
   private bool _autoFlush;
   private bool _flushOnClose = true;
   private string _filePath;
   private Stream _stream;
   private bool _leaveStreamOpen;
   private bool _needsReload;


   public static IniFile Open(Stream stream, bool leaveStreamOpen = false)
   {
      if (stream == null)
      {
         throw new ArgumentNullException(nameof(stream));
      }

      var ini = new IniFile();
      ini.DoOpenFromStream(stream, leaveStreamOpen);
      ini.DoOpen(false);
      return ini;
   }


   public static IniFile Open(string filePath)
   {
      return Open(filePath, false);
   }


   public static IniFile Open(string filePath, bool createIfNotExist)
   {
      return Open(filePath, createIfNotExist, false);
   }


   public static IniFile Open(string filePath, bool createIfNotExist, bool lazyLoad)
   {
      var ini = new IniFile();
      ini.DoOpenFromFile(filePath, createIfNotExist, lazyLoad);
      return ini;
   }


   public IniFile()
   {
      // nothing here; just to document that public ctor is available
   }


   public void Load(string filePath)
   {
      Load(filePath, false);
   }


   public void Load(string filePath, bool createIfNotExist)
   {
      DoOpenFromFile(filePath, createIfNotExist, false);
   }


   public void Save()
   {
      if (_stream == null)
      {
         if (_filePath == null || !File.Exists(_filePath))
         {
            throw new InvalidOperationException("Ini file not opened");
         }

         OpenFileStream();
      }

      Flush();
   }


   public void Save(string filePath, bool overwrite = false)
   {
      if (File.Exists(filePath) && !overwrite)
      {
         throw new InvalidOperationException("File already exists");
      }

      if (overwrite)
      {
         File.Delete(filePath);
      }

      CreateFileStream(filePath, true);
      Flush();
   }


   private void DoOpenFromFile(string filePath, bool createIfNotExist, bool lazyLoad)
   {
      CreateFileStream(filePath, createIfNotExist);
      DoOpen(lazyLoad);
   }


   private void CreateFileStream(string filePath, bool createIfNotExist)
   {
      var asm = Assembly.GetExecutingAssembly();
      if (String.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
      {
         filePath = Path.Combine(Path.GetDirectoryName(asm.Location), filePath);
      }
      else
      {
         if (!Path.IsPathRooted(filePath))
         {
            filePath = Path.Combine(Path.GetDirectoryName(asm.Location), filePath);
         }
      }

      if (createIfNotExist)
      {
         if (!File.Exists(filePath))
         {
            FileStream s = File.Create(filePath);
            s.Close();
         }
      }
      else
      {
         if (!File.Exists(filePath))
         {
            throw new InvalidOperationException(
               String.Format("INI File {0} does not exist.", filePath));
         }
      }

      _filePath = filePath;
      OpenFileStream();
   }


   private void OpenFileStream()
   {
      var fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
      DoOpenFromStream(fs, false);
   }


   private void DoOpenFromStream(Stream stream, bool leaveStreamOpen)
   {
      if (_stream != null)
      {
         throw new InvalidOperationException(
            "INI file already opened. Use RevertToSaved() to refresh state from disc.");
      }

      _stream = stream;
      _stream.Position = 0; // read from beginning
      _leaveStreamOpen = leaveStreamOpen;
   }


   private void DoOpen(bool lazyLoad)
   {
      NeedsReload = lazyLoad;
      if (!lazyLoad)
      {
         LoadContent();
      }
   }


   ~IniFile()
   {
      Dispose(false);
   }


   public void Dispose()
   {
      Dispose(true);
   }


   private void Dispose(bool disposing)
   {
      if (disposing)
      {
         Close();
      }
      else
      {
         CloseStream();
      }
   }


   private bool CanWrite
   {
      get { return _stream?.CanWrite ?? false; }
   }


   public void Close()
   {
      if (_flushOnClose)
      {
         Flush();
      }

      CloseStream();
      _filePath = string.Empty;
   }


   private void CloseStream()
   {
      if (!_leaveStreamOpen)
      {
         _stream?.Dispose();
      }

      _stream = null;
   }


   public string FilePath
   {
      get { return _filePath; }
   }


   public bool IsOpen
   {
      get { return _stream != null; }
   }


   public void RevertToSaved()
   {
      LoadContent();
   }


   public bool AutoFlush
   {
      [DebuggerStepThrough]
      get { return _autoFlush; }
      [DebuggerStepThrough]
      set { _autoFlush = value; }
   }


   public bool FlushOnClose
   {
      [DebuggerStepThrough]
      get { return _flushOnClose; }
      [DebuggerStepThrough]
      set { _flushOnClose = value; }
   }


   private void ReloadIfRequired()
   {
      if (_needsReload)
      {
         LoadContent();
      }

      _needsReload = false;
   }



   protected void LoadContent()
   {
      using (StreamReader sr = new StreamReader(_stream, Encoding.UTF8, true, 1024, true))
      {
         LoadFromReader(sr);
      }
   }


   public void Flush()
   {
      if (!CanWrite)
      {
         return;
      }

      if (!IsDirty)
      {
         return;
      }

      using (MemoryStream tempStream = new MemoryStream())
      using (StreamWriter tempWriter = new StreamWriter(tempStream))
      {
         tempWriter.AutoFlush = true;
         WriteSections(tempWriter);
         CopyStream(tempStream, _stream);
         _stream.Flush();
      }

      MarkClean();
   }


   private void WriteSections(StreamWriter sw)
   {
      foreach (KeyValuePair<string, IniSection> sectionPair in Sections)
      {
         IniSection currentSection = sectionPair.Value;

         if (sw.BaseStream.Position > 0)
         {
            sw.WriteLine(); // line break before next section
         }

         // Write section name
         sw.WriteLine(String.Format("[{0}]", sectionPair.Key));

         // write values
         foreach (string key in currentSection.Keys)
         {
            if (AllowEolComments)
            {
               string val = currentSection.GetWriteValue(key, true);
               sw.WriteLine(String.Format("{0}={1}", key, val));
            }
            else
            {
               var cm = currentSection.GetComment(key);
               if (!String.IsNullOrEmpty(cm))
               {
                  sw.WriteLine();
                  sw.WriteLine(String.Format("# {0}", cm));
               }

               var val = currentSection.GetWriteValue(key, false);
               sw.WriteLine(String.Format("{0}={1}", key, val));
            }
         }

         currentSection.MarkClean();
      }
   }


   private void CopyStream(Stream input, Stream output)
   {
      input.Position = 0;
      output.Position = 0;
      byte[] buffer = new byte[32768];
      int read;
      while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
      {
         output.Write(buffer, 0, read);
      }
   }


   public override bool AllowKeylessEntries
   {
      get { return base.AllowKeylessEntries; }
      set
      {
         base.AllowKeylessEntries = value;
         _needsReload = true;
      }
   }


   public override bool AllowEolComments
   {
      get { return base.AllowEolComments; }
      set
      {

         base.AllowEolComments = value;
         _needsReload = true;

      }
   }


   protected bool NeedsReload
   {
      get { return _needsReload; }
      set { _needsReload = value; }
   }


   public override bool ContainsSection(string sectionName)
   {
      ReloadIfRequired();
      return base.ContainsSection(sectionName);
   }


   protected internal override void OnModified()
   {
      base.OnModified();
      if (_autoFlush)
      {
         Flush();
      }
   }


   public override IniSection GetSection(string sectionName)
   {
      {
         ReloadIfRequired();
         return base.GetSection(sectionName);
      }
   }
}
