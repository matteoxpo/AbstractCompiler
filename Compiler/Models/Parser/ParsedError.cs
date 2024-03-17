namespace Compiler.Models.Parser;

public class ParsedError
{
    public ParsedError(string message, int startIndex, int endIndex)
    {
        Message = new string(message);
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public string Message { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string Position { get => $"{StartIndex}: {EndIndex}"; }

    public override string ToString()
    {
        return $"Error: {Message}, Position: {StartIndex}-{EndIndex}";
    }

}
