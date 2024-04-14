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
        private static bool[] isReclined;
        private static int _currentIndex;
        private static List<ParsedError> _errors;
        private static Lexeme CurrentLexeme => _lexemes[_currentIndex];

        private static List<(Lexeme, bool)> _usedArgs;

        private static Stack<char> _openBrackets;

        private static void Consume() => _currentIndex++;
        
        private static void ReportError(string message, int startIndex, int length) => _errors.Add(new ParsedError(message, startIndex, length));
        private static bool Match(LexemeType expectedType, bool useNeutralization = true) => Match(new List<LexemeType> { expectedType }, useNeutralization);
        private static bool Match(IEnumerable<LexemeType> expectedType, bool useNeutralization = true)
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
                if (useNeutralization)
                {
                    Neutralization(expectedType);
                }
                return false;
            }
            throw new EarlyEndOfExpressionException();
        }
        private static void MatchOperation() 
        {
            if (!Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus, LexemeType.SignDevide, LexemeType.SignMultiply }, false))
            {
                ReportError($"Ожидалась операция [{LexemeType.SignPlus}, {LexemeType.SignMinus}, {LexemeType.SignDevide}, {LexemeType.SignMultiply}], пришло: {CurrentLexeme.Type}", CurrentLexeme.StartIndex, CurrentLexeme.Value.Length);
                Consume();
            }
        }
        private static void MatchOperand()
        {
            if (Match(new List<LexemeType>() { LexemeType.Identifier, LexemeType.Integer, LexemeType.Float }))
            {
                CheckIsArgumentDeclared();
            }
        }
        private static void MatchNum() 
        { 
        
        }

        public static List<ParsedError> Parse(List<Lexeme> lexemes)
        {
            _lexemes = lexemes;
            _currentIndex = 0;
            _errors = new List<ParsedError>();
            _usedArgs = new List<(Lexeme, bool)>();
            _openBrackets = new Stack<char>();

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

            Match(LexemeType.OpenBracket);
            Match(LexemeType.InverseSlash);
            ARGFUNC();
        }
        private static void ARGFUNC()
        {
            if (Match(LexemeType.SignMinus, false))
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
            if (Match(LexemeType.Identifier))
            {
                _usedArgs.Add((_lexemes[_currentIndex - 1], false));
            }
                
            ARGFUNC();
        }

        private static void ARROW()
        {
            if (Match(LexemeType.SignMore))
            {
                FIRST_OPERAND();
            }
        }

        private static void FIRST_OPERAND()
        {
            CheckCloseBracketBeforeOperand();
            CheckOpenBracket();
            MatchOperand();
            OPERATION();
        }

        private static void OPERATION()
        {
            CheckCloseBracketBeforeOperation();
            MatchOperation();
            CheckOpenBracket();
            OTHER_OPERANDS();
        }

        private static void OTHER_OPERANDS()
        {
            MatchOperand();

            if (!CheckCloseBracketAfterOperand()) 
            {
                NUMS();
            }
            else
            {
                OPERATION();
            }
        }

        private static void NUMS() 
        {
            foreach(var arg in _usedArgs)
            {
                if (Match(new List<LexemeType>() { LexemeType.SignPlus, LexemeType.SignMinus }, false)) 
                {
                    //Consume(); 
                }
                Match(new List<LexemeType>() { LexemeType.Float, LexemeType.Integer});
            }
            Match(LexemeType.EndOfExpression);
            ReportUnusedArguments();
        }
     
        private static void Neutralization(IEnumerable<LexemeType> expectedType) 
        {
            var message = CreateExpectedTypeMessage(expectedType);
            
            var startIndex = CurrentLexeme.StartIndex;
            var parseIndex = _currentIndex;

            while (parseIndex < _lexemes.Count && !expectedType.Contains(_lexemes[parseIndex].Type))
            {
                Console.WriteLine(_lexemes[parseIndex].Type);
                parseIndex++;
            }

            if (parseIndex >= _lexemes.Count)
            {
                ReportError(message, startIndex, CurrentLexeme.Value.Length);
            }
            else
            {
                _currentIndex = parseIndex;
                ReportError(message + $"\nОткинутые элементы с {startIndex} по {_lexemes[parseIndex - 1].EndIndex}", startIndex, _lexemes[parseIndex - 1].EndIndex - startIndex);
            }

            Consume();

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
            return new string($"Ожидалось:[ {types} ], пришло {CurrentLexeme.Type}");
        }
     
        private static void CheckIsArgumentDeclared()
        {
            var checkedLexeme = _lexemes[_currentIndex - 1];
            if (checkedLexeme.Type != LexemeType.Identifier)
            {
                return;
            }

            var updatedArgs = new List<(Lexeme, bool)>();
            bool isInArgs = false;
            foreach (var arg in _usedArgs)
            {
                if (arg.Item1.Value == checkedLexeme.Value)
                {
                    updatedArgs.Add((arg.Item1, true));
                    isInArgs = true;
                }
                else
                {
                    updatedArgs.Add(arg);
                }
            }

            _usedArgs = updatedArgs;
            if (!isInArgs)
            {
                ReportError($"Имя '{checkedLexeme.Value}' не существует в текущем контексте", checkedLexeme.StartIndex, checkedLexeme.Value.Length);
            }
        }
        private static void ReportUnusedArguments()
        {
            foreach (var arg in _usedArgs)
            {
                if (!arg.Item2)
                {
                    ReportError($"Неиспользованный аргумент с именем '{arg.Item1.Value}'", arg.Item1.StartIndex, arg.Item1.Value.Length);
                }
            }
        }
        private static bool CheckOpenBracket()
        {
            bool isMatched = false;
            while (Match(LexemeType.OpenBracket, false))
            {
                isMatched = true;
                _openBrackets.Push('(');
            }
            return isMatched;
        }
        private static void CheckCloseBracketBeforeOperation() 
        {
            while (Match(LexemeType.CloseBracket, false))
            {
                if (_openBrackets.Count() == 0)
                {
                    ReportError("Закрывающая скобка без открывающей", _lexemes[_currentIndex - 1].StartIndex, _lexemes[_currentIndex - 1].Value.Length);
                }
                else
                {
                    _openBrackets.Pop();
                }
            }
        }
   
        private static bool CheckCloseBracketAfterOperand() 
        {
            if (Match(LexemeType.CloseBracket, false))
            {
                if (_openBrackets.Count() == 0)
                {
                    return false;
                }
                else 
                {
                    _openBrackets.Pop();
                    bool isMatched = Match(LexemeType.CloseBracket, false);
                    while (isMatched && _openBrackets.Count() != 0)
                    {
                        isMatched = Match(LexemeType.CloseBracket, false);
                        _openBrackets.Pop();
                    }
                    if (isMatched && _openBrackets.Count() == 0) 
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;

        }

        private static void CheckCloseBracketBeforeOperand()
        {
            while (Match(LexemeType.CloseBracket, false))
            {
                if (_openBrackets.Count() == 0)
                {
                    ReportError("Закрывающая скобка без открывающей", _lexemes[_currentIndex - 1].StartIndex, _lexemes[_currentIndex - 1].Value.Length);
                }
                ReportError("Закрывающая скобка логически неправильно расположена", _lexemes[_currentIndex - 1].StartIndex, _lexemes[_currentIndex - 1].Value.Length);

            }
        }
    }
}
