using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;
    public GameObject constructingScreenUI;

    public List<string> inventoryItemList = new List<string>();

    // Category Buttons
    Button toolsBTN;
    Button constructionBTN;

    // Craft Buttons (Tools)
    Button craftAxeBTN;

    // Craft Buttons (Construction)
    Button craftFoundationBTN;
    Button craftWallBTN;
    Button craftCampfireBTN;

    // Requirement Text (Tools)
    TextMeshProUGUI AxeReq1, AxeReq2;

    // Requirement Text (Construction)
    TextMeshProUGUI FoundationReq1, FoundationReq2;
    TextMeshProUGUI WallReq1, WallReq2;
    TextMeshProUGUI CampfireReq1, CampfireReq2;

    public bool isOpen;

    // Blueprints
    Blueprint AxeBLP;
    Blueprint FoundationBLP;
    Blueprint WallBLP;
    Blueprint CampfireBLP;

    // Toggle: keep buttons visible but disabled (true) or hide them (false)
    [Header("UX")]
    [SerializeField] bool showDisabledButtons = true;

    // === Costs (Stick + Stone) ===
    [Header("Construction Costs (Stick + Stone)")]
    [SerializeField] int foundationStick = 0;
    [SerializeField] int foundationStone = 2;

    [SerializeField] int wallStick = 3;
    [SerializeField] int wallStone = 1;

    [SerializeField] int campfireStick = 2;
    [SerializeField] int campfireStone = 2;

    public static CraftingSystem Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        isOpen = false;

        // --- Blueprints ---
        AxeBLP        = new Blueprint("Axe",        2, "Stone", 3,               "Stick", 3);
        FoundationBLP = new Blueprint("Foundation", 2, "Stick", foundationStick, "Stone", foundationStone);
        WallBLP       = new Blueprint("Wall",       2, "Stick", wallStick,       "Stone", wallStone);
        CampfireBLP   = new Blueprint("Campfire",   2, "Stick", campfireStick,   "Stone", campfireStone);

        // --- Category Buttons (under craftingScreenUI) ---
        toolsBTN = craftingScreenUI.transform.Find("ToolsButton")?.GetComponent<Button>();
        if (toolsBTN != null) toolsBTN.onClick.AddListener(OpenToolsCategory);
        else Debug.LogError("ToolsButton not found under craftingScreenUI");

        var constructionBtnTf = craftingScreenUI.transform.Find("ConstructionButton");
        if (constructionBtnTf != null)
        {
            constructionBTN = constructionBtnTf.GetComponent<Button>();
            constructionBTN.onClick.AddListener(OpenConstructionCategory);
        }
        else
        {
            Debug.LogError("ConstructionButton not found under craftingScreenUI");
        }

        // --- Tools UI wiring ---
        var axeTf = toolsScreenUI.transform.Find("Axe");
        if (axeTf != null)
        {
            AxeReq1 = axeTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            AxeReq2 = axeTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftAxeBTN = axeTf.Find("Button").GetComponent<Button>();
            craftAxeBTN.onClick.AddListener(delegate { CraftAnyItem(AxeBLP); });
        }
        else Debug.LogError("toolsScreenUI/Axe not found");

        // --- Construction UI wiring ---
        var foundationTf = constructingScreenUI.transform.Find("Foundation");
        if (foundationTf != null)
        {
            FoundationReq1 = foundationTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            FoundationReq2 = foundationTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftFoundationBTN = foundationTf.Find("Button").GetComponent<Button>();
            craftFoundationBTN.onClick.AddListener(delegate { CraftAnyItem(FoundationBLP); });
        }
        else Debug.LogError("constructingScreenUI/Foundation not found");

        var wallTf = constructingScreenUI.transform.Find("Wall");
        if (wallTf != null)
        {
            WallReq1 = wallTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            WallReq2 = wallTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftWallBTN = wallTf.Find("Button").GetComponent<Button>();
            craftWallBTN.onClick.AddListener(delegate { CraftAnyItem(WallBLP); });
        }
        else Debug.LogError("constructingScreenUI/Wall not found");

        var campfireTf = constructingScreenUI.transform.Find("Campfire");
        if (campfireTf != null)
        {
            CampfireReq1 = campfireTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            CampfireReq2 = campfireTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftCampfireBTN = campfireTf.Find("Button").GetComponent<Button>();
            craftCampfireBTN.onClick.AddListener(delegate { CraftAnyItem(CampfireBLP); });
        }
        else Debug.LogError("constructingScreenUI/Campfire not found");
    }

    // === Category switching ===
    public void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(true);
        if (constructingScreenUI != null) constructingScreenUI.SetActive(false);
    }

    public void OpenConstructionCategory()
    {
        craftingScreenUI.SetActive(false);
        if (toolsScreenUI != null) toolsScreenUI.SetActive(false);
        if (constructingScreenUI != null) constructingScreenUI.SetActive(true);
    }

    // === Crafting ===
    void CraftAnyItem(Blueprint blueprintToCraft)
    {
        // Guard: double-check resources so UI and inventory never desync
        if (blueprintToCraft.numOfRequirements == 2)
        {
            if (!HasEnough(blueprintToCraft.Req1, blueprintToCraft.Req1amount)) return;
            if (!HasEnough(blueprintToCraft.Req2, blueprintToCraft.Req2amount)) return;
        }

        // Add crafted item to inventory
        InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName);

        // Remove resources
        if (blueprintToCraft.numOfRequirements == 1)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
        }
        else if (blueprintToCraft.numOfRequirements == 2)
        {
            if (blueprintToCraft.Req1amount > 0)
                InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
            if (blueprintToCraft.Req2amount > 0)
                InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2amount);
        }

        // Refresh
        StartCoroutine(calculate());
        RefreshNeededItems();
    }

    bool HasEnough(string itemName, int needed)
    {
        if (needed <= 0) return true;
        int count = 0;
        foreach (var n in InventorySystem.Instance.itemList)
            if (n == itemName) count++;
        return count >= needed;
    }

    public IEnumerator calculate()
    {
        yield return new WaitForSeconds(0.1f);
        InventorySystem.Instance.ReCalculateList();
    }

    void Update()
    {
        RefreshNeededItems();

        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {
            craftingScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            craftingScreenUI.SetActive(false);
            toolsScreenUI.SetActive(false);
            if (constructingScreenUI != null) constructingScreenUI.SetActive(false);

            if (!InventorySystem.Instance.isOpen)
                Cursor.lockState = CursorLockMode.Locked;

            isOpen = false;
        }
    }

    public void RefreshNeededItems()
    {
        int stone_count = 0;
        int stick_count = 0;

        inventoryItemList = InventorySystem.Instance.itemList;

        foreach (string itemName in inventoryItemList)
        {
            switch (itemName)
            {
                case "Stone": stone_count++; break;
                case "Stick": stick_count++; break;
            }
        }

        // ***** AXE (Stone + Stick) *****
        if (AxeReq1 != null) AxeReq1.text = "3 stone [" + stone_count + "]";
        if (AxeReq2 != null) AxeReq2.text = "3 stick [" + stick_count + "]";
        SetButtonState(craftAxeBTN, stone_count >= 3 && stick_count >= 3);

        // ***** FOUNDATION (Stick + Stone) *****
        if (FoundationReq1 != null) FoundationReq1.text = foundationStick + " stick [" + stick_count + "]";
        if (FoundationReq2 != null) FoundationReq2.text = foundationStone + " stone [" + stone_count + "]";
        SetButtonState(craftFoundationBTN, stick_count >= foundationStick && stone_count >= foundationStone);

        // ***** WALL (Stick + Stone) *****
        if (WallReq1 != null) WallReq1.text = wallStick + " stick [" + stick_count + "]";
        if (WallReq2 != null) WallReq2.text = wallStone + " stone [" + stone_count + "]";
        SetButtonState(craftWallBTN, stick_count >= wallStick && stone_count >= wallStone);

        // ***** CAMPFIRE (Stick + Stone) *****
        if (CampfireReq1 != null) CampfireReq1.text = campfireStick + " stick [" + stick_count + "]";
        if (CampfireReq2 != null) CampfireReq2.text = campfireStone + " stone [" + stone_count + "]";
        SetButtonState(craftCampfireBTN, stick_count >= campfireStick && stone_count >= campfireStone);
    }

    void SetButtonState(Button btn, bool canCraft)
    {
        if (btn == null) return;
        if (showDisabledButtons)
        {
            btn.gameObject.SetActive(true);
            btn.interactable = canCraft;
        }
        else
        {
            btn.gameObject.SetActive(canCraft);
        }
    }
}
