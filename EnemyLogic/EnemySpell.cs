using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpell", menuName = "ScriptableObjects/EnemySpellData", order = 1)]
public class EnemySpell : ScriptableObject
{
    [Header("Name of the Spell")]
    public string SpellName;
    [Header("Type of Spell")]
    public string Spell_Type;
    [Header("Components of the Spell")]
    public List<ShapeGroup.SubShape> SpellComponents;
    [Header("Damage of the Spell")]
    public float Damage;
    [Header("Prefab for feedback")]
    public GameObject FeedbackPrefab;
    [Header("SubSpell Sprite")]
    public Sprite SubSepll;
    [Header("Collider Object")]
    public GameObject ColliderObject;

}
