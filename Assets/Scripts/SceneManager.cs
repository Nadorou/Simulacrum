using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SceneManagerEvent(SceneManager sender);

public class SceneManager : MonoBehaviour {

    [SerializeField]
    protected GameObject m_sceneParent;
    public FreezeFrameState m_startFreezeFrame;

    public SceneManager m_nextScene;

    public event SceneManagerEvent SceneSwitchTriggered;
    public event SceneManagerEvent SceneSwitchFinished;

    private void Awake()
    {
        DisableScene();
    }

    public void DisableScene()
    {
        gameObject.SetActive(false);
    }

    public void EnableScene()
    {
        gameObject.SetActive(true);
        m_sceneParent.SetActive(true);
        m_startFreezeFrame.gameObject.SetActive(true);
        m_startFreezeFrame.StartFreezeFrameProgression();
    }

    public void OnSceneSwitchTriggered()
    {
        if (SceneSwitchTriggered != null)
        {
            SceneSwitchTriggered(this);
        }
    }

    public void OnSceneSwitchFinished()
    {
        if (SceneSwitchFinished != null)
        {
            SceneSwitchFinished(this);
        }
    }
}
