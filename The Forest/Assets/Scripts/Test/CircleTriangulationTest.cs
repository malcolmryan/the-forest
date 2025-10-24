/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using WordsOnPlay.Utils;
using WordsOnPlay.Geometry;
using System.Collections;
using System.Collections.Generic;

public class CircleTriangulationTest : MonoBehaviour
{

#region Parameters
    [SerializeField] private Rect bounds;
    [SerializeField] private int rngSeed;
    [SerializeField] private int nPoints;    
#endregion 

#region State
    private System.Random rng;
    private Triangulation triangulation;
    private Queue<Vertex> vertexQueue;
    private IEnumerator coroutine;
#endregion

#region Init & Destroy
    void Awake()
    {
        rng = new System.Random(rngSeed);
        triangulation = new Triangulation(bounds);
        Vector2 p;
        for (int i = 0; i < nPoints; i++)
        {
            p = Vector3.right;
            float angle = (float)rng.NextDouble() * 360;
            Debug.Log($"angle_{i} = {angle}");
            // 11 = 168.2333
            // 57 = 264.7431
            // 18 = 243.7852
            // 68 = 297.6532
            p = p.Rotate(angle);
            AddVertex(p, $"V_{i}");
        }
    }

    private void AddVertex(Vector2 point, string name = "V") 
    {
        Vertex v = new Vertex(point, name);

        triangulation.EnqueueVertex(v);

        if (!triangulation.IsRunning)
        {
            StartCoroutine(triangulation.RunCR());
        }
    }

#endregion 

#region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        bounds.DrawGizmo(transform);

        if (Application.isPlaying) 
        {
            if (triangulation != null)
            {
                triangulation.DrawGizmo(transform);
            }
        }
                
    }

#endregion 
}
