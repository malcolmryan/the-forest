/**
 * Player movement
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMove : MonoBehaviour
{

#region Parameters
    [SerializeField] private float speed = 2;
    [SerializeField] private float turnSpeed = 360;
#endregion 

#region Components
    private new Rigidbody2D rigidbody;
#endregion

#region State
    private Vector2 movementDir = Vector2.zero;
#endregion

#region Properties
#endregion

#region Actions
    private Actions actions;
    private InputAction moveAction;
#endregion

#region Events
#endregion

#region Init & Destroy
    void Awake()
    {
        actions = new Actions();
        moveAction = actions.movement.move;

        rigidbody = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        actions.movement.Enable();
    }

    void OnDisble()
    {
        actions.movement.Disable();
    }
#endregion Init

#region Update
    void Update()
    {
        movementDir = moveAction.ReadValue<Vector2>();
    }
#endregion Update

#region FixedUpdate
    void FixedUpdate()
    {        
        rigidbody.velocity = speed * movementDir;

        if (movementDir.sqrMagnitude > 0) {
            float d = Vector2.SignedAngle(transform.up, movementDir);

            if (Mathf.Abs(d) < turnSpeed * Time.fixedDeltaTime)
            {
                rigidbody.rotation = Vector2.SignedAngle(Vector2.up, movementDir);
            }
            else {
                rigidbody.rotation = rigidbody.rotation + Mathf.Sign(d) * turnSpeed * Time.fixedDeltaTime;
            }
        }
    }
#endregion FixedUpdate

#region Gizmos
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Don't run in the editor
            return;
        }
    }
#endregion Gizmos
}
