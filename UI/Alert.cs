using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Alert : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI message;
    public GameObject inputArea;
    public TextMeshProUGUI inputText;
    public TextMeshProUGUI warning;

    public UIFader fader;
    
    public View dismissButton;
    public View confirmationButton;

    public ViewInputController inputController;   
    private View previousRootView;

    private Action confirmationAction;
    private Action dismissAction;
    private Func<string, string> typingHandlerFunction;

    // Start is called before the first frame update
    private void Start()
    {
        gameObject.SetActive(false);
        fader.gameObject.SetActive(false);        
    }

    // Show the alert with an input field
    public void ShowInputAlert(string titleText, string messageText, Func<string, string> _typingHandlerFunction, Action _confirmationAction)
    {
        Show(titleText, messageText, _confirmationAction, null, true, _typingHandlerFunction);
    }

    // Show the alert
    public void Show(
        string titleText,
        string messageText,
        Action _confirmationAction = null,
        Action _dismissAction = null,
        bool showingTextField = false,
        Func<string, string> _typingHandlerFunction = null
    )
    {
        confirmationAction = _confirmationAction;
        dismissAction = _dismissAction;
        typingHandlerFunction = _typingHandlerFunction;

        previousRootView = inputController.rootView;

        title.text = titleText;
        message.text = messageText;

        bool multipleButtons = _confirmationAction != null;
        confirmationButton.gameObject.SetActive(multipleButtons);
        inputArea.SetActive(showingTextField);
        warning.gameObject.SetActive(false);

        dismissButton.GetComponentInChildren<TextMeshProUGUI>().text = multipleButtons ? "Cancel" : "OK";

        UnityEvent dismissEvent = new UnityEvent();
        dismissEvent.AddListener(delegate {
            Dismiss();
        });

        UnityEvent confirmationEvent = new UnityEvent();
        confirmationEvent.AddListener(delegate {
            Confirm();
        });

        dismissButton.action = dismissEvent;
        confirmationButton.action = confirmationEvent;

        fader.Show();
        GetComponent<UISlider>().Show();
        View thisView = GetComponent<View>();        
        inputController.rootView = thisView;
        thisView.Refresh();
    }

    // Hide the alert
    private void HideAlert()
    {
        if (previousRootView != null)
        {
            inputController.rootView = previousRootView;
        }      
        else
        {
            Debug.LogWarning("Exiting alert with no root view");
        }  
        fader.Dismiss();                
        GetComponent<UISlider>().Dismiss();
    }

    public void Dismiss()
    {
        if (dismissAction != null)
        {
            dismissAction();
        }    
        HideAlert();
    }

    public void Confirm()
    {
        if (confirmationAction != null)
        {
            confirmationAction();
        }    
        HideAlert();
    }

    // Handle any typing response for the text field
    public void OnType(string input)
    {
        string warningMessage = typingHandlerFunction?.Invoke(input);
        if (warningMessage != "")
        {
            warning.text = warningMessage;
            warning.gameObject.SetActive(true);
        }
        else
        {
            warning.gameObject.SetActive(false);
        }
    }
}
