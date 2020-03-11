using System;
using System.Collections.Generic;
using System.Linq;
using SFML.System;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;

namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Analyzer analyzer = new Analyzer("test12.mp3");
            RenderWindow rw = new RenderWindow(new VideoMode(800, 600), "Analyze");
            
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
                        rw.Draw(s);
                    }
                }

                rw.Display();
            }
            analyzer.Free();
            //Music music = new Music("example2.ogg");
            // music.Play();
            //while (music.Status==SoundStatus.Playing)
            //{

            //}
            /*Clock clock = new Clock();
            Sound music = new Sound();
            music.SoundBuffer = new SoundBuffer("example2.ogg");
            music.Play();
            FFT fft = new FFT();
            int ind = 1;

            Console.WriteLine($"{music.SoundBuffer.Samples.Length} {music.SoundBuffer.SampleRate} {music.SoundBuffer.Samples.Length / music.SoundBuffer.SampleRate / 2} {music.SoundBuffer.Duration.AsSeconds()}");
            clock.Restart();
            float[] data = fft.ComplexFFT(Array.ConvertAll(music.SoundBuffer.Samples, s => float.Parse(s.ToString())),
                (ulong) music.SoundBuffer.Samples.Length,
                music.SoundBuffer.SampleRate, 1);
            Console.WriteLine(data.Length);
            while (music.Status == SoundStatus.Playing)
            {
                if (clock.ElapsedTime.AsMilliseconds() > 100)
                {
                    Console.WriteLine(data[(int)(music.SoundBuffer.SampleRate*ind)]);
                    clock.Restart();
                    ind++;
                }
            }
            Console.WriteLine(music.SoundBuffer.SampleRate/5*ind);*/
        }
    }
    
}