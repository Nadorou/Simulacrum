using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulacrumAssembly : MonoBehaviour {

    [SerializeField]
    private int m_targetAssemblyPieceAmount = 4;
    private int m_assemblyCounter = 0;

    [SerializeField]
    private FreezeFrameState m_triggeredFrame;
    [SerializeField]
    private float m_triggerTransitionTime;

    public void AddAssemblyPiece()
    {
        m_assemblyCounter++;
        AssemblyCheck();
    }

    private void AssemblyCheck()
    {
        if(m_assemblyCounter >= m_targetAssemblyPieceAmount)
        {
            AssemblyFinished();
        }
    }

    private void AssemblyFinished()
    {
        GameManager.instance.NewFreezeFrame(m_triggeredFrame, m_triggerTransitionTime);
    }
}
