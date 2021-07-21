using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using KartGame.KartSystems;

public struct GhostTransform
{
    public Vector3 position;
    public Quaternion rotation;

    public GhostTransform(Transform transform)
    {
        this.position = transform.position;
        this.rotation = transform.rotation;
    }
}

public class GhostManager : MonoBehaviour
{
    public Transform kart;
    public Transform ghostKart;
    public Transform cameraPlaceholder;
    public CinemachineVirtualCamera cinemachineCam; 

    public bool recording;
    public bool playing;

    private List<GhostTransform> recordedGhostTransfroms = new List<GhostTransform>();
    private GhostTransform lastRecordedGhostTransform;
    private TimeManager m_TimeManager;

    public bool isGhostEnd;
    private bool isRecordingStarted;

    // for FPS
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    float m_refreshTime = 0.5f;

    //int t = 0;
    float m_startTime;
    float m_startPlayTime;
    float timeInterval;

    // Start is called before the first frame update
    void Start()
    {
        m_TimeManager = FindObjectOfType<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // calculateFPS();

        if (recording && !isRecordingStarted)
        {
            isRecordingStarted = true;
            StartCoroutine(StartRecording());
        }

        if(playing == true)
        {
            // Debug.Log("frames recorded: " + recordedGhostTransfroms.Count + " End record time: " + Time.time.ToString("f6"));
            Play();
        }
    }

    IEnumerator StartRecording()
    {
        while (recording && !m_TimeManager.timePause)
        {
            if (kart.position != lastRecordedGhostTransform.position || kart.rotation != lastRecordedGhostTransform.rotation)
            {
                var newGhostTransform = new GhostTransform(kart);
                recordedGhostTransfroms.Add(newGhostTransform);

                lastRecordedGhostTransform = newGhostTransform;
            }
            yield return new WaitForSeconds(0.01F);
        }

        isRecordingStarted = false;
    }

    void Play()
    {
        ghostKart.gameObject.SetActive(true);
        StartCoroutine(StartGhost());
 
        cinemachineCam.Follow = cameraPlaceholder;
        cinemachineCam.LookAt = cameraPlaceholder;

        playing = false;
    }

    IEnumerator StartGhost()
    {
        for (int i = 0; i < recordedGhostTransfroms.Count && !Input.GetKeyDown(KeyCode.Space); i++)
        {
            ghostKart.position = recordedGhostTransfroms[i].position;
            ghostKart.rotation = recordedGhostTransfroms[i].rotation;

            //yield return new WaitForFixedUpdate();
            //yield return new WaitForEndOfFrame();
            //yield return new WaitForSeconds(0.0061F); 
            //yield return new WaitForSecondsRealtime(0.0063F);

            yield return new WaitForSeconds(0.01F);
        }

        isGhostEnd = true;
    }

    void calculateFPS()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            // This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
        Debug.Log(m_lastFramerate);
    }
}
