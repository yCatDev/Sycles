using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


namespace SyclesInternals.Gameplay
{

    public class BeatDotController : MonoBehaviour
    {

        private Shape _shape;
        private BeatType _beatType;
        private BeatProvider _bp;
        public Vector3 startScale, startPos;

        void Awake()
        {
            transform.localScale = Vector3.zero;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            
            _shape = GetComponent<Shape>();
           
            //StartCoroutine(Fadein());
        }
        
        // Update is called once per frame
        void Update()
        {

        }

        public BeatType GetType() => _beatType;
        
        public void Setup(BeatProvider instance, BeatType type)
        {
            _bp = instance;
            _beatType = type;
            //startPos = transform.position;
           //transform.position = Vector3.zero;
            //transform.DOMove(startPos, 0.25f);
            transform.DOScale(startScale, 0.25f);
        }
        
        public void Hide()
        {
            //StopAllCoroutines();
            //StartCoroutine(Fadeout());
            transform.DOScale(Vector3.zero, 0.5f).onComplete+=()=>Destroy(this.gameObject);
            var r = Color.gray.r;
            var g = Color.gray.g;
            var b = Color.gray.b;
            DOTween.To(() => _shape.colour.r, (x) => _shape.colour.r = x, r, 0.5f);
            DOTween.To(() => _shape.colour.g, (x) => _shape.colour.g = x, g, 0.5f);
            DOTween.To(() => _shape.colour.b, (x) => _shape.colour.b = x, b, 0.5f);
        }

        private IEnumerator Fadeout()
        {
            while (transform.localScale != Vector3.zero)
            {
                transform.localScale =
                    Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.fixedDeltaTime / 2);
                _shape.colour = Color.Lerp(_shape.colour, Color.gray, Time.fixedDeltaTime * 10);
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            Destroy(this.gameObject);
        }
        
        private IEnumerator Fadein()
        {
            while (transform.localScale != startScale)
            {
                transform.localScale =
                    Vector3.MoveTowards(transform.localScale, startScale, Time.fixedDeltaTime);
               // _shape.colour = Color.Lerp(_shape.colour, Color.gray, Time.fixedDeltaTime * 10);
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            //Destroy(this.gameObject);
        }

    }
}
