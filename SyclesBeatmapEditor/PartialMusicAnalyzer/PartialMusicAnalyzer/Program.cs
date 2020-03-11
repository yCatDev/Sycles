using System;
using System.Collections.Generic;
using System.Linq;


namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Analyzer analyzer = new Analyzer("example.mp3");
            Console.ReadLine();
            analyzer.Free();
        }
    }
    
}