using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class RussianKeyboard : MonoBehaviour
{
    public event Action<char> OnKeyPressed;
    public event Action OnBackspacePressed;
    public event Action OnSpacePressed;

    [Header("Keyboard Rows")]
    [SerializeField] private Transform _upperRow;
    [SerializeField] private Transform _homeRow;
    [SerializeField] private Transform _bottomRow;
    [SerializeField] private Button _backspaceButton;
    [SerializeField] private Button _spaceButton;
    [SerializeField] private Button _yoButton;  // Кнопка Ё
    [SerializeField] private Button _dashButton; // Кнопка -

    [Header("Settings")]
    [SerializeField] private Color _normalKeyColor = Color.white;
    [SerializeField] private Color _pressedKeyColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private float _keyPressAnimationDuration = 0.1f;

    private Dictionary<Button, char> _keyMapping = new Dictionary<Button, char>();

    public void Awake()
    {
        if (_upperRow == null || _homeRow == null || _bottomRow == null)
        {
            Debug.LogError("Keyboard rows are not assigned in inspector!");
            return;
        }
        InitializeKeyboard();
        SetupSpecialButtons();
    }

    private void InitializeKeyboard()
    {
        SetupRow(_upperRow, new char[] { 'й', 'ц', 'у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з' });

        SetupRow(_homeRow, new char[] { 'ф', 'ы', 'в', 'а', 'п', 'р', 'о', 'л', 'д', 'ж', 'х' });

        SetupRow(_bottomRow, new char[] { 'я', 'ч', 'с', 'м', 'и', 'т', 'ь', 'ъ', 'б', 'ю', 'э' });
    }

    private void SetupRow(Transform rowParent, char[] letters)
    {
        for (int i = 0; i < letters.Length; i++)
        {
            if (i < rowParent.childCount)
            {
                var button = rowParent.GetChild(i).GetComponent<Button>();
                if (button != null)
                {
                    _keyMapping.Add(button, letters[i]);
                    button.GetComponent<KeyboardButton>().SetLetter(letters[i]);
                    button.onClick.AddListener(() => OnKeyButtonPressed(button));
                }
            }
        }
    }

    private void SetupSpecialButtons()
    {
        // Кнопка Backspace
        _backspaceButton.onClick.AddListener(() =>
        {
            OnBackspacePressed?.Invoke();
            AnimateButtonPress(_backspaceButton);
        });

        // Кнопка Пробела
        _spaceButton.onClick.AddListener(() =>
        {
            OnSpacePressed?.Invoke();
            OnKeyPressed?.Invoke(' ');
            AnimateButtonPress(_spaceButton);
        });

        // Кнопка Ё
        _yoButton.onClick.AddListener(() =>
        {
            OnKeyPressed?.Invoke('ё');
            AnimateButtonPress(_yoButton);
        });

        // Кнопка дефиса
        _dashButton.onClick.AddListener(() =>
        {
            OnKeyPressed?.Invoke('-');
            AnimateButtonPress(_dashButton);
        });
    }

    private void OnKeyButtonPressed(Button button)
    {
        OnKeyPressed?.Invoke(button.GetComponent<KeyboardButton>().GetLetter());
        AnimateButtonPress(button);
    }

    private void AnimateButtonPress(Button button)
    {
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            StartCoroutine(AnimateButtonColor(image));
        }
    }

    private System.Collections.IEnumerator AnimateButtonColor(Image image)
    {
        image.color = _pressedKeyColor;
        yield return new WaitForSeconds(_keyPressAnimationDuration);
        image.color = _normalKeyColor;
    }
}