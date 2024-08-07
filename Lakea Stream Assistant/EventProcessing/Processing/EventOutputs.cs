﻿using Lakea_Stream_Assistant.Enums;
using Lakea_Stream_Assistant.EventProcessing.Battle_Simulator;
using Lakea_Stream_Assistant.EventProcessing.Misc;
using Lakea_Stream_Assistant.Models.Events;
using Lakea_Stream_Assistant.Models.Events.EventItems;
using Lakea_Stream_Assistant.Singletons;
using Lakea_Stream_Assistant.Static;

namespace Lakea_Stream_Assistant.EventProcessing.Processing
{
    //This class handles the outputs that are triggered from events
    public class EventOutputs
    {
        private BattleManager battleManager;
        private EventInput handleEvents;
        private PythonScripts pythonScripts;
        private LakeaCaptured captured;
        private Random random = new Random();

        public EventOutputs(EventInput handleEvents, ConfigSettings settings, LakeaCaptured captured)
        {
            this.handleEvents = handleEvents;
            this.battleManager = new BattleManager(handleEvents, settings.ResourcePath);
            this.pythonScripts = new PythonScripts(settings.PythonExePath, settings.ResourcePath);
            this.captured = captured;
        }

        #region OBS Outputs

        //Set OBS source active status, resets after duration expires if there is a duration
        public void SetActiveOBSSource(Dictionary<string, string> args, int duration, bool active, Callbacks callback, bool invoked = false)
        {
            OBS.SetSourceEnabled(args["Source"], active);
            if (!invoked && duration > 0)
            {
                Task.Delay(duration * 1000).ContinueWith(t => SetActiveOBSSource(args, duration, !active, null, true));
            }
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "Duraction", duration.ToString() },
                    { "Active", active.ToString() }
                };
                foreach (var arg in args)
                {
                    if (!callbackArgs.ContainsKey(arg.Key))
                    {
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Set random OBS source active, resets after duration expires if there is a duration
        public void SetRandomActiveOBSSource(Dictionary<string, string> args, int duration, bool active, Callbacks callback)
        {
            int sourceCount = 0;
            for (int i = 0; i < args.Count; i++)
            {
                if (args.ContainsKey("Source" + i))
                {
                    sourceCount++;
                }
            }
            int ran = random.Next(1, sourceCount + 1);
            string key = "Source" + ran;
            string source = args[key];
            OBS.SetSourceEnabled(source, active);
            if (duration > 0)
            {
                Dictionary<string, string> newArgs = new Dictionary<string, string>();
                newArgs.Add("Source", args[key]);
                Task.Delay(duration * 1000).ContinueWith(t => { SetActiveOBSSource(newArgs, duration, !active, null, true); });
            }
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "SourceName", source },
                    { "SourceNumber", ran.ToString() },
                    { "Duration", duration.ToString() },
                    { "Active", active.ToString() }
                };
                for (int i = 1; i <= sourceCount; i++)
                {
                    key = "Source" + i;
                    callbackArgs.Add("arg" + i, args[key]);
                }
                foreach (var arg in args)
                {
                    if (!arg.Key.Contains("Source"))
                    {
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Loop through a set of OBS sources
        public void LoopOBSSources(Dictionary<string, string> args, Callbacks callback)
        {
            bool anyActive = false;
            for (int i = 1; i <= args.Count; i++)
            {
                if (args.ContainsKey("Source" + i))
                {
                    bool enabled = OBS.GetSourceEnabled(args["Source" + i]);
                    if (enabled)
                    {
                        anyActive = true;
                        OBS.SetSourceEnabled(args["Source" + i], false);
                        if (args.ContainsKey("Source" + (i + 1)))
                        {
                            OBS.SetSourceEnabled(args["Source" +  (i + 1)], true);
                        }
                        else
                        {
                            OBS.SetSourceEnabled(args["Source1"], true);
                        }
                        break;
                    }
                }
            }
            if (!anyActive)
            {
                OBS.SetSourceEnabled(args["Source1"], true);
            }
        }

        //Changes OBS scene
        public void ChangeOBSScene(Dictionary<string, string> args, Callbacks callback)
        {
            if (args.ContainsKey("Transition"))
            {
                OBS.ChangeScene(args["Scene"], args["Transition"]);
            }
            else
            {
                OBS.ChangeScene(args["Scene"]);
            }
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    if (!callbackArgs.ContainsKey(arg.Key))
                    {
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                }
                createCallback(callbackArgs, callback);
            }
        }

        #endregion

        #region Twitch Outputs

        //Send Twitch chat message
        public void SendTwitchChatMessage(Dictionary<string, string> args, Callbacks callback)
        {
            Twitch.WriteToChat(args["Message"]);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Sends a list of Chat Messages
        public void SendTwitchChatMessageList(Dictionary<string, string> args, Callbacks callback)
        {
            int messageCount = -1;
            foreach(var arg in args)
            {
                if (arg.Key.Contains("Message"))
                {
                    messageCount++;
                }
            }
            var task = Task.Run(() =>
            {
                int count = 0;
                while(count <= messageCount)
                {
                    Twitch.WriteToChat(args["Message" +  count]);
                    count++;
                    Thread.Sleep(1000);
                }
            });
            task.Wait();
            if(callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Send Random Twitch Chat Message
        public void SendTwitchRandomChatMessage(Dictionary<string, string> args, Callbacks callback)
        {
            int messageCount = 0;
            for (int i = 0; i < args.Count; i++)
            {
                if (args.ContainsKey("Message" + i))
                {
                    messageCount++;
                }
            }
            int ran = random.Next(1, messageCount + 1);
            string key = "Message" + ran;
            Twitch.WriteToChat(args[key]);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                callbackArgs.Add("SourceNumber", ran.ToString());
                foreach (var arg in args)
                {
                    if (!callbackArgs.ContainsKey(arg.Key))
                    {
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                    else
                    {
                        callbackArgs.Remove(arg.Key);
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Send Twitch Whisper
        public void SendTwitchWhisperMessage(Dictionary<string, string> args, Callbacks callback)
        {
            Twitch.WriteWhisperToUser(args["DisplayName"], args["Message"]);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
            }
        }

        #endregion

        #region Battle Simulator

        //Function to call the Battle Manager to get a characters information
        public void GetCharacterSheet(Dictionary<string, string> args, Callbacks callback)
        {
            Dictionary<string, string> messageArgs = battleManager.GetCharacterSheet(args["AccountID"], args["DisplayName"]);
            if(messageArgs != null)
            {
                SendTwitchChatMessage(messageArgs, callback);
            }
        }

        //Function to call the Battle Manager to get a characters statistics
        public void GetCharacterStatistics(Dictionary<string, string> args, Callbacks callback)
        {
            Dictionary<string, string> messageArgs = battleManager.GetCharacterStatistics(args["AccountID"], args["DisplayName"]);
            if(messageArgs != null)
            {
                SendTwitchChatMessage(messageArgs, callback);
            }
        }

        //Function to call the Battle Manager to train a character
        public void OtherBattleSimEvent(Dictionary<string, string> args, Callbacks callback)
        {
            battleManager.Other(args["Type"], args["AccountID"], args["DisplayName"]);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Function to call the Battle Manager for a monster encounter
        public void Battle(Dictionary<string, string> args, Callbacks callback)
        {
            List<string> battleArgs = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                if (args.ContainsKey("Arg" + i))
                {
                    battleArgs.Add(args["Arg" + i]);
                }
            }
            battleManager.Battle(args["Type"], args["AccountID"], args["DisplayName"], battleArgs);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        #endregion

        #region Lakea Captures

        //Lakea has been captured
        public void CaptureLakea(Dictionary<string, string> args, Callbacks callback)
        {
            int captureDuration = int.Parse(args["Duration"]);
            captured.LakeaCaught(this, captureDuration);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        #endregion

        #region Miscellaneous

        //Call Python object to run a python script
        public void RunPythonScript(Dictionary<string, string> args, Callbacks callback)
        {
            pythonScripts.RunPythonScript(args);
            if (callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //For null events that don't require any actions
        public void NullEvent(string message)
        {
            Terminal.Output("Lakea: " + message);
            Logs.Instance.NewLog(LogLevel.Info, message);
        }

        #endregion

        //Creates a callback object with the passed arguments and reruns the New Event function
        private void createCallback(Dictionary<string, string> args, Callbacks callback)
        {
            if (callback.Delay > 0)
            {
                Task.Delay(callback.Delay * 1000).ContinueWith(t =>
                {
                    handleEvents.NewEvent(new LakeaCallback(EventSource.Lakea, EventType.Lakea_Callback, callback, args));
                });
            }
            else
            {
                handleEvents.NewEvent(new LakeaCallback(EventSource.Lakea, EventType.Lakea_Callback, callback, args));
            }
        }
    }
}
