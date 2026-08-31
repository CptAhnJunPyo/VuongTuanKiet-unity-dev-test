using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    private int boardSizeX;

    private int boardSizeY;

    private Cell[,] m_cells;

    private Transform m_root;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];

        CreateBoard();
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }
    }

    internal void Fill()
    {
        int totalCells = boardSizeX * boardSizeY;

        // Ensure divisible by 3
        if (totalCells % 3 != 0)
        {
            Debug.LogError($"Board size must create a cell count divisible by 3! Current: {boardSizeX}x{boardSizeY} = {totalCells}");
            return;
        }

        // Generate item pool with counts divisible by 3
        List<NormalItem.eNormalType> itemPool = GenerateItemPool(totalCells);

        // Shuffle pool
        itemPool = itemPool.OrderBy(x => UnityEngine.Random.value).ToList();

        // Assign to cells
        int index = 0;
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();

                item.SetType(itemPool[index++]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);
            }
        }
    }

    private List<NormalItem.eNormalType> GenerateItemPool(int totalCells)
    {
        List<NormalItem.eNormalType> pool = new List<NormalItem.eNormalType>();

        Array allTypes = Enum.GetValues(typeof(NormalItem.eNormalType));

        // Use ALL available fish types so every board contains the full set.
        int numTypes = Mathf.Max(1, allTypes.Length);

        // Work in triplets so every type count is a multiple of 3 and the
        // pool size always equals totalCells exactly.
        int totalTriplets = totalCells / 3;

        if (totalTriplets < numTypes)
        {
            Debug.LogWarning($"Board has only {totalTriplets} triplet(s) but {numTypes} fish types exist. " +
                $"Not every type will appear. Increase board size to at least {numTypes * 3} cells.");
        }

        // Distribute triplets across types round-robin. With totalTriplets >= numTypes
        // this guarantees each type appears at least once (in a multiple of 3).
        for (int t = 0; t < totalTriplets; t++)
        {
            NormalItem.eNormalType type = (NormalItem.eNormalType)allTypes.GetValue(t % numTypes);

            pool.Add(type);
            pool.Add(type);
            pool.Add(type);
        }

        return pool;
    }

    public bool IsEmpty()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Helper methods for automation
    public Cell FindCellWithType(NormalItem.eNormalType type)
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (cell.IsEmpty) continue;

                NormalItem nItem = cell.Item as NormalItem;
                if (nItem != null && nItem.ItemType == type)
                {
                    return cell;
                }
            }
        }

        return null;
    }

    public Dictionary<NormalItem.eNormalType, List<Cell>> GetItemsByType()
    {
        Dictionary<NormalItem.eNormalType, List<Cell>> result =
            new Dictionary<NormalItem.eNormalType, List<Cell>>();

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (cell.IsEmpty) continue;

                NormalItem nItem = cell.Item as NormalItem;
                if (nItem != null)
                {
                    if (!result.ContainsKey(nItem.ItemType))
                        result[nItem.ItemType] = new List<Cell>();

                    result[nItem.ItemType].Add(cell);
                }
            }
        }

        return result;
    }

    public Cell GetFirstNonEmptyCell()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty)
                {
                    return m_cells[x, y];
                }
            }
        }
        return null;
    }
    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }
}
