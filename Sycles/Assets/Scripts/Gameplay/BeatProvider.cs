using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace SyclesInternals.Gameplay
{
    public class BeatProvider : MonoBehaviour
    {

        [SerializeField] private TextAsset beatmapFile; 
        [SerializeField] private PlayerController player;

        [SerializeField] private GameObject usualBeatDot, shiftBeatDot, holdBeatDot;

        private float _startPlayerSpeed;

        [SerializeField] private Circle circle;

        private float _lastStep = 0;
        
        private List<Beat> _beatmap = new List<Beat>();
        private int _buildIndex = 0;
        private Queue<BeatInfo> _dots = new Queue<BeatInfo>();
        private int _direction = 1;
        private bool _locked = false;
        private AudioSource _music;
        //private BassAudio _music;
        private AudioDeltaCalculator _adc;
        
        private float _rmsValue;
        private float[] _samples = new float[1024];
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;

        private bool _start = false, _wasInRange = false;
        private float _missCount, _hitCount, _fpsCount, _c, _lastDelay = -1;
        private int _completeness;
        [SerializeField] private UnityEngine.UI.Text fpsText, scoreText;
        private Metaball2D playerBall;
        [SerializeField] private AudioClip hit;

        // Start is called before the first frame update
        void Start()
        {
            DOTween.Init();
          
            _beatmap = JsonConvert.DeserializeObject<List<Beat>>(beatmapFile.text);
            CleanUp(ref _beatmap);
            
            _c = 1f / _beatmap.Count;
            //_music = GetComponent<BassAudio>();
            _music = GetComponent<AudioSource>();
            _adc = GetComponent<AudioDeltaCalculator>();

            
            playerBall = player.GetComponent<Metaball2D>();
            
            
        }

        IEnumerator Begin()
        {
           
            
            yield return null;
           
            _start = true;
            _music.Play();
        }
        
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && !_start)
            {
                StartCoroutine(Begin());
            }
            if (!_start) return;
            if ( _dots.Count < 3 && !_locked)
            {
                _dots.Enqueue(CreateBeatDot());
            }
            else
            {
                    var dot = _dots.Peek();
                    player.SetMovement(dot.Angle, dot.Time);
                    //Debug.Log($"speed {player.rotateSpeed} = {dot.Speed}*{_direction}");
                    var ball = dot.GameObject.GetComponent<Metaball2D>();
                    //
                    var checking = CheckDot(dot, 0.15f);
//                    Debug.Log($"{checking} {dot.GameObject.name} {dot.Angle} {player.angle}");
                    if (CheckDot(dot, 0.1f))
                    {
                        
                        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                        {
                            ball.SetColor(Color.white);
                            _hitCount++;
                            Click(dot);
                        }else    _wasInRange = true; 
                    }
                    else
                    {
                        
                        if (_wasInRange)
                        {
                            //ball.SetColor(Color.white);
                            _missCount++;
                            Click(dot);
                        }else
                        {
                            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) _missCount++;
                        }
                    }
            }
            
//            Debug.Log($"Speed {player.rotateSpeed} {_tempo} {_direction}");
            _fpsCount = Mathf.Lerp(_fpsCount, 1f / Time.unscaledDeltaTime, Time.fixedDeltaTime);
            fpsText.text = $"FPS {(int) (_fpsCount)}";
            
            scoreText.text = $"Hits {_hitCount}, misses {_missCount} \n Accuracy {Math.Floor(_c*(_hitCount-_missCount)*100)}%";
        }

        private bool CheckDot(BeatInfo dot, float offset)
        {
//            Debug.Log($"{dot.Angle-player.angle}");
            if (_direction > 0)
            {
                if (dot.Angle-player.angle>offset)
                    return true;
                else return false;
            }
            else
            {
                if (player.angle - dot.Angle>offset)
                    return true;
                else return false;
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
        

        private void Click(BeatInfo obj)
        {
            var dot = obj.GameObject.GetComponent<BeatDotController>();
            dot.Hide();
            float diff = 0;
            //if (_music.GetTimeInSeconds() > obj.Delay) ;
            //    diff = _music.GetTimeInSeconds() - obj.Delay;
            if (_music.time > obj.Delay) ;
                diff = _music.time - obj.Delay;
            Debug.Log($"Current track time {_music.time}, current beat time {obj.Delay} ADC delta {_adc.GetDeltaTime()}");
            //Debug.Log($"Current track time {_music.GetTimeInSeconds()}, current beat time {obj.Delay} ADC delta {_adc.GetDeltaTime()}");
            if (dot.GetType() == BeatType.Shift)
            {
                ChangeDirection();
            }

            _dots.Dequeue();
            _dots.Peek().Time -= diff;
            player.ResetDelta();
            _wasInRange = false;
        }
        
        
        private void LateUpdate()
        {
            /*_tempo = CalculateMusicTempo();
            if (float.IsInfinity(_tempo) || float.IsNaN(_tempo))
                _tempo = 0;*/
            //player.rotateSpeed = Mathf.Lerp(player.rotateSpeed,_startPlayerSpeed+_tempo,Time.fixedDeltaTime);
        }
        

        private BeatInfo CreateBeatDot()
        {
            BeatInfo info = new BeatInfo();
            GameObject dot;
            switch (_beatmap[_buildIndex].BeatType)
            {
                case BeatType.Regular:
                case BeatType.Hold:
                default:
                    dot = Instantiate(usualBeatDot);
                    break;
                case BeatType.Shift:
                    dot = Instantiate(shiftBeatDot);
                    //info.Direction = _buildDirection*-1;
                    _locked = true;
                    break;
            }

           // if (_lastDelay < 0) _lastDelay = _music.GetTimeInSeconds(); 
            if (_lastDelay < 0) _lastDelay = _music.time; 
            var t = Mathf.Abs(_beatmap[_buildIndex].Delay-_lastDelay);
            var step =/* _beatmap[_buildIndex].Tempo**/t;

            _lastDelay = _beatmap[_buildIndex].Delay;
            //Debug.Log($"#{_buildIndex} {t}  {anglePerSecond} {_beatmap[_buildIndex].Tempo} {_direction} {_currentTempo}");
            //Debug.Log($"Time: {t}, AnglePerSecond: {anglePerSecond}, Step: {step}, n: {1/t}");
//            Debug.Log($"#{_buildIndex} Creating beat dot, calculated time {t}, time now {_music.GetTimeInSeconds()}, needed time {_beatmap[_buildIndex].Delay}, step {step} unscaleddelta {Time.unscaledDeltaTime} fixeddelta {Time.fixedDeltaTime} usual delta {Time.deltaTime} adc {_adc.GetDeltaTime()}");
            _lastStep += step*_direction;    
            dot.transform.position = InternalMath.SetPositionCircular(_lastStep, circle.xradius);
            dot.GetComponent<BeatDotController>().Setup(this, _beatmap[_buildIndex].BeatType);
            

            info.Index = _buildIndex;
            info.Angle = _lastStep;
            info.Delay = _beatmap[_buildIndex].Delay;
            //info.Speed = Time.deltaTime/t;
            info.Time = t;
            info.GameObject = dot;
            
            _buildIndex++;
            return info;
        }

        
        
        public void ChangeDirection()
        {
            _direction *= -1;
            _locked = false;
            Debug.Log($"Change direction {_direction}");
            //player.rotateSpeed = _beatmap[_currentIndex].Tempo * _direction;
        }
    }
}