/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{
    public class VoronoiDiagram
    {   
        private Graph graph;
        public Graph Graph => graph;

        private Dictionary<Face,Vertex> mapFV;  // old face to new vertex
        private Dictionary<Vertex,Face> mapVF;  // old vertex to new face

        public VoronoiDiagram(Triangulation triangulation)
        {
            graph = new Graph();
            MakeVertices(triangulation);
            MakeFaces(triangulation);
            MakeEdges(triangulation);
        }

        private void MakeVertices(Triangulation triangulation)
        {
            mapFV = new Dictionary<Face,Vertex>();

            // make a vertex at the circumcentre of each triangle
            foreach (Triangle t in triangulation)
            {     
                Vertex v = graph.AddVertex(t.Circumcentre, t.Name);
                mapFV[t.face] = v;
            }
        }

        private void MakeFaces(Triangulation triangulation)
        {
            mapVF = new Dictionary<Vertex,Face>();

            Triangle root = triangulation.Root;
            Vertex rA = root.edges[0].fromVertex;
            Vertex rB = root.edges[1].fromVertex;
            Vertex rC = root.edges[2].fromVertex;

            // make a face for every vertex
            foreach (Vertex v in triangulation.Graph.Vertices)
            {     
                if (v == rA || v == rB || v == rC)
                {
                    // Don't create faces for the root vertices
                    mapVF[v] = null;                
                }
                else
                {
                    Face f = graph.AddFace();
                    mapVF[v] = f;                    
                }
            }
        }

        private void MakeEdges(Triangulation triangulation)
        {
            // every vertex has three outgoing edges
            // corresponding to the three neighbouring triangles

            foreach (Triangle t in triangulation)
            {

                //  FROM:                  TO:
                //      F1                   v1
                //    c-----b                 |
                //     \ F /       ===>    Fc v  Fb
                //   F2 \ / F0               / \ 
                //       a                 v2   v0
                //                            Fa

                Face f = t.face;
                Vertex v = mapFV[f];
                (HalfEdge forward, HalfEdge backward)? ePrev = null;
                HalfEdge eOld = f.edge;

                for (int i = 0 ; i < 3; i++)
                {
                    Vertex vOld = eOld.fromVertex;      
                    Face fOld = eOld.flip.face;

                    if (fOld != null)
                    {
                        Vertex vNew = mapFV[fOld];
                        Face fNew = mapVF[vOld];
                        var eNew = graph.GetOrAddEdge(v,vNew);
                        eNew.backward.face = fNew;

                        if (fNew != null && fNew.edge == null)
                        {
                            fNew.edge = eNew.backward;                        
                        }

                        if (v.edge == null)
                        {
                            // configure this edge at the end of the loop
                            vNew.edge = eNew.forward;
                        }
                        
                        if (ePrev != null) 
                        {
                            eNew.backward.next = ePrev.Value.forward;                    
                        }
                        ePrev = eNew;
                        eOld = eOld.next;                                    
                    }
                }

                if (ePrev != null) 
                {
                    // close the loop
                    v.edge.flip.next = ePrev.Value.forward;
                }
            }
        }

        public void DrawGizmos(Transform transform = null)
        {
            DrawGizmosVertices(transform);
            DrawGizmosEdges(transform);
        }

        public void DrawGizmosVertices(Transform transform = null)
        {
            foreach (Vertex v in graph.Vertices)
            {
                Vector3 p = v.position;
                if (transform != null)
                {
                    p = transform.TransformPoint(p);
                }

                Gizmos.DrawSphere(p, 0.1f);
            }
        }

        public void DrawGizmosEdges(Transform transform = null)
        {
            foreach (var e in graph.Edges)
            {
                Vector3 p = e.forward.fromVertex.position;
                Vector3 q = e.backward.fromVertex.position;

                if (transform ! == null)
                {
                    p = transform.TransformPoint(p);
                    q = transform.TransformPoint(q);                    
                }

                Gizmos.DrawLine(p,q);
            }
        }

    }
}