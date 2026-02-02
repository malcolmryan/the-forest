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

[RequireComponent(typeof(GraphGizmo))]
public class TriangulationTest : MonoBehaviour
{

#region Parameters
    [SerializeField] private Rect bounds;
    [SerializeField] private bool drawTriangulationGizmo = false;
    [SerializeField] private bool removeRoot = true;
#endregion 

#region State
    private int nVertices = 0;
    private Triangulation triangulation;
    private Queue<Vertex> vertexQueue;
    private IEnumerator coroutine;
#endregion

#region Components
    private GraphGizmo graphGizmo;
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

        graphGizmo = GetComponent<GraphGizmo>();
        graphGizmo.Graph = removeRoot ? triangulation.MakeGraph() : triangulation.Graph;
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
                graphGizmo.Graph = removeRoot ? triangulation.MakeGraph() : triangulation.Graph;
            }
        }
    }

    private void AddVertex(Vector3 point, string name = null) 
    {
        if (name == null)
        {
            name = $"V{nVertices}";
        }
        Vector2 p = transform.InverseTransformPoint(point);
        triangulation.AddVertex(p, name);

        if (!triangulation.IsRunning)
        {
            StartCoroutine(triangulation.RunCR());
        }
        nVertices++;
    }

#endregion 

#region Gizmos
    void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        bounds.DrawGizmo(transform);

        if (Application.isPlaying && drawTriangulationGizmo) 
        {
            if (triangulation != null)
            {
                triangulation.DrawGizmo(transform);
            }
        }
                
    }

#endregion 
}
