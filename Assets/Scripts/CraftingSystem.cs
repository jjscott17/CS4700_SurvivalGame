using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI;
    public GameObject toolsScreenUI;

    // Construction category screen
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

    // Construction Blueprints
    Blueprint FoundationBLP;
    Blueprint WallBLP;
    Blueprint CampfireBLP;

    // === Tunable costs (Stick + Stone for construction) ===
    [Header("Construction Costs (Stick + Stone)")]
    [SerializeField] int foundationStick = 4;
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
        // Axe: Stone + Stick (fixed amounts)
        AxeBLP        = new Blueprint("Axe",        2, "Stone", 3,                "Stick", 3);

        // Construction: Stick + Stone (amounts from inspector)
        FoundationBLP = new Blueprint("Foundation", 2, "Stick", foundationStick,  "Stone", foundationStone);
        WallBLP       = new Blueprint("Wall",       2, "Stick", wallStick,        "Stone", wallStone);
        CampfireBLP   = new Blueprint("Campfire",   2, "Stick", campfireStick,    "Stone", campfireStone);

        // --- Category Buttons ---
        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(OpenToolsCategory);

        var constructionBtnTf = craftingScreenUI.transform.Find("ConstructionButton");
        if (constructionBtnTf != null)
        {
            constructionBTN = constructionBtnTf.GetComponent<Button>();
            constructionBTN.onClick.AddListener(OpenConstructionCategory);
        }
        else
        {
            Debug.LogWarning("CraftingSystem: 'ConstructionButton' not found under craftingScreenUI. Did you create it?");
        }

        // --- Tools UI wiring ---
        AxeReq1 = toolsScreenUI.transform.Find("Axe").transform.Find("Req1").GetComponent<TextMeshProUGUI>();
        AxeReq2 = toolsScreenUI.transform.Find("Axe").transform.Find("Req2").GetComponent<TextMeshProUGUI>();
        craftAxeBTN = toolsScreenUI.transform.Find("Axe").transform.Find("Button").GetComponent<Button>();
        craftAxeBTN.onClick.AddListener(delegate { CraftAnyItem(AxeBLP); });

        // --- Construction UI wiring (Foundation) ---
        var foundationTf = constructingScreenUI.transform.Find("Foundation");
        if (foundationTf != null)
        {
            FoundationReq1 = foundationTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            FoundationReq2 = foundationTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftFoundationBTN = foundationTf.Find("Button").GetComponent<Button>();
            craftFoundationBTN.onClick.AddListener(delegate { CraftAnyItem(FoundationBLP); });
        }

        // Wall
        var wallTf = constructingScreenUI.transform.Find("Wall");
        if (wallTf != null)
        {
            WallReq1 = wallTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            WallReq2 = wallTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftWallBTN = wallTf.Find("Button").GetComponent<Button>();
            craftWallBTN.onClick.AddListener(delegate { CraftAnyItem(WallBLP); });
        }

        // Campfire
        var campfireTf = constructingScreenUI.transform.Find("Campfire");
        if (campfireTf != null)
        {
            CampfireReq1 = campfireTf.Find("Req1").GetComponent<TextMeshProUGUI>();
            CampfireReq2 = campfireTf.Find("Req2").GetComponent<TextMeshProUGUI>();
            craftCampfireBTN = campfireTf.Find("Button").GetComponent<Button>();
            craftCampfireBTN.onClick.AddListener(delegate { CraftAnyItem(CampfireBLP); });
        }
    }

    // === Category switching ===
    void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(true);
        if (constructingScreenUI != null) constructingScreenUI.SetActive(false);
    }

    void OpenConstructionCategory()
    {
        craftingScreenUI.SetActive(false);
        if (toolsScreenUI != null) toolsScreenUI.SetActive(false);
        if (constructingScreenUI != null) constructingScreenUI.SetActive(true);
    }

    // === Crafting ===
    void CraftAnyItem(Blueprint blueprintToCraft)
    {
        // Optional: sanity check before crafting (prevents desync if inventory changed)
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

        // Refresh list
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
        yield return new WaitForSeconds(1f);
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

        // ***** AXE (Stone + Stick) ***** //
        if (AxeReq1 != null) AxeReq1.text = "3 stone [" + stone_count + "]";
        if (AxeReq2 != null) AxeReq2.text = "3 stick [" + stick_count + "]";
        if (craftAxeBTN != null) craftAxeBTN.gameObject.SetActive(stone_count >= 3 && stick_count >= 3);

        // ***** FOUNDATION (Stick + Stone) ***** //
        if (FoundationReq1 != null) FoundationReq1.text = foundationStick + " stick [" + stick_count + "]";
        if (FoundationReq2 != null) FoundationReq2.text = foundationStone + " stone [" + stone_count + "]";
        if (craftFoundationBTN != null)
            craftFoundationBTN.gameObject.SetActive(stick_count >= foundationStick && stone_count >= foundationStone);

        // ***** WALL (Stick + Stone) ***** //
        if (WallReq1 != null) WallReq1.text = wallStick + " stick [" + stick_count + "]";
        if (WallReq2 != null) WallReq2.text = wallStone + " stone [" + stone_count + "]";
        if (craftWallBTN != null)
        {
            bool ok = stick_count >= wallStick && stone_count >= wallStone;
            craftWallBTN.gameObject.SetActive(ok);
        }

        // ***** CAMPFIRE (Stick + Stone) ***** //
        if (CampfireReq1 != null) CampfireReq1.text = campfireStick + " stick [" + stick_count + "]";
        if (CampfireReq2 != null) CampfireReq2.text = campfireStone + " stone [" + stone_count + "]";
        if (craftCampfireBTN != null)
            craftCampfireBTN.gameObject.SetActive(stick_count >= campfireStick && stone_count >= campfireStone);
    }
}
