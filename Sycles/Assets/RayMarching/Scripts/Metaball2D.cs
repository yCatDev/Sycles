using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Metaball2D : MonoBehaviour
{

    [SerializeField] public Color color;


    private void Awake()
    {
 
        //MetaballSystem2D.Add(this);
    }

    /*private void OnValidate()
    {
        if (gameObject.scene.name == null) return;
        if (MetaballSystem2D.Contains(this)) return;
        collider = GetComponent<CircleCollider2D>();
        MetaballSystem2D.Add(this);
    }*/

    private void OnBecameVisible()
    {
        //IsInView = true;
    }

    private void OnBecameInvisible()
    {
        //IsInView = false;
    }

    private void Update()
    {
        MetaballSystem2D.Add(this);
    }

    public Vector4 GetColor()
    {
        return new Vector4(color.r, color.g, color.b,1);
    }
    
    
    public void SetColor(Color color)
    {
        this.color = color; 
    }
    
    
    public float GetRadius()
    {
        return transform.localScale.x;
    }

    private void OnDestroy()
    {
        MetaballSystem2D.Remove(this);
    }
}
