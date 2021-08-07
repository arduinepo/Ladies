using UnityEngine;

namespace Assets
{
    public class Damier : MonoBehaviour
    {
        public CaseSelector[,] cases;
        private void Awake()
        {
            cases = new CaseSelector[10, 10];
            Color brown = new Color(139, 69, 19);
            bool whiteOrBrown = true;

            for (int i = 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    tile.GetComponent<Renderer>().material.color = whiteOrBrown ? Color.white : brown;
                    tile.transform.position = new Vector3(i, 0.001f, j);
                    tile.transform.rotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(1, 0.001f, 1);
                    cases[i, j] = tile.AddComponent<CaseSelector>();
                    cases[i, j].set(i, j,tile);
                    whiteOrBrown = !whiteOrBrown;
                }
                whiteOrBrown = !whiteOrBrown;
            }

        }

        public void initDisplay(Grille g)
        {
            for (int i = 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    
                }
            }
        }

        public void display(Action a)
        {

        }


    }

    public class CaseSelector : MonoBehaviour
    {
        public int ligne, colonne;
        GameObject tile;

        public void set(int i, int j, GameObject tile)
        {
            ligne = i;
            colonne = j;
            this.tile = tile;
        }

        public void OnMouseUpAsButton()
        {
            Jeu.jeuEnCours.selectCase(ligne, colonne);
        }
    }

}