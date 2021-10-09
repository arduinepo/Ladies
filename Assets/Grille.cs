using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        static List<int>[] DIRS = new List<int>[4];

        const int HAUT_GAUCHE = 0, HAUT_DROIT = 1, BAS_DROIT = 2, BAS_GAUCHE = 3;
        const int HAUT = -1, GAUCHE = -1, BAS = 1, DROITE = 1;
        public const int PION_BLANC = 1, PION_NOIR = -1, DAME_BLANC = 2, DAME_NOIR = -2, VIDE = 0;

        public int[,] grille;
        public int taille;
        public bool tourBlanc;
        public int nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;

        public List<Action> actionsPossibles;
        private int maxPionsPris;
        private Queue<Prise> prisesEnCours;
        List<Prise> prisesEtendues;
        Queue<PriseDame> prisesDamesEnCours;
        List<PriseDame> prisesDamesEtendues;

        int nbToursSansPriseNiMouvPion = 0, nbTours1DameVs3Pieces = -1;
        bool egal = false;
        #endregion

        static Grille()
        {
            DIRS[HAUT_GAUCHE] = new int[3] { HAUT_GAUCHE, HAUT_DROIT, BAS_GAUCHE }.ToList();
            DIRS[HAUT_DROIT] = new int[3] { HAUT_DROIT, BAS_DROIT, HAUT_GAUCHE }.ToList();
            DIRS[BAS_DROIT] = new int[3] { BAS_DROIT, BAS_GAUCHE, HAUT_DROIT }.ToList();
            DIRS[BAS_GAUCHE] = new int[3] { BAS_GAUCHE, HAUT_GAUCHE, BAS_DROIT }.ToList();

            for (byte i = 0; i < 10; ++i)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                    CASES[i, j] = new Case(i, j);

            for (byte i = 0; i < 9; ++i)
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
            for (byte i = 1; i < 10; ++i)
                for (byte j = (byte)(i % 2 == 0 ? 1 : 0); j < 10; j += 2)
                {
                    if (i > 1)
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
            for (byte i = 0; i < 10; ++i)
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
            tourBlanc = true;
            nbPionsBlancs = nbPionsNoirs = 20;
            nbDamesBlancs = nbDamesNoirs = 0;
        }

        public Grille(Grille g, Action a)
        {
            grille = (int[,])g.grille.Clone();
            taille = g.taille;
            tourBlanc = g.tourBlanc;
            nbPionsBlancs = g.nbPionsBlancs;
            nbPionsNoirs = g.nbPionsNoirs;
            nbDamesBlancs = g.nbDamesBlancs;
            nbDamesNoirs = g.nbDamesNoirs;

            nbToursSansPriseNiMouvPion = g.nbToursSansPriseNiMouvPion;
            nbTours1DameVs3Pieces = g.nbTours1DameVs3Pieces;
            egal = g.egal;
            avancer(a);
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
            tourBlanc = !this.tourBlanc;
            if (!(a is Prise || a is MouvementNoir || a is MouvementBlanc))
                nbToursSansPriseNiMouvPion++;
            else nbToursSansPriseNiMouvPion = 0;
            if ((nbDamesBlancs == 1 && (nbDamesNoirs + nbPionsNoirs <= 3)) || (nbDamesNoirs == 1 && (nbDamesBlancs + nbPionsBlancs <= 3)))
                nbTours1DameVs3Pieces++;
            return a;
        }

        private void avancer(Action a)
        {
            a.realiser(this);
            tourBlanc = !this.tourBlanc;
            if (!(a is Prise || a is MouvementNoir || a is MouvementBlanc))
                nbToursSansPriseNiMouvPion++;
            else nbToursSansPriseNiMouvPion = 0;
            if ((nbDamesBlancs == 1 && (nbDamesNoirs + nbPionsNoirs <= 3)) || (nbDamesNoirs == 1 && (nbDamesBlancs + nbPionsBlancs <= 3)))
                nbTours1DameVs3Pieces++;
            /*
            bool prec = configsPrecedentes.TryGetValue(this, out nbToursMemePosition);
            if (prec)
                configsPrecedentes[this]=++nbToursMemePosition;
            configsPrecedentes.Add(this, 1);
            */
        }

        public bool partieFinie()
        {
            if (nbToursSansPriseNiMouvPion == 25 || nbTours1DameVs3Pieces == 16)
            {
                egal = true;
                return true;
            }
            if (nbDamesBlancs + nbPionsBlancs == 0 || nbPionsNoirs + nbDamesNoirs == 0)
                return true;
            genereActionsPossibles();
            return actionsPossibles.Count == 0;
        }

        public int resultatBlanc()
        {
            if (egal)
                return 0;
            if (nbDamesBlancs + nbPionsBlancs == 0)
                return -1;
            if (nbPionsNoirs + nbDamesNoirs == 0)
                return 1;
            genereActionsPossibles();
            if (actionsPossibles.Count == 0)
                return tourBlanc ? -1 : 1;
            return 0;
        }

        public int resultatNoir()
        {
            if (egal)
                return 0;
            if (nbDamesBlancs + nbPionsBlancs == 0)
                return 1;
            if (nbPionsNoirs + nbDamesNoirs == 0)
                return -1;
            if (tourBlanc)
            {
                genereActionsPossibles();
                if (actionsPossibles.Count == 0)
                    return 1;
            }
            else
            {
                tourBlanc = false;
                genereActionsPossibles();
                if (actionsPossibles.Count == 0)
                    return -1;
            }
            return 0;
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
        * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
        */
        void mouvementsPossibles()
        {
            if (tourBlanc)
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

            if (tourBlanc)
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

            for (int i = prisesEtendues.Count - 1; i >= 0 && prisesEtendues[i].taille == maxPionsPris; --i)
                actionsPossibles.Add(prisesEtendues[i]);

            prisesDamesEtendues.RemoveAll(p => p.taille < maxPionsPris);
            for (int i = prisesDamesEtendues.Count - 1; i > 0; --i)
                for (int j = i - 1; j >= 0; --j)
                    if (prisesDamesEtendues[i].prendMemePionsMemeOrdre(prisesDamesEtendues[j]))
                    {
                        prisesDamesEtendues.RemoveAt(i);
                        break;
                    }
            actionsPossibles.AddRange(prisesDamesEtendues);
        }

        void prisesPionBlanc(int lignePion, int colonnePion)
        {
            Prise p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; ++position)
            {
                int ligneAdjacente = getLigneVoisine(lignePion, position), colAdjacente = getColonneVoisine(colonnePion, position);
                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] < 0)
                {
                    int ligneArrivee = getLigneVoisine(ligneAdjacente, position), colArrivee = getColonneVoisine(colAdjacente, position);
                    if (grille[ligneArrivee, colArrivee] == VIDE)
                    {
                        prisesEnCours.Enqueue(new PriseBlanc(CASES[lignePion, colonnePion], CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], position));
                        while (prisesEnCours.Count > 0)
                        {
                            p2 = prisesEnCours.Dequeue();
                            int lA = p2.cases[p2.cases.Count - 1].ligne;
                            int cA = p2.cases[p2.cases.Count - 1].colonne;
                            bool poursuivie = false;
                            int dir = p2.lastDir;
                            foreach (int pos2 in DIRS[dir])
                            {
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
                            if (!poursuivie && p2.nombrePionsPris() >= maxPionsPris)
                            {
                                prisesEtendues.Add(p2);
                                maxPionsPris = p2.taille;
                            }


                        }
                    }
                }
            }
        }

        void prisesPionNoir(byte lignePion, byte colPion)
        {
            Prise p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; ++position)
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
                            int dir = p2.lastDir;
                            foreach (int pos2 in DIRS[dir])
                            {
                                ligneAdjacente = getLigneVoisine(lA, pos2);
                                colAdjacente = getColonneVoisine(cA, pos2);
                                if (ligneAdjacente > 0 && ligneAdjacente < taille - 1 && colAdjacente > 0 && colAdjacente < taille - 1 && grille[ligneAdjacente, colAdjacente] > 0 && !p2.pionVirtuellementPris(ligneAdjacente, colAdjacente))
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
                                        else prisesEnCours.Enqueue(new PriseNoir(p2, CASES[ligneAdjacente, colAdjacente], CASES[ligneArrivee, colArrivee], pos2));
                                    }
                                }
                            }
                            if (!poursuivie && p2.nombrePionsPris() >= maxPionsPris)
                            {
                                prisesEtendues.Add(p2);
                                maxPionsPris = p2.taille;
                            }
                        }
                    }
                }
            }
        }

        void prisesDameBlanc(byte lignePion, byte colPion)
        {
            Case adv = null;
            PriseDame p1, p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; ++position)
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
                                p1 = new PriseBlanche(CASES[lignePion, colPion], adv, CASES[l2, c2], position);
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
                                        else prisesDamesEnCours.Enqueue(new PriseBlanche(p2, adv, CASES[l2, c2], pos2));
                                        premiereArrivee = true;

                                    }
                                    else prisesDamesEnCours.Enqueue(new PriseBlanche(p2, adv, CASES[l2, c2], pos2));
                                }
                            }
                            else secondPionRencontre = true;
                        }
                    }
                    if (!poursuivie && p2.nombrePionsPris() >= maxPionsPris)
                    {
                        prisesEtendues.Add(p2);
                        maxPionsPris = p2.taille;
                    }
                }
            }
        }

        void prisesDameNoir(byte lignePion, byte colPion)
        {
            Case adv = null;
            PriseDame p1, p2;
            for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; ++position)
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
                    if (!poursuivie && p2.nombrePionsPris() >= maxPionsPris)
                    {
                        prisesEtendues.Add(p2);
                        maxPionsPris = p2.taille;
                    }
                }
            }
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