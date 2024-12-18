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

public class TreeFactory : MonoBehaviour
{

#region Singleton
    private static TreeFactory instance = null;
    public static TreeFactory Instance { get { return instance; } }
#endregion 

#region Parameters
    [SerializeField] private Rect rect;
    [SerializeField] private int nTrees;
    [SerializeField] private TreeShadow treePrefab;
    [SerializeField] private int nFails = 10;
#endregion 

#region State
    private Vertex[] vertices;
    private KDTree kdTree;
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
    }

    private void GenerateTrees()
    {
        float minDistance = Mathf.Min(rect.width / 2, rect.height / 2);

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

                pos = rect.RandomPoint();

            } while (NearestTreeDistance(pos) < minDistance);

            // create a tree

            TreeShadow tree = Instantiate(
                treePrefab, transform.position, Quaternion.identity, transform);
            tree.transform.localPosition = pos;
            vertices[i] = new Vertex(pos);
            kdTree.AddVertex(vertices[i]);
        }

        kdTree.Rebuild();
    }

    private float NearestTreeDistance(Vector3 pos) 
    {
        // TODO: Replace this with a KD Tree
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            float d = (t.localPosition - pos).magnitude;
            nearest = Mathf.Min(nearest, d);
        }

        return nearest;
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
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        rect.DrawGizmo(transform);

        if (kdTree != null)
        {
            Gizmos.color = Color.yellow;
            kdTree.DrawGizmo(transform);
        }
    }
#endregion Gizmos
}
