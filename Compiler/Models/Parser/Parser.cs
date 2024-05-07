using Compiler.Models.Lexical;
using System.Text;
namespace Compiler.Models.Parser
{
    public class EarlyEndOfExpressionException : Exception
    {
        public EarlyEndOfExpressionException(string message) : base(message){ }
        public EarlyEndOfExpressionException() : base(){ }
    }
    public static class Parser
    {
        private static List<Lexeme>? _lexemes;
        private static int _currentIndex;
        private static List<ParsedError> _errors;
        private static Lexeme CurrentLexeme => _lexemes[_currentIndex];

        private static void Consume() => _currentIndex++;
        
        private static void ReportError(string message, int startIndex, int length) => _errors.Add(new ParsedError(message, startIndex, length));
        private static bool Match(LexemeType expectedType, List<LexemeType>? boundaryLexemes) => Match(new List<LexemeType> { expectedType }, boundaryLexemes);
        private static bool Match(IEnumerable<LexemeType> expectedType, List<LexemeType>? boundaryLexemes)
        {
            if (_currentIndex < _lexemes.Count)
            {
                while (CurrentLexeme.Type == LexemeType.NewLine) 
                { 
                    Consume(); 
                }
                foreach(var type in expectedType)
                {
                    if (type == CurrentLexeme.Type)
                    {
                        Consume();
                        return true;
                    }
                }
                if (boundaryLexemes != null)
                {
                    Neutralization(expectedType, boundaryLexemes);
                }
                return false;
            }
            else 
            {
                ReportError(CreateExpectedTypeMessage(expectedType), _lexemes.Last().StartIndex, _lexemes.Last().Length);
                throw new EarlyEndOfExpressionException();
            }
            return false;
        }

        public static List<ParsedError> Parse(List<Lexeme> lexemes)
        {
            _lexemes = lexemes;
            _currentIndex = 0;
            _errors = new List<ParsedError>();

            try
            {
                Program();
            }
            catch (EarlyEndOfExpressionException ex)
            {
            }

            return _errors;
        }
        private static void Program()
        {
            while (_currentIndex < _lexemes.Count)
            {
                LF();
            }
        }
        private static void LF()
        {
            while (CurrentLexeme.Type == LexemeType.EndOfExpression)
            {
                Consume();
            }

            Match(LexemeType.OpenBracket, new List<LexemeType> { LexemeType.InverseSlash});
            Match(LexemeType.InverseSlash, new List<LexemeType> { LexemeType.SignMinus});
            ARGFUNC();
        }
        private static void ARGFUNC()
        {
            if (Match(LexemeType.SignMinus, null))
            {
                ARROW();
            }
            else
            {
                ARGFUNCREM();
            }
        }

        private static void ARGFUNCREM()
        {
            Match(LexemeType.Identifier, new List<LexemeType> { LexemeType.SignMinus });
                
            ARGFUNC();
        }

        private static void ARROW()
        {
            Match(LexemeType.SignMore, new List<LexemeType> { LexemeType.Identifier });
            FIRST_OPERAND();
        }

        private static void FIRST_OPERAND()
        {
            Match(LexemeType.Identifier, new List<LexemeType> { LexemeType.SignMinus, LexemeType.SignPlus, LexemeType.SignMultiply, LexemeType.SignDevide });   
            OPERATION();
        }

        private static void OPERATION()
        {
            //MatchOperation();
            Match(new List<LexemeType> { LexemeType.SignMinus, LexemeType.SignPlus, LexemeType.SignMultiply, LexemeType.SignDevide }, new List<LexemeType> { LexemeType.Identifier });
            SECOND_OPERAND();
        }

        private static void SECOND_OPERAND()
        {
            Match(LexemeType.Identifier, new List<LexemeType> { LexemeType.SignMinus, LexemeType.SignPlus, LexemeType.SignMultiply, LexemeType.SignDevide });
            if (Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus, LexemeType.SignDevide, LexemeType.SignMultiply }, null))
            {
                SECOND_OPERAND();
            }
            else 
            {
               Match(LexemeType.CloseBracket, new List<LexemeType> { LexemeType.SignMinus, LexemeType.SignPlus, LexemeType.Float, LexemeType.Integer});
               NUMS();
            }
        }

        private static void NUMS() 
        {
            Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus }, null); 
            Match(new List<LexemeType>() { LexemeType.Float, LexemeType.Integer}, new List<LexemeType> { LexemeType.SignMore, LexemeType.SignPlus, LexemeType.Identifier});
            Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus }, null);
            Match(new List<LexemeType>() { LexemeType.Float, LexemeType.Integer}, new List<LexemeType> { LexemeType.EndOfExpression });
            Match(LexemeType.EndOfExpression, new List<LexemeType> { });
        }

        private static void Neutralization(IEnumerable<LexemeType> expectedType, List<LexemeType>? boundaryLexemes)
        {
            var messageExpType = CreateExpectedTypeMessage(expectedType);
            var messageSkipedType = CreateSkipedTypeMessage(expectedType);

            var startIndex = CurrentLexeme.StartIndex;

            var parseIndex = _currentIndex;

            while (parseIndex < _lexemes.Count)
            {
                if (expectedType.Contains(_lexemes[parseIndex].Type))
                {
                    ReportError(messageExpType, startIndex, _lexemes[parseIndex - 1].EndIndex - startIndex);
                    _currentIndex = parseIndex;
                    Consume();
                    return;
                }
                if (boundaryLexemes!.Contains(_lexemes[parseIndex].Type))
                {
                    ReportError(messageSkipedType, startIndex, 1);
                    _currentIndex = parseIndex;
                    return;
                }
                parseIndex++;
            }
            ReportError(messageSkipedType, startIndex, 1);
            if (parseIndex < _lexemes.Count)
            {
                Consume();
            }

        }

        private static void Neutraliz(IEnumerable<LexemeType> expectedType, List<LexemeType>? boundaryLexemes) 
        {
            var message = CreateExpectedTypeMessage(expectedType);
            
            var startIndex = CurrentLexeme.StartIndex;
            var parseIndex = _currentIndex;

            var isBounded = false;
            if (parseIndex < _lexemes.Count && boundaryLexemes!.Contains(_lexemes[parseIndex].Type))
            {
                isBounded = true;
            }

            while (parseIndex < _lexemes.Count && !expectedType.Contains(_lexemes[parseIndex].Type))
            {
                parseIndex++;
            }
            if (parseIndex >= _lexemes.Count)
            {
                if (boundaryLexemes != null)
                {
                    var parseIndex_2 = _currentIndex;
                    while (parseIndex_2 < _lexemes.Count && !boundaryLexemes.Contains(_lexemes[parseIndex_2].Type))
                    {
                        parseIndex_2++;
                    }
                    if (parseIndex_2 < _lexemes.Count)
                    {
                        _currentIndex = parseIndex_2;
                        ReportError(message, startIndex, CurrentLexeme.Value.Length);
                        Consume();
                    }
                    else 
                    {
                        ReportError(CreateSkipedTypeMessage(expectedType), startIndex, 1);
                    }
                }
                else
                {
                    ReportError(CreateSkipedTypeMessage(expectedType), startIndex, 1);
                }
            }
            else
            {
                if (isBounded && !expectedType.Contains(_lexemes[parseIndex].Type))
                {
                    ReportError(CreateSkipedTypeMessage(expectedType), _lexemes[_currentIndex - 1].EndIndex, 1);
                    return;
                }
                for (var i = _currentIndex; i < parseIndex; i++) 
                {
                //ReportError(message + $"\nОткинутые элементы с {startIndex} по {_lexemes[parseIndex - 1].EndIndex}", startIndex, _lexemes[parseIndex - 1].EndIndex - startIndex);
                    ReportError(message + $"\nОткинут элемен с {_lexemes[i].StartIndex} по {_lexemes[i].EndIndex}", _lexemes[i].StartIndex, _lexemes[i].EndIndex - _lexemes[i].StartIndex);
                }
                _currentIndex = parseIndex;
                Consume();

            }


        }

        private static string CreateExpectedTypeMessage(IEnumerable<LexemeType> expectedType)
        {
            var types = new StringBuilder();

            int i = 0;
            foreach (var type in expectedType)
            {
                types.Append(type.ToString() + ((expectedType.Count() > 1 && i < expectedType.Count() - 1) ? ", " : ""));
                i++;            
            }
            try
            {
                var currentType = CurrentLexeme.Type.ToString();
                return new string($"Ожидалось:[ {types} ], пришло {currentType}");
            }
            catch 
            {
                return new string($"Ожидалось:[ {types} ], выражение кончилось");
            }
        }
        private static string CreateSkipedTypeMessage(IEnumerable<LexemeType> expectedType)
        {
            var types = new StringBuilder();

            int i = 0;
            foreach (var type in expectedType)
            {
                types.Append(type.ToString() + ((expectedType.Count() > 1 && i < expectedType.Count() - 1) ? ", " : ""));
                i++;
            }
            try
            {
                var currentType = CurrentLexeme.Type.ToString();
                return new string($"Пропущено:[ {types} ]");
            }
            catch
            {
                return new string($"Пропущено:[ {types} ]");
            }
        }
    }
}
