using DG.Tweening;
using UnityEngine;

namespace SyclesInternals.BeatmapEditor
{
    public class EditorPointerController : MonoBehaviour
    {
        [SerializeField] private float targetX = 5f;
        [SerializeField] private float targetTime = 5f;


        public float currentX = -1;

        private bool _locked = false;
        // Start is called before the first frame update
        private void Start()
        {
            DOTween.Init();
            DOTween.useSmoothDeltaTime = true;
            DOTween.defaultEaseType = Ease.Linear;
            
            SetMovement(0,0);
            //_delta = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            var pos = transform.position;
            pos.x = currentX;
            transform.position = pos;
        }

        public Vector3 GetPosition() => transform.position;

        public void SetMovement(float angle, float time)
        {
            targetX = angle;
            targetTime = time;

            if (!_locked)
            {
                DOTween.To(() => this.currentX, (a) => this.currentX = a, targetX, targetTime)
                    .onComplete+=() => _locked = false;
                _locked = true;
            }
        }
        
    }
}