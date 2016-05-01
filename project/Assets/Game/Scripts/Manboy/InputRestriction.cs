using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[RequireComponent(typeof(InputField))]
public class InputRestriction : MonoBehaviour {

    public enum LetterCase
    {
        Standard,
        Lowercase,
        Uppercase
    }

    private InputField inputField;

    public LetterCase letterCase;
    //public bool restrictSpace;
    public bool restrictNumeric;
    public bool restrictSpecial;
    public bool restrictLetters;

    //private Regex spaceRegex = new Regex(@"\s+");
    private Regex numericRegex = new Regex("^[0-9]");
    private Regex specialRegex = new Regex ("[^a-zA-Z0-9_]+");
    private Regex lettersRegex = new Regex(@"(?i)^[a-z]+");

    void Awake()
    {
        inputField = GetComponent<InputField>();
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    void OnDestroy()
    {
        inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(string s)
    {
        /*if(restrictSpace)
        {
            s = spaceRegex.Replace(s, string.Empty);
        }*/

        if(restrictNumeric)
        {
            s = numericRegex.Replace(s, "");
        }

        if(restrictSpecial)
        {
            s = specialRegex.Replace(s, "");
        }

        if(restrictLetters)
        {
            s = lettersRegex.Replace(s, "");
        }

        switch(letterCase)
        {
            case LetterCase.Uppercase:
                s = s.ToUpper();
                break;

            case LetterCase.Lowercase:
                s = s.ToLower();
                break;
        }

        //inputField.text = s;
    }

}
