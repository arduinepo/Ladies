using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class IAlphaBeta : Joueur
    {
        public static IAlphaBeta ia;
        Heuristique heuristique;
        int profondeur;
        public const int MAX = Int32.MaxValue, MIN = Int32.MinValue;
        static float rand;
        public static Thread recherche;
        public static Action todo;

        public IAlphaBeta(Grille g, Jeu j, Heuristique h, int p) : base(g, j)
        {
            heuristique = h;
            profondeur = p;
            ia = this;
        }

        public override void choisitAction()
        {
            recherche = new Thread(rechercheAction);
            rand = UnityEngine.Random.value;
            recherche.Start();
        }

        public static void rechercheAction()
        {
            todo = null;
            Grille g = ia.g;
            Heuristique heuristique = ia.heuristique;
            int profondeur = ia.profondeur;
            if (g.actionsPossibles.Count == 1)
                todo = g.actionsPossibles[0];
            else
            {
                int alpha = MIN;
                List<Action> bestActions = new List<Action>(g.actionsPossibles.Count);
                foreach (Action a in g.actionsPossibles)
                {
                    int valeurFils = new NoeudMin(g, a).alphaBeta(profondeur - 1, heuristique, alpha, MAX);
                    if (valeurFils > alpha)
                    {
                        alpha = valeurFils;
                        bestActions.Clear();
                        bestActions.Add(a);
                    }
                    else if (valeurFils == alpha)
                        bestActions.Add(a);
                }
                todo = bestActions[(int)(rand * bestActions.Count)];
            }
            Debug.Log("recherche finie");
        }

    }

    public abstract class Noeud : Grille
    {
        public int valeur;
        public Noeud[] suivants;

        public Noeud(Grille g, Action a) : base(g, a) { }
        public abstract int alphaBeta(int p, Heuristique h, int a, int b);
    }

    public class NoeudMax : Noeud
    {
        public NoeudMax(Grille g, Action a) : base(g, a)
        {
            valeur = IAlphaBeta.MIN;
        }

        public override int alphaBeta(int profondeur, Heuristique h, int alpha, int beta)
        {
            if (partieFinie())
                return h.evalueFin(this);
            if (profondeur > 0)
            {
                suivants = new Noeud[actionsPossibles.Count];
                for (int i = 0; i < actionsPossibles.Count; ++i)
                {
                    Noeud n = new NoeudMin(this, actionsPossibles[i]);
                    suivants[i] = n;
                    int vFils = n.alphaBeta(profondeur - 1, h, alpha, beta);
                    if (vFils > valeur)
                        valeur = vFils;
                    if (beta <= valeur)
                        return valeur;
                    if (alpha < valeur)
                        alpha = valeur;
                }
                return valeur;
            }
            return h.evalue(this);
        }

    }

    public class NoeudMin : Noeud
    {
        public NoeudMin(Grille g, Action a) : base(g, a)
        {
            valeur = IAlphaBeta.MAX;
        }

        public override int alphaBeta(int profondeur, Heuristique h, int alpha, int beta)
        {
            if (partieFinie())
                return h.evalueFin(this);
            if (profondeur > 0)
            {
                suivants = new Noeud[actionsPossibles.Count];
                for (int i = 0; i < actionsPossibles.Count; ++i)
                {
                    Noeud n = new NoeudMax(this, actionsPossibles[i]);
                    suivants[i] = n;
                    int vFils = n.alphaBeta(profondeur - 1, h, alpha, beta);
                    if (vFils < valeur)
                        valeur = vFils;
                    if (alpha >= valeur)
                        return valeur;
                    if (beta > valeur)
                        beta = valeur;
                }
                return valeur;
            }
            return h.evalue(this);
        }

    }

}
