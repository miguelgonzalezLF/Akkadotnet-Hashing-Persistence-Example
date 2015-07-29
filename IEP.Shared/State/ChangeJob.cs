using System;

namespace IEP.Shared.State
{
    public class ChangeJob
    {
        public ChangeJob(string fileName, string change, ConsoleColor color)
        {
            FileName = fileName;
            Change = change;
            Color = color;
        }

        public string FileName { get; set; }
        public string Change { get; set; }
        public ConsoleColor Color { get; set; }
    }
}