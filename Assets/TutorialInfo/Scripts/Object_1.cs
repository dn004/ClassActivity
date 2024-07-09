using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Object_1 : MonoBehaviour
{


    [SerializeField] private Readme mySO;
    [SerializeField] private RawImage myImg;
    [SerializeField] private TMP_Text titleText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        myImg.texture = mySO.icon;
        titleText.text = mySO.title;
    }
}
