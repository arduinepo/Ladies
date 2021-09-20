using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class IAlphaBeta : Joueur
    {
        Heuristique heuristique;
        int profondeur;
        public const int MAX = Int32.MaxValue, MIN = Int32.MinValue;

        public IAlphaBeta(Grille g,Heuristique h,int p) : base(g)
        {
            heuristique = h;
            profondeur = p;
        }

        public override Action choisirAction()
        {
            if (g.actionsPossibles.Count == 1)
                return g.actionsPossibles[0];
            int alpha = MIN;
            List<Action> bestActions = new List<Action>(g.actionsPossibles.Count);
            int i = 0;
            foreach (Action a in g.actionsPossibles)
            {
                int valeurFils = new NoeudMin(g,a).alphaBeta(profondeur-1, heuristique, alpha, MAX);
                if (valeurFils > alpha)
                {
                    alpha = valeurFils;
                    bestActions.Clear();
                    bestActions.Add(a);
                }
                else if (valeurFils == alpha)
                    bestActions.Add(a);
            }
            return bestActions[(int)(UnityEngine.Random.value*bestActions.Count)];
        }


    }

    public abstract class Noeud : Grille
    {
        public int valeur;
        public List<Noeud> suivants;

        public Noeud(Grille g, Action a) : base(g, a) { }
        public abstract int alphaBeta(int p, Heuristique h, int a, int b);
    }

    public class NoeudMax:Noeud
    {
        public NoeudMax(Grille g,Action a):base(g,a)
        {
            valeur = IAlphaBeta.MIN;
        }

        public override int alphaBeta(int profondeur, Heuristique h, int alpha, int beta)
        {
            if (partieFinie())
                return h.evalueFin(this);
            if (profondeur > 0)
            {
                suivants = new List<Noeud>(actionsPossibles.Count);
                foreach (Action ac in actionsPossibles)
                {
                    Noeud n = new NoeudMin(this, ac);
                    suivants.Add(n);
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

    public class NoeudMin:Noeud
    {
        public NoeudMin(Grille g,Action a):base(g,a)
        {
            valeur = IAlphaBeta.MAX;
        }

        public override int alphaBeta(int profondeur,Heuristique h, int alpha, int beta)
        {
            if (partieFinie())
                return h.evalueFin(this);
            if (profondeur > 0)
            {
                suivants = new List<Noeud>(actionsPossibles.Count);
                foreach (Action ac in actionsPossibles)
                {
                    Noeud n = new NoeudMax(this, ac);
                    suivants.Add(n);
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
