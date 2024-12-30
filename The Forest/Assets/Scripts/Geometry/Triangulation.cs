/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;
using System.Collections;
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
        public Face face;

        public string Name 
        {
            get { return $"{edges[0].fromVertex.name},{edges[1].fromVertex.name},{edges[2].fromVertex.name}"; }
        }

        public Triangle(Triangulation triangulation, Vertex va, Vertex vb, Vertex vc) 
        {
            this.edges = new HalfEdge[3];
            va.edge = edges[0] = HalfEdge.CreateEdgePair(va, vb);
            vb.edge = edges[1] = HalfEdge.CreateEdgePair(vb, vc);
            vc.edge = edges[2] = HalfEdge.CreateEdgePair(vc, va);
                        
            edges[0].next = edges[1];
            edges[1].next = edges[2];
            edges[2].next = edges[0];

            edges[0].flip.next = edges[2];
            edges[1].flip.next = edges[0];
            edges[2].flip.next = edges[1];

            this.face = new Face(edges[0]);
            edges[0].face = face;
            edges[1].face = face;
            edges[2].face = face;

            triangulation.faceToTriangle[face] = this;
            triangulation.nTriangles++;

            this.children = null;
        }

        public Triangle(Triangulation triangulation, HalfEdge e0, HalfEdge e1, HalfEdge e2)
        {
            this.edges = new HalfEdge[3];
            edges[0] = e0;
            edges[1] = e1;
            edges[2] = e2;
            
            this.face = e0.face;
            triangulation.faceToTriangle[face] = this;

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
    private Dictionary<Face, Triangle> faceToTriangle;
    private int nTriangles = 0;

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

        nTriangles = 0;
        faceToTriangle = new Dictionary<Face, Triangle>();

        Vertex va = new Vertex(a, "R_a");
        Vertex vb = new Vertex(b, "R_b");
        Vertex vc = new Vertex(c, "R_c");

        root = new Triangle(this, va, vb, vc);
    }

    public void AddVertex(Vertex v)
    {
        Debug.Log($"Adding vertex {v.name}");
        Triangle t = EnclosingTriangle(v.position);
        Debug.Log($"Enclosing triangle: {t.Name}");
        
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

        HalfEdge eva = HalfEdge.CreateEdgePair(v, a);
        HalfEdge evb = HalfEdge.CreateEdgePair(v, b);
        HalfEdge evc = HalfEdge.CreateEdgePair(v, c);
        Debug.Log($"Adding edge {eva.Name}");
        Debug.Log($"Adding edge {evb.Name}");
        Debug.Log($"Adding edge {evc.Name}");

        HalfEdge eav = eva.flip;
        HalfEdge ebv = evb.flip;
        HalfEdge ecv = evc.flip;

        Face fvab = CreateFace(eab, ebv, eva);
        Face fvbc = CreateFace(ebc, ecv, evb);
        Face fvca = CreateFace(eca, eav, evc);

        Triangle ta = new Triangle(this, eab, ebv, eva);
        Triangle tb = new Triangle(this, ebc, ecv, evb);
        Triangle tc = new Triangle(this, eca, eav, evc);

        t.children = new List<Triangle>();
        t.children.Add(ta);
        t.children.Add(tb);
        t.children.Add(tc);

        queue.Enqueue(eab);
        queue.Enqueue(ebc);
        queue.Enqueue(eca);
    }

    private Queue<HalfEdge> queue = new Queue<HalfEdge>();

    public void FlipEdges() 
    {
        int iterations = 0;
        int maxIterations = 10000;

        while (queue.Count > 0 && iterations < maxIterations)
        {
            HalfEdge e = queue.Dequeue();
            Debug.Log($"Processing edge: {e.Name}");

            if (IsInterior(e) && !IsDelaunay(e))
            {
                HalfEdge ebd = e;
                HalfEdge eda = ebd.next;
                HalfEdge eab = eda.next;

                HalfEdge edb = ebd.flip;
                HalfEdge ebc = edb.next;
                HalfEdge ecd = ebc.next;

                //     D                   D
                //    /|\                 / \
                //   / | \               /   \
                //  A  |  C    ===>     A --- C 
                //   \ | /               \   /
                //    \|/                 \ /
                //     B                   B

                Face fbda = ebd.face;
                Face fdbc = edb.face;

                Vertex va = eab.fromVertex;
                Vertex vc = ecd.fromVertex;

                HalfEdge eac = HalfEdge.CreateEdgePair(va, vc);
                HalfEdge eca = eac.flip;

                Debug.Log($"Flipping {e.Name} to {eac.Name}");

                // Trinagle ABC

                eca.next = eab;
                eab.next = ebc;
                ebc.next = eca;

                Face fabc = new Face(eca);
                eca.face = fabc;
                eab.face = fabc;
                ebc.face = fabc;

                Triangle tabc = new Triangle(this, eca, eab, ebc);

                // Triangle ACD

                eac.next = ecd;
                ecd.next = eda;
                eda.next = eac;
                
                Face facd = new Face(eac);
                eac.face = facd;
                ecd.face = facd;
                eda.face = facd;

                Triangle tacd = new Triangle(this, eac, ecd, eda);

                // Connect to old triangles

                Triangle tbda = faceToTriangle[fbda];
                tbda.children = new List<Triangle>();
                tbda.children.Add(tabc);
                tbda.children.Add(tacd);

                Triangle tdbc = faceToTriangle[fdbc];
                tdbc.children = new List<Triangle>();
                tdbc.children.Add(tabc);
                tdbc.children.Add(tacd);

                // enqueue the surrounding edges
                queue.Enqueue(eab);
                queue.Enqueue(ebc);
                queue.Enqueue(eda);
                queue.Enqueue(ecd);

                iterations++;
            }
        }

    }

    private bool IsInterior(HalfEdge edge)
    {
        return edge.face != null && edge.flip.face != null;
    }

    private bool IsDelaunay(HalfEdge edge) 
    {

        //     D
        //    /|\
        //   / | \
        //  A  |  C
        //   \ | /
        //    \|/
        //     B
        // 
        // e = BD
        //
        // satisfies the Delaunay constraint if angle(DAB) + angle(BCD) <= 180°
        //
        // https://en.wikipedia.org/wiki/Delaunay_triangulation#Visual_Delaunay_definition:_Flipping

        // Due to floating point rounding errors this result may be >180 for both (angleA + angleC) and (angleB + angleD)
        // So instead test which is smaller of the two sums.

        HalfEdge ebd = edge;
        HalfEdge eda = ebd.next;
        HalfEdge eab = eda.next;

        HalfEdge edb = ebd.flip;
        HalfEdge ebc = edb.next;
        HalfEdge ecd = ebc.next;

        Vector2 vab = eab.Direction;
        Vector2 vbc = ebc.Direction;
        Vector2 vcd = ecd.Direction;
        Vector2 vda = eda.Direction;

        // unsigned angle in degrees
        float angleA = Vector2.Angle(-vda, vab); 
        float angleB = Vector2.Angle(-vab, vbc); 
        float angleC = Vector2.Angle(-vbc, vcd); 
        float angleD = Vector2.Angle(-vcd, vda); 

        return (angleA + angleC) <= (angleB + angleD);
        
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

#region Gizmos
    public void DrawGizmo(Transform transform = null)
    {
        DrawGizmo(root, transform);
    }

    private void DrawGizmo(Triangle triangle, Transform transform = null)
    {
        if (triangle.children == null)
        {
            Vector3 a = triangle.edges[0].fromVertex.position;
            Vector3 b = triangle.edges[1].fromVertex.position;
            Vector3 c = triangle.edges[2].fromVertex.position;

            if (transform != null) 
            {
                a = transform.TransformPoint(a);
                b = transform.TransformPoint(b);
                c = transform.TransformPoint(c);
            }

            Gizmos.color =  (IsDelaunay(triangle.edges[0]) ? Color.green : Color.red);
            Gizmos.DrawLine(a, b);
            Gizmos.color =  (IsDelaunay(triangle.edges[1]) ? Color.green : Color.red);
            Gizmos.DrawLine(b, c);
            Gizmos.color =  (IsDelaunay(triangle.edges[2]) ? Color.green : Color.red);
            Gizmos.DrawLine(c, a);
        }
        else
        {
            foreach (Triangle child in triangle.children)
            {
                DrawGizmo(child);
            }
        }
    }
#endregion

}

}
