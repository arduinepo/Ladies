using System;
using System.Collections.Generic;
using System.Text;

namespace Assets
{

    //Collection de tous les mouvements et prises uniques par pion possibles
    //1 : passer en matrice 1D
    //2 : passer en matrice 1D sans les cases injouables

    //doubler méthodes de recherche d'action suivant noirs ou blancs pour diminuer nombre de if

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
        protected bool tour;
        public int nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;

        protected List<Action> actionsPossibles;
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
            tour = TOUR_BLANCS;
            nbPionsBlancs = nbPionsNoirs = 20;
            nbDamesBlancs = nbDamesNoirs = 0;
        }

        public Grille(Grille g)
        {
            grille = (int[,])g.grille.Clone();
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
            mouvementsPossibles();
        }

        /* Réalise l'action a, et promeut le pion en dame si nécessaire. Bascule le tour des joueurs.*/
        protected void realiserAction(Action a)
        {
            if (a.realiser(this))
            {
                Case c = a.cazArrivee();
                ennoblir(c.ligne, c.colonne);
            }
            tour = !this.tour;
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
                nbPionsBlancs--;
                nbDamesBlancs++;
            }
            else
            {
                grille[l, c] = DAME_NOIR;
                nbPionsNoirs--;
                nbDamesNoirs++;
            }
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
        * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
        */
        void mouvementsPossibles()
        {
            if (tour == TOUR_BLANCS)
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 1 : 0); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_BLANC)
                            for (short pos = HAUT_GAUCHE; pos <= HAUT_DROIT; pos++)
                            {
                                byte l = getLigneVoisine(i, pos), c = getColonneVoisine(j, pos);
                                if (l >= 0 && l < grille.Length && c >= 0 && c < grille.Length && grille[l, c] == VIDE)
                                    actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            }
                        else if (grille[i, j] == DAME_BLANC)
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for(byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos); 
                                    ligne >= 0 && ligne < grille.Length && col >= 0 && col < grille.Length
                                    && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos),
                                        col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j][ligne*10+col]);
            }
            else
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 1 : 0); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_NOIR)
                            for (short pos = BAS_GAUCHE; pos <= BAS_DROIT; pos++)
                            {
                                byte l = getLigneVoisine(i, pos), c = getColonneVoisine(j, pos);
                                if (l >= 0 && l < grille.Length && c >= 0 && c < grille.Length && grille[l, c] == VIDE)
                                    actionsPossibles.Add(MOUVEMENTS_PIONS[i, j, pos]);
                            }
                        else if (grille[i, j] == DAME_NOIR)
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                                for (byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos);
                                    ligne >= 0 && ligne < grille.Length && col >= 0 && col < grille.Length
                                    && grille[ligne, col] == VIDE; ligne = getLigneVoisine(ligne, pos),
                                        col = getColonneVoisine(col, pos))
                                    actionsPossibles.Add(MOUVEMENTS_DAMES[i, j][ligne * 10 + col]);
            }
        }

        /* Génère toutes les prises possibles de tous les pions du joueur qui a le tour. */
        void prises()
        {
            if (tour == TOUR_BLANCS)
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 1 : 0); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_BLANC || grille[i, j] == DAME_BLANC)
                            prisesPion(i, j);
            }
            else
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 1 : 0); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_NOIR || grille[i, j] == DAME_NOIR)
                            prisesPion(i, j);
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
                prisesEtendues.Add(p);
            } while (prisesEnCours.Count > 0);
            prisesEtendues.RemoveAt(0);
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
            int l = p.caseDepart.ligne, co = p.caseDepart.colonne;
            int ligneArrivee = c.ligne, colArrivee = c.colonne;
            if (p is PriseDame)
            {
                for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
                {
                    bool premierPionNonRencontre = true, secondPionNonRencontre = true;
                    PriseDame priseD = null;
                    for (byte ligne = getLigneVoisine(ligneArrivee, position), col = getColonneVoisine(colArrivee,
                        position); ligne >= 0 && ligne < grille.Length && col >= 0 && col < grille.Length
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
                            prises.Add(new Prise(priseD, ligne, col));
                    }
                }
            }
            else for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
                {
                    byte ligne = getLigneVoisine(ligneArrivee, position), col = getColonneVoisine(colArrivee, position);
                    if (peutEtendrePrise(p, position) && !p.pionVirtuellementPris(ligne, col))
                        prises.Add(new Prise(p, getLigneVoisine(ligne, position), getColonneVoisine(col, position)));
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

        bool caseApresSautLibreOuContientPionPreneur(Prise p, byte l, byte c, short pos)
        {
            byte ligne = getLigneVoisine(l, pos), col = getColonneVoisine(c, pos);
            if (ligne < 0 || ligne >= grille.Length || col < 0 || col >= grille.Length)
                return false;
            return grille[ligne, col] == VIDE || (ligne == p.caseDepart.ligne && col == p.caseDepart.colonne);
        }

        bool peutEtendrePrise(Prise p, short positionCase)
        {
            Case c = p.cazArrivee(), c2 = p.caseDepart;
            byte l = c.ligne, co = c.colonne;
            return contientPionAdverse(grille[c2.ligne, c2.colonne], l, co,
                positionCase) && caseApresSautLibreOuContientPionPreneur(p, getLigneVoisine(l, positionCase), getColonneVoisine(co, positionCase), positionCase);
        }

        bool contientPionAdverse(int pion, int ligne, int col, int position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                    return ligne + HAUT >= 0 && col + GAUCHE >= 0 && ((pion > 0 && grille[ligne + HAUT, col + GAUCHE] < 0)
                        || (pion < 0 && grille[ligne + HAUT, col + GAUCHE] > 0));
                case (HAUT_DROIT):
                    return ligne + HAUT >= 0 && col + DROITE < grille.Length
                        && ((pion > 0 && grille[ligne + HAUT, col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + HAUT, col + DROITE] > 0));
                case (BAS_DROIT):
                    return ligne + BAS < grille.Length && col + DROITE < grille.Length
                        && ((pion > 0 && grille[ligne + BAS, col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + BAS, col + DROITE] > 0));
                case (BAS_GAUCHE):
                    return ligne + BAS < grille.Length && col + GAUCHE >= 0
                        && ((pion > 0 && grille[ligne + BAS, col + GAUCHE] < 0)
                            || (pion < 0 && grille[ligne + BAS, col + GAUCHE] > 0));
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