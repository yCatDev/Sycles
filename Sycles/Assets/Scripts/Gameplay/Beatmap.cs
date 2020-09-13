using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SyclesInternals.Gameplay
{

    public class BeatInfo
    {
        public int Index;
        public GameObject GameObject;
        public float Delay;
        public float Time;
        public float Tempo;
        public float Angle;
        public int Direction;
        public BeatType Type;
    }
    
   public enum BeatType
      {
          Regular,
          Hold,
          Shift
      }; 
    
    internal struct Beat
       {
           public float Delay { get; set; }
           public float Tempo { get; set; }
           public BeatType BeatType { get; set; }
       }
    
    public class Beatmap
    {
        public string json
        {
            get => "";
            set
            {
                beats = JsonConvert.DeserializeObject<List<Beat>>(value);
            }
        }    
        
        private List<Beat> beats = new List<Beat>();
    }
}