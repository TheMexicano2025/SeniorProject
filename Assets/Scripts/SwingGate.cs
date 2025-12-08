using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingGate : MonoBehaviour, Interactable
{
    [Header("Door References")]
    [Tooltip("Left door (Fence_1)")]
    public Transform leftDoor;
    
    [Tooltip("Right door (Fence_2)")]
    public Transform rightDoor;
    
    [Header("Left Door Settings")]
    [Tooltip("Left door closed rotation Y")]
    public float leftClosedRotationY = -180f;
    
    [Tooltip("Left door open rotation Y")]
    public float leftOpenRotationY = -90f;
    
    [Header("Right Door Settings")]
    [Tooltip("Right door closed rotation Y")]
    public float rightClosedRotationY = 0f;
    
    [Tooltip("Right door open rotation Y")]
    public float rightOpenRotationY = -90f;
    
    [Header("Animation Settings")]
    [Tooltip("How fast the doors swing (seconds)")]
    public float swingDuration = 1f;
    
    [Tooltip("Are the doors currently open?")]
    public bool isOpen = false;
    
    private bool isSwinging = false;

    public bool CanInteract(GameObject player)
    {
        return !isSwinging;
    }

    public void Interact(GameObject player)
    {
        if (!isSwinging)
        {
            StartCoroutine(ToggleDoors());
        }
    }

    public string GetInteractionPrompt()
    {
        if (isSwinging)
        {
            return "";
        }
        
        return isOpen ? "Close Gate [E]" : "Open Gate [E]";
    }

    private IEnumerator ToggleDoors()
    {
        isSwinging = true;
        
        float leftStartRotation = isOpen ? leftOpenRotationY : leftClosedRotationY;
        float leftEndRotation = isOpen ? leftClosedRotationY : leftOpenRotationY;
        
        float rightStartRotation = isOpen ? rightOpenRotationY : rightClosedRotationY;
        float rightEndRotation = isOpen ? rightClosedRotationY : rightOpenRotationY;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < swingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / swingDuration;
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            if (leftDoor != null)
            {
                float currentLeftRotation = Mathf.Lerp(leftStartRotation, leftEndRotation, smoothT);
                leftDoor.rotation = Quaternion.Euler(0f, currentLeftRotation, 0f);
            }
            
            if (rightDoor != null)
            {
                float currentRightRotation = Mathf.Lerp(rightStartRotation, rightEndRotation, smoothT);
                rightDoor.rotation = Quaternion.Euler(0f, currentRightRotation, 0f);
            }
            
            yield return null;
        }
        
        if (leftDoor != null)
        {
            leftDoor.rotation = Quaternion.Euler(0f, leftEndRotation, 0f);
        }
        
        if (rightDoor != null)
        {
            rightDoor.rotation = Quaternion.Euler(0f, rightEndRotation, 0f);
        }
        
        isOpen = !isOpen;
        isSwinging = false;
        
        Debug.Log($"Gate is now {(isOpen ? "open" : "closed")}");
    }
}

