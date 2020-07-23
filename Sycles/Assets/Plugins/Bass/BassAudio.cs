using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManagedBass;
using UnityEditor;

public class BassAudio : MonoBehaviour
{

    public AudioClip AudioFile;

    private int handle;
    private string filename;
    
    // Start is called before the first frame update
    void Start()
    {
        Bass.Init();
        filename = Application.streamingAssetsPath+'/'+AssetDatabase.GetAssetPath(AudioFile);
        filename = filename.Replace("/StreamingAssets/Assets/", "/");
        handle = Bass.CreateStream(filename);
        Debug.Log($"{Bass.LastError}");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*var err = Bass.LastError; 
        if (err!=Errors.OK)
            Debug.Log(err);*/
    }

    public void Play()
    {
        Bass.ChannelPlay(handle);
        Bass.SuppressMP3ErrorCorruptionSilence = true;
        Debug.Log(Bass.LastError);
    }

    public float GetTimeInSeconds() =>
        (float) Bass.ChannelBytes2Seconds(handle, Bass.ChannelGetPosition(handle, PositionFlags.Bytes));

    private void OnApplicationQuit()
    {
        Bass.StreamFree(handle);
        Bass.Free();
    }
}
