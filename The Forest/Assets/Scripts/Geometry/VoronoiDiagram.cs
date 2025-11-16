/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{

public partial class VoronoiDiagram
{   
    private HashSet<Vertex> vertices;

    public VoronoiDiagram(IEnumerable<Triangle> triangles)
    {
        vertices = new HashSet<Vertex>();
        Dictionary<Triangle,Vertex> mapTV = new Dictionary<Triangle,Vertex>();
        Dictionary<Face,Triangle> mapFT = new Dictionary<Face,Triangle>();

        foreach (Triangle t in triangles)
        {
            Vertex v = new Vertex(t.Circumcentre, t.Name);
            vertices.Add(v);
            mapTV[t] = v;
            mapFT[t.face] = t;
        }

        foreach (Triangle t in triangles)
        {
            Vertex v = mapTV[t];
            for (int i = 0; i < 3; i++)
            {
                HalfEdge e = t.edges[i].flip;
                if (e != null)
                {
                    Triangle tNext = mapFT[e.face];
                    Vertex vNext = mapTV[tNext];
                    
                                        
                }
            }
        }
    }

}
}