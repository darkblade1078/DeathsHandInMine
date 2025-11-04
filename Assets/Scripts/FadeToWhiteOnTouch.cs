using System.Collections;
using UnityEngine;

public class FadeToWhiteOnTouch : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 0.1f; // Delay before fade starts
    [SerializeField] private float fadeDuration = 2f; // How long the fade takes
    [SerializeField] private Color fadeColor = Color.white; // Color to fade to
    [SerializeField] private bool fadeInOut = false; // If true, fades back out after reaching white
    [SerializeField] private float holdDuration = 1f; // How long to hold white before fading back
    
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player"; // Tag to identify player
    [SerializeField] private Transform ovrCameraRig; // Optional: specific camera rig reference
    
    private GameObject fadeOverlay;
    private Material fadeMaterial;
    private Camera vrCamera;
    private bool fadeTriggered = false;
    private bool fadeInProgress = false;
    
    void Start()
    {
        Debug.Log("FadeToWhiteOnTouch: Script started");
        
        // Ensure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log("FadeToWhiteOnTouch: Collider set as trigger");
        }
        else
        {
            Debug.LogWarning("FadeToWhiteOnTouch: No collider found! Add a collider component.");
        }
        
        // Setup fade overlay
        SetupFadeOverlay();
    }
    
    private Camera FindVRCamera()
    {
        // Try to find OVR Center Eye Anchor camera first
        GameObject centerEye = GameObject.Find("CenterEyeAnchor");
        if (centerEye != null)
        {
            Camera cam = centerEye.GetComponent<Camera>();
            if (cam != null) return cam;
        }
        
        // Fallback to main camera
        return Camera.main;
    }
    
    private void SetupFadeOverlay()
    {
        // Find the VR camera
        vrCamera = FindVRCamera();
        
        if (vrCamera == null)
        {
            Debug.LogError("FadeToWhiteOnTouch: Could not find VR camera for fade overlay!");
            return;
        }
        
        // Create a quad to display the fade overlay
        fadeOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fadeOverlay.name = "Fade To White Overlay";
        
        // Remove the collider (we don't need it)
        Destroy(fadeOverlay.GetComponent<Collider>());
        
        // Position it in front of the camera
        fadeOverlay.transform.SetParent(vrCamera.transform);
        fadeOverlay.transform.localPosition = new Vector3(0, 0, 0.3f);
        fadeOverlay.transform.localRotation = Quaternion.identity;
        
        // Scale it to cover VR field of view completely
        float distance = 0.3f;
        float height = 2.0f * distance * Mathf.Tan(vrCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * vrCamera.aspect;
        fadeOverlay.transform.localScale = new Vector3(width * 1.5f, height * 1.5f, 1f);
        
        // Create material for the overlay
        fadeMaterial = new Material(Shader.Find("Sprites/Default"));
        if (fadeMaterial.shader == null || fadeMaterial.shader.name == "Hidden/InternalErrorShader")
        {
            fadeMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        }
        
        // Start with transparent
        fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        // Ensure the material renders properly in VR
        fadeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        
        // Apply material to quad
        fadeOverlay.GetComponent<Renderer>().material = fadeMaterial;
        
        // Initially hide the overlay
        fadeOverlay.SetActive(false);
        
        Debug.Log("FadeToWhiteOnTouch: Fade overlay setup complete");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (fadeTriggered || fadeInProgress)
            return;
            
        // Check if the collider belongs to the player
        if (IsPlayer(other))
        {
            Debug.Log($"FadeToWhiteOnTouch: Player detected - {other.name}");
            StartFadeToWhite();
        }
    }
    
    private bool IsPlayer(Collider other)
    {
        // Check by tag
        if (other.CompareTag(playerTag))
            return true;
            
        // Check if it's part of OVR Camera Rig
        if (ovrCameraRig != null && other.transform.IsChildOf(ovrCameraRig))
            return true;
            
        // Check common VR player names
        string objName = other.name.ToLower();
        return objName.Contains("player") || 
               objName.Contains("ovrcamerarig") || 
               objName.Contains("centereyeanchor") ||
               objName.Contains("trackingspace") ||
               objName.Contains("hand") ||
               objName.Contains("controller");
    }
    
    public void StartFadeToWhite()
    {
        if (fadeTriggered)
        {
            Debug.Log("FadeToWhiteOnTouch: Fade already triggered!");
            return;
        }
        
        fadeTriggered = true;
        fadeInProgress = true;
        
        Debug.Log("FadeToWhiteOnTouch: Starting fade to white animation");
        
        // Show the overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(true);
        }
        
        // Start the fade coroutine
        StartCoroutine(FadeToWhiteCoroutine());
    }
    
    private IEnumerator FadeToWhiteCoroutine()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(fadeDelay);
        
        Debug.Log("FadeToWhiteOnTouch: Beginning fade animation");
        
        // Fade from transparent to white over the specified duration
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            if (fadeMaterial != null)
            {
                fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            
            yield return null;
        }
        
        // Ensure final alpha is 1 (completely white)
        if (fadeMaterial != null)
        {
            fadeMaterial.color = fadeColor;
        }
        
        Debug.Log("FadeToWhiteOnTouch: Fade to white complete");
        
        // If fade in/out is enabled, hold white then fade back out
        if (fadeInOut)
        {
            yield return new WaitForSeconds(holdDuration);
            
            Debug.Log("FadeToWhiteOnTouch: Starting fade back out");
            
            // Fade back out
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
                
                if (fadeMaterial != null)
                {
                    fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                }
                
                yield return null;
            }
            
            // Hide overlay
            if (fadeOverlay != null)
            {
                fadeOverlay.SetActive(false);
            }
            
            Debug.Log("FadeToWhiteOnTouch: Fade out complete");
            fadeInProgress = false;
        }
        else
        {
            // Stay white permanently
            fadeInProgress = false;
            Debug.Log("FadeToWhiteOnTouch: Staying white - effect complete");
        }
        
        // Call completion event
        OnFadeComplete();
    }
    
    // Virtual method that can be overridden or used for events
    protected virtual void OnFadeComplete()
    {
        Debug.Log("FadeToWhiteOnTouch: Fade effect completed");
        // Override this method or add UnityEvents here for custom behavior
        // For example: load next scene, trigger other effects, etc.
    }
    
    // Public methods for external control
    public void ManualTrigger()
    {
        StartFadeToWhite();
    }
    
    public void ResetFade()
    {
        fadeTriggered = false;
        fadeInProgress = false;
        
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(false);
        }
        
        if (fadeMaterial != null)
        {
            fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
        
        Debug.Log("FadeToWhiteOnTouch: Fade reset");
    }
    
    // Property to check if fade has been triggered
    public bool HasTriggered => fadeTriggered;
    public bool IsInProgress => fadeInProgress;
    
    void OnDestroy()
    {
        // Clean up
        if (fadeOverlay != null)
        {
            Destroy(fadeOverlay);
        }
    }
}