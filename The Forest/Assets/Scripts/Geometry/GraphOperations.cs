/**
 * Operations on Graph strucutre
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{
    public static class GraphOperations
    {

#region Clone
        public static Graph Clone(Graph graph)
        {
            Graph copy = new Graph();

            Dictionary<Vertex, Vertex> vMap = CloneVertices(copy, graph);
            Dictionary<HalfEdge, HalfEdge> eMap = CloneEdges(copy, graph, vMap);
            Dictionary<Face, Face> fMap = CloneFaces(copy, graph, eMap);

            // connect them together
            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = vMap[vOld];
                HalfEdge eOld = vOld.edge;

                if (vOld.edge != null)
                {
                    HalfEdge eNew = eMap[eOld];
                    vNew.edge = eNew;

                    do
                    {
                        eNew.next = eMap[eOld.next];
                        eNew.flip = eMap[eOld.flip];
                        eNew.face = fMap[eOld.face];

                        eOld = eOld.flip.next;
                        eNew = eMap[eOld];
                    } while (eOld != vOld.edge);
                }
            }
            return copy;
        }

        private static Dictionary<Vertex, Vertex> CloneVertices(Graph copy, Graph graph)
        {
            Dictionary<Vertex, Vertex> vMap = new Dictionary<Vertex, Vertex>();

            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = copy.AddVertex(vOld.position, vOld.name);
                vMap[vOld] = vNew;
            }

            return vMap;
        }

        private static Dictionary<HalfEdge, HalfEdge> CloneEdges(Graph copy, Graph graph, Dictionary<Vertex, Vertex> vMap)
        {
            Dictionary<HalfEdge, HalfEdge> eMap = new Dictionary<HalfEdge, HalfEdge>();

            // copy outgoing edges from each vertex 
            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = vMap[vOld];

                if (vOld.edge != null)
                {
                    HalfEdge eOld = vOld.edge;

                    do
                    {
                        HalfEdge eNew = copy.AddEdge(vNew);
                        eMap[eOld] = eNew;

                        eOld = eOld.flip.next;
                    } while (eOld != vOld.edge);
                }
            }

            return eMap;
        }

        private static Dictionary<Face, Face> CloneFaces(Graph copy, Graph graph, Dictionary<HalfEdge, HalfEdge> eMap)
        {
            Dictionary<Face, Face> fMap = new Dictionary<Face, Face>();
            fMap[graph.Exterior] = copy.Exterior;

            foreach (Face fOld in graph.Faces)
            {
                Face fNew = copy.AddFace(eMap[fOld.edge]);
                fMap[fOld] = fNew;
            }

            return fMap;
        }
#endregion

#region Vertex lookup
        /// <summary>
        /// Lookup a vertex in the graph by name.
        /// Note: this is slow, because I don't want to keep a dictionary
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        public static Vertex FindVertex(Graph graph, string name)
        {
            foreach (Vertex v in graph.Vertices)
            {
                if (v.name.Equals(name))
                {
                    return v;
                }
            }

            return null;
        }
#endregion

#region Changing graph structure

        /// <summary>
        /// Delete a vertex and all connected edges
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vertex"></param>

        public static void DeleteVertex(Graph graph, Vertex vertex)
        {
            while (vertex.edge != null)
            {
                DeleteEdge(graph, vertex.edge);
            }

            graph.RemoveVertex(vertex);            
        }

        /// <summary>
        /// Delete an edge from the graph. Merge the faces on either side.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="edge"></param>

        public static void DeleteEdge(Graph graph, HalfEdge edge)
        {
            DeleteHalfEdge(graph, edge);
            DeleteHalfEdge(graph, edge.flip);
            MergeFaces(graph, edge);
        }

        private static void DeleteHalfEdge(Graph graph, HalfEdge edge)
        {
            HalfEdge ePrev = PreviousEdge(graph, edge);
            ePrev.next = edge.flip.next;

            if (edge.fromVertex.edge == edge)
            {
                // set to null if this is the last outgoing edge
                edge.fromVertex.edge = edge.flip.next == edge ? null : edge.flip.next;
            }

            if (edge.face.edge == edge)
            {
                // set to null if this is the last edge
                edge.face.edge = edge.next == edge ? null : edge.next;
            }

            graph.RemoveEdge(edge);
        }

        private static void MergeFaces(Graph graph, HalfEdge edge)
        {
            // Don't delete the exterior face

            Face keptFace = edge.face;
            Face deletedFace = edge.flip.face;

            if (deletedFace == graph.Exterior)
            {
                keptFace = graph.Exterior;
                deletedFace = edge.face;
            }

            if (deletedFace != keptFace)
            {
                HalfEdge e = deletedFace.edge;                
                do
                {
                    e.face = keptFace;
                    e = e.next;
                }
                while (e != deletedFace.edge);

                graph.RemoveFace(deletedFace);
            }
        }

        private static HalfEdge PreviousEdge(Graph graph, HalfEdge edge)
        {
            HalfEdge ePrev = edge;

            while (ePrev.next != edge)
            {
                ePrev = ePrev.next;
            }

            return ePrev;
        }

#endregion

    }

}