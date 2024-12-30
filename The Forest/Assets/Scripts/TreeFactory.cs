/**
 *
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using WordsOnPlay.Utils;
using WordsOnPlay.Geometry;
using System.Collections;

public class TreeFactory : MonoBehaviour
{

#region Singleton
    private static TreeFactory instance = null;
    public static TreeFactory Instance { get { return instance; } }
#endregion 

#region Parameters
    [SerializeField] private Rect bounds;
    [SerializeField] private int nTrees;
    [SerializeField] private TreeShadow treePrefab;
    [SerializeField] private int nFails = 10;
#endregion 

#region State
    private Vertex[] vertices;
    private KDTree kdTree;
    private Triangulation triangulation;
#endregion

#region Init & Destroy
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are multiple TreeFactories in the Scene.");
        }
        instance = this;

        vertices = new Vertex[nTrees];
        kdTree = new KDTree();
        GenerateTrees();
        Triangulate();
    }

    private void GenerateTrees()
    {
        float minDistance = Mathf.Min(bounds.width / 2, bounds.height / 2);

        for (int i = 0; i < nTrees; i++)
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

                pos = bounds.RandomPoint();

            } while (NearestTreeDistance(pos) < minDistance);

            // create a tree

            TreeShadow tree = Instantiate(
                treePrefab, transform.position, Quaternion.identity, transform);
            tree.transform.localPosition = pos;
            vertices[i] = new Vertex(pos, $"T{i}");
            kdTree.AddVertex(vertices[i]);
        }

        kdTree.Rebuild();
    }

    private float NearestTreeDistance(Vector2 pos) 
    {
        if (kdTree.Count == 0)
        {
            return float.PositiveInfinity;
        }
        Vertex v = kdTree.Nearest(pos);
        return Vector2.Distance(v.position, pos);
    }

    private void Triangulate()
    {
        triangulation = new Triangulation(bounds);
        for (int i = 0; i < vertices.Length; i++)
        {
            triangulation.AddVertex(vertices[i]);
            triangulation.FlipEdges();
        }
    }

    private void OnApplicationQuit()
    {
        triangulation = null;
        kdTree = null;
    }
#endregion Init

#region Public Methods
    public Vector2 NearestTree(Vector2 pos) 
    {
        pos = transform.InverseTransformPoint(pos);
        Vertex v = kdTree.Nearest(pos);
        return transform.TransformPoint(v.position);
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
            Gizmos.color = Color.green;
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
