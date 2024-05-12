using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Compiler.Models.MRegex
{
    public class RegexSettings
    {
        public List<string> Rules { get; set; }
    }

    public static class RegexParser
    {
        private static string GetFullPath(string path) => System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, path);

        public static List<RegexCorrectValue> Parse(string rawText)
        {
            var correctValues = new List<RegexCorrectValue>();

            // Чтение правил из файла
            var settings = LoadRegexSettings(GetFullPath(@"Models\MRegex\RegexSettings.json"));

            foreach (var rule in settings.Rules)
            {
                // Применение регулярного выражения к rawText
                
                var matches = System.Text.RegularExpressions.Regex.Matches(rawText, rule);

                // Добавление совпадающих значений в correctValues
                foreach (Match match in matches)
                {
                    correctValues.Add(new RegexCorrectValue(match.Value, rule, match.Index, match.Length));
                }
            }

            return correctValues;
        }

        private static RegexSettings LoadRegexSettings(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                var serializer = new JsonSerializer();
                return (RegexSettings)serializer.Deserialize(file, typeof(RegexSettings));
            }
        }
    }
}