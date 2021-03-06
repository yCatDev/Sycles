﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace SyclesInternals.Gameplay
{
    public class BeatProvider : MonoBehaviour
    {
        [SerializeField] private Camera camera;
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
                    var c = CheckDot(dot, 0.0f);
                    if (c)
                    {
                        _wasInRange = true;
                        if (InputHelper.RegisteredKeyWasPressed())
                        {
                            switch (dot.Type)
                            {
                                case BeatType.Hold:
                                case BeatType.Regular:
                                default:
                                    Click(dot, InputHelper.IsAPressed);
                                    break;
                                case BeatType.Shift:
                                    Click(dot, InputHelper.IsBPressed);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (_wasInRange)
                        {
                            _missCount++;
                            HideDot(dot.GameObject.GetComponent<BeatDotController>(), dot.Delay);
                        }else
                        {
                            if (InputHelper.RegisteredKeyWasPressed()) _missCount++;
                        }
                    }

                   
                    var look = camera.GetComponent<LookAtConstraint>();
                    if (!float.IsInfinity(CalculateMusicTempo()))
                    {
                        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, 5 + CalculateMusicTempo(),
                            _adc.GetDeltaTime());
                        look.roll = Mathf.Lerp(look.roll, 
                            CalculateMusicTempo()*randDir()*5,_adc.GetDeltaTime());
                    }
            }
            
//            Debug.Log($"Speed {player.rotateSpeed} {_tempo} {_direction}");
            _fpsCount = Mathf.Lerp(_fpsCount, 1f / Time.unscaledDeltaTime, Time.fixedDeltaTime);
            fpsText.text = $"FPS {(int) (_fpsCount)}";
            scoreText.text = $"Hits {_hitCount}, misses {_missCount} \n Accuracy {Math.Floor(_c*(_hitCount-_missCount)*100)}%";
        }

        private int randDir()
        {
            var r = Random.Range(0, 1);
            return r == 0 ? 1 : -1;
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


        private bool CheckDot(BeatInfo dot, float offset)
        {
//            Debug.Log($"{Vector2.Distance(player.transform.position, dot.GameObject.transform.position)}");
            if (_direction > 0)
            {
                if ((dot.Angle)-player.angle>offset)
                    return true;
                else return false;
            }
            else
            {
                if (player.angle - (dot.Angle)>offset)
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
        

        private void Click(BeatInfo obj, Func<bool> checker)
        {
            var dot = obj.GameObject.GetComponent<BeatDotController>();
            bool hit = checker() && Vector2.Distance(player.transform.position, obj.GameObject.transform.position)<2f;
            Debug.Log($"{Vector2.Distance(player.transform.position, obj.GameObject.transform.position)}");
            if (!hit)
            {
                _missCount++;
                return;
            }
            dot.GetComponent<Metaball2D>().SetColor(Color.white);
            _hitCount++;
            
            HideDot(dot, obj.Delay);
        }

        private void HideDot(BeatDotController dot, float delay)
        {
            dot.Hide();
            
            Debug.Log($"Current track time {_music.time}, current beat time {delay}");
            if (dot.GetType() == BeatType.Shift)
                ChangeDirection();

            _dots.Dequeue();
            _dots.Peek().Time -= CalculateDiff(delay);
            
            _wasInRange = false;
        }
        
        private float CalculateDiff(float delay)
        {
            if (_music.time > delay)
                return _music.time - delay;
            return 0;
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
            var step = t*_beatmap[_buildIndex].Tempo/*+/* _beatmap[_buildIndex].Tempo*#1#
                InternalMath.GeomProgression(0.25f, _beatmap[_buildIndex].Tempo, t)*/;

            _lastDelay = _beatmap[_buildIndex].Delay;
            //Debug.Log($"#{_buildIndex} {t}  {anglePerSecond} {_beatmap[_buildIndex].Tempo} {_direction} {_currentTempo}");
            //Debug.Log($"Time: {t}, AnglePerSecond: {anglePerSecond}, Step: {step}, n: {1/t}");
//            Debug.Log($"#{_buildIndex} Creating beat dot, calculated time {t}, time now {_music.GetTimeInSeconds()}, needed time {_beatmap[_buildIndex].Delay}, step {step} unscaleddelta {Time.unscaledDeltaTime} fixeddelta {Time.fixedDeltaTime} usual delta {Time.deltaTime} adc {_adc.GetDeltaTime()}");
            _lastStep += step*_direction;    
            dot.transform.position = InternalMath.SetPositionCircular(_lastStep, circle.xradius);
            dot.GetComponent<BeatDotController>().Setup(this, _beatmap[_buildIndex].BeatType);
            

            info.Index = _buildIndex;
            info.Angle = _lastStep;
            info.Tempo = _beatmap[_buildIndex].Tempo;
            info.Delay = _beatmap[_buildIndex].Delay;
            info.Type = _beatmap[_buildIndex].BeatType;
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