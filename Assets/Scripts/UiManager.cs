using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject _loginUI;
    public GameObject _registerUI;
    public GameObject _dataUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        LoginScreen();
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        _loginUI.SetActive(true);
        _dataUI.SetActive(false);
        _registerUI.SetActive(false);
    }
    public void RegisterScreen() // Regester button
    {
        _loginUI.SetActive(false);
        _registerUI.SetActive(true);
    }

    public void DataScreen()
    {
        _loginUI.SetActive(false);
        _dataUI.SetActive(true);
    }
}