﻿using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using Lakea_Stream_Assistant.Models.OBS;
using Lakea_Stream_Assistant.Enums;

namespace Lakea_Stream_Assistant.Singletons
{
    //Singleton that connects to OBS and manages calls via the OBS Web Socket library
    public sealed class OBS
    {
        public static bool Initiliased = false;
        private static OBSResources resources;
        private static OBSWebsocket client;
        private static CancellationTokenSource keepAliveTokenSource;
        private static readonly int keepAliveInterval = 500;

        #region Initiliase

        //Initialises the connected with the passed in configuration data
        public static void Init(Config config)
        {
            try
            {
                Console.WriteLine("OBS: Connecting...");
                Logs.Instance.NewLog(LogLevel.Info, "Connecting to OBS...");
                client = new OBSWebsocket();
                client.Connected += onConnect;
                client.ConnectAsync("ws://" + config.OBS.IP + ":" + config.OBS.Port, config.OBS.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to OBS: " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Fatal, ex);
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        //Once connected, sets the keep alive token to avoid disconnected and calls for resources to be collected
        private static void onConnect(object sender, EventArgs e)
        {
            Console.WriteLine("OBS: Connected");
            Logs.Instance.NewLog(LogLevel.Info, "Connected to OBS");
            keepAliveTokenSource = new CancellationTokenSource();
            CancellationToken keepAliveToken = keepAliveTokenSource.Token;
            Task statPollKeepAlive = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(keepAliveInterval);
                    if (keepAliveToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }, keepAliveToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            getResources();
            Initiliased = true;
        }

        //Once connected, gether source and scene information that can be referred back to later in the 'resources' object
        private static void getResources()
        {
            try
            {
                Console.WriteLine("OBS: Fetching Scenes...");
                Logs.Instance.NewLog(LogLevel.Info, "Fetching OBS Scenes...");
                var scenes = client.ListScenes();
                List<string> sceneList = new List<string>();
                foreach (var scene in scenes)
                {
                    sceneList.Add(scene.Name);
                }
                Console.WriteLine("OBS: Fetching Sources...");
                Logs.Instance.NewLog(LogLevel.Info, "Fetching OBS Sources...");
                IDictionary<string, int> sourceDict = new Dictionary<string, int>();
                foreach (var scene in scenes)
                {
                    var sceneSources = client.GetSceneItemList(scene.Name);
                    foreach (var source in sceneSources)
                    {
                        if (!sourceDict.ContainsKey(source.SourceName))
                        {
                            sourceDict.Add(source.SourceName, source.ItemId);
                        }
                    }
                }
                Console.WriteLine("OBS: Fetching Transitions...");
                Logs.Instance.NewLog(LogLevel.Info, "Fetching OBS Transitions...");
                var sceneTransitions = client.GetSceneTransitionList();
                List<string> transitionNames = new List<string>();
                foreach (var transition in sceneTransitions.Transitions)
                {
                    transitionNames.Add(transition.Name);
                }
                Console.WriteLine("OBS: Initialising Resources...");
                Logs.Instance.NewLog(LogLevel.Info, "Initialising OBS Resources...");
                resources = new OBSResources(sceneList, sourceDict, transitionNames);
                Console.WriteLine("OBS: Initialised OBS Resources");
                Logs.Instance.NewLog(LogLevel.Info, "Initialised OBS Resources");
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Get Resources -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Fatal, ex);
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        #endregion

        //Returns the current active scene
        public static string GetCurrentScene()
        {
            try
            {
                return client.GetCurrentProgramScene();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Get Current Scene -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
            return string.Empty;
        }

        //Changes the scene to the passed in scene
        public static void ChangeScene(string scene)
        {
            try
            {
                Console.WriteLine("OBS: Changing Scene -> " + scene);
                Logs.Instance.NewLog(LogLevel.Info, "Changing OBS Scene -> " + scene);
                client.SetCurrentProgramScene(scene);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Change Scenes -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
        }

        //Changes the scene to the passed in scene with the passesd in transition
        //Todo: Client resets the default transition after 3 seconds, will cause issues for transitions that are longer than
        //3 seconds, should get transition length then reset after that period of time
        public static void ChangeScene(string scene, string transition)
        {
            try
            {
                Console.WriteLine("OBS: Changing Scene with Transition -> Scene - " + scene + ", Transition - " + transition);
                Logs.Instance.NewLog(LogLevel.Info, "Changing OBS Scene with Transition -> Scene - " + scene + ", Transition - " + transition);
                string curTransition = client.GetCurrentSceneTransition().Name;
                client.SetCurrentSceneTransition(transition);
                client.SetCurrentProgramScene(scene);
                Task.Delay(3000).ContinueWith(t => { client.SetCurrentSceneTransition(curTransition); });
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Change Scenes with Transition -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
        }

        //Disables or enables a source with a scene arguement passed in
        public static void SetSourceEnabled(string scene, string source, bool active)
        {
            try
            {
                Console.WriteLine("OBS: Setting Source State '" + active + "' -> '" + source + "' in '" + scene + "'");
                Logs.Instance.NewLog(LogLevel.Info, "Setting OBS Source State '" + active + "' -> '" + source + "' in '" + scene + "'");
                int sourceID = resources.GetSourceId(source);
                client.SetSceneItemEnabled(scene, sourceID, active);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Set Source Enabled -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
        }

        //Disables or enables a source by searching for it in the active scene and any nested scenes
        public static void SetSourceEnabled(string source, bool active)
        {
            try
            {
                string curScene = client.GetCurrentProgramScene();
                Console.WriteLine("OBS: Setting Source State '" + active + "' -> '" + source + "' in '" + curScene + "'");
                Logs.Instance.NewLog(LogLevel.Info, "Setting OBS Source State '" + active + "' -> '" + source + "' in '" + curScene + "'");
                string scene = searchForSource(curScene, source);
                int sourceID = resources.GetSourceId(source);
                client.SetSceneItemEnabled(scene, sourceID, active);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OBS: Failed to Set Source Enabled -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
        }

        //Will search any nested scenes in a scene for a source
        private static string searchForSource(string scene, string source)
        {
            var sources = client.GetSceneItemList(scene);
            foreach (var item in sources)
            {
                if(item.SourceName == source)
                {
                    return scene;
                }
                else if(item.SourceType == SceneItemSourceType.OBS_SOURCE_TYPE_SCENE)
                {
                    string newScene = searchForSource(item.SourceName, source);
                    if(newScene != string.Empty)
                    {
                        return newScene;
                    }
                }
            }
            return string.Empty;
        }
    }
}
