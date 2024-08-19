/**
 *
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;

public class CameraFollow : MonoBehaviour
{

#region Parameters
    [SerializeField] private Transform target;
#endregion 

#region Init & Destroy
    void Awake()
    {
        transform.position = target.position;
    }
#endregion Init

#region Update
    void LateUpdate()
    {
        transform.position = target.position;
    }
#endregion Update


}
