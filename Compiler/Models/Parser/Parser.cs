using Compiler.Models.Lexical;
using System.Text;
namespace Compiler.Models.Parser
{
    public static class Parser
    {
        private static List<Lexeme>? _lexemes;
        private static bool[] isReclined;
        private static int _currentIndex;
        private static bool _useRecline;
        private static List<ParsedError> _errors;
        private static Lexeme CurrentLexeme => _lexemes[_currentIndex];

        private static Stack<LexemeType> _requiredTypes;

        private static void Consume() => _currentIndex++;
        
        private static void ReportError(string message, int startIndex, int endIndex) => _errors.Add(new ParsedError(message, startIndex, endIndex));
        private static void Match(LexemeType expectedType) => Match(new List<LexemeType> { expectedType });
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
            }
            Recline(expectedType);
        }
        public static List<ParsedError> Parse(IEnumerable<Lexeme> lexemes, bool useRecline)
        {
            _useRecline = useRecline;
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
                Match(LexemeType.OpenBracket);
                Match(LexemeType.InverseSlash);
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
                    ARGFUNCREM();
                }
            }
        }

        private static void ARGFUNCREM()
        {
            Match( LexemeType.Identifier);
            ARGFUNC();
            //if (_currentIndex < _lexemes.Count && (CurrentLexeme.Type == LexemeType.Identifier || CurrentLexeme.Type == LexemeType.NewLine))
            //{
                //Consume();
            //}
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
        private static void Recline(IEnumerable<LexemeType> expectedType) 
        {
            var message = CreateExpectedTypeMessage(expectedType);
            
            //if (!_useRecline)
            //{
            //    //if (expectedType.First == LexemeType.)

            //    ReportError(message, CurrentLexeme.StartIndex, CurrentLexeme.EndIndex);
            //    if (_currentIndex < _lexemes.Count)
            //    {
            //        Consume();
            //    }
            //    return;
            //}
            
            var startIndex = CurrentLexeme.StartIndex;
            var parseIndex = _currentIndex;

            while (parseIndex < _lexemes.Count && !expectedType.Contains(_lexemes[parseIndex].Type))
            {
                parseIndex++;
            }
            if (parseIndex >= _lexemes.Count) 
            {
                ReportError(message , startIndex, CurrentLexeme.EndIndex + 1);
            }

            _currentIndex = parseIndex;

            if (_currentIndex >= _lexemes.Count)
            {
                _currentIndex--;
                ReportError(message + $"\nОткинутые элементы с {startIndex} по {CurrentLexeme.EndIndex}", startIndex, CurrentLexeme.EndIndex + 1);
                _currentIndex++;
            }
            else if (_currentIndex < _lexemes.Count) 
            {
                ReportError(message + $"\nОткинутые элементы с {startIndex} по {CurrentLexeme.EndIndex}", startIndex, CurrentLexeme.EndIndex - 1);
                _currentIndex++;
            } 

        }

        private static string CreateExpectedTypeMessage(IEnumerable<LexemeType> expectedType)
        {
            var types = new StringBuilder();

            foreach (var type in expectedType)
            {
                types.Append(type.ToString() + (expectedType.Count() > 1 ? ", " : ""));
            }

            return new string($"Ожидалось:[ {types} ], пришло {CurrentLexeme.Type}");
        }
        private static void Recline(LexemeType expectedType) => Recline(new List<LexemeType> { expectedType });
    }
}
