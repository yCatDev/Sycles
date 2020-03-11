using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpectrumTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type file name and press p to play audio");
            Console.WriteLine("File name is name of audio file stored in Resoureces");
            Console.WriteLine("File name (ass.wav): ");
            string fileName = "example2.ogg";
            //Console.ReadLine();
            Console.WriteLine("\nLoading...");

            Spectrum spectrum = new Spectrum(fileName);

            bool isPlayed = false;
            float time_per_frame = 1 / 60.9f;
            float time_since_last_update = 0.9f;
            Clock c = new Clock();

            float time_fps = 0.9f;
            int count_frame = 0;

            RenderWindow window = new RenderWindow(new VideoMode(1000, 600), "Spectrum!");
            spectrum.Play();
            isPlayed = true;


            while (window.IsOpen)
            {
                window.DispatchEvents();
                spectrum.Update(time_per_frame);
                
                window.Clear();
                spectrum.Draw(window);
                window.Display();

                time_fps += c.ElapsedTime.AsSeconds();
                count_frame++;
                if (time_fps >= 1.0f)
                {
                    time_fps -= 1.0f;
                    
                    count_frame = 0;
                }
            }
        }
    }
}