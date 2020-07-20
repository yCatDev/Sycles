using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SFML.System;
using ManagedBass;

using Utils = Microsoft.VisualBasic.CompilerServices.Utils;

namespace PartialMusicAnalyzer
{
   class SoundAnalysis
    {
        
        
        private readonly float[] _fft;      
        
        private int _lines = 64;            
        private Timer _timer;
        private List<string> _devicelist;
        public float beat;
        private Clock _clock;
            
        private int handle;

        public byte[] newSpectrumData;
        public byte[] oldSpetrumData;
        private List<float> _peakArray = new List<float>();
        private List<int> _maximusCounter = new List<int>();
        private float _max = 0;
        private float _lastTempo;


        internal delegate void BeatHandler(float delay, float tempo, BeatType type);
        internal delegate void TickHandler(byte[] spec);

        public event BeatHandler OnBeat;
        public event TickHandler OnSpectrum;
        
        public SoundAnalysis(string filename)
        {
            _clock = new Clock();
            _clock.Restart();
            _fft = new float[16384];
            _timer = new Timer();
            _timer.Elapsed+= OnTick;
            _timer.Interval = TimeSpan.FromMilliseconds(50).Milliseconds; 

            newSpectrumData = new byte[_lines];
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

            oldSpetrumData = new byte[1];
            
            _timer.Enabled = _timer.AutoReset = true;
            _timer.Start();
        }
        
        private void OnTick(object sender, EventArgs e)
        {
            float tick_time  = (float) Bass.ChannelBytes2Seconds(handle,Bass.ChannelGetPosition(handle, PositionFlags.Bytes));
          
            int ret = Bass.ChannelGetData(handle,_fft, (int)DataFlags.FFT16384);
            if (ret < -1) return;
            int x, y;
            int b0 = 0;
            float peak = 0;
            float peakSum = 0;
            float sum = 0;
            int count = 0;
            float tmpMax = 0;
            _max = 0;
            //FFT from BASS_WASAPI sample
            for (x = 0; x < _lines; x++)
            {
                peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (_lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (_max<y) _max = y;
                if (y > 199) count++;
                //if (min > y) min = y;*/
                
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                
                sum += y * y;
                peakSum += peak;
                newSpectrumData[x] = ((byte)y);
            }

            var type = BeatType.Regular;
            
            float tempo = (float)Math.Log10(Math.Sqrt(sum / 1024f));
            if (tempo < 0) tempo = _lastTempo;
            var diff = Math.Abs(tempo - _lastTempo);
            if (diff > 0 && diff < 0.01) type = BeatType.Hold; 
            _lastTempo = tempo;
            _max = Math.Clamp(Math.Abs(255-Math.Abs(tempo-peakSum)*255), 50,255);

            foreach (var b in newSpectrumData)
            {
                if (b > _max) count++;
            }

            
            float averagePeak = 0;
            int averageMax = 0;
            
            if (_maximusCounter.Count > 0)
                averageMax = _maximusCounter.Sum() / _maximusCounter.Count;

            if (_maximusCounter.Count > 0 && (averageMax < count))
            {
                type = BeatType.Shift;
                _maximusCounter.Clear();
               
            }
            else
            {
                _maximusCounter.Add(count);
            }

            
            
            float delay = _clock.ElapsedTime.AsSeconds();
            if (delay>0.25f && DetectBeat(ref beat, newSpectrumData,  oldSpetrumData))
            {
                Console.WriteLine($"Beat! delay: {tick_time}, tempo is {tempo}, type is {type.ToString()} - average max {averageMax} count {count} max value {_max} average peak {averagePeak}  peak {peakSum}");
                OnBeat?.Invoke(tick_time, tempo, type);
                _clock.Restart();
            }

            oldSpetrumData = newSpectrumData;
            newSpectrumData = new byte[_lines];
            
            OnSpectrum?.Invoke(newSpectrumData);
        }

        private bool DetectBeat(ref float beat,byte[] specNew, byte[] specOld)
        {
            float tmp_beat = 0;
            
            for (int i = 0; i < specOld.Length; i++)
            {
               // if (specNew[i]>30 && specOld[i]>30)
                    tmp_beat += (specNew[i] - specOld[i]);
            }
            
            tmp_beat /= specOld.Length;
            
            if (tmp_beat>0.5f && tmp_beat > beat)
            {
                beat = tmp_beat;
                //Console.WriteLine($"Beat! Difference is {beat}");
                return true;
            }

            beat = tmp_beat/1.1f;
            return false;
        }
        
        public void Free()
        {
            Bass.StreamFree(handle);
            Bass.Free();
        }
        
    }

   class BeatMap
   {
       private List<Beat> _beats;
       public BeatMap()
       {
           _beats = new List<Beat>(0);
           _beats.Add(new Beat(){BeatType = 0, Tempo =  0, Delay =  0});
       }

       public new void Add(Beat beat)
       {
           
           
           if (Math.Abs(beat.Delay - _beats.Last().Delay) < 0.15f)
           {
               _beats.Remove(_beats.Last());
           }
           _beats.Add(beat);
       }

       public List<Beat> GetArray() => _beats;
   }
   
   public enum BeatType
   {
       Regular,
       Hold,
       Shift
   }; 
   struct Beat
   {
       public float Delay { get; set; }
       public float Tempo { get; set; }
       public BeatType BeatType { get; set; }
   }
   
}