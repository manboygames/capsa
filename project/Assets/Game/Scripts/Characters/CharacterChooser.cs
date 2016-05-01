using AdvancedInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterChooser : MonoBehaviour
{
    public Character currentCharacter;
    public InputField nameInputField;

    private Character[] characters;
    private int selectionIndex;

    void Awake()
    {
        nameInputField.onEndEdit.AddListener(OnEndEdit);
    }

    IEnumerator Start()
    {
        while(CharacterManager.Instance == null)
        {
            yield return null;
        }

        characters = CharacterManager.Instance.characters.ToArray();
        RandomizeCharacter();
    }

    void Update()
    {
        if(nameInputField.isFocused)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousCharacter();
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCharacter();
        }
    }

    private void SetActiveCharacter(int index)
    {
        currentCharacter.SetCharacter(characters[index]);
    }

    public void RandomizeCharacter()
    {
        int random = Random.Range(0, characters.Length);

        if(random == selectionIndex)
        {
            RandomizeCharacter();
            return;
        }

        selectionIndex = random;

        SetActiveCharacter(selectionIndex);
    }

    public void NextCharacter()
    {
        if (selectionIndex == characters.Length - 1)
        {
            selectionIndex = 0;
        }
        else
        {
            selectionIndex++;
        }

        SetActiveCharacter(selectionIndex);
    }

    public void PreviousCharacter()
    {
        if (selectionIndex == 0)
        {
            selectionIndex = characters.Length - 1;
        }
        else
        {
            selectionIndex--;
        }

        SetActiveCharacter(selectionIndex);
    }

    public void LoadNextScene()
    {
        if(nameInputField.text.Length > 0)
        {
            CharacterManager.Instance.characterName = nameInputField.text;
            CharacterManager.Instance.characterID = currentCharacter.characterID;
            SceneDirector.Instance.LoadScene();
        }
    }

    private void OnEndEdit(string s)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(s.Length > 0)
            {
                LoadNextScene();
            }
        }
    }

    void OnDestroy()
    {
        nameInputField.onEndEdit.RemoveAllListeners();
    }

}
