using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteManager : MonoBehaviour
{
    public bool isDebug;
    
    // объект рулетки
    public Roulette roulette;
    // Time
    private TimeRoulette time;
    // текст для продолжения
    public UnityEngine.UI.Text textRoulette;
    // реклама просмотрена
    private bool isAdsFinished;
    // блокировка для блока обновления
    private bool isBlock;
    // текст изначальный рулетки
    private string textAbout;

    private void Start()
    {
        time = Singleton<TimeRoulette>.Instance;
        isAdsFinished = false;
        isBlock = true;

        Roulette.WinEvent += GiveReward;
    }

    private void OnDestroy()
    {
        Roulette.WinEvent -= GiveReward;
    }

    private void GiveReward()
    {
        Debug.Log("Winner item is: " + roulette.GetWinnerItemName());
    }

    /// <summary>
    /// 
    /// </summary>
    public void AdFinished()
    {
        isAdsFinished = true;
    }

    public void Block(bool condition)
    {
        isBlock = condition;
    }

    private void Update()
    {
        if (!isBlock)
        {            
            if (textRoulette.text != null)
            {
                textRoulette.text = time.TimeLeft();
            }
        }        
    }

    /// <summary>
    /// Click play in roulette menu
    /// </summary>
    public void Click()
    {
        // блокировку счетчика отключать-
        isBlock = false;

        if (isAdsFinished || isDebug)
        {
            roulette.Play();
            isAdsFinished = false;
        }
        else
        {
            if (time.IsCanContinue)
            {
                // give a reward
                // ...
                // adsunity.ShowRewardedAd();
                
                time.Save();
            }
        }
    }    
}
