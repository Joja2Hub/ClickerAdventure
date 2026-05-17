using System;

[Serializable]
public class PlayerStatsSaveData
{
    public int level = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;
    public int money = 100;
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentDmg = 1;
}
