using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.WordLists
{
    public static class RandomLineFromFile
    {
        static Dictionary<FileNames, List<string>> FilesToLines { get; set; } = new Dictionary<FileNames, List<string>>();
        static Dictionary<FileNames, string> Files { get; set; } = new Dictionary<FileNames, string>
        {
            { FileNames.Nouns, "WordLists\\Nouns.txt" }
        };
        static Random rand { get; set; } = new Random();
        public static string GetRandomLine(FileNames file)
        {
            if (!FilesToLines.ContainsKey(file))
            {
                FilesToLines[file] = new List<string>(File.ReadAllLines(Files[file]));
            }
            return FilesToLines[file][rand.Next(0, FilesToLines[file].Count)];
        }
        public static List<string> GetRandomLines(FileNames file, int count)
        {
            List<string> strings = new List<string>();
            for (int i =0; i< count; i++)
            {
                strings.Add(GetRandomLine(file));
            }
            return strings;
        }
    }
}
