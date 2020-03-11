using System;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace SpectrumTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SoundAnalysis analyzer = new SoundAnalysis("test11.mp3");
            RenderWindow rw = new RenderWindow(new VideoMode(800, 600), "Audio visualization");
            rw.Closed += (sender, eventArgs) =>
            {
                analyzer.Free();
                Environment.Exit(0);
            };
            
            while (rw.IsOpen)
            {
                rw.DispatchEvents();
                rw.Clear(Color.White);
                if (analyzer.spetrumData.Length>0)
                {
                    for (int i = 0; i < analyzer.spetrumData.Length; i++)
                    {
                        var rnd = new Random();
                        var s = new RectangleShape();
                        s.OutlineColor = Color.Black;
                        s.OutlineThickness = 10f;
                        s.Position = new Vector2f(20*i, 600);
                        s.Size = new Vector2f(20, -analyzer.spetrumData[i]/2);
                        
                        var r = new RectangleShape();
                        r.OutlineColor = Color.Black;
                        r.OutlineThickness = 10f;
                        r.Position = new Vector2f(20*i, 0);
                        r.Size = new Vector2f(20, analyzer.spetrumData[i]/2);
                        rw.Draw(s);
                        rw.Draw(r);
                    }
                }

                rw.Display();
            }
            analyzer.Free();
        }
    }
    
}