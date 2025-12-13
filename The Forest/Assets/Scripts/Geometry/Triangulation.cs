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

public partial class Triangulation : IEnumerable<Triangle>
{
    private Rect bounds;
    private Triangle root;
    public Triangle Root => root;

    private Dictionary<Face, Triangle> faceToTriangle;
    private HashSet<Triangle> leaves;

    private Graph graph;
    public Graph Graph => graph;

    private Queue<Vertex> vertexQueue = new Queue<Vertex>();
    private Queue<HalfEdge> flipQueue = new Queue<HalfEdge>();

    private bool isRunning = false;
    public bool IsRunning => isRunning;

#region IEnumerables
    public IEnumerator<Triangle> GetEnumerator()
    {
        return leaves.GetEnumerator();        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<Triangle> GetTriangleEnumerator()
    {
        return leaves.GetEnumerator();        
    }

#endregion

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

        graph = new Graph();
        Vertex va = graph.AddVertex(a, "R_a");
        Vertex vb = graph.AddVertex(b, "R_b");
        Vertex vc = graph.AddVertex(c, "R_c");

        root = new Triangle(graph, va, vb, vc);
        faceToTriangle[root.face] = root;

        leaves = new HashSet<Triangle> { root };
    }

    public void AddVertex(Vector2 position, string name)
    {
        Vertex v = graph.AddVertex(position, name);
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
        graph.RemoveFace(t.face);
        
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

        HalfEdge eva, eav, evb, ebv, evc, ecv;

        (eva, eav) = graph.AddEdge(v, a);
        (evb, ebv) = graph.AddEdge(v, b);
        (evc, ecv) = graph.AddEdge(v, c);

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

        Debug.Log($"[Triangulation.AddVertex] v.edge = {v.edge}");
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
        graph.RemoveEdge(e);

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
        graph.RemoveFace(fbda);
        graph.RemoveFace(fdbc);

        Vertex va = eab.fromVertex;
        Vertex vc = ecd.fromVertex;

        var (eac, eca) = graph.AddEdge(va, vc);
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

        Face fabc = graph.AddFace(eca);
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
        // however testing involves trig and causes rounding errors.
        //
        // alternatively, we can test whether D lies within the circumcircle of ABC by 
        // evaluating the determinant of:
        //
        // | Ax  Ay  Ax^2 + Ay^2 1 |  
        // | Bx  By  Bx^2 + By^2 1 | 
        // | Cx  Cy  Cx^2 + Cy^2 1 |
        // | Dx  Dy  Dx^2 + Dy^2 1 |
        //
        // https://en.wikipedia.org/wiki/Delaunay_triangulation#Visual_Delaunay_definition:_Flipping

        HalfEdge ebd = edge;
        HalfEdge eda = ebd.next;
        HalfEdge eab = eda.next;

        HalfEdge edb = ebd.flip;
        HalfEdge ebc = edb.next;
        HalfEdge ecd = ebc.next;

        Vector2 a = eab.fromVertex;
        Vector2 b = ebd.fromVertex;
        Vector2 c = ecd.fromVertex;
        Vector2 d = eda.fromVertex;

        Matrix4x4 m = new Matrix4x4();
        m[0,0] = a.x; m[0,1] = a.y; m[0,2] = a.x * a.x + a.y * a.y; m[0,3] = 1;
        m[1,0] = b.x; m[1,1] = b.y; m[1,2] = b.x * b.x + b.y * b.y; m[1,3] = 1;
        m[2,0] = c.x; m[2,1] = c.y; m[2,2] = c.x * c.x + c.y * c.y; m[2,3] = 1;
        m[3,0] = d.x; m[3,1] = d.y; m[3,2] = d.x * d.x + d.y * d.y; m[3,3] = 1;

        return m.determinant >= 0; // D is inside Cicum(ABC) so the edge BD is delaunay
    }

    private Face CreateFace(HalfEdge eab, HalfEdge ebc, HalfEdge eca)
    {
        Face face = graph.AddFace(eab);
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
