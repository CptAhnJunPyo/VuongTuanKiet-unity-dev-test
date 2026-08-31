using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPlay;

    [SerializeField] private Button btnAutoWin;

    [SerializeField] private Button btnAutoLose;

    [SerializeField] private Button btnTimeAtkMode;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnPlay.onClick.AddListener(OnClickPlay);
        if (btnTimeAtkMode) btnTimeAtkMode.onClick.AddListener(OnClickTimeAtk);
        if (btnAutoWin) btnAutoWin.onClick.AddListener(OnClickAutoWin);
        if (btnAutoLose) btnAutoLose.onClick.AddListener(OnClickAutoLose);
    }

    private void OnDestroy()
    {
        if (btnPlay) btnPlay.onClick.RemoveAllListeners();
        if (btnAutoWin) btnAutoWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
        if (btnTimeAtkMode) btnTimeAtkMode.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }
    public void OnClickTimeAtk()
    {
        m_mngr.LoadLevelTimeAtkMode();
    }
    private void OnClickPlay()
    {
        m_mngr.LoadLevel();
    }

    private void OnClickAutoWin()
    {
        m_mngr.LoadLevelAutoWin();
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAutoLose();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
