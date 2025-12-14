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
    [SerializeField] private int rngSeed = 0;
#endregion 

#region State
    private System.Random rng;
    private Vector2[] vertices;
    private KDTree kdTree;
    private Triangulation triangulation;
    private VoronoiDiagram voronoi;
#endregion

#region Init & Destroy
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are multiple TreeFactories in the Scene.");
        }
        instance = this;

        rng = new System.Random(rngSeed);
        BuildMap();
    }

    private void OnApplicationQuit()
    {
        triangulation = null;
        kdTree = null;
    }

#endregion Init

#region Map generation
    private void BuildMap()
    {
        vertices = new Vector2[nTrees];
        kdTree = new KDTree();
        GenerateTrees();
        Triangulate();
        voronoi = new VoronoiDiagram(triangulation);
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

                pos = bounds.RandomPoint(rng);

            } while (NearestTreeDistance(pos) < minDistance);

            // create a tree

            TreeShadow tree = Instantiate(
                treePrefab, transform.position, Quaternion.identity, transform);
            tree.transform.localPosition = pos;
            vertices[i] = pos;
            kdTree.AddPoint(pos);
        }

        kdTree.Rebuild();
    }

    private float NearestTreeDistance(Vector2 pos) 
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


#region Public Methods
    public Vector2? NearestTree(Vector2 pos) 
    {
        pos = transform.InverseTransformPoint(pos);
        Vector2? v = kdTree.Nearest(pos);       
        return v == null ? null : transform.TransformPoint(v.Value);
    }
#endregion

#region Gizmos

    [Header("Gizmos")]
    [SerializeField] private bool drawBoundsGizmo = false;
    [SerializeField] private bool drawKDTreeGizmo = false;
    [SerializeField] private bool drawTriangulationGizmo = false;
    [SerializeField] private bool drawVoronoi = true;

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

            if (drawVoronoi && triangulation != null)
            {
                Gizmos.color = Color.magenta;
                voronoi.DrawGizmos();
            }
        }
                
    }
#endregion Gizmos
}
