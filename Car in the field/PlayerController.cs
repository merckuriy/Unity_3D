using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float speed = 100.0f;
    private float zBound = 6.0f;
    private Rigidbody playerRb;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        ConstrainMovement();
    }

    // Move the player based on arrow key input
    void MovePlayer()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        playerRb.AddForce(Vector3.forward * speed * verticalInput);
        playerRb.AddForce(Vector3.right * speed * horizontalInput);
    }

    // Prevent the player from leaving the top and bottom of the screen
    void ConstrainMovement()
    {
        if (transform.position.z < -zBound)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -zBound);
        }

        if (transform.position.z > zBound)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zBound);
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.CompareTag("Enemy")){
            Debug.Log("Player has collided with enemy.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup")){
            Destroy(other.gameObject);
        }
    }
}
