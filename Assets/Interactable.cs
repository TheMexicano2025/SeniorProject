using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{

    string GetInteractionPrompt();
    
    void Interact (GameObject player);

    bool CanInteract(GameObject player);
}
