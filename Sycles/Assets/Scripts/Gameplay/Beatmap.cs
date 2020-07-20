using System.Collections.Generic;
using Newtonsoft.Json;

namespace SyclesInternals.Gameplay
{
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