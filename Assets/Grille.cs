using System;
using System.Collections.Generic;
using System.Text;

namespace Assets
{
    //Collection de tous les mouvements et prises uniques par pion possibles

    //refaire géné prises
    //doubler méthodes de recherche d'action suivant noirs ou blancs pour diminuer nombre de if
    //1 : passer en matrice 1D
    //2 : passer en matrice 1D sans les cases injouables

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
        static Dictionary<int, Mouvement>[,] MOUVEMENTS_DAMES = new Dictionary<int, Mouvement>[10, 10];

        const int HAUT_GAUCHE = 0, HAUT_DROIT = 1, BAS_DROIT = 2, BAS_GAUCHE = 3;
        const int HAUT = -1, GAUCHE = -1, BAS = 1, DROITE = 1;
        public const int PION_BLANC = 1, PION_NOIR = -1, DAME_BLANC = 2, DAME_NOIR = -2, VIDE = 0;
        const bool TOUR_BLANCS = true;
        protected const int NUL = 0, VICTOIRE_BLANC = -1, VICTOIRE_NOIR = 1;

        public int[,] grille;
        public int taille;
        public List<Case> pionsBlancs, pionsNoirs, damesBlancs, damesNoirs;
        protected bool tour;
        public int nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;

        public List<Action> actionsPossibles;
        private int maxPionsPris;
        private Queue<Prise> prisesEnCours, prisesEtendues;
        private Queue<PriseDame>prisesDamesEnCours, prisesDamesEtendues;
        #endregion

        static Grille()
        {
            for (byte i = 0; i < 9; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if(j>0)
                        MOUVEMENTS_PIONS[i, j, BAS_GAUCHE] = new Mouvement(i, j, (byte)(i + 1), (byte)(j - 1));
                    if(j<9)
                        MOUVEMENTS_PIONS[i, j, BAS_DROIT] = new Mouvement(i, j, (byte)(i + 1), (byte)(j + 1));
                }
            for (byte i = 1; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if (j > 0)
                        MOUVEMENTS_PIONS[i, j, HAUT_GAUCHE] = new Mouvement(i, j, (byte)(i + 1), (byte)(j - 1));
                    if (j < 9)
                        MOUVEMENTS_PIONS[i, j, HAUT_DROIT] = new Mouvement(i, j, (byte)(i + 1), (byte)(j + 1));
                }
            for (byte i = 0; i < 10; i++)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    CASES[i, j] = new Case(i, j);
                    MOUVEMENTS_DAMES[i, j] = new Dictionary<int, Mouvement>(20);
                    for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                    {
                        for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); 
                            ligne >= 0 && ligne < 10 && col >= 0 && col < 10; 
                            ligne = getLigneVoisine(ligne, pos),col = getColonneVoisine(col, pos) )
                            MOUVEMENTS_DAMES[i, j].Add((byte)(ligne * 10 + col), new Mouvement(i, j, ligne, col));
                    }
                }
        }

        public Grille()
        {
            grille = (int[,])GRILLE_10.Clone();
            taille = 10;
            tour = TOUR_BLANCS;
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
            if(actionsPossibles.Count==0)
                mouvementsPossibles();
        }

        public Action realiseActionCases(Case c1,Case c2)
        {
            foreach (Action a in actionsPossibles)
                if (a.commenceEtPasse(c1,c2))
                    return realiserAction(a);
            return null;
        }

        /* Réalise l'action a, et promeut le pion en dame si nécessaire. Bascule le tour des joueurs.*/
        public Action realiserAction(Action a)
        {
            if (a.realiser(this))
            {
                Case c = a.cazArrivee();
                ennoblir(c.ligne, c.colonne);
            }
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

        private void ennoblir(byte l, byte c)
        {
            if (grille[l, c] == PION_BLANC)
            {
                grille[l, c] = DAME_BLANC;
                Case ca = CASES[l, c];
                pionsBlancs.Remove(ca);
                damesBlancs.Add(ca);
                nbPionsBlancs--;
                nbDamesBlancs++;
            }
            else
            {
                grille[l, c] = DAME_NOIR;
                Case ca = CASES[l, c];
                pionsNoirs.Remove(ca);
                damesNoirs.Add(ca);
                nbPionsNoirs--;
                nbDamesNoirs++;
            }
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
        * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
        * 
        * itérations HAUT/BAS GAUCHE/DROIT à enlever, car 2 boucles seulement
        */
        void mouvementsPossibles()
        {
            if (tour == TOUR_BLANCS)
            {
                foreach (Case c in pionsBlancs)
                {
                    int pos = HAUT_GAUCHE;
                    byte l = getLigneVoisine(c.ligne, pos), co = getColonneVoisine(c.colonne, pos);
                    if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                            actionsPossibles.Add(MOUVEMENTS_PIONS[l, co, pos]);
                    pos = HAUT_DROIT;
                    l = getLigneVoisine(c.ligne, pos);
                    co = getColonneVoisine(c.colonne, pos);
                    if (l >= 0 && l < taille && co >= 0 && co < taille && grille[l, co] == VIDE)
                        actionsPossibles.Add(MOUVEMENTS_PIONS[l, co, pos]);
                }
                foreach(Case c in damesBlancs)
                {
                    for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                        for (byte ligne = getLigneVoisine(c.ligne, pos), col = getColonneVoisine(c.colonne, pos);
                            ligne >= 0 && ligne < taille && col >= 0 && col < taille
                            && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos),
                                col = getColonneVoisine(col, pos))
                            actionsPossibles.Add(MOUVEMENTS_DAMES[c.ligne, c.colonne][ligne * 10 + col]);
                }
            }
            else
            {
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
                            actionsPossibles.Add(MOUVEMENTS_DAMES[c.ligne, c.colonne][ligne * 10 + col]);
                }
            }
        }

        /* Génère toutes les prises possibles de tous les pions du joueur qui a le tour. */
        void prises()
        {
            prisesEnCours = new Queue<Prise>();
            prisesDamesEnCours = new Queue<PriseDame>();
            prisesEtendues = new Queue<Prise>();
            prisesDamesEtendues = new Queue<PriseDame>();
            maxPionsPris = 0;
            if (tour == TOUR_BLANCS)
            {
                foreach (Case c in pionsBlancs)
                    prisesPionBlanc(c.ligne, c.colonne);
                foreach(Case c in damesBlancs)
                    prisesPionNoir(c.ligne, c.colonne);
            }
            else
            {
                foreach (Case c in pionsNoirs)
                    prisesDameBlanc(c.ligne, c.colonne);
                foreach (Case c in damesNoirs)
                    prisesDameNoir(c.ligne, c.colonne);
            }
            foreach(Prise p in prisesEtendues)
            {
                int n = p.nombrePionsPris();
                if (n > maxPionsPris)
                    maxPionsPris = n;
            }
            foreach (Prise p in prisesDamesEtendues)
            {
                int n = p.nombrePionsPris();
                if (n > maxPionsPris)
                    maxPionsPris = n;
            }
            foreach (Prise p in prisesEtendues)
            {
                if (p.nombrePionsPris() == maxPionsPris)
                    actionsPossibles.Add(p);
            }
            foreach (Prise p in prisesDamesEtendues)
            {
                if (p.nombrePionsPris() == maxPionsPris)
                    actionsPossibles.Add(p);
            }
            triePrisesDames();
        }

        void prisesPionBlanc(byte l1, byte c1)
        {
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                byte l2 = getLigneVoisine(l1, position), c2 = getColonneVoisine(c1, position);
                if (l2 > 0 && l2 < taille-1 && c2 > 0 && c2 < taille-1 && grille[l2, c2] < 0)
                {
                    byte l3 = getLigneVoisine(l2, position), c3 = getColonneVoisine(c2, position);
                    if (grille[l3, c3] == VIDE)
                    {
                        Prise p = new Prise(CASES[l1, c1], CASES[l2, c2]);
                        p.cases.Add(CASES[l3, c3]);
                        prisesEnCours.Enqueue(p);
                        while (prisesEnCours.Count > 0)
                        {
                            Prise p2 = prisesEnCours.Dequeue();
                            bool poursuivie = false;
                            for (short pos2 = HAUT_GAUCHE; pos2 <= BAS_GAUCHE; pos2++)
                            {
                                l2 = getLigneVoisine(l3, pos2);
                                c2 = getColonneVoisine(c3, pos2);
                                if (l2 > 0 && l2 < taille-1 && c2 > 0 && c2 < taille-1)
                                {
                                    l3 = getLigneVoisine(l2, pos2);
                                    c3 = getColonneVoisine(c2, pos2);
                                    if (grille[l2, c2] < 0 && (grille[l3, c3] == VIDE || (l3 == p2.cases[0].ligne && c3 == p2.cases[0].colonne)))
                                    {
                                        Case ca = CASES[l2, c2];
                                        if (!poursuivie)
                                        {
                                            p2.cases.Add(ca);
                                            p2.cases.Add(CASES[l3,c3]);
                                            prisesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else
                                        {
                                            Prise p3 = new Prise(p2, ca);
                                            p3.cases.Add(CASES[l3, c3]);
                                            prisesEnCours.Enqueue(p3);
                                        }
                                    }
                                }
                            }
                            if (!poursuivie)
                                prisesEtendues.Enqueue(p2);
                        }
                    }
                }
            }
        }

        void prisesPionNoir(byte l1, byte c1)
        {
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                byte l2 = getLigneVoisine(l1, position), c2 = getColonneVoisine(c1, position);
                if (l2 > 0 && l2 < taille - 1 && c2 > 0 && c2 < taille - 1 && grille[l2, c2] > 0)
                {
                    byte l3 = getLigneVoisine(l2, position), c3 = getColonneVoisine(c2, position);
                    if (grille[l3, c3] == VIDE)
                    {
                        Prise p = new Prise(CASES[l1, c1], CASES[l2, c2]);
                        p.cases.Add(CASES[l3, c3]);
                        prisesEnCours.Enqueue(p);
                        while (prisesEnCours.Count > 0)
                        {
                            Prise p2 = prisesEnCours.Dequeue();
                            bool poursuivie = false;
                            for (short pos2 = HAUT_GAUCHE; pos2 <= BAS_GAUCHE; pos2++)
                            {
                                l2 = getLigneVoisine(l3, pos2);
                                c2 = getColonneVoisine(c3, pos2);
                                if (l2 > 0 && l2 < taille - 1 && c2 > 0 && c2 < taille - 1)
                                {
                                    l3 = getLigneVoisine(l2, pos2);
                                    c3 = getColonneVoisine(c2, pos2);
                                    if (grille[l2, c2] > 0 && (grille[l3, c3] == VIDE || (l3 == p2.cases[0].ligne && c3 == p2.cases[0].colonne)))
                                    {
                                        Case ca = CASES[l2, c2];
                                        if (!poursuivie)
                                        {
                                            p2.cases.Add(ca);
                                            p2.cases.Add(CASES[l3, c3]);
                                            prisesEnCours.Enqueue(p2);
                                            poursuivie = true;
                                        }
                                        else
                                        {
                                            Prise p3 = new Prise(p2, ca);
                                            p3.cases.Add(CASES[l3, c3]);
                                            prisesEnCours.Enqueue(p3);
                                        }
                                    }
                                }
                            }
                            if (!poursuivie)
                                prisesEtendues.Enqueue(p2);
                        }
                    }
                }
            }
        }

        void prisesDameBlanc(byte l1, byte c1)
        {
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                PriseDame prise = null;
                bool premierPionNonRencontre = true, secondPionNonRencontre = true;
                bool premiereArrivee = false;
                for (byte l2 = getLigneVoisine(l1, position), c2 = getColonneVoisine(c1,position); 
                    l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && secondPionNonRencontre; 
                    l2 = getLigneVoisine(l2, position), c2 = getColonneVoisine(c2, position))
                {
                    if (grille[l2, c2] != VIDE)
                    {
                        //
                        if (premierPionNonRencontre)
                        {
                            premierPionNonRencontre = false;
                            if (grille[l2, c2]<0
                                && caseApresSautLibre(l1,c1, l2, c2, position))
                                prise = new PriseDame(CASES[l1, c1], CASES[l2, c2]);
                            else secondPionNonRencontre = false;
                        }
                        else secondPionNonRencontre = false;
                    }
                    //
                    else if (!premierPionNonRencontre && prise != null)
                    {
                        if (!premiereArrivee)
                        {
                            prise.cases.Add(CASES[l2, c2]);
                            premiereArrivee = true;
                        }
                        else prise = new PriseDame(prise, CASES[l2, c2]);
                        prisesEnCours.Enqueue(prise);
                    }
                }

                while (prisesEnCours.Count > 0)
                {
                    PriseDame p2 = prisesDamesEnCours.Dequeue();
                    bool poursuivie = false;

                    for (short pos2 = HAUT_GAUCHE; pos2 <= BAS_GAUCHE; pos2++)
                    {
                        prise = null;
                        premierPionNonRencontre = true;
                        secondPionNonRencontre = true;
                        premiereArrivee = false;
                        for (byte l2 = getLigneVoisine(l1, position), c2 = getColonneVoisine(c1, position);
                            l2 >= 0 && l2 < taille && c2 >= 0 && c2 < taille && secondPionNonRencontre;
                            l2 = getLigneVoisine(l2, position), c2 = getColonneVoisine(c2, position))
                        {
                            if (grille[l2, c2] != VIDE)
                            {
                                if (premierPionNonRencontre)
                                {
                                    premierPionNonRencontre = false;
                                    if (grille[l2, c2] < 0 && !p2.pionVirtuellementPris(l2, c2)
                                        && caseApresSautLibreOuContientPionPreneur(p2, l2, c2, position))
                                    {
                                        if (!poursuivie)
                                            p2.cases.Add(CASES[l2, c2]);
                                        else
                                        {
                                            prise = new PriseDame(p2, CASES[l2, c2]);
                                            poursuivie = true;
                                        }
                                    }
                                    else secondPionNonRencontre = false;
                                }
                                else secondPionNonRencontre = false;
                            }
                            else if (!premierPionNonRencontre && prise != null)
                            {
                                if (!premiereArrivee)
                                {
                                    prise.cases.Add(CASES[l2, c2]);
                                    premiereArrivee = true;
                                }
                                else prise = new PriseDame(prise, CASES[l2, c2]);
                                prisesEnCours.Enqueue(prise);
                            }
                        }
                    }
                    if (!poursuivie)
                        prisesEtendues.Enqueue(p2);
                }
            }
        }

        void prisesDameNoir(byte l, byte co)
        {
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
            {
                PriseDame prise = null;
                bool premierPionNonRencontre = true, secondPionNonRencontre = true;
                bool poursuivie = false;
                for (byte li = getLigneVoisine(l, position), col = getColonneVoisine(co, position);
                    li >= 0 && li < taille && col >= 0 && col < taille && secondPionNonRencontre;
                    li = getLigneVoisine(li, position), col = getColonneVoisine(col, position))
                {
                    if (grille[li, col] != VIDE)
                    {
                        if (premierPionNonRencontre)
                        {
                            premierPionNonRencontre = false;
                            if (grille[li, col] > 0
                                && caseApresSautLibre(l, co, li, col, position))
                                prise = new PriseDame(CASES[l, co], CASES[li, col]);
                            else secondPionNonRencontre = false;
                        }
                        else secondPionNonRencontre = false;
                    }
                    else if (!premierPionNonRencontre && prise != null)
                    {
                        if (!poursuivie)
                        {
                            prise.cases.Add(CASES[li, col]);
                            poursuivie = true;
                        }
                        else prise = new PriseDame(prise, CASES[li, col]);
                        prisesEnCours.Enqueue(prise);
                    }
                }

                while (prisesEnCours.Count > 0)
                {
                    Prise p2 = prisesEnCours.Dequeue();
                    //bool ici ou boucle suivante ?
                    poursuivie = false;

                    for (short pos2 = HAUT_GAUCHE; pos2 <= BAS_GAUCHE; pos2++)
                    {
                        prise = null;
                        premierPionNonRencontre = true;
                        secondPionNonRencontre = true;
                        for (byte li = getLigneVoisine(l, position), col = getColonneVoisine(co, position);
                            li >= 0 && li < taille && col >= 0 && col < taille && secondPionNonRencontre;
                            li = getLigneVoisine(li, position), col = getColonneVoisine(col, position))
                        {
                            if (grille[li, col] != VIDE)
                            {
                                if (premierPionNonRencontre)
                                {
                                    premierPionNonRencontre = false;
                                    if (grille[li, col] > 0 && !p2.pionVirtuellementPris(li, col)
                                        && caseApresSautLibreOuContientPionPreneur(prise, li, col, position))
                                        p2.cases.Add(CASES[li, col]);
                                    else secondPionNonRencontre = false;
                                }
                                else secondPionNonRencontre = false;
                            }
                            else if (!premierPionNonRencontre && prise != null)
                            {
                                if (!poursuivie)
                                {
                                    prise.cases.Add(CASES[li, col]);
                                    poursuivie = true;
                                }
                                else prise = new PriseDame(prise, CASES[li, col]);
                                prisesEnCours.Enqueue(prise);
                            }
                        }
                    }
                    if (!poursuivie)
                        prisesEtendues.Enqueue(p2);
                }
            }
        }

        /* Supprime les prises "doublons" : les prises du même pion preneur, prenant les mêmes pions adverses dans le même ordre, se terminant sur la même case, et caractérisées par des cases étapes/intermédiaires différentes. */
        void triePrisesDames()
        {
            //
            List<Prise> prises = new List<Prise>();
            PriseDame p1, p2;
            for (byte i = 0; i < actionsPossibles.Count; i++)
            {
                Action p = this.actionsPossibles[i];
                if (p is PriseDame)
                {
                    p1 = (PriseDame)p;
                    for (byte j = (byte)(actionsPossibles.Count - 1); j > i; j--)
                    {
                        p = this.actionsPossibles[j];
                        if (p is PriseDame)
                        {
                            p2 = (PriseDame)p;
                            if (p1.prendMemePionsMemeOrdre(p2))
                                prises.RemoveAt(j);
                        }
                    }
                }
            }
        }

        bool caseApresSautLibreOuContientPionPreneur(Prise p, byte l, byte c, short pos)
        {
            byte ligne = getLigneVoisine(l, pos), col = getColonneVoisine(c, pos);
            if (ligne < 0 || ligne >= taille || col < 0 || col >= taille)
                return false;
            return grille[ligne, col] == VIDE || (ligne == p.cases[0].ligne && col == p.cases[0].colonne);
        }

        bool caseApresSautLibre(byte ld,byte cd, byte l, byte c, short pos)
        {
            byte ligne = getLigneVoisine(l, pos), col = getColonneVoisine(c, pos);
            return (ligne >= 0 && ligne < taille) && (col >= 0 && col < taille) && grille[ligne, col] == VIDE;
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