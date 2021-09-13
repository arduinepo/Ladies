using UnityEngine;

namespace Assets
{
    public class Damier : MonoBehaviour
    {
        public CaseSelector[,] cases;
        private void Awake()
        {
            cases = new CaseSelector[10, 10];
            Color brown = new Color(139/255.0f, 69/255.0f, 19/255.0f);
            bool whiteOrBrown = true;

            for (int i = 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    tile.GetComponent<Renderer>().material.color = whiteOrBrown?Color.white:brown;
                    tile.transform.position = new Vector3(i, 0, j);
                    tile.transform.rotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(0.1f, 1, 0.1f);
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
                    switch (g.grille[i, j])
                    {
                        case Grille.PION_BLANC:addPion(i, j, true);break;
                        case Grille.DAME_BLANC:addDame(i, j, true);break;
                        case Grille.PION_NOIR:addPion(i, j, false);break;
                        case Grille.DAME_NOIR:addDame(i, j, false);break;
                    }
                }
            }
        }

        void addPion(int i,int j,bool blanc)
        {
            GameObject pion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pion.GetComponent<Renderer>().material.color = blanc ? Color.white : Color.black;
            pion.transform.position = new Vector3(i, 0.1f, j);
            pion.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        }

        void addDame(int i,int j,bool blanc)
        {
            GameObject pion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pion.GetComponent<Renderer>().material.color = blanc ? Color.white : Color.black;
            pion.transform.position = new Vector3(i, 0.2f, j);
            pion.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
            cases[i, j].pion = pion;
        }

        public void display(Action a)
        {
            if(a is Mouvement)
            {
                Mouvement m = (Mouvement)a;
                if(m is ArriveeBlanc)
                {

                }
                else cases[m.caseArrivee.ligne, m.caseArrivee.colonne].pion = cases[m.caseDepart.ligne, m.caseDepart.colonne].pion;
                cases[m.caseDepart.ligne, m.caseDepart.colonne].pion = null;
            }
            else
            {

            }
        }


    }

    public class CaseSelector : MonoBehaviour
    {
        public int ligne, colonne;
        public GameObject tile,pion;

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