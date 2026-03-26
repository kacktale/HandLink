using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    public Transform hartUI;
    public GameObject hartObj;
    public TextMeshProUGUI bestScoreTxt;
    public TextMeshProUGUI currentScoreText;
    public float bestScore = 0;

    public List<GameObject> harts = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;   
    }
}
