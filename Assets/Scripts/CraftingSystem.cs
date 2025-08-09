using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CraftingSystem : MonoBehaviour
{

    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;

    public List<string> inventoryItemList = new List<string>();


    // Categopry Buttons
    Button toolsBTN;

    // Craft Buttons
    Button craftAxeBTN;

    // Requirement Text
    TextMeshProUGUI AxeReq1, AxeReq2;

    public bool isOpen;

    // All Blueprints

    //public Blueprint AxeBLP;

    //private Blueprint AxeBLP = new Blueprint("Axe", 2, "Stone", 3, "Stick", 3);


    public static CraftingSystem Instance { get; set; }
    
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

        // Axe
        //GameObject gO1 = new GameObject("AxeBLP");
        //AxeBLP = gO1.AddComponent<Blueprint>();
        //AxeBLP = new Blueprint();
        /*
        AxeBLP.itemName = "Axe";
        AxeBLP.numOfRequirements = 2;
        AxeBLP.Req1 = "Stone";
        AxeBLP.Req1amount = 3;
        AxeBLP.Req2 = "Stick";
        AxeBLP.Req2amount = 3;
        */
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;
        Blueprint AxeBLP = new Blueprint("Axe", 2, "Stone", 3, "Stick", 3);

        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenToolsCategory(); });

        // Axe
        AxeReq1 = toolsScreenUI.transform.Find("Axe").transform.Find("Req1").GetComponent<TextMeshProUGUI>();
        AxeReq2 = toolsScreenUI.transform.Find("Axe").transform.Find("Req2").GetComponent<TextMeshProUGUI>();

        craftAxeBTN = toolsScreenUI.transform.Find("Axe").transform.Find("Button").GetComponent<Button>();
        craftAxeBTN.onClick.AddListener(delegate { CraftAnyItem(AxeBLP); });
    }

    void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(true);
    }

    void CraftAnyItem(Blueprint blueprintToCraft)
    {
        // Add item into inventory
        InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName);

        // Remove resources from inventory
        if (blueprintToCraft.numOfRequirements == 1)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
        }
        else if (blueprintToCraft.numOfRequirements == 2)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2amount);
        }

        // Refresh list
        StartCoroutine(calculate());
        
        RefreshNeededItems();

    }

    public IEnumerator calculate()
    {
        yield return new WaitForSeconds(1f);
        InventorySystem.Instance.ReCalculateList();
    }


    // Update is called once per frame
    void Update()
    {

        RefreshNeededItems();

        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {

            Debug.Log("c is pressed");
            craftingScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            isOpen = true;

        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            craftingScreenUI.SetActive(false);
            toolsScreenUI.SetActive(false);


            if (!InventorySystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            isOpen = false;
        }
    }

    private void RefreshNeededItems()
    {
        int stone_count = 0;
        int stick_count = 0;

        inventoryItemList = InventorySystem.Instance.itemList;

        foreach (string itemName in inventoryItemList)
        {
            switch (itemName)
            {
                case "Stone":
                    stone_count++;
                    break;
                case "Stick":
                    stick_count++;
                    break;
            }
        }

        // ***** AXE ***** //
        AxeReq1.text = "3 stone [" + stone_count + "]";
        AxeReq2.text = "3 stick [" + stick_count + "]";

        if(stone_count >= 3 && stick_count >= 3)
        {
            craftAxeBTN.gameObject.SetActive(true);
        }    
        else
        {
            craftAxeBTN.gameObject.SetActive(false);
        }

    }


}

