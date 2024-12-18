/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;

namespace WordsOnPlay.Geometry
{
public class HalfEdge 
{
    public Vertex vertex;
    public HalfEdge prev;
    public HalfEdge next;
    public HalfEdge opposite;
    public Face face;
}
}
