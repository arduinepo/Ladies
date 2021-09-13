using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    //1 : passer en matrice 1D sans les cases injouables
    //Collection de tous les prises uniques par position

    public class Grille
    {
        #region properties
        protected static int[,] GRILLE_10 = new int[10, 10] {
            {0,-1,0,-1,0,-1,0,-1,0,-1},
            {-1,0,-1,0,-1,0,-1,0,-1,0},
            {0,-1,0,-1,0,-1,0,-1,0,-1},
            {-1,0,-1,0,-1,0,-1,0,-1,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0},
            {0,1,0,1,0,1,0,1,0,1},
            {1,0,1,0,1,0,1,0,1,0},
            {0,1,0,1,0,1,0,1,0,1},
            {1,0,1,0,1,0,1,0,1,0}
        };
        public static Case[,] CASES = new Case[10, 10];
        static Mouvement[,,] MOUVEMENTS_PIONS = new Mouvement[10, 10, 4];
        static Dictionary<int, Mouvement>[,,] MOUVEMENTS_DAMES = new Dictionary<int, Mouvement>[10, 10, 2];
        static List<int>[] DIRS = new List<int>[4]{
            new int[3]{0,1,3 }.ToList(),new int[3]{1,2,0}.ToList(),new int[3]{2,3,1}.ToList(),new int[3]{3,0,2}.ToList()
        };

        const int HAUT_GAUCHE = 0, HAUT_DROIT = 1, BAS_DROIT = 2, BAS_GAUCHE = 3;
        const int HAUT = -1, GAUCHE = -1, BAS = 1, DROITE = 1;
        public const int PION_BLANC = 1, PION_NOIR = -1, DAME_BLANC = 2, DAME_NOIR = -2, VIDE = 0;
        const bool TOUR_BLANCS = true;
        protected const int NUL = 0, VICTOIRE_BLANC = -1, VICTOIRE_NOIR = 1;

        public int[,] grille;
        public int taille;
        //public List<Case> pionsBlancs, pionsNoirs, damesBlancs, damesNoirs;
        protected bool tour;
        public int nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;

        public List<Action> actionsPossibles;
        private int maxPionsPris;
        private Queue<Prise> prisesEnCours;
        List<Prise> prisesEtendues;
        Queue<PriseDame> prisesDamesEnCours;
        List<PriseDame> prisesDamesEtendues;
        #endregion

        static Grille()
        {
            for (byte i = 0; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                    CASES[i, j] = new Case(i, j);

            for (byte i = 0; i < 9; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if (i < 8)
                    {
                        if (j > 0)
                            MOUVEMENTS_PIONS[i, j, BAS_GAUCHE] = new MouvementNoir(CASES[i, j], CASES[i + 1, j - 1]);
                        if (j < 9)
                            MOUVEMENTS_PIONS[i, j, BAS_DROIT] = new MouvementNoir(CASES[i, j], CASES[i + 1, j + 1]);
                    }
                    else
                    {
                        if (j > 0)
                            MOUVEMENTS_PIONS[i, j, BAS_GAUCHE] = new ArriveeNoir(CASES[i, j], CASES[i + 1, j - 1]);
                        if (j < 9)
                            MOUVEMENTS_PIONS[i, j, BAS_DROIT] = new ArriveeNoir(CASES[i, j], CASES[i + 1, j + 1]);
                    }
                }
            for (byte i = 1; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if (i > 0)
                    {
                        if (j > 0)
                            MOUVEMENTS_PIONS[i, j, HAUT_GAUCHE] = new MouvementBlanc(CASES[i, j], CASES[i - 1, j - 1]);
                        if (j < 9)
                            MOUVEMENTS_PIONS[i, j, HAUT_DROIT] = new MouvementBlanc(CASES[i, j], CASES[i - 1, j + 1]);
                    }
                    else
                    {
                        if (j > 0)
                            MOUVEMENTS_PIONS[i, j, HAUT_GAUCHE] = new ArriveeBlanc(CASES[i, j], CASES[i - 1, j - 1]);
                        if (j < 9)
                            MOUVEMENTS_PIONS[i, j, HAUT_DROIT] = new ArriveeBlanc(CASES[i, j], CASES[i - 1, j + 1]);
                    }

                }
            for (byte i = 0; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    MOUVEMENTS_DAMES[i, j, 0] = new Dictionary<int, Mouvement>(20);
                    MOUVEMENTS_DAMES[i, j, 1] = new Dictionary<int, Mouvement>(20);
                    for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                    {
                        for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos);
                            ligne >= 0 && ligne < 10 && col >= 0 && col < 10;
                            ligne = getLigneVoisine(ligne, pos), col = getColonneVoisine(col, pos))
                        {
                            MOUVEMENTS_DAMES[i, j, 0].Add(ligne * 10 + col, new MouvementBlanche(CASES[i, j], CASES[ligne, col]));
                            MOUVEMENTS_DAMES[i, j, 1].Add(ligne * 10 + col, new MouvementNoire(CASES[i, j], CASES[ligne, col]));
                        }
                    }
                }
        }

        public Grille()
        {
            grille = (int[,])GRILLE_10.Clone();
            taille = 10;
            tour = TOUR_BLANCS;
            /*
            pionsBlancs = new List<Case>(20);
            pionsNoirs = new List<Case>(20);
            damesBlancs = new List<Case>(10);
            damesNoirs = new List<Case>(10);
            for (byte i = 0; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if (j < 4)
                        pionsNoirs[nbPionsNoirs++] = CASES[i, j];
                    if (j > 5)
                        pionsBlancs[nbPionsBlancs++] = CASES[i, j];
                }
            */
            nbPionsBlancs = nbPionsNoirs = 20;
            nbDamesBlancs = nbDamesNoirs = 0;
        }

        public Grille(Grille g)
        {
            grille = (int[,])g.grille.Clone();
            taille = 10;
            tour = g.tour;
            nbPionsBlancs = g.nbPionsBlancs;
            nbPionsNoirs = g.nbPionsNoirs;
            nbDamesBlancs = g.nbDamesBlancs;
            nbDamesNoirs = g.nbDamesNoirs;
        }

        public void genereActionsPossibles()
        {
            actionsPossibles = new List<Action>();
            prises();
            if (actionsPossibles.Count == 0)
                mouvementsPossibles();
        }

        public Action realiseActionCases(Case c1, Case c2)
        {
            foreach (Action a in actionsPossibles)
                if (a.commenceEtPasse(c1, c2))
                    return realiserAction(a);
            return null;
        }

        /* Réalise l'action a, et promeut le pion en dame si nécessaire. Bascule le tour des joueurs.*/
        public Action realiserAction(Action a)
        {
            a.realiser(this);
            tour = !this.tour;
            return a;
        }

        protected byte nombrePionsJoueur(bool blancs)
        {
            return (byte)(blancs ? nbPionsBlancs + nbDamesBlancs : nbPionsNoirs + nbDamesNoirs);
        }

        /* Renvoie VICTOIRE_BLANC/NOIR, si tous les pions noirs/blancs ont été pris ou ne peuvent pas se déplacer ;
            renvoie NUL si aucun des deux joueurs ne peut déplacer un seul pion. */
        protected short resultat()
        {
            if (nbPionsNoirs + nbDamesNoirs == 0)
                return VICTOIRE_BLANC;
            else if (nbPionsBlancs + nbDamesBlancs == 0)
                return VICTOIRE_NOIR;
            else
            {
                if (actionsPossibles != null && actionsPossibles.Count == 0)
                {
                    tour = !tour;
                    genereActionsPossibles();
                    if (actionsPossibles != null && actionsPossibles.Count == 0)
                        return NUL;
                    if (tour)
                        return VICTOIRE_BLANC;
                    return VICTOIRE_NOIR;
                }
            }
            return NUL;
        }

        protected bool partieFinie()
        {
            return (nbPionsBlancs == 0 && nbDamesBlancs == 0) || (nbPionsNoirs == 0 && nbDamesNoirs == 0)
                || (actionsPossibles != null && actionsPossibles.Count == 0);
        }

        protected bool joueurAGagne(bool joueurBlanc)
        {
            return (joueurBlanc ? (nbPionsNoirs == 0 && nbDamesNoirs == 0)
                : (nbPionsBlancs == 0 && nbDamesBlancs == 0))
                || (joueurBlanc != tour && actionsPossibles.Count == 0);
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
        * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
        */
        void mouvementsPossibles()
        {
            if (tour == TOUR_BLANCS)
            {
                for (int i = 0; i < taille; i += 2)
                    for (int j = 1; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_BLANC)
                        {
                            int pos = HAUT_GAUCHE;
                            byte l = getLigneVoisine(i, pos), co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            pos = HAUT_DROIT;
                            l = getLigneVoisine(i, pos);
                            co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                        }
                        else if (grille[i, j] == DAME_BLANC)
                        {
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); ligne >= 0 && ligne < taille && col >= 0 && col < taille
                                                                && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos), col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j, 0][ligne * 10 + col]);
                        }
                    }
                for (int i = 1; i < taille; i += 2)
                    for (int j = 0; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_BLANC)
                        {
                            int pos = HAUT_GAUCHE;
                            byte l = getLigneVoisine(i, pos), co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            pos = HAUT_DROIT;
                            l = getLigneVoisine(i, pos);
                            co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                        }
                        else if (grille[i, j] == DAME_BLANC)
                        {
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); ligne >= 0 && ligne < taille && col >= 0 && col < taille
                                                                && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos), col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j, 0][ligne * 10 + col]);
                        }
                    }
            }
            else
            {
                for (int i = 0; i < taille; i += 2)
                    for (int j = 1; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_NOIR)
                        {
                            int pos = BAS_GAUCHE;
                            byte l = getLigneVoisine(i, pos), co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            pos = BAS_DROIT;
                            l = getLigneVoisine(i, pos);
                            co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                        }
                        else if (grille[i, j] == DAME_NOIR)
                        {
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); ligne >= 0 && ligne < taille && col >= 0 && col < taille
                                                                && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos), col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j, 0][ligne * 10 + col]);
                        }
                    }
                for (int i = 1; i < taille; i += 2)
                    for (int j = 0; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_NOIR)
                        {
                            int pos = BAS_GAUCHE;
                            byte l = getLigneVoisine(i, pos), co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            pos = BAS_DROIT;
                            l = getLigneVoisine(i, pos);
                            co = getColonneVoisine(j, pos);
                            if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                                actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                        }
                        else if (grille[i, j] == DAME_NOIR)
                        {
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); ligne >= 0 && ligne < taille && col >= 0 && col < taille
                                                                && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos), col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j, 0][ligne * 10 + col]);
                        }
                    }

                /*
                foreach (Case c in pionsNoirs)
                {
                    short pos = BAS_GAUCHE;
                    byte l = getLigneVoisine(c.ligne, pos), co = getColonneVoisine(c.colonne, pos);
                    if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                            actionsPossibles.Add(MOUVEMENTS_PIONS[l, co, pos]);
                    pos = BAS_DROIT;
                    l = getLigneVoisine(c.ligne, pos);
                    co = getColonneVoisine(c.colonne, pos);
                    if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                        actionsPossibles.Add(MOUVEMENTS_PIONS[l, co, pos]);
                }
                foreach (Case c in damesNoirs)
                {
                    for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                        for (byte ligne = getLigneVoisine(c.ligne, pos), col = getColonneVoisine(c.colonne, pos);
                            ligne >= 0 && ligne < taille && col >= 0 && col < taille
                            && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos),
                                col = getColonneVoisine(col, pos))
                            actionsPossibles.Add(MOUVEMENTS_DAMES[c.ligne, c.colonne,1][ligne * 10 + col]);
                }
                */
            }
        }

        /* Génère toutes les prises possibles de tous les pions du joueur qui a le tour. */
        void prises()
        {
            prisesEnCours = new Queue<Prise>();
            prisesDamesEnCours = new Queue<PriseDame>();
            prisesEtendues = new List<Prise>();
            prisesDamesEtendues = new List<PriseDame>();
            maxPionsPris = 0;

            if (tour == TOUR_BLANCS)
            {
                for (byte i = 0; i < taille; i += 2)
                    for (byte j = 1; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_BLANC)
                            prisesPionBlanc(i, j);
                        if (grille[i, j] == DAME_BLANC)
                            prisesDameBlanc(i, j);
                    }
                for (byte i = 1; i < taille; i += 2)
                    for (byte j = 0; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_BLANC)
                            prisesPionBlanc(i, j);
                        if (grille[i, j] == DAME_BLANC)
                            prisesDameBlanc(i, j);
                    }
            }
            else
            {
                for (byte i = 0; i < taille; i += 2)
                    for (byte j = 1; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_NOIR)
                            prisesPionNoir(i, j);
                        if (grille[i, j] == DAME_NOIR)
                            prisesDameNoir(i, j);
                    }
                for (byte i = 1; i < taille; i += 2)
                    for (byte j = 0; j < taille; j += 2)
                    {
                        if (grille[i, j] == PION_NOIR)
                            prisesPionNoir(i, j);
                        if (grille[i, j] == DAME_NOIR)
                            prisesDameNoir(i, j);
                    }
            }
            int n;
            foreach (Prise p in prisesEtendues)
            {
                n = p.nombrePionsPris();
                if (n > maxPionsPris)
                    maxPionsPris = n;
            }
            foreach (Prise p in prisesDamesEtendues)
            {
                n = p.nombrePionsPris();
                if (n > maxPionsPris)
                    maxPionsPris = n;
            }
            foreach (Prise p in prisesEtendues)
                if (p.nombrePionsPris() == maxPionsPris)
                    actionsPossibles.Add(p);
            foreach (Prise p in prisesDamesEtendues)
                if (p.nombrePionsPris() == maxPionsPris)
                    actionsPossibles.Add(p);
            triePrisesDames();
        }

        void prisesPionBlanc(int lignePion, int colonnePion)
        {
            Prise p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                int ligneAdjacente = getLigneVoisine(lignePion, position), colAdjacente = getColonneVoisine(colonnePion, position);
                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] < 0)
                {
                    int ligneArrivee = getLigneVoisine(ligneAdjacente, position), colArrivee = getColonneVoisine(colAdjacente, position);
                    if (grille[ligneArrivee, colArrivee] == VIDE)
                    {
                        prisesEnCours.Enqueue(new PriseBlanc(CASES[lignePion, colonnePion], CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee],position));
                        while (prisesEnCours.Count > 0)
                        {
                            p2 = prisesEnCours.Dequeue();
                            int lA = p2.cases[p2.cases.Count - 1].ligne;
                            int cA = p2.cases[p2.cases.Count - 1].colonne;
                            bool poursuivie = false;
                            int i = 0;
                            int dir = p2.lastDir;
                            for (; i < 3; i++)
                            {
                                int pos2 = DIRS[dir][i];
                                ligneAdjacente = getLigneVoisine(lA, pos2);
                                colAdjacente = getColonneVoisine(cA, pos2);
                                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] < 0 && !p2.pionVirtuellementPris(ligneAdjacente, colAdjacente))
                                {
                                    ligneArrivee = getLigneVoisine(ligneAdjacente, pos2);
                                    colArrivee = getColonneVoisine(colAdjacente, pos2);
                                    if (grille[ligneArrivee, colArrivee] == VIDE || (ligneArrivee == p2.cases[0].ligne && colArrivee == p2.cases[0].colonne))
                                    {
                                        if (!poursuivie)
                                        {
                                            p2.poursuit(CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], pos2);
                                            prisesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else prisesEnCours.Enqueue(new PriseBlanc(p2, CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], pos2));
                                    }
                                }
                            }
                            if (!poursuivie)
                                prisesEtendues.Add(p2);
                        }
                    }
                }
            }
        }

        void prisesPionNoir(byte lignePion, byte colPion)
        {
            Prise p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                int ligneAdjacente = getLigneVoisine(lignePion, position), colAdjacente = getColonneVoisine(colPion, position);
                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] > 0)
                {
                    int ligneArrivee = getLigneVoisine(ligneAdjacente, position), colArrivee = getColonneVoisine(colAdjacente, position);
                    if (grille[ligneArrivee, colArrivee] == VIDE)
                    {
                        prisesEnCours.Enqueue(new PriseNoir(CASES[lignePion, colPion], CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], position));
                        while (prisesEnCours.Count > 0)
                        {
                            p2 = prisesEnCours.Dequeue();
                            int lA = p2.cases[p2.cases.Count - 1].ligne;
                            int cA = p2.cases[p2.cases.Count - 1].colonne;
                            bool poursuivie = false;
                            int i = 0;
                            int dir = p2.lastDir;
                            for (; i < 3; i++)
                            {
                                int pos2 = DIRS[dir][i];
                                ligneAdjacente = getLigneVoisine(lA, pos2);
                                colAdjacente = getColonneVoisine(cA, pos2);
                                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] > 0 && !p2.pionVirtuellementPris(ligneAdjacente, colAdjacente))
                                {
                                    ligneArrivee = getLigneVoisine(ligneAdjacente, pos2);
                                    colArrivee = getColonneVoisine(colAdjacente, pos2);
                                    if (grille[ligneArrivee, colArrivee] == VIDE || (ligneArrivee == p2.cases[0].ligne && colArrivee == p2.cases[0].colonne))
                                    {
                                        if (!poursuivie) {
                                            p2.poursuit(CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], pos2);
                                            prisesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else prisesEnCours.Enqueue(new PriseNoir(p2, CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], pos2));
                                    }
                                }
                            }
                            if (!poursuivie)
                                prisesEtendues.Add(p2);
                        }
                    }
                }
            }
        }

        void prisesDameBlanc(byte lignePion, byte colPion)
        {
            Case adv = null;
            PriseDame p1, p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                p1 = null;
                bool premierPionRencontre = false, secondPionRencontre = false;
                bool premiereArrivee = false;
                for (byte l2 = getLigneVoisine(lignePion, position), c2 = getColonneVoisine(colPion, position);
                    l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && !secondPionRencontre;
                    l2 = getLigneVoisine(l2, position), c2 = getColonneVoisine(c2, position))
                {
                    if (grille[l2, c2] < 0)
                    {
                        if (!premierPionRencontre)
                        {
                            premierPionRencontre = true;
                            adv = CASES[l2, c2];
                        }
                        else secondPionRencontre = true;
                    }
                    else if (grille[l2, c2] == 0)
                    {
                        if (premierPionRencontre)
                        {
                            if (!premiereArrivee)
                            {
                                p1 = new PriseBlanche(CASES[lignePion, colPion], adv, CASES[l2, c2],position);
                                premiereArrivee = true;
                                prisesDamesEnCours.Enqueue(p1);
                            }
                            else prisesDamesEnCours.Enqueue(new PriseBlanche(p1, CASES[l2, c2]));
                            
                        }
                    }
                    else secondPionRencontre = true;
                }

                while (prisesDamesEnCours.Count > 0)
                {
                    p2 = prisesDamesEnCours.Dequeue();
                    bool poursuivie = false;
                    int lA  = p2.cases[p2.cases.Count - 1].ligne;
                    int cA  = p2.cases[p2.cases.Count - 1].colonne;
                    int lastDir = p2.lastDir;
                    foreach (int pos2 in DIRS[lastDir])
                    {
                        byte l2 = getLigneVoisine(lA, pos2), c2 = getColonneVoisine(cA, pos2);
                        premierPionRencontre = false;
                        secondPionRencontre = false;
                        premiereArrivee = false;
                        for (;l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && !secondPionRencontre;
                            l2 = getLigneVoisine(l2, pos2), c2 = getColonneVoisine(c2, pos2))
                        {
                            if (grille[l2, c2] < 0)
                            {
                                if (!premierPionRencontre)
                                {
                                    if (!p2.pionVirtuellementPris(l2, c2))
                                    {
                                        premierPionRencontre = true;
                                        adv = CASES[l2, c2];
                                    }
                                    else secondPionRencontre = true;
                                }
                                else secondPionRencontre = true;
                            }
                            else if (grille[l2, c2] == 0|| (l2==p2.cases[0].ligne&&c2==p2.cases[0].colonne))
                            {
                                if (premierPionRencontre)
                                {
                                    if (!premiereArrivee)
                                    {
                                        if (!poursuivie)
                                        {
                                            p2.poursuit(adv, CASES[l2, c2], pos2);
                                            prisesDamesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else prisesDamesEnCours.Enqueue(new PriseBlanche(p2, adv, CASES[l2, c2], pos2));
                                        premiereArrivee = true;
                                        
                                    }
                                    else prisesDamesEnCours.Enqueue(new PriseBlanche(p2, adv, CASES[l2, c2], pos2));
                                }
                            }
                            else secondPionRencontre = true;
                        }
                    }
                    if (!poursuivie)
                        prisesDamesEtendues.Add(p2);
                }
            }
        }

        void prisesDameNoir(byte lignePion, byte colPion)
        {
            Case adv = null;
            PriseDame p1, p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                p1 = null;
                bool premierPionRencontre = false, secondPionRencontre = false;
                bool premiereArrivee = false;
                for (byte l2 = getLigneVoisine(lignePion, position), c2 = getColonneVoisine(colPion, position);
                    l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && !secondPionRencontre;
                    l2 = getLigneVoisine(l2, position), c2 = getColonneVoisine(c2, position))
                {
                    if (grille[l2, c2] > 0)
                    {
                        if (!premierPionRencontre)
                        {
                            premierPionRencontre = true;
                            adv = CASES[l2, c2];
                        }
                        else secondPionRencontre = true;
                    }
                    else if (grille[l2, c2] == 0)
                    {
                        if (premierPionRencontre)
                        {
                            if (!premiereArrivee)
                            {
                                p1 = new PriseNoire(CASES[lignePion, colPion], adv, CASES[l2, c2], position);
                                premiereArrivee = true;
                                prisesDamesEnCours.Enqueue(p1);
                            }
                            else prisesDamesEnCours.Enqueue(new PriseNoire(p1, CASES[l2, c2]));

                        }
                    }
                    else secondPionRencontre = true;
                }

                while (prisesDamesEnCours.Count > 0)
                {
                    p2 = prisesDamesEnCours.Dequeue();
                    bool poursuivie = false;
                    int lA = p2.cases[p2.cases.Count - 1].ligne;
                    int cA = p2.cases[p2.cases.Count - 1].colonne;
                    int lastDir = p2.lastDir;
                    foreach (int pos2 in DIRS[lastDir])
                    {
                        byte l2 = getLigneVoisine(lA, pos2), c2 = getColonneVoisine(cA, pos2);
                        premierPionRencontre = false;
                        secondPionRencontre = false;
                        premiereArrivee = false;
                        for (; l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && !secondPionRencontre;
                            l2 = getLigneVoisine(l2, pos2), c2 = getColonneVoisine(c2, pos2))
                        {
                            if (grille[l2, c2] > 0)
                            {
                                if (!premierPionRencontre)
                                {
                                    if (!p2.pionVirtuellementPris(l2, c2))
                                    {
                                        premierPionRencontre = true;
                                        adv = CASES[l2, c2];
                                    }
                                    else secondPionRencontre = true;
                                }
                                else secondPionRencontre = true;
                            }
                            else if (grille[l2, c2] == 0 || (l2 == p2.cases[0].ligne && c2 == p2.cases[0].colonne))
                            {
                                if (premierPionRencontre)
                                {
                                    if (!premiereArrivee)
                                    {
                                        if (!poursuivie)
                                        {
                                            p2.poursuit(adv, CASES[l2, c2], pos2);
                                            prisesDamesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else prisesDamesEnCours.Enqueue(new PriseNoire(p2, adv, CASES[l2, c2], pos2));
                                        premiereArrivee = true;

                                    }
                                    else prisesDamesEnCours.Enqueue(new PriseNoire(p2, adv, CASES[l2, c2], pos2));
                                }
                            }
                            else secondPionRencontre = true;
                        }
                        
                    }
                    if (!poursuivie)
                        prisesDamesEtendues.Add(p2);
                }
            }
        }

        /* Supprime les prises "doublons" : les prises du même pion preneur, prenant les mêmes pions adverses dans le même ordre, se terminant sur la même case, et caractérisées par des cases étapes/intermédiaires différentes. */
        void triePrisesDames()
        {
            List<PriseDame> prises = new List<PriseDame>();
            for (int i = 0; i < prisesDamesEtendues.Count - 1; i++)
            {
                for (int j = actionsPossibles.Count - 1; j > i; j--)
                    if (prisesDamesEtendues[i].prendMemePionsMemeOrdre(prisesDamesEtendues[j]))
                        prises.Add(prisesDamesEtendues[j]);
            }
            foreach (PriseDame p in prises)
                prisesDamesEtendues.Remove(p);
        }

        const byte ERR = 101;

        Case caseVoisine(Case c, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE): return CASES[c.ligne - 1, c.colonne - 1];
                case (HAUT_DROIT): return CASES[c.ligne - 1, c.colonne + 1];
                case (BAS_DROIT): return CASES[c.ligne + 1, c.colonne + 1];
                case (BAS_GAUCHE): return CASES[c.ligne + 1, c.colonne - 1];
            }
            return null;
        }

        static byte getLigneVoisine(int ligne, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                case (HAUT_DROIT):
                    return (byte)(ligne + HAUT);
                case (BAS_DROIT):
                case (BAS_GAUCHE):
                    return (byte)(ligne + BAS);
            }
            return ERR;
        }

        static byte getColonneVoisine(int col, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                case (BAS_GAUCHE):
                    return (byte)(col + GAUCHE);
                case (HAUT_DROIT):
                case (BAS_DROIT):
                    return (byte)(col + DROITE);
            }
            return ERR;
        }

        public override String ToString()
        {
            StringBuilder s = new StringBuilder(300);
            for (byte i = 0; i < grille.Length; i++)
            {
                for (byte j = 0; j < grille.Length; j++)
                {
                    switch (grille[i, j])
                    {
                        case (DAME_BLANC):
                            s.Append(" B ");
                            break;
                        case (PION_BLANC):
                            s.Append(" b ");
                            break;
                        case (VIDE):
                            s.Append(" - ");
                            break;
                        case (PION_NOIR):
                            s.Append(" n ");
                            break;
                        case (DAME_NOIR):
                            s.Append(" N ");
                            break;
                    }
                }
                s.Append("\n");
            }
            return s.ToString();
        }

    }

}