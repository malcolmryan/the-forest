/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;

namespace WordsOnPlay.Geometry
{
public class GeometryGizmos
{
    public const float defaultVertexRadius = 0.1f;
    private static Mesh gizmoMesh = CreateGizmoMesh();

    private static Mesh CreateGizmoMesh()
    {
        Mesh mesh = new Mesh();
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.vertices = new Vector3[3];
            mesh.triangles = new int[]
            {
                0, 1, 2
            };
            mesh.normals = new Vector3[]
            {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward
            };
        }

        return mesh;
    }

    public static void DrawGizmo(
        Vertex v, float radius = defaultVertexRadius, bool solid = false, Transform transform = null) 
    {
        Vector3 p = v.position;
        
        if (transform != null)
        {
            p = transform.TransformPoint(p);
        }

        if (solid)
        {
            Gizmos.DrawSphere(p, radius);            
        }
        else 
        {
            Gizmos.DrawWireSphere(p, radius);
        }
    }

    public static void DrawGizmo(HalfEdge e, Transform transform = null) 
    {
        Vector3 a = e.fromVertex.position;
        Vector3 b = e.next.fromVertex.position;

        if (transform != null)
        {
            a = transform.TransformPoint(a);
            b = transform.TransformPoint(b);
        }

        Gizmos.DrawLine(a, b);
    }

    public static void DrawGizmo(Face f, bool solid = false, Transform transform = null) 
    {
        HalfEdge e = f.edge;

        if (solid)
        {
            // Note: this assumes that the face is convex
            do 
            {
                DrawTriangleGizmo(e, true, transform);
                e = e.next;
            } while (e != f.edge);
 
        }
        else 
        {
            do 
            {
                DrawGizmo(e);
                e = e.next;
            } while (e != f.edge);
        }
    }


    public static void DrawTriangleGizmo(HalfEdge e, bool solid = false, Transform transform = null)
    {
        Vector3 a = e.fromVertex.position;
        Vector3 b = e.next.fromVertex.position;
        Vector3 c = e.next.next.fromVertex.position;

        if (transform != null) 
        {
            a = transform.TransformPoint(a);
            b = transform.TransformPoint(b);
            c = transform.TransformPoint(c);
        }

        if (solid)
        {
            gizmoMesh.vertices[0] = a;
            gizmoMesh.vertices[1] = b;
            gizmoMesh.vertices[2] = c;

            Graphics.DrawMeshNow(gizmoMesh, Vector3.zero, Quaternion.identity);
        }
        else 
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
}

