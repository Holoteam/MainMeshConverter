using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;


public class XYZMConvert : MonoBehaviour
{
    Vector3[] mVerts;
    Vector2[] mUVs;
    int[] mTris;
    Material xyzm_mat;

    // Start is called before the first frame update
    void Start()
    {

        string path = @"/Users/billy/Documents/Purdue Academics/Fall 2020/ENGR 17911/MeshConverter/Meshes/handPythonTest.xyzm"; //Ok so I know the path is legit
        int imageWidth;
        int imageHeight;
        int imageSize;
        int n;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        using (FileStream fs = File.OpenRead(path))
        {
            //Reading the size data in from the header
            //This is may inital way with reading the bytes directly from steam

            /*
            //Start of section that finds the width and height
            byte[] spareBytes = new byte[500]; //I need to make an array to hold int bytes. transfer to ints.
            byte[] currentByte = new byte[1];
            byte[] widthBytes = new byte[5];
            byte[] heightBytes = new byte[5];

            n = fs.Read(spareBytes, 0, 28);

            //store the bytes into an array that can be converted to the int
            int locationW = 0;
            int space = 0;
            while(space == 0)
            {
                n = fs.Read(widthBytes, locationW, 1);
                if((int)widthBytes[locationW] == 32) //Checks if the space is read
                {
                    space++;
                    widthBytes[locationW] = 0;
                    locationW = locationW - 2;
                }
                locationW++;
            }
            space = 0;
            n = fs.Read(spareBytes, 0, 2);

            int locationH = 0;

            while(space == 0)
            {
                n = fs.Read(heightBytes, locationH, 1);
                if((int)heightBytes[locationH] == 32) //Checks if the space is read
                {
                    space++;
                    heightBytes[locationH] = 0;
                    locationH = locationH - 2;
                }
                locationH++;
            }

            //Converst the bytes to the actual integer values I want
            string widthString = System.Text.Encoding.UTF8.GetString(widthBytes, 0, locationW + 1);
            string heightString = System.Text.Encoding.UTF8.GetString(heightBytes, 0, locationH + 1);
            imageWidth = int.Parse(widthString);
            imageHeight = int.Parse(heightString);
            imageSize = imageWidth * imageHeight;
            Debug.Log(imageWidth);
            Debug.Log(imageHeight);
            Debug.Log(imageSize);
            Debug.Log("This should not print");
            */







            imageWidth = 480; //Hand: 480, Zeus: 800
            imageHeight = 640;
            imageSize = imageWidth * imageHeight;

            //End of section that finds width and height

            mVerts = new Vector3[imageSize];
            mUVs = new Vector2[imageSize];
            mTris = new int[imageSize];

            float[] m_xyzData = new float[imageSize*3];
            byte[] thisReadsfour = new byte[40];
            byte[] dummyFloatByte = new byte[imageSize*12];
            byte[] m_textureData = new byte [imageSize*3]; //Had this as a byte, had to change for compiler
            byte[] m_maskData = new byte [imageSize]; //Same as above.


            //Reads everything in
            //Dummy array to hold the byte values for now

            n = fs.Read(thisReadsfour, 0, 37); //Playing with the index
            n = fs.Read(dummyFloatByte, 0, imageSize*12); //should read in 4 times the values because each float is 4 bits.
            n = fs.Read(m_textureData, 0, imageSize*3);
            n = fs.Read(m_maskData, 0, imageSize);

            //for (int i = 0; i < imageSize; i++)
            //   if (m_maskData[i] != 0)
            //        m_maskData[i] = 255;

            //Convert the bytes to float values in the array
            int count = 0;
            for(int i = 0; i < imageSize*3; i++)
            {
                m_xyzData[i] = BitConverter.ToSingle(dummyFloatByte, count);
                count = count + 4;
            }
            Debug.Log(count);

            //Puts the float values into the mVerts to represent the vertecies with color.
            List<Color32> mesh_colors = new List<Color32>();
            int vertCount = 0;
            count = 0;
            while(count < imageSize*3)
            {
                mVerts[vertCount] = new Vector3(m_xyzData[count], m_xyzData[count+1], m_xyzData[count+2]);
                mesh_colors.Add(new Color((int)m_textureData[count] / 255.0f, (int)m_textureData[count+1] / 255.0f, (int)m_textureData[count+2] / 255.0f));
                count = count + 3;
                vertCount++;
            }
            count = 0;
            Debug.Log(vertCount);


            List<int> triangles = new List<int>();
            //float average_triangle_size = 0;
            //int num_of_triangles = 0;
            int interval = 1;
            for (int i = 0; i < imageHeight - interval; i+=interval)
            {
                int lineNo = i * imageWidth;
                for (int j = 0; j < imageWidth - interval; j+=interval)
                {
                    int id0 = lineNo + j;
                    int id1 = id0 + imageWidth * interval;
                    int id2 = id1 + interval;
                    int id3 = id0 + interval;

                    //This way works but creats the large triangels at the edge
                    /*
                    if(m_maskData[id0] != 0 & m_maskData[id1] != 0 & m_maskData[id2] != 0) //Checks if mask is valid
                    {
                        triangles.Add(id0);
                        triangles.Add(id2);
                        triangles.Add(id1);

                    }
                    if(m_maskData[id0] != 0 & m_maskData[id2] != 0 & m_maskData[id3] != 0)
                    {
                        triangles.Add(id0);
                        triangles.Add(id3);
                        triangles.Add(id2);
                    }
                    */

                    if(m_maskData[id0] != 0 && m_maskData[id1] != 0 && m_maskData[id2] != 0 && m_maskData[id3] != 0)
                    {
                        float triangle_limit = 3.5f; //5.5 for hand,
                        if(find_area(mVerts[id0], mVerts[id1], mVerts[id2]) < triangle_limit)
                        {
                            triangles.Add(id0);
                            triangles.Add(id2);
                            triangles.Add(id1);
                        }
                        if(find_area(mVerts[id0], mVerts[id2], mVerts[id3]) < triangle_limit)
                        {
                            triangles.Add(id0);
                            triangles.Add(id3);
                            triangles.Add(id2);
                        }


                        //This is to find the average size
                        /*
                        num_of_triangles = num_of_triangles + 2;
                        average_triangle_size = average_triangle_size + find_area(mVerts[id0], mVerts[id1], mVerts[id2]);
                        average_triangle_size = average_triangle_size + find_area(mVerts[id0], mVerts[id2], mVerts[id3]);
                        */

                    }
                }
            }




            //Finding average area
            //average_triangle_size = average_triangle_size / num_of_triangles;
            //Debug.Log(average_triangle_size);


            //Creating the uv array
            for (int i = 0; i < mUVs.Length; i++)
            {
                mUVs[i] = new Vector2(mVerts[i].x, mVerts[i].z);
            }
            //This is the end of the creation of uv array

            int[] triangleArray = triangles.ToArray(); //Converts list to an array.
            Color32[] colors = mesh_colors.ToArray(); //Conversts color list to array

            mesh.vertices = mVerts;
            //mesh.uv = mUVs;
            mesh.triangles = triangleArray;
            mesh.colors32 = colors;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    float find_area(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        Vector3 normal_vector = Vector3.Cross(side1, side2);
        float area = normal_vector.magnitude / 2;
        return area;
    }
}
