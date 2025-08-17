using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{

    public static SelectionManager Instance { get; set; }


    public bool onTarget;

    public GameObject interaction_Info_UI;
    TextMeshProUGUI interaction_text;

    public GameObject selectedObject;

    private void Start()
    {
        onTarget = false;
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;

            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
                selectedObject = interactable.gameObject;
                interaction_text.text = interactable.GetItemName();
                interaction_Info_UI.SetActive(true);
            }
            else // if there is a hit, but without an interactable script
            {
                onTarget = false;
                interaction_Info_UI.SetActive(false);
            }

        }
        else // if there is no hit
        {
            onTarget = false;
            interaction_Info_UI.SetActive(false);
        }
    }
    public void EnableSelection()
{
    enabled = true;        // turn this component back on so Update() runs
    ClearSelectionUI();    // reset UI state
}

public void DisableSelection()
{
    enabled = false;       // stop running Update() while menus are open
    ClearSelectionUI();
}

private void ClearSelectionUI()
{
    onTarget = false;
    selectedObject = null;
    if (interaction_Info_UI != null)
        interaction_Info_UI.SetActive(false);
}

}