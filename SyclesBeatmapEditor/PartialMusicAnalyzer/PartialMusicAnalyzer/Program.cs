using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.Window;


namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            SoundAnalysis analyzer = new SoundAnalysis("example.mp3");
            var window = new RenderWindow(new VideoMode(300,300),"Beat detector");
            analyzer.WriteSpectrumToFile("example.mp3","a.txt");
            window.Closed += (sender, eventArgs) =>
            {
                analyzer.Dispose();
                Environment.Exit(0);
            };
            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear();
                window.Display();
            }
        }
    }
    
}