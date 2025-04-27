using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardButton : MonoBehaviour
{
    public char _letter;

    public void SetLetter(char letter){
        _letter = letter;
    }

    public char GetLetter(){
        return _letter;
    }
}
