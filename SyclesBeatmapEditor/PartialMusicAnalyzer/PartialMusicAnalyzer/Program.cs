using System;
using System.Collections.Generic;
using System.Linq;


namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            SoundAnalysis analyzer = new SoundAnalysis("example.mp3");
            Console.ReadLine();
            analyzer.Free();
        }
    }
    
}