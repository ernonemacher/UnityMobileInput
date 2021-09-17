// ----------------------------------------------------------------------------
// The MIT License
// UnityMobileInput https://github.com/mopsicus/UnityMobileInput
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using NiceJson;
using Firebase.Crashlytics;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Mopsicus.Plugins
{

    /// <summary>
    /// Mobile plugin interface
    /// Each plugin must implement it
    /// </summary>
    public interface IPlugin
    {

        /// <summary>
        /// Plaugin name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Callback on get data
        /// </summary>
        void OnData(JsonObject data);

        /// <summary>
        /// Callback on get error
        /// </summary>
        void OnError(JsonObject data);
    }

    /// <summary>
    /// Plugin service to manager all mobile plugins
    /// </summary>
	public class Plugins : MonoBehaviour
    {

#if UNITY_ANDROID
        /// <summary>
        /// Mask for Java classes
        /// </summary>
        public const string ANDROID_CLASS_MASK = "ru.mopsicus.{0}.Plugin";
#elif UNITY_IOS
        /// <summary>
        /// Init iOS plugins
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void pluginsInit (string data);
#endif		

        /// <summary>
        /// Gameobject name on scene to receive data
        /// ACHTUNG! Do not change it
        /// </summary>
        const string _dataObject = "Plugins";

        /// <summary>
        /// Function name to receive data
        /// ACHTUNG! Do not change it
        /// </summary>
        const string _dataReceiver = "OnDataReceive";

        private void Awake()
        {
            name = _dataObject;
            DontDestroyOnLoad(gameObject);
            InitPlugins();
        }

        /// <summary>
        /// Init all plugins in app
        /// </summary>
        void InitPlugins()
        {		
            JsonObject data = new JsonObject();
            data["object"] = _dataObject;
            data["receiver"] = _dataReceiver;
#if UNITY_IOS
            pluginsInit (data.ToJsonString ());
#endif

        }

        /// <summary>
        /// Handler to process data to plugin
        /// </summary>
        /// <param name="data">data from plugin</param>
        void OnDataReceive(string data)
        {
            try
            {
                JsonObject info = (JsonObject)JsonNode.ParseJsonString(data);
                if (MobileInput.Plugin != null)
                {
                    if (info.ContainsKey("error"))
                    {
                        MobileInput.Plugin.OnError(info);
                    }
                    else
                    {
                        MobileInput.Plugin.OnData(info);
                    }
                }
                else
                {
                    Debug.LogError(string.Format("{0} plugin does not exists", info["name"]));
                }
            }
            catch (Exception e)
            {
                Crashlytics.LogException(e);
                Debug.LogError(string.Format("Plugins receive error: {0}, stack: {1}", e.Message, e.StackTrace));
            }

        }

    }

}