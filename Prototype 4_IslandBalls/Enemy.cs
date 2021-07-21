using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody enemyRb;
    GameObject player;
    public float speed = 12f;
    // Start is called before the first frame update
    void Start()
    {
        enemyRb = GetComponent<Rigidbody>(); 
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 lookAtDirection = (player.transform.position - transform.position).normalized;
        enemyRb.AddForce(lookAtDirection * speed);

        if (transform.position.y < -10) { Destroy(gameObject); }
    }
}
