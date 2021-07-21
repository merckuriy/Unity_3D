using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float rpm;
    [SerializeField] float horsePower = 0;
    [SerializeField] float turnSpeed = 45.0f;
    private float horizontalInput;
    private float forwardInput;
    private Rigidbody playerRb;
    public GameObject centerOfMass;
    public TextMeshProUGUI speedometerText;
    public TextMeshProUGUI rpmText;
    public List<WheelCollider> allWheels;
    [SerializeField] int wheelsOnGround;
    GameObject player;

    private Vector3 startPos;
    private Quaternion startRot;


    private void Start()
    {
        player = GameObject.Find("Player");
        playerRb = GetComponent<Rigidbody>();
        playerRb.centerOfMass = centerOfMass.transform.position;

        startPos = player.transform.position;
        startRot = player.transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        if (IsOnGround())
        {
            // Move the vehicle 
            //transform.Translate(Vector3.forward * Time.deltaTime * horsePower/1000 * forwardInput);
            playerRb.AddRelativeForce(Vector3.forward * horsePower * forwardInput);
            // Поворот влево/вправо
            transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);

            speed = Mathf.RoundToInt(playerRb.velocity.magnitude * 3.6f); // for miles per hour (mph) * 2.237f
            speedometerText.SetText("Speed: " + speed + "kph");

            rpm = Mathf.RoundToInt((speed % 30) * 80);
            rpmText.SetText("RPM: " + rpm);
        } else {
            if(player.transform.position.y < -100){
                player.transform.position = startPos;
                player.transform.rotation = startRot;
            }
        }

        if (Input.GetKeyDown("r"))
        {
            player.transform.position = startPos;
            player.transform.rotation = startRot;
        }
    }

    bool IsOnGround()
    {
        wheelsOnGround = 0;
        foreach(WheelCollider wheel in allWheels)
        {
            // Фикс для остановленного вращения окружности WheelCollider. Иначе при старте машина вместе езды крутится по оси Y.
            wheel.motorTorque = forwardInput;

            if (wheel.isGrounded)
            {
                wheelsOnGround++;
            }
        }

        return wheelsOnGround > 1;
    }
}
