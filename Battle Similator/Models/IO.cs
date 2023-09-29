﻿using Battle_Similator.Models.Creatures;
using Battle_Similator.Models.Encounters;

namespace Battle_Similator.Models
{
    public class IO
    {
        string path;

        public IO(string config)
        {
            if(config == "LAKEA")
            {
                path = Environment.CurrentDirectory + "\\Applications\\Battle Simulator\\Creatures\\";
            }
            else if(config == "DEBUG")
            {
                path = Environment.CurrentDirectory + "\\Creatures\\";
            }
            else
            {
                Environment.Exit((int)ExitCode.Invalid_Args);
            }
        }

        public void SaveCharacterData(Character character)
        {
            try
            {
                string filePath = path + "Characters\\" + character.ID + ".txt";
                string characterString = "NAME:" + character.Name + "\nID:" + character.ID + "\nLEVEL:" + character.Level + "\nXP:" + character.XP + "\nHP:" + character.HPMax + 
                    "\nSTR:" + character.Strength + "\nDEX:" + character.Dexterity + "\nCON:" + character.Constitution;
                File.WriteAllText(filePath, characterString);
            }
            catch (Exception)
            {
                Environment.Exit((int)ExitCode.IO_Save_Error);
            }
        }

        public Character LoadCharacterData(string id, string name)
        {
            try
            {
                string filePath = path + "Characters\\" + id + ".txt";
                if(File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    Dictionary<string, string> props = new Dictionary<string, string>();
                    foreach(string line in lines)
                    {
                        string[] parts = line.Split(":");
                        switch(parts[0])
                        {
                            case "NAME":
                                props.Add("NAME", parts[1]);
                                break;
                            case "ID":
                                props.Add("ID", parts[1]);
                                break;
                            case "LEVEL":
                                props.Add("LEVEL", parts[1]);
                                break;
                            case "XP":
                                props.Add("XP", parts[1]);
                                break;
                            case "HP":
                                props.Add("HP", parts[1]);
                                break;
                            case "STR":
                                props.Add("STR", parts[1]);
                                break;
                            case "DEX":
                                props.Add("DEX", parts[1]);
                                break;
                            case "CON":
                                props.Add("CON", parts[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    return new Character(props["NAME"], props["ID"], Int32.Parse(props["XP"]), Int32.Parse(props["LEVEL"]), Int32.Parse(props["HP"]), Int32.Parse(props["STR"]),
                        Int32.Parse(props["DEX"]), Int32.Parse(props["CON"]));
                }
                else
                {
                    return new Character(name, id);
                }
            }
            catch(Exception)
            {
                Environment.Exit((int)ExitCode.IO_Load_Error);
                return null;
            }
        }

        public string[] LoadMonstersByStrength(string strength)
        {
            try
            {
                string filepath = path + "Monsters\\" + strength + "MONSTERS.txt";
                return File.ReadAllLines(filepath);
            }
            catch(Exception)
            {
                Environment.Exit((int)ExitCode.IO_Load_Error);
                return null;
            }
        }

        public Monster LoadMonsterData(string id)
        {
            try
            {
                string filePath = path + "Monsters\\" + id + ".txt";
                string[] lines = File.ReadAllLines(filePath);
                Dictionary<string, string> props = new Dictionary<string, string>();
                foreach (string line in lines)
                {
                    string[] parts = line.Split(":");
                    switch(parts[0])
                    {
                        case "NAME":
                            props.Add("NAME", parts[1]);
                            break;
                        case "ID":
                            props.Add("ID", parts[1]);
                            break;
                        case "LEVEL":
                            props.Add("LEVEL", parts[1]);
                            break;
                        case "HP":
                            props.Add("HP", parts[1]);
                            break;
                        case "STR":
                            props.Add("STR", parts[1]);
                            break;
                        case "DEX":
                            props.Add("DEX", parts[1]);
                            break;
                        case "CON":
                            props.Add("CON", parts[1]);
                            break;
                        default:
                            break;
                    }
                }
                return new Monster(props["NAME"], props["ID"], Int32.Parse(props["LEVEL"]), Int32.Parse(props["HP"]), Int32.Parse(props["STR"]), Int32.Parse(props["DEX"]),
                    Int32.Parse(props["CON"]));
            }
            catch (Exception)
            {
                Environment.Exit((int)ExitCode.IO_Load_Error);
                return null;
            }
        }

        public void SaveBattleResultData(EncounterResult result)
        {
            string filePath = path + "..\\BATTLERESULT.txt";
            string resultString = "ENCOUNTER_TYPE:" + result.EncounterType + "\nCHARACTER_NAME:" + result.Character.Name + "\nCHARACTER_ID:" + result.Character.ID +
                "\nMONSTER_NAME:" + result.Monster.Name + "\nMONSTER_ID:" + result.Monster.ID + "\nWINNER:" + result.Winner + "\nXP_GAINED:" + result.XPGained + 
                "\nLEVEL_UP:" + result.LevelUp.ToString().ToUpper() + "\nCHARACTER_LEVEL:" + result.Character.Level;
            File.WriteAllText(filePath, resultString);
        }

        public void SaveTrainingResultData(Character character, int xpGained, bool levelUp)
        {
            string filePath = path + "..\\TRAININGRESULT.txt";
            string resultString = "CHARACTER_NAME:" + character.Name + "\nCHARACTER_ID:" + character.ID + "\nXP_GAINED:" + xpGained + "\nLEVEL_UP:" + 
                levelUp.ToString().ToUpper() + "\nCHARACTER_LEVEL:" + character.Level;
            File.WriteAllText (filePath, resultString);
        }
    }
}
