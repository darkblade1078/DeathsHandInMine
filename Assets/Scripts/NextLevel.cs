using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private Transform ovrCameraRig;
    [SerializeField] private Collider triggerCollider;
    
    [Header("Timer Reference")]
    [SerializeField] private BedTimer bedTimer;
    
    private bool timerStarted = false;
    
    void Start()
    {
        // Ensure trigger collider is set up properly
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Check if OVR Camera Rig is inside the trigger and start timer if not already started
        if (!timerStarted && IsOVRCameraRigInside())
        {
            StartBedTimer();
        }
    }
    
    private bool IsOVRCameraRigInside()
    {
        if (ovrCameraRig == null || triggerCollider == null)
            return false;
            
        // Check if the OVR Camera Rig position is inside the trigger collider bounds
        return triggerCollider.bounds.Contains(ovrCameraRig.position);
    }
    
    private void StartBedTimer()
    {
        if (bedTimer != null)
        {
            timerStarted = true;
            bedTimer.StartTimer();
            Debug.Log("OVR Camera Rig detected inside trigger! Starting bed timer.");
        }
        else
        {
            Debug.LogWarning("No BedTimer assigned to NextLevel script!");
        }
    }
    
    // Optional: Reset functionality
    public void ResetTrigger()
    {
        timerStarted = false;
    }
}