﻿using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GRTools.Localization
{
    public class LocalizationResourcesLoader : LocalizationLoader
    {
        public override void LoadManifestAsync(Action<LocalizationInfo[]> completed)
        {
            if (completed != null)
            {
                var request = Resources.LoadAsync<LocalizationManifest>(Path.Combine(RootPath, ManifestPath));
                request.completed += operation =>
                {
                    if (operation.isDone)
                    {
                        LocalizationInfo[] infoList = (request.asset as LocalizationManifest)?.InfoList;
                        if (infoList != null)
                        {
                            LocalizationInfo[] newInfoList = new LocalizationInfo[infoList.Length];
                            for (int i = 0; i < infoList.Length; i++)
                            {
                                newInfoList[i] = infoList[i];
                            }
                            completed(newInfoList);
                        }
                        else
                        {
                            completed(new LocalizationInfo[0]);
                        }
                        Resources.UnloadAsset(request.asset);
                    }
                };
            }
        }

        public override void LoadLocalizationTextAsset(LocalizationInfo info, Action<Object> completed)
        {
            LoadAssetAsync(info, info.TextAssetPath, false, completed);
        }

        public override void LoadAssetAsync<TAsset>(LocalizationInfo info, string assetName, bool defaultAsset, Action<TAsset> completed)
        {
            if (!string.IsNullOrEmpty(assetName) && completed != null)
            {
                var request = Resources.LoadAsync<TAsset>(Path.Combine(RootPath, info.AssetsPath, assetName));
                request.completed += operation =>
                {
                    if (operation.isDone)
                    {
                        completed(request.asset as TAsset);
                    }
                };
            }
        }
    }
}
