using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPreference
{
    public string Key { get; }
    public string Name { get; }    
    public string Description { get; }
    public void LoadFromPlayerPrefs();
}

public class StringPreference: IPreference
{
    private string key;
    private string name;    

    public string value;    
    public string Key => key;
    public string Name => name;
    public string Description => value;

    public void LoadFromPlayerPrefs()
    {
        throw new System.NotImplementedException();
    }
}

public class DiscretePreference: IPreference
{
    private string key;
    private string name;    
    
    public int value;
    private string[] descriptions;
    public string[] explanations;

    public string Description
    {
        get
        {
            return DescriptionFor(value);
        }
    }

    public string DescriptionFor(int value)
    {
        if (value >= 0 && value < descriptions.Length)
        {
            return descriptions[value];
        }        
        else
        {
            return "Missing description";
        }
    }

    public void Decrement(bool looping)
    {
        if (value <= 0)
        {
            if (looping)
            {
                value = descriptions.Length - 1;
            }
        }
        else
        {
            value--;
        }
    }

    public void Increment(bool looping)
    {
        if (value + 1 >= descriptions.Length)
        {
            if (looping)
            {
                value = 0;
            }
        }
        else
        {
            value++;
        }
    }
    
    public string Key => key;
    public string Name => name;

    public void LoadFromPlayerPrefs()
    {
        throw new System.NotImplementedException();
    }

    public DiscretePreference(string _key, string _name, string[] _descriptions, int defaultValue = 0, string[] _explanations = null)
    {
        key = _key;
        name = _name;
        descriptions = _descriptions;
        value = defaultValue;        
        explanations = _explanations;
    }
}

public class BooleanPreference: IPreference
{
    private string key;
    private string name;

    public bool value;

    private string onDescription;
    private string offDescription;

    public string Description
    {
        get
        {
            return DescriptionFor(value);
        }
    }

    public string DescriptionFor(bool value)
    {
        return value ? onDescription : offDescription;
    }
    
    public void Toggle()
    {
        value = !value;
    }

    public string Key => key;
    public string Name => name;

    public void LoadFromPlayerPrefs()
    {
        throw new System.NotImplementedException();
    }

    public BooleanPreference(string _key, string _name, string _onDescrption = "On", string _offDescription = "Off", bool defaultValue = true)
    {
        key = _key;
        name = _name;
        onDescription = _onDescrption;
        offDescription = _offDescription;
        value = defaultValue;        
    }
}