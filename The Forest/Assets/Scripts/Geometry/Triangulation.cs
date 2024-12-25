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
        public Face face;
        public List<Triangle> children;

        public Triangle(Vector2 a, Vector2 b, Vector2 c) 
        {
            Vertex va = new Vertex(a);
            Vertex vb = new Vertex(b);
            Vertex vc = new Vertex(c);

            HalfEdge eab = new HalfEdge(va);
            HalfEdge ebc = new HalfEdge(vb);
            HalfEdge eca = new HalfEdge(vc);
                        
            va.edge = eab;
            vb.edge = ebc;
            vc.edge = eca;

            eab.next = ebc;
            ebc.next = eca;
            eca.next = eab;

            this.face = new Face(eab);

            this.children = null;
        }

        public Triangle(Face face)
        {
            this.face = face;
            this.children = null;
        }

        public bool Contains(Vector2 p) 
        {
            return face.Contains(p, strict: false);
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
        
        HalfEdge eab = t.face.edge;
        HalfEdge ebc = eab.next;
        HalfEdge eca = ebc.next;

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
        Face fvbc = CreateFace(eab, ebv, eva);
        Face fvca = CreateFace(eab, ebv, eva);

        t.children = new List<Triangle>();
        t.children.Add(new Triangle(fvab));
        t.children.Add(new Triangle(fvbc));
        t.children.Add(new Triangle(fvca));
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
            GeometryGizmos.DrawGizmo(triangle.face, transform: transform);
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
