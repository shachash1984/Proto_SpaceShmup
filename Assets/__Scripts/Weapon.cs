using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum for various possible weapon types
//includes "shield type" to allow a shield power-up
//items marked [NI] below ar Not Implemented in this book
public enum WeaponType
{
    NONE,
    BLASTER,
    SPREAD,
    PHASER,
    MISSILE,
    LASER,
    SHIELD
}
[System.Serializable]
public class WeaponDefinition {

    public WeaponType type = WeaponType.NONE;
    public string letter; // the letter on the power-up
    public Color color = Color.white; //color of collar and Power-up
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float continuosDamage = 0;
    public float delayBetweenShots = 0;
    public float velocity = 20;

    //Note: Weapon prefabs, colors, etc. are set in Main

}

public class Weapon : MonoBehaviour {
    static public Transform PROJECTLE_ANCHOR;
    public bool ___________________________;
    [SerializeField]
    private WeaponType _type = WeaponType.BLASTER;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShot; // time last shot was fired

    void Awake()
    {
        collar = transform.Find("Collar").gameObject;
    }

    void Start()
    {        
        SetType(_type);
        if(PROJECTLE_ANCHOR == null)
        {
            GameObject go = new GameObject("_Projectile_Anchor");
            PROJECTLE_ANCHOR = go.transform;
        }
        GameObject parentGO = transform.parent.gameObject;
        if (parentGO.tag == "Hero")
            Hero.S.fireDelegate += Fire;
    }

    public WeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.NONE)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
            this.gameObject.SetActive(true);

        def = Main.GetWeaponDefinition(_type);
        collar.GetComponent<Renderer>().material.color = def.color;
        lastShot = 0; // you can always fire immdiately after _type is set
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (Time.time - lastShot < def.delayBetweenShots)
            return;

        Projectile p;
        switch (type)
        {
            case WeaponType.BLASTER:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                break;

            case WeaponType.SPREAD:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(-0.2f, 0.9f, 0) * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(0.2f, 0.9f, 0) * def.velocity;
                break;
        }        
    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate(def.projectilePrefab) as GameObject;
        if (transform.parent.gameObject.tag == "Hero")
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.parent = PROJECTLE_ANCHOR;
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShot = Time.time;
        return p;
    }
}
