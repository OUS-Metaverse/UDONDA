
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class Keyboard : UdonSharpBehaviour
{
    [SerializeField] private GameObject keyboard1;
    [SerializeField] private GameObject keyboard2;
    [SerializeField] private GameManager gameManager;
    
    public bool isDesktopMode = false;

    void Update()
    {
        if (!isDesktopMode) return;
        string input = Input.inputString;
        foreach (char c in input)
        {
            gameManager.OnInputKey(char.ToLower(c));
        }
    }

    public void ToggleKeyboard()
    {
        keyboard1.SetActive(!keyboard1.activeSelf);
        keyboard2.SetActive(!keyboard2.activeSelf);
    }

    public void SendBackquote()
    {
        gameManager.OnInputKey('`');
    }

    public void Send1()
    {
        gameManager.OnInputKey('1');
    }

    public void Send2()
    {
        gameManager.OnInputKey('2');
    }

    public void Send3()
    {
        gameManager.OnInputKey('3');
    }

    public void Send4()
    {
        gameManager.OnInputKey('4');
    }

    public void Send5()
    {
        gameManager.OnInputKey('5');
    }

    public void Send6()
    {
        gameManager.OnInputKey('6');
    }

    public void Send7()
    {
        gameManager.OnInputKey('7');
    }

    public void Send8()
    {
        gameManager.OnInputKey('8');
    }

    public void Send9()
    {
        gameManager.OnInputKey('9');
    }

    public void Send0()
    {
        gameManager.OnInputKey('0');
    }

    public void SendMinus()
    {
        gameManager.OnInputKey('-');
    }

    public void SendEquals()
    {
        gameManager.OnInputKey('=');
    }

    public void SendBackspace()
    {
        gameManager.OnInputKey('\b');
    }

    public void SendQ() {
        gameManager.OnInputKey('q');
    }

    public void SendW() {
        gameManager.OnInputKey('w');
    }

    public void SendE() {
        gameManager.OnInputKey('e');
    }

    public void SendR() {
        gameManager.OnInputKey('r');
    }

    public void SendT() {
        gameManager.OnInputKey('t');
    }

    public void SendY() {
        gameManager.OnInputKey('y');
    }

    public void SendU() {
        gameManager.OnInputKey('u');
    }

    public void SendI() {
        gameManager.OnInputKey('i');
    }

    public void SendO() {
        gameManager.OnInputKey('o');
    }

    public void SendP() {
        gameManager.OnInputKey('p');
    }

    public void SendLeftBracket() {
        gameManager.OnInputKey('[');
    }

    public void SendRightBracket() {
        gameManager.OnInputKey(']');
    }

    public void SendBackslash() {
        gameManager.OnInputKey('\\');
    }

    public void SendA() {
        gameManager.OnInputKey('a');
    }

    public void SendS() {
        gameManager.OnInputKey('s');
    }

    public void SendD() {
        gameManager.OnInputKey('d');
    }

    public void SendF() {
        gameManager.OnInputKey('f');
    }

    public void SendG() {
        gameManager.OnInputKey('g');
    }

    public void SendH() {
        gameManager.OnInputKey('h');
    }

    public void SendJ() {
        gameManager.OnInputKey('j');
    }

    public void SendK() {
        gameManager.OnInputKey('k');
    }

    public void SendL() {
        gameManager.OnInputKey('l');
    }

    public void SendSemicolon() {
        gameManager.OnInputKey(';');
    }

    public void SendQuote() {
        gameManager.OnInputKey('\'');
    }

    public void SendAtSign() {
        gameManager.OnInputKey('@');
    }

    public void SendZ() {
        gameManager.OnInputKey('z');
    }

    public void SendX() {
        gameManager.OnInputKey('x');
    }

    public void SendC() {
        gameManager.OnInputKey('c');
    }

    public void SendV() {
        gameManager.OnInputKey('v');
    }

    public void SendB() {
        gameManager.OnInputKey('b');
    }

    public void SendN() {
        gameManager.OnInputKey('n');
    }

    public void SendM() {
        gameManager.OnInputKey('m');
    }

    public void SendComma() {
        gameManager.OnInputKey(',');
    }

    public void SendPeriod() {
        gameManager.OnInputKey('.');
    }

    public void SendSlash() {
        gameManager.OnInputKey('/');
    }

    public void SendSpace() {
        gameManager.OnInputKey(' ');
    }

    public void SendTilde() {
        gameManager.OnInputKey('~');
    }

    public void SendExclamation() {
        gameManager.OnInputKey('!');
    }

    public void SendHash() {
        gameManager.OnInputKey('#');
    }

    public void SendDollar() {
        gameManager.OnInputKey('$');
    }

    public void SendPercent() {
        gameManager.OnInputKey('%');
    }

    public void SendCaret() {
        gameManager.OnInputKey('^');
    }

    public void SendAmpersand() {
        gameManager.OnInputKey('&');
    }

    public void SendAsterisk() {
        gameManager.OnInputKey('*');
    }

    public void SendLeftParenthesis() {
        gameManager.OnInputKey('(');
    }

    public void SendRightParenthesis() {
        gameManager.OnInputKey(')');
    }

    public void SendUnderscore() {
        gameManager.OnInputKey('_');
    }

    public void SendPlus() {
        gameManager.OnInputKey('+');
    }

    public void SendCurlyLeft() {
        gameManager.OnInputKey('{');
    }

    public void SendCurlyRight() {
        gameManager.OnInputKey('}');
    }

    public void SendPipe() {
        gameManager.OnInputKey('|');
    }

    public void SendColon() {
        gameManager.OnInputKey(':');
    }

    public void SendDoubleQuote() {
        gameManager.OnInputKey('\'');
    }

    public void SendLessThan() {
        gameManager.OnInputKey('<');
    }

    public void SendGreaterThan() {
        gameManager.OnInputKey('>');
    }

    public void SendQuestion() {
        gameManager.OnInputKey('?');
    }
}
