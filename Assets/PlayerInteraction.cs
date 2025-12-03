using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How far the player can interact")]
    public float interactionRange = 3f;
    
    [Tooltip("Key to press for interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Raycast Settings")]
    public Transform cameraTransform;
    public LayerMask interactableLayer;

    [Header("UI")]
    public TMP_Text interactionPromptText;

    private Interactable currentInteractable;
    private GameObject currentInteractableObject;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogWarning("PlayerInteraction: No camera found!");
            }
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckForInteractable();
        
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            if (currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        bool foundInteractable = false;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionRange, interactableLayer, QueryTriggerInteraction.Collide))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                foundInteractable = true;
                
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    currentInteractableObject = hit.collider.gameObject;
                    UpdatePrompt(interactable.GetInteractionPrompt());
                }
            }
        }

        if (!foundInteractable && currentInteractable != null)
        {
            currentInteractable = null;
            currentInteractableObject = null;
            HidePrompt();
        }
    }

    private void UpdatePrompt(string message)
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = message;
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
        }
    }
}
