using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Channels;
using System.Timers;
using SFML.System;
using SFML.Audio;
using ManagedBass;
//using static ManagedBass.Bass;
//using ManagedBass.Wasapi;
using Utils = Microsoft.VisualBasic.CompilerServices.Utils;

namespace SpectrumTest
{
   class SoundAnalysis
    {
        
        
        private readonly float[] _fft;      
        private List<byte> _spectrumdata;
        private int _lines = 64;            
        private Timer _timer;
        private List<string> _devicelist;
        public float beat;
        private Clock _clock;
            
        private int handle;

        public byte[] spetrumData;

        internal delegate void BeatHandler();

        public event BeatHandler OnBeat;
        
        public SoundAnalysis(string filename)
        {
            _clock = new Clock();
            _clock.Restart();
            _fft = new float[16384];
            _timer = new Timer();
            _timer.Elapsed+= OnTick;
            _timer.Interval = TimeSpan.FromMilliseconds(10).Milliseconds; 

            _spectrumdata = new List<byte>();
            Init(filename);
        }
        
        private void Init(string filename)
        { 
            
            
            bool result = Bass.Init();
            if (!result) throw new Exception("Init Error");
            
            handle = Bass.CreateStream(filename);
            Bass.ChannelPlay(handle);
            Bass.SuppressMP3ErrorCorruptionSilence = true;
            Console.WriteLine(Bass.LastError);

            spetrumData = new byte[1];
            
            _timer.Enabled = _timer.AutoReset = true;
            _timer.Start();
        }
        
        private void OnTick(object sender, EventArgs e)
        {
            
            int ret = Bass.ChannelGetData(handle,_fft, (int)DataFlags.FFT16384);
            if (ret < -1) return;
            int x, y;
            int b0 = 0;
            
            //FFT from BASS_WASAPI sample
            for (x = 0; x < _lines; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (_lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                _spectrumdata.Add((byte)y);
            }
            
            float tmp_beat = 0;
            float middleBeat = 0;
            
            for (int i = 0; i < spetrumData.Length; i++)
            {
                tmp_beat += (_spectrumdata[i]-spetrumData[i]);
            }
            tmp_beat /= spetrumData.Length;
            if (tmp_beat > beat && _clock.ElapsedTime.AsSeconds()>0.15f)
            {
                beat = tmp_beat;
                OnBeat?.Invoke();
                Console.WriteLine($"Beat on {_clock.ElapsedTime.AsSeconds()}");
                _clock.Restart();
            }
            else
            {
                beat = 0;
            }

            
            spetrumData = _spectrumdata.ToArray();
            _spectrumdata.Clear();

        }
        
        public void Free()
        {
            Bass.StreamFree(handle);
            Bass.Free();
        }
    }
}