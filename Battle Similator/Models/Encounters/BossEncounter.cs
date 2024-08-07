﻿using Battle_Similator.Models.Creatures;
using Battle_Similator.Models.Resources;

namespace Battle_Similator.Models.Encounters
{
    public class BossEncounter
    {
        private IO io;
        private HealthBarImage healthBar;
        private string[] bossList;

        public BossEncounter(IO io, string config, string resourcePath)
        {
            this.io = io;
            healthBar = new HealthBarImage(io, config, resourcePath);
            this.bossList = io.LoadBossList();
        }

        public void Start(string characterID, string characterName, string subTier)
        {
            Encounter encounter = preEncounter(characterID, characterName, subTier);
            EncounterResult result = encounter.Run();
            postEncounter(result);
        }

        private Encounter preEncounter(string characterID, string characterName, string subTier)
        {
            Character character = io.LoadCharacterData(characterID, characterName);
            character.SetTwitchSubTier(subTier);
            character.ResetOnDeath = false;
            Monster monster;
            if (io.CurrentBossFileExists())
            {
                monster = io.LoadNPCData(CreatureType.Boss, "CURRENTBOSS");
            }
            else
            {
                monster = io.LoadNPCData(CreatureType.Boss, bossList[0]);
            }
            return new Encounter(character, monster, "BOSSBATTLE");
        }

        private void postEncounter(EncounterResult result)
        {
            string bossBeaten = "\nBOSS_BEATEN:";
            string allBossesBeaten = "\nALL_BOSSES_BEATEN:";
            result.Character.NewBossFought();
            if(result.Winner.Equals(result.Character.ID))
            {
                bossBeaten += "TRUE";
                string finalBoss = bossList[bossList.Length - 1];
                string currentBoss = result.Monster.ID + "-" + result.Monster.Name;
                if(finalBoss.Equals(currentBoss))
                {
                    allBossesBeaten += "TRUE";
                }
                else
                {
                    allBossesBeaten += "FALSE";
                    string thisBoss = result.Monster.ID + "-" + result.Monster.Name;
                    string nextBossString = "";
                    int index = 0;
                    foreach (string boss in bossList)
                    {
                        if (thisBoss.Equals(boss))
                        {
                            nextBossString = bossList[index + 1];
                        }
                        else
                        {
                            index++;
                        }
                    }
                    Monster nextBoss = io.LoadNPCData(CreatureType.Boss, nextBossString);
                    io.SaveCurrentBossData(nextBoss);
                }
                io.SaveCharacterData(result.Character);
                distributeXP(result.Character.ID, result.Monster.XPValue);
            }
            else
            {
                bossBeaten += "FALSE";
                allBossesBeaten += "FALSE";
                io.SaveCharacterData(result.Character);
                io.SaveCurrentBossData(result.Monster);
                io.AppendCurrentBossFighters(result.Character.ID);
            }
            string resultsString = "ENCOUNTER_TYPE:" + result.EncounterType + "\nCHARACTER_NAME:" + result.Character.Name + "\nCHARACTER_ID:" + 
                result.Character.ID + "\nMONSTER_NAME:" + result.Monster.Name + "\nMONSTER_ID:" + result.Monster.ID + "\nWINNER:" + result.Winner + "\nXP_GAINED:" +
                result.XPGained + "\nLEVEL_UP:" + result.LevelUp.ToString().ToUpper() + "\nCHARACTER_LEVEL:" + result.Character.Level + bossBeaten + allBossesBeaten;
            io.SaveResultData(resultsString);
            healthBar.GenerateHealthBarImage(result.Monster);
        }

        private void distributeXP(string id, int totalXP)
        {
            string[] fightersArray = io.LoadCurrentBossFighters();
            List<string> fighters = new List<string> { id };
            foreach(string fighter in fightersArray)
            {
                if (!fighters.Contains(fighter))
                {
                    fighters.Add(fighter);
                }
            }
            int xpGain = (int)totalXP / fighters.Count;
            if(xpGain % 5 != 0)
            {
                int remainder = xpGain % 5;
                xpGain -= remainder;
            }
            foreach(string fighter in fighters)
            {
                Character character = io.LoadCharacterData(fighter);
                character.NewBossBeaten();
                int characterXPGain = character.ApplySubBonusXP(xpGain);
                character.IncreaseXP(characterXPGain);
                io.SaveCharacterData(character);
            }
            io.DeleteCurrentBossFighters();
        }
    }
}
