/**
 * Half-edge Graph data structure
 * 
 * Note: This is just a container for keeping track of nodes, edges and faces. 
 * It does not maintain any guarantees about the structure of the graph
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using UnityEngine;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry 
{
    public class Graph
    {
        private HashSet<Vertex> vertices;
        public IEnumerable<Vertex> Vertices => vertices;

        private HashSet<(HalfEdge, HalfEdge)> edges;
        public IEnumerable<(HalfEdge, HalfEdge)> Edges => edges;
        private Dictionary<(Vertex, Vertex), (HalfEdge forward, HalfEdge backward)> edgeMap;

        private HashSet<Face> faces;
        public IEnumerable<Face> Faces => faces;

#region Constructors
        public Graph()
        {
            vertices = new HashSet<Vertex>();
            edges = new HashSet<(HalfEdge, HalfEdge)>();
            edgeMap = new Dictionary<(Vertex, Vertex), (HalfEdge, HalfEdge)>();
            faces = new HashSet<Face>();
        }
#endregion

#region Vertices
        public Vertex AddVertex(Vector2 position, string name)
        {
            Vertex v = new Vertex(position, name);
            vertices.Add(v);
            return v;            
        }

        public bool RemoveVertex(Vertex vertex)
        {
            return vertices.Remove(vertex);
        }
#endregion

#region Face
        public Face AddFace(HalfEdge edge = null)
        {
            Face f = new Face(edge);
            faces.Add(f);
            return f;          
        }

        public bool RemoveFace(Face face)
        {
            return faces.Remove(face);
        }

#endregion

#region Edges
        public (HalfEdge forward, HalfEdge backward) AddEdge(Vertex a, Vertex b) 
        {
            HalfEdge e = new HalfEdge(a);
            e.flip = new HalfEdge(b);
            e.flip.flip = e;

            var edge = (e, e.flip);
            edges.Add(edge);
            edgeMap[(a,b)] = edge;

            return edge;
        }

        public bool RemoveEdge(HalfEdge e)
        {
            return RemoveEdge1(e) || RemoveEdge1(e.flip);
        }

        private bool RemoveEdge1(HalfEdge e)
        {
            var edge = (e, e.flip);
            if (edges.Contains(edge))
            {
                Vertex va = e.fromVertex;
                Vertex vb = e.flip.fromVertex;
                edgeMap.Remove((va,vb));                
                return edges.Remove(edge);
            }

            return false;            
        }

        public bool RemoveEdge(Vertex va, Vertex vb)
        {
            return RemoveEdge1(va,vb) || RemoveEdge1(vb,va);            
        }

        private bool RemoveEdge1(Vertex va, Vertex vb)
        {
            if (edgeMap.ContainsKey((va,vb)))
            {
                return RemoveEdge1(edgeMap[(va,vb)].forward);
            }
            return false;
        }

        public bool HasEdge(Vertex va, Vertex vb)
        {
            return edgeMap.ContainsKey((va, vb)) || edgeMap.ContainsKey((vb, va));
        }

        public (HalfEdge forward, HalfEdge backward)? GetEdge(Vertex va, Vertex vb)
        {
            if (edgeMap.ContainsKey((va, vb)))
            {
                return edgeMap[(va, vb)];
            }
            else if (edgeMap.ContainsKey((vb, va)))
            {
                var e = edgeMap[(vb, va)];
                return (e.backward, e.forward); // flip the edge to match the vertex ordering
            }

            return null;
        }

        public (HalfEdge forward, HalfEdge backward) GetOrAddEdge(Vertex va, Vertex vb)
        {
            var edge = GetEdge(va, vb);
            if (edge == null)
            {
                return AddEdge(va, vb);
            }
            return edge.Value;
        }

#endregion


    }
}