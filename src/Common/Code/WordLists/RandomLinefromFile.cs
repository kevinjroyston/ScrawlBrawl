using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Common.Code.WordLists
{
    public static class RandomLineFromFile
    {
        static ConcurrentDictionary<FileNames, ConcurrentBag<string>> FilesToLines { get; set; } = new ConcurrentDictionary<FileNames, ConcurrentBag<string>>();
        static ConcurrentDictionary<FileNames, string> Files { get; set; } = new ConcurrentDictionary<FileNames, string>(new Dictionary<FileNames, string>
        {
            { FileNames.Nouns, "WordLists\\Nouns.txt" }
        });

        /// <summary>
        /// Needed because for some reason the default working directory is in the src folder -_-
        /// https://github.com/dotnet/project-system/issues/2239
        /// </summary>
        static string WorkingDirectory { get; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private static object ReadFileLock { get; set; } = new Object();
        private static Random Rand { get; set; } = new Random();
        public static string GetRandomLine(FileNames file)
        {
            if (!FilesToLines.ContainsKey(file))
            {
                lock (ReadFileLock)
                {
                    if (!FilesToLines.ContainsKey(file))
                    {
                        FilesToLines[file] = new ConcurrentBag<string>(
                            File.ReadAllLines(Path.Combine(WorkingDirectory, Files[file])));
                    }
                }
            }
            // TODO: fix inefficiency below.
            return FilesToLines[file].ToArray()[Rand.Next(0, FilesToLines[file].Count)];
        }
        public static List<string> GetRandomLines(FileNames file, int count)
        {
            List<string> strings = new List<string>();
            for (int i = 0; i < count; i++)
            {
                strings.Add(GetRandomLine(file));
            }
            return strings;
        }
    }
}
