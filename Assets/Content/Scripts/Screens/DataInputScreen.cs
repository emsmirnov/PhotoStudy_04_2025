using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DataInputScreen : ScreenBase
{
    [SerializeField] private Button _nameInputButton;
    [SerializeField] private Button _surnameInputButton;

    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _surnameText;

    [SerializeField] private TextMeshProUGUI _namePlaceholder;
    [SerializeField] private TextMeshProUGUI _surnamePlaceholder;

    [SerializeField] private RussianKeyboard _keyboard;

    private const string NAME_PLACEHOLDER = "ИМЯ";
    private const string SURNAME_PLACEHOLDER = "ФАМИЛИЯ";
    private Color _placeholderColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

    private bool _isNameActive = false;
    private bool _isSurNameActive = false;
    private string _currentName = "";
    private string _currentSurname = "";
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.8f;

    public override void Initialize()
    {
        _nameInputButton.onClick.AddListener(ActivateNameField);
        _surnameInputButton.onClick.AddListener(ActivateSurnameField);
        _mainMenuButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.AddListener(OnMainPressed);
        _nextButton.onClick.RemoveAllListeners();
        _nextButton.onClick.AddListener(OnNextPressed);
        SetupKeyboard();
        ResetFields();
        ActivateNameField();
    }

    private void SetupKeyboard()
    {
        _keyboard.OnKeyPressed += HandleKeyPress;
        _keyboard.OnBackspacePressed += HandleBackspace;
        _keyboard.OnSpacePressed += HandleSpace;
    }

    private void ActivateNameField()
    {
        _isNameActive = true;
        _isSurNameActive = false;
        UpdatePlaceholderVisibility();
    }

    private void ActivateSurnameField()
    {
        _isNameActive = false;
        _isSurNameActive = true;
        UpdatePlaceholderVisibility();
    }

    private void HandleKeyPress(char character)
    {
        if (_isNameActive)
        {
            if (_currentName.Length == 0)
                character = char.ToUpper(character);
            else if (char.IsWhiteSpace(_currentName[_currentName.Length - 1]) || _currentName[_currentName.Length - 1] == '-')
                character = char.ToUpper(character);

            _currentName += character;
            _nameText.text = _currentName;
        }
        if (_isSurNameActive)
        {
            if (_currentSurname.Length == 0)
                character = char.ToUpper(character);
            else if (char.IsWhiteSpace(_currentSurname[_currentSurname.Length - 1]) || _currentSurname[_currentSurname.Length - 1] == '-')
                character = char.ToUpper(character);

            _currentSurname += character;
            _surnameText.text = _currentSurname;
        }

        UpdatePlaceholderVisibility();
    }

    private void HandleBackspace()
    {
        if (_isNameActive && _currentName.Length > 0)
        {
            _currentName = _currentName.Remove(_currentName.Length - 1);
            _nameText.text = _currentName;
        }
        else if (!_isNameActive && _currentSurname.Length > 0)
        {
            _currentSurname = _currentSurname.Remove(_currentSurname.Length - 1);
            _surnameText.text = _currentSurname;
        }

        UpdatePlaceholderVisibility();
    }

    private void HandleSpace()
    {
        if (_isNameActive)
        {
            _currentName += " ";
            _nameText.text = _currentName;
        }
        else
        {
            _currentSurname += " ";
            _surnameText.text = _currentSurname;
        }
    }

    private void UpdatePlaceholderVisibility()
    {
        _namePlaceholder.gameObject.SetActive(string.IsNullOrEmpty(_currentName) && !_isNameActive);
        _surnamePlaceholder.gameObject.SetActive(string.IsNullOrEmpty(_currentSurname) && !_isSurNameActive);
    }

    private void ResetFields()
    {
        _currentName = "";
        _currentSurname = "";
        _nameText.text = "";
        _surnameText.text = "";

        _namePlaceholder.text = NAME_PLACEHOLDER;
        _namePlaceholder.color = _placeholderColor;

        _surnamePlaceholder.text = SURNAME_PLACEHOLDER;
        _surnamePlaceholder.color = _placeholderColor;

        UpdatePlaceholderVisibility();
    }

    public override IEnumerator AnimateShow()
    {
        ResetFields();
        yield return AnimateFadeIn(_canvasGroup, 0.3f);
    }

    public override IEnumerator AnimateHide()
    {
        yield return AnimateFadeOut(_canvasGroup, 0.3f);
    }

    public (string name, string surname) GetEnteredData()
    {
        return (_currentName, _currentSurname);
    }

    public void ClearFields()
    {
        ResetFields();
    }

    private void OnNextPressed()
    {
        _isNameActive = false;
        _isSurNameActive = false;
        var emptyFields = false;
        if (string.IsNullOrWhiteSpace(_currentName))
        {
            _namePlaceholder.color = Color.red;
            emptyFields = true;
        }
        //if (string.IsNullOrWhiteSpace(_currentSurname))
        //{
        //    _surnamePlaceholder.color = Color.red;
        //    emptyFields = true;
        //}
        if (emptyFields)
        {
            UpdatePlaceholderVisibility();
            return;
        }
        GlobalChosesDataContainer.Instance.Name = _currentName;
        GlobalChosesDataContainer.Instance.Surname = _currentSurname;
        ScreenManager.Instance.ShowScreen<CoverCategoryScreen>();
    }

    private void OnMainPressed()
    {
        GlobalChosesDataContainer.Instance.Name = "";
        GlobalChosesDataContainer.Instance.Surname = "";
        ScreenManager.Instance.StartScreens();
        // ScreenManager.Instance.ShowScreen<ScreensaverScreen>();
    }
}