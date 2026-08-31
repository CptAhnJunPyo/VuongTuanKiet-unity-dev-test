using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_WON,
        GAME_OVER,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings; 

    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private AutoPlayController m_autoPlay;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel()
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        State = eStateGame.GAME_STARTED;
    }
    public void LoadLevelTimeAtkMode(BoardController.eMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, mode);
        State = eStateGame.GAME_STARTED;
    }
    public void LoadLevelAuto(AutoPlayController.eMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        State = eStateGame.GAME_STARTED;

        m_autoPlay = this.gameObject.AddComponent<AutoPlayController>();
        m_autoPlay.StartAutoPlay(m_boardController, mode);
    }

    public void GameOver(bool won)
    {
        StartCoroutine(WaitBoardController(won));
    }

    internal void ClearLevel()
    {
        if (m_autoPlay != null)
        {
            m_autoPlay.Stop();
            Destroy(m_autoPlay);
            m_autoPlay = null;
        }

        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController(bool won)
    {
        if (m_autoPlay != null)
        {
            m_autoPlay.Stop();
        }

        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = won ? eStateGame.GAME_WON : eStateGame.GAME_OVER;
    }
}
