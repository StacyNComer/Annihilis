using UnityEngine;

/// <summary>
/// Sets a projectile's velocity when it enters play. 
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ProjectileMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        //Start the projectile's movement.
        rb.velocity = transform.forward * speed;
    }
}
