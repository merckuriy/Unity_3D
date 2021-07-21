using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeKartExt2 : MonoBehaviour
{
    ArcadeKart kart;
    [HideInInspector]
    public Vector3 LastPinPosition;
    public Quaternion LastPinRotation;
    // Start is called before the first frame update
    void Start()
    {
        kart = GetComponent<ArcadeKart>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        BackToTrack();
    }

    void BackToTrack()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetKart();
        }

        if (kart.Rigidbody.transform.position.y < -10) {
            ResetKart();
        }
    }

    void ResetKart()
    {
        // Reset the agent back to its last known agent checkpoint
        transform.localRotation = LastPinRotation;
        transform.position = LastPinPosition;
        kart.Rigidbody.velocity = default;
    }

    public void OnCollect(GameObject other)
    {
        LastPinPosition = other.transform.position;
        LastPinRotation = transform.rotation;
        LastPinPosition.y += 1;
    }
}
