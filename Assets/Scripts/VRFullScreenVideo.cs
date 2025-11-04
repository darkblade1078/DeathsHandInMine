using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VRFullScreenVideo : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoClip videoClip;
    
    [Header("Display Settings")]
    public float quadDistance = 10f;
    public float quadWidth = 30f;
    public float quadHeight = 20f;
    public bool stretchToFit = true;
    public bool followCamera = false; // Changed to false by default
    public Material customMaterial; // Assign a material in Inspector
    
    [Header("Environment")]
    public bool makeEnvironmentBlack = false;
    public Color backgroundColor = Color.black;
    
    [Header("Scene Transition")]
    public bool autoTransition = true; // Automatically go to next scene when video ends
    
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private GameObject videoQuad;
    private Material videoMaterial;
    
    void Start()
    {
        if (makeEnvironmentBlack)
        {
            SetupBlackEnvironment();
        }
        
        CreateVideoQuad();
        SetupVideoPlayer();
        
        // Position the quad once at start
        PositionQuad();
        
        PlayVideo();
    }
    
    void SetupBlackEnvironment()
    {
        // Set camera background to solid black
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = backgroundColor;
            Debug.Log("Environment set to black");
        }
        
        // Find all OVRCameraRig cameras and set them too
        OVRCameraRig[] rigs = FindObjectsOfType<OVRCameraRig>();
        foreach (OVRCameraRig rig in rigs)
        {
            Camera[] cameras = rig.GetComponentsInChildren<Camera>();
            foreach (Camera cam in cameras)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
            }
        }
    }
    
    void CreateVideoQuad()
    {
        // Create a large quad
        videoQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        videoQuad.name = "FullScreenVideoQuad";
        
        // Remove the collider (we don't need it)
        Destroy(videoQuad.GetComponent<Collider>());
        
        // Use custom material if provided, otherwise create one
        if (customMaterial != null)
        {
            videoMaterial = new Material(customMaterial);
            Debug.Log("Using custom material: " + customMaterial.shader.name);
        }
        else
        {
            // Try to find a working shader
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("UI/Default");
            
            videoMaterial = new Material(shader);
            Debug.Log("Created material with shader: " + shader.name);
        }
        
        videoQuad.GetComponent<Renderer>().material = videoMaterial;
        
        // Set custom width and height to stretch
        videoQuad.transform.localScale = new Vector3(quadWidth, quadHeight, 1f);
        
        Debug.Log("Video quad created with dimensions: " + quadWidth + "x" + quadHeight);
    }
    
    void SetupVideoPlayer()
    {
        // Add VideoPlayer component
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        
        if (videoClip == null)
        {
            Debug.LogError("No video clip assigned!");
            return;
        }
        
        // Create render texture with exact video dimensions
        renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        
        // Configure video player
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false; // Changed to false so video ends and triggers transition
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.waitForFirstFrame = true;
        
        // Assign texture to material
        videoMaterial.mainTexture = renderTexture;
        
        // Events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.loopPointReached += OnVideoFinished; // Added event for when video ends
        
        videoPlayer.Prepare();
        
        Debug.Log("VideoPlayer setup complete");
    }
    
    void PositionQuad()
    {
        if (videoQuad != null && Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            videoQuad.transform.position = cam.position + cam.forward * quadDistance;
            videoQuad.transform.rotation = Quaternion.LookRotation(videoQuad.transform.position - cam.position);
        }
    }
    
    void Update()
    {
        // Only follow camera if enabled
        if (followCamera && videoQuad != null && Camera.main != null)
        {
            PositionQuad();
        }
    }
    
    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared - Duration: " + vp.length + " seconds");
    }
    
    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("Video error: " + message);
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing");
        
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
    
    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            Debug.Log("Playing video");
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        if (videoQuad != null)
        {
            videoQuad.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        
        if (videoQuad != null)
        {
            Destroy(videoQuad);
        }
    }
}