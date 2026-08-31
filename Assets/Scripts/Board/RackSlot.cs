using UnityEngine;

public class RackSlot : MonoBehaviour
{
    public Item Item { get; private set; }

    public Cell OriginCell { get; private set; }

    public bool IsEmpty => Item == null;

    public void AssignItem(Item item)
    {
        AssignItem(item, null);
    }

    public void AssignItem(Item item, Cell originCell)
    {
        Item = item;
        OriginCell = originCell;
        if (Item != null)
        {
            Item.SetViewRoot(this.transform);
        }
    }
    public void Clear()
    {
        if (Item != null)
        {
            Item = null;
        }
        OriginCell = null;
    }

    public void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
        OriginCell = null;
    }
}
