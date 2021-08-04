using System;
using UnityEngine;

namespace Assets
{
    public class Damier : MonoBehaviour
    {
        private Grille grille;
        public GameObject Wall;

        private void Start()
        {
            grille = new Grille();
            Mesh mesh = ConstructTileMesh(4, 1.0);
            Color brown = new Color(139, 69, 19);
            bool whiteOrBrown = true;

            for (float i = 0; i < 10; i += 1) { 
                for (float j = 0; j < 10; j += 1)
                {
                    GameObject tile = Instantiate(Wall, new Vector3(i, 0.001f, j), Quaternion.identity);
                    tile.GetComponent<MeshFilter>().mesh = mesh;
                    tile.GetComponent<Renderer>().material.color = whiteOrBrown ? Color.white : brown;
                    tile.transform.localScale = new Vector3(1, 0.001f, 1);
                    whiteOrBrown = !whiteOrBrown;
                    tile.GetComponent<Collider>
                    tile.AddComponent<CaseSelector>();
                    tile.GetComponent<CaseSelector>().setLC((byte)i, (byte)j,this);
                }
                whiteOrBrown = !whiteOrBrown;
            }
        }


        private static Mesh ConstructTileMesh(int numSides, double tileSize)
        {
            // Generate the vertices to be used for the mesh
            Vector2[] vertices2D = new Vector2[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var x = tileSize * Math.Cos(2 * Math.PI * i / numSides);
                var y = tileSize * Math.Sin(2 * Math.PI * i / numSides);
                Vector2 tempVec = new Vector2((float)x, (float)y);
                vertices2D[i] = tempVec;
            }

            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = indices
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

    }

    public class CaseSelector : MonoBehaviour
    {
        public byte ligne,colonne;
        public Damier damier;
        public void setLC(byte i, byte j,Damier g)
        {
            ligne = i;
            colonne = j;
            damier = g;
        }

        public void OnMouseUpAsButton()
        {
            
        }
    }
}