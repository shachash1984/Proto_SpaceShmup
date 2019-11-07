using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    public Vector2 rotMinMax = new Vector2(15,90);
    public Vector2 driftMinMax = new Vector2(0.25f, 2);
    public float lifeTime = 6f;
    public float fadeTime = 4f;
    public bool ____________________________;
    public WeaponType type;
    public GameObject cube;
    public TextMesh letter;
    public Vector3 rotPerSecond;
    public float birthTime;

    void Awake()
    {
        cube = transform.Find("Cube").gameObject;
        letter = GetComponent<TextMesh>();
        Vector3 vel = Random.onUnitSphere; // get random XYZ velocity, Random.onUnitySphere returns a vector point on the surface of a sphere with a radius of 1 
        vel.z = 0;
        vel.Normalize(); // making the length of vel 1
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        GetComponent<Rigidbody>().velocity = vel;
        //setting rotation to R:[0,0,0]
        transform.rotation = Quaternion.identity;
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y));
        InvokeRepeating("CheckOffScreen", 2f, 3f);
        birthTime = Time.time;
    }

    void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);
        //fade out over time
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;
        //for lifetime seconds, u will be <= 0. then it will transition to 1 over "fadeTime" seconds
        if(u >= 1)
        {
            Destroy(this.gameObject);
            return;
        }
        //use u to determine the alpha value of the cube & letter
        if (u > 0)
        {
            Color c = cube.GetComponent<Renderer>().material.color;
            c.a = 1f - u;
            cube.GetComponent<Renderer>().material.color = c;

            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
    }
    //a different version of SetType() from the on in Weapon and Projectile
    public void SetType(WeaponType wt)
    {
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        cube.GetComponent<Renderer>().material.color = def.color;
        letter.color = def.color;
        letter.text = def.letter;
        type = wt;
    }

    public void AbsorbedBy(GameObject target)
    {
        //called by the Hero class when a PowerUp is collected
        //we could Tween into the target and shrink in size - maybe later
        Destroy(this.gameObject);
    }

    void CheckOffScreen()
    {
        if (Utils.ScreenBoundsCheck(cube.GetComponent<Collider>().bounds, BoundsTest.OFFSCREEN) != Vector3.zero)
            Destroy(this.gameObject);        
    }
}
