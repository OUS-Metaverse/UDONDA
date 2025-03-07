
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Keyboard : UdonSharpBehaviour
{
    [SerializeField] private GameObject keyboard1;
    [SerializeField] private GameObject keyboard2;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int _waitMs = 700;
    private long _standbyTime = 0;
    
    public bool isDesktopMode = false;
    private bool shiftMode = false;
    private bool ShiftMode {
        get => shiftMode;
        set {
            shiftMode = value;
            keyboard1.SetActive(!shiftMode);
            keyboard2.SetActive(shiftMode);
        }
    }
    private bool shiftLock = false;
    private bool _standbyDoubleTap = false;
    private bool StandbyDoubleTap {
        get => _standbyDoubleTap;
        set {
            _standbyDoubleTap = value;
            _standbyTime = _standbyDoubleTap ? System.DateTime.Now.Millisecond : 0;
        }
    }

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
            if (!StandbyDoubleTap) return;
            if (System.DateTime.Now.Millisecond - _standbyTime > _waitMs)
            {
                StandbyDoubleTap = false;
            }
        }
    }

    public void OnInteractKey(string s)
    {
        if (s == "↑" || s == "↓")
        {
            ToggleKeyboard();
            return;
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
        if (StandbyDoubleTap)
        {
            StandbyDoubleTap = false;
            ShiftMode = true;
            shiftLock = true;
            return;
        }
        ShiftMode = !ShiftMode;
        if (ShiftMode)
        {
            StandbyDoubleTap = true;
        }
        shiftLock = false;
    }

}