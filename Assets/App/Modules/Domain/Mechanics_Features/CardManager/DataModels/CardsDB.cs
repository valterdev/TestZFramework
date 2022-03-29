using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CardsBD", menuName = "ZFramework Assets (SO)/Cards DB")]
public class CardsDB : ScriptableObject
{
    public CardInfo[] Cards;

    [Serializable]
    public class CardInfo
    {
        public int ID;
        public Texture2D Img;

        public string NameKey;
        public string DescrKey;

        public CardEffect[] Effects;
    }

    [Serializable]
    public class CardEffect
    {
        public string Key;
        public float Value;
    }
}
