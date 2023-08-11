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
        public void SetActiveOBSSource(IDictionary<string, string> args, int duration, bool active, Callbacks callback, bool invoked = false)
        {
            Singletons.OBS.SetSourceEnabled(args["Source"], active);
            if (!invoked && duration > 0)
            {
                Task.Delay(duration * 1000).ContinueWith(t => SetActiveOBSSource(args, duration, !active, null, true));
            }
            if (callback != null)
            {
                IDictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "source", args["Source"] },
                    { "duraction", duration.ToString() },
                    { "active", active.ToString() }
                };
                createCallback(callbackArgs, callback);
            }
        }

        //Set random OBS source active, resets after duration expires if there is a duration
        public void SetRandomActiveOBSSource(IDictionary<string, string> args, int duration, bool active, Callbacks callback)
        {
            int ran = random.Next(1, args.Count + 1);
            string key = "Source" + ran;
            string source = args[key];
            Singletons.OBS.SetSourceEnabled(source, active);
            if (duration > 0)
            {
                IDictionary<string, string> newArgs = new Dictionary<string, string>();
                newArgs.Add("Source", args[key]);
                Task.Delay(duration * 1000).ContinueWith(t => { SetActiveOBSSource(newArgs, duration, !active, null, true); });
            }
            if (callback != null)
            {
                IDictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "sourceName", source },
                    { "sourceNumber", ran.ToString() },
                    { "duration", duration.ToString() },
                    { "active", active.ToString() }
                };
                for (int i = 1; i < args.Count + 1; i++)
                {
                    key = "Source" + i;
                    callbackArgs.Add("arg" + i, args[key]);
                }
                createCallback(callbackArgs, callback);
            }
        }

        //Changes OBS scene
        public void ChangeOBSScene(IDictionary<string, string> args, Callbacks callback)
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
                IDictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "scene", args["Scene"] },
                };
                createCallback(callbackArgs, callback);
            }
        }

        #endregion

        #region Twitch Outputs

        public void SendTwitchChatMessage(IDictionary<string, string> args, Callbacks callback)
        {
            Singletons.Twitch.WriteToChat(args["Message"]);
            if (callback != null)
            {
                IDictionary<string, string> callbackArgs = new Dictionary<string, string>
                {
                    { "message", args["Message"] }
                };
                createCallback(callbackArgs, callback);
            }
        }

        #endregion

        //For null events that don't require any actions
        public void NullEvent(string message)
        {
            Console.WriteLine("Lakea: " + message);
            Logs.Instance.NewLog(LogLevel.Info, "Null Event -> ");
        }

        //Creates a callback object with the passed arguments and reruns the New Event function
        private void createCallback(IDictionary<string, string> args, Callbacks callback)
        {
            if(callback.Delay > 0)
            {
                handleEvents.NewEvent(new LakeaCallback(EventSource.Lakea, EventType.Lakea_Callback, callback, args));
            }
            else
            {
                Task.Delay(callback.Delay * 1000).ContinueWith(t =>
                {
                    handleEvents.NewEvent(new LakeaCallback(EventSource.Lakea, EventType.Lakea_Callback, callback, args));
                });
            }
        }
    }
}