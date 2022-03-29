using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInv
{
    public int ID;
    public CardsDB.CardEffect[] Effects;

    public CardInv(int id, CardsDB.CardEffect[] effects)
    {
        ID = id;
        Effects = effects;
    }
}
