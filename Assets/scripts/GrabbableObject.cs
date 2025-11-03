using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    [Header("Grabbable Settings")]
    public bool isGrabbable = true;
    public float grabStrength = 1000f;
    
    private Rigidbody rb;
    private bool wasKinematic;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wasKinematic = rb.isKinematic;
        
        // Ensure the object has the Grabbable tag
        if (!gameObject.CompareTag("Grabbable") && !gameObject.name.ToLower().Contains("cube"))
        {
            gameObject.tag = "Grabbable";
        }
        
        Debug.Log($"Grabbable object initialized: {gameObject.name}");
    }
    
    public void OnGrabbed()
    {
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
        
        Debug.Log($"Object grabbed: {gameObject.name}");
    }
    
    public void OnReleased()
    {
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
        
        Debug.Log($"Object released: {gameObject.name}");
    }
    
    void OnDrawGizmos()
    {
        // Draw a green outline to show this object is grabbable
        if (isGrabbable)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}