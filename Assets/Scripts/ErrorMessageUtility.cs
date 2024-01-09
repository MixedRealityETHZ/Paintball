using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessageUtility : MonoBehaviour
{
    private static ErrorMessageUtility instance;

    [SerializeField]
    private TMP_Text text;

    private readonly List<string> errorMessages = new();
    private bool isDirty;

    public static void ShowError(string message)
    {
        Debug.LogError(message);

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (instance == null)
        {
            return;
        }

        if (instance.errorMessages.Contains(message))
        {
            return;
        }

        instance.errorMessages.Add(message);
        instance.isDirty = true;
    }

    public static void ShowErrorAndThrow(Exception exception)
    {
        ShowError(exception.Message);
        throw exception;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            ShowError("Another instance of the ErrorMessageUtility already exists.");
            throw new InvalidOperationException("Another instance of the ErrorMessageUtility already exists.");
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (!this.isDirty || this.text == null || this.errorMessages.Count == 0)
        {
            return;
        }

        this.text.text = string.Join('\n', this.errorMessages);
        this.isDirty = false;
    }
}
