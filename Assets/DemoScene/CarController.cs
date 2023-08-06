using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    Rigidbody rb;

    [Header("Movement Variables")]
    public Vector2 moveVector;
    public float moveVelocity;
    public float accel;
    public float turnVelocity;

    [Header("Suspension Variables")]
    public Transform[] wheels;
    public float suspensionHeight = 1f;
    public float maxSuspensionForce;
    public LayerMask groundLayer;
    public float sideFric;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        foreach (Transform w in wheels)
        {
            RaycastHit hit;
            Debug.DrawRay(w.position, -w.up, Color.green);
            if (Physics.Raycast(w.position, -w.up, out hit, suspensionHeight, groundLayer))
            {
                float suspensionForce = (1 - (hit.distance / suspensionHeight)) * maxSuspensionForce;
                print("Force: " + Vector3.up * suspensionForce);
                rb.AddForceAtPosition(Vector3.up * suspensionForce, w.position);
            }
        }

        rb.AddForce(transform.forward * moveVector.y * moveVelocity);

        rb.AddTorque(0f, turnVelocity * moveVector.x * Time.fixedDeltaTime, 0f);

        rb.AddForce(-rb.velocity.x * sideFric, 0f, 0f);
    }
}