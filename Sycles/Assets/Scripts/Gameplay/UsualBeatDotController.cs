using System;
using System.Collections;
using UnityEngine;

namespace SyclesInternals.Gameplay
{
    public class UsualBeatDotController : MonoBehaviour
    {

        private Transform _target;
        private Shape _shape;
        
        private void Start()
        {
            
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _shape = GetComponent<Shape>();
            StartCoroutine(Move());
        }

        private IEnumerator Move()
        {
            
            
            while (transform.localScale!=Vector3.zero)
            {
                transform.localScale =
                    Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.fixedDeltaTime / 2);
                _shape.colour = Color.Lerp(_shape.colour, Color.gray, Time.fixedDeltaTime * 10);    
                yield return null;
            }
            yield return new WaitForEndOfFrame();
            Destroy(this.gameObject);
        }
        
    }
}