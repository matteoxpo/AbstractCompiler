namespace Compiler.Models.Parser;

public class ParsedError
{
    public ParsedError(string message, int startIndex, int length)
    {
        Message = new string(message);
        StartIndex = startIndex;
        Length = length;
    }

    public string Message { get; set; }
    public int StartIndex { get; set; }
    public int Length { get; set; } 
    public string Position { get => $"{StartIndex}: {StartIndex + Length}"; }

    public override string ToString()
    {
        return $"Error: {Message}, Position: {StartIndex}-{Length}";
    }

}
