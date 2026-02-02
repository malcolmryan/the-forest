/**
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6000.0.53f1
 */

using UnityEngine;
using WordsOnPlay.Utils;
using WordsOnPlay.Geometry;

public class GridFactory : MonoBehaviour
{

#region Parameters
    [SerializeField] private Rect bounds;
    [SerializeField] private int nCells;
    [SerializeField] private Transform pointPrefab;
    [SerializeField] private int nFails = 10;
    [SerializeField] private int rngSeed = 0;
#endregion 

#region Connected Objects
#endregion

#region Components
    private GraphGizmo graphGizmo;
#endregion

#region State
    private System.Random rng;
    private Vector2[] vertices;
    private KDTree kdTree;
    private Triangulation triangulation;
    private Graph graph;
#endregion

#region Properties
    public Graph Graph => graph;
#endregion

#region Events
#endregion

#region Init & Destroy
    void Awake()
    {
        rng = new System.Random(rngSeed);
        BuildMap();

        graphGizmo = GetComponent<GraphGizmo>();    // optional
        if (graphGizmo != null)
        {
            graphGizmo.Graph = triangulation.MakeGraph();
        }
    }

    private void OnApplicationQuit()
    {
        triangulation = null;
        kdTree = null;
    }
#endregion 

#region Map generation
    private void BuildMap()
    {
        int nPoints = (nCells + 1) * (nCells + 1);
        vertices = new Vector2[nPoints];
        kdTree = new KDTree();
        GeneratePoints();
        Triangulate();
//        graph = triangulation.MakeGraph();
    }

    private void GeneratePoints()
    {
        float minDistance = Mathf.Min(bounds.width / 2, bounds.height / 2);

        int k = 0;

        // add points around edge
        for (int i = 0; i <= nCells; i++)
        {
            float x = i * 1f / nCells;

            vertices[k] = bounds.Point(x,0);
            kdTree.AddPoint(vertices[k]);
            k++;

            vertices[k] = bounds.Point(x,1);
            kdTree.AddPoint(vertices[k]);
            k++;
        }

        for (int i = 1; i < nCells; i++)
        {
            float y = i * 1f / nCells;

            vertices[k] = bounds.Point(0,y);
            kdTree.AddPoint(vertices[k]);
            k++;

            vertices[k] = bounds.Point(1,y);
            kdTree.AddPoint(vertices[k]);
            k++;
        }

        // add internal points
        int nPoints = (nCells+1) * (nCells+1);
        for (; k < nPoints; k++)
        {
            Vector3 pos;
            int tries = 0;

            // find a point not too close to the others

            do 
            {
                tries++; 
                if (tries > nFails)
                {
                    tries = 0;
                    minDistance /= 2;
                }

                pos = bounds.RandomPoint(rng);

            } while (NearestDistance(pos) < minDistance);

            // create a point
            vertices[k] = pos;
            kdTree.AddPoint(pos);
        }

        kdTree.Rebuild();

        for (int i = 0; i < vertices.Length; i++)
        {
            Transform point = Instantiate(pointPrefab, transform.position, Quaternion.identity, transform);
            point.transform.localPosition = vertices[i];            
        }
    }

    private float NearestDistance(Vector2 pos) 
    {
        Vector2? v = kdTree.Nearest(pos); 
        return v == null ? float.PositiveInfinity : Vector2.Distance(v.Value, pos);
    }

    private void Triangulate()
    {
        triangulation = new Triangulation(bounds);

        for (int i = 0; i < vertices.Length; i++)
        {
            triangulation.AddVertex(vertices[i], $"V{i}");
        }

        triangulation.Run();
    }
#endregion


#region Update
    void Update()
    {
    }
#endregion

#region Gizmos

    [Header("Gizmos")]
    [SerializeField] private bool drawBoundsGizmo = false;
    [SerializeField] private bool drawKDTreeGizmo = false;
    [SerializeField] private bool drawTriangulationGizmo = false;

    void OnDrawGizmos()
    {
        if (drawBoundsGizmo)
        {
            Gizmos.color = Color.magenta;
            bounds.DrawGizmo(transform);
        }

        if (Application.isPlaying) 
        {
            if (drawKDTreeGizmo && kdTree != null)
            {
                Gizmos.color = Color.yellow;
                kdTree.DrawGizmo(transform);
            }
            
            if (drawTriangulationGizmo && triangulation != null)
            {
                Gizmos.color = Color.cyan;
                triangulation.DrawGizmo(transform);
            }
        }
                
    }
#endregion Gizmos
}
