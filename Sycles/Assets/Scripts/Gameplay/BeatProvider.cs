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
        private int _buildIndex = 0, _currentIndex = 0;
        private Queue<BeatInfo> _dots = new Queue<BeatInfo>();
        private int _direction = 1;
        private bool _locked = false;
        private AudioSource _music;
        private AudioDeltaCalculator _adc;

        private float _tempo, _currentTempo;
        private float _rmsValue;
        private float[] _samples = new float[1024];
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;

        private bool _start = false, _wasInRange = false;
        private float _missCount, _hitCount, _fpsCount, _c;
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
            _music = GetComponent<AudioSource>();
            _adc = GetComponent<AudioDeltaCalculator>();

            _startPlayerSpeed = player.rotateSpeed;
            player.rotateSpeed = 0;
            playerBall = player.GetComponent<Metaball2D>();
            
            SetTempo();
        }

        IEnumerator Begin()
        {
           
            /*player.rotateSpeed =*/ _currentTempo = _beatmap[0].Tempo;
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
                /*if (Input.anyKey)
                {*/
                    
                    var dot = _dots.Peek();
                    player.rotateSpeed = dot.Speed*_direction;
                    var ball = dot.GameObject.GetComponent<Metaball2D>();
                    if (Vector2.Distance(player.GetPosition(), dot.GameObject.transform.position) < 0.55f)
                    {
                        
                        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                        {

                            ball.SetColor(Color.white);
                            _hitCount++;
                            //_music.PlayOneShot(hit);
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
                    
                /*}*/
            }
            player.SetDelta(Time.fixedDeltaTime);
            /*player.rotateSpeed =*/ _currentTempo = Mathf.Lerp(player.rotateSpeed, _tempo,Time.fixedDeltaTime);
//            Debug.Log($"Speed {player.rotateSpeed} {_tempo} {_direction}");
            _fpsCount = Mathf.Lerp(_fpsCount, 1f / Time.unscaledDeltaTime, Time.fixedDeltaTime);
            fpsText.text = $"FPS {(int) (_fpsCount)}";
            
            scoreText.text = $"Hits {_hitCount}, misses {_missCount} \n Accuracy {Math.Floor(_c*(_hitCount-_missCount)*100)}%";
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
            Debug.Log($"Current track time {_music.time}, current beat time {obj.Delay} ADC delta {_adc.GetDeltaTime()}");
            if (dot.GetType() == BeatType.Shift)
            {
                ChangeDirection();
                _locked = false;
            }

            _dots.Dequeue();
            SetTempo();
            _wasInRange = false;
        }
        
        private void SetTempo()
        {
            _tempo = _beatmap[_currentIndex].Tempo*_direction;
            _currentIndex++;
        }
        
        private void LateUpdate()
        {
            /*_tempo = CalculateMusicTempo();
            if (float.IsInfinity(_tempo) || float.IsNaN(_tempo))
                _tempo = 0;*/
            //player.rotateSpeed = Mathf.Lerp(player.rotateSpeed,_startPlayerSpeed+_tempo,Time.fixedDeltaTime);
        }

        private float CalculateMusicTempo()
        {
            _music.GetOutputData(_samples, 0); // fill array with samples
            float sum = 0;
            for (int i = 0; i < QSamples; i++)
            {
                sum += _samples[i] * _samples[i]; // sum squared samples
            }
            _rmsValue = Mathf.Sqrt(sum / QSamples);
            return ((Mathf.Abs(Mathf.Log10(_rmsValue / RefValue)))+_rmsValue);
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
                    _locked = true;
                    break;
            }

            var t = Mathf.Abs(_beatmap[_buildIndex].Delay-_music.time);
            var anglePerSecond = _beatmap[_buildIndex].Tempo * Time.fixedDeltaTime;
            var step = InternalMath.ArithmeticProgression(_beatmap[_buildIndex].Tempo, anglePerSecond, t)*_direction;


            //Debug.Log($"#{_buildIndex} {t}  {anglePerSecond} {_beatmap[_buildIndex].Tempo} {_direction} {_currentTempo}");
            //Debug.Log($"Time: {t}, AnglePerSecond: {anglePerSecond}, Step: {step}, n: {1/t}");
            Debug.Log($"#{_buildIndex} Creating beat dot, calculated time {t}, time now {_music.time}, needed time {_beatmap[_buildIndex].Delay}, angle {anglePerSecond}, step {step} unscaleddelta {Time.unscaledDeltaTime} fixeddelta {Time.fixedDeltaTime} usual delta {Time.deltaTime} current tempo {_currentTempo} adc {_adc.GetDeltaTime()}");
            _lastStep += step;    
           dot.transform.position = InternalMath.SetPositionCircular(player.angle+step, circle.xradius);
           dot.GetComponent<BeatDotController>().Setup(this, _beatmap[_buildIndex].BeatType);
            _buildIndex++;

            info.Angle = step;
            info.Delay = _beatmap[_buildIndex].Delay;
            info.Speed = step / t;
            info.Time = t;
            info.GameObject = dot;
            
            return info;
        }

        
        
        public void ChangeDirection()
        {
            player.direction = _direction *= -1;
            //player.rotateSpeed = _beatmap[_currentIndex].Tempo * _direction;
        }
    }
}