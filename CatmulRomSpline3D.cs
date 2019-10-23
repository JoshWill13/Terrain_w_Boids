using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on Lab tutorial
public class CatmulRomSpline3D {

    private Vector3[] counterPoints;
    private Vector3[] tangents;
    private float parametric_range;

    private Vector4 h_func(float t) {
        float t2 = t * t;
        float t3 = t2 * t;
        float hx = 2 * t3 - 3 * t2 + 1;     //h00 = 2t^3 - 3t^2 + 1
        float hy = t3 - 2 * t2 + t;         //h10 = t^3 - 2t^2 + t
        float hz = -2 * t3 + 3 * t2;        //h01 = -2t^3 + 3T^2
        float hw = t3 - t2;                 //h11 = t^3 - t^2

        return new Vector4(hx, hy, hz, hw);
    }


    public Vector3[] control_points {
        //setting counterPoints to passed in value
        set {
            counterPoints = value;
            parametric_range = counterPoints.Length - 3; //need to change for our own use
            tangents = new Vector3[counterPoints.Length - 2];
            for (int i = 0; i<tangents.Length; ++i) {
                tangents[i] = (counterPoints[i + 2] - counterPoints[i])/ 2.0f/*another parameter to change*/;
            }
        }
    }


    public Vector3 Sample(float t) {
        if (t > 0 && t < parametric_range) {
            int knot_prev = Mathf.FloorToInt(t);
            float local_param = t - (float)(knot_prev);
            Vector4 h = h_func(local_param);
            Vector3 p0 = counterPoints[knot_prev + 1];
            Vector3 m0 = tangents[knot_prev];
            Vector3 p1 = counterPoints[knot_prev+2];
            Vector3 m1 = tangents[knot_prev + 1];

            //Interpolation polynomial p(t) = h00(t)p0 + h10(t)m0 + h01(t)p1 + h11(t)m1
            return h.x * p0 + h.y * m0 + h.z * p1 + h.w * m1;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
