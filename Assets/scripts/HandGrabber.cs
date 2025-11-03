using UnityEngine;
using UnityEngine.XR;

public class HandGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 0.15f;
    public LayerMask grabbableLayer = -1;
    public KeyCode leftGrabKey = KeyCode.Q;
    public KeyCode rightGrabKey = KeyCode.E;
    
    [Header("Hand References")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool useKeyboardInput = true;
    
    private GameObject leftGrabbedObject;
    private GameObject rightGrabbedObject;
    private Rigidbody leftGrabbedRigidbody;
    private Rigidbody rightGrabbedRigidbody;
    
    void Start()
    {
        // If hand transforms are not assigned, try to find them
        if (leftHandTransform == null)
        {
            // Try common OVR hand names
            GameObject leftHand = GameObject.Find("LeftHand") ?? 
                                GameObject.Find("OVRHandPrefab_L") ?? 
                                GameObject.Find("LeftControllerAnchor");
            if (leftHand != null) leftHandTransform = leftHand.transform;
        }
        
        if (rightHandTransform == null)
        {
            // Try common OVR hand names
            GameObject rightHand = GameObject.Find("RightHand") ?? 
                                 GameObject.Find("OVRHandPrefab_R") ?? 
                                 GameObject.Find("RightControllerAnchor");
            if (rightHand != null) rightHandTransform = rightHand.transform;
        }
        
        Debug.Log($"Left hand transform: {leftHandTransform != null}");
        Debug.Log($"Right hand transform: {rightHandTransform != null}");
        
        if (useKeyboardInput)
        {
            Debug.Log($"Keyboard controls: Left grab = {leftGrabKey}, Right grab = {rightGrabKey}");
        }
    }
    
    void Update()
    {
        // Use keyboard input for testing (can be replaced with hand tracking later)
        bool leftGrabbing = useKeyboardInput ? Input.GetKey(leftGrabKey) : false;
        bool rightGrabbing = useKeyboardInput ? Input.GetKey(rightGrabKey) : false;
        
        UpdateHandGrabbing(leftGrabbing, leftHandTransform, ref leftGrabbedObject, ref leftGrabbedRigidbody);
        UpdateHandGrabbing(rightGrabbing, rightHandTransform, ref rightGrabbedObject, ref rightGrabbedRigidbody);
    }
    
    void UpdateHandGrabbing(bool isGrabbing, Transform handTransform, ref GameObject grabbedObject, ref Rigidbody grabbedRigidbody)
    {
        if (handTransform == null) return;
        
        if (isGrabbing && grabbedObject == null)
        {
            // Try to grab nearby object
            TryGrabObject(handTransform, ref grabbedObject, ref grabbedRigidbody);
        }
        else if (!isGrabbing && grabbedObject != null)
        {
            // Release object
            ReleaseObject(ref grabbedObject, ref grabbedRigidbody);
        }
        
        // Update grabbed object position
        if (grabbedObject != null)
        {
            if (grabbedRigidbody != null)
            {
                // Use physics-based movement for smoother interaction
                Vector3 targetPosition = handTransform.position;
                Vector3 force = (targetPosition - grabbedRigidbody.position) * 1500f;
                grabbedRigidbody.AddForce(force);
                
                // Add some damping
                grabbedRigidbody.velocity *= 0.85f;
                grabbedRigidbody.angularVelocity *= 0.85f;
            }
            else
            {
                // Direct position/rotation setting for kinematic objects
                grabbedObject.transform.position = handTransform.position;
                grabbedObject.transform.rotation = handTransform.rotation;
            }
        }
    }
    
    // This method can be extended later with actual hand tracking
    public void SetLeftHandGrabbing(bool grabbing)
    {
        // This can be called from OVR hand tracking components
    }
    
    public void SetRightHandGrabbing(bool grabbing)
    {
        // This can be called from OVR hand tracking components
    }
    
    void TryGrabObject(Transform handTransform, ref GameObject grabbedObject, ref Rigidbody grabbedRigidbody)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(handTransform.position, grabDistance, grabbableLayer);
        
        foreach (Collider col in nearbyObjects)
        {
            // Check if object is grabbable
            if (col.gameObject.CompareTag("Grabbable") || col.gameObject.name.ToLower().Contains("cube"))
            {
                grabbedObject = col.gameObject;
                grabbedRigidbody = col.GetComponent<Rigidbody>();
                
                Debug.Log($"Grabbed: {grabbedObject.name}");
                
                // Make object kinematic while grabbed for better control
                if (grabbedRigidbody != null)
                {
                    grabbedRigidbody.isKinematic = true;
                }
                
                break;
            }
        }
    }
    
    void ReleaseObject(ref GameObject grabbedObject, ref Rigidbody grabbedRigidbody)
    {
        if (grabbedObject != null)
        {
            Debug.Log($"Released: {grabbedObject.name}");
            
            // Restore physics
            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = false;
            }
            
            grabbedObject = null;
            grabbedRigidbody = null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw grab spheres
        Gizmos.color = Color.green;
        if (leftHandTransform != null)
        {
            Gizmos.DrawWireSphere(leftHandTransform.position, grabDistance);
        }
        
        if (rightHandTransform != null)
        {
            Gizmos.DrawWireSphere(rightHandTransform.position, grabDistance);
        }
        
        // Draw grab status
        if (Application.isPlaying)
        {
            // Show grabbed objects
            if (leftGrabbedObject != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftHandTransform.position, leftGrabbedObject.transform.position);
            }
            
            if (rightGrabbedObject != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rightHandTransform.position, rightGrabbedObject.transform.position);
            }
        }
    }
}