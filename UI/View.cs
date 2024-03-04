using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class View : MonoBehaviour, IPointerClickHandler
{
    public enum Direction
    {
        horizontal,
        vertical
    }

    // A name used for debugging
    public string viewName;

    // The direction in which any child views are arranged
    public Direction layoutDirection;

    // The default selection index
    public int initialSelection = 0;

    // Should the selection be remembered when the view is dismissed?
    public bool rememberSelection;

    // An action to perform if submit is pressed with the view active
    public UnityEvent action;  

    // An action to perform when the view is selected (as the active view)
    public UnityEvent onSelection;

    // An action to perform when the view is the root view and the launch button (gamepad west) is pressed
    public UnityEvent launchAction;

    // Content to reveal when this view is made active
    // public GameObject disclosedContent;

    // Should selection of any children loop through options
    // public bool looping = false;    

    // Should the selection indeces of any child view be synchronised?
    public bool synchroniseChildViews = false;

    // Display debug messages
    public bool debugging;

    // Initialisation
    private bool initialised;
    
    // Has the view selection changed from the default?
    private bool selectionStored;

    // A reference to the current view input controller
    private ViewInputController viewInputController;

    // Is the view currently selected? 
    public bool Selected
    {
        get
        {
            return _selection;
        }
    }
    
    private bool _selection;

    // Does the view have child views?
    public bool HasChildren
    {
        get
        {
            return children.Length > 0;
        }
    }

    // How many children does the view have??
    public int ChildCount
    {
        get
        {
            return children.Length;
        }
    }

    // Any views that are children of this component
    private View[] children;

    // The index of the currently selected child view
    public int CurrentSelectionIndex
    {
        get
        {            
            // Check that there are child views
            if (children.Length == 0)
            {
                Debug.LogError("Attempt to access non-existant subviews of View '" + viewName + "' ");
                return 0;
            }

            // Find all currently selected children
            View[] selectedChildren = Array.FindAll(children, c => c.Selected == true);

            if (selectedChildren.Length > 0)
            {
                if (selectedChildren.Length > 1)
                {
                    Debug.LogWarning("Multiple child views of View '" + viewName + "' were selected");
                }

                // Return the selection index
                return Array.IndexOf(children, selectedChildren[0]);
            }
            else
            {                
                return 0;
            }            
        }
    }

    // The currently selected child view
    private View CurrentSelection
    {
        get
        {            
            return children[CurrentSelectionIndex];
        }
    }

    // Is the first child view currently selected?
    private bool AtSelectionStart
    {
        get
        {      
            if (children.Length == 0)
            {
                return true;
            }      
            else return CurrentSelectionIndex == 0;
        }
    }

    // Is the final child view currently selected?
    private bool AtSelectionEnd
    {
        get
        {      
            if (children.Length == 0)
            {
                return true;
            }      
            else return CurrentSelectionIndex + 1 == children.Length;
        }
    }

    // Can the view navigate down?
    public bool CanNavigateDown
    {
        get
        {      
            if (children.Length == 0)
            {
                return false;
            }      
            else if (layoutDirection == Direction.vertical)
            {
                if (AtSelectionEnd)
                {
                    return children[^1].CanNavigateDown;
                }
                else
                {
                    return true;
                }
            }
            else             
            {
                return CurrentSelection.CanNavigateDown;
            }
        }
    }

    // Can the view navigate up?
    public bool CanNavigateUp
    {
        get
        {      
            if (children.Length == 0)
            {
                return false;
            }      
            else if (layoutDirection == Direction.vertical)
            {
                if (AtSelectionStart)
                {
                    return children[0].CanNavigateUp;
                }
                else
                {
                    return true;
                }
            }
            else             
            {
                return CurrentSelection.CanNavigateUp;
            }
        }
    }

    // Can the view navigate left?
    public bool CanNavigateLeft
    {
        get
        {      
            if (children.Length == 0)
            {
                return false;
            }      
            else if (layoutDirection == Direction.horizontal)
            {
                if (AtSelectionStart)
                {
                    return children[0].CanNavigateLeft;
                }
                else
                {
                    return true;
                }
            }
            else             
            {
                return CurrentSelection.CanNavigateLeft;
            }
        }
    }

    // Can the view navigate right?
    public bool CanNavigateRight
    {
        get
        {      
            if (children.Length == 0)
            {
                return false;
            }      
            else if (layoutDirection == Direction.horizontal)
            {
                if (AtSelectionEnd)
                {
                    return children[^1].CanNavigateRight;
                }
                else
                {
                    return true;
                }
            }
            else             
            {
                return CurrentSelection.CanNavigateRight;
            }
        }
    }

    // The active child of this view, if any
    public View ActiveChild
    {
        get
        {
            if (!initialised) { Initialise(); }

            if (children.Length > 0)
            {
                return CurrentSelection.ActiveChild;
            }
            else
            {
                return this;
            }
        }
    }

    private void Awake()
    {
        if (!initialised) { Initialise(); }
    }

    public void Refresh()
    {
        if (debugging)
        {
            Debug.Log("Refreshing " + viewName);
        }

        Initialise();        

        void Branch(Transform target)
        {                        
            foreach (Transform child in target)
            {
                if (child.TryGetComponent(out View view))
                {                    
                    view.Initialise();    
                    view.FocusScrollView(false);
                }

                if (child.TryGetComponent(out RowControl rowControl))
                {                    
                    rowControl.UpdateVisuals();
                }

                Branch(child);
            }
        }

        Branch(transform);
    }

    private void Initialise()
    {        
        initialised = true;
        
        viewInputController = FindObjectOfType<ViewInputController>();

        // Initialise the children array
        children = FindChildViews();

        // If there is no previous selection stored or the view does not remember its selection
        if (!selectionStored || !rememberSelection)
        {
            // Select the default child, if any
            SelectChild(initialSelection);
        }         
    }

    // Select the first child view
    public void SelectFirstChild()
    {                
        SelectChild(0);
    }

    // Select the nth child view
    public void SelectChild(int n)
    {                
        if (children.Length > 0)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetSelection(i == n);
            }
        }

        UpdateChildVisuals();
        selectionStored = true;
    }

    // Updates the visuals of the entire view hierarchy subtended by this view
    public void UpdateChildVisuals()
    {
        UpdateVisuals();

        void Branch(Transform target)
        {
            foreach (Transform child in target)
            {
                if (child.gameObject.TryGetComponent(out View childView))
                {                
                    childView.UpdateVisuals();
                }
                Branch(child);
            }
        }

        Branch(transform);
        
    }

    // Find all the children of this view which are direct children if only views are considered
    private View[] FindChildViews()
    {
        if (debugging)
        {
            Debug.Log("Finding child views of " + viewName);
        }
        List<View> childrenList = new();
        
        void SearchView(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.TryGetComponent(out View childView))
                {                
                    if (childView.gameObject.activeInHierarchy)
                    {
                        childrenList.Add(childView);                
                    }                    
                }
                else
                {
                    SearchView(child.transform);
                }
            }        
        }

        SearchView(transform);

        if (debugging)
        {
            Debug.Log("Found " + childrenList.Count + " children of view '" + viewName + "'");
        }

        return childrenList.ToArray();
    }

    // Select the specified child view
    public void SelectChild(View childToSelect)
    {
        void Branch(Transform target)
        {
            foreach (Transform child in target)
            {
                if (child.gameObject.TryGetComponent(out View childView))
                {                
                    childView.SetSelection(childView == childToSelect);
                }

                Branch(child);
            }
        }

        Branch(transform);
    }

    public void NavigateLeft(bool muted = false)
    {
        if (children.Length > 0)
        {
            if (CurrentSelection.CanNavigateLeft)
            {
                if (synchroniseChildViews)
                {
                    foreach (View child in children)
                    {
                        if (child.CanNavigateLeft)
                        {
                            child.NavigateLeft(child != CurrentSelection);    
                        }                        
                    }
                }
                else
                {
                    CurrentSelection.NavigateLeft();
                }       
            }
            else
            {
                if (layoutDirection == Direction.horizontal)
                {
                    SelectPrevious(muted);
                }
            }
        }     
    }

    public void NavigateRight(bool muted = false)
    {
        if (children.Length > 0)
        {
            if (CurrentSelection.CanNavigateRight)
            {
                if (synchroniseChildViews)
                {
                    foreach (View child in children)
                    {
                        if (child.CanNavigateRight)
                        {
                            child.NavigateRight(child != CurrentSelection);    
                        }                        
                    }
                }
                else
                {
                    CurrentSelection.NavigateRight();
                }                
            }
            else
            {
                if (layoutDirection == Direction.horizontal)
                {
                    SelectNext(muted);
                }
            }
        }    
    }

    public void NavigateUp(bool muted = false)
    {
        if (children.Length > 0)
        {
            if (CurrentSelection.CanNavigateUp)
            {
                if (synchroniseChildViews)
                {
                    foreach (View child in children)
                    {
                        if (child.CanNavigateUp)
                        {
                            child.NavigateUp(child != CurrentSelection);    
                        }                        
                    }
                }
                else
                {
                    CurrentSelection.NavigateUp();
                }       
            }
            else
            {
                if (layoutDirection == Direction.vertical)
                {
                    SelectPrevious(muted);
                }
            }
        }    
    }

    public void NavigateDown(bool muted = false)
    {
        if (children.Length > 0)
        {
            if (CurrentSelection.CanNavigateDown)
            {
                if (synchroniseChildViews)
                {
                    foreach (View child in children)
                    {
                        if (child.CanNavigateDown)
                        {
                            child.NavigateDown(child != CurrentSelection);    
                        }                        
                    }
                }
                else
                {
                    CurrentSelection.NavigateDown();
                }       
            }
            else
            {
                if (layoutDirection == Direction.vertical)
                {
                    SelectNext(muted);
                }
            }
        }    
    }

    private void SelectPrevious(bool muted = false)
    {
        // Debug.Log("Select Previous");
        if (CurrentSelectionIndex > 0)
        {
            if (children.Length > 0)
            {
                int previousSelectionIndex = CurrentSelectionIndex;

                if (!muted) AudioManager.Play("Menu Navigation");

                for (int i = 0; i < children.Length; i++)
                {
                    children[i].SetSelection(i == previousSelectionIndex - 1);
                }

                FocusScrollView();
                selectionStored = true;
            }
        }
    }

    private void SelectNext(bool muted = false)
    {
        // Debug.Log("Select Next");
        if (CurrentSelectionIndex + 1 < children.Length)
        {        
            if (children.Length > 0)
            {
                int previousSelectionIndex = CurrentSelectionIndex;

                if (!muted) AudioManager.Play("Menu Navigation");

                for (int i = 0; i < children.Length; i++)
                {
                    children[i].SetSelection(i == previousSelectionIndex + 1);
                }

                FocusScrollView();
                selectionStored = true;
            }                
        }        
    }

    public void SetSelection(bool newValue)
    {
        if (debugging && newValue)
        {
            Debug.Log("View '" + viewName + "' Selected");
        }

        _selection = newValue;        
    }

    // If the view has a scroll rect component, scroll to the current selection
    private void FocusScrollView(bool animated = true)
    {        
        if (TryGetComponent(out ScrollRect scroller)) 
        {            
            float i = CurrentSelectionIndex;
            float n = ChildCount;

            if (debugging)
            {
                if (animated)
                {
                    Debug.Log("Focusing '" + viewName + "': i=" + i + ", n=" + n + " with animation");
                }                
                else 
                {
                    Debug.Log("Focusing '" + viewName + "': i=" + i + ", n=" + n + " without animation");
                }
            }

            if (n > 1)
            {
                float y0 = scroller.normalizedPosition.y;
                float y1 = 1 - (i / (n-1));        

                if (animated)
                {
                    LeanTween.cancel(gameObject);                    
                    LeanTween.value(gameObject, 0, 1, 1)
                    .setOnUpdate((float t) => {
                        scroller.normalizedPosition = new Vector2(0, (1-t)*y0 + t*y1);
                    })
                    .setEase(LeanTweenType.easeOutExpo);    
                }
                else
                {
                    LeanTween.cancel(gameObject);                    
                    LeanTween.delayedCall(0.05f, () => {
                        scroller.normalizedPosition = new Vector2(0, y1);
                    });
                    Canvas.ForceUpdateCanvases();
                }
            }  
        }          
    }

    private void UpdateVisuals()
    {
        if (!initialised) { Initialise(); }

        bool active = this == viewInputController.rootView.ActiveChild;
        
        if (TryGetComponent(out SelectableViewStyler selectableViewStyler))
        {
            selectableViewStyler.SetHighlight(active);
        }        
    }

    public void OnPointerClick(PointerEventData eventData)
    {        
        if (!HasChildren)
        {
            Debug.Log("Clicked: " + viewName);
            viewInputController.rootView.SelectChild(this);
            viewInputController.rootView.UpdateChildVisuals();
            onSelection.Invoke();
            action.Invoke();
        }
    }

    // Generates a debug description of the view and its selection state
    public string DebugDescription()
    {        
        string Branch(View view, int depth)
        {
            var lineStart = "";
            for (int i = 0; i < depth; i++)
            {
                lineStart += "----";
            }
            
            var output = "";
            output += lineStart;
            string viewDescription = view.viewName == "" ? "Unnamed View" : view.viewName;
            viewDescription = viewDescription.ToLower();
            if (view.Selected) viewDescription = viewDescription.ToUpper();
            output += viewDescription;

            if (view.children.Length > 0)
            {
                output+="\n" + lineStart + "{\n";
                foreach (View child in view.children)
                {                                      
                    output+= Branch(child, depth + 1);
                    output += "\n";
                }
                output += lineStart + "}";
            }
            return output;
        }
        return Branch(this, 0);
    }
}
