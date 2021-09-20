using UnityEngine;

namespace Assets
{
    public class Damier : MonoBehaviour
    {
        CaseSelector[,] cases;
        Grille g;
        Color gold= new Color(1, 215 / 255.0f, 0), brown = new Color(139 / 255.0f, 69 / 255.0f, 19 / 255.0f);

        private void Awake()
        {
            cases = new CaseSelector[10, 10];
            bool whiteOrBrown = true;

            for (int i = 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    tile.GetComponent<Renderer>().material.color = whiteOrBrown ? Color.white : brown;
                    tile.transform.position = new Vector3(i, 0, j);
                    tile.transform.rotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(0.1f, 1, 0.1f);
                    cases[i, j] = tile.AddComponent<CaseSelector>();
                    cases[i, j].set(i, j, tile);
                    whiteOrBrown = !whiteOrBrown;
                }
                whiteOrBrown = !whiteOrBrown;
            }

        }

        public void initDisplay(Grille g)
        {
            this.g = g;
            for (int i = 0; i < 10; i += 1)
            {
                for (int j = 0; j < 10; j += 1)
                {
                    switch (g.grille[i, j])
                    {
                        case Grille.PION_BLANC: addPion(i, j, true); break;
                        case Grille.DAME_BLANC: addDame(i, j, true); break;
                        case Grille.PION_NOIR: addPion(i, j, false); break;
                        case Grille.DAME_NOIR: addDame(i, j, false); break;
                    }
                }
            }
        }

        void addPion(int i, int j, bool blanc)
        {
            GameObject pion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pion.GetComponent<Renderer>().material.color = blanc ? Color.white : Color.black;
            pion.transform.position = new Vector3(i, 0.1f, j);
            pion.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            cases[i, j].pion = pion;
        }

        void addDame(int i, int j, bool blanc)
        {
            GameObject pion = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pion.GetComponent<Renderer>().material.color = blanc ? Color.white : Color.black;
            pion.transform.position = new Vector3(i, 0.1f, j);
            pion.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            cases[i, j].pion = pion;
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.transform.parent = pion.transform;
            crown.GetComponent<Renderer>().material.color = gold;
            crown.transform.localPosition = new Vector3(0, 1, 0);
            crown.transform.localScale= new Vector3(0.5f, 0.5f, 0.5f);
        }

        //montrer exécution de l'action par déplacement en cloche
        public void display(Action a)
        {
            if (a is Mouvement)
            {
                Mouvement m = (Mouvement)a;
                cases[m.caseArrivee.ligne, m.caseArrivee.colonne].pion = cases[m.caseDepart.ligne, m.caseDepart.colonne].pion;
                cases[m.caseDepart.ligne, m.caseDepart.colonne].pion = null;
                cases[m.caseArrivee.ligne, m.caseArrivee.colonne].pion.transform.position = new Vector3(m.caseArrivee.ligne, 0.1f, m.caseArrivee.colonne);
                if (m is ArriveeBlanc || m is ArriveeNoir)
                {
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    crown.transform.parent = cases[m.caseArrivee.ligne, m.caseArrivee.colonne].pion.transform;
                    crown.GetComponent<Renderer>().material.color = gold;
                    crown.transform.localPosition = new Vector3(0, 1, 0);
                    crown.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
            else
            {
                Prise p = (Prise)a;
                for (int i = 1; i < p.cases.Count; i += 2)
                {
                    GameObject.Destroy(cases[p.cases[i].ligne, p.cases[i].colonne].pion);
                    cases[p.cases[i].ligne, p.cases[i].colonne].pion = null;
                }
                Case arr = p.cases[p.cases.Count - 1];
                cases[arr.ligne, arr.colonne].pion = cases[p.cases[0].ligne, p.cases[0].colonne].pion;
                cases[arr.ligne, arr.colonne].pion.transform.position = new Vector3(arr.ligne, 0.1f, arr.colonne);
                if ((p is PriseNoir && arr.ligne == g.taille-1) || (p is PriseBlanc && arr.ligne == 0))
                {
                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    crown.transform.parent = cases[arr.ligne, arr.colonne].pion.transform;
                    crown.GetComponent<Renderer>().material.color = gold;
                    crown.transform.localPosition = new Vector3(0, 1, 0);
                    crown.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
        }


    }

    public class CaseSelector : MonoBehaviour
    {
        public int ligne, colonne;
        public GameObject tile, pion;
        public Collider coll;
        RaycastHit hit;

        public void set(int i, int j, GameObject tile)
        {
            ligne = i;
            colonne = j;
            this.tile = tile;
            coll = tile.GetComponent<Collider>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)&& coll.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
                Jeu.jeuEnCours.selectCase(ligne, colonne);
        }

    }

    

}