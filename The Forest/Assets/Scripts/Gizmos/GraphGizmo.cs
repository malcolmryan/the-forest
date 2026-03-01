/**
 * Graph Gizmo
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using System.Collections.Generic;
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

        [Header("Verify")]
        [SerializeField] bool verifyVertices = true;
        [SerializeField] bool verifyEdges = true;
        [SerializeField] bool verifyFaces = true;
        [SerializeField] Color badColor = Color.red;

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

        private Dictionary<Vertex,string> vertexErrors; 
        private Dictionary<HalfEdge,string> edgeErrors; 
        private Dictionary<Face,string> faceErrors; 
#endregion

#region Init
    public void Awake()
    {
        vertexErrors = new Dictionary<Vertex,string>();
        edgeErrors = new Dictionary<HalfEdge,string>(); 
        faceErrors = new Dictionary<Face,string>();         
    }

#endregion

#region Gizmos
        public void OnDrawGizmos()
        {
            if (graph == null)
            {
                return;
            }

            VerifyGraph();

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

        private void VerifyGraph()
        {
            vertexErrors.Clear();
            edgeErrors.Clear();
            faceErrors.Clear();

            if (verifyVertices)
            {
                GraphOperations.VerifyVertices(graph, vertexErrors);
            }

            if (verifyEdges)
            {
                GraphOperations.VerifyEdges(graph, edgeErrors);
            }

            if (verifyFaces)
            {
                GraphOperations.VerifyFaces(graph, faceErrors);
            }

        }

        public void DrawVertexGizmo(Vertex v, Color? color = null)
        {
            Gizmos.color = (color == null ? defaultVertexColor : color.Value);
    
            if (vertexErrors[v] != null)
            {
                color = badColor;
            }

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
                string label;
                if (vertexErrors[v] == null)
                {
                    label = v.name;
                }
                else
                {
                    label = $"{v.name}: {vertexErrors[v]}";
                }
                Handles.Label(p, label);                                
            }
        }    

        public void DrawHalfEdgeGizmo(HalfEdge e, Color? color = null)
        {
            Gizmos.color = (color == null ? defaultEdgeColor : color.Value);

            if (edgeErrors[e] != null)
            {
                color = badColor;
            }

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
            
            if (faceErrors[f] != null)
            {
                color = badColor;
            }

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
