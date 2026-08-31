using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public enum eMode
    {
        NORMAL,
        TIME_ATTACK
    }
    public bool IsBusy { get; private set; }
    
    private Board m_board;

    private BottomRack m_bottomRack;

    private GameManager m_gameManager;

    private Camera m_cam;

    private GameSettings m_gameSettings;

    private bool m_gameOver;

    private bool m_isAutoplaying;

    private eMode m_mode;

    private float m_timeRemaining;
    private bool m_timerRunning;
    [SerializeField] private Text m_timerText;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        StartGame(gameManager, gameSettings, eMode.NORMAL);
    }

    public void StartGame(GameManager gameManager, GameSettings gameSettings, eMode mode)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_mode = mode;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        m_bottomRack = new BottomRack(this.transform, gameSettings.BoardSizeY);
        m_bottomRack.OnRackFull += OnRackFull;
        m_bottomRack.OnMatchCleared += OnMatchCleared;

        m_board.Fill();

        if (m_mode == eMode.TIME_ATTACK)
        {
            m_timeRemaining = gameSettings.TimeAttackSeconds;
            m_timerRunning = true;
            GameObject go = GameObject.Find("TextTimer");
            if (go != null)
            {
                m_timerText = go.GetComponent<Text>();
                UpdateTimerText();
            }
        }

        IsBusy = false;
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                if (m_mode == eMode.TIME_ATTACK && !m_gameOver)
                {
                    m_timerRunning = true;
                }
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                if (m_mode == eMode.TIME_ATTACK)
                {
                    m_timerRunning = false;
                }
                break;
            case GameManager.eStateGame.GAME_OVER:
            case GameManager.eStateGame.GAME_WON:
                m_gameOver = true;
                m_timerRunning = false;
                break;
        }
    }

    public void Update()
    {
        // Time attack countdown (runs unless paused or game over)
        if (m_mode == eMode.TIME_ATTACK && m_timerRunning && !m_gameOver)
        {
            m_timeRemaining -= Time.deltaTime;
            if (m_timeRemaining < 0f) m_timeRemaining = 0f;
            UpdateTimerText();

            if (m_timeRemaining <= 0f)
            {
                m_timerRunning = false;
                // time's up -> lose unless board already cleared
                bool won = (m_board != null && m_board.IsEmpty());
                m_gameManager.GameOver(won);
                return;
            }
        }

        if (m_gameOver) return;
        if (IsBusy) return;
        if (m_isAutoplaying) return; // Don't process input during automation

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty)
                {
                    CollectCellItem(cell);
                    return;
                }

                // Time Attack: allow withdrawing from bottom rack slots
                RackSlot slot = hit.collider.GetComponent<RackSlot>();
                if (slot != null && !slot.IsEmpty && m_mode == eMode.TIME_ATTACK)
                {
                    Cell origin = slot.OriginCell;
                    Item item = m_bottomRack.RemoveItem(slot);
                    if (item != null)
                    {
                        if (origin != null && origin.IsEmpty)
                        {
                            IsBusy = true;
                            origin.Assign(item);
                            item.SetViewRoot(this.transform);
                            // animate item moving into the cell

                            item.AnimationMoveToPosition();
                            StartCoroutine(CheckWinConditionAfterDelay());
                        }
                        else
                        {
                            // Cannot return to origin (null or occupied) - put back to rack
                            m_bottomRack.AddItem(item, null);
                        }
                    }
                }
            }
        }
    }

    public void CollectCellItem(Cell cell)
    {
        if (cell == null || cell.IsEmpty) return;

        if (m_bottomRack.IsFull && m_mode != eMode.TIME_ATTACK)
        {
            // Rack is full & not Time Mode, trigger lose
            m_gameManager.GameOver(false);
            return;
        }
        IsBusy = true;

        // Remove item from cell
        Item item = cell.Item;
        cell.Free();

        // Add to bottom rack
        switch (m_mode)
        {
            case BoardController.eMode.NORMAL:
                m_bottomRack.AddItem(item);
                break;
            case BoardController.eMode.TIME_ATTACK:
                m_bottomRack.AddItem(item, cell);
                break;
        }

        // AddItem animates, so wait a bit before checking win
        StartCoroutine(CheckWinConditionAfterDelay());
    }

    private IEnumerator CheckWinConditionAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (m_board.IsEmpty() && m_bottomRack.IsEmpty)
        {
            m_gameManager.GameOver(true); // Win
        }
        else
        {
            IsBusy = false;
        }
    }

    private void OnRackFull()
    {
        // Rack is full without matches - lose condition
        m_gameManager.GameOver(false);
    }
    private void OnTimeOut()
    {
        m_gameManager.GameOver(false);
    }
    private void OnMatchCleared()
    {
        // After match clears, check win condition
        StartCoroutine(CheckWinConditionAfterDelay());
    }

    private void UpdateTimerText()
    {
        if (m_timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(m_timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        m_timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void SetAutoplaying(bool isAutoplaying)
    {
        m_isAutoplaying = isAutoplaying;
    }

    public Board GetBoard()
    {
        return m_board;
    }

    public BottomRack GetBottomRack()
    {
        return m_bottomRack;
    }

    internal void Clear()
    {
        if (m_board != null)
        {
            m_board.Clear();
        }

        if (m_bottomRack != null)
        {
            m_bottomRack.OnRackFull -= OnRackFull;
            m_bottomRack.OnMatchCleared -= OnMatchCleared;
            m_bottomRack.Clear();
        }
    }
}

