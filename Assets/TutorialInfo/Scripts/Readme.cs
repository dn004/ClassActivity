using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;	


[CreateAssetMenu(fileName = "ScriptableObject" , menuName = "SO")]

public class Readme : ScriptableObject
{
    public Texture icon;
    public string title;
    public Section[] sections;
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }
}
