using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Master : MonoBehaviour {
    public ComputeShader raymarching;

    RenderTexture target;
    Camera cam;
    List<ComputeBuffer> buffersToDispose;
    private bool inited = false, initedRaymarching;
    private int threadGroupsX;
    private int threadGroupsY;

    void Init () {
        cam = Camera.current;
    }


    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        Init ();
        buffersToDispose = new List<ComputeBuffer> ();

        InitRenderTexture ();
        CreateScene ();
        SetParameters ();

        
            raymarching.SetTexture(0, "Source", source);
            raymarching.SetTexture(0, "Destination", target);
            threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
            threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
            
        raymarching.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        Graphics.Blit (target, destination);

        foreach (var buffer in buffersToDispose) {
            buffer.Dispose ();
        }
    }

    void CreateScene () {
        Shape[] allShapes = FindObjectsOfType<Shape> ();
        //allShapes.Sort ((a, b) => a.operation.CompareTo (b.operation));

        //List<Shape> orderedShapes = new List<Shape> ();

        /*for (int i = 0; i < allShapes.Count; i++) {
            // Add top-level shapes (those without a parent)
            if (allShapes[i].transform.parent == null) {

                Transform parentShape = allShapes[i].transform;
                orderedShapes.Add (allShapes[i]);
                allShapes[i].numChildren = parentShape.childCount;
                // Add all children of the shape (nested children not supported currently)
                for (int j = 0; j < parentShape.childCount; j++) {
                    if (parentShape.GetChild (j).GetComponent<Shape> () != null) {
                        orderedShapes.Add (parentShape.GetChild (j).GetComponent<Shape> ());
                        orderedShapes[orderedShapes.Count - 1].numChildren = 0;
                    }
                }
            }

        }*/

        ShapeData[] shapeData = Array.ConvertAll(allShapes, (x) => x.data);
        /*for (int i = 0; i < allShapes.Length; i++) {
            var s = allShapes[i];
            
        }*/

        ComputeBuffer shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize ());
        shapeBuffer.SetData (shapeData);
        raymarching.SetBuffer (0, "shapes", shapeBuffer);
        raymarching.SetInt ("numShapes", shapeData.Length);

        buffersToDispose.Add (shapeBuffer);
    }

    void SetParameters () {
//        bool lightIsDirectional = lightSource.type == LightType.Directional;
        raymarching.SetMatrix ("_CameraToWorld", cam.cameraToWorldMatrix);
        raymarching.SetMatrix ("_CameraInverseProjection", cam.projectionMatrix.inverse);
       // raymarching.SetVector ("_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
       // raymarching.SetBool ("positionLight", !lightIsDirectional);
    }

    void InitRenderTexture ()
    {
        if (inited) return;
        inited = true;
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight) {
            if (target != null) {
                target.Release ();
            }
            target = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create ();
        }
    }

    public struct ShapeData {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 colour;
        public int shapeType;
        public int operation;
        public float blendStrength;
        public int numChildren;

        public static int GetSize () {
            return sizeof (float) * 10 + sizeof (int) * 3;
        }
    }
}
