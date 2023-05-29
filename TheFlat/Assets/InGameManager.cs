using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        OpenHuds();
    }
    
    private void OpenHuds()
    {
        UIWindowService.OpenWindow<UI_HUD_MissionList_Controller>(ui =>
        {
            // ui.Window
        });
    }
}