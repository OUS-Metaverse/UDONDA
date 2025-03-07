
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Keyboard : UdonSharpBehaviour
{
    [SerializeField] private GameObject keyboard1;
    [SerializeField] private GameObject keyboard2;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int _waitFrame = 10;
    
    public bool isDesktopMode = false;
    private bool shiftMode = false;
    private bool ShiftMode {
        get => shiftMode;
        set {
            shiftMode = value;
            keyboard1.SetActive(shiftMode);
            keyboard2.SetActive(!shiftMode);
        }
    }
    private bool shiftLock = false;
    private bool standbyDoubleTap = false;

    void Update()
    {
        if (isDesktopMode)
        {
            string input = Input.inputString;
            foreach (char c in input)
            {
                gameManager.OnInputKey(char.ToLower(c));
            }
        }
        else
        {
            if (!standbyDoubleTap) return;
            if (_waitFrame > 0)
            {
                _waitFrame--;
            }
            else
            {
                standbyDoubleTap = false;
                _waitFrame = 10;
            }
        }
    }

    public void OnInteractKey(string s)
    {
        if (s == "↑" || s == "↓")
        {
            ToggleKeyboard();
        }
        else if (s == "BackSpace")
        {
            gameManager.OnInputKey('\b');
        }
        else if (s == "␣")
        {
            gameManager.OnInputKey(' ');
        }
        else
        {
            gameManager.OnInputKey(s[0]);
        }
        if (!shiftLock) ShiftMode = false;
    }

    public void ToggleKeyboard()
    {
        if (standbyDoubleTap)
        {
            if (!keyboard2.activeSelf) return;
            standbyDoubleTap = false;
            ShiftMode = true;
            shiftLock = true;
            return;
        }
        ShiftMode = !ShiftMode;
        standbyDoubleTap = true;
        shiftLock = false;
    }

}