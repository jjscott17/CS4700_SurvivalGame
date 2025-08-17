using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; set; }

    [Header("State")]
    public GameObject itemToBeConstructed;
    public bool inConstructionMode = false;
    public GameObject constructionHoldingSpot;

    [Header("Validation")]
    public bool isValidPlacement;

    [Header("Ghost Selection")]
    public bool selectingAGhost;
    public GameObject selectedGhost;

    [Header("Ghost Materials")]
    public Material ghostSelectedMat;
    public Material ghostSemiTransparentMat;
    public Material ghostFullTransparentMat;

    [Header("Placement")]
    public float rotateStepDegrees = 15f;           // Q/E rotation step
    public string placedTag = "placedStructure";    // generic tag for placed objects

    // Manager tracks all ghosts spawned from placed items
    public List<GameObject> allGhostsInExistence = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Starts placement by loading a world prefab from Resources by name (e.g., "FoundationMode1").
    /// </summary>
    public void ActivateConstructionPlacement(string itemToConstruct)
    {
        var prefab = Resources.Load<GameObject>(itemToConstruct);
        if (prefab == null)
        {
            Debug.LogError($"ConstructionManager: Could not find Resources/{itemToConstruct}.prefab");
            return;
        }

        GameObject item = Instantiate(prefab);

        // Change name so it won't be "(Clone)"
        item.name = itemToConstruct;

        // Hold under a staging parent (UI/holder) so Constructable can do its ghost logic
        if (constructionHoldingSpot != null)
            item.transform.SetParent(constructionHoldingSpot.transform, false);

        itemToBeConstructed = item;
        itemToBeConstructed.tag = "activeConstructable";

        // Disable solid collider so mouse rays don’t immediately hit the object itself
        var c = itemToBeConstructed.GetComponent<Constructable>();
        if (c != null && c.solidCollider != null)
            c.solidCollider.enabled = false;

        inConstructionMode = true;
        selectingAGhost = false;
        selectedGhost = null;
    }

    void Update()
    {
        if (itemToBeConstructed != null && inConstructionMode)
        {
            // 1) Validate + color
            if (CheckValidConstructionPosition())
            {
                isValidPlacement = true;
                itemToBeConstructed.GetComponent<Constructable>().SetValidColor();
            }
            else
            {
                isValidPlacement = false;
                itemToBeConstructed.GetComponent<Constructable>().SetInvalidColor();
            }

            // 2) Ghost hover detection (ray from cursor)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var selectionTransform = hit.transform;
                if (selectionTransform.CompareTag("ghost"))
                {
                    // Hide the free-style preview if we are selecting a ghost
                    itemToBeConstructed.SetActive(false);
                    selectingAGhost = true;
                    selectedGhost = selectionTransform.gameObject;
                }
                else
                {
                    itemToBeConstructed.SetActive(true);
                    selectingAGhost = false;
                    selectedGhost = null;
                }
            }

            // 3) Rotation controls
            if (Input.GetKeyDown(KeyCode.Q)) RotatePreview(-rotateStepDegrees);
            if (Input.GetKeyDown(KeyCode.E)) RotatePreview(+rotateStepDegrees);

            // 4) Place
            if (Input.GetMouseButtonDown(0)) // LMB
            {
                if (isValidPlacement && !selectingAGhost)
                {
                    PlaceItemFreeStyle();
                }
                else if (selectingAGhost && selectedGhost != null)
                {
                    PlaceItemInGhostPosition(selectedGhost);
                }
            }

            // 5) Cancel (RMB or Esc)
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }

    private void RotatePreview(float deltaY)
    {
        if (itemToBeConstructed == null) return;
        var t = itemToBeConstructed.transform;
        t.rotation = Quaternion.Euler(0f, t.eulerAngles.y + deltaY, 0f);
    }

    private void CancelPlacement()
    {
        if (itemToBeConstructed != null)
            Destroy(itemToBeConstructed);

        itemToBeConstructed = null;
        inConstructionMode = false;
        selectingAGhost = false;
        selectedGhost = null;
    }

    private void PlaceItemInGhostPosition(GameObject copyOfGhost)
    {
        Vector3 ghostPosition = copyOfGhost.transform.position;
        Quaternion ghostRotation = copyOfGhost.transform.rotation;

        // Reveal preview again (we hid it when hovering a ghost)
        itemToBeConstructed.SetActive(true);

        // Move under scene root (two parents up in your original code)
        itemToBeConstructed.transform.SetParent(transform.parent != null && transform.parent.parent != null
            ? transform.parent.parent
            : null, true);

        itemToBeConstructed.transform.SetPositionAndRotation(ghostPosition, ghostRotation);

        var c = itemToBeConstructed.GetComponent<Constructable>();
        if (c != null)
        {
            c.ExtractGhostMembers();        // make ghosts independent
            c.SetDefaultColor();
            if (c.solidCollider != null) c.solidCollider.enabled = true;
        }

        itemToBeConstructed.tag = placedTag;

        // Track ghosts globally
        GetAllGhosts(itemToBeConstructed);
        PerformGhostDeletionScan();

        itemToBeConstructed = null;
        selectedGhost = null;
        selectingAGhost = false;
        inConstructionMode = false;
    }

    private void PlaceItemFreeStyle()
    {
        // Move under scene root (two parents up in your original code)
        itemToBeConstructed.transform.SetParent(transform.parent != null && transform.parent.parent != null
            ? transform.parent.parent
            : null, true);

        var c = itemToBeConstructed.GetComponent<Constructable>();
        if (c != null)
        {
            c.ExtractGhostMembers();
            c.SetDefaultColor();
            if (c.solidCollider != null) c.solidCollider.enabled = true;
            c.enabled = false; // stop following preview logic if any
        }

        itemToBeConstructed.tag = placedTag;

        GetAllGhosts(itemToBeConstructed);
        PerformGhostDeletionScan();

        itemToBeConstructed = null;
        inConstructionMode = false;
        selectingAGhost = false;
        selectedGhost = null;
    }

    private bool CheckValidConstructionPosition()
    {
        if (itemToBeConstructed == null) return false;
        var c = itemToBeConstructed.GetComponent<Constructable>();
        return c != null && c.isValidToBeBuilt;
    }

    private void GetAllGhosts(GameObject itemRoot)
    {
        var c = itemRoot.GetComponent<Constructable>();
        if (c == null || c.ghostList == null) return;

        foreach (GameObject ghost in c.ghostList)
        {
            if (ghost != null)
            {
                allGhostsInExistence.Add(ghost);
            }
        }
    }

    private void PerformGhostDeletionScan()
    {
        // Flag duplicates by position (2 decimals)
        foreach (GameObject ghost in allGhostsInExistence)
        {
            if (ghost == null) continue;
            var gi = ghost.GetComponent<GhostItem>();
            if (gi == null || gi.hasSamePosition) continue;

            foreach (GameObject ghostX in allGhostsInExistence)
            {
                if (ghostX == null || ghostX == ghost) continue;

                if (XPositionToAccurateFloat(ghost) == XPositionToAccurateFloat(ghostX) &&
                    ZPositionToAccurateFloat(ghost) == ZPositionToAccurateFloat(ghostX))
                {
                    var giX = ghostX.GetComponent<GhostItem>();
                    if (giX != null)
                    {
                        giX.hasSamePosition = true;
                        break;
                    }
                }
            }
        }

        // Remove flagged
        for (int i = 0; i < allGhostsInExistence.Count; i++)
        {
            var g = allGhostsInExistence[i];
            if (g == null) continue;
            var gi = g.GetComponent<GhostItem>();
            if (gi != null && gi.hasSamePosition)
            {
                DestroyImmediate(g);
                allGhostsInExistence[i] = null;
            }
        }
    }

    private float XPositionToAccurateFloat(GameObject ghost)
    {
        if (ghost == null) return 0f;
        float x = ghost.transform.position.x;
        return Mathf.Round(x * 100f) / 100f;
    }

    private float ZPositionToAccurateFloat(GameObject ghost)
    {
        if (ghost == null) return 0f;
        float z = ghost.transform.position.z;
        return Mathf.Round(z * 100f) / 100f;
    }
}
