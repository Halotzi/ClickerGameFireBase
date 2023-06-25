using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Text _clicksText;

    public void NewScoreElement(string username, int clicks)
    {
        _usernameText.text = username;
        _clicksText.text = clicks.ToString();
    }
}
