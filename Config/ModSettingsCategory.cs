using System.Text;
using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal abstract class ModSettingsCategory
{
	// Melon config elements may be intantiated only once, so track them in static maps
	private static readonly Dictionary<string, MelonPreferences_Category> __categories = new();
	private static readonly Dictionary<string, MelonPreferences_Entry> __entries = new();
	private static readonly string __line = new('-', 80);

	private readonly LanguageConfig _languageCfg;
	private readonly MelonPreferences_Category _category;
	private readonly string _filePath;


	protected ModSettingsCategory(string catId, string filePath, LanguageConfig lng)
	{
		_languageCfg = lng;

		if (!__categories.TryGetValue(catId, out var cat))
		{
			cat = MelonPreferences.CreateCategory(catId);
			__categories.Add(catId, cat);
		}

		_category = cat;
		_category.SetFilePath(filePath);
		_filePath = filePath;
	}


	public virtual void Save()
	{
		Category.SetFilePath(_filePath, false);
		Category.SaveToFile();
	}


	protected MelonPreferences_Category Category
	{
		get { return _category; }
	}


	protected MelonPreferences_Entry<T> CreateEntry<T>(string name, T value, string? defaultDescr = null)
	{
		if (!__entries.TryGetValue(_category.Identifier + "_" + name, out var entry))
		{
			entry = Category.CreateEntry(name, value);
			__entries.Add(_category.Identifier + "_" + name, entry);
		}

		entry.Description = _languageCfg?.GetText("Config/" + Category.Identifier, name, defaultDescr ?? "") ?? defaultDescr;
		entry.Description = entry.Description?.Replace(Environment.NewLine, "\n"); // melon likes \n
		return (MelonPreferences_Entry<T>)entry;
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
		StringBuilder sb = new();

		if (maxLineWidth < 1) {
			return text;
		}

		// Parse each line of text
		for (pos = 0; pos < text.Length; pos = next)
		{
			// Find end of line
			int eol = text.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);

			if (eol == -1) {
				next = eol = text.Length;
			}
			else {
				next = eol + Environment.NewLine.Length;
			}

			// Copy this line of text, breaking into smaller lines as needed
			if (eol > pos)
			{
				do
				{
					int len = eol - pos;

					if (len > maxLineWidth) {
						len = BreakLine(text, pos, maxLineWidth);
					}

					sb.Append(text, pos, len);
					sb.Append(Environment.NewLine);

					// Trim whitespace following break
					pos += len;

					while (pos < eol && Char.IsWhiteSpace(text[pos])) {
						pos++;
					}

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
		while (i >= 0 && !Char.IsWhiteSpace(text[startIndex + i])) {
			i--;
		}

		if (i < 0) {
			return desiredMaxLineLength; // No whitespace found; break at maximum length
		}

		// Find start of whitespace
		while (i >= 0 && Char.IsWhiteSpace(text[startIndex + i])) {
			i--;
		}

		// Return length of text before whitespace
		return i + 1;
	}
}
