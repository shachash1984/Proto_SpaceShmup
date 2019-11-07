using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy {
    //Enemy_2 uses a sin wave to modify a 2-point linear interpolation
    public Vector3[] points;
    public float birthTime;
    public float lifeTime = 10;
    public float sinEccentricity; // determines how much the sin wave will affect movement

    void Start()
    {
        points = new Vector3[2];
        Vector3 cbMin = Utils.camBounds.min;
        Vector3 cbMax = Utils.camBounds.max;

        Vector3 v = Vector3.zero;
        //picking a random point on the left side of the screen
        v.x = cbMin.x - Main.S.enemySpawnPadding;
        v.y = Random.Range(cbMin.y, cbMax.y);
        points[0] = v;

        //[icking a random point on the right side of the screen
        v = Vector2.zero;
        v.x = cbMax.x + Main.S.enemySpawnPadding;
        v.y = Random.Range(cbMin.y, cbMax.y);

        //possibly swap sides
        if(Random.value < 0.5f)
        {
            points[0].x *= -1;
            points[1].x *= -1;
        }

        birthTime = Time.time;
    }

    public override void Move()
    {
        //Bezier curves work based on a u value between 0 & 1
        float u = (Time.time - birthTime) / lifeTime;

        //if u>1, then it has been longer than lifeTime since birthTime
        if (u > 1)
        {
            //this Enemy_2 has finished its life
            Destroy(this.gameObject);
            return;
        }
        //Adjust u by adding an easing curve based on a Sine wave
        u = u + sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2));

        //interpolate the two linear interpolation points
        pos = (1 - u) * points[0] + u * points[1];        
    }

}
