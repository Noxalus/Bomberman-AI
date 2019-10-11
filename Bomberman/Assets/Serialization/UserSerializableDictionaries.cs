using System;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class EBonusTypeBoolDictionary : SerializableDictionary<EBonusType, bool> { }

[Serializable]
public class EBonusTypeBonusSpriteDictionary : SerializableDictionary<EBonusType, BonusSprites> { }