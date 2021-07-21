using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using KartGame.KartSystems;
using UnityEngine.SceneManagement;
using KartGame.AI;
using Cinemachine;
using System;

public enum GameState{Play, Won, Lost}

public class GameFlowManager : MonoBehaviour
{
    [Header("Parameters")]
    public bool overrideGameMode;
    public MyGameMode myGameMode;

    [Tooltip("Duration of the fade-to-black at the end of the game")]
    public float endSceneLoadDelay = 3f;
    [Tooltip("The canvas group of the fade-to-black screen")]
    public CanvasGroup endGameFadeCanvasGroup;

    public GhostManager ghostManager;
    [HideInInspector]
    public bool isGameEnded;

    [Header("Win")]
    [Tooltip("This string has to be the name of the scene you want to load when winning")]
    public string winSceneName = "WinScene";
    [Tooltip("Duration of delay before the fade-to-black, if winning")]
    public float delayBeforeFadeToBlack = 4f;
    [Tooltip("Duration of delay before the win message")]
    public float delayBeforeWinMessage = 2f;
    [Tooltip("Sound played on win")]
    public AudioClip victorySound;

    [Tooltip("Prefab for the win game message")]
    public DisplayMessage winDisplayMessage;

    public PlayableDirector raceCountdownTrigger;

    [Header("Lose")]
    [Tooltip("This string has to be the name of the scene you want to load when losing")]
    public string loseSceneName = "LoseScene";
    [Tooltip("Prefab for the lose game message")]
    public DisplayMessage loseDisplayMessage;


    public GameState gameState { get; private set; }

    public bool autoFindKarts = true;
    public ArcadeKart playerKart;

    ArcadeKart[] karts;
    ObjectiveManager m_ObjectiveManager;
    TimeManager m_TimeManager;
    float m_TimeLoadEndGameScene;
    string m_SceneToLoad;
    float elapsedTimeBeforeEndScene = 0;

    void OnEnable()
    {
        if (overrideGameMode) {
            SceneParameters.gameMode = myGameMode;
        }

        if (SceneParameters.gameMode == MyGameMode.Pins) {
            GameObject.Find("AICheckpoints").SetActive(false);
            SceneParameters.FindIncludingInactive("Pins").SetActive(true);
            GameObject.Find("KartClassic_Player").SetActive(false);
            SceneParameters.FindIncludingInactive("4x4_Player").SetActive(true);
            GameObject.Find("InvisibleWalls").SetActive(false);
            GameObject.Find("MLAgents").SetActive(false);
            SceneParameters.FindIncludingInactive("ObjectiveCrashMode").SetActive(true);
            GameObject.Find("Position").SetActive(false);

            ObjectiveCompleteLaps objective = FindObjectOfType<ObjectiveCompleteLaps>();
            objective.isTimed = true;
            objective.lapsToComplete = 1;

            CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();
            cam.Follow = cam.LookAt = GameObject.Find("4x4_Player").transform;

            AudioSource audio = GameObject.Find("GameHUD").GetComponent<AudioSource>();
            audio.clip = Resources.Load<AudioClip>("Juan Serrano ft. Miguel Lara - Bocaccio (Dr. Kucho! Remix)");
            audio.volume = 0.2f;
            audio.Play();

        } 
    }

    void Start()
    {
        if (autoFindKarts) {
            karts = FindObjectsOfType<ArcadeKart>();
            if (karts.Length > 0) {
                foreach (ArcadeKart k in karts) {
                    //if (k.GetComponent<KartAgent>() == null) {
                    //    playerKart = k;
                    //    //Debug.Log("t " + k.GetComponent<ArcadeKartExt>().getPosition());
                    //}
                    if (k.GetComponent<ArcadeKartExt>() != null || k.GetComponent<ArcadeKartExt2>() != null) {
                        playerKart = k;
                    }
                }
                //if (!playerKart) playerKart = karts[0];    
            }
            DebugUtility.HandleErrorIfNullFindObject<ArcadeKart, GameFlowManager>(playerKart, this);
        }

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
        DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        m_TimeManager = FindObjectOfType<TimeManager>();
        DebugUtility.HandleErrorIfNullFindObject<TimeManager, GameFlowManager>(m_TimeManager, this);

        AudioUtility.SetMasterVolume(1);

        winDisplayMessage.gameObject.SetActive(false);
        loseDisplayMessage.gameObject.SetActive(false);

        m_TimeManager.StopRace();
        foreach (ArcadeKart k in karts) {
            k.SetCanMove(false);
        }

        //run race countdown animation
        ShowRaceCountdownAnimation();
        StartCoroutine(ShowObjectivesRoutine());

        StartCoroutine(CountdownThenStartRaceRoutine());
    }

    public KartAgent[] GetAgentKarts()
    {
        return FindObjectsOfType<KartAgent>();
    }

    public ArcadeKartExt GetPlayerKart()
    {
        if (playerKart == null){
            karts = FindObjectsOfType<ArcadeKart>();
            if (karts.Length > 0) {
                foreach (ArcadeKart k in karts) {
                    if (k.GetComponent<ArcadeKartExt>() != null || k.GetComponent<ArcadeKartExt2>() != null) {
                        playerKart = k;
                    }

                }
            } else return null;
        }
        return playerKart.GetComponent<ArcadeKartExt>();
    }

    IEnumerator CountdownThenStartRaceRoutine() {
        yield return new WaitForSeconds(3f);
        StartRace();
    }

    void StartRace() {
        if (SceneParameters.gameMode == MyGameMode.Pins) {
            ghostManager.recording = true;
            ghostManager.playing = false;
        }

        foreach (ArcadeKart k in karts)
        {
			k.SetCanMove(true);
        }
        m_TimeManager.StartRace();
    }

    void ShowRaceCountdownAnimation() {
        raceCountdownTrigger.Play();
    }

    IEnumerator ShowObjectivesRoutine() {
        while (m_ObjectiveManager.Objectives.Count == 0)
            yield return null;
        yield return new WaitForSecondsRealtime(0.2f);
        for (int i = 0; i < m_ObjectiveManager.Objectives.Count; i++)
        {
           if (m_ObjectiveManager.Objectives[i].displayMessage)m_ObjectiveManager.Objectives[i].displayMessage.Display();
           yield return new WaitForSecondsRealtime(1f);
        }
    }


    void Update()
    {

        if (gameState != GameState.Play)
        {
            elapsedTimeBeforeEndScene += Time.deltaTime;
            if(elapsedTimeBeforeEndScene >= endSceneLoadDelay)
            {

                float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
                endGameFadeCanvasGroup.alpha = timeRatio;

                float volumeRatio = Mathf.Abs(timeRatio);
                float volume = Mathf.Clamp(1 - volumeRatio, 0, 1);
                AudioUtility.SetMasterVolume(volume);

                // See if it's time to load the end scene (after the delay)
                if (Time.time >= m_TimeLoadEndGameScene) {
                    SceneManager.LoadScene(m_SceneToLoad);
                    gameState = GameState.Play;
                }
            }
        }
        else
        {
            if (m_ObjectiveManager.AreAllObjectivesCompleted()) {
                if (SceneParameters.gameMode == MyGameMode.Laps && playerKart.GetComponent<ArcadeKartExt>().getPosition() != 1 || m_TimeManager.timePauseWasEnabled)
                    EndGame(false);
                else
                    EndGame(true);
            }
                

            if (m_TimeManager.IsFinite && m_TimeManager.IsOver)
                EndGame(false);
        }
    }

    void EndGame(bool win)
    {
        m_TimeManager.StopRace();

        if (SceneParameters.gameMode == MyGameMode.Laps) 
        {
            playerKart.GetComponent<KartAgent>().enabled = true;
            playerKart.GetComponent<BasicController>().enabled = true;

        }else if(SceneParameters.gameMode == MyGameMode.Pins) 
        {
            playerKart.gameObject.SetActive(false);
        }


        if (SceneParameters.gameMode == MyGameMode.Laps || SceneParameters.gameMode == MyGameMode.Pins && ghostManager.isGhostEnd){
            endGameFinal(win);
        }

        // Set 'playing is true' only once
        if (isGameEnded) return;
        isGameEnded = true;

        if (SceneParameters.gameMode == MyGameMode.Pins) {
            ghostManager.recording = false;
            ghostManager.playing = true;
        }
    }

    void endGameFinal(bool win)
    {
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Remember that we need to load the appropriate end scene after a delay
        gameState = win ? GameState.Won : GameState.Lost;
        endGameFadeCanvasGroup.gameObject.SetActive(true);
        if (win)
        {
            m_SceneToLoad = winSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // play a sound on win
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = victorySound;
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
            audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

            // create a game message
            winDisplayMessage.delayBeforeShowing = delayBeforeWinMessage;
            winDisplayMessage.gameObject.SetActive(true);
        }
        else
        {
            m_SceneToLoad = loseSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // create a game message
            loseDisplayMessage.delayBeforeShowing = delayBeforeWinMessage;
            loseDisplayMessage.gameObject.SetActive(true);
        }
    }
}
