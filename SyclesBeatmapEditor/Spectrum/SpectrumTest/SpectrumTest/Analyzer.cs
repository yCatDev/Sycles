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
   class Analyzer
    {
        
        private readonly float[] _fft;               //buffer for fft data
        //private int _lastlevel;             //last output level
        //private int _hanctr;                //last output level counter
        private List<byte> _spectrumdata;   //spectrum data buffer
        private bool _initialized;          //initialized flag
        private int devindex;               //used device index
        
        private int _lines = 64;            // number of spectrum lines
        private Timer _t;
        private List<string> _devicelist;

        private int handle;

        public byte[] spetrumData;
        
        //ctor
        public Analyzer(string filename)
        {
            _fft = new float[8192];
            _t = new Timer();
            _t.Elapsed+= _t_Tick;
            _t.Interval = TimeSpan.FromMilliseconds(25).Milliseconds; //40hz refresh rate//25

            _spectrumdata = new List<byte>();
            _initialized = false;

            
            Init(filename);
        }

        // Serial port for arduino output


        // flag for display enable
        public bool DisplayEnable { get; set; }

        //flag for enabling and disabling program functionality
       
        // initialization
        private void Init(string filename)
        { 
            
            
            bool result = Bass.Init();
            if (!result) throw new Exception("Init Error");
            
            handle = Bass.CreateStream(filename);
            Bass.ChannelPlay(handle);
            Bass.SuppressMP3ErrorCorruptionSilence = true;
            Console.WriteLine(Bass.LastError);

            spetrumData = new byte[1];
            
            _t.Enabled = _t.AutoReset = true;
            _t.Start();
        }

        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {
            
            int ret = Bass.ChannelGetData(handle,_fft, (int)DataFlags.FFT8192);  //get ch.annel fft data
            Console.WriteLine(Bass.LastError);
            
            if (ret < -1) return;
            int x, y;
            int b0 = 0;

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
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
                //Console.SetCursorPosition(0,0);
                //Console.WriteLine(y);
                //Console.Write("{0, 3} ", y);
            }

            spetrumData = _spectrumdata.ToArray();
            _spectrumdata.Clear();

        }
        

        //cleanup
        public void Free()
        {
            Bass.StreamFree(handle);
            Bass.Free();
        }
    }
}