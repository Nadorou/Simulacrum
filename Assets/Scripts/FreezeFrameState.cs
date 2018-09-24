﻿using System.Collections;
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

    public bool isStartFrame = false;
    public bool isEndFrame = false;
    public FreezeFrameState autoProgressFrame;
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

    [Header("Point of interest mechanic")]
    [SerializeField]
    protected Transform m_poi;
    [SerializeField]
    protected string m_poiSound = "exhale";

    protected bool m_isFading = false;
    protected Renderer[] m_freezeFrameRenderers;
    protected Renderer[] m_endFadeOnlyRenderers;
    protected Renderer[] m_startFadeOnlyRenderers;

    [SerializeField]
    protected bool m_isInteractiveFrame = false;

    [Header("Debug")]
    public Renderer[] renderList;

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
        if (autoProgressFrame == null && !isEndFrame)
        {
            m_isInteractiveFrame = true;
        }
        if (!isStartFrame)
        {
            SetTransparent(true);
            SetFrameAlpha(0, true);
            gameObject.SetActive(false);
        }        
    }

    protected void CreateRenderArray()
    {
        m_freezeFrameRenderers = m_freezeFrameObjectParent.GetComponentsInChildren<Renderer>();
        m_endFadeOnlyRenderers = m_endFadeOnlyParent.GetComponentsInChildren<Renderer>();
        m_startFadeOnlyRenderers = m_startFadeOnlyParent.GetComponentsInChildren<Renderer>();

        renderList = m_freezeFrameRenderers;
    }

    public virtual void StartFreezeFrameProgression()
    {
        if(autoProgressFrame != null)
        {
            StartCoroutine(WaitForNextFrame());
        }
        if (m_isInteractiveFrame)
        {
            Debug.Log("Called from startfreezeframe");
            OnInteractiveFrameStarted();
        }

        OnFrameStarted();
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
            GameManager.instance.PoIEffect(m_poi.position, m_poiSound);
        }

        if (m_isInteractiveFrame)
        {
            Debug.Log("Called from start frame");
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
            GameManager.instance.NewFreezeFrame(autoProgressFrame, m_fadeTime);
        } else
        {
            GameManager.instance.NewFreezeFrame(autoProgressFrame);
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
            if(r != null && (!r.material.name.Contains("Mat_HighlightObject") || !fadingIn))
            {
                Color renderColor = r.material.color;
                renderColor.a = a;
                r.material.color = renderColor;
            }            
        }
        if (!fadingIn)
        {
            foreach (Renderer r in m_endFadeOnlyRenderers)
            {
                if(r != null)
                {
                    Color renderColor = r.material.color;
                    renderColor.a = a;
                    r.material.color = renderColor;
                }
                
            }
        } else
        {
            foreach (Renderer r in m_startFadeOnlyRenderers)
            {
                if (r != null && !r.material.name.Contains("Mat_HighlightObject"))
                {
                    Color renderColor = r.material.color;
                    renderColor.a = a;
                    r.material.color = renderColor;
                }
                
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
            if (isEndFrame)
            {
                StartCoroutine(DelayedSceneProgression());
            } else if (autoProgressFrame != null)
            {
                StartCoroutine(WaitForNextFrame());
            }
        } else
        {
            gameObject.SetActive(false);
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
        m_isFading = true;
        float transparency;
        float timeSpent = 0f;
        float percentageDone = 0f;
        bool fadeComplete = false;

        if (!fadingIn)
        {
            SetTransparent(false);
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

    protected void SetTransparent(bool fadingIn)
    {
        foreach (Renderer r in m_freezeFrameRenderers)
        {
            if(r != null && !r.material.name.Contains("Mat_HighlightObject"))
            {
                StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Fade);
            }
        }
        if (fadingIn)
        {
            foreach (Renderer r in m_startFadeOnlyRenderers)
            {
                if (r != null && !r.material.name.Contains("Mat_HighlightObject"))
                {
                    StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Fade);
                }
            }
        } else
        {
            foreach (Renderer r in m_endFadeOnlyRenderers)
            {
                if (r != null && !r.material.name.Contains("Mat_HighlightObject"))
                {
                    StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Fade);
                }
            }
        }
        
    }

    protected void SetOpaque()
    {
        foreach (Renderer r in m_freezeFrameRenderers)
        {
            if(r != null && !r.material.name.Contains("Mat_HighlightObject"))
            {
                StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
            }
        }
        foreach (Renderer r in m_endFadeOnlyRenderers)
        {
            if(r != null && !r.material.name.Contains("Mat_HighlightObject"))
            {
                StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
            }
        }
        foreach (Renderer r in m_startFadeOnlyRenderers)
        {
            if(r != null && !r.material.name.Contains("Mat_HighlightObject"))
            {
                StandardShaderUtils.ChangeRenderMode(r.material, StandardShaderUtils.BlendMode.Opaque);
            }
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
        Debug.Log("OnInteractiveFrameStarted called on " + gameObject.name);
        if(InteractiveFrameStarted != null)
        {
            InteractiveFrameStarted(this);
        }
    }
    #endregion
}
