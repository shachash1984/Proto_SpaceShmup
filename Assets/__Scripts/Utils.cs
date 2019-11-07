using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoundsTest
{
    CENTER,    //Is the center of the GameObject on screen?
    ONSCREEN,  //Are the bounds entirely on screen?
    OFFSCREEN  //Are the bounds entirely off screen?
}

public class Utils
{

    //============================ Bounds Functions =============================\\

    //create bounds that encapsulates the 2 bounds passed in
    public static Bounds BoundsUnion(Bounds b0, Bounds b1)
    {
        if (b0.size == Vector3.zero && b1.size != Vector3.zero)
            return b1;
        else if (b0.size != Vector3.zero && b1.size == Vector3.zero)
            return b0;
        else if (b0.size == Vector3.zero && b1.size == Vector3.zero)
            return b0;
        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return b0;
    }

    //set bounds to encapsulate the bounds of the renderer and collider
    //iterating recursively through all of the GameObject's children
    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        if (go.GetComponent<Renderer>() != null)
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        if (go.GetComponent<Collider>() != null)
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        foreach (Transform t in go.transform)
        {
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }
        return b;
    }

    //read-only property to hold the bounds of the cam (orthographic)
    static public Bounds camBounds
    {
        get
        {
            if (_camBounds.size == Vector3.zero)
                SetCameraBounds();
            return _camBounds;
        }
    }

    static private Bounds _camBounds;

    //used by _camBounds to set camera bounds and can also be called directly
    public static void SetCameraBounds(Camera cam = null)
    {
        if (cam == null)
            cam = Camera.main;
        //camera needs to be orthographic
        //camera rotation needs to be [0,0,0]
        Vector3 boundTLN = cam.ScreenToWorldPoint(Vector3.zero); // top left near cam bound
        Vector3 boundBRF = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)); // bottom right far cam bounds
        //adjusting the Z's
        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;
        //finding the center
        Vector3 center = (boundTLN + boundBRF) / 2;
        _camBounds = new Bounds(center, Vector3.zero);
        //expand _camBounds to encapsulate the extents
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }

    //check to see if the Bounds bnd are within the camBounds
    public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest test = BoundsTest.CENTER)
    {
        return BoundsInBoundsCheck(camBounds, bnd, test);
    }

    //check to see whether Bounds lilB are within Bounds bigB
    //the behavior of this function is different based on the BoundsTest selected
    public static Vector3 BoundsInBoundsCheck(Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.ONSCREEN)
    {
        //get the center of lilB
        Vector3 pos = lilB.center;
        //initialize the offset at [0,0,0]
        Vector3 off = Vector3.zero;

        switch (test)
        {
            //the CENTER test determines what off (offset) would have to be applied to lilB to move its center back inside bigB
            case BoundsTest.CENTER:
                if (bigB.Contains(pos))
                    return Vector3.zero;

                if (pos.x > bigB.max.x)
                    off.x = pos.x - bigB.max.x;
                else if (pos.x < bigB.min.x)
                    off.x = pos.x - bigB.min.x;

                if (pos.y > bigB.max.y)
                    off.y = pos.y - bigB.max.y;
                else if (pos.y < bigB.min.y)
                    off.y = pos.y - bigB.min.y;

                if (pos.z > bigB.max.z)
                    off.z = pos.z - bigB.max.z;
                else if (pos.z < bigB.min.z)
                    off.z = pos.z - bigB.min.z;

                return off;

            //the ONSCREEN test determines what offset would have to be applied to keep all of lilB inside bigB
            case BoundsTest.ONSCREEN:
                if (bigB.Contains(lilB.min) && bigB.Contains(lilB.max))
                    return Vector3.zero;

                if (lilB.max.x > bigB.max.x)
                    off.x = lilB.max.x - bigB.max.x;
                else if (lilB.min.x < bigB.min.x)
                    off.x = lilB.min.x - bigB.min.x;

                if (lilB.max.y > bigB.max.y)
                    off.y = lilB.max.y - bigB.max.y;
                else if (lilB.min.y < bigB.min.y)
                    off.y = lilB.min.y - bigB.min.y;

                if (lilB.max.z > bigB.max.z)
                    off.z = lilB.max.z - bigB.max.z;
                else if (lilB.min.z < bigB.min.z)
                    off.z = lilB.min.z - bigB.min.z;

                return off;

            //the OFFSCREEN test determines what offset would need to be applied to move any tiny part of lilB inside of bigB
            case BoundsTest.OFFSCREEN:
                bool cMin = bigB.Contains(lilB.min);
                bool cMax = bigB.Contains(lilB.max);
                if (cMin || cMax)
                    return Vector3.zero;

                if (lilB.min.x > bigB.max.x)
                    off.x = lilB.min.x - bigB.max.x;
                else if (lilB.max.x < bigB.min.x)
                    off.x = lilB.max.x - bigB.min.x;

                if (lilB.min.y > bigB.max.y)
                    off.y = lilB.min.y - bigB.max.y;
                else if (lilB.max.y < bigB.min.y)
                    off.y = lilB.max.y - bigB.min.y;

                if (lilB.min.z > bigB.max.z)
                    off.z = lilB.min.z - bigB.max.z;
                else if (lilB.min.z < bigB.min.z)
                    off.z = lilB.max.z - bigB.min.z;

                return off;
        }
        return Vector3.zero;
    }


    //=============================Transform Functions============================\\

    //this function will iteratively climb up the transform.parent tree until it either finds a parent with a tag != "Untagged" or no parent
    public static GameObject FindTaggedParent(GameObject go)
    {
        if (go.tag != "Untagged")
            return go;
        if (go.transform.parent == null)
            return null;

        return FindTaggedParent(go.transform.parent.gameObject);
    }
    //overload version handling Transform
    public static GameObject FindTaggedParent(Transform t)
    {
        return FindTaggedParent(t.gameObject);
    }


    //=============================Materials Functions============================\\

    //Returns a list of all Materials on this GameObject or its children
    static public Material[] GetAllMaterials(GameObject go)
    {
        List<Material> mats = new List<Material>();
        if (go.GetComponent<Renderer>() != null)
            mats.Add(go.GetComponent<Renderer>().material);

        foreach (Transform t in go.transform)
        {
            mats.AddRange(GetAllMaterials(t.gameObject));
        }
        return mats.ToArray();
    }

    //=============================Linear Interpolation Functions============================\\

    //The standard Vector Lerp functions in Unity dont allow for extrapolation (u is clamped to 0 <= u <= 1)
    static public Vector3 Lerp(Vector3 vFrom, Vector3 vTo, float u)
    {
        Vector3 res = (1 - u) * vFrom + u * vTo;
        return res;
    }

    //While most bezier curves are 3-4 points, it is possible to have any number of points using this recursive function
    //this uses the lerp function above because the Vector3.Lerp doesnt allow extrapolation

    static public Vector3 Bezier(float u, List<Vector3> vList)
    {
        //if there is only one element in vList return it
        if (vList.Count == 1)
            return vList[0];

        //Create vListR, which is all but the 0th element of vList. e.g., if vList = [0,1,2,3,4] then vListR = [1,2,3,4]
        List<Vector3> vListR = vList.GetRange(1, vList.Count - 1);

        //And create vListL, which is all but the last element of vList. e.g., if vList = [0,1,2,3,4] then vListR = [0,1,2,3]
        List<Vector3> vListL = vList.GetRange(0, vList.Count - 1);

        //the result is the lerp of the bezier of these two shorter lists
        Vector3 res = Lerp(Bezier(u, vListL), Bezier(u, vListR), u);
        //the Bezier function recursively calls itself here to split the lists down until there is only one value in each
        return res;
    }

    //This version allows an array or a series of Vector3s as input which is then converted into a List<Vector3>
    static public Vector3 Bezier(float u, params Vector3[] vecs)
    {
        return (Bezier(u, new List<Vector3>(vecs)));
    }

}






