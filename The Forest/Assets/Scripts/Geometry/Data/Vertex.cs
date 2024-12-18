/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;

namespace WordsOnPlay.Geometry
{

public class Vertex 
{
    public Vector2 position;
    public HalfEdge edge;

    public Vertex(Vector2 position)
    {
        this.position = position;
    }
}

}