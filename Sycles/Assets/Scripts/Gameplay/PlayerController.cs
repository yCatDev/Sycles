using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SyclesInternals.Gameplay
{
    public class PlayerController : MonoBehaviour
    {

        [SerializeField] private float targetAngle = 5f;
        [SerializeField] private float targetTime = 5f;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private Vector3 up;
        [SerializeField] private AudioDeltaCalculator adc;
        private float _delta;

        private Vector3 _centre;
        public float angle = -1;
        public int direction = 1;
        private bool _locked = false;
        private float elapsed;
        
        // Start is called before the first frame update
        private void Start()
        {
            DOTween.Init();
            DOTween.defaultEaseType = Ease.Linear;
            DOTween.defaultUpdateType = UpdateType.Fixed;
            
            _centre = transform.position;
            SetMovement(0,0);
            //_delta = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            
            if (elapsed < targetTime )
            {
                angle = Mathf.Lerp( this.angle, targetAngle, elapsed / targetTime );
                elapsed += Time.deltaTime;
            }
            else
            {
                _locked = false;
            }
            
            transform.position = InternalMath.SetPositionCircular(this.angle, radius);
        }

      
        public Vector3 GetPosition() => transform.position;

        public void SetMovement(float angle, float time)
        {
            targetAngle = angle;
            targetTime = time;
            if (!_locked)
            {
                elapsed = 0;
                _locked = true;
            }

            /*if (!_loked)
            {
                DOTween.To(() => this.angle, (a) => this.angle = a, targetAngle, targetTime)
                    .onComplete+=() => _loked = false;
                _loked = true;
            }*/
        }
        
        
    }
}