﻿using System;
using System.Collections;
using System.Collections.Generic;
using GRTools.Localization;
using GRTools.Utils;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSample : MonoBehaviour
{
    [SerializeField] private Text languageList;
    
    private int index = 0;

    private void Awake()
    {
        LocalizationManager.Init(true, SystemLanguage.ChineseSimplified, "", LocalizationFileType.Txt);
        index = LocalizationManager.Singleton.CurrentLanguageIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateList();
    }

    public void ChangeLanguage()
    {
        index++;
        index = index >= LocalizationManager.Singleton.FileList.Length ? 0 : index;
        LocalizationManager.Singleton.ChangeToLanguage(index);
        UpdateList();

    }

    public void ChangeToCsv()
    {
        LocalizationManager.Singleton.LoadAllLocalizationFilesData("Csv");
    }
    
    public void ChangeToJson()
    {
        LocalizationManager.Singleton.LoadAllLocalizationFilesData("Json", LocalizationFileType.Json);
    }

    private void UpdateList()
    {
        var str = "";
        for (int i = 0; i < LocalizationManager.Singleton.FileList.Length; i++)
        {
            LocalizationFile file = LocalizationManager.Singleton.FileList[i];
            if (LocalizationManager.Singleton.CurrentLanguageType == file.Type)
            {
                str += "<color=#FF0000FF>" + file.FileName + "</color>";
            }
            else
            {
                str += file.FileName;
            }
            str += "\n";
        }

        languageList.text = str;
    }
}