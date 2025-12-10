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

public class VoronoiDiagram
{   
    private HashSet<Vertex> vertices;
    private Dictionary<Face,Vertex> mapFV;

    public VoronoiDiagram(Triangulation triangulation)
    {
        MakeVertices(triangulation);
        MakeEdges(triangulation);
    }

    private void MakeVertices(Triangulation triangulation)
    {
        vertices = new HashSet<Vertex>();
        mapFV = new Dictionary<Face,Vertex>();

        // make a vertex at the circumcentre of each triangle
        foreach (Triangle t in triangulation)
        {
            Vertex v = new Vertex(t.Circumcentre, t.Name);
            vertices.Add(v);
            mapFV[t.face] = v;
        }
    }

    private void MakeEdges(Triangulation triangulation)
    {
        // every vertex has three outgoing edges
        // corresponding to the three neighbouring triangles

        foreach (Triangle t in triangulation)
        {
            Vertex va = mapFV[t.face];

            // add edges in clockwise order as the linked
            // list construction reverses them
            for (int i = 2; i >= 0; i--)
            {
                Face fb = t.edges[i].flip.face;
                if (fb != null)
                {
                    Vertex vb = mapFV[fb];
                    HalfEdge e = va.edge;
                    va.edge = HalfEdge.CreateEdgePair(va, vb);
                    va.edge.next = e;                    
                }

            }
        }
        

    }

}
}