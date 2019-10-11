using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(EBonusTypeBoolDictionary))]
[CustomPropertyDrawer(typeof(EBonusTypeBonusSpriteDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}