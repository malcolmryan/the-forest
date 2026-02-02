/**
 * Graph Gizmo
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using UnityEngine;
using UnityEditor;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{
    public class GraphGizmo : MonoBehaviour
    {
#region Parameters
        [SerializeField] Transform parentTransform = null;

        [Header("Vertices")]
        [SerializeField] bool drawVertices = false;
        [SerializeField] bool labelVertices = true;
        [SerializeField] float vertexSize = 0.1f;
        [SerializeField] Color defaultVertexColor = Color.white;

        [Header("Edges")]
        [SerializeField] bool drawEdges = true;
        [SerializeField] float arrowSize = 0.1f;
        [SerializeField] float arrowPosition = 0.45f;
        [SerializeField] Quaternion arrowRotation = Quaternion.AngleAxis(-30, Vector3.forward);
        [SerializeField] Color defaultEdgeColor = Color.white;

        [Header("Faces")]
        [SerializeField] bool drawFaces = true;
        [SerializeField] Color defaultFaceColor = Color.white;
#endregion

#region State
        private Graph graph;
        public Graph Graph
        {
            get => graph;
            set
            {
                graph = value;
            }
        }
#endregion

#region Gizmos
        public void OnDrawGizmos()
        {
            if (graph == null)
            {
                return;
            }

            foreach (Vertex v in graph.Vertices)
            {
                DrawVertexGizmo(v);
            }

            if (drawEdges)
            {
                foreach (HalfEdge e in graph.Edges)
                {
                    DrawHalfEdgeGizmo(e);
                }                
            }

            if (drawFaces)
            {
                foreach (Face f in graph.Faces)
                {
                    DrawFaceGizmo(f);
                }                                
            }
        }

        public void DrawVertexGizmo(Vertex v, Color? color = null)
        {
            Gizmos.color = (color == null ? defaultVertexColor : color.Value);
    
            Vector3 p = v.position;
            if (parentTransform != null) 
            {
                p = parentTransform.TransformPoint(p);
            }

            if (drawVertices)
            {
                Gizmos.DrawWireSphere(p, vertexSize);                
            }
            if (labelVertices)
            {
                Handles.Label(p, v.name);                
            }
        }    

        public void DrawHalfEdgeGizmo(HalfEdge e, Color? color = null)
        {
            Gizmos.color = (color == null ? defaultEdgeColor : color.Value);

            Vertex va = e.fromVertex;
            Vertex vb = e.flip.fromVertex;
        
            Vector3 pa = va.position;
            Vector3 pb = vb.position;
            Vector3 pc = pa + (pb-pa) * arrowPosition;
            Vector3 dir = (pb - pa).normalized;
            dir = arrowRotation * dir;

            if (parentTransform != null)
            {
                pa = parentTransform.TransformPoint(pa);
                pb = parentTransform.TransformPoint(pb);
                pc = parentTransform.TransformPoint(pc);
                dir = parentTransform.TransformDirection(dir);
            }

            Gizmos.DrawLine(pa, pb);
            Gizmos.DrawLine(pc - dir * arrowSize, pc);
        }

        public void DrawFaceGizmo(Face f, Color? color = null)
        {
            Gizmos.color = (color == null ? defaultFaceColor : color.Value);
            
            HalfEdge e = f.edge;

            do
            {
                Vector2 pa = e.fromVertex.position;
                Vector2 pb = e.next.fromVertex.position;
                Vector2 pc = e.next.next.fromVertex.position;

                // only draw if the triangle is anticlockwise
                DrawTriangleGizmo(pa, pb, pc);
                // if (pa.IsOnLeft(pb, pc))
                // {
                // }
                
                e = e.next;
            }
            while (e != f.edge);

        }

        private Vector3[] triangle = new Vector3[3];

        public void DrawTriangleGizmo(Vector3 pa, Vector3 pb, Vector3 pc)
        {
            if (parentTransform != null)
            {
                pa = parentTransform.TransformPoint(pa);
                pb = parentTransform.TransformPoint(pb);
                pc = parentTransform.TransformPoint(pc);
            }
            
            triangle[0] = pa;
            triangle[1] = pb;
            triangle[2] = pc;

            Handles.DrawAAConvexPolygon(triangle);
        }

    }
}

#endregion
