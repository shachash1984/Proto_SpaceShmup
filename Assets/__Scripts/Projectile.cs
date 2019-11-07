using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    [SerializeField]
    private WeaponType _type;
    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    void Awake()
    {
        //test to see whether this has passed off screen every 2 seconds
        InvokeRepeating("CheckOffScreen", 2f, 2f);
    }

    public void SetType(WeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GetWeaponDefinition(_type);
        GetComponent<Renderer>().material.color = def.projectileColor;
    }

    void CheckOffScreen()
    {
        if (Utils.ScreenBoundsCheck(this.GetComponent<Collider>().bounds, BoundsTest.OFFSCREEN) != Vector3.zero)
            Destroy(this.gameObject);
    }
	
}
