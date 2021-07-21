using KartGame.AI;
using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeKartExt : MonoBehaviour
{
    ArcadeKart kart;
    private Collider[] Colliders;
    public ushort InitCheckpointIndex;
    int checkpointIndex;
    [HideInInspector]
    public int lapsCount = 0;
    KartAgent[] agents;

    void Start()
    {
        kart = GetComponent<ArcadeKart>();
        Colliders = GameObject.Find("Track_Training").GetComponent<DebugCheckpointRay>().Colliders;

        agents = FindObjectsOfType<KartAgent>();
    }
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
        Transform checkpoint = Colliders[checkpointIndex].transform;
        transform.localRotation = checkpoint.rotation;
        transform.position = checkpoint.position;
        kart.Rigidbody.velocity = default;
    }

    void OnTriggerEnter(Collider other)
    {
        FindCheckpointIndex(other, out int index);

        if (other.gameObject.layer == LayerMask.NameToLayer("TrainingCheckpoints") && (index > checkpointIndex || index == 0 && checkpointIndex == Colliders.Length - 1)) {
            if (index == 0) {
                lapsCount++;
            }
            checkpointIndex = index;
        }
    }

    void FindCheckpointIndex(Collider checkPoint, out int index)
    {
        for (int i = 0; i < Colliders.Length; i++) {
            if (Colliders[i].GetInstanceID() == checkPoint.GetInstanceID()) {
                index = i;
                return;
            }
        }
        index = -1;
    }

    public ushort getPosition()
    {
        ushort pos = 1;

        foreach (KartAgent agent in agents) {
            if (agent.GetLapsCount() < lapsCount) {
                continue;
            }else if(agent.GetLapsCount() > lapsCount) {
                pos++;
                continue;
            }   

            if(agent.GetCheckpoint() > checkpointIndex) {
                pos++;
            }else if(agent.GetCheckpoint() == checkpointIndex) {
                var nextCheckpoint = (checkpointIndex + 1) % Colliders.Length;
                var nextCollider = Colliders[nextCheckpoint];
                float distance = (nextCollider.transform.position - kart.transform.position).magnitude;
                if (agent.GetNextCheckpointDistance() < distance) {
                    pos++;
                }
            }
        }

        return pos;
    }
}
