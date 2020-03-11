using System;
using SFML.System;
using SFML.Audio;

namespace PartialMusicAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Analyzer analyzer = new Analyzer();
            analyzer.Enable = true;
            Music music = new Music("example2.ogg");
            music.Play();
            while (music.Status==SoundStatus.Playing)
            {
                
            }
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