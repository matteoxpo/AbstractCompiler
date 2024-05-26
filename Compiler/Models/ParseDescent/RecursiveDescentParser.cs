
namespace Compiler.Models.ParseDescent;
using System;
using System.Collections.Generic;

public class ParsingResult
{
    public bool Success { get; set; }
    public List<string> Messages { get; }
    public string Result { 
        get 
        {
            return string.Join("\n", Messages);
        }
    }

    public ParsingResult(bool success, List<string> messages)
    {
        Success = success;
        Messages = messages;
    }

    public void AddMessage(string message)
    {
        Messages.Add(message);
    }
}

public class RecursiveDescentParser
{
    private string input;
    private int index;
    private ParsingResult result;

    public ParsingResult Parse(string input)
    {
        this.input = input.Replace("\r", ""); // удаление символа \r
        index = 0;
        result = new ParsingResult(true, new List<string>());

        try
        {
            E();
        }
        catch (Exception e)
        {
            result.Success = false;
            result.AddMessage($"Parsing error: {e.Message}");
        }

        return result;
    }

    private void E()
    {
        E1();
        if (Match("⇒"))
        {
            E1();
        }
    }

    private void E1()
    {
        E2();
        if (Match("∨"))
        {
            E2();
        }
    }

    private void E2()
    {
        E3();
        if (Match("∧"))
        {
            E3();
        }
    }

    private void E3()
    {
        if (Match("¬"))
        {
            E4();
        }
        else
        {
            E4();
        }
    }

    private void E4()
    {
        if (Match("("))
        {
            E1();
            if (!Match(")"))
            {
                throw new Exception($"Expected closing parenthesis ')', but got '{input.Substring(index)}'");
            }
        }
        else if (!MatchVariable())
        {
            throw new Exception($"Expected ID, but got '{input.Substring(index)}'");
        }
        //else
        //{
        //    throw new Exception($"Syntax error, expected '(' or identifier, but got '{input.Substring(index)}'");
        //}
    }

    private bool MatchVariable() 
    {
        SkipSpace();

        int startIndex = index;
        if (index < input.Length && char.IsLetter(input[index]))
        {
            Consume();
        } 
        else if (index < input.Length)
        {
            return false;
        } 
        else 
        {
            throw new Exception("Early end of expr");
        }

        while(index < input.Length && (char.IsLetter(input[index]) || char.IsDigit(input[index] ))) 
        {
            Consume();
        }
        result.AddMessage($"exp:var_name got: {input[index]}");
        return true;
    }

    private bool Match(string expected)
    {
        SkipSpace();
        if (index + expected.Length <= input.Length && input.Substring(index, expected.Length) == expected)
        {
            result.AddMessage($"exp:{expected} got: {input.Substring(index, expected.Length)}");
            Consume();
            return true;
        }
        else if (index + expected.Length > input.Length )
        {
            //throw new Exception("Early end of expr");
        }
        return false;
    }
    private void SkipSpace()
    {
        while( index < input.Length && input[index] == ' ' ) 
        {
            Consume();
        }
    }

    private void Consume(int count = 1)
    {
        index += count;
    }
}