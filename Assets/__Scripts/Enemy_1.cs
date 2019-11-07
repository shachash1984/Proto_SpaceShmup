using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy {

    public float waveFrequency = 2;
    public float waveWidth = 4; // sinus wave width
    public float waveRotY = 45;

    private float x0 = -12345; // initial value of x in pos
    private float birthTime;


	void Start () {
        //Setting x0 to he initial x position of Enemy_1
        //This works because the position had already been set by Main.SpawnEnemy() before Start() runs and there is no Start() on Enemy
        x0 = pos.x;
        birthTime = Time.time;
	}

    public override void Move()
    {
        Vector3 tempPos = pos; // because pos is a property
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency; // theta adjusts based on time
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        //rotating on the Y axis
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);

        //base.Move() still handles the movement down in Y
        base.Move();
    }
}
