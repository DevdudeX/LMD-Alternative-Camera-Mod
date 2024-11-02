namespace LMSR_AlternativeCameraMod.Language.Ini;

[Serializable]
internal sealed class IniEntry
{
   private string _value;
   private string _comment;


   public IniEntry(string value)
   {
      _value = value;
   }


   public IniEntry(string value, string comment)
   {
      _value = value;
      _comment = comment;
   }


   public string Value
   {
      get { return _value; }
      set { _value = value; }
   }


   public string Comment
   {
      get { return _comment; }
      set { _comment = value; }
   }


   public override string ToString()
   {
      return _value;
   }


   public string ToString(bool allowEolComments)
   {
      return _value + (allowEolComments && !String.IsNullOrEmpty(_comment) ? "   //" + _comment : "");
   }
}

