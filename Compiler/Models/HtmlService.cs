using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Compiler.Models;

public static class HtmlService
{
    public static void Open(string path)
    {
        new Process() { StartInfo = new ProcessStartInfo(path) { UseShellExecute = true } }.Start();
    }

    public static bool IsUrl(string input) => Regex.IsMatch(input, @"^(https?|ftp)://[^\s/$.?#].[^\s]*$");
}
