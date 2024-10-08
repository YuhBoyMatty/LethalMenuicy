﻿using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Steamworks;

namespace LethalMenu.Menu.Core
{
    internal class MenuFragment
    {
        public void GetImage(string url, Action<Texture2D> Action)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            AsyncOperation operation = www.SendWebRequest();
            operation.completed += (op) =>
            {
                if (www.result != UnityWebRequest.Result.Success) return;
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                Action?.Invoke(texture);
            };
        }

        public static void CheckForMessage()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://icyrelic.com/release/lethalmenu/message.json");
            www.SendWebRequest().completed += (op) =>
            {
                if (www.result != UnityWebRequest.Result.Success || HUDManager.Instance == null) return;
                JObject json = JObject.Parse(www.downloadHandler.text);
                void DisplayMessage(string key)
                {
                    if (json[key] != null && json[key]["Show"].Value<bool>()) 
                        HUDManager.Instance.DisplayTip("Lethal Menu", json[key]["Message"].Value<string>());
                }
                if (json["global"]["Show"].Value<bool>())
                {
                    DisplayMessage("global");
                    System.Threading.Tasks.Task.Delay(8000).ContinueWith(_ => DisplayMessage(Settings.version ?? "default"));
                }
                else DisplayMessage(Settings.version ?? "default");
            };
        }

        public static async void InjectNotification() => await InjectMessage();

        public static async Task InjectMessage()
        {
            if (HUDManager.Instance == null) return;
            if (Loader.Load == null) HUDManager.Instance.DisplayTip("Lethal Menu", "Lethal Menu injected!");
            if (Loader.Load != null) HUDManager.Instance.DisplayTip("Lethal Menu", "Lethal Menu is already injected!");
            await Task.Delay(10000);
            CheckForMessage();
        }
    }
}