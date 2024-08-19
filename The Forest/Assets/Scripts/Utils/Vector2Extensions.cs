using UnityEngine;
using System.Collections;

/**
 * Extension methods for the Vector2 class
 */

namespace WordsOnPlay.Utils {
public static class Vector2Extensions  {

	/**
	 * Test if vector v1 is on the left of v2
	 */

	public static bool IsOnLeft(this Vector2 v1, Vector2 v2) {
		return v1.x * v2.y < v1.y * v2.x;
	}

	/**
	 * Rorate a 2D vector anticlockwise by the given angle (in degrees)
	 */

	public static Vector2 Rotate(this Vector2 v, float angle) {
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		return q * v;
	}

	/**
	 * Project this vector onto another
	 */
	public static Vector2 Project(this Vector2 v, Vector2 onto) {
		Vector2 u = onto.normalized;
		return Vector2.Dot(v, u) * u;
	}

}
}