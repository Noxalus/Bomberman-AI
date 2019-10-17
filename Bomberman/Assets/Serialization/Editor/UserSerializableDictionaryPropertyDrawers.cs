using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(EBonusTypeBoolDictionary))]
[CustomPropertyDrawer(typeof(EBonusTypeBonusSpriteDictionary))]
[CustomPropertyDrawer(typeof(EEntityTypeSpriteDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}