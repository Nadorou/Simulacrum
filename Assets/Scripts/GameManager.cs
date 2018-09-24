using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public delegate void AudioClipEventHandler(object sender, AudioClip clip);

public class GameManager : MonoBehaviour {

    public GameObject hand;

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
    [SerializeField]
    private AudioClip m_PossessionMusic;
    private bool m_runHapticGrabCue = false;

    public VRTK_PolicyList m_teleportPolicyList;
    private VRTK_BasicTeleport m_teleporter;

    public static event AudioClipEventHandler HapticCueEvent;

    private bool m_rigIsPossessed = false;

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

        if(m_teleportPolicyList != null)
        {
            m_teleporter = m_teleportPolicyList.gameObject.GetComponent<VRTK_BasicTeleport>();
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

    private IEnumerator RunStartup()
    {
        yield return null;
        m_currentScene.EnableScene();
        m_currentFreezeFrameState = m_currentScene.m_startFreezeFrame;
        SteamVR_Fade.Start(Color.black, 0);
        SteamVR_Fade.Start(Color.clear, m_startupFadeDuration);
        AudioManager.instance.FadeInMusic(m_startupFadeDuration);
        DisableTeleporting();
        yield return new WaitForSeconds(m_startupFadeDuration);
        EnableTeleporting();
    }

    private IEnumerator RunExitSequence()
    {
        DisableTeleporting();
        SteamVR_Fade.Start(Color.black, m_startupFadeDuration);
        yield return new WaitForSeconds(5f);
        AudioManager.instance.FadeOutMusic(m_startupFadeDuration);
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
        DisableTeleporting();
        //Fade camera out
        newScene.OnSceneSwitchTriggered();
        SteamVR_Fade.Start(Color.black, fadeTime);
        AudioManager.instance.FadeOutMusic(fadeTime);
        yield return new WaitForSeconds(fadeTime);
        yield return new WaitForSeconds(5f);
        if (m_rigIsPossessed)
        {
            DispossessIKRig();
        }
        //Fade completed, setup the new scene
        oldScene.DisableScene();
        newScene.EnableScene();
        m_currentScene = newScene;
        m_currentFreezeFrameState = newScene.m_startFreezeFrame;

        //Fade back in
        SteamVR_Fade.Start(Color.clear, fadeTime);
        AudioManager.instance.SwapMusicAndFade(newScene.m_sceneMusic, fadeTime);
        yield return new WaitForSeconds(fadeTime);
        EnableTeleporting();
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
        PoIEffect(poiPosition, "exhale");
    }

    public void PoIEffect(Vector3 poiPosition, string audioKey)
    {
        m_poiSource.transform.position = poiPosition;
        AudioManager.instance.RequestAudio(audioKey, m_poiSource);
    }

    private void OnInteractiveFrameStarted(FreezeFrameState sender)
    {
        StartCoroutine(HapticPulseLoop(10f));
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
            yield return new WaitForSeconds(15f);
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
            VRTK_ControllerHaptics.TriggerHapticPulse(controller.Value, 0.8f, 1f, 0.01f);
        }

        OnHapticCueEvent(m_hapticCueNoise);
    }

    public void OneShotHapticPulse(VRTK_ControllerReference controller)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(controller, 0.8f, 1f, 0.01f);
    }

    private void OnHapticCueEvent(AudioClip clip)
    {
        if (HapticCueEvent != null)
        {
            HapticCueEvent(this, clip);
        }
    }

    public void PossessIKRig(GameObject riggedObject, GameObject[] removeObject = null)
    {
        //This will be called when we want to possess the simulacra      
        StartCoroutine(PossessionSequence(riggedObject, 4f, removeObject));        
    }

    public void DispossessIKRig()
    {
        //This will be called when we want to dispossess the simulacra
        ToggleHands(true);
        m_rigIsPossessed = false;
        EnableTeleporting();
    }

    private IEnumerator PossessionSequence(GameObject rig, float fadeTime, GameObject[] removeObject)
    {        
        DisableTeleporting();
        SteamVR_Fade.Start(Color.black, fadeTime);
        AudioManager.instance.FadeOutMusic(fadeTime);
        yield return new WaitForSeconds(fadeTime);

        ToggleHands(false);
        rig.SetActive(true);
        if(removeObject != null)
        {
            foreach(GameObject go in removeObject)
            {
                go.SetActive(false);
            }            
        }
        m_rigIsPossessed = true;

        yield return new WaitForSeconds(1.5f);

        Color m_simColor = new Color(0.2f, 0, 0, 0.2f);
        SteamVR_Fade.Start(m_simColor, fadeTime);
        AudioManager.instance.SwapMusicAndFade(m_PossessionMusic, fadeTime);
        DisableTeleporting();
        yield return new WaitForSeconds(fadeTime);
    }

    private void ToggleHands(bool state)
    {
        GameObject rightHand = VRTK_DeviceFinder.GetModelAliasController(VRTK_DeviceFinder.GetControllerRightHand());
        GameObject leftHand = VRTK_DeviceFinder.GetModelAliasController(VRTK_DeviceFinder.GetControllerLeftHand());
        if (state)
        {
            VRTK_ObjectAppearance.SetRendererVisible(rightHand);
            VRTK_ObjectAppearance.SetRendererVisible(leftHand);
        } else
        {
            hand = rightHand;
            VRTK_ObjectAppearance.SetRendererHidden(rightHand);
            VRTK_ObjectAppearance.SetRendererHidden(leftHand);
        }

    }

    private void DisableTeleporting()
    {
        m_teleporter.blinkTransitionSpeed = 0f;
        //m_teleportPolicyList.identifiers = new List<string>();
    }

    private void EnableTeleporting()
    {
        m_teleporter.blinkTransitionSpeed = 0.6f;
        //List<string> identifiers = new List<string>();
        //identifiers.Add("TeleportSurface");
        //m_teleportPolicyList.identifiers = identifiers;
    }
}
