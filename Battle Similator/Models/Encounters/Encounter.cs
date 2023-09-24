﻿using Battle_Similator.Models.Creatures;

namespace Battle_Similator.Models.Encounters
{
    public class Encounter
    {
        private Character character;
        private Monster monster;
        private Random random;

        public Encounter(Character character, Monster monster)
        {
            this.character = character;
            this.monster = monster;
            random = new Random();
        }

        public EncounterResult Run()
        {
            while (monster.IsAlive && character.IsAlive)
            {
                int characterRoll = random.Next(1, 21);
                int monsterRoll = random.Next(1, 21);
                if (characterRoll == 20 && monsterRoll != 20)
                {
                    criticalHit(character, monster);
                }
                else if (monsterRoll == 20 && characterRoll != 20)
                {
                    criticalHit(monster, character);
                }
                else
                {
                    characterRoll += character.DexterityMod;
                    monsterRoll += monster.DexterityMod;
                    if (characterRoll > monsterRoll)
                    {
                        hit(character, monster);
                    }
                    else if (characterRoll < monsterRoll)
                    {
                        hit(monster, character);
                    }
                }
            }
            if(character.IsAlive)
            {
                character.IncreaseXP(monster.XPValue);
                return new EncounterResult(character, monster, character.ID, monster.XPValue);
            }
            else
            {
                return new EncounterResult(character, monster, monster.Name, 0);
            }
        }

        private void criticalHit(Creature attacker, Creature target)
        {
            int damage = 0;
            for (int count = 0; count < 3; count++)
            {
                damage += random.Next(1, attacker.StrengthMod);
            }
            damage += (attacker.Level - target.Level) / 3;
            if (damage < 1)
            {
                damage = 1;
            }
            target.TakeDamage(damage);
        }

        private void hit(Creature attacker, Creature target)
        {
            int damage = random.Next(1, attacker.StrengthMod) + (attacker.Level - target.Level) / 3;
            if (damage < 1)
            {
                damage = 1;
            }
            target.TakeDamage(damage);
        }
    }
}