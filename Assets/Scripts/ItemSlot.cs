using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



public class ItemSlot : MonoBehaviour, IDropHandler
{

    public GameObject Item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }

            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");

        //if there is not item already then set our item.
        
        if (!Item)
        {

            DragDrop.itemBeingDragged.transform.SetParent(transform);
            DragDrop.itemBeingDragged.transform.localPosition = new Vector2(0, 0);

        }
        

        /*
        // nothing being dragged? stop
        if (DragDrop.itemBeingDragged == null)
            return;

        // dragged item got destroyed mid-drag? stop
        if (DragDrop.itemBeingDragged.transform == null)
        {
            DragDrop.itemBeingDragged = null;
            return;
        }

        // only drop if slot empty
        if (Item == null)
        {
            DragDrop.itemBeingDragged.transform.SetParent(transform);
            DragDrop.itemBeingDragged.transform.localPosition = Vector2.zero;
        }
        */

    }
}
