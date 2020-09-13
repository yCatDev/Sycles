using System;
using UnityEngine;

namespace SyclesInternals
{
    public static class InternalMath
    {
        public static Vector3 SetPositionCircular(float angle, float radius, Vector3 center)
        {
            var offset = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
            
            return center + offset;
        }
        
        public static Vector3 SetPositionCircular(float angle, float radius)
            => SetPositionCircular(angle, radius, Vector3.zero);
        
        public static float GeomProgression(float a, float q, float n) 
        {
            var b = (float) Math.Pow(q, n);
            return a * b;
        }

        public static float ArithmeticProgression(float a, float d, float n)
        {
            var b = 2f * a + (n - 1) * d;
            return (b / 2f) * n;
        }
        
    }
}