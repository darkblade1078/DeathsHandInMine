using System.Collections;
using UnityEngine;

public class AngelicDoorEffect : MonoBehaviour
{
    [Header("Collider Detection")]
    [SerializeField] private BoxCollider targetBoxCollider; // The collider to monitor
    [SerializeField] private float checkInterval = 0.1f; // How often to check collider state
    
    [Header("Door Object")]
    [SerializeField] private GameObject doorObject; // The door to apply effect to
    [SerializeField] private Renderer doorRenderer; // Door's renderer component
    
    [Header("Angelic Effect Settings")]
    [SerializeField] private Material angelicMaterial; // Material with angelic shader
    [SerializeField] private Material originalMaterial; // Original door material
    [SerializeField] private bool useEmissionGlow = true;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private float glowIntensity = 2f;
    
    [Header("Light Effect")]
    [SerializeField] private Light angelicLight; // Optional light component
    [SerializeField] private bool createLightAutomatically = true;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private float lightIntensity = 3f;
    [SerializeField] private float lightRange = 10f;
    
    [Header("Animation")]
    [SerializeField] private bool animateEffect = true;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.3f;
    
    private bool colliderWasEnabled = false;
    private bool effectActive = false;
    private Material runtimeMaterial; // Copy of material for runtime modifications
    private float baseGlowIntensity;
    private float baseLightIntensity;
    
    void Start()
    {
        Debug.Log("AngelicDoorEffect: Script started");
        
        // Setup components
        SetupDoorRenderer();
        SetupLight();
        
        // Store original values
        baseGlowIntensity = glowIntensity;
        baseLightIntensity = lightIntensity;
        
        // Start checking collider state
        StartCoroutine(CheckColliderState());
    }
    
    void SetupDoorRenderer()
    {
        // Auto-find door object if not assigned
        if (doorObject == null)
        {
            doorObject = gameObject; // Use the same GameObject
        }
        
        // Auto-find renderer if not assigned
        if (doorRenderer == null)
        {
            doorRenderer = doorObject.GetComponent<Renderer>();
            if (doorRenderer == null)
            {
                Debug.LogError("AngelicDoorEffect: No Renderer found on door object!");
                return;
            }
        }
        
        // Store original material if not set
        if (originalMaterial == null)
        {
            originalMaterial = doorRenderer.material;
        }
        
        Debug.Log("AngelicDoorEffect: Door renderer setup complete");
    }
    
    void SetupLight()
    {
        if (createLightAutomatically && angelicLight == null)
        {
            // Create a new light component
            GameObject lightObject = new GameObject("Angelic Light");
            lightObject.transform.SetParent(doorObject.transform);
            lightObject.transform.localPosition = Vector3.forward * 0.5f; // Slightly in front of door
            
            angelicLight = lightObject.AddComponent<Light>();
            angelicLight.type = LightType.Point;
            angelicLight.color = lightColor;
            angelicLight.intensity = 0f; // Start with no intensity
            angelicLight.range = lightRange;
            angelicLight.shadows = LightShadows.Soft;
            
            Debug.Log("AngelicDoorEffect: Auto-created angelic light");
        }
        
        // Ensure light starts disabled
        if (angelicLight != null)
        {
            angelicLight.intensity = 0f;
        }
    }
    
    IEnumerator CheckColliderState()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            if (targetBoxCollider != null)
            {
                bool isCurrentlyEnabled = targetBoxCollider.enabled;
                
                // Check for state change
                if (isCurrentlyEnabled && !colliderWasEnabled)
                {
                    // Collider just got enabled
                    OnColliderEnabled();
                }
                else if (!isCurrentlyEnabled && colliderWasEnabled)
                {
                    // Collider just got disabled
                    OnColliderDisabled();
                }
                
                colliderWasEnabled = isCurrentlyEnabled;
            }
        }
    }
    
    void OnColliderEnabled()
    {
        Debug.Log("AngelicDoorEffect: Box collider enabled - activating angelic effect!");
        
        if (!effectActive)
        {
            effectActive = true;
            StartCoroutine(ActivateAngelicEffect());
        }
    }
    
    void OnColliderDisabled()
    {
        Debug.Log("AngelicDoorEffect: Box collider disabled - deactivating angelic effect!");
        
        if (effectActive)
        {
            effectActive = false;
            StartCoroutine(DeactivateAngelicEffect());
        }
    }
    
    IEnumerator ActivateAngelicEffect()
    {
        // Apply angelic material if provided
        if (angelicMaterial != null && doorRenderer != null)
        {
            // Create runtime copy to avoid modifying the original
            runtimeMaterial = new Material(angelicMaterial);
            doorRenderer.material = runtimeMaterial;
        }
        else if (doorRenderer != null)
        {
            // Use original material but enable emission
            runtimeMaterial = new Material(originalMaterial);
            doorRenderer.material = runtimeMaterial;
            
            if (useEmissionGlow && runtimeMaterial.HasProperty("_EmissionColor"))
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
            }
        }
        
        if (animateEffect)
        {
            // Animate the effect fade-in
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeInDuration;
                
                UpdateEffectIntensity(progress);
                yield return null;
            }
        }
        
        UpdateEffectIntensity(1f);
        
        // Start pulsing effect if enabled
        if (pulseEffect)
        {
            StartCoroutine(PulseEffect());
        }
    }
    
    IEnumerator DeactivateAngelicEffect()
    {
        // Stop pulsing
        StopCoroutine(PulseEffect());
        
        if (animateEffect)
        {
            // Animate the effect fade-out
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = 1f - (elapsedTime / fadeInDuration);
                
                UpdateEffectIntensity(progress);
                yield return null;
            }
        }
        
        // Restore original material
        if (doorRenderer != null && originalMaterial != null)
        {
            doorRenderer.material = originalMaterial;
        }
        
        // Turn off light
        if (angelicLight != null)
        {
            angelicLight.intensity = 0f;
        }
    }
    
    IEnumerator PulseEffect()
    {
        while (effectActive)
        {
            float pulseValue = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            float intensity = 1f + pulseValue;
            
            UpdateEffectIntensity(intensity);
            yield return null;
        }
    }
    
    void UpdateEffectIntensity(float intensity)
    {
        // Update emission glow
        if (useEmissionGlow && runtimeMaterial != null)
        {
            if (runtimeMaterial.HasProperty("_EmissionColor"))
            {
                Color emissionColor = glowColor * (baseGlowIntensity * intensity);
                runtimeMaterial.SetColor("_EmissionColor", emissionColor);
            }
        }
        
        // Update light intensity
        if (angelicLight != null)
        {
            angelicLight.intensity = baseLightIntensity * intensity;
        }
    }
    
    // Public methods for external control
    public void ForceActivateEffect()
    {
        OnColliderEnabled();
    }
    
    public void ForceDeactivateEffect()
    {
        OnColliderDisabled();
    }
    
    public void SetGlowIntensity(float newIntensity)
    {
        glowIntensity = newIntensity;
        baseGlowIntensity = newIntensity;
    }
    
    public void SetLightIntensity(float newIntensity)
    {
        lightIntensity = newIntensity;
        baseLightIntensity = newIntensity;
    }
    
    // Property to check if effect is currently active
    public bool IsEffectActive => effectActive;
    
    void OnDestroy()
    {
        // Clean up runtime material
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}