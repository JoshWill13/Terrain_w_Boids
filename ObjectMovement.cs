using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovement : MonoBehaviour {

    public float ViewRadius;
    [Range(0.001f, 3)]
    public float Acceleration;
    [Range(0.001f, 1)]
    public float speed = 0.1f;
    [Range(0.001f, 1)]
    public float collisionWeight;
    [Range(0.001f, 1)]
    public float alignmentWeight;
    [Range(0.001f, 10)]
    public float cohesionWeight;
    [Range(0.001f, 1)]
    public float followWeight;
    [Range(0, 2)]
    public float minVelocity;
    [Range(0, 2)]
    public float maxVelocity;
    [Range(0.01f, 10)]
    public float predatorAvoidanceWeight;
    public float predatorAcceleration;
    public float predatorMaxVelocity;

    public bool EntireFlockCentering;
    public bool PredatorAttack;


    //catmull rom spline
    private float current_t;
    private CatmulRomSpline3D spline;
    public Vector3[] cps = new Vector3[6];

    public int FlockSize;
    public GameObject RedArrow;
    public GameObject Seagull;
    public GameObject Predator;
    public GameObject[] Boids;
    private int BoidToAttack;
    private List<GameObject> object_list; 

    // Use this for initialization
    void Start () {
        spline = new CatmulRomSpline3D();
        spline.control_points = cps;
        current_t = 0;

        Boids = new GameObject[FlockSize];
        for (int i = 0, k = -1; i < FlockSize; i++) {
            if (i % 10 == 0) {
                k++;
            }
            Vector3 position = new Vector3((i%10)*4,60 +(k*4),10);

            GameObject boid = Instantiate(Seagull, transform.position, transform.rotation) as GameObject;
            boid.transform.localPosition = position;
            Boids[i] = boid;
        }
        BoidToAttack = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //lab code
        current_t += speed;
        if (current_t > 3) current_t = current_t - 3;
        Vector3 curr_pos = EvaluateAt(current_t);
        Vector3 next_pos = EvaluateAt(current_t + speed);
        gameObject.transform.position = curr_pos;
        Vector3 forward = next_pos - curr_pos;
        if (forward.sqrMagnitude > 1e-5)
        {
            forward.Normalize();
            gameObject.transform.forward = forward;
        }

        //for all boids calculate all forces needed, then apply that velocity
        foreach (GameObject boid in Boids)
        {
            List<GameObject> visibleBoids = CalcVisibleBoids(boid, boid.transform.position, ViewRadius);
            Vector3 collisionAvoidance = CalcAvoidance(boid, visibleBoids);
            Vector3 alignment = CalcVelocityMatch(boid, visibleBoids);
            Vector3 cohesion = CalcFlockCentering(boid, visibleBoids);
            Vector3 followArrow = CalcFollowGoal(boid, visibleBoids);  // Experimental
            Vector3 totalDemand;
            if (Vector3.Distance(Predator.transform.position, boid.transform.position) < 25)
            {
                Vector3 predAvoid = PredatorAvoidance(boid);
                totalDemand = collisionAvoidance * collisionWeight + alignment * alignmentWeight + cohesion * cohesionWeight +
                    followArrow * followWeight + predAvoid * predatorAvoidanceWeight;
            }
            else
            {
                totalDemand = collisionAvoidance * collisionWeight + alignment * alignmentWeight + cohesion * cohesionWeight + followArrow * followWeight;
            }
            Rigidbody rb = boid.GetComponent<Rigidbody>();
            rb.velocity += totalDemand * Acceleration;
            if (rb.velocity.magnitude < minVelocity)
            {
                rb.velocity = rb.velocity.normalized * minVelocity;
            }
            else if (rb.velocity.magnitude > maxVelocity)
            {
                rb.velocity = rb.velocity.normalized * maxVelocity;
            }
            boid.transform.position += rb.velocity;
            boid.transform.rotation = Quaternion.LookRotation(rb.velocity);
        }

        if (!PredatorAttack)
        {
            Vector3 followArrow = PredatorWait(Predator);
            Vector3 totalDemand = followArrow * followWeight+ (new Vector3(Random.Range(-1,1), Random.Range(-1, 1), Random.Range(-1, 1)))*0.2f;
            Rigidbody rb = Predator.GetComponent<Rigidbody>();
            rb.velocity += totalDemand * Acceleration;
            if (rb.velocity.magnitude < minVelocity)
            {
                rb.velocity = rb.velocity.normalized * minVelocity;
            }
            else if (rb.velocity.magnitude > maxVelocity)
            {
                rb.velocity = rb.velocity.normalized * maxVelocity;
            }
            Predator.transform.position += rb.velocity;
            Predator.transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
        else
        {
            if (BoidToAttack >= FlockSize) {
                BoidToAttack = 0;
            }

            Vector3 attackDir = PredatorAttackMovement(Boids[BoidToAttack]);
            Rigidbody rb = Predator.GetComponent<Rigidbody>();
            rb.velocity += attackDir * predatorAcceleration;
            if (rb.velocity.magnitude > predatorMaxVelocity)
            {
                rb.velocity = rb.velocity.normalized * predatorMaxVelocity;
            }
            Predator.transform.position += rb.velocity;
            Predator.transform.rotation = Quaternion.LookRotation(rb.velocity);
            if (Vector3.Distance(Predator.transform.position, Boids[BoidToAttack].transform.position) < 0.8f) {
                PredatorAttack = false;
                Boids[BoidToAttack].transform.position = new Vector3(0,0,0);
                BoidToAttack++;
            }
        }
        if (Input.GetKey(KeyCode.Z))
        {
            PredatorAttack = true;
        }
    }

    Vector3 EvaluateAt(float t) {
        if (t > 3) t -= 3;
        return spline.Sample(t);
    }

    List<GameObject> CalcVisibleBoids(GameObject thisBoid, Vector3 boidPos, float visibleRange) {
        List<GameObject> visBoids = new List<GameObject>();
        foreach (GameObject GO in Boids){
            if (GO != thisBoid) {
                if (Vector3.Distance(thisBoid.transform.position, GO.transform.position) <= ViewRadius) {
                    visBoids.Add(GO);
                }
            }
        }
        return visBoids;
    }

    Vector3 CalcAvoidance(GameObject thisBoid, List<GameObject> list) {
        Vector3 avoid = new Vector3(0,0,0);
        foreach (GameObject GO in list)
        {
            if (GO != thisBoid)
            {
                avoid += GO.transform.position-thisBoid.transform.position;
            }
        }
        avoid /= list.Count;
        avoid = avoid * -1;
        avoid.Normalize();
        return avoid;
    }

    Vector3 CalcVelocityMatch(GameObject thisBoid, List<GameObject> list) {
        Vector3 velocityMatch = new Vector3(0, 0, 0);
        foreach (GameObject GO in list){
            if (GO != thisBoid)
            {
                Rigidbody rb = GO.GetComponent<Rigidbody>();
                velocityMatch += rb.velocity;
            }
        }
        velocityMatch /= list.Count;
        velocityMatch.Normalize();
        return velocityMatch;
    }

    Vector3 CalcFlockCentering(GameObject thisBoid, List<GameObject> list) {
        if (EntireFlockCentering)
        {
            Vector3 coh = new Vector3(thisBoid.transform.position.x, thisBoid.transform.position.y, thisBoid.transform.position.z);
            foreach (GameObject GO in Boids)
            {
                if (GO != thisBoid)
                {
                coh += new Vector3(GO.transform.position.x,
                GO.transform.position.y,
                GO.transform.position.z);
                }
            }
            coh /= FlockSize;
            Vector3 v = new Vector3(coh.x - thisBoid.transform.position.x, coh.y - thisBoid.transform.position.y,
                coh.z - thisBoid.transform.position.z);
            v.Normalize();
            return v;
        }
        else
        {
            Vector3 coh = new Vector3(thisBoid.transform.position.x, thisBoid.transform.position.y, thisBoid.transform.position.z);
            foreach (GameObject GO in list)
            {
                if (GO != thisBoid)
                {
                coh += new Vector3(GO.transform.position.x,
                GO.transform.position.y,
                GO.transform.position.z);
                }
            }
            coh /= (list.Count + 1);
            Vector3 v = new Vector3(coh.x - thisBoid.transform.position.x, coh.y - thisBoid.transform.position.y,
                coh.z - thisBoid.transform.position.z);
            v.Normalize();
            return v;
        }
    }

    Vector3 CalcFollowGoal(GameObject thisBoid, List<GameObject> list)
    {
        Vector3 fg = new Vector3(RedArrow.transform.position.x - thisBoid.transform.position.x,
                                RedArrow.transform.position.y - thisBoid.transform.position.y,
                                RedArrow.transform.position.z - thisBoid.transform.position.z);
        fg.Normalize();
        return fg;
    }


    Vector3 PredatorWait(GameObject thisBoid)
    {
        Vector3 fg = new Vector3(125 - thisBoid.transform.position.x,
                                70 - thisBoid.transform.position.y,
                                125 - thisBoid.transform.position.z);
        fg.Normalize();
        return fg;
    }

    Vector3 PredatorAttackMovement(GameObject prey) {
        Vector3 ad = new Vector3(prey.transform.position.x - Predator.transform.position.x,
                                prey.transform.position.y - Predator.transform.position.y,
                                prey.transform.position.z - Predator.transform.position.z);
        ad.Normalize();
        return ad;
    }

    Vector3 PredatorAvoidance(GameObject thisBoid) {
        Vector3 avoid = Predator.transform.position - thisBoid.transform.position;
        avoid = avoid * -1;
        avoid.Normalize();
        return avoid;
    }
}
