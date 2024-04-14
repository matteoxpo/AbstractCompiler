using Compiler.Models.Lexical;

namespace Compiler.Models.Lexical;
public static class LexicalAnalyzer
{
    public static List<char> ReservedLexicals = new List<char>() { ' ', '\n', '\\', '-', '+', '/', '(', ')', '\t', '\r', '>' };
    public static List<Lexeme> Analyze(string rawText)
    {
        var lexemes = new List<Lexeme>();
        int index = 0;
        while (index < rawText.Length)
        {
            char currentChar = rawText[index];
            switch (currentChar)
            {
                case ';':
                    lexemes.Add(new Lexeme(LexemeType.EndOfExpression, ";", index));
                    break;
                case '(':
                    lexemes.Add(new Lexeme(LexemeType.OpenBracket, "(", index));
                    break;
                case '\\':
                    lexemes.Add(new Lexeme(LexemeType.InverseSlash, "\\", index));
                    break;
                case ' ':
                case '\t':
                    //lexemes.Add(new Lexeme(LexemeType.Whitespace, " \'  \'", index));
                    break;
                case '\r':
                case '\n':
                    lexemes.Add(new Lexeme(LexemeType.NewLine, "\\n", index));
                    index++;
                    break;
                case '>':
                    lexemes.Add(new Lexeme(LexemeType.SignMore, currentChar.ToString(), index));
                    break;
                case '-':
                    lexemes.Add(new Lexeme(LexemeType.SignMinus, currentChar.ToString(), index));
                    break;
                case '+':
                    lexemes.Add(new Lexeme(LexemeType.SignPlus, currentChar.ToString(), index));
                    break;
                case '*':
                    lexemes.Add(new Lexeme(LexemeType.SignMultiply, currentChar.ToString(), index));
                    break;
                case ':':
                    lexemes.Add(new Lexeme(LexemeType.SignDevide, currentChar.ToString(), index));
                    break;
                case ')':
                    lexemes.Add(new Lexeme(LexemeType.CloseBracket, currentChar.ToString(), index));
                    break;
                default:
                    if (char.IsDigit(currentChar))
                    {
                        int startIndex = index;
                        while (index < rawText.Length && (char.IsDigit(rawText[index]) || rawText[index] == '.'))
                        {
                            if (rawText[index] == '.')
                            {
                                if (rawText.Substring(startIndex, index - startIndex).Contains('.'))
                                {
                                    lexemes.Add(new Lexeme(LexemeType.InvalidCharacter, currentChar.ToString(), index));
                                    index++;
                                    break;
                                }
                            }
                            index++;
                        }
                        //int endIndex = index - 1;
                        int endIndex = index - 1; // Исправление: использование правильного endIndex
                        string number = rawText.Substring(startIndex, endIndex - startIndex + 1);
                        if (number.Contains('.'))
                        {
                            lexemes.Add(new Lexeme(LexemeType.Float, number, startIndex));
                        }
                        else
                        {
                            lexemes.Add(new Lexeme(LexemeType.Integer, number, startIndex));
                        }
                        index--;
                    }
                    else if (char.IsLetter(currentChar))
                    {
                        var startIndex = index;
                        while (index + 1 < rawText.Length && (!ReservedLexicals.Contains(rawText[index + 1])))
                        {
                            index++;
                        }

                        lexemes.Add(new Lexeme(LexemeType.Identifier, rawText.Substring(startIndex, index - startIndex + 1), startIndex));
                    }
                    else
                    {
                        lexemes.Add(new Lexeme(LexemeType.InvalidCharacter, currentChar.ToString(), index));
                    }
                    break;
            }
            index++;
        }
        return lexemes;
    }
}

