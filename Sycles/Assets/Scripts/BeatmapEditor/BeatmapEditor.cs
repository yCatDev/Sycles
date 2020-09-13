using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SyclesInternals.Gameplay;
using UnityEngine;

namespace SyclesInternals.BeatmapEditor
{
    
    public class BeatmapEditor : MonoBehaviour
    {
        [SerializeField] private TextAsset beatmap;
        [SerializeField] private EditorPointerController pointer;

        [SerializeField] private GameObject editorUsualDot, editorShiftDot, editorHoldDot;
        
        private AudioSource _music;
        
        private List<Beat> _beatmap = new List<Beat>();
        private List<EditorDotData> _dots = new List<EditorDotData>();
        private float _lastDelay;
        private float _lastStep;
        private int _index;
        private bool _wasInRange;

        // Start is called before the first frame update
        void Start()
        {
            _music = GetComponent<AudioSource>();
            
            _beatmap = JsonConvert.DeserializeObject<List<Beat>>(beatmap.text);
            CleanUp(ref _beatmap);

            foreach (var beat in _beatmap)
            {
                _dots.Add(CreateBeatDot(beat));
            }
            
            _music.Play();
            
            pointer.SetMovement(0,0);
        }
        
        private bool CheckDot(EditorDotData dot, float offset)
        {
            if (dot.targetX-pointer.currentX>offset)
                return true;
            else return false;
            
        }
        
        // Update is called once per frame
        void Update()
        {
            var dot = _dots[_index];
            pointer.SetMovement(dot.targetX, dot.targetTime);
            
            if (CheckDot(dot, 0.1f))
            {
                _wasInRange = true; 
            }
            else
            {
                if (_wasInRange)
                {
                    
                    var obj = _dots[_index];
                    
                    float diff = 0;
                    if (_music.time > obj.delay) ;
                    diff = _music.time - obj.delay;

                    _dots.RemoveAt(_index);
                    _dots[_index].targetTime -= diff;
                    _wasInRange = false;
                    _index++;
                }
            }
        }
        private void CleanUp(ref List<Beat> map)
        {
            for (int i = 0; i < map.Count; i++)
            {
                if (map[i].Tempo <= 0 || map[i].Delay <= 0)
                {
                    map.RemoveAt(i);
                    i--;
                }
            }
        }
        
        private EditorDotData CreateBeatDot(Beat beat)
        {
            GameObject dot;
            switch (beat.BeatType)
            {
                case BeatType.Regular:
                case BeatType.Hold:
                default:
                    dot = Instantiate(editorUsualDot);
                    break;
                case BeatType.Shift:
                    dot = Instantiate(editorShiftDot);
                    break;
            }

           // if (_lastDelay < 0) _lastDelay = _music.GetTimeInSeconds(); 
            if (_lastDelay < 0) _lastDelay = _music.time; 
            var t = Mathf.Abs(beat.Delay-_lastDelay);
            var step = t+InternalMath.GeomProgression(0.25f, beat.Tempo, t);

            _lastDelay = beat.Delay;
            _lastStep += step;
            var pos = dot.transform.position;
            pos.x = _lastStep;
            dot.transform.position = pos;

            var data = dot.AddComponent<EditorDotData>();
            data.targetTime = t;
            data.targetX = _lastStep;
            data.delay = beat.Delay;
            
            return data;
        }
    }
    
    
}