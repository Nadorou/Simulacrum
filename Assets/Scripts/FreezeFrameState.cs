using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void FreezeFrameEvent(FreezeFrameState sender);

public class FreezeFrameState : MonoBehaviour {

    [SerializeField]
    protected GameObject m_freezeFrameObjectParent;
    [SerializeField]
    protected GameObject m_endFadeOnlyParent;
    [SerializeField]
    protected GameObject m_startFadeOnlyParent;
    [SerializeField]
    protected GameObject m_noFadeParent;

    public bool m_isStartFrame = false;
    public bool m_isEndFrame = false;
    public FreezeFrameState m_AutoProgressFrame;
    [SerializeField]
    private float m_AutoProgressDelay = 15f;

    [SerializeField]
    private bool m_useOwnFadeTime = false;
    [SerializeField]
    private float m_fadeTime;
    public float fadeTime
    {
        get
        {
            return m_fadeTime;
        }
    }

    [SerializeField]
    protected Transform m_poi;

    protected bool m_isFading = false;
    protected Renderer[] m_freezeFrameRenderers;
    protected Renderer[] m_endFadeOnlyRenderers;
    protected Renderer[] m_startFadeOnlyRenderers;

    protected bool m_isInteractiveFrame = false;

    public event FreezeFrameEvent FrameEnded;
    public event FreezeFrameEvent FrameStarted;
    public static event FreezeFrameEvent InteractiveFrameStarted;

    #region ==LIFE CYCLE==
    protected virtual void Awake()
    {
        SetupFreezeFrame();
    }

    protected virtual void SetupFreezeFrame()
    {
        CreateRenderArray();

        if (!m_isStartFrame)
        {
            SetTransparent();
            SetFrameAlpha(0, true);
            gameObject.SetActive(false);
        } else
        {
            if (m_AutoProgressFrame != null)
            {
                StartCoroutine(WaitForNextFrame());
            } else if (!m_isEndFrame)
            {
                m_isInteractiveFrame = true;
            }
        }
    }

    protected void CreateRenderArray()
    {
        m_freezeFrameRenderers = m_freezeFrameObjectParent.GetComponentsInChildren<Renderer>();
        m_endFadeOnlyRenderers = m_endFadeOnlyParent.GetComponentsInChildren<Renderer>();
        m_startFadeOnlyRenderers = m_startFadeOnlyParent.GetComponentsInChildren<Renderer>();
    }

    public virtual void StartFrame(float fadeDuration)
    {
        gameObject.SetActive(true);

        if (!m_noFadeParent.activeSelf) //This is disabled when we leave a frame, so if we return to an old one it must be re-enabled.
        {
            m_noFadeParent.SetActive(true);
            m_startFadeOnlyParent.SetActive(true);
        }

        FadeIn(fadeDuration);
        if(m_poi != null)
        {
            GameManager.instance.PoIEffect(m_poi.position);
        }

        if (m_isInteractiveFrame)
        {
            OnInteractiveFrameStarted();
        }

        OnFrameStarted();
    }

    public virtual void EndFrame(float fadeDuration)
    {
        m_noFadeParent.SetActive(false); //Prevents objects from stacking on top of each other.
        m_startFadeOnlyParent.SetActive(false);
        FadeOut(fadeDuration);

        OnFrameEnded();
    }

    protected IEnumerator WaitForNextFrame()
    {
        yield return new WaitForSeconds(m_AutoProgressDelay);
        if (m_useOwnFadeTime)
        {
            GameManager.instance.NewFreezeFrame(m_AutoProgressFrame, m_fadeTime);
        } else
        {
            GameManager.instance.NewFreezeFrame(m_AutoProgressFrame);
        }
    }

    protected IEnumerator DelayedSceneProgression()
    {
        yield return new WaitForSeconds(m_AutoProgressDelay);
        GameManager.instance.NextScene();
    }

    #endregion

    #region ==FADE FUNCTIONS==
    protected void SetFrameAlpha(float a, bool fadingIn)
    {
        foreach (Renderer r in m_freezeFrameRenderers)
        {
            Color renderColor = r.material.color;
            renderColor.a = a;
            r.material.color = renderColor;
        }
        if (!fadingIn)
        {
            foreach (Renderer r in m_endFadeOnlyRenderers)
            {
                Color renderColor = r.material.color;
                renderColor.a = a;
                r.material.color = renderColor;
            }
        } else
        {
            foreach (Renderer r in m_startFadeOnlyRenderers)
            {
                Color renderColor = r.material.color;
                renderColor.a = a;
                r.material.color = renderColor;
            }
        }
    }

    protected virtual void FadeOut(float duration)
    {
        if (!m_isFading)
        {
            StartCoroutine(RunFade(false, duration));
        } else
        {
            Debug.Log("Attempted to start a freeze frame fade out but one is already going on.");
        }
    }

    protected virtual void FadeIn(float duration)
    {
        if (!m_isFading)
        {
            StartCoroutine(RunFade(true, duration));
        }
        else
        {
            Debug.Log("Attempted to start a freeze frame fade in but one is already going on.");
        }
    }

    /// <summary>
    /// Function that is run at the end of a fade
    /// </summary>
    /// <param name="wasFadedIn">True if the frame was just faded in, false if it was just faded out.</param>
    protected virtual void FadeCompleted(bool wasFadedIn)
    {
        if (wasFadedIn)
        {
            foreach(Renderer r in m_freezeFrameRenderers)
            {
                SetOpaque();
            }
            if (m_isEndFrame)
            {
                StartCoroutine(DelayedSceneProgression());
            } else if (m_AutoProgressFrame != null)
            {
                StartCoroutine(WaitForNextFrame());
            }
            Debug.Log("A fade in was completed");
        } else
        {
            gameObject.SetActive(false);
            Debug.Log("A fade out was completed");
        }
        
    }

    /// <summary>
    /// Fades the freeze frame in or out
    /// </summary>
    /// <param name="fadingIn">True if the frame is to be faded in, false if the material is to be faded out</param>
    /// <param name="duration">Duration of the fade in seconds</param>
    /// <returns></returns>
    protected virtual IEnumerator RunFade(bool fadingIn, float duration)
    {
        Debug.Log("Fade started");
        m_isFading = true;
        float transparency;
        float timeSpent = 0f;
        float percentageDone = 0f;
        bool fadeComplete = false;

        if (!fadingIn)
        {
            SetTransparent();
        }

        while (!fadeComplete)
        {
            timeSpent += Time.deltaTime;
            percentageDone = timeSpent / duration;
            if (percentageDone > 1)
                percentageDone = 1;

            if (fadingIn)
            {
                transparency = percentageDone;
            }
            else
            {
                transparency = 1 - percentageDone;
            }

            SetFrameAlpha(transparency, fadingIn);

            if(percentageDone >= 1)
            {
                fadeComplete = true;
            }

            yield return null;
        }

        m_isFading = false;
        FadeCompleted(fadingIn);
    }

    protected void SetTransparent()
    {
        foreach (Renderer r in m_freezeFrameRenderers)
        {
            StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Fade);
        }
        foreach (Renderer r in m_startFadeOnlyRenderers)
        {
            StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Fade);
        }
    }

    protected void SetOpaque()
    {
        foreach (Renderer r in m_freezeFrameRenderers)
        {
            StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
        }
        foreach (Renderer r in m_endFadeOnlyRenderers)
        {
            StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
        }
        foreach (Renderer r in m_startFadeOnlyRenderers)
        {
            StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
        }
    }
    #endregion

    #region ==EVENT PAYLOAD==
    protected void OnFrameEnded()
    {
        if (FrameEnded != null)
        {
            FrameEnded(this);
        }
    }

    protected void OnFrameStarted()
    {
        if (FrameStarted != null)
        {
            FrameStarted(this);
        }
    }

    protected void OnInteractiveFrameStarted()
    {
        if(InteractiveFrameStarted != null)
        {
            InteractiveFrameStarted(this);
        }
    }
    #endregion
}
