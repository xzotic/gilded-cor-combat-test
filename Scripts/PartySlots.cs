[System.Serializable]
public class PartySlot
{
    public BattleCharacter occupant;

    public bool IsEmpty => occupant == null;
    //public bool IsAlive => occupant != null && occupant.IsAlive;
}