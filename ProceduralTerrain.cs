using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrain : MonoBehaviour {

    private float[,] PerlinNoise = new float[251,251];
    Texture2D RayTracingResult;
    public bool Show2DNoise;
    public bool Waves;
    public bool DayCycle;
    public int HeightMultiplier = 75;

    Mesh mesh;

    public Material GreenMat;
    static int xSize = 250;
    static int zSize = 250;
    public int TreeNumber;
    public int RockNumber;
    public GameObject regularLight;
    public GameObject sun;

    public Transform prefab;
    public Transform prefab2;

    Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];// = new Vector3[(xSize+1)*(zSize +1)];
    int[] triangles;
    Vector3[] norms;

    public int frequency;       //pattern changes more with increased frequency

    [Range(1, 8)]
    public int octaves = 1;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;


    // Use this for initialization
    void Start () {
        Camera this_camera = gameObject.GetComponent<Camera>();
        this_camera.transform.position = new Vector3(125, 80, -60);
        this_camera.transform.rotation = Quaternion.Euler(new Vector3(25, 0, 0));

        //if Day cycle switch light source
        if (DayCycle) {
            regularLight.SetActive(false);
            sun.SetActive(true);
        }

        //Create corners of a quad
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));
        float stepSize = 1f / 251;  //
        //interpolate these corners in our loops
        for (int y = 0; y < 251; y++)
        {
            //interpolate bottom left and top left corner based on y
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            //interpolate bottom right and top right corner based on y
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < 251; x++)
            {
                //interpolate these two points based on x
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = Perlin2D(point, frequency);//If we just want regular perlin noise without fractul
                sample = Fractal(point, frequency,octaves, lacunarity, persistence);
                PerlinNoise[x, y] = sample;
            }
        }

        //Change camera to diplay generated noise
        if (Show2DNoise)
        {
            Debug.Assert(this_camera);      //logs error message on failure
            int pixel_width = this_camera.pixelWidth;
            int pixel_height = this_camera.pixelHeight;
            RayTracingResult = new Texture2D(pixel_width, pixel_height);
            for (int i = 0; i < pixel_width; ++i)
            {
                for (int j = 0; j < pixel_height; ++j)
                {
                    if (i < 250 && j < 250)
                    {
                        float cv = PerlinNoise[i, j];
                        Color PN = new Color(cv, cv, cv, 1);    //color is from black to white
                        RayTracingResult.SetPixel(i, j, PN);
                    }
                    else {  //If not in 250x250 quadrant show black
                        RayTracingResult.SetPixel(i, j, Color.black);
                    }
                }
            }
            RayTracingResult.Apply();
        }

        //Build Mesh and Return a Vector3 array of norms
        norms = MeshBuilder();

        //Try random vertices for object placement until all are placed
        //will loop forever if too many objects or not enough land
        bool[,] marked = new bool[xSize+1,zSize+1];
        while (TreeNumber > 0) {
            int x = Random.Range(0, 251);
            int z = Random.Range(0, 251);
            if ((PerlinNoise[x, z]*HeightMultiplier) > 0 && (marked[x,z] == false)){
                marked[x, z] = true;
                Instantiate(prefab, new Vector3(x, PerlinNoise[x,z]*HeightMultiplier, z), Quaternion.FromToRotation(transform.up, norms[x *(xSize+1)+ z])/*transform.rotation*/);
                TreeNumber--;
            }
        }
        while (RockNumber > 0){
            int x = Random.Range(0, 251);
            int z = Random.Range(0, 251);
            if ((PerlinNoise[x, z] * HeightMultiplier) > 0 && (marked[x, z] == false)){
                marked[x, z] = true;
                Instantiate(prefab2, new Vector3(x, PerlinNoise[x, z] * HeightMultiplier, z), Quaternion.FromToRotation(transform.up, norms[x * (xSize + 1) + z])/*transform.rotation*/);
                RockNumber--;
            }
        }

        if (Waves)
        {
            InvokeRepeating("SlowUpdate", 0.0f, 0.25f); //updates waves every 0.25 seconds
        }
    }

    //From Sebastian Lague youtube videos
    Vector3[] CalculateNormals() {
        Vector3[] VertNorms = new Vector3[vertices.Length];
        int TriangleCount = triangles.Length / 3;

        for (int i = 0; i < TriangleCount; i++) {
            //get vertices of each triangle
            int NormalTriangleIndex = i*3;
            int VertA = triangles[NormalTriangleIndex];
            int VertB = triangles[NormalTriangleIndex+1];
            int VertC = triangles[NormalTriangleIndex+2];

            //Calculate each triangle normal
            Vector3 triangleNormal = SurfaceNormalFromIndices(VertA, VertB, VertC);
            //Set the vertices normal to that of it's triangle
            VertNorms[VertA] += triangleNormal;
            VertNorms[VertB] += triangleNormal;
            VertNorms[VertC] += triangleNormal;
        }

        for (int i = 0; i < VertNorms.Length; i++) {
            VertNorms[i].Normalize();
        }

        return VertNorms;
    }

    //from Sebastian Lague youtube
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        //cross two of the triangle lengths to get normal
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    //Will draw normal lines if bool is set
    public bool showNormals;
    private void OnDrawGizmosSelected()
    {
        if (showNormals && vertices != null)
        {
            Gizmos.color = Color.yellow;
            for (int v = 0; v < vertices.Length; v++)
            {
                Gizmos.DrawRay(vertices[v], norms[v]);
            }
        }
    }

    //updates waves every 0.25 seconds using sin waves
    private void SlowUpdate()
    {
        //if below certain height
        for (int i = 0; i < vertices.Length; i++) {
            if (vertices[i].y <= -7) {
                float scale = 2.0f;
                float speed = 0.001f;
                float noiseStrength = 1f;
                float noiseWalk = 1f;
                vertices[i].y = Mathf.Sin(speed + vertices[i].x + vertices[i].y + vertices[i].z) * scale -8;
                vertices[i].y = Mathf.PerlinNoise(vertices[i].x + noiseWalk, vertices[i].y + Mathf.Sin(Time.time * 0.1f)) * noiseStrength -8;
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        //mesh.normals = CalculateNormals(); //works but causes lag due to being computationally heavy
    }

    //based on https://answers.unity.com/questions/1540286/dynamically-create-grid-like-plane.html
    Vector3 [] MeshBuilder() {
        triangles = new int[xSize*zSize*6];
        Vector2[] uv = new Vector2[vertices.Length];


        GameObject go = new GameObject("terrainName");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = GreenMat;
        Debug.Log(mr.bounds);

        mesh = new Mesh();

        for (int x=0, i=0; x<= xSize; x++) {
            for (int z = 0; z<= zSize; z++) {
                float y = PerlinNoise[x, z] * HeightMultiplier;
                if (y <= -8) {  //if below this height raise it to create a flat water level
                    y = -8;
                }
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2((float)z / zSize, (float)x / xSize);    //set to respective float between 0 & 1
                i++;
            }
        } 


        int vertexIndex = 0;
        int tri = 0;

        //set triangles in each mesh square, must be in certain order to render properly from top side
        for (int x = 0; x < xSize; x++){
            for (int z = 0; z < zSize; z++){
                triangles[tri + 1] = (0 + vertexIndex);
                triangles[tri + 0] = (vertexIndex + xSize +1);
                triangles[tri + 2] = (vertexIndex + 1);
                triangles[tri + 4] = (vertexIndex + 1);
                triangles[tri + 3] = (vertexIndex + xSize +1);
                triangles[tri + 5] = (vertexIndex + xSize +2);

                vertexIndex++;
                tri += 6;
            }
            vertexIndex++;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mf.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.normals = CalculateNormals();
        //mesh.RecalculateNormals(); we are calculating our own normals instead

        Vector3[] norms = mesh.normals;
        Debug.Log(mr.bounds);
        Debug.Log(mesh.bounds.extents.y);
        return norms;
    }
	
    //Shows 2D noise if set
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Show2DNoise)
        {
            //Show the generated ray tracing image on screen
            Graphics.Blit(RayTracingResult, destination);
        }
        else {
            Graphics.Blit(source, destination);
        }
    }

    //Based on https://catlikecoding.com/unity/tutorials/noise/
    private const int gradientsMask2D = 7;
    private static float sqr2 = Mathf.Sqrt(2f);
    private const int hashMask = 255;

    public static float Perlin2D(Vector3 point, float frequency){
        point *= frequency;
        int ix0 = Mathf.FloorToInt(point.x);    //returns largest integer smaller to or equal to x
        int iy0 = Mathf.FloorToInt(point.y);    //returns largest integer smaller to or equal to y
        float tx0 = point.x - ix0;
        float ty0 = point.y - iy0;
        float tx1 = tx0 - 1f;
        float ty1 = ty0 - 1f;
        ix0 &= hashMask;
        iy0 &= hashMask;
        int ix1 = ix0 + 1;
        int iy1 = iy0 + 1;

        int h0 = hash[ix0];
        int h1 = hash[ix1];
        Vector2 g00 = gradients2D[hash[h0 + iy0] & gradientsMask2D];
        Vector2 g10 = gradients2D[hash[h1 + iy0] & gradientsMask2D];
        Vector2 g01 = gradients2D[hash[h0 + iy1] & gradientsMask2D];
        Vector2 g11 = gradients2D[hash[h1 + iy1] & gradientsMask2D];
        //compute gradients
        float v00 = Dot(g00, tx0, ty0);
        float v10 = Dot(g10, tx1, ty0);
        float v01 = Dot(g01, tx0, ty1);
        float v11 = Dot(g11, tx1, ty1);

        float tx = Smooth(tx0);
        float ty = Smooth(ty0);
        //interpolate the gradients
        return Mathf.Lerp(
            Mathf.Lerp(v00, v10, tx),
            Mathf.Lerp(v01, v11, tx),
            ty) * sqr2;
    }

    private static Vector2[] gradients2D = {
        //up, down, left, right
        new Vector2( 1f, 0f),
        new Vector2(-1f, 0f),
        new Vector2( 0f, 1f),
        new Vector2( 0f,-1f),
        //diagonals but normalized to have same length
        new Vector2( 1f, 1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2( 1f,-1f).normalized,
        new Vector2(-1f,-1f).normalized
    };

    //smoothes transitions
    private static float Smooth(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    //Permutation array to genereate seemingly random numbers
    //same one ken perlin used
    //doubled in size to prevent out of bounds in
    private static int[] hash = {
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
         57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
         74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
         60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
         65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
         52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
         81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
         57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
         74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
         60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
         65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
         52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
         81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };

    private static float Dot(Vector2 g, float x, float y)
    {
        return g.x * x + g.y * y;
    }

    public static float Fractal (Vector3 point, float frequency, int octaves, float lacunarity, float persistence)
    {
        float sum = Perlin2D(point, frequency);
        float amplitude = 1f;
        float range = 1f;
        for (int o = 1; o < octaves; o++)
        {
            frequency *= lacunarity;    //higher lucranarity = more gaps
            amplitude *= persistence;   //persistance = how much influence each octave has
            range += amplitude;
            sum += Perlin2D(point, frequency) * amplitude;
        }
        return sum / range;
    }
}
