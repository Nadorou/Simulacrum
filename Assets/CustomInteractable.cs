using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomInteractable : VRTK_InteractableObject {

    [Header("Custom Interactable Settings")]
    [SerializeField][Tooltip("Optional game object that becomes disabled upon the user first grabbing the object")]
    private GameObject m_attentionObject;

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
    }

    protected void OnDestroy()
    {
        InteractableObjectGrabbed -= OnGrabbed;
    }

    protected void OnFrameStarted(FreezeFrameState frame)
    {
        GameManager.HapticCueEvent += OnHapticCue;
    }

    protected void OnFrameEnded(FreezeFrameState frame)
    {
        GameManager.HapticCueEvent -= OnHapticCue;
    }

    private void OnHapticCue(object sender, AudioClip clip)
    {
        if(m_audioSource != null)
        {
            m_audioSource.PlayOneShot(clip);
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
                m_attentionObject.SetActive(false);
            }
        }

        if(CustomInteractableGrabbed != null)
        {
            CustomInteractableGrabbed(sender, e);
        }
    }

    
}
