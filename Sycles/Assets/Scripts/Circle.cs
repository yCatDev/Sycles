using System;

namespace SyclesInternals
{
    using UnityEngine;
    using System.Collections;
 
    [ExecuteInEditMode]
    public class Circle : MonoBehaviour
    {
        public int segments;
        public float xradius;
        public float yradius;
        LineRenderer line;
       
        void Start ()
        {
            Setup();
        }


        private void OnValidate()
        {
            Setup();
        }

        private void Setup()
        {
            line = gameObject.GetComponent<LineRenderer>();
       
            line.SetVertexCount (segments + 1);
            line.useWorldSpace = false;
            CreatePoints ();
        }
        
        void CreatePoints ()
        {
            float x;
            float y;
            float z = 0f;
       
            float angle = 20f;
       
            for (int i = 0; i < (segments + 1); i++)
            {
                x = Mathf.Sin (Mathf.Deg2Rad * angle) * xradius;
                y = Mathf.Cos (Mathf.Deg2Rad * angle) * yradius;
                   
                line.SetPosition (i,new Vector3(x,y,z) );
                   
                angle += (360f / segments);
            }
        }
    }
}