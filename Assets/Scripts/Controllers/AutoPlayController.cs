using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Drives the game automatically for testing purposes.
/// WIN mode: prioritizes completing triplets to clear the board.
/// LOSE mode: deliberately collects distinct types to fill the rack without matching.
/// Each automated action is spaced by a fixed delay so the process is watchable.
/// </summary>
public class AutoPlayController : MonoBehaviour
{
    public enum eMode
    {
        WIN,
        LOSE
    }

    private const float DELAY_BETWEEN_MOVES = 0.5f;

    private BoardController m_boardController;
    private Board m_board;
    private BottomRack m_rack;

    private eMode m_mode;
    private bool m_isRunning;

    public void StartAutoPlay(BoardController boardController, eMode mode)
    {
        m_boardController = boardController;
        m_board = boardController.GetBoard();
        m_rack = boardController.GetBottomRack();
        m_mode = mode;

        m_isRunning = true;

        // Suppress manual input while the automation is driving.
        m_boardController.SetAutoplaying(true);

        StartCoroutine(AutoPlayCoroutine());
    }

    public void Stop()
    {
        m_isRunning = false;
    }

    private IEnumerator AutoPlayCoroutine()
    {
        while (m_isRunning)
        {
            yield return new WaitForSeconds(DELAY_BETWEEN_MOVES);

            // Board cleared - nothing left to do (WIN handled by BoardController).
            if (m_board.IsEmpty())
            {
                break;
            }

            Cell target = (m_mode == eMode.WIN) ? ChooseWinningCell() : ChooseLosingCell();

            if (target == null)
            {
                // No valid move available.
                break;
            }

            m_boardController.CollectCellItem(target);
        }

        m_isRunning = false;
    }

    /// 1. If the rack holds a pair (2 of a type), grab the matching third from the board.
    /// 2. Else if the rack holds a single, grab another of that same type to build toward a triplet.
    /// 3. Else start a type that has at least 3 available on the board (safe to complete).
    /// 4. Fallback: any remaining item.
    
    private Cell ChooseWinningCell()
    {
        Dictionary<NormalItem.eNormalType, int> rackCounts = m_rack.GetItemCounts();
        Dictionary<NormalItem.eNormalType, List<Cell>> boardItems = m_board.GetItemsByType();

        // 1. Complete an existing pair in the rack.
        foreach (var kvp in rackCounts)
        {
            if (kvp.Value == 2 && boardItems.ContainsKey(kvp.Key))
            {
                return boardItems[kvp.Key][0];
            }
        }

        // 2. Extend an existing single in the rack.
        foreach (var kvp in rackCounts)
        {
            if (kvp.Value == 1 && boardItems.ContainsKey(kvp.Key))
            {
                return boardItems[kvp.Key][0];
            }
        }

        // 3. Start a new type that can be fully completed (3+ on board).
        //    Only do this if there is rack space to avoid stranding singles.
        int freeSlots = BottomRack.RACK_CAPACITY - RackOccupancy(rackCounts);
        if (freeSlots >= 1)
        {
            foreach (var kvp in boardItems)
            {
                if (kvp.Value.Count >= 3)
                {
                    return kvp.Value[0];
                }
            }
        }

        // 4. Fallback: collect anything still on the board.
        return m_board.GetFirstNonEmptyCell();
    }

    /// <summary>
    /// LOSE strategy: always collect a type NOT already in the rack, so the rack
    /// fills with distinct items and never triggers a match. Fills all 5 slots -> lose.
    /// </summary>
    private Cell ChooseLosingCell()
    {
        HashSet<NormalItem.eNormalType> typesInRack = m_rack.GetUniqueTypes();
        Dictionary<NormalItem.eNormalType, List<Cell>> boardItems = m_board.GetItemsByType();

        // Prefer a brand-new type the rack does not yet contain.
        foreach (var kvp in boardItems)
        {
            if (!typesInRack.Contains(kvp.Key))
            {
                return kvp.Value[0];
            }
        }

        // If every remaining board type is already represented in the rack,
        // collecting any of them still can't help the player - just fill up.
        return m_board.GetFirstNonEmptyCell();
    }

    private int RackOccupancy(Dictionary<NormalItem.eNormalType, int> rackCounts)
    {
        return rackCounts.Values.Sum();
    }
}
