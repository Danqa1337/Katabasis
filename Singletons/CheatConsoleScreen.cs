using Assets.Scripts;
using ConsoleCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum ConsoleCommandName
{
    Spawn,
    Destroy,
    MapAllTiles,
    Tp,
    Null,
    LevelUp,
    MakeAbyss,
    DetachPart,
    RestorePart,
    Build,
    FindStructure,
    AddEffect,
    AddXP,
    RemoveEffect,
    ClearEffects,
    Sacrifice,
    MakeAlly,
    MakeEnemy,
    SpawnPack,
    SpawnPackGeneric,
    AddToSquad,
    GoToLocation,
    Resurrect,
    GrantPerk,
    RevokePerk,
    SetGodRelations,
    SetGodAttention,
}

public class CheatConsoleScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _consoleInputField;
    [SerializeField] public TextMeshProUGUI _oldInputText;
    [SerializeField] private TextMeshProUGUI _sugestionsText;
    [SerializeField] private TextMeshProUGUI _currentSugestionText;

    private int _prevInputNum;
    private List<string> _prevousInputs = new List<string>();
    private List<string> _currentInput;
    private CircularList<string> _sugestions = new CircularList<string>();

    public void Awake()
    {
        _currentInput = new List<string>();
    }

    private void Start()
    {
        _prevousInputs.Add(PlayerPrefs.GetString("lastConsoleInput"));
    }

    private void OnEnable()
    {
        UiManager.OnShowCanvas += DoOnCanvasShow;
        UiManager.OnHideCanvas += DoOnCanvasHide;
        _consoleInputField.onValueChanged.AddListener(delegate { ReadInput(); });
        _consoleInputField.onSubmit.AddListener(delegate { OnSubmit(); });
        CheatConsole.OnCommandExecuted += ProcessCommandResult;
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= DoOnCanvasShow;
        UiManager.OnHideCanvas -= DoOnCanvasHide;
        _consoleInputField.onValueChanged.RemoveListener(delegate { ReadInput(); });
        _consoleInputField.onSubmit.RemoveListener(delegate { OnSubmit(); });
        CheatConsole.OnCommandExecuted -= ProcessCommandResult;
    }

    private void UpdateSugestions()
    {
        _sugestions = new CircularList<string>();
        _currentSugestionText.text = "";
        Array sugestionsList = null;
        if (_currentInput.Count != 0)
        {
            if (_currentInput.Count == 1)
            {
                sugestionsList = Enum.GetValues(typeof(ConsoleCommandName));
            }
            else if (_currentInput.Count == 2)
            {
                var commandName = ConsoleCommandName.AddEffect;
                if (_currentInput[0].DecodeCharSeparatedEnumsAndGetFirst<ConsoleCommandName>(out commandName))
                {
                    sugestionsList = GetSugestions(commandName);
                }
                else
                {
                    sugestionsList = null;
                }
            }

            if (sugestionsList != null)
            {
                foreach (var item in sugestionsList)
                {
                    if (item.ToString().ToLower().StartsWith(_currentInput[_currentInput.Count - 1].ToLower()))
                    {
                        _sugestions.Add(item.ToString());
                    }
                }
                _sugestionsText.text = "";
            }
        }
        else
        {
            foreach (var item in Enum.GetValues(typeof(ConsoleCommandName)))
            {
                _sugestions.Add(item.ToString());
            }
        }
        foreach (var item in _sugestions)
        {
            _sugestionsText.text += "\n" + item;
        }
    }

    private string[] GetSugestions(ConsoleCommandName consoleCommand) => (consoleCommand) switch
    {
        ConsoleCommandName.Spawn =>

            Enum.GetNames(typeof(ComplexObjectName)).Concat(Enum.GetNames(typeof(SimpleObjectName))).ToArray()
        ,
        ConsoleCommandName.SpawnPack => Enum.GetNames(typeof(PackName)),
        ConsoleCommandName.SpawnPackGeneric => Enum.GetNames(typeof(SimpleObjectName)),
        ConsoleCommandName.DetachPart => Enum.GetNames(typeof(BodyPartTag)),
        ConsoleCommandName.RestorePart => Enum.GetNames(typeof(BodyPartTag)),
        ConsoleCommandName.Build => Enum.GetNames(typeof(StructureName)),
        ConsoleCommandName.FindStructure => Enum.GetNames(typeof(StructureName)),
        ConsoleCommandName.RemoveEffect => Enum.GetNames(typeof(EffectName)),
        ConsoleCommandName.AddEffect => Enum.GetNames(typeof(EffectName)),
        ConsoleCommandName.GoToLocation => Registers.GlobalMapRegister.Locations.Select(l => l.Id.ToString()).ToArray(),
        ConsoleCommandName.GrantPerk => Enum.GetNames(typeof(PerkName)),
        ConsoleCommandName.RevokePerk => Enum.GetNames(typeof(PerkName)),
        ConsoleCommandName.SetGodRelations => Registers.GodsRegister.GetAllGods().Select(g => g.Index.ToString()).ToArray(),
        ConsoleCommandName.SetGodAttention => Registers.GodsRegister.GetAllGods().Select(g => g.Index.ToString()).ToArray(),
        _ => null,
    };

    private void ToNextSugestion()
    {
        if (_sugestions.Count > 0)
        {
            string newInput = "";
            for (int i = 0; i < _currentInput.Count - 1; i++)
            {
                newInput += _currentInput[i] + ".";
            }

            newInput += _sugestions.Current;
            _sugestions.Next();
            _currentSugestionText.text = newInput;
        }
    }

    public void ScrollUp()
    {
        if (_prevousInputs.Count > 0)
        {
            _prevInputNum++;
            if (_prevInputNum >= _prevousInputs.Count) _prevInputNum = _prevousInputs.Count - 1;
            _consoleInputField.text = _prevousInputs[_prevInputNum];
        }
    }

    public void ScrollDown()
    {
        if (_prevousInputs.Count > 0)
        {
            _prevInputNum--;
            if (_prevInputNum < 0) _prevInputNum = 0;
            _consoleInputField.text = _prevousInputs[_prevInputNum];
        }
    }

    private void ReadInput()
    {
        _currentInput.Clear();
        string updatedInput;

        if (_consoleInputField.text != "")
        {
            var splitedInput = _consoleInputField.text.Split('.');
            foreach (var item in splitedInput)
            {
                _currentInput.Add(item);
            }
        }

        UpdateSugestions();
    }

    public void OnSubmit()
    {
        if (_currentSugestionText.text != "")
        {
            UseSugestion();
        }
        else
        {
            if (_currentInput.Count > 0 && _currentInput[0] != "")
            {
                _oldInputText.text += "\n " + _consoleInputField.text;

                var command = DecodeCommand();
                if (command != ConsoleCommandName.Null)
                {
                    PlayerPrefs.SetString("lastConsoleInput", _consoleInputField.text);
                    ExecuteCommand(command);
                }
                else
                {
                    ThrowWrongInput();
                }
            }
            ClearInput();
        }

        _consoleInputField.caretPosition = _consoleInputField.text.Length;
        _consoleInputField.Select();
        _consoleInputField.ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToNextSugestion();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) ScrollUp();
        if (Input.GetKeyDown(KeyCode.DownArrow)) ScrollDown();
    }

    private void UseSugestion()
    {
        if (_currentSugestionText.text != "")
        {
            _consoleInputField.text = _currentSugestionText.text;
            _currentSugestionText.text = "";
        }
    }

    private void ClearInput()
    {
        string inputSummary = "";
        foreach (var item in _currentInput)
        {
            if (item != _currentInput[0] && item != "") inputSummary += ".";
            inputSummary += item;
        }

        if (inputSummary != "")
        {
            if (_prevousInputs.Count == 0 || _prevousInputs[_prevousInputs.Count - 1] != inputSummary)
            {
                _prevousInputs.Add(inputSummary);
            }

            _prevInputNum = _prevousInputs.Count;
            _consoleInputField.text = "";
        }
    }

    private ConsoleCommandName DecodeCommand()
    {
        var command = ConsoleCommandName.Null;
        _currentInput[0].DecodeCharSeparatedEnumsAndGetFirst<ConsoleCommandName>(out command);
        return command;
    }

    private void ExecuteCommand(ConsoleCommandName command)
    {
        var arguments = new List<string>();
        arguments.AddRange(_currentInput);
        arguments.RemoveAt(0);
        CheatConsole.ExecuteCommand(command, arguments.ToArray());
    }

    private void NewLine(string line, string color)
    {
        _oldInputText.text += "\n  <color=" + color + ">" + line + "</color>";
    }

    private void ThrowWrongInput()
    {
        NewLine("Wrong input", "red");
    }

    private void ProcessCommandResult(ConsoleCommands.ConsoleCommandResult result)
    {
        _oldInputText.text += "\n  <color=" + result.Color + ">" + result.Message + "</color>";
    }

    private void DoOnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Console)
        {
            _consoleInputField.gameObject.SetActive(true);
            _consoleInputField.ActivateInputField();
        }
    }

    private void DoOnCanvasHide(UIName uIName)
    {
        if (uIName == UIName.Console)
        {
            _consoleInputField.gameObject.SetActive(false);
            _consoleInputField.DeactivateInputField(true);
        }
    }
}