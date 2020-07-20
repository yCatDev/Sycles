using System.Collections.Generic;
using UnityEngine;

namespace SyclesInternals.Gameplay
{
    public class AudioDeltaCalculator: MonoBehaviour
    {
        float timeLastFrame;
        List<float> audioDeltaTimes = new List<float>();
        float smoothAudioDeltaTime;
        AudioSource source;
 
// Adjust this number until things feel right
        int framesToSmooth = 8;

        void Start()
        {
            source = GetComponent<AudioSource>();
        }
        
        void LateUpdate()
        {
            // Add the deltaTime this frame to a list
            float deltaThisFrame = source.time - timeLastFrame;
            audioDeltaTimes.Add(deltaThisFrame);
            timeLastFrame = source.time;
 
            // If the list is too large, remove the oldest value
            if (audioDeltaTimes.Count > framesToSmooth)
            {
                audioDeltaTimes.RemoveAt(0);
            }
 
            // Get the average of all values in the list
            float average = 0;
            foreach (float delta in audioDeltaTimes)
            {
                average += delta;
            }
            smoothAudioDeltaTime = average / audioDeltaTimes.Count;
        }

        public float GetDeltaTime() => smoothAudioDeltaTime > 0 ? smoothAudioDeltaTime : Time.unscaledDeltaTime;

    }
}