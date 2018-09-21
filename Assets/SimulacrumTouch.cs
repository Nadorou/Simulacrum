using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimulacrumTouch : MonoBehaviour {

    public UnityEvent OnBadGuysTouched;
    public float m_eventTime;

    public static bool m_touchHasBeenPerformed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Debtor"))
        {
            OnBadGuysTouched.Invoke();
        }
    }

    public virtual void CallForNewFreezeFrame(FreezeFrameState frame)
    {
        if (!m_touchHasBeenPerformed)
        {
            m_touchHasBeenPerformed = true;
            GameManager.instance.NewFreezeFrame(frame, m_eventTime);
        }        
    }
}
