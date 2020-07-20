using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Newtonsoft.Json;


namespace PartialMusicAnalyzer
{
    class Program
    {
     
        static void Main(string[] args)
        {
            
            BeatMap beats = new BeatMap();
            string filename = "xevel";
            SoundAnalysis analyzer = new SoundAnalysis($"{filename}.mp3");
            analyzer.OnBeat += (delay, tempo,type) =>
            {
                beats.Add(new Beat()
                {
                    Delay = delay,
                    Tempo = tempo,
                    BeatType = type
                });
            }; 
            RenderWindow rw = new RenderWindow(new VideoMode(1250, 600), "Audio visualization");
               
            rw.Closed += (sender, eventArgs) =>
            {
                var json = JsonConvert.SerializeObject(beats.GetArray());
                var file = new StreamWriter($"{filename}.json");
                file.Write(json);
                file.Flush();
                file.Close();
                analyzer.Free();
                Environment.Exit(0);
            };
            
            Color col = Color.White;

            var s = new RectangleShape();
            s.OutlineColor = Color.Black;
            s.OutlineThickness = 10f;
            
            while (rw.IsOpen)
            {
                rw.DispatchEvents();
                rw.Clear(col);
                
                    for (int i = 0; i < analyzer.oldSpetrumData.Length; i++)
                    {
                        s.Position = new Vector2f(20 * i, 600);
                        s.Size = new Vector2f(20, -analyzer.oldSpetrumData[i] / 2);
                        
                        rw.Draw(s);
                        
                        s.Position = new Vector2f(20 * i, 0);
                        s.Size = new Vector2f(20, analyzer.oldSpetrumData[i] / 2);

                        rw.Draw(s);
                    }

                rw.Display();
            }
        }
        
    }
    
}