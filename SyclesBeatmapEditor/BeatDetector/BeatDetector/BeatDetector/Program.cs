﻿using System;
using System.Linq;
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
            RenderWindow rw = new RenderWindow(new VideoMode(1250, 600), "Audio visualization");
               
            rw.Closed += (sender, eventArgs) =>
            {
                analyzer.Free();
                Environment.Exit(0);
            };
            
            Color col = Color.White;
            Text text = new Text(" ", new Font("arial.ttf"));
            text.Position = new Vector2f(0, 300);
            text.FillColor = Color.Black;
            
            var beat = new CircleShape();
            beat.Position = new Vector2f(0, 300);
            beat.FillColor = Color.Black;
            beat.OutlineColor = Color.Black;
            analyzer.OnBeat += () => col = Color.Black;
            while (rw.IsOpen)
            {
                rw.DispatchEvents();
                rw.Clear(col);
                if (col.R != 255)
                {
                    col = new Color((byte) (col.R+1),(byte) (col.R+1),(byte) (col.R+1));
                }
                if (analyzer.spetrumData.Length>0)
                {
                    
                    for (int i = 0; i < analyzer.spetrumData.Length; i++)
                    {
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
                rw.Draw(beat);
                rw.Display();
            }
            analyzer.Free();
        }

        private static string GenStr(byte[] analyzerSpetrumData)
        {
            var res = "";
            foreach (var b in analyzerSpetrumData)
            {
                res += b.ToString() + " ";
            }

            return res;
        }
    }
    
}