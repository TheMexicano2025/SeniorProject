using UnityEngine;
using TMPro;

// this script handles all player interactions in the game
// it shoots a ray from the camera to detect interactable objects
// and shows prompts when you can interact with something
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f; 
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
        // find the main camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }

        // hide the prompt at start
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // constantly check what we're looking at
        CheckForInteractable();
        
        // if E is pressed and we're looking at something interactable
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            if (currentInteractable.CanInteract(gameObject))
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    // shoot a ray forward to see if we're looking at anything interactable
    private void CheckForInteractable()
    {
        RaycastHit hit;
        bool foundInteractable = false;

        // shoot ray from camera forward
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionRange, interactableLayer, QueryTriggerInteraction.Collide))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            // found something interactable
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                foundInteractable = true;
                
                // if this is a new object update the prompt
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    currentInteractableObject = hit.collider.gameObject;
                    UpdatePrompt(interactable.GetInteractionPrompt());
                }
            }
        }

        // stopped looking at the interactable so hide the prompt
        if (!foundInteractable && currentInteractable != null)
        {
            currentInteractable = null;
            currentInteractableObject = null;
            HidePrompt();
        }
    }

    // show the interaction prompt text
    private void UpdatePrompt(string message)
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = message;
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    // hide the prompt when not looking at anything
    private void HidePrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    // draws a yellow line in the scene view to see the interaction range
    private void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionRange);
        }
    }
}
