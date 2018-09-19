using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using UnityEngine.Events;

public class SnapZoneEvent : VRTK_SnapDropZone {

    public UnityEvent m_onSnappedEvent;
    public float m_eventTime;
    public bool m_disableOnSnap;

    protected override void Awake()
    {
        base.Awake();
        ObjectSnappedToDropZone += OnObjectSnapped;
    }

    private void OnDestroy()
    {
        ObjectSnappedToDropZone -= OnObjectSnapped;
    }

    protected virtual void OnObjectSnapped(object sender, SnapDropZoneEventArgs e)
    {
        Debug.Log("Object snapped runs");
        m_onSnappedEvent.Invoke();
        if (m_disableOnSnap)
        {
            DisableInteractivity(e.snappedObject);
        }
    }

    protected virtual void DisableInteractivity(GameObject snappedObject)
    {
        GetComponent<Collider>().enabled = false;
        snappedObject.GetComponent<Collider>().enabled = false;
    }

    #region ==EVENT FUNCTIONS==
    public virtual void CallForNewFreezeFrame(FreezeFrameState frame)
    {
        GameManager.instance.NewFreezeFrame(frame, m_eventTime);
    }

    public virtual void CallForSimulacraPossession()
    {
        GameManager.instance.PossessIKRig();
    }
    #endregion
}
