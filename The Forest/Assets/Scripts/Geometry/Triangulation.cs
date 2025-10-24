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

public partial class Triangulation 
{
    private Rect bounds;
    private Triangle root;
    private Dictionary<Face, Triangle> faceToTriangle;
    private HashSet<Triangle> leaves;
    private Queue<Vertex> vertexQueue = new Queue<Vertex>();
    private Queue<HalfEdge> flipQueue = new Queue<HalfEdge>();
    private bool isRunning = false;
    public bool IsRunning => isRunning;

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

        Vertex va = new Vertex(a, "R_a");
        Vertex vb = new Vertex(b, "R_b");
        Vertex vc = new Vertex(c, "R_c");

        root = new Triangle(va, vb, vc);
        faceToTriangle[root.face] = root;

        leaves = new HashSet<Triangle> { root };
    }

    public void EnqueueVertex(Vertex v)
    {
        vertexQueue.Enqueue(v);
    }

    public IEnumerator RunCR()
    {
        isRunning = true;
        while (vertexQueue.Count > 0)
        {
            Vertex v = vertexQueue.Dequeue();
            AddVertex(v);
            yield return FlipEdgesCR();
        }
        isRunning = false;
    }

    public void Run()
    {
        while (vertexQueue.Count > 0)
        {
            Vertex v = vertexQueue.Dequeue();
            AddVertex(v);
            FlipEdges();
        }
    }

    private void AddVertex(Vertex v)
    {
        Debug.Log($"[Triangulation.AddVertex] Adding vertex {v.name}");
        Triangle t = EnclosingTriangle(v.position);
        Debug.Log($"[Triangulation.AddVertex] Enclosing triangle: {t.Name}");
        
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
        Debug.Log($"[Triangulation.AddVertex] Adding edge {eva.Name}");
        Debug.Log($"[Triangulation.AddVertex] Adding edge {evb.Name}");
        Debug.Log($"[Triangulation.AddVertex] Adding edge {evc.Name}");

        HalfEdge eav = eva.flip;
        HalfEdge ebv = evb.flip;
        HalfEdge ecv = evc.flip;

        Face fvab = CreateFace(eab, ebv, eva);
        Face fvbc = CreateFace(ebc, ecv, evb);
        Face fvca = CreateFace(eca, eav, evc);

        Triangle tvab = new Triangle(eva, eab, ebv);
        faceToTriangle[fvab] = tvab;

        Triangle tvbc = new Triangle(evb, ebc, ecv);
        faceToTriangle[fvbc] = tvbc;

        Triangle tvca = new Triangle(evc, eca, eav);
        faceToTriangle[fvca] = tvca;

        t.children = new List<Triangle> { tvab, tvbc, tvca };

        leaves.Remove(t);
        leaves.Add(tvab);
        leaves.Add(tvbc);
        leaves.Add(tvca);

        flipQueue.Enqueue(eab);
        flipQueue.Enqueue(ebc);
        flipQueue.Enqueue(eca);
    }

    private IEnumerator FlipEdgesCR() 
    {
        while (flipQueue.Count > 0)
        {
            yield return null;

            HalfEdge e = flipQueue.Dequeue();

            if (IsInterior(e) && !IsDelaunay(e))
            {
                FlipEdge(e);
            }
        }
    }

    private void FlipEdges() 
    {
        while (flipQueue.Count > 0)
        {
            HalfEdge e = flipQueue.Dequeue();

            if (IsInterior(e) && !IsDelaunay(e))
            {
                FlipEdge(e);
            }
        }
    }

    private void FlipEdge(HalfEdge e)
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

        Debug.Log($"[Triangulation.FlipEdge] Flipping {e.Name} to {eac.Name}");

        Triangle tabc = MakeTriangle(eab, ebc, eca);
        Triangle tacd = MakeTriangle(eac, ecd, eda);
        leaves.Add(tacd);
        leaves.Add(tabc);

        // Connect to old triangles

        Triangle tbda = faceToTriangle[fbda];
        tbda.children = new List<Triangle> { tabc, tacd };
        leaves.Remove(tbda);

        Triangle tdbc = faceToTriangle[fdbc];
        tdbc.children = new List<Triangle> { tabc, tacd };
        leaves.Remove(tdbc);

        // enqueue the surrounding edges
        flipQueue.Enqueue(eab);
        flipQueue.Enqueue(ebc);
        flipQueue.Enqueue(eda);
        flipQueue.Enqueue(ecd);
    }

    private Triangle MakeTriangle(HalfEdge eab, HalfEdge ebc, HalfEdge eca)
    {
        eca.next = eab;
        eab.next = ebc;
        ebc.next = eca;

        Face fabc = new Face(eca);
        eca.face = fabc;
        eab.face = fabc;
        ebc.face = fabc;

        Triangle tabc = new Triangle(eca, eab, ebc);
        faceToTriangle[fabc] = tabc;
        
        return tabc;
    }

    public void RemoveRoot() 
{
HashSet<Triangle> newLeaves = new HashSet<Triangle>();

HashSet<Vertex> rootVertices = new HashSet<Vertex>();
rootVertices.Add(root.edges[0].fromVertex);
rootVertices.Add(root.edges[1].fromVertex);
rootVertices.Add(root.edges[2].fromVertex);

foreach (Triangle t in leaves)
{
    if (rootVertices.Contains(t.edges[0].fromVertex)
    || rootVertices.Contains(t.edges[1].fromVertex) 
    || rootVertices.Contains(t.edges[2].fromVertex))
    {
        continue;
    }
    else 
    {
        newLeaves.Add(t);
    }
}

leaves = newLeaves;
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

        // Due to floating point rounding errors this result may be >180 for both (angleA + angleC) and (angleB + angleD)
        // So accept this if the other option is also bad        
        return (angleA + angleC) <= 180 || (angleB + angleD) > 180;
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
        foreach(Triangle triangle in leaves)
        {
            DrawTriangleGizmo(triangle);
        }
    }

    private void DrawTriangleGizmo(Triangle triangle, Transform transform = null)
    {
        HalfEdge e0 = triangle.edges[0];
        HalfEdge e1 = triangle.edges[1];
        HalfEdge e2 = triangle.edges[2];

        Vector3 a = e0.fromVertex.position;
        Vector3 b = e1.fromVertex.position;
        Vector3 c = e2.fromVertex.position;

        if (transform != null) 
        {
            a = transform.TransformPoint(a);
            b = transform.TransformPoint(b);
            c = transform.TransformPoint(c);
        }

        Gizmos.color = Color.white;
        // Gizmos.DrawWireSphere(a, 0.1f);
        // Gizmos.DrawWireSphere(b, 0.1f);
        // Gizmos.DrawWireSphere(c, 0.1f);
        Handles.Label(a, e0.fromVertex.name);
        Handles.Label(b, e1.fromVertex.name);
        Handles.Label(c, e2.fromVertex.name);

        // shrink the triangle a little to make it visible
        Vector3 p = (a+b+c) / 3;
        a = p + (a-p) * 0.95f;
        b = p + (b-p) * 0.95f;
        c = p + (c-p) * 0.95f;

        Gizmos.color = (!IsInterior(e0) || IsDelaunay(e0)) ? Color.green : Color.red;
        Gizmos.DrawLine(a, b);
        Gizmos.color = (!IsInterior(e1) || IsDelaunay(e1)) ? Color.green : Color.red;
        Gizmos.DrawLine(b, c);
        Gizmos.color = (!IsInterior(e2) || IsDelaunay(e2)) ? Color.green : Color.red;
        Gizmos.DrawLine(c, a);
        
    } 
#endregion

}

}
