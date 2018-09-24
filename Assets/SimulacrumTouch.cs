using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

public class SimulacrumTouch : MonoBehaviour {

    public UnityEvent OnBadGuysTouched;
    public float m_eventTime;

    public static bool m_touchHasBeenPerformed = false;

    private enum Hand
    {
        left, right
    }

    [SerializeField]
    private Hand m_hand;
    public AudioSource m_audioSource;

    private void Awake()
    {
        if(m_audioSource == null)
        {
            m_audioSource = GetComponent<AudioSource>();
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Debtor"))
        {
            Debug.Log("Found a bad guy");
            OnBadGuysTouched.Invoke();
        }
    }

    public virtual void CallForNewFreezeFrame(FreezeFrameState frame)
    {
        if (!m_touchHasBeenPerformed)
        {
            Debug.Log("Performing first touch");
            m_touchHasBeenPerformed = true;
            if(m_audioSource != null)
            {
                Debug.Log("Playing audio");
                m_audioSource.Play();
            }
            GameManager.instance.NewFreezeFrame(frame, m_eventTime);
        }        
    }

    public virtual void TriggerHandPulse()
    {
        VRTK_ControllerReference controller;
        if(m_hand == Hand.right)
        {
            controller = VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Right);
        } else if (m_hand == Hand.left)
        {
            controller = VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Left);
        } else
        {
            controller = VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.None);
        }

        GameManager.instance.OneShotHapticPulse(controller);
    }
}
