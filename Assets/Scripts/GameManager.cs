using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public delegate void AudioClipEventHandler(object sender, AudioClip clip);

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public SceneManager m_currentScene;
    public FreezeFrameState m_currentFreezeFrameState;
    [SerializeField]
    private float m_startupFadeDuration = 10f;
    public float m_sceneFadeDuration = 10f;
    [SerializeField]
    private float m_standardFreezeFrameFadeDuration = 4f;
    [SerializeField]
    private AudioSource m_poiSource;

    [SerializeField]
    private AudioClip m_hapticCueNoise;
    private bool m_runHapticGrabCue = false;

    public static event AudioClipEventHandler HapticCueEvent;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this)
        {
            Destroy(gameObject);
        }

        FreezeFrameState.InteractiveFrameStarted += OnInteractiveFrameStarted;
        CustomInteractable.CustomInteractableGrabbed += OnInteractableGrabbed;
    }

    private void OnDestroy()
    {
        FreezeFrameState.InteractiveFrameStarted -= OnInteractiveFrameStarted;
        CustomInteractable.CustomInteractableGrabbed -= OnInteractableGrabbed;
    }

    private void Start()
    {        
        StartCoroutine(RunStartup());
    }

    private void Update()
    {
              
    }

    private IEnumerator RunStartup()
    {        
        m_currentScene.EnableScene();
        m_currentFreezeFrameState = m_currentScene.m_startFreezeFrame;
        SteamVR_Fade.Start(Color.black, 0f);
        yield return null;
        SteamVR_Fade.Start(Color.clear, m_startupFadeDuration);
    }

    private IEnumerator RunExitSequence()
    {
        SteamVR_Fade.Start(Color.black, m_startupFadeDuration);
        yield return new WaitForSeconds(m_startupFadeDuration);
        Debug.Log("Application Quit Executed");
        Application.Quit();
    }

    #region ==Scene Progression==
    public void ProgressToNewScene(SceneManager scene, float fadeTime)
    {        
        StartCoroutine(SceneSwitchProgression(m_currentScene, scene, fadeTime));
    }

    public void NextScene()
    {
        if (m_currentScene.m_nextScene != null)
        {
            ProgressToNewScene(m_currentScene.m_nextScene, m_sceneFadeDuration);
        } else
        {
            StartCoroutine(RunExitSequence());
        }
    }

    private IEnumerator SceneSwitchProgression(SceneManager oldScene, SceneManager newScene, float fadeTime)
    {
        //Fade camera out
        newScene.OnSceneSwitchTriggered();
        SteamVR_Fade.Start(Color.black, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        //Fade completed, setup the new scene
        oldScene.DisableScene();
        newScene.EnableScene();
        m_currentScene = newScene;
        m_currentFreezeFrameState = newScene.m_startFreezeFrame;

        //Fade back in
        SteamVR_Fade.Start(Color.clear, fadeTime);
        yield return new WaitForSeconds(fadeTime);
        newScene.OnSceneSwitchFinished();
    }
    #endregion

    #region ==Freeze frame progression==
    //Switches to a new frame state
    public void NewFreezeFrame(FreezeFrameState frame)
    {
        NewFreezeFrame(frame, m_standardFreezeFrameFadeDuration);        
    }

    public void NewFreezeFrame(FreezeFrameState frame, float fadeTime)
    {
        m_currentFreezeFrameState.EndFrame(fadeTime);
        frame.StartFrame(fadeTime);

        m_currentFreezeFrameState = frame;
    }

    public void PoIEffect(Vector3 poiPosition)
    {
        m_poiSource.transform.position = poiPosition;
        AudioManager.instance.RequestAudio("exhale", m_poiSource);
    }

    private void OnInteractiveFrameStarted(FreezeFrameState sender)
    {
        StartCoroutine(HapticPulseLoop(sender.fadeTime));
    }

    private void OnObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {

    }
    #endregion


    private IEnumerator HapticPulseLoop(float initialDelay)
    {
        m_runHapticGrabCue = true;
        yield return new WaitForSeconds(initialDelay);
        while (m_runHapticGrabCue)
        {
            TriggerHapticPulse();
            yield return new WaitForSeconds(8f);
        }
    }

    //This ends the HapticPulseLoop coroutine
    private void OnInteractableGrabbed(object sender, InteractableObjectEventArgs e)
    {
        if (m_runHapticGrabCue)
        {
            m_runHapticGrabCue = false;
        }
    }

    private void TriggerHapticPulse()
    {
        foreach (KeyValuePair<uint, VRTK_ControllerReference> controller in VRTK_ControllerReference.controllerReferences)
        {
            VRTK_ControllerHaptics.TriggerHapticPulse(controller.Value, m_hapticCueNoise);
        }

        OnHapticCueEvent(m_hapticCueNoise);
    }

    private void OnHapticCueEvent(AudioClip clip)
    {
        if (HapticCueEvent != null)
        {
            HapticCueEvent(this, clip);
        }
    }

    public void PossessIKRig()
    {
        //This will be called when we want to possess the simulacra
    }

    public void DispossessIKRig()
    {
        //This will be called when we want to dispossess the simulacra
    }
}
