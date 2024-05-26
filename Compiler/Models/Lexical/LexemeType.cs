namespace Compiler.Models
{
    public enum LexemeType : int
    {
        Identifier = 0,         // имя переменной
        Whitespace,             // пробел
        NewLine,                // \n
        CloseBracket,           // )
        OpenBracket,            // (
        InverseSlash,           // /
        Integer,                // {цифры}
        Float,                  // {цифры.цифры}
        SignMinus,              // -
        SignPlus,               // +
        SignMultiply,           // *
        SignDevide,             // /
        SignMore,               // >
        EndOfExpression,        // ;

        InvalidCharacter        // ОШИБКА
    }
}
