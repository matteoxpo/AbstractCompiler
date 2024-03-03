namespace Compiler.Models.Lexical
{
    public class Lexeme
    {

        public Lexeme(LexemeType type, string value, int startIndex)
        {
            Initialize(type, value, startIndex, startIndex + 1);
        }

        public Lexeme(LexemeType type, string value, int startIndex, int endIndex)
        {
            Initialize(type, value, startIndex, endIndex);
        }

        private void Initialize(LexemeType type, string value, int startIndex, int endIndex)
        {
            Type = type;
            Value = value;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public int LexemeCode { get => (int)Type; }
        public string LexemeName { get => Type.ToString(); }
        public LexemeType Type { get; set; }
        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Position { get => $"{StartIndex}: {EndIndex}"; }
    }
}
