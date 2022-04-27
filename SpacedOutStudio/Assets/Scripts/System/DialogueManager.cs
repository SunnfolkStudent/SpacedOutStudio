using System.Collections;
using Inputs;
using TMPro;
using UnityEngine;

namespace System
{
    [RequireComponent(typeof(ChoiceManager))]
    [RequireComponent(typeof(SceneController))]
    [RequireComponent(typeof(InputManager))]
    public class DialogueManager : MonoBehaviour
    {
        public string dialogueChoicesChar = "/c";
        public string choiceOneChar = "/c1";
        public string choiceTwoChar = "/c2";
        public string choiceThreeChar = "/c3";
        public string returnChar = "/r";
        
        public string[] script;

        public string sceneToLoad;
        //public GameObject dialogueGameObject;
        public GameObject logGameObject;
        public TMP_Text dialogueText;
        public TMP_Text namePlate;
        public string replaceString;
        public string playerName;
        private InputManager _inputManager;
        private int _currentDialogue;
        private TMP_Text _logText;
        private string[] _names;
        private SceneController _sceneController;
        public float textSpeed = 1;
        public bool loop;
        private bool _generatingDialogue;
        private bool _stopGeneratingDialogue;
        private bool[] _choiceDialogues;
        private bool[] _choiceDialogues1;
        private bool[] _choiceDialogues2;
        private bool[] _choiceDialogues3;
        private bool[] _returnDialogues;
        private ChoiceManager _choiceManager;
        private bool _inChoices1;
        private bool _inChoices2;
        private bool _inChoices3;
        private int _currentOptions;
        public bool RlvlMattering;
        public int minRlvlForEnding;


        private void Start()
        {
            _choiceManager = GetComponent<ChoiceManager>();
            _sceneController = GetComponent<SceneController>();
            _logText = logGameObject.GetComponentInChildren<TMP_Text>();
            _inputManager = GetComponent<InputManager>();
            ExtractAndReplaceNames();
            dialogueText.text = "";
            namePlate.text = _names[_currentDialogue];
        }

        private void Update()
        {
            if (_inputManager.interact)
            {
                if (!_generatingDialogue)
                {
                    NextDialogue();
                }
                else
                {
                    _stopGeneratingDialogue = true;
                }
            }

            if (_inputManager.log)
            {
                Log();
            }
        }

        public void NextDialogue()
        {
            if (logGameObject.activeSelf || _choiceManager.showingDialogue) return;

            if (_currentDialogue < script.Length -1)
            {
                _currentDialogue++;
            }
            else
            {
                if (!loop)
                {
                    _sceneController.LoadWithTransition();
                    return;
                }
                _currentDialogue = 0;
            }
            
            if (_inChoices1)
            {
                if (!_choiceDialogues1[_currentDialogue])
                {
                    _inChoices1 = false;
                    GotoCorrectDialogue(_choiceManager.currentChoices,_returnDialogues);
                    return;
                }
            }
            else if (_inChoices2)
            {
                if (!_choiceDialogues1[_currentDialogue])
                {
                    _inChoices2 = false;
                    GotoCorrectDialogue(_choiceManager.currentChoices,_returnDialogues);
                    return;
                }
            }
            else if (_inChoices3)
            {
                if (!_choiceDialogues1[_currentDialogue])
                {
                    _inChoices3 = false;
                    GotoCorrectDialogue(_choiceManager.currentChoices,_returnDialogues);
                    return;
                }
            }

            if (_choiceDialogues[_currentDialogue])
            {
                _choiceManager.DialogueOptionsShow();
            }
            else
            {
                StartCoroutine(GradualText());
            }
        }
        

        public IEnumerator GradualText()
        {
            _generatingDialogue = true;
            namePlate.text = _names[_currentDialogue];
            dialogueText.text = "";
            var charArray = script[_currentDialogue].ToCharArray();
            for (var i = 0; i < charArray.Length; i++)
            {
                if (_stopGeneratingDialogue) continue;
                yield return new WaitForSeconds(textSpeed/100);
                dialogueText.text = dialogueText.text.Insert(i,charArray[i].ToString());
            }

            if (_stopGeneratingDialogue)
            {
                dialogueText.text = script[_currentDialogue];
            }
            _stopGeneratingDialogue = false;

            _generatingDialogue = false;
        }

        public void Log()
        {
            //TODO Fix log with dialogue choices
            if (!logGameObject.activeSelf)
            {
                logGameObject.SetActive(true);
                for (var i = 0; i < _currentDialogue+1; i++)
                {
                    _logText.text += "\n" + _names[i] + ": " + script[i];
                }
            }
            else
            {
                logGameObject.SetActive(false);
                _logText.text = "Log:";
            }
        }
//TODO inputfield.GetComponent<Text>().text

        private void ExtractAndReplaceNames()
        {
            //TODO positive and negative choices
            //TODO Different Dialogues after choices
            _names = new string[script.Length];
            _choiceDialogues = new bool[script.Length];
            _choiceDialogues1 = new bool[script.Length];
            _choiceDialogues2 = new bool[script.Length];
            _choiceDialogues3 = new bool[script.Length];
            _returnDialogues = new bool[script.Length];
            for (var i = 0; i < script.Length; i++)
            {
                if (script[i] == dialogueChoicesChar)
                {
                    _choiceDialogues[i] = true;
                }
                else if (script[i].Contains(choiceOneChar))
                {
                    script[i] = script[i].Remove(0, choiceOneChar.Length);
                    _choiceDialogues1[i] = true;
                }
                else if (script[i].Contains(choiceTwoChar))
                {
                    script[i] = script[i].Remove(0, choiceTwoChar.Length);
                    _choiceDialogues2[i] = true;
                }
                else if (script[i].Contains(choiceThreeChar))
                {
                    script[i] = script[i].Remove(0, choiceThreeChar.Length);
                    _choiceDialogues3[i] = true;
                }
                else if (script[i].Contains(returnChar))
                {
                    script[i] = script[i].Remove(0, returnChar.Length);
                    _returnDialogues[i] = true;
                }
                
                script[i] = script[i].Replace(replaceString, playerName);
                _names[i] = script[i].Split(":")[0];
                script[i] = script[i].Remove(0, script[i].IndexOf(":", StringComparison.Ordinal)+2);
            }
        }

        public void PositiveChoice(int currentChoice)
        {
            _inChoices1 = true;
            GotoCorrectDialogue(currentChoice,_choiceDialogues1);
        }

        public void NeutralChoice(int currentChoice)
        {
            _inChoices2 = true;
            GotoCorrectDialogue(currentChoice,_choiceDialogues2);
        }

        public void NegativeChoice(int currentChoice)
        {
            _inChoices3 = true;
            GotoCorrectDialogue(currentChoice,_choiceDialogues3);
        }

        public void GotoCorrectDialogue(int currentChoice, bool[] boolArray)
        {
            var inStreak = false;
            var skipNumber = currentChoice;
            for (var i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    if (!inStreak)
                    {
                        skipNumber--;
                    }
                    inStreak = true;
                    if (skipNumber > 0) continue;
                    _currentDialogue = i;
                    StartCoroutine(GradualText());
                    return;
                }
                inStreak = false;
            }
        }
    }
}