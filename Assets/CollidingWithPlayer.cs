using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidingWithPlayer : MonoBehaviour
{
    // Reference to the RecordPlayer script
    public RecordPlayer recordPlayer;

    private void OnTriggerEnter(Collider other)
    {
        // Check if we collided with the record player
        if (other.GetComponent<RecordPlayer>() != null)
        {
            recordPlayer = other.GetComponent<RecordPlayer>();
            recordPlayer.recordPlayerActive = true;
            Debug.Log("Vinyl placed on record player — activating!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<RecordPlayer>() != null)
        {
            recordPlayer.recordPlayerActive = false;
            Debug.Log("Vinyl removed — stopping record player.");
        }
    }
}