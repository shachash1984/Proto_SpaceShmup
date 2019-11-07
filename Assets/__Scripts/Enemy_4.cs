using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//part is another seralizable data storage class just like WeaponDefinition
[System.Serializable]
public class Part
{
    //defined in the inspector
    public string name; // name of part
    public float health; // health of part
    public string[] protectedBy; // the other parts

    //dynamically defined
    public GameObject go;
    public Material mat; //the material to show damage    
}

public class Enemy_4 : Enemy {
    //Enemy_4 will start offscreen and then pick a random point on screen to move to.
    //once arrived, it will pick another random point and continue until the player shot it down
    public Vector3[] points;
    public Part[] parts;
    public float timeStart;
    public float duration = 4; //duration of movement

    void Start()
    {
        points = new Vector3[2];
        points[0] = pos;
        points[1] = pos;

        InitMovement();
        //Cache GameObject & Material of each Part in parts
        Transform t;
        foreach (Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        //pick a point to move to (on screen)
        Vector3 p1 = Vector3.zero;
        float esp = Main.S.enemySpawnPadding;
        Bounds cBounds = Utils.camBounds;
        p1.x = Random.Range(cBounds.min.x + esp, cBounds.max.x - esp);
        p1.y = Random.Range(cBounds.min.y + esp, cBounds.max.y - esp);

        points[0] = points[1];
        points[1] = p1;

        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;
        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); //Apply Ease Out to u
        pos = (1 - u) * points[0] + u * points[1];
    }

    //This will override the OnCollisionEnter that is part of Enemy.cs because of the way that MonoBehaviour
    //declares common Unity functions like OnCollisionEnter(), the override keyword is not necessary

    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                //Enemies dont take damage unless they're on screen
                bounds.center = transform.position + boundsCenterOffset;
                if(bounds.extents == Vector3.zero || Utils.ScreenBoundsCheck(bounds, BoundsTest.OFFSCREEN) != Vector3.zero)
                {
                    Destroy(other);
                    break;
                }

                //Hurt this Enemy:
                //Find the GameObject that was hit
                //The Collision coll has contacts[], an array of ContactPoints
                //Because there was a collision, wer'e guaranteed that there is at least a contacts[0]
                //and ContactPoints have a reference to thisCollider, which will be the collider for the
                //part of Enemy_4 that was hit
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if(prtHit == null)
                {
                    //happens very rarely, thisCollider on contacts[0] will be the ProjectileHero instead of the ship part
                    //if so, seek otherCollider instead
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }

                //check whether this part is still protected
                if(prtHit.protectedBy != null)
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        //if one of the protecting parts hasnt been destroyed
                        if (!Destroyed(s))
                        {
                            //dont damage this part yet
                            Destroy(other);
                            return;
                        }
                    }
                }

                //It's not protected so make it take damage
                //get the damage amount from the Projectile.type & Main.W_DEFS
                prtHit.health -= Main.W_DEFS[p.type].damageOnHit;
                //Show Damage on the part
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    //instead of destroying this enemy, disable the damaged part
                    prtHit.go.SetActive(false);
                }
                //Check to see if thw whole ship is destroyed
                bool allDestroyed = true;
                foreach (Part prt in parts)
                {
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                if (allDestroyed)
                {
                    //Tell the main singleton that ship has been destroyed
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }        
    }

    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name.Equals(n))
                return prt;
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go.Equals(go))
                return prt;
        }
        return null;
    }
    
    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return (Destroyed(FindPart(n)));
    }

    bool Destroyed(Part prt)
    {
        if (prt == null) //if no real part was passed in
            return true;

        return (prt.health <= 0);
    }

    //changes the color of just one Part to Red instead of the whole ship
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        remainingDamageFrames = showDamageForFrames;
    }


}
