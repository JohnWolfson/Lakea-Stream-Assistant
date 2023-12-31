﻿using Lakea_Stream_Assistant.Enums;
using Lakea_Stream_Assistant.Models.Events.EventLists;
using Lakea_Stream_Assistant.Singletons;
using Lakea_Stream_Assistant.Static;

namespace Lakea_Stream_Assistant.EventProcessing.Processing
{
    //handles stored 'EventItem' and calls the corresponding 'EventOutputs' function
    public class EventProcesser
    {
        private EventOutputs outputs;

        public EventProcesser(EventOutputs outputs)
        {
            this.outputs = outputs;
        }

        // Receives 'EventItem' object and calls the corresponding 'EventOutputs' function with the relevant arguments
        public void ProcessEvent(EventItem item)
        {
            try
            {
                if(item != null)
                {
                    switch (item.EventGoal)
                    {
                        case EventGoal.Null:
                            outputs.NullEvent("Null Event -> " + item.Name);
                            break;
                        case EventGoal.Battle_Simulator_Character_Sheet:
                            outputs.GetCharacterSheet(item.Args, item.Callback);
                            break;
                        case EventGoal.Battle_Simulator_Encounter:
                            outputs.Battle(item.Args, item.Callback);
                            break;
                        case EventGoal.Battle_Simulator_Nonencounter:
                            outputs.OtherBattleSimEvent(item.Args, item.Callback);
                            break;
                        case EventGoal.OBS_Disable_Source:
                            outputs.SetActiveOBSSource(item.Args, item.Duration, false, item.Callback);
                            break;
                        case EventGoal.OBS_Enable_Source:
                            outputs.SetActiveOBSSource(item.Args, item.Duration, true, item.Callback);
                            break;
                        case EventGoal.OBS_Enable_Random_Source:
                            outputs.SetRandomActiveOBSSource(item.Args, item.Duration, true, item.Callback);
                            break;
                        case EventGoal.OBS_Disable_Random_Source:
                            outputs.SetRandomActiveOBSSource(item.Args, item.Duration, false, item.Callback);
                            break;
                        case EventGoal.OBS_Loop_Sources:
                            outputs.LoopOBSSources(item.Args, item.Callback);
                            break;
                        case EventGoal.OBS_Change_Scene:
                            outputs.ChangeOBSScene(item.Args, item.Callback);
                            break;
                        case EventGoal.Python_Run_Script:
                            outputs.RunPythonScript(item.Args, item.Callback);
                            break;
                        case EventGoal.Twitch_Send_Chat_Message:
                            outputs.SendTwitchChatMessage(item.Args, item.Callback);
                            break;
                        case EventGoal.Twitch_Send_Chat_Message_List:
                            outputs.SendTwitchChatMessageList(item.Args, item.Callback);
                            break;
                        case EventGoal.Twitch_Send_Random_Chat_Message:
                            outputs.SendTwitchRandomChatMessage(item.Args, item.Callback);
                            break;
                        case EventGoal.Twitch_Send_Whisper_Message:
                            outputs.SendTwitchWhisperMessage(item.Args, item.Callback);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Terminal.Output("Lakea: Process Event Error -> " + ex.Message);
                Logs.Instance.NewLog(LogLevel.Error, ex);
            }
        }
    }
}
