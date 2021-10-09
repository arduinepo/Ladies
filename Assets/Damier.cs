using UnityEngine;
using System.Collections.Generic;

namespace Assets
{
    public class Damier : MonoBehaviour
    {
        CaseSelector[,] cases;
        Grille g;
        public static Color gold = new Color(1, 215 / 255.0f, 0), brown = new Color(139 / 255.0f, 69 / 255.0f, 19 / 255.0f);

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
            pion.AddComponent<Jump>();
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
            crown.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            pion.AddComponent<Jump>();
        }

        public void display(Action a)
        {
            if (a is Mouvement)
            {
                Mouvement m = (Mouvement)a;
                Vector3 mi = new Vector3((m.caseDepart.ligne + m.caseArrivee.ligne) / 2, 0.5f, (m.caseDepart.colonne + m.caseArrivee.colonne) / 2);
                cases[m.caseDepart.ligne, m.caseDepart.colonne].pion.GetComponent<Jump>().set(mi, new Vector3(m.caseArrivee.ligne, 0.1f, m.caseArrivee.colonne));
                cases[m.caseArrivee.ligne, m.caseArrivee.colonne].pion = cases[m.caseDepart.ligne, m.caseDepart.colonne].pion;
                if (m is ArriveeBlanc || m is ArriveeNoir)
                    cases[m.caseDepart.ligne, m.caseDepart.colonne].pion.GetComponent<Jump>().promDame = true;
            }
            else
            {
                Prise p = (Prise)a;
                List<GameObject> pions = new List<GameObject>(p.taille);
                for (int i = 1; i < p.cases.Count; i += 2)
                    pions.Add(cases[p.cases[i].ligne, p.cases[i].colonne].pion);
                Case arr = p.cases[p.cases.Count - 1];
                if ((p is PriseNoir && arr.ligne == g.taille - 1) || (p is PriseBlanc && arr.ligne == 0))
                    cases[p.cases[0].ligne, p.cases[0].colonne].pion.GetComponent<Jump>().promDame = true;
                cases[p.cases[0].ligne, p.cases[0].colonne].pion.GetComponent<Jump>().set(p, pions);
                cases[arr.ligne, arr.colonne].pion = cases[p.cases[0].ligne, p.cases[0].colonne].pion;
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
            if (Input.GetMouseButtonDown(0) && coll.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
                Jeu.jeuEnCours.selectCase(ligne, colonne);
        }

    }

    public class Jump : MonoBehaviour
    {
        Vector3 p0, p1, p2;
        Vector3[] points;
        bool jump = false;
        int pos = 0;
        static float step;
        Prise p;
        List<GameObject> pions;
        int cur = 0;
        public bool promDame = false;

        void Awake()
        {
            p0 = transform.position;
            
        }

        public void set(Vector3 p15, Vector3 p3)
        {
            p1 = new Vector3((p0.x + p15.x) / 2, p15.y, (p0.z + p15.z) / 2);
            p2 = new Vector3((p3.x + p15.x) / 2, p15.y, (p3.z + p15.z) / 2);
            if (step == 0.0f)
            {
                step = Time.smoothDeltaTime * 5.0f;
            }
            float s = step, minus = 1.0f - s;
            points = new Vector3[(int)(1.0f / step)];
            float sqMinus = minus * minus, sqS = s * s;
            for (int i = 0; i < points.Length - 1; i++, s += step, minus -= step, sqMinus = minus * minus, sqS = s * s)
                points[i] = p0 * (sqMinus * minus) + 3 * p1 * (s * sqMinus) + 3 * p2 * (sqS * minus) + p3 * (sqS * s);
            points[points.Length - 1] = p3;
            jump = true;
        }

        public void set(Prise p, List<GameObject> pions)
        {
            this.p = p;
            this.pions = pions;
            set(new Vector3(p.cases[cur * 2 + 1].ligne, 0.5f, p.cases[cur * 2 + 1].colonne), new Vector3(p.cases[cur * 2 + 2].ligne, 0.1f, p.cases[cur * 2 + 2].colonne));
        }

        void FixedUpdate()
        {
            if (jump)
            {
                transform.position = points[pos++];
                if (pos == points.Length)
                {
                    pos = 0;
                    p0 = transform.position;
                    if (p != null)
                    {
                        GameObject.Destroy(pions[cur++]);
                        if (cur < p.taille)
                            set(new Vector3(p.cases[cur * 2 + 1].ligne, 0.5f, p.cases[cur * 2 + 1].colonne), new Vector3(p.cases[cur * 2 + 2].ligne, 0.1f, p.cases[cur * 2 + 2].colonne));
                        else
                        {
                            if (promDame)
                            {
                                addCrown();
                                promDame = false;
                            }
                            p = null;
                            pions = null;
                            jump = false;
                            cur = 0;
                            Jeu.jeuEnCours.displayEnded = true;
                        }
                    }
                    else
                    {
                        if (promDame)
                        {
                            addCrown();
                            promDame = false;
                        }
                        jump = false;
                        Jeu.jeuEnCours.displayEnded = true;
                    }
                }
            }
        }

        void addCrown()
        {
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.transform.parent = transform;
            crown.GetComponent<Renderer>().material.color = Damier.gold;
            crown.transform.localPosition = new Vector3(0, 1, 0);
            crown.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            promDame = false;
        }

    }

}