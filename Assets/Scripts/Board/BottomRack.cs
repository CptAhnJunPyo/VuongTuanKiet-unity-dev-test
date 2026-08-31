using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BottomRack
{
    public const int RACK_CAPACITY = 5;

    public event Action OnRackFull = delegate { };
    public event Action OnMatchCleared = delegate { };

    private RackSlot[] m_slots;

    private Transform m_root;

    public BottomRack(Transform root, int boardSizeY)
    {
        m_root = root;

        m_slots = new RackSlot[RACK_CAPACITY];

        CreateSlots(boardSizeY);
    }

    private void CreateSlots(int boardSizeY)
    {
        // Position rack one row below the board's bottom edge.
        float yPos = -boardSizeY * 0.5f - 1f;
        float originX = -(RACK_CAPACITY * 0.5f) + 0.5f;
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < RACK_CAPACITY; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);

            go.transform.position = new Vector3(originX + i, yPos, 0f);
            go.transform.SetParent(m_root);

            RackSlot slot = go.AddComponent<RackSlot>();
            m_slots[i] = slot;
        }
    }

    public bool IsFull
    {
        get { return m_slots.All(slot => !slot.IsEmpty); }
    }

    public bool IsEmpty
    {
        get { return m_slots.All(slot => slot.IsEmpty); }
    }

    public void AddItem(Item item)
    {
        AddItem(item, null);
    }

    public void AddItem(Item item, Cell originCell)
    {
        RackSlot emptySlot = FindEmptySlot();
        if (emptySlot == null)
        {
            // Rack is full - trigger lose condition
            OnRackFull?.Invoke();
            return;
        }

        emptySlot.AssignItem(item, originCell);

        // Animate item to slot position (transition from board to rack).
        item.View.DOMove(emptySlot.transform.position, 0.3f).OnComplete(() =>
        {
            CheckForMatches();
        });
    }

    // Remove an item from the rack without matching (used for reversible moves).
    // Returns the item so the caller can send it back to its origin cell.
    public Item RemoveItem(RackSlot slot)
    {
        if (slot == null || slot.IsEmpty) return null;

        Item item = slot.Item;
        slot.Clear();
        return item;
    }

    private RackSlot FindEmptySlot()
    {
        return m_slots.FirstOrDefault(slot => slot.IsEmpty);
    }

    // Find the slot belonging to a tapped GameObject (used for reversible moves).
    public RackSlot GetSlotByGameObject(GameObject go)
    {
        if (go == null) return null;
        return m_slots.FirstOrDefault(slot => slot != null && slot.gameObject == go);
    }

    private void CheckForMatches()
    {
        // Count each item type in rack
        Dictionary<NormalItem.eNormalType, List<RackSlot>> typeGroups =
            new Dictionary<NormalItem.eNormalType, List<RackSlot>>();

        foreach (RackSlot slot in m_slots)
        {
            if (slot.IsEmpty) continue;

            NormalItem nItem = slot.Item as NormalItem;
            if (nItem != null)
            {
                if (!typeGroups.ContainsKey(nItem.ItemType))
                    typeGroups[nItem.ItemType] = new List<RackSlot>();

                typeGroups[nItem.ItemType].Add(slot);
            }
        }

        // Check if any type has exactly 3 or more
        foreach (var group in typeGroups)
        {
            if (group.Value.Count >= 3)
            {
                // Clear 3 items of this type
                ClearMatches(group.Value.Take(3).ToList());
                return; // Only clear one triplet at a time
            }
        }
    }

    private void ClearMatches(List<RackSlot> slotsToClean)
    {
        foreach (var slot in slotsToClean)
        {
            slot.ExplodeItem();
        }

        OnMatchCleared?.Invoke();
    }

    public Dictionary<NormalItem.eNormalType, int> GetItemCounts()
    {
        Dictionary<NormalItem.eNormalType, int> counts =
            new Dictionary<NormalItem.eNormalType, int>();

        foreach (var slot in m_slots)
        {
            if (slot.IsEmpty) continue;

            NormalItem nItem = slot.Item as NormalItem;
            if (nItem != null)
            {
                if (!counts.ContainsKey(nItem.ItemType))
                    counts[nItem.ItemType] = 0;

                counts[nItem.ItemType]++;
            }
        }

        return counts;
    }

    public HashSet<NormalItem.eNormalType> GetUniqueTypes()
    {
        HashSet<NormalItem.eNormalType> types =
            new HashSet<NormalItem.eNormalType>();

        foreach (var slot in m_slots)
        {
            if (slot.IsEmpty) continue;

            NormalItem nItem = slot.Item as NormalItem;
            if (nItem != null)
            {
                types.Add(nItem.ItemType);
            }
        }

        return types;
    }

    public void Clear()
    {
        foreach (var slot in m_slots)
        {
            if (!slot.IsEmpty)
            {
                slot.Item.Clear();
                slot.Clear();
            }

            GameObject.Destroy(slot.gameObject);
        }
    }
}

