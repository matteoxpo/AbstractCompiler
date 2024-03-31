using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Compiler.Models;

public static class HtmlService
{
    public static void Open(string path)
    {
        new Process() { StartInfo = new ProcessStartInfo(path) { UseShellExecute = true }}.Start();
        //if (!System.IO.File.Exists(path) && !IsUrl(path))
            //throw new ArgumentException($"Path: \'{path}\' is incorrect");

    }

    public static bool IsUrl(string input) => Regex.IsMatch(input, @"^(https?|ftp)://[^\s/$.?#].[^\s]*$");
}
