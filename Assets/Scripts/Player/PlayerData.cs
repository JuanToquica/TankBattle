[System.Serializable]
public class PlayerData
{
    public int coins;
    public int selectedTankColorIndex;
    public int armorStrengthLevel;
    public int mainGunDamageLevel;
    public int railgunDamageLevel;
    public int machineGunDamageLevel;
    public int rocketDamageLevel;
    public bool[] colorsPurchased;

    public PlayerData()
    {
        coins = 0;
        selectedTankColorIndex = 0;
        armorStrengthLevel = 0;
        mainGunDamageLevel = 0;
        railgunDamageLevel = 0;
        machineGunDamageLevel = 0;
        rocketDamageLevel = 0;
        colorsPurchased = new bool[5];
        colorsPurchased[0] = true;
        for (int i = 1; i < colorsPurchased.Length; i++)
        {
            colorsPurchased[i] = false;
        }
    }
}
