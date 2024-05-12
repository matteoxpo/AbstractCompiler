namespace Compiler.Models.MRegex;
public class RegexCorrectValue
{
    public RegexCorrectValue(string value, string regexRule, int startIndex, int length)
    {
        Value = value;
        RegexRule = regexRule;
        StartIndex = startIndex;
        Length = length;
    }

    public string Value { get; }
    public string RegexRule { get; }
    public int StartIndex { get; set; }
    public int Length { get; }
    public string Position { get => $"{StartIndex}: {StartIndex + Length}"; }

    public override string ToString()
    {
        return $"Found regex correct value: {Value}, Rule: {RegexRule}, Position: {StartIndex}-{Length}";
    }
}
