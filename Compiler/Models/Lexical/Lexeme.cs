namespace Compiler.Models.Lexical
{
    public sealed class Lexeme
    {

        public Lexeme(LexemeType type, string value, int startIndex)
        {
            Type = type;
            Value = value;
            StartIndex = startIndex;
        }

        public int LexemeCode { get => (int)Type; }
        public string LexemeName { get => Type.ToString(); }
        public LexemeType Type { get; set; }
        public string Value { get; set; }
        public int Length => Value.Length;
        public int EndIndex => StartIndex + Length;
        public int StartIndex { get; set; }
        public string Position { get => $"{StartIndex}: {StartIndex + Length}"; }
    }
}
