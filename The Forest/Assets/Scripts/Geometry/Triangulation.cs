/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{

[Serializable]
public class Triangulation 
{
    [Serializable]
    private class Triangle 
    {
        public List<Triangle> children;
        public HalfEdge[] edges;

        public Triangle(Vector2 a, Vector2 b, Vector2 c) 
        {
            Vertex va = new Vertex(a);
            Vertex vb = new Vertex(b);
            Vertex vc = new Vertex(c);

            this.edges = new HalfEdge[3];
            va.edge = edges[0] = new HalfEdge(va);
            vb.edge = edges[1] = new HalfEdge(vb);
            vc.edge = edges[2] = new HalfEdge(vc);
                        
            edges[0].next = edges[1];
            edges[1].next = edges[2];
            edges[2].next = edges[0];

            Face face = new Face(edges[0]);
            edges[0].face = face;
            edges[1].face = face;
            edges[2].face = face;

            this.children = null;
        }

        public Triangle(HalfEdge e0, HalfEdge e1, HalfEdge e2)
        {
            this.edges = new HalfEdge[3];
            edges[0] = e0;
            edges[1] = e1;
            edges[2] = e2;
            
            this.children = null;
        }

        public bool Contains(Vector2 p) 
        {
            for (int i = 0; i < 3; i++) 
            {
                if (edges[i].Side(p) < 0)
                {
                    return false;
                }
            }
            return true;
        }

    }


    private Rect bounds;
    private Triangle root;

    public Triangulation(Rect bounds)
    {
        this.bounds = bounds;

        float x = bounds.xMin;
        float y = bounds.yMin;
        float w = bounds.width;
        float h = bounds.height;

        // 
        // Enclose the bounds in a triangle as:
        //   c--+-----+--b
        //    \ |     | /
        //     \|     |/
        //      +-----+
        //       \   /
        //        \ /
        //         a

        Vector2 a = new Vector2(x+w/2, y-h);
        Vector2 b = new Vector2(x+w+w/2, y+h);
        Vector2 c = new Vector2(x-w/2, y+h);

        root = new Triangle(a, b, c);
    }

    public void AddVertex(Vector2 p)
    {
        Triangle t = EnclosingTriangle(p);
        
        HalfEdge eab = t.edges[0];
        HalfEdge ebc = t.edges[1];
        HalfEdge eca = t.edges[2];

        Vertex a = eab.fromVertex;
        Vertex b = ebc.fromVertex;
        Vertex c = eca.fromVertex;

        //            a
        //           /|\
        //          / | \
        //         /  |  \
        //        / _ v _ \
        //       /_/     \_\
        //      b ----------c

        Vertex v = new Vertex(p);

        HalfEdge eva = HalfEdge.CreateEdgePair(v, a);
        HalfEdge evb = HalfEdge.CreateEdgePair(v, b);
        HalfEdge evc = HalfEdge.CreateEdgePair(v, c);

        HalfEdge eav = eva.flip;
        HalfEdge ebv = evb.flip;
        HalfEdge ecv = evc.flip;

        Face fvab = CreateFace(eab, ebv, eva);
        Face fvbc = CreateFace(ebc, ecv, evb);
        Face fvca = CreateFace(eca, eav, evc);

        t.children = new List<Triangle>();
        t.children.Add(new Triangle(eab, ebv, eva));
        t.children.Add(new Triangle(ebc, ecv, evb));
        t.children.Add(new Triangle(eca, eav, evc));
    }

    private Face CreateFace(HalfEdge eab, HalfEdge ebc, HalfEdge eca)
    {
        Face face = new Face(eab);
        eab.next = ebc;
        ebc.next = eca;
        eca.next = eab;
        eab.face = face;
        ebc.face = face;
        eca.face = face;

        return face;    
    }

    private Triangle EnclosingTriangle(Vector2 p) 
    {
        if (!root.Contains(p))
        {
            throw new ArgumentException($"Point {p} lies outside the bounding triangle");
        }

        Triangle t = root;

        while (t.children != null)
        {
            foreach (Triangle child in t.children)
            {
                if (child.Contains(p))
                {
                    t = child;
                    break;
                }
            }
        }

        return t;
    }

    public void DrawGizmo(Transform transform = null)
    {
        DrawGizmo(root, transform);
    }

    private void DrawGizmo(Triangle triangle, Transform transform = null)
    {
        if (triangle.children == null)
        {
            GeometryGizmos.DrawTriangleGizmo(triangle.edges[0], transform: transform);
        }
        else
        {
            foreach (Triangle child in triangle.children)
            {
                DrawGizmo(child);
            }
        }
    }
}

}
