using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackoutAudioTransition : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float volume = 1f;
    
    [Header("Environment Settings")]
    [SerializeField] private Color blackoutColor = Color.black;
    [SerializeField] private bool disableLighting = true;
    
    [Header("Scene Transition")]
    [SerializeField] private bool autoTransition = true;
    [SerializeField] private float delayAfterAudio = 0.5f; // Small delay after audio ends
    
    private Camera vrCamera;
    private Light[] allLights;
    private bool[] originalLightStates;
    private GameObject blackoutQuad;
    private Material blackoutMaterial;
    private bool isPlaying = false;
    
    void Start()
    {
        SetupAudioSource();
        CreateBlackoutQuad();
        SetupBlackEnvironment();
        PlayAudioClip();
    }
    
    void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = false;
        
        if (audioClip == null)
        {
            Debug.LogError("No audio clip assigned to BlackoutAudioTransition!");
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
    
    private void CreateBlackoutQuad()
    {
        // Find the VR camera
        vrCamera = FindVRCamera();
        
        // Create a quad to display black overlay
        blackoutQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        blackoutQuad.name = "Blackout Overlay Quad";
        
        // Remove the collider (we don't need it)
        Destroy(blackoutQuad.GetComponent<Collider>());
        
        // Position it in front of the camera
        if (vrCamera != null)
        {
            blackoutQuad.transform.SetParent(vrCamera.transform);
            blackoutQuad.transform.localPosition = new Vector3(0, 0, 0.5f);
            blackoutQuad.transform.localRotation = Quaternion.identity;
            
            // Scale it to cover VR field of view completely
            float distance = 0.5f;
            float height = 2.0f * distance * Mathf.Tan(vrCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * vrCamera.aspect;
            
            // Make it large enough to ensure complete coverage
            blackoutQuad.transform.localScale = new Vector3(width * 1.5f, height * 1.5f, 1f);
        }
        
        // Create black material
        blackoutMaterial = new Material(Shader.Find("Sprites/Default"));
        if (blackoutMaterial.shader == null || blackoutMaterial.shader.name == "Hidden/InternalErrorShader")
        {
            blackoutMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        }
        
        blackoutMaterial.color = blackoutColor;
        
        // Ensure the material renders properly in VR
        blackoutMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        blackoutMaterial.SetInt("_ZWrite", 0);
        blackoutMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        
        // Apply material to quad
        blackoutQuad.GetComponent<Renderer>().material = blackoutMaterial;
        
        Debug.Log("Blackout quad created");
    }
    
    void SetupBlackEnvironment()
    {
        // Disable all lighting if requested
        if (disableLighting)
        {
            allLights = FindObjectsOfType<Light>();
            originalLightStates = new bool[allLights.Length];
            
            for (int i = 0; i < allLights.Length; i++)
            {
                originalLightStates[i] = allLights[i].enabled;
                allLights[i].enabled = false;
            }
        }
        
        Debug.Log("Environment set to pitch black");
    }
    
    public void PlayAudioClip()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.Play();
            isPlaying = true;
            Debug.Log("Playing audio clip: " + audioClip.name);
            
            // Start coroutine to check when audio finishes
            StartCoroutine(WaitForAudioToFinish());
        }
        else
        {
            Debug.LogWarning("Cannot play audio - missing AudioSource or AudioClip");
            if (autoTransition)
            {
                LoadNextScene();
            }
        }
    }
    
    private IEnumerator WaitForAudioToFinish()
    {
        // Wait while audio is playing
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        
        isPlaying = false;
        Debug.Log("Audio finished playing");
        
        // Wait for additional delay if specified
        if (delayAfterAudio > 0)
        {
            yield return new WaitForSeconds(delayAfterAudio);
        }
        
        // Transition to next scene if enabled
        if (autoTransition)
        {
            LoadNextScene();
        }
    }
    
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentSceneIndex + 1;
        
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Loading next scene index: " + nextIndex);
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("No next scene to load! This is the last scene in build order.");
        }
    }
    
    // Public method to manually stop audio and transition
    public void StopAudioAndTransition()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        StopAllCoroutines();
        
        if (autoTransition)
        {
            LoadNextScene();
        }
    }
    
    // Restore original environment settings (useful for testing)
    public void RestoreEnvironment()
    {
        // Hide blackout quad
        if (blackoutQuad != null)
        {
            blackoutQuad.SetActive(false);
        }
        
        // Restore lights
        if (allLights != null && originalLightStates != null)
        {
            for (int i = 0; i < allLights.Length && i < originalLightStates.Length; i++)
            {
                if (allLights[i] != null)
                {
                    allLights[i].enabled = originalLightStates[i];
                }
            }
        }
        
        Debug.Log("Environment restored to original state");
    }
    
    // Public properties for external scripts
    public bool IsPlaying => isPlaying;
    public float AudioLength => audioClip != null ? audioClip.length : 0f;
    public float CurrentTime => audioSource != null ? audioSource.time : 0f;
    
    void OnDestroy()
    {
        // Clean up if object is destroyed
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (blackoutQuad != null)
        {
            Destroy(blackoutQuad);
        }
    }
}