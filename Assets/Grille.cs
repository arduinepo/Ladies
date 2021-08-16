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
        int taille;
        public List<Case> pionsBlancs, pionsNoirs, damesBlancs, damesNoirs;
        protected bool tour;
        public int nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;
        public List<Action> actionsPossibles;
        private int maxPionsPris;
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
            maxPionsPris = 0;
            if (tour == TOUR_BLANCS)
            {
                foreach (Case c in pionsBlancs)
                    prisesPion(c.ligne, c.colonne);
                foreach(Case c in damesBlancs)
                    prisesPion(c.ligne, c.colonne);
            }
            else
            {
                foreach (Case c in pionsNoirs)
                    prisesPion(c.ligne, c.colonne);
                foreach (Case c in damesNoirs)
                    prisesPion(c.ligne, c.colonne);
            }
            triePrisesDames();
        }

        /* Génère toutes les prises possibles d'un pion. */

        // OPTI
        void prisesPion(byte ligne, byte colonne)
        {
            List<Prise> prisesEnCours = new List<Prise>(), prisesEtendues = new List<Prise>();
            if (Math.Abs(grille[ligne, colonne]) == 2)
                prisesEnCours.Add(new PriseDame(ligne, colonne));
            else prisesEnCours.Add(new Prise(ligne, colonne));
            do
            {
                Prise p = prisesEnCours[0];
                prisesEnCours.RemoveAt(0);
                prisesEnCours.AddRange(etendrePrise(p));
                //prisesEtendues.Add(p);
            } while (prisesEnCours.Count > 0);
            prisesEtendues.RemoveAt(0);
            prisesEtendues.RemoveAll(x => x.nombrePionsPris() < maxPionsPris);
            actionsPossibles.AddRange(prisesEtendues);
        }

        /* Etend la prise p : renvoie toutes les prises résultant de son extension aux pions adverses voisins du pion 
         * preneur ou situés sur la diagonale de la dame preneuse, si une case libre se trouve derrière eux. */

        //OPTI : créer nouvelle prise seulement si plusieurs prises possibles à partir du même pion
             //   boucler jusqu'à 
        
        List<Prise> etendrePrise(Prise p)
        {
            List<Prise> prises = new List<Prise>();
            Case c = p.cazArrivee();
            int l = p.cases[0].ligne, co = p.cases[0].colonne;
            int ligneArrivee = c.ligne, colArrivee = c.colonne;
            if (p is PriseDame)
            {
                for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
                {
                    bool premierPionNonRencontre = true, secondPionNonRencontre = true;
                    PriseDame priseD = null;
                    for (byte ligne = getLigneVoisine(ligneArrivee, position), col = getColonneVoisine(colArrivee,
                        position); ligne >= 0 && ligne < taille && col >= 0 && col < taille
                         && secondPionNonRencontre; ligne = getLigneVoisine(ligne, position), col = getColonneVoisine(col, position))
                    {
                        if (grille[ligne, col] != VIDE)
                        {
                            if (premierPionNonRencontre)
                            {
                                premierPionNonRencontre = false;
                                if (caseOccupeeParAdversaire(grille[l, co], ligne, col)
                                    && caseApresSautLibreOuContientPionPreneur(p, ligne, col, position)
                                    && !p.pionVirtuellementPris(ligne, col))
                                    priseD = new PriseDame(p, ligne, col);
                                    
                                else
                                    secondPionNonRencontre = false;
                            }
                            else
                                secondPionNonRencontre = false;
                        }
                        else if (!premierPionNonRencontre && priseD != null)
                        {
                            PriseDame p1 = new PriseDame(priseD, ligne, col);
                            prises.Add(p1);
                            int n = p1.nombrePionsPris();
                            if (n > maxPionsPris)
                                maxPionsPris = n;
                        }
                            
                    }
                }
            }
            else for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
                {
                    byte ligne = getLigneVoisine(ligneArrivee, position), col = getColonneVoisine(colArrivee, position);
                    if (peutEtendrePrise(p, position) && !p.pionVirtuellementPris(ligne, col))
                    {
                        Prise p1 = new Prise(p, getLigneVoisine(ligne, position), getColonneVoisine(col, position));
                        prises.Add(p1);
                        int n = p1.nombrePionsPris();
                        if (n > maxPionsPris)
                            maxPionsPris = n;
                    }
                }
            
            return prises;
        }

        /* Supprime les prises "doublons" : les prises du même pion preneur, prenant les mêmes pions adverses dans le même ordre, se terminant sur la même case, et caractérisées par des cases étapes/intermédiaires différentes. */
        void triePrisesDames()
        {
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

        bool caseOccupeeParAdversaire(int pion, int ligneCase, int colCase)
        {
            int pionCase = grille[ligneCase, colCase];
            return (pion < 0 && pionCase > 0) || (pion > 0 && pionCase < 0);
        }

        bool caseContientBlanc(int l, int c)
        {
            return grille[l,c]>0;
        }

        bool caseContientNoir(int l,int c)
        {
            return grille[l, c] < 0;
        }

        bool caseApresSautLibreOuContientPionPreneur(Prise p, byte l, byte c, short pos)
        {
            byte ligne = getLigneVoisine(l, pos), col = getColonneVoisine(c, pos);
            if (ligne < 0 || ligne >= taille || col < 0 || col >= taille)
                return false;
            return grille[ligne, col] == VIDE || (ligne == p.cases[0].ligne && col == p.cases[0].colonne);
        }

        bool peutEtendrePrise(Prise p, short positionCase)
        {
            Case c = p.cazArrivee(), c2 = p.cases[0];
            byte l = c.ligne, co = c.colonne;
            return contientPionAdverse(grille[c2.ligne, c2.colonne], l, co, positionCase) && 
                caseApresSautLibreOuContientPionPreneur(p, getLigneVoisine(l, positionCase), getColonneVoisine(co, positionCase), positionCase);
        }

        bool contientPionAdverse(int pion, int ligne, int col, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                    return ligne + HAUT >= 0 && col + GAUCHE >= 0 && ((pion > 0 && grille[ligne + HAUT, col + GAUCHE] < 0)
                        || (pion < 0 && grille[ligne + HAUT, col + GAUCHE] > 0));
                case (HAUT_DROIT):
                    return ligne + HAUT >= 0 && col + DROITE < taille
                        && ((pion > 0 && grille[ligne + HAUT, col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + HAUT, col + DROITE] > 0));
                case (BAS_DROIT):
                    return ligne + BAS < taille && col + DROITE < taille
                        && ((pion > 0 && grille[ligne + BAS, col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + BAS, col + DROITE] > 0));
                case (BAS_GAUCHE):
                    return ligne + BAS < taille && col + GAUCHE >= 0
                        && ((pion > 0 && grille[ligne + BAS, col + GAUCHE] < 0)
                            || (pion < 0 && grille[ligne + BAS, col + GAUCHE] > 0));
            }
            return false;
        }

        bool contientPionBlanc(int ligne, int col, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                    return ligne + HAUT >= 0 && col + GAUCHE >= 0 && grille[ligne + HAUT, col + GAUCHE] > 0;
                case (HAUT_DROIT):
                    return ligne + HAUT >= 0 && col + DROITE < taille && grille[ligne + HAUT, col + DROITE] > 0;
                case (BAS_DROIT):
                    return ligne + BAS < taille && col + DROITE < taille && grille[ligne + BAS, col + DROITE] > 0;
                case (BAS_GAUCHE):
                    return ligne + BAS < taille && col + GAUCHE >= 0 &&  grille[ligne + BAS, col + GAUCHE] > 0;
            }
            return false;
        }

        bool contientPionNoir(int ligne, int col, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                    return ligne + HAUT >= 0 && col + GAUCHE >= 0 && grille[ligne + HAUT, col + GAUCHE] < 0;
                case (HAUT_DROIT):
                    return ligne + HAUT >= 0 && col + DROITE < taille && grille[ligne + HAUT, col + DROITE] < 0;
                case (BAS_DROIT):
                    return ligne + BAS < taille && col + DROITE < taille && grille[ligne + BAS, col + DROITE] < 0;
                case (BAS_GAUCHE):
                    return ligne + BAS < taille && col + GAUCHE >= 0 && grille[ligne + BAS, col + GAUCHE] < 0;
            }
            return false;
        }

        const byte ERR = 101;

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