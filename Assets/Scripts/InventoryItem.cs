using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // --- Is this item trashable --- //
    public bool isTrashable;

    // --- Item Info UI --- //
    private GameObject itemInfoUI;
    private Text itemInfoUI_itemName;
    private Text itemInfoUI_itemDescription;
    private Text itemInfoUI_itemFunctionality;

    [Header("Display")]
    public string thisName, thisDescription, thisFunctionality;

    // --- Consumption (optional, currently not used to avoid external deps) --- //
    private GameObject itemPendingConsumption;
    public bool isConsumable;
    public float healthEffect;
    public float caloriesEffect;
    public float hydrationEffect;

    // --- Equip / Quick Slot --- //
    public bool isEquippable;
    private GameObject itemPendingEquipping;
    public bool isInsideQuickSlot;

    // --- Use (e.g., building kit / blueprint) --- //
    public bool isSelected;
    public bool isUseable;
    public GameObject itemPendingToBeUsed;

    // === Lightweight Quick Slot (no EquipSystem needed) ===
    public static int MaxQuickSlots = 4;
    private static readonly List<InventoryItem> QuickSlotItems = new List<InventoryItem>();

    public static bool QuickSlotsFull() => QuickSlotItems.Count >= MaxQuickSlots;

    public static bool AddToQuickSlots(InventoryItem item)
    {
        if (item == null || QuickSlotsFull()) return false;
        if (QuickSlotItems.Contains(item)) return true;
        QuickSlotItems.Add(item);
        item.isInsideQuickSlot = true;
        // TODO: update quick-slot UI here if you have one
        return true;
    }

    public static bool RemoveFromQuickSlots(InventoryItem item)
    {
        if (item == null) return false;
        bool removed = QuickSlotItems.Remove(item);
        if (removed) item.isInsideQuickSlot = false;
        // TODO: update quick-slot UI here if you have one
        return removed;
    }

    private void OnDestroy()
    {
        // Ensure we don't keep stale references if this item is destroyed
        if (isInsideQuickSlot) RemoveFromQuickSlots(this);
    }

    private void Start()
    {
        // NOTE: If your InventorySystem property is named ItemInfoUI (capital UI),
        // change ItemInfoUi below to match it exactly.
        itemInfoUI = InventorySystem.Instance.ItemInfoUi;
        if (itemInfoUI != null)
        {
            itemInfoUI_itemName = itemInfoUI.transform.Find("itemName").GetComponent<Text>();
            itemInfoUI_itemDescription = itemInfoUI.transform.Find("itemDescription").GetComponent<Text>();
            itemInfoUI_itemFunctionality = itemInfoUI.transform.Find("itemFunctionality").GetComponent<Text>();
        }
    }

    // Triggered when the mouse enters into the area of the item that has this script.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemInfoUI == null) return;
        itemInfoUI.SetActive(true);
        if (itemInfoUI_itemName) itemInfoUI_itemName.text = thisName;
        if (itemInfoUI_itemDescription) itemInfoUI_itemDescription.text = thisDescription;
        if (itemInfoUI_itemFunctionality) itemInfoUI_itemFunctionality.text = thisFunctionality;
    }

    // Triggered when the mouse exits the area of the item that has this script.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemInfoUI == null) return;
        itemInfoUI.SetActive(false);
    }

    // Triggered when the mouse is clicked over the item that has this script.
    public void OnPointerDown(PointerEventData eventData)
    {
        // Right Mouse Button Click
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // (Optional) Consumable logic removed to avoid external deps; re-enable if you have PlayerState, etc.
            // if (isConsumable)
            // {
            //     itemPendingConsumption = gameObject;
            //     consumingFunction(healthEffect, caloriesEffect, hydrationEffect);
            // }

            // Equip into quick slots (no EquipSystem required)
            if (isEquippable && isInsideQuickSlot == false && QuickSlotsFull() == false)
            {
                AddToQuickSlots(this);
            }

            // Use (e.g., start construction placement)
            if (isUseable)
            {
                itemPendingToBeUsed = gameObject;
                UseItem();
            }
        }
    }

    private void UseItem()
    {
        if (itemInfoUI) itemInfoUI.SetActive(false);

        // Close inventory/crafting UI
        InventorySystem.Instance.isOpen = false;
        InventorySystem.Instance.inventoryScreenUI.SetActive(false);

        CraftingSystem.Instance.isOpen = false;
        CraftingSystem.Instance.craftingScreenUI.SetActive(false);
        CraftingSystem.Instance.toolsScreenUI.SetActive(false);
        CraftingSystem.Instance.constructingScreenUI.SetActive(false);

        // Return control to player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Requires you to have added EnableSelection() to SelectionManager (as we discussed)
        SelectionManager.Instance.EnableSelection();

        // Begin construction mode based on the item name (matches your existing pattern)
        switch (gameObject.name)
        {
            case "Foundation":
case "Foundation(Clone)":
    ConstructionManager.Instance.ActivateConstructionPlacement("FoundationMode1");
    break;
case "Wall":
case "Wall(Clone)":
    ConstructionManager.Instance.ActivateConstructionPlacement("WallMode1");
    break;
case "Campfire":
case "Campfire(Clone)":
    ConstructionManager.Instance.ActivateConstructionPlacement("CampfireMode1");
    break;

            default:
                break;
        }
    }

    // Triggered when the mouse button is released over the item that has this script.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // (Optional) If you re-enable consumables:
            // if (isConsumable && itemPendingConsumption == gameObject)
            // {
            //     Destroy(gameObject);
            //     InventorySystem.Instance.ReCalculateList();
            //     CraftingSystem.Instance.RefreshNeededItems();
            // }

            if (isUseable && itemPendingToBeUsed == gameObject)
            {
                // Remove this inventory entry after "use" (matches your original behavior)
                Destroy(gameObject); // use Destroy (runtime-safe) rather than DestroyImmediate
                InventorySystem.Instance.ReCalculateList();
                CraftingSystem.Instance.RefreshNeededItems();
            }
        }
    }
}
