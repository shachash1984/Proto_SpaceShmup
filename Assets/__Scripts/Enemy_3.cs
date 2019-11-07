using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_3 : Enemy {

    //Enemy_3 will move following a Bezier curve, which is a linear interpolation between more than two points
    public Vector3[] points;
    public float birthTime;
    public float lifeTime = 10;

    void Start()
    {
        points = new Vector3[3];
        //the start position has already been set by Main.SpawnEnemy()
        points[0] = pos;        

        float xMin = Utils.camBounds.min.x + Main.S.enemySpawnPadding;
        float xMax = Utils.camBounds.max.x - Main.S.enemySpawnPadding;

        Vector3 v;
        //picking a random middle position in the bottom half of the screen
        v = Vector3.zero;
        v.x = Random.Range(xMin, xMax);
        v.y = Random.Range(Utils.camBounds.min.y-50, -10);

        //picking a random final position above the top of the screen
        v = Vector3.zero;
        v.y = pos.y;
        v.x = Random.Range(xMin, xMax);
        points[2] = v;

        birthTime = Time.time;
    }

    public override void Move()
    {
        //bezier curves work based on a value between 0 & 1
        float u = (Time.time - birthTime) / lifeTime;
        if(u > 1)
        {
            Destroy(this.gameObject);
            return;
        }
        //interpolate the 3 Bezier curve points
        Vector3 p01, p12;
        u = u - 0.2f * Mathf.Sin(u * Mathf.PI * 2); //Optional for speeding up movement
        p01 = (1 - u) * points[0] + u * points[1];
        p12 = (1 - u) * points[1] + u * points[2];
        pos = (1 - u) * p01 + u * p12;
    }
}
