﻿using System.Text;
using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal abstract class ModSettingsCategory
{
   private readonly LanguageConfig _languageCfg;
   private readonly MelonPreferences_Category _category;
   private static string __line = new string('-', 80);

   protected ModSettingsCategory(string catId, string filePath, LanguageConfig lng)
   {
      _languageCfg = lng;
      _category = MelonPreferences.CreateCategory(catId);
      Category.SetFilePath(filePath);
   }


   public virtual void Save()
   {
      Category.SaveToFile();
   }

   

   protected MelonPreferences_Category Category
   {
      get { return _category; }
   }


   protected MelonPreferences_Entry<T> CreateEntry<T>(string name, T value, string? defaultDescr = null)
   {
      var descr = _languageCfg?.GetText("Config/" + Category.Identifier, name, defaultDescr ?? "") ?? defaultDescr;
      return Category.CreateEntry(name, value, null, FormatDescription(descr));
   }

   
   public virtual string? Validate(LanguageConfig language)
   {
      return null; // no errors 
   }


   private static string FormatDescription(string? descr)
   {
      if (String.IsNullOrEmpty(descr)) return String.Empty;
      var str = __line + Environment.NewLine + WordWrap(descr, 80);
      return str;
   }


   private static string WordWrap(string text, int maxLineWidth)
   {
      int pos, next;
      StringBuilder sb = new StringBuilder();

      if (maxLineWidth < 1)
         return text;

      // Parse each line of text
      for (pos = 0; pos < text.Length; pos = next)
      {
         // Find end of line
         int eol = text.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);

         if (eol == -1)
            next = eol = text.Length;
         else
            next = eol + Environment.NewLine.Length;

         // Copy this line of text, breaking into smaller lines as needed
         if (eol > pos)
         {
            do
            {
               int len = eol - pos;
               
               if (len > maxLineWidth)
                  len = BreakLine(text, pos, maxLineWidth);

               sb.Append(text, pos, len);
               sb.Append(Environment.NewLine);

               // Trim whitespace following break
               pos += len;

               while (pos < eol && Char.IsWhiteSpace(text[pos]))
                  pos++;

            } while (eol > pos);
         }
         else sb.Append(Environment.NewLine); // Empty line
      }

      return sb.ToString().TrimEnd();
   }


   /// <summary>
   /// Locates position to break the given line so as to avoid
   /// breaking words.
   /// </summary>
   /// <param name="text">String that contains line of text</param>
   /// <param name="startIndex">Index where line of text starts</param>
   /// <param name="desiredMaxLineLength">Maximum line length</param>
   /// <returns>The modified line length</returns>
   private static int BreakLine(string text, int startIndex, int desiredMaxLineLength)
   {
      // Find last whitespace in line
      int i = desiredMaxLineLength - 1;
      while (i >= 0 && !Char.IsWhiteSpace(text[startIndex + i]))
         i--;
      
      if (i < 0)
         return desiredMaxLineLength; // No whitespace found; break at maximum length
      
      // Find start of whitespace
      while (i >= 0 && Char.IsWhiteSpace(text[startIndex + i]))
         i--;
      // Return length of text before whitespace
      return i + 1;
   }
}