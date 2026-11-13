
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float characterInterval = 0.05f;

    private Coroutine typingCoroutine;

    public bool IsTyping { get; private set; }

    public event Action OnTypingCompleted;

    public void StartTyping(string text)
    {
        StopTyping();

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        IsTyping = true;

        messageText.text = text;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate();

        int characterCount = messageText.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            messageText.maxVisibleCharacters = i + 1;

            yield return new WaitForSeconds(characterInterval);
        }

        messageText.maxVisibleCharacters = characterCount;

        IsTyping = false;
        typingCoroutine = null;

        OnTypingCompleted?.Invoke();
    }

    public void CompleteText()
    {
        if (!IsTyping)
            return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;

        messageText.maxVisibleCharacters =
            messageText.textInfo.characterCount;

        IsTyping = false;

        OnTypingCompleted?.Invoke();
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsTyping = false;
    }
}
