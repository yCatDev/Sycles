using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SyclesInternals.Gameplay
{
    public class PlayerController : MonoBehaviour
    {

        public float rotateSpeed = 5f;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private Vector3 up;
        private float _delta;

        private Vector3 _centre;
        public float angle = -1;
        public int direction = 1;
        
        // Start is called before the first frame update
        private void Start()
        {
            _centre = transform.position;
        }

        // Update is called once per frame
        private void Update()
        {
            this.angle += rotateSpeed * _delta;

            transform.position = InternalMath.SetPositionCircular(this.angle, radius);


            var targetPos = _centre;
            targetPos.x = targetPos.x - transform.position.x;
            targetPos.y = targetPos.y - transform.position.y;
            var angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        public Vector3 GetPosition() => transform.position;
        public void SetDelta(float delta) => _delta = delta;

    }
}