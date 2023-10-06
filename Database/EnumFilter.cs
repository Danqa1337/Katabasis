using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnumFilter<T> where T : Enum
{
    [SerializeField] private List<T> _allowedList = new List<T>();
    [SerializeField] private List<T> _forbidenList = new List<T>();
    [SerializeField] private bool _allowsAny = false;

    private T _enumAnyValue;

    public bool Allows(T name)
    {
        return !_forbidenList.Contains(name) && (_allowedList.Contains(name) || name.Equals(_enumAnyValue) || _allowsAny);
    }

    public EnumFilter()
    {
        _allowsAny = true;
        SetAnyValue();
    }

    public EnumFilter(string enumString)
    {
        if (enumString.Length > 0)
        {
            foreach (string splited in enumString.Split(','))
            {
                if (splited != "")
                {
                    bool forbiden = false;
                    if (splited[0] == '!')
                    {
                        forbiden = true;
                        splited.Remove(0, 1);
                    }
                    var enumValues = Enum.GetValues(typeof(T));

                    bool enumFound = false;
                    foreach (T enumValue in enumValues)
                    {
                        var enumToString = enumValue.ToString().ToLower();

                        if (enumToString == splited.ToLower())
                        {
                            //Debug.Log(e.ToString());
                            if (forbiden)
                            {
                                _forbidenList.Add(enumValue);
                            }
                            else
                            {
                                if (enumToString == "any") _allowsAny = true;
                                _allowedList.Add(enumValue);
                            }

                            enumFound = true;
                            break;
                        }
                    }

                    if (!enumFound) throw new NullReferenceException("There is no such name " + splited + " inside " + typeof(T).ToString() + " enum");
                }
            }
        }
        else
        {
            _allowsAny = true;
        }
        SetAnyValue();
    }

    private void SetAnyValue()
    {
        var enumValues = Enum.GetValues(typeof(T));
        var anyValueFound = false;

        foreach (T value in enumValues)
        {
            if (value.ToString() == "Any")
            {
                anyValueFound = true;
                _enumAnyValue = value;
            }
        }
        if (!anyValueFound)
        {
            throw new Exception("Enum " + typeof(T) + " has no Any value");
        }
    }

    public EnumFilter(IEnumerable<T> allowedEnums, IEnumerable<T> forbiddenEnums)
    {
        this._allowedList = allowedEnums.ToList();
        this._forbidenList = forbiddenEnums.ToList();
    }

    public EnumFilter(IEnumerable<T> allowedEnums)
    {
        foreach (var item in allowedEnums)
        {
            if (item.ToString() == "Any")
            {
                _allowsAny = true;
                break;
            }
        }
        this._allowedList = allowedEnums.ToList();
        this._forbidenList = new List<T>();
    }

    public override string ToString()
    {
        var message = "Enum filter allows ";
        if (_allowsAny)
        {
            message += "any ";

            if (_forbidenList.Count > 0)
            {
                message += "except ";
                foreach (var item in _forbidenList)
                {
                    message += item.ToString();
                    message += ", ";
                }
            }
        }
        else
        {
            foreach (var item in _allowedList)
            {
                message += item.ToString();
                message += ", ";
            }
        }

        return message;
    }
}