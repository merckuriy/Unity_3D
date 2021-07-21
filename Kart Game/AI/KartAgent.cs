using Unity.MLAgents;
using KartGame.KartSystems;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace KartGame.AI
{
    /// <summary>
    /// Sensors hold information such as the position of rotation of the origin of the raycast and its hit threshold
    /// to consider a "crash".
    /// </summary>
    [System.Serializable]
    public struct Sensor
    {
        public Transform Transform;
        public float HitThreshold;
    }

    /// <summary>
    /// We only want certain behaviours when the agent runs.
    /// Training would allow certain functions such as OnAgentReset() be called and execute, while Inferencing will
    /// assume that the agent will continuously run and not reset.
    /// </summary>
    public enum AgentMode
    {
        Training,
        Inferencing
    }

    /// <summary>
    /// The KartAgent will drive the inputs for the KartController.
    /// </summary>
    public class KartAgent : Agent, IInput
    {
        /// <summary>
        /// How many actions are we going to support when we use our own custom heuristic? Right now we want the X/Y
        /// axis for acceleration and steering.
        /// </summary>
        //const int LocalActionSize = 2;

        #region Training Modes
        [Tooltip("Are we training the agent or is the agent production ready?")]
        public AgentMode Mode = AgentMode.Training;
        [Tooltip("What is the initial checkpoint the agent will go to? This value is only for inferencing.")]
        public ushort InitCheckpointIndex;
        #endregion

        #region Senses
        [Header("Observation Params")]
        [Tooltip("How far should the agent shoot raycasts to detect the world?")]
        public float RaycastDistance;
        [Tooltip("What objects should the raycasts hit and detect?")]
        public LayerMask Mask;
        [Tooltip("Sensors contain ray information to sense out the world, you can have as many sensors as you need.")]
        public Sensor[] Sensors;

        [Header("Checkpoints")]
        [Tooltip("What are the series of checkpoints for the agent to seek and pass through?")]
        public Collider[] Colliders;
        [Tooltip("What layer are the checkpoints on? This should be an exclusive layer for the agent to use.")]
        public LayerMask CheckpointMask;

        [Space]
        [Tooltip("Would the agent need a custom transform to be able to raycast and hit the track? " +
            "If not assigned, then the root transform will be used.")]
        public Transform AgentSensorTransform;
        #endregion

        #region Rewards
        [Header("Rewards")]
        [Tooltip("What penatly is given when the agent crashes?")]
        public float HitPenalty = -1f;
        [Tooltip("How much reward is given when the agent successfully passes the checkpoints?")]
        public float PassCheckpointReward;
        [Tooltip("Should typically be a small value, but we reward the agent for moving in the right direction.")]
        public float TowardsCheckpointReward;
        [Tooltip("Typically if the agent moves faster, we want to reward it for finishing the track quickly.")]
        public float SpeedReward;
        #endregion

        #region ResetParams
        [Header("Inference Reset Params")]
        [Tooltip("What is the unique mask that the agent should detect when it falls out of the track?")]
        public LayerMask OutOfBoundsMask;
        [Tooltip("What are the layers we want to detect for the track and the ground?")]
        public LayerMask TrackMask;
        [Tooltip("How far should the ray be when casted? For larger karts - this value should be larger too.")]
        public float GroundCastDistance;
        #endregion

        #region Debugging
        [Header("Debug Option")]
        [Tooltip("Should we visualize the rays that the agent draws?")]
        public bool ShowRaycasts;
        #endregion

        ArcadeKart kart;
        float acceleration;
        float steering;
        //float[] localActions;
        // Current last passed checkpoint
        int checkpointIndex;
        float timePassed, timePassed_2;
        float prevDistance;
        int lapsCount = 0;

        //public override void Initialize()
        //{
        //    Debug.Log("Initialize");
        //    base.Initialize();

        //    if (Mode == AgentMode.Inferencing)
        //    {
        //        checkpointIndex = InitCheckpointIndex;
        //    }
        //}

        void Awake()
        {
            kart = GetComponent<ArcadeKart>();
            if (AgentSensorTransform == null)
            {
                AgentSensorTransform = transform;
            }
        }

        void Start()
        {
            //localActions = new float[LocalActionSize];

            // If the agent is training, then at the start of the simulation, pick a random checkpoint to train the agent.
            //AgentReset(); 
            //OnEpisodeBegin();

            if (Mode == AgentMode.Inferencing)
            {
                checkpointIndex = InitCheckpointIndex;
            }
        }

        void LateUpdate()
        {
            BackToTrack();

            switch (Mode)
            {
                case AgentMode.Inferencing:
                    if (ShowRaycasts)
                    {
                        Debug.DrawRay(transform.position, Vector3.down * GroundCastDistance, Color.cyan);
                    }
                    // We want to place the agent back on the track if the agent happens to launch itself outside of the track.
                    if (Physics.Raycast(transform.position, Vector3.down, out var hit, GroundCastDistance, TrackMask)
                        && ((1 << hit.collider.gameObject.layer) & OutOfBoundsMask) > 0)
                    {
                        Debug.Log("Ground trigger");
                        ResetKart();
                    }

                    if (kart.GetCanMove())
                    {
                        // Reset the kart if it doesn't move
                        if (kart.Rigidbody.velocity.magnitude < 1.0f) {
                            timePassed += Time.deltaTime;
                        } else {
                            timePassed = 0;
                        }
                        
                        if (timePassed > 0.7f) {
                            ResetKart();
                            timePassed = 0;
                        }

                        // Reset the car if it moves from in the opposite direction
                        var next = (checkpointIndex + 1) % Colliders.Length;
                        var nextCollider = Colliders[next];
                        float distance = (nextCollider.transform.position - kart.transform.position).magnitude;
                        if (distance > prevDistance) {
                            prevDistance = distance;
                            timePassed_2 += Time.deltaTime;
                        }else{
                            timePassed_2 = 0;
                        }

                        if (timePassed_2 > 0.5f){
                            ResetKart();
                            timePassed_2 = 0;
                            prevDistance = 0;
                        }
                    }

                    break;
            }
        }

        void BackToTrack()
        {
            if (kart.Rigidbody.transform.position.y < -10) {
                ResetKart();
            }
        }

        public int GetCheckpoint()
        {
            return checkpointIndex;
        }

        public int GetLapsCount()
        {
            return lapsCount;
        }

        public float GetNextCheckpointDistance()
        {
            var next = (checkpointIndex + 1) % Colliders.Length;
            var nextCollider = Colliders[next];
            return (nextCollider.transform.position - kart.transform.position).magnitude;
        }

        //public bool IsInTheAir()
        //{
        //    return (kart.AirPercent >= 1);
        //}

        void ResetKart()
        {
            // Reset the agent back to its last known agent checkpoint
            Transform checkpoint = Colliders[checkpointIndex].transform;
            transform.localRotation = checkpoint.rotation;
            transform.position = checkpoint.position;
            kart.Rigidbody.velocity = default;
            acceleration = steering = 0f;
        }

        void OnTriggerEnter(Collider other)
        {

            int maskedValue = 1 << other.gameObject.layer;
            int triggered = maskedValue & CheckpointMask;

            FindCheckpointIndex(other, out int index);

            // Ensure that the agent touched the checkpoint and the new index is greater than the m_CheckpointIndex.
            if (triggered > 0 && (index > checkpointIndex || index == 0 && checkpointIndex == Colliders.Length - 1))
            {
                if(index == 0) {
                    lapsCount++;
                }
                AddReward(PassCheckpointReward);
                checkpointIndex = index;
            }
        }

        void FindCheckpointIndex(Collider checkPoint, out int index)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                if (Colliders[i].GetInstanceID() == checkPoint.GetInstanceID())
                {
                    index = i;
                    return;
                }
            }
            index = -1;
        }

        //float Sign(float value)
        int Sign(float value)
        {
            if (value > 0)
            {
                return 1;
            } else if (value < 0)
            {
                return -1;
            }
            return 0;
        }

        //void InterpretDiscreteActions(float[] actions)
        //{
        //    steering     = actions[0] - 1f;
        //    acceleration = Mathf.FloorToInt(actions[1]) == 1 ? 1 : 0;
        //}
        void InterpretDiscreteActions(ActionBuffers actions)
        {
            //Debug.Log("Interpret: steering: " + actions.DiscreteActions[0] + ", acceleration: " + actions.DiscreteActions[1]);
            //steering = actions.ContinuousActions[0] - 1f;
            //acceleration = Mathf.FloorToInt(actions.ContinuousActions[1]) == 1 ? 1 : 0;

            //steering = actions.DiscreteActions[0] - 1;
            steering = actions.DiscreteActions[0] - 1;
            acceleration = actions.DiscreteActions[1] == 1 ? 1 : 0;
        }

        //public override void collectObservations()
        public override void CollectObservations(VectorSensor sensor)
        {
            //Debug.Log("CollectObservations");
            //AddVectorObs(kart.LocalSpeed());
            sensor.AddObservation(kart.LocalSpeed());

            // Add an observation for direction of the agent to the next checkpoint.
            int next = (checkpointIndex + 1) % Colliders.Length;
            Collider nextCollider = Colliders[next];
            if (nextCollider == null) return;

            Vector3 direction = (nextCollider.transform.position - kart.transform.position).normalized;
            //AddVectorObs(Vector3.Dot(kart.Rigidbody.velocity.normalized, direction));
            sensor.AddObservation(Vector3.Dot(kart.Rigidbody.velocity.normalized, direction));

            if (ShowRaycasts)
            {
                Debug.DrawLine(AgentSensorTransform.position, nextCollider.transform.position, Color.magenta);
            }

            for (int i = 0; i < Sensors.Length; i++)
            {
                var current = Sensors[i];
                var xform = current.Transform;
                var hit = Physics.Raycast(AgentSensorTransform.position, xform.forward, out var hitInfo,
                    RaycastDistance, Mask, QueryTriggerInteraction.Ignore);

                if (ShowRaycasts)
                {
                    Debug.DrawRay(AgentSensorTransform.position, xform.forward * RaycastDistance, Color.green);
                    Debug.DrawRay(AgentSensorTransform.position, xform.forward * RaycastDistance * current.HitThreshold, Color.red);
                }

                // Мб вернуть.
                var hitDistance = (hit ? hitInfo.distance : RaycastDistance) / RaycastDistance;

                //AddVectorObs(hitDistance);
                sensor.AddObservation(hitDistance);

                if (hitDistance < current.HitThreshold) {
                    AddReward(HitPenalty);

                    //Done();
                    //EndEpisode(); - ?
                    //AgentReset(); 
                    // Поставить карт на трассу, если врезался (только в режиме тренировки).
                    OnEpisodeBegin();
                }
            }
        }

        //public override void AgentAction(float[] vectorAction)
        public override void OnActionReceived(ActionBuffers vectorAction)
        {
            //Debug.Log("OnActionReceived");
            InterpretDiscreteActions(vectorAction);

            // Find the next checkpoint when registering the current checkpoint that the agent has passed.
            int next = (checkpointIndex + 1) % Colliders.Length;
            Collider nextCollider = Colliders[next];
            Vector3 direction = (nextCollider.transform.position - kart.transform.position).normalized;
            float reward = Vector3.Dot(kart.Rigidbody.velocity.normalized, direction);

            if (ShowRaycasts)
            {
                Debug.DrawRay(AgentSensorTransform.position, kart.Rigidbody.velocity, Color.blue);
            }

            // Add rewards if the agent is heading in the right direction
            AddReward(reward * TowardsCheckpointReward);
            AddReward(kart.LocalSpeed() * SpeedReward);
            //Debug.Log("Common reward: " + GetCumulativeReward());
        }

        //public override void AgentReset()
        public override void OnEpisodeBegin()
        {
            switch (Mode)
            {
                case AgentMode.Training:
                    checkpointIndex = Random.Range(0, Colliders.Length - 1);
                    Collider collider = Colliders[checkpointIndex];
                    transform.localRotation = collider.transform.rotation;
                    transform.position = collider.transform.position;
                    kart.Rigidbody.velocity = default;
                    acceleration = 0f;
                    steering = 0f;
                    break;
                default:
                    break;
            }
        }

        //public override float[] Heuristic()
        //{
        //    localActions[0] = Input.GetAxis("Horizontal") + 1;
        //    localActions[1] = Sign(Input.GetAxis("Vertical"));
        //    return localActions;
        //}
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Debug.Log("Heurisic " + actionsOut.DiscreteActions.Length + " " + actionsOut.ContinuousActions.Length);
            //Debug.Log("Heurisic " + Input.GetAxis("Horizontal") + ", " + Input.GetAxis("Vertical"));

            //actionsOut.ContinuousActions.Array[0] = Input.GetAxis("Horizontal") + 1;
            //actionsOut.ContinuousActions.Array[1] = Sign(Input.GetAxis("Vertical"));

            // 0 - left, 1 - no steering, 2 - right
            actionsOut.DiscreteActions.Array[0] = Sign(Input.GetAxis("Horizontal")) + 1;
            actionsOut.DiscreteActions.Array[1] = Sign(Input.GetAxis("Vertical"));
        }

        public Vector2 GenerateInput()
        {
            //Debug.Log("Agent GenInput: " + steering + " " + acceleration);
            return new Vector2(steering, acceleration);
        }
    }
}
