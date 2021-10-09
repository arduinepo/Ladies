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
        public bool tourIA = false, displayEnded = false;

        void Awake()
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().transform.LookAt(new Vector3(4.5f, 0, 4.5f));
            jeuEnCours = this;
            depart = null;
            c2 = null;
            d = GameObject.FindGameObjectWithTag("Damier").GetComponent<Damier>();
            g = new Grille();
            j1 = new IAlphaBeta(g, this, new BaseN(1, 3), 9);
            d.initDisplay(g);
            g.genereActionsPossibles();
        }

        void Update()
        {
            if (!tourIA && depart != null && c2 != null)
            {
                Action a = g.realiseActionCases(depart, c2);
                if (a != null)
                {
                    d.display(a);
                    displayEnded = false;
                    tourIA = true;
                    if (g.partieFinie())
                        switch (g.resultatBlanc())
                        {
                            case -1: Debug.Log("DEFAITE"); break;
                            case 0: Debug.Log("NUL"); break;
                            case 1: Debug.Log("VICTOIRE"); break;
                        }
                    else j1.choisitAction();
                }
                depart = c2 = null;
            }
            if (tourIA && !IAlphaBeta.recherche.IsAlive && displayEnded)
            {
                d.display(g.realiserAction(IAlphaBeta.todo));
                displayEnded = false;
                tourIA = false;
                if (g.partieFinie())
                {
                    switch (g.resultatBlanc())
                    {
                        case -1: Debug.Log("DEFAITE"); break;
                        case 0: Debug.Log("NUL"); break;
                        case 1: Debug.Log("VICTOIRE"); break;
                    }
                }
                IAlphaBeta.todo = null;
            }
        }

        public void actionne(Action a)
        {
            if (tourIA  && displayEnded)
            {
                d.display(g.realiserAction(IAlphaBeta.todo));
                displayEnded = false;
                tourIA = false;
                if (g.partieFinie())
                {
                    switch (g.resultatBlanc())
                    {
                        case -1: Debug.Log("DEFAITE"); break;
                        case 0: Debug.Log("NUL"); break;
                        case 1: Debug.Log("VICTOIRE"); break;
                    }
                }
                IAlphaBeta.todo = null;
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
        protected Jeu j;

        public Joueur(Grille g, Jeu jeu)
        {
            this.g = g;
            j = jeu;
        }

        public abstract void choisitAction();

    }

    public class JoueurAléatoire : Joueur
    {
        public JoueurAléatoire(Grille g, Jeu j) : base(g, j) { }

        public override void choisitAction()
        {
            j.actionne(g.actionsPossibles[(int)(Random.value * g.actionsPossibles.Count)]);
        }

    }

}
