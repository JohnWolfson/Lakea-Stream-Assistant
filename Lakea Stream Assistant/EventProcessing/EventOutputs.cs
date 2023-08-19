﻿using Lakea_Stream_Assistant.Enums;
using Lakea_Stream_Assistant.Models.Events;
using Lakea_Stream_Assistant.Models.Events.EventItems;
using Lakea_Stream_Assistant.Singletons;

namespace Lakea_Stream_Assistant.EventProcessing
{
    //This class handles the outputs that are triggered from events
    public class EventOutputs
    {
        private EventInput handleEvents;
        private Random random = new Random();

        public EventOutputs(EventInput handleEvents)
        {
            this.handleEvents = handleEvents;
        }

        #region OBS Outputs

        //Set OBS source active status, resets after duration expires if there is a duration
        public void SetActiveOBSSource(Dictionary<string, string> args, int duration, bool active, Callbacks callback, bool invoked = false)
        {
            Singletons.OBS.SetSourceEnabled(args["Source"], active);
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
                foreach(var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Set random OBS source active, resets after duration expires if there is a duration
        public void SetRandomActiveOBSSource(Dictionary<string, string> args, int duration, bool active, Callbacks callback)
        {
            int sourceCount = 0;
            for(int i = 0; i < args.Count; i++)
            {
                if(args.ContainsKey("Source" + i))
                {
                    sourceCount++;
                }
            }
            int ran = random.Next(1, sourceCount + 1);
            string key = "Source" + ran;
            string source = args[key];
            Singletons.OBS.SetSourceEnabled(source, active);
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
                foreach(var arg in args)
                {
                    if (!arg.Key.Contains("Source"))
                    {
                        callbackArgs.Add(arg.Key, arg.Value);
                    }
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Changes OBS scene
        public void ChangeOBSScene(Dictionary<string, string> args, Callbacks callback)
        {
            if (args.ContainsKey("Transition"))
            {
                Singletons.OBS.ChangeScene(args["Scene"], args["Transition"]);
            }
            else
            {
                Singletons.OBS.ChangeScene(args["Scene"]);
            }
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

        #region Twitch Outputs

        //Send Twitch chat message
        public void SendTwitchChatMessage(Dictionary<string, string> args, Callbacks callback)
        {
            Singletons.Twitch.WriteToChat(args["Message"]);
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

        //Send Twitch Whisper
        public void SendTwitchWhisperMessage(Dictionary<string, string> args, Callbacks callback)
        {
            Singletons.Twitch.WriteWhisperToUser(args["DisplayName"], args["Message"]);
            if(callback != null)
            {
                Dictionary<string, string> callbackArgs = new Dictionary<string, string>();
                foreach (var arg in args)
                {
                    callbackArgs.Add(arg.Key, arg.Value);
                }
            }
        }

        #endregion

        //For null events that don't require any actions
        public void NullEvent(string message)
        {
            Console.WriteLine("Lakea: " + message);
            Logs.Instance.NewLog(LogLevel.Info, "Null Event Output -> ");
        }

        //Creates a callback object with the passed arguments and reruns the New Event function
        private void createCallback(Dictionary<string, string> args, Callbacks callback)
        {
            if(callback.Delay > 0)
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
