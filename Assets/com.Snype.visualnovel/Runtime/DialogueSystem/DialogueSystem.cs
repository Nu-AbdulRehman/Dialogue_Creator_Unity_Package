using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header ("Text Fields")]
    [SerializeField] GameObject dialogueText;

    [Header("Visuals")]
    [SerializeField] Image characterVisuals;

    [Header("Options")]
    [SerializeField] List<Button> optionsButtons;
    [SerializeField] List<GameObject> optionsTexts;

    [Header("     Conversations")]
    [ConversationDropdown]
    [SerializeField] Conversation currentConversation;

    ConversationsDatabase conversationsDatabase;

    ConversationNode nextChosen = null;

    private void Start()
    {
        conversationsDatabase = AssetDatabase.LoadAssetAtPath<ConversationsDatabase>("Assets/ConversationsDatabase.asset");
        if(conversationsDatabase == null)
        {
            Debug.LogError("No Conversations Database Found! Please Set It Up Using The Conversations Handler Window Under Window/Visual Novel Creator. If A Database Exists Please Ensure It Is Situated Under The Assets Folder");
            return;
        }
        PlayConversation();
    }

    void PlayConversation()
    {
        if(currentConversation == null)
        {
            Debug.LogError("No Conversation Chosen Please Choose A Valid One From The Dropdown");
        }
        else
        {
            StartCoroutine(PlayConversation(() => Debug.Log("Conversation Done")));
        }
    }

    IEnumerator PlayConversation(System.Action onComplete)
    {
        ConversationNode currentNode = null;
        bool currentVerseDone = false;
        float delay = 0.015f;
        float wait = 1f;

        currentNode = currentConversation.nodesList[0];

        while (currentNode != null && currentNode.children.Count >= 0)
        {
            if(!currentNode.isCondition)
            {
                SetCharacterVisuals(currentNode);

                DisplayVerse(currentNode.verse, delay, () => currentVerseDone = true);

                yield return new WaitUntil(() => currentVerseDone == true);

                yield return new WaitForSeconds(wait);

                currentVerseDone = false;

                if (currentNode.children.Count > 0)
                {
                    currentNode = currentNode.children[0];
                }
                else
                {
                    currentNode = null;
                }
            }
            else
            {
                DisplayChoices(currentNode);

                AssignOptions(currentNode, currentNode.numOfConditions);

                yield return new WaitUntil(() => nextChosen != null);

                currentNode = nextChosen;

                nextChosen = null;
            }
        }

        onComplete?.Invoke();
    }

    void DisplayVerse(string verseInput, float delay, System.Action onComplete)
    {
        Text regText = dialogueText.GetComponent<Text>();
        TMP_Text tmpText = dialogueText.GetComponent<TMP_Text>();
        if (dialogueText != null)
        {
            if(regText)
            {
                StartCoroutine(DisplayVerse(regText, verseInput, delay, onComplete));
            }
            else if(tmpText)
            {
                StartCoroutine(DisplayVerse(tmpText, verseInput, delay, onComplete));
            }
        }
        else
        {
            Debug.LogError("No Dialogue Text Assigned, Please Assign It To The Dialogue System Under The Inspector");
        }
    }

    IEnumerator DisplayVerse(Text output, string verseInput, float delay, System.Action onComplete)
    {
        string outputString = "";
        foreach(char ch in verseInput)
        {
            outputString += ch;
            output.text = outputString;
            yield return new WaitForSeconds(delay);
        }
        onComplete?.Invoke();
    }


    IEnumerator DisplayVerse(TMP_Text output, string verseInput, float delay, System.Action onComplete)
    {
        string outputString = "";
        foreach (char ch in verseInput)
        {
            outputString += ch;
            output.text = outputString;
            yield return new WaitForSeconds(delay);
        }
        onComplete?.Invoke();
    }

    void SetCharacterVisuals(ConversationNode currentNode)
    { 
        if(characterVisuals != null)
        {
            characterVisuals.sprite = currentNode.dialogue.character.characterSprite;
        }
        else
        {
            Debug.LogError("No Character Visuals Assigned, Please Assign It To The Dialogue System Under The Inspector");
        }
    }

    void DisplayChoices(ConversationNode currentNode)
    {
        if(currentNode.numOfConditions > optionsTexts.Count)
        {
            Debug.LogError("Not Enough Options Texts Assigned, Please Assign Them To The Dialogue System Under The Inspector. \n Current: " + optionsTexts.Count + ", Required: " + currentNode.numOfConditions);
        }
        else
        {
            for(int i = 0; i < currentNode.optionsText.Count; i++)
            {
                Debug.Log(currentNode.optionsText[i]);
                if (optionsTexts[i].GetComponent<Text>() != null)
                {
                    optionsTexts[i].GetComponent<Text>().text = currentNode.optionsText[i];
                }
                else if(optionsTexts[i].GetComponent<TMP_Text>() != null)
                {
                    optionsTexts[i].GetComponent<TMP_Text>().text = currentNode.optionsText[i];
                }
                else
                {
                    Debug.LogError("No Valid Text Component Added To The Options Text GameObject Number " + i + 1 + ", Only TMPro Text Or Legacy Text Components Are Valid!");
                }
            }
        }
    }

    void AssignOptions(ConversationNode currentNode, int numberOfOptions)
    {   
        if(numberOfOptions > optionsButtons.Count)
        {
            Debug.LogError("Not Enough Options Buttons Assigned, Please Assign Them To The Dialogue System Under The Inspector. \n Current: " + optionsButtons.Count + ", Required: " + numberOfOptions);
        }
        else
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                optionsButtons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            }

            for (int i = 0; i < numberOfOptions; i++)
            {
                int optionIndex = i;
                optionsButtons[i].GetComponent<Button>().onClick.AddListener(() => IthChoiceChosen(currentNode, optionIndex));
            }
        }
    }

    void IthChoiceChosen(ConversationNode currentNode, int i)
    {
        if(currentNode.children.Count < i + 1)
        {
            Debug.LogError("Dialogue Node After Condition Not Assigned, Please Assign It In The Conversation Creator Window");
        }
        else
        {
            nextChosen = currentNode.children[i];
        }
    }
}
