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

public class TriangulationTest : MonoBehaviour
{

#region Parameters
    [SerializeField] private Rect bounds;
#endregion 

#region State
    private Triangulation triangulation;
    private Queue<Vertex> vertexQueue;
    private IEnumerator coroutine;
#endregion

#region Init & Destroy
    void Awake()
    {
        triangulation = new Triangulation(bounds);

        for (int i = 0; i < transform.childCount; i++) 
        {
            Transform child = transform.GetChild(i);
            AddVertex(child.position, child.gameObject.name);
        }
    }
#endregion 

#region Update
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(transform.forward, transform.position);

            Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition); 

            float t = 0;
            
            if (plane.Raycast(ray, out t)) {
                AddVertex(ray.GetPoint(t));
            }
        }
    }

    private void AddVertex(Vector3 point, string name = "V") 
    {
        Vector2 p = transform.InverseTransformPoint(point);
        Vertex v = new Vertex(p, name);

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
                triangulation.DrawVoronoiGizmo(transform);
            }
        }
                
    }

#endregion 
}
