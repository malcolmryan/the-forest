/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;

namespace WordsOnPlay.Geometry
{

[Serializable]
public class Vertex 
{
    public Vector2 position;
    public HalfEdge edge;
    public string name;

    public Vertex(Vector2 position, string name)
    {
        this.position = position;
        this.name = name;
    }

    public static implicit operator Vector2(Vertex v) => v.position;

    public static Vector2 operator -(Vertex v1, Vertex v2)
    {
        return v1.position - v2.position;
    }

}

}