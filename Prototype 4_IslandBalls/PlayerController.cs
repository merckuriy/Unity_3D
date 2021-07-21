using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f;
    public bool hasPowerup;
    public GameObject powerupIndicator;
    private Rigidbody playerRb;
    private GameObject focalPoint;
    private float powerupStrength = 15.0f;
    private Vector3 startPos;
    [HideInInspector]
    public bool gameOver;
    float startTime1, startTime2;
    private Vector3 initialCheckPos;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
        startPos = playerRb.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float forwardInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * forwardInput * speed * Time.deltaTime*50);
        powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        if(playerRb.position.y < -10 || GetPlayerIsNotMoved())
        {
            playerRb.position = startPos;
            gameOver = true;
            hasPowerup = false;
            powerupIndicator.SetActive(false);
        }
    }

    private bool GetPlayerIsNotMoved()
    {
        if(startTime1 == 0){
            startTime1 = Time.time;
            startTime2 = startTime1;
            initialCheckPos = ExtensionMethods.Round(transform.position);
        }

        // Check moving every 0.5 seconds
        if (Time.time - startTime1 > 0.5){
            if (ExtensionMethods.Round(transform.position) != initialCheckPos)
            {
                startTime2 = Time.time;
                initialCheckPos = ExtensionMethods.Round(transform.position);
            }

            startTime1 = Time.time;
        }

        // If the player doesn't move for more than 10 seconds
        if (Time.time - startTime2 > 10)
        {
            startTime1 = 0;
            return true;
        }

        return false;
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            Destroy(other.gameObject);
            powerupIndicator.SetActive(true);
            StartCoroutine(PowerupCountdownRoutine());
        }
    }

    IEnumerator PowerupCountdownRoutine() {
        yield return new WaitForSeconds(14);
        hasPowerup = false;
        powerupIndicator.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            Rigidbody enemyRigidBody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;

            enemyRigidBody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
        }
    }
}

public static class ExtensionMethods
{
    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        return new Vector3(
            (float)System.Math.Round((double)vector3.x, decimalPlaces),
            (float)System.Math.Round((double)vector3.y, decimalPlaces),
            (float)System.Math.Round((double)vector3.z, decimalPlaces));
    }
}
