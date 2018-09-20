using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResettingInteractable : CustomInteractable {

    protected Vector3 m_originPosition;
    protected Quaternion m_originRotation;

    protected override void Awake()
    {
        base.Awake();
        RegisterInitialPosition();
    }

    protected override void OnFrameStarted(FreezeFrameState frame)
    {
        base.OnFrameStarted(frame);
        ResetPosition();
        
    }

    protected virtual void RegisterInitialPosition()
    {
        m_originPosition = transform.position;
        m_originRotation = transform.rotation;
    }

    protected virtual void ResetPosition()
    {
        if (snappedInSnapDropZone)
        {
            GetStoredSnapDropZone().ForceUnsnap();
        }

        transform.SetPositionAndRotation(m_originPosition, m_originRotation);
    }
}
