using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 1;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        rb.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        /*var movement = transform.forward * speed * Time.deltaTime;
        transform.position += movement;

        rb.velocity = movement;*/
    }
}
