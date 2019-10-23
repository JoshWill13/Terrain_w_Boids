using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
public class Hermite_Spline : MonoBehaviour 
{
	public List<GameObject> controlPoints = new List<GameObject>();
	public Color color = Color.white;
	public float width = 0.2f;
	public int numberOfPoints = 20;
	LineRenderer lineRenderer;
    int i = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));

        //
        if (null == lineRenderer || controlPoints == null || controlPoints.Count < 2)
        {
            return; // not enough points specified
        }

        // update line renderer
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        if (numberOfPoints < 2)
        {
            numberOfPoints = 2;
        }
        lineRenderer.positionCount = numberOfPoints * (controlPoints.Count - 1);

        // loop over segments of spline
        Vector3 p0, p1, m0, m1;

        for (int j = 0; j < controlPoints.Count - 1; j++)
        {
            // check control points
            if (controlPoints[j] == null || controlPoints[j + 1] == null ||
                (j > 0 && controlPoints[j - 1] == null) ||
                (j < controlPoints.Count - 2 && controlPoints[j + 2] == null))
            {
                return;
            }
            // determine control points of segment
            p0 = controlPoints[j].transform.position;
            p1 = controlPoints[j + 1].transform.position;

            if (j > 0)
            {
                m0 = 0.5f * (controlPoints[j + 1].transform.position - controlPoints[j - 1].transform.position);
            }
            else
            {
                m0 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
            }
            if (j < controlPoints.Count - 2)
            {
                m1 = 0.5f * (controlPoints[j + 2].transform.position - controlPoints[j].transform.position);
            }
            else
            {
                m1 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
            }

            // set points of Hermite curve
            Vector3 position;
            float t;
            float pointStep = 1.0f / numberOfPoints;

            if (j == controlPoints.Count - 2)
            {
                pointStep = 1.0f / (numberOfPoints - 1.0f);
                // last point of last segment should reach p1
            }
            for (int i = 0; i < numberOfPoints; i++)
            {
                t = i * pointStep;
                position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0    //b00 = 2t^3 - 3t^2 +a
                    + (t * t * t - 2.0f * t * t + t) * m0                   //b10 = t^3 - 2t^2 + t
                    + (-2.0f * t * t * t + 3.0f * t * t) * p1               //b01 = -2t^3 + 3t^2
                    + (t * t * t - t * t) * m1;                             //b11 = t^3 - t^2
                lineRenderer.SetPosition(i + j * numberOfPoints,
                    position);
                //transform.position = Vector3.MoveTowards(transform.position, position, 10 * Time.deltaTime);
                
            }
        }
        InvokeRepeating("SlowUpdate", 0.0f, 0.05f);
    }

	void SlowUpdate () 
	{
        if (i >= (20 * 24)-1) {
            i = 0;
        }
        i++;
        transform.position = Vector3.MoveTowards(transform.position, lineRenderer.GetPosition(i), 100 * Time.deltaTime);
        if (i == (20 * 24) - 1)
        {
            var q = Quaternion.LookRotation(lineRenderer.GetPosition(0) - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 100 * Time.deltaTime);
        }
        else
        {
            var q = Quaternion.LookRotation(lineRenderer.GetPosition(i + 10) - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 100 * Time.deltaTime);
        }
        /*float step = 10 * Time.deltaTime;
        Vector3 dir = lineRenderer.GetPosition(i+10) - transform.position;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);*/
            /*if (null == lineRenderer || controlPoints == null || controlPoints.Count < 2)
            {
                return; // not enough points specified
            }

            // update line renderer
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            if (numberOfPoints < 2)
            {
                numberOfPoints = 2;
            }
            lineRenderer.positionCount = numberOfPoints * (controlPoints.Count - 1);

            // loop over segments of spline
            Vector3 p0, p1, m0, m1;

            for(int j = 0; j < controlPoints.Count - 1; j++)
            {
                // check control points
                if (controlPoints[j] == null || controlPoints[j + 1] == null ||
                    (j > 0 && controlPoints[j - 1] == null) ||
                    (j < controlPoints.Count - 2 && controlPoints[j + 2] == null))
                {
                    return;  
                }
                // determine control points of segment
                p0 = controlPoints[j].transform.position;
                p1 = controlPoints[j + 1].transform.position;

                if (j > 0) 
                {
                    m0 = 0.5f * (controlPoints[j + 1].transform.position - controlPoints[j - 1].transform.position);
                }
                else
                {
                    m0 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
                }
                if (j < controlPoints.Count - 2)
                {
                    m1 = 0.5f * (controlPoints[j + 2].transform.position - controlPoints[j].transform.position);
                }
                else
                {
                    m1 = controlPoints[j + 1].transform.position - controlPoints[j].transform.position;
                }

                // set points of Hermite curve
                Vector3 position;
                float t;
                float pointStep = 1.0f / numberOfPoints;

                if (j == controlPoints.Count - 2)
                {
                    pointStep = 1.0f / (numberOfPoints - 1.0f);
                    // last point of last segment should reach p1
                }  
                for(int i = 0; i < numberOfPoints; i++) 
                {
                    t = i * pointStep;
                    position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0 
                        + (t * t * t - 2.0f * t * t + t) * m0 
                        + (-2.0f * t * t * t + 3.0f * t * t) * p1 
                        + (t * t * t - t * t) * m1;
                    lineRenderer.SetPosition(i + j * numberOfPoints, 
                        position);
                    //transform.position = Vector3.MoveTowards(transform.position, position, 10 * Time.deltaTime);

                }*/
        
	}
}
