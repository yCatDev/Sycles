using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MetaballSystem2D
{
    private static List<Metaball2D> metaballs =  new List<Metaball2D>();

    static MetaballSystem2D()
    {
        metaballs = new List<Metaball2D>();
    }

    public static void Add(Metaball2D metaball)
    {
        var ind = metaballs.IndexOf(metaball);
        if (ind < 0)
            metaballs.Add(metaball);
        else
            metaballs[ind] = metaball;
    }

    public static bool Contains(Metaball2D metaball)
    {
        return metaballs.Contains(metaball);
    }
    
    public static List<Metaball2D> Get()
    {
        return metaballs;
    }

    private static void CleanUp()
    {
        for (var index = 0; index < metaballs.Count; index++)
        {
            var metaball = metaballs[index];
            if (metaball == null)
            {
                metaballs.Remove(metaball);
                index--;
            }
        }
    }
    
    public static void Remove(Metaball2D metaball)
    {
        metaballs.Remove(metaball);
    }

    public static void Update(int index, Metaball2D metaball2D)
    {
        metaballs[index] = metaball2D;
    }
}
