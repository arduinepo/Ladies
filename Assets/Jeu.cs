using UnityEngine;

namespace Assets
{
    public class Jeu : MonoBehaviour
    {
        public static Jeu jeuEnCours;
        public Joueur j1;
        public Grille g;
        public Damier d;
        Case depart, c2;

        void Awake()
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().transform.LookAt(new Vector3(4.5f, 0, 4.5f));
            jeuEnCours = this;
            depart = null;
            c2 = null;
            d = GameObject.FindGameObjectWithTag("Damier").GetComponent<Damier>();
            g = new Grille();
            j1 = new IAlphaBeta(g, new BaseN(1, 3), 9);
            d.initDisplay(g);
            g.genereActionsPossibles();
        }

        private void Update()
        {
            if (depart != null && c2 != null)
            {
                Action a = g.realiseActionCases(depart, c2);
                if (a != null)
                {
                    d.display(a);
                    if (g.partieFinie())
                    {
                        switch (g.resultatBlanc())
                        {
                            case -1: Debug.Log("DEFAITE"); break;
                            case 0: Debug.Log("NUL"); break;
                            case 1: Debug.Log("VICTOIRE"); break;
                        }
                    }
                    else
                    {
                        depart = c2 = null;
                        d.display(g.realiserAction(j1.choisirAction()));
                        if (g.partieFinie())
                        {
                            switch (g.resultatBlanc())
                            {
                                case -1: Debug.Log("DEFAITE"); break;
                                case 0: Debug.Log("NUL"); break;
                                case 1: Debug.Log("VICTOIRE"); break;
                            }
                        }
                    }
                }
                depart = c2 = null;
            }
        }

        public void selectCase(int l, int c)
        {
            if (depart == null)
                depart = Grille.CASES[l, c];
            else
                c2 = Grille.CASES[l, c];
        }

    }

    public abstract class Joueur
    {
        protected Grille g;

        public Joueur(Grille g)
        {
            this.g = g;
        }

        public abstract Action choisirAction();


    }

    public class JoueurAléatoire : Joueur
    {
        public JoueurAléatoire(Grille g) : base(g) { }

        public override Action choisirAction()
        {
            return g.actionsPossibles[(int)(Random.value * g.actionsPossibles.Count)];
        }


    }



}