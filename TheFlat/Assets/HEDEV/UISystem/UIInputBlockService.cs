using UnityEngine;

public static class InputBlockService
{
    public delegate void InputDelegate();

    private static event InputDelegate EnableEvent;
    private static event InputDelegate DisableEvent;

    private static bool m_isBlock = false;
    public static bool IsBlock => m_isBlock;
    

    public static void RegisterEnableEvent(InputDelegate enableDelegate)
    {
        EnableEvent += enableDelegate;
    }
    
    public static void RemoveEnableEvent(InputDelegate enableDelegate)
    {
        EnableEvent -= enableDelegate;
    }
    
    public static void RegisterDisableEvent(InputDelegate disableDelegate)
    {
        DisableEvent += disableDelegate;
    }
   
    public static void RemoveDisableEvent(InputDelegate disableDelegate)
    {
        DisableEvent -= disableDelegate;
    }
    
    public static void Release()
    {
        if (!m_isBlock)
        {
            Debug.Log("Input already unblocked!");
            return;
        }
        m_isBlock = false;
        EnableEvent?.Invoke();
        Debug.Log("Input unblocked!");
    }
    
    public static void Block()
    {
        if (m_isBlock)
        {
            Debug.Log("Input already blocked!");
            return;
        }
        
        DisableEvent?.Invoke();
        m_isBlock = true;
        Debug.Log("Input blocked!");
    }
}