using System;
using System.Numerics;
using System.Collections.Generic;
using SFML.System;
using SFML.Audio;
using SFML.Graphics;

namespace SpectrumTest
{
    
    
    public class Spectrum
    {
        public const int BUFFER_SIZE = 10;
        private List<Complex> _samples;
        private Complex _base;
        private int _sampleRate;
        private SoundBuffer _soundBuffer;
        private Sound _sound;
        private VertexArray VA1, VA2;
        private float[] _hammingWindow;
        private int _samplesRate;
        
        public Spectrum(string FileName)
        {
           _samples = new List<Complex>();

           VA1 = new VertexArray(PrimitiveType.Quads);
           VA2 = new VertexArray(PrimitiveType.LineStrip);
           _hammingWindow = new float[BUFFER_SIZE];
           
           _sound = new Sound();
           _soundBuffer = new SoundBuffer(FileName);
           _sound.SoundBuffer = _soundBuffer;

           _samplesRate = (int) _soundBuffer.SampleRate;
           
           for (int i = 0; i < BUFFER_SIZE; i++)
           {
               _samples.Add(new Complex());
               _hammingWindow[i] = (float) (0.54f - 0.46f * Math.Cos((2 * Math.PI * i) / (BUFFER_SIZE)));
           }
           TakeSamplesAndHammingWindow();
        }

        public void TakeSamplesAndHammingWindow()
        {
            int offset = (int) (_sound.PlayingOffset.AsSeconds() * _samplesRate);

            for (int i = 0; (i < BUFFER_SIZE); i++)
            {
                _samples[i] = new Complex(_soundBuffer.Samples[i + offset] * (float)_hammingWindow[i], 0);
                
            }   
        }

        public void Update(float dt)
        {
            TakeSamplesAndHammingWindow();
            mfft(ref _samples, false);
            UpdateVertexArray();
        }

        public void Draw(RenderWindow rw)
        {
            rw.Draw(VA1);
        }

        public void UpdateVertexArray()
        {
            VA1.Clear();
            //VA2.Clear();

            Vector2f original_pos = new Vector2f(50f, 500f);


            for (int i = 0; i < BUFFER_SIZE / 2 && i < 200; i++)
            {
                //sf::Vector2f samples_pos(i + i, abs(_samples[i]));
                float dB = (float) Math.Abs(_samples[i].Real);

                VA1.Append(new Vertex(new Vector2f(30 + 5 * i, 500), Color.Blue));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i, 500 - dB / 100000f), Color.White));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i + 3, 500 - dB / 100000f), Color.White));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i + 3, 500), Color.Blue));

                VA1.Append(new Vertex(new Vector2f(30 + 5 * i, 500), new Color(255, 255, 255, 100)));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i + 3, 500), new Color(255, 255, 255, 100)));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i + 3, 500 + dB / 500000.0f), new Color(255, 255, 255, 0)));
                VA1.Append(new Vertex(new Vector2f(30 + 5 * i, 500 + dB / 500000.0f), new Color(255, 255, 255, 0)));

                //VA2.append(sf::Vertex(sf::Vector2f(30 + 5 * i, 500 - dB / 100000.f), sf::Color::White));
		
            }
        }

        public void Play()
        {
            _sound.Play();
        }
        
        private void mfft(ref List<Complex> a, bool invert) {
            int n = (int)a.Count;

            for (int i = 1, j = 0; i<n; ++i) {
                int bit = n >> 1;
                for (; j >= bit; bit >>= 1)
                    j -= bit;
                j += bit;
                if (i < j)
                {
                    swap(ref a, a[i], a[j]);
                }
            }

            for (int len = 2; len <= n; len <<= 1) {
                double ang = 2 * Math.PI / len * (int)(invert ? -1 : 1);
                Complex wlen = new Complex(Math.Cos(ang), Math.Sin(ang));
                for (int i = 0; i<n; i += len) {
                    Complex w = Complex.One;
                    for (int j = 0; j<len / 2; ++j)
                    {
                        Complex u = a[i + j];
                        Complex v = a[i + j + len / 2] * w;
                        a[i + j] = u + v;
                        a[i + j + len / 2] = u - v;
                        w *= wlen;
                    }
                }
            }
            if (invert)
                for (int i = 0; i<n; ++i)
                    a[i] /= n;
        }
        private void swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }
        private void swap<R,T>(ref R obj, T a, T b) where R: IList<T>
        {
            var t = a;
            obj[obj.IndexOf(a)] = b;
            obj[obj.IndexOf(b)] = t;
        }
    }
    
}