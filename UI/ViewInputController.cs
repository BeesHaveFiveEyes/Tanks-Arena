using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ViewInputController : MonoBehaviour
{
    // The currently active root view element
    public View rootView;

    // The view to return to when dismissin this view
    public View previousView;

    // Input controls
    private UIControls controls;

    // Is the controller active
    public bool active = true;

    // An action to perform on escape
    public Action<View> escapeAction;

    // Is input locked?
    public bool locked = false;

    // Should the root view debug description be printed?
    public bool debugging;

    // Internal trackers
    private bool navigationInitiated;  

    private void Start()
    {        
        ConfigureControls();        
    }

    // Override the root view
    public void SetRootView(View view, bool reversable)
    {
        View reverseTarget = rootView;
        rootView = view;        
        previousView = reversable ? reverseTarget : null;
        view.Refresh();
    }

    private void NavigateInDirection(Vector2 direction)
    {
        // Horizontal navigation
        if (direction.x < -0.5f)
        {
            rootView.NavigateLeft();
        }
        else if (direction.x > 0.5f)
        {
            rootView.NavigateRight();
        }

        // Vertical navigation
        if (direction.y < -0.5f)
        {
            rootView.NavigateDown();
        }
        else if (direction.y > 0.5f)
        {
            rootView.NavigateUp();
        }

        if (debugging && rootView != null)
        {
            Debug.Log(rootView.DebugDescription());
        }
    }

    private void ConfigureControls()
    {        
        // Create and enable the controls
        controls = new UIControls();        
        controls.Navigation.Enable();        

        // Navigation
        controls.Navigation.Navigate.performed += ctx => {            

            if(!locked && active && rootView != null)
            {
                if (rootView != null)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();
                    if (value.magnitude > 0.2f)
                    {     
                        if (!navigationInitiated)
                        {
                            navigationInitiated = true;
                            NavigateInDirection(value);
                            rootView.UpdateChildVisuals();
                            UnityEvent selectionAction = rootView.ActiveChild.onSelection;
                            if (selectionAction != null)
                            {
                                selectionAction.Invoke();
                            }
                        }                                       
                    }                  
                    else
                    {
                        navigationInitiated = false;
                    }
                }                               
                else if (active)
                {
                    Debug.LogWarning("Navigation attempted with no root view.");
                }
            }            
        };

        // Submit
        controls.Navigation.Submit.performed += ctx => {            
            if(!locked && active && rootView != null)
            {
                UnityEvent action = rootView.ActiveChild.action;
                if (action != null)
                {
                    AudioManager.Play("Menu Setting");
                    action.Invoke();
                }
            }            
        };

        // Escape
        controls.Navigation.Cancel.performed += ctx => {            
            if(!locked && rootView != null && previousView != null)
            {                
                escapeAction(previousView);                
                previousView = null;                                                
            }            
        };

        // Launch
        controls.Navigation.Launch.performed += ctx => {            
            if(!locked && rootView != null)
            {                
                if (rootView.launchAction != null)
                {
                    AudioManager.Play("Menu Setting");
                    rootView.launchAction.Invoke();
                }                                           
            }            
        };
    }
}
