namespace Compiler.Models
{
    public enum LexemeType : int
    {
        Identifier = 0,         // имя переменной
        Whitespace,             // пробел
        StartOfLambdaArguments, // \
        NewLine,                // \n
        CarriageReturn,         // \r
        LambdaArrow,            // ->
        OpenBracket,            // (
        CloseBracket,           // )
        Integer,                // {цифры}
        Float,                  // {цифры.цифры}
        SignMinus,              // -
        SignPlus,               // +
        SignMultiply,           // *
        SignDevide,             // /

        InvalidCharacter        // ОШИБКА
    }
}
