using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;


namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TICKET
            SoundAnalysis analyzer = new SoundAnalysis("test9.mp3");
            RenderWindow rw = new RenderWindow(new VideoMode(1250, 600), "Audio visualization");
               
            rw.Closed += (sender, eventArgs) =>
            {
                analyzer.Free();
                Environment.Exit(0);
            };
            
            Color col = Color.White;
            
            //analyzer.OnBeat += () => col = Color.Black;
            while (rw.IsOpen)
            {
                rw.DispatchEvents();
                rw.Clear(col);
           
                if (analyzer.oldSpetrumData.Length>0)
                {
                    
                    for (int i = 0; i < analyzer.oldSpetrumData.Length; i++)
                    {
                        var s = new RectangleShape();
                        s.OutlineColor = Color.Black;
                        s.OutlineThickness = 10f;
                        s.Position = new Vector2f(20*i, 600);
                        s.Size = new Vector2f(20, -analyzer.oldSpetrumData[i]/2);
                        
                        var r = new RectangleShape();
                        r.OutlineColor = Color.Black;
                        r.OutlineThickness = 10f;
                        r.Position = new Vector2f(20*i, 0);
                        r.Size = new Vector2f(20, analyzer.oldSpetrumData[i]/2);
                        
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