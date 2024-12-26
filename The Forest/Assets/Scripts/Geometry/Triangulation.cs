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

        public Triangle(Triangulation triangulation, Vector2 a, Vector2 b, Vector2 c) 
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

            this.face = new Face(edges[0]);
            edges[0].face = face;
            edges[1].face = face;
            edges[2].face = face;

            triangulation.faceToTriangle[face] = this;

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

        faceToTriangle = new Dictionary<Face, Triangle>();
        root = new Triangle(this, a, b, c);
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

        // FlipEdges(eab);
        // FlipEdges(ebc);
        // FlipEdges(eca);
    }

    Queue<HalfEdge> queue = new Queue<HalfEdge>();

    public IEnumerator FlipEdges() 
    {
        while (queue.Count > 0)
        {
            HalfEdge e = queue.Dequeue();

            if (!IsDelaunay(e))
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

                // Trinagle ABC

                eca.next = eab;
                eab.next = ebc;
                ebc.next = eca;

                Face fabc = new Face(eca);
                eca.face = fabc;
                eab.face = fabc;
                ebc.face = fabc;

                Triangle tabc = new Triangle(this, eca, ebc, ebc);

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
            }

            yield return new WaitForSeconds(1);
        }
    }


    private bool IsDelaunay(HalfEdge edge) {

        // ignore exterior edges
        if (edge.flip == null) 
        {
            return true;
        }

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

        HalfEdge ebd = edge;
        HalfEdge eda = ebd.next;
        HalfEdge eab = eda.next;

        HalfEdge edb = ebd.flip;
        HalfEdge ebc = edb.next;
        HalfEdge ecd = ebc.next;

        Vector2 vab = eab.Direction;
        Vector2 vad = -eda.Direction;
        float angleA = Vector2.Angle(vab, vad); // unsigned angle in degrees

        Vector2 vcb = -ebc.Direction;
        Vector2 vcd = ecd.Direction;
        float angleC = Vector2.Angle(vcb, vcd); // unsigned angle in degrees

        return angleA + angleC <= 180;
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
