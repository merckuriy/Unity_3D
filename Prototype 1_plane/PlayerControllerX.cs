using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    private float speed = 20.0f;
    private float rotationSpeed = 100.0f;
    private float verticalInput;
    private Rigidbody playerRb;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // get the user's vertical input
        verticalInput = Input.GetAxis("Vertical");

        // move the plane forward at a constant rate
        //transform.Translate(Vector3.forward * Time.deltaTime * speed); // Телепортация (Самолёт будет пролетать сквозь объекты).
        //playerRb.AddRelativeForce(Vector3.forward * speed/100); // Сохраняет инерцию.
        playerRb.velocity = transform.forward * speed; // Постоянная скорость относительно локального направления.

        // tilt the plane up/down based on up/down arrow keys
        transform.Rotate(Vector3.left, rotationSpeed * Time.deltaTime * verticalInput);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Sphere"))
        {
            playerRb.AddForce(-transform.forward * 350, ForceMode.Impulse);
        }
    }
}
