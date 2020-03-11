using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;
using SFML.System;
using SFML.Audio;
using ManagedBass;
//using static ManagedBass.Bass;
using ManagedBass.Wasapi;
using Utils = Microsoft.VisualBasic.CompilerServices.Utils;

namespace PartialMusicAnalyzer
{
   class Analyzer
    {
        private bool _enable;               //enabled status
        //private DispatcherTimer _t;         //timer that refreshes the display
        public readonly float[] _fft;               //buffer for fft data
        //private ProgressBar _l, _r;         //progressbars for left and right channel intensity
        //private WASAPIPROC _process;        //callback function to obtain data
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter
        public List<byte> _spectrumdata;   //spectrum data buffer
        //private Spectrum _spectrum;         //spectrum dispay control
        //private ComboBox _devicelist;       //device list
        private bool _initialized;          //initialized flag
        private int devindex;               //used device index
        //private Chart _chart;

        private int _lines = 64;            // number of spectrum lines
        private Timer _t;
        private List<string> _devicelist;

        //ctor
        public Analyzer()
        {
            _fft = new float[8192];
            _lastlevel = 0;
            _hanctr = 0; 
            _t = new Timer();
            _t.Elapsed+= _t_Tick;
            _t.Interval = TimeSpan.FromMilliseconds(25).Milliseconds; //40hz refresh rate//25
            _t.Enabled = false;
            
            _spectrumdata = new List<byte>();
            _initialized = false;

            
            Init();
        }

        // Serial port for arduino output


        // flag for display enable
        public bool DisplayEnable { get; set; }

        //flag for enabling and disabling program functionality
        public bool Enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
                if (value)
                {
                    if (!_initialized)
                    {
                        bool result = false;
                        for (int i = 0; i < BassWasapi.DeviceCount; i++)
                        {
                            var device = BassWasapi.GetDeviceInfo(i);
                            if (device.IsEnabled && device.IsLoopback)
                            {
                              result = BassWasapi.Init(i, 0, 0, WasapiInitFlags.Buffer, 1f, 0.5f, Process, IntPtr.Zero);
                              break;
                            }
                        }
                        if (!result)
                        {
                            var error = Bass.LastError;
                            
                        }
                        else
                        {
                            _initialized = true;
                            
                        }
                    }
                    BassWasapi.Start();
                    _t.AutoReset = true;
                    _t.Start();
                    
                }
                else BassWasapi.Stop(true);
                System.Threading.Thread.Sleep(500);
                _t.Enabled = value;
            }
        }

        // initialization
        private void Init()
        {
            for (int i = 0; i < BassWasapi.DeviceCount; i++)
            {
                var device = BassWasapi.GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    Console.WriteLine(string.Format("{0} - {1}", i, device.Name));
                }
            }
            bool result = false;
            Bass.Configure(Configuration.UpdateThreads, false);
            result = Bass.Init(0, 44100, DeviceInitFlags.Default, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }

        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {
            
            int ret = BassWasapi.GetData(_fft, (int)DataFlags.FFT8192);  //get ch.annel fft data
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
                Console.SetCursorPosition(0,0);
                Console.WriteLine(y);
                //Console.Write("{0, 3} ", y);
            }
            
            _spectrumdata.Clear();


            int level = BassWasapi.GetLevel();
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            if (_hanctr > 3)
            {
                _hanctr = 0;
                Free();
                Bass.Init(0, 44100, DeviceInitFlags.Default, IntPtr.Zero);
                _initialized = false;
                Enable = true;
            }

            
        }

        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public void Free()
        {
            BassWasapi.Free();
            Bass.Free();
        }
    }
}