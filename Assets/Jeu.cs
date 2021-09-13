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
            jeuEnCours = this;
            depart = null;
            c2 = null;
            d = GameObject.FindGameObjectWithTag("Damier").GetComponent<Damier>();
            g = new Grille();
            j1 = new JoueurAléatoire(false, g);
            d.initDisplay(g);
            g.genereActionsPossibles();
        }

        public void selectCase(int l, int c)
        {
            if (depart == null)
                depart = Grille.CASES[l, c];
            else
            {
                c2 = Grille.CASES[l, c];
                d.display(g.realiseActionCases(depart, c2));
                depart = c2 = null;
                tourIA();
            }
        }

        void tourIA()
        {
            g.genereActionsPossibles();
            d.display(g.realiserAction(j1.choisirAction()));
        }

    }

    public abstract class Joueur
    {
        bool blanc;
        protected Grille g;

        public Joueur(bool blanc, Grille g)
        {
            this.blanc = blanc;
            this.g = g;
        }

        public abstract Action choisirAction();

    }

    public class JoueurAléatoire : Joueur
    {
        public JoueurAléatoire(bool b, Grille g) : base(b, g) { }

        public override Action choisirAction()
        {
            return g.actionsPossibles[(int)(Random.value * g.actionsPossibles.Count)];
        }

    }

    

}