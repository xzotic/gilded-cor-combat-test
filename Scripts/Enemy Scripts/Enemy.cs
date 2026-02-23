using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public string enemyName;
    public int id;
    public int maxHP;
    public int currentHP;
    public bool isBoss;

    [Header("Sorcery Resistances")]
    public float sorceryRes;
    public float flameRes;
    public float fleshRes;
    public float lightRes;
    public float fateRes;
    public float plagueRes;
    public float galeRes;
    public float natureRes;
    public float deathRes;
    public float starRes;
    public float darkRes;

    [Header("Physical Resistances")]
    public float physRes;
    public float severRes;
    public float impactRes;
    public float punctureRes;

    private void Start()
    {
        currentHP = maxHP;
    }
}
