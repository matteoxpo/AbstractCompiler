using Compiler.Models.Lexical;
namespace Compiler.Models.Parser
{
    public static class Parser
    {
        private static List<Lexeme>? _lexemes;
        private static int _currentIndex;
        private static List<ParsedError> _errors;
        private static Lexeme CurrentLexeme => _lexemes[_currentIndex];

        private static void Consume() => _currentIndex++;
        
        private static void ReportError(string message, int startIndex, int endIndex) => _errors.Add(new ParsedError(message, startIndex, endIndex != startIndex ? endIndex : startIndex + 1));
        
        private static void Match(IEnumerable<LexemeType> expectedType)
        {
            if (_currentIndex < _lexemes.Count)
            {
                foreach(var type in expectedType)
                {
                    if (type == CurrentLexeme.Type)
                    {
                        Consume();
                        return;
                    }
                }
                ReportError($"Ожидался {expectedType}, получено {CurrentLexeme.Type}", CurrentLexeme.StartIndex, CurrentLexeme.EndIndex);
                Consume();
            }
        }
        private static void Match(LexemeType expectedType)
        {
            if (_currentIndex < _lexemes.Count)
            {
                if (CurrentLexeme.Type == expectedType)
                {
                    Consume();
                }
                else
                {
                    ReportError($"Ожидался {expectedType}, получено {CurrentLexeme.Type}", CurrentLexeme.StartIndex, CurrentLexeme.EndIndex);
                    Consume();
                }
            } else if (expectedType == LexemeType.EndOfExpression) { 
                ReportError($"Ожидался {expectedType}, получено ничего", _lexemes[_currentIndex - 1].EndIndex + 1, _lexemes[_currentIndex - 1].EndIndex + 1);
            }
        }

        public static List<ParsedError> Parse(IEnumerable<Lexeme> lexemes)
        {
            _lexemes = lexemes?.Where(lexeme => lexeme.Type != LexemeType.Whitespace).ToList() ?? throw new ArgumentNullException(nameof(lexemes));
            _currentIndex = 0;
            _errors = new List<ParsedError>();
            Program();
            return _errors;
        }

        private static void Program()
        {
            while (_currentIndex < _lexemes.Count)
            {
                if (CurrentLexeme.Type == LexemeType.NewLine)
                {
                    Consume();
                }
                if (_currentIndex >= _lexemes.Count)
                {
                    break;
                }
                LF();
            }
        }

        private static void LF()
        {
            if (_currentIndex < _lexemes.Count)
            {
                Match(LexemeType.StartOfLambdaArguments);
                ARGFUNC();
            }
        }

        private static void ARGFUNC()
        {
            if (_currentIndex < _lexemes.Count)
            {
                if (CurrentLexeme.Type == LexemeType.SignMinus)
                {
                    Match(LexemeType.SignMinus);
                    ARROW();
                }
                else
                {
                    Match(LexemeType.Identifier);
                    ARGFUNCREM();
                }
            }
        }

        private static void ARGFUNCREM()
        {
            if (_currentIndex < _lexemes.Count && CurrentLexeme.Type == LexemeType.Identifier)
            {
                Consume();
            }
            if (_currentIndex < _lexemes.Count && CurrentLexeme.Type == LexemeType.NewLine)
            {
                Consume();
            }
            else
            {
                ARGFUNC();
            }
        }

        private static void ARROW()
        {
            if (_currentIndex < _lexemes.Count)
            {
                Match(LexemeType.SignMore);
                ARG1();
            }
        }

        private static void ARG1()
        {
            if (_currentIndex < _lexemes.Count)
            {
                Match(LexemeType.Identifier);
                OPERATION();
            }
        }

        private static void OPERATION()
        {
            if (_currentIndex < _lexemes.Count)
            {
                Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus, LexemeType.SignDevide, LexemeType.SignMultiply});
                ARG2();
            }
        }

        private static void ARG2()
        {
            if (_currentIndex < _lexemes.Count)
            {
                Match(LexemeType.Identifier);
            }
            if (_currentIndex < _lexemes.Count)
            {
                Match(LexemeType.CloseBracket);
                NUM1();
            }
        }

        private static void NUM1() 
        {
            if (_currentIndex < _lexemes.Count)
            {
                if (CurrentLexeme.Type == LexemeType.SignPlus || CurrentLexeme.Type == LexemeType.SignMinus)
                {
                    Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus });
                }
                Match(new List<LexemeType>() { LexemeType.Float, LexemeType.Integer});
                NUM2();
            }
        }
        private static void NUM2() 
        {
            if (_currentIndex < _lexemes.Count)
            {
                if (CurrentLexeme.Type == LexemeType.SignPlus || CurrentLexeme.Type == LexemeType.SignMinus) 
                { 
                    Match(new List<LexemeType> () { LexemeType.SignPlus, LexemeType.SignMinus });
                }
                Match(new List<LexemeType>() { LexemeType.Float, LexemeType.Integer });

                Match(LexemeType.EndOfExpression);
            }
        }
    }
}
