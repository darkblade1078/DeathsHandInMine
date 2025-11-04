using System.Collections;
using UnityEngine;

public class FadeFromBlack : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 0.5f; // Delay before fade starts
    [SerializeField] private float fadeDuration = 2f; // How long the fade takes
    [SerializeField] private Color fadeColor = Color.black; // Color to fade from
    [SerializeField] private bool autoStart = true; // Auto start fade on scene load
    
    private GameObject fadeOverlay;
    private Material fadeMaterial;
    private Camera vrCamera;
    private bool fadeInProgress = false;
    
    void Start()
    {
        Debug.Log("FadeFromBlack: Script started successfully!");
        
        // Setup fade overlay
        SetupFadeOverlay();
        
        if (autoStart)
        {
            StartFadeFromBlack();
        }
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
            Debug.LogError("FadeFromBlack: Could not find VR camera for fade overlay!");
            return;
        }
        
        // Create a quad to display the fade overlay
        fadeOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fadeOverlay.name = "Fade From Black Overlay";
        
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
        
        // Start with full opacity (completely black)
        fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        
        // Ensure the material renders properly in VR
        fadeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        
        // Apply material to quad
        fadeOverlay.GetComponent<Renderer>().material = fadeMaterial;
        
        Debug.Log("FadeFromBlack: Fade overlay setup complete - starting black");
    }
    
    public void StartFadeFromBlack()
    {
        if (fadeInProgress)
        {
            Debug.Log("FadeFromBlack: Fade already in progress!");
            return;
        }
        
        fadeInProgress = true;
        
        Debug.Log("FadeFromBlack: Starting fade from black animation");
        
        // Ensure overlay is visible and black
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(true);
            if (fadeMaterial != null)
            {
                fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            }
        }
        
        // Start the fade coroutine
        StartCoroutine(FadeFromBlackCoroutine());
    }
    
    private IEnumerator FadeFromBlackCoroutine()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(fadeDelay);
        
        Debug.Log("FadeFromBlack: Beginning fade animation");
        
        // Fade from black to transparent over the specified duration
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration); // Start at 1, fade to 0
            
            if (fadeMaterial != null)
            {
                fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            
            yield return null;
        }
        
        // Ensure final alpha is 0 (completely transparent)
        if (fadeMaterial != null)
        {
            fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
        
        Debug.Log("FadeFromBlack: Fade animation complete - hiding overlay");
        
        // Hide the overlay completely
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(false);
        }
        
        fadeInProgress = false;
        
        // Call completion event
        OnFadeComplete();
    }
    
    // Virtual method that can be overridden or used for events
    protected virtual void OnFadeComplete()
    {
        Debug.Log("FadeFromBlack: Fade complete - scene fully visible");
        // Override this method or add UnityEvents here for custom behavior
    }
    
    // Public method to manually trigger fade (useful for other scripts)
    public void TriggerFade()
    {
        StartFadeFromBlack();
    }
    
    // Public method to instantly show scene (skip fade)
    public void InstantShow()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(false);
        }
        
        if (fadeMaterial != null)
        {
            fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
        
        fadeInProgress = false;
        Debug.Log("FadeFromBlack: Instant show - fade skipped");
    }
    
    // Public method to reset to black (useful for restarting)
    public void ResetToBlack()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.SetActive(true);
        }
        
        if (fadeMaterial != null)
        {
            fadeMaterial.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }
        
        fadeInProgress = false;
        Debug.Log("FadeFromBlack: Reset to black");
    }
    
    // Property to check if fade is currently running
    public bool IsFading => fadeInProgress;
    
    void OnDestroy()
    {
        // Clean up
        if (fadeOverlay != null)
        {
            Destroy(fadeOverlay);
        }
    }
}