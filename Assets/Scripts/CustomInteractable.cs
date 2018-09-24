using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomInteractable : VRTK_InteractableObject {

    [Header("Custom Interactable Settings")]
    [SerializeField][Tooltip("Optional game object that becomes disabled upon the user first grabbing the object")]
    private GameObject m_attentionObject;
    private Light m_attentionLight;
    private float m_lightMax;
    private float m_lightMin;

    public static event InteractableObjectEventHandler CustomInteractableGrabbed;
    

    private FreezeFrameState m_freezeFrameParent;
    private AudioSource m_audioSource;

    protected override void Awake()
    {
        base.Awake();
        m_audioSource = GetComponent<AudioSource>();

        InteractableObjectGrabbed += OnGrabbed;

        m_freezeFrameParent = FindFreezeFrameParent();
        if (m_freezeFrameParent != null)
        {
            m_freezeFrameParent.FrameStarted += OnFrameStarted;
            m_freezeFrameParent.FrameEnded += OnFrameEnded;
        }
        if(m_attentionObject != null)
        {
            m_attentionLight = m_attentionObject.GetComponent<Light>();
            if(m_attentionLight != null)
            {
                m_lightMax = m_attentionLight.intensity;
                m_lightMin = m_lightMax / 10;

                m_attentionLight.intensity = m_lightMin;
            }
        }
    }

    protected void OnDestroy()
    {
        InteractableObjectGrabbed -= OnGrabbed;
    }

    protected virtual void OnFrameStarted(FreezeFrameState frame)
    {
        GameManager.HapticCueEvent += OnHapticCue;
    }

    protected virtual void OnFrameEnded(FreezeFrameState frame)
    {
        GameManager.HapticCueEvent -= OnHapticCue;
    }

    private void OnHapticCue(object sender, AudioClip clip)
    {
        if(m_audioSource != null)
        {
            Debug.Log("Interactable object received cue to play " + clip.name);
            m_audioSource.PlayOneShot(clip);
        }

        if(m_attentionLight != null)
        {
            StartCoroutine(LightPulse(5f, m_lightMin, m_lightMax));
        }
    }

    private IEnumerator LightPulse(float pulseTime, float intensityMin, float intensityMax)
    {
        float halfPulse = pulseTime / 2;

        float timeSpent = 0;

        while (timeSpent < halfPulse)
        {
            timeSpent += Time.deltaTime;

            m_attentionLight.intensity = Mathf.Lerp(intensityMin, intensityMax, timeSpent / halfPulse);
            yield return null;
        }

        timeSpent = 0;

        while (timeSpent < halfPulse)
        {
            timeSpent += Time.deltaTime;

            m_attentionLight.intensity = Mathf.Lerp(intensityMax, intensityMin, timeSpent / halfPulse);
            yield return null;
        }
    }

    private FreezeFrameState FindFreezeFrameParent()
    {
        FreezeFrameState f;
        f = GetComponentInParent<FreezeFrameState>();
        return f;
    }

    private void OnGrabbed(object sender, InteractableObjectEventArgs e)
    {
        if(m_attentionObject != null)
        {
            if (m_attentionObject.activeSelf)
            {
                //m_attentionObject.SetActive(false);
            }
        }

        if(CustomInteractableGrabbed != null)
        {
            CustomInteractableGrabbed(sender, e);
        }
    }

    
}
