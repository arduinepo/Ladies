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
        static short[,] GRILLE_10 = new short[10, 10] {
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

        static Case[,] GRILLE_CASES;

        const byte HAUT_GAUCHE = 1, HAUT_DROIT = 2, BAS_DROIT = 3, BAS_GAUCHE = 4;
        const short HAUT = -1, GAUCHE = -1, BAS = 1, DROITE = 1;
        const short PION_BLANC = 1, PION_NOIR = -1, DAME_BLANC = 2, DAME_NOIR = -2, VIDE = 0;
        const bool TOUR_BLANCS = true, TOUR_NOIRS = false;

        short[,] grille;
        bool tour;
        byte nbPionsBlancs, nbPionsNoirs, nbDamesBlancs, nbDamesNoirs;

        List<Action> actionsPossibles;
        #endregion

        static Grille()
        {
            GRILLE_CASES = new Case[10, 10];
            for (byte i = 0; i < 10; i++)
                for (byte j = 0; j < 10; j++)
                    GRILLE_CASES[i, j] = new Case(i, j);
        }
        public Grille()
        {
            grille = (short[,])GRILLE_10.Clone();
            tour = TOUR_BLANCS;
            nbPionsBlancs = nbPionsNoirs = 20;
            nbDamesBlancs = nbDamesNoirs = 0;
        }

        public void genereActionsPossibles()
        {
            actionsPossibles = new List<Action>();
            prises();
            mouvementsPossibles();
        }

        /* Réalise l'action a, et promeut le pion en dame si nécessaire. Bascule le tour des joueurs.*/
        void realiserAction(Action a)
        {
            bool promotion;
            if (a is PriseDame)
                promotion = prendre((PriseDame)a);
            else if (a is Prise)
                promotion = prendre((Prise)a);
            else
                promotion = deplacer((Mouvement)a);
            if (promotion)
            {
                Case c = a.cazArrivee();
                ennoblir(c.ligne, c.colonne);
            }

            tour = !this.tour;
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

        /* Réalise le mouvement m, renvoie vrai si le pion doit être promu en dame. */
        bool deplacer(Mouvement m)
        {
            Case a = m.cazArrivee(), d = m.caseDepart;
            byte l1 = a.ligne, l2 = d.ligne, c1 = a.colonne, c2 = d.colonne;
            grille[l1, c1] = grille[l2, c2];
            grille[l2, c2] = VIDE;
            return (l1 == grille.Length - 1 && grille[l1, c1] == PION_NOIR) || (l1 == 0 && grille[l1, c1] == PION_BLANC);
        }

        private bool prendre(PriseDame p)
        {
            Case c = null;
            for (int i = 0; i < p.cases.Count; i += 2)
            {
                c = p.cases[i];
                switch (grille[c.ligne, c.colonne])
                {
                    case PION_BLANC:
                        nbPionsBlancs--;
                        break;
                    case DAME_BLANC:
                        nbDamesBlancs--;
                        break;
                    case PION_NOIR:
                        nbPionsNoirs--;
                        break;
                    case DAME_NOIR:
                        nbDamesNoirs--; break;
                }
                grille[c.ligne, c.colonne] = 0;
            }
            grille[c.ligne, c.colonne] = grille[p.caseDepart.ligne, p.caseDepart.colonne];
            grille[p.caseDepart.ligne, p.caseDepart.colonne] = 0;
            return false;
        }

        /* Réalise la prise p ; met à jour le nombre de pions/dames restants. Renvoie vrai le pion preneur doit être promu en dame. */
        private bool prendre(Prise p)
        {
            Case c = null, casePrec = p.caseDepart;
            for (int i = 0; i < p.cases.Count; i++)
            {
                c = p.cases[i];
                int lignePion = casePrec.ligne > c.ligne ? casePrec.ligne - 1 : c.ligne - 1,
                        colPion = casePrec.colonne > c.colonne ? casePrec.colonne - 1 : c.colonne - 1;
                switch (grille[lignePion, colPion])
                {
                    case PION_BLANC:
                        nbPionsBlancs--;
                        break;
                    case DAME_BLANC:
                        nbDamesBlancs--;
                        break;
                    case PION_NOIR:
                        nbPionsNoirs--;
                        break;
                    case DAME_NOIR:
                        nbDamesNoirs--; break;
                }
                grille[lignePion, colPion] = 0;
                casePrec = c;
            }
            grille[c.ligne, c.colonne] = grille[p.caseDepart.ligne, p.caseDepart.colonne];
            grille[p.caseDepart.ligne, p.caseDepart.colonne] = 0;
            return (c.ligne == grille.Length - 1 && grille[c.ligne, c.colonne] == PION_NOIR) ||
                (c.ligne == 0 && grille[c.ligne, c.colonne] == PION_BLANC);
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
        * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
        */
        void mouvementsPossibles()
        {
            if (tour == TOUR_BLANCS)
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 0 : 1); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_BLANC)
                            for (short pos = HAUT_GAUCHE; pos <= HAUT_DROIT; pos++)
                            {
                                byte l = getLigneVoisine(i, pos), c = getColonneVoisine(j, pos);
                                if (l >= 0 && l < grille.Length && c >= 0 && c < grille.Length && grille[l, c] == VIDE)
                                    actionsPossibles.Add(new Mouvement(i, j, l, c));
                            }
                        else if (grille[i, j] == DAME_BLANC)
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                            {
                                byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos);
                                while (ligne >= 0 && ligne < grille.Length && col >= 0 && col < grille.Length
                                && grille[ligne, col] == VIDE)
                                {
                                    actionsPossibles.Add(new Mouvement(i, j, ligne, col));
                                    ligne = getLigneVoisine(ligne, pos);
                                    col = getColonneVoisine(col, pos);
                                }
                            }
            }
            else
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 0 : 1); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_NOIR)
                            for (short pos = BAS_GAUCHE; pos <= BAS_DROIT; pos++)
                            {
                                byte l = getLigneVoisine(i, pos), c = getColonneVoisine(j, pos);
                                if (l >= 0 && l < grille.Length && c >= 0 && c < grille.Length && grille[l, c] == VIDE)
                                    actionsPossibles.Add(new Mouvement(i, j, l, c));
                            }
                        else if (grille[i, j] == DAME_NOIR)
                            for (byte pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                            {
                                byte ligne = getLigneVoisine(i, pos), col = getColonneVoisine(j, pos);
                                while (ligne >= 0 && ligne < grille.Length && col >= 0 && col < grille.Length
                                && grille[ligne, col] == VIDE)
                                {
                                    actionsPossibles.Add(new Mouvement(i, j, ligne, col));
                                    ligne = getLigneVoisine(ligne, pos);
                                    col = getColonneVoisine(col, pos);
                                }
                            }
            }
        }

        /* Génère toutes les prises possibles de tous les pions du joueur qui a le tour. */
        void prises()
        {
            if (tour == TOUR_BLANCS)
            {
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 0 : 1); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_BLANC || grille[i, j] == DAME_BLANC)
                            prisesPion(i, j);
            }
            else
                for (byte i = 0; i < grille.Length; i++)
                    for (byte j = (byte)((i % 2 == 0) ? 0 : 1); j < grille.Length; j += 2)
                        if (grille[i, j] == PION_NOIR || grille[i, j] == DAME_NOIR)
                            prisesPion(i, j);
            triePrisesDames();
        }

        /* Génère toutes les prises possibles d'un pion. */
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

        /* Etend la prise p : renvoie toutes les prises résultant de son extension aux pions adverses voisins du pion preneur ou situés sur la diagonale de la dame preneuse, si une case libre se trouve derrière eux. */
        List<Prise> etendrePrise(Prise p)
        {
            List<Prise> prises = new List<Prise>();
            Case c = p.cazArrivee();
            byte ligneArrivee = c.ligne, colArrivee = c.colonne;
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
                                if (caseOccupeeParAdversaire(grille[p.ligneDepart(), p.colonneDepart()], ligne, col)
                                    && caseApresSautLibreOuContientPionPreneur(p, ligne, col, position)
                                    && !p.pionVirtuellementPris(ligne, col))
                                    priseD = PriseDame.prise(p, ligne, col);
                                else
                                    secondPionNonRencontre = false;
                            }
                            else
                                secondPionNonRencontre = false;
                        }
                        else if (!premierPionNonRencontre && priseD != null)
                            prises.Add(Prise.prise(priseD, ligne, col));
                    }
                }
            }
            else for (short position = HAUT_GAUCHE; position <= BAS_GAUCHE; position++)
                {
                    byte ligne = getLigneVoisine(ligneArrivee, position), col = getColonneVoisine(colArrivee, position);
                    if (peutEtendrePrise(p, position) && !p.pionVirtuellementPris(ligne, col))
                        prises.Add(Prise.prise(p, getLigneVoisine(ligne, position), getColonneVoisine(col, position)));
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
                        if (p is PriseDame) {
                            p2 = (PriseDame)p;
                            if(p1.prendMemePionsMemeOrdre(p2))
                                prises.RemoveAt(j);
                        }
                    }
                }
            }
        }

        bool caseOccupeeParAdversaire(short pion, byte ligneCase, byte colCase)
        {
           short pionCase = grille[ligneCase,colCase];
           return (pion < 0 && pionCase > 0) || (pion > 0 && pionCase < 0);
        }

        bool caseApresSautLibreOuContientPionPreneur(Prise p, byte l, byte c, short pos)
        {
            byte ligne = getLigneVoisine(l, pos), col = getColonneVoisine(c, pos);
            if (ligne < 0 || ligne >= grille.Length || col < 0 || col >= grille.Length)
                return false;
            return grille[ligne,col] == VIDE || (ligne == p.ligneDepart() && col == p.colonneDepart());
        }

        bool peutEtendrePrise(Prise p, short positionCase)
        {
            Case c = p.cazArrivee();
            byte l = c.ligne, co = c.colonne;
            return contientPionAdverse(grille[p.ligneDepart(),p.colonneDepart()], l, co,
                positionCase) && this.caseApresSautLibreOuContientPionPreneur(p, this.getLigneVoisine(l, positionCase), this.getColonneVoisine(co, positionCase), positionCase);
        }

        bool contientPionAdverse(short pion, byte ligne, byte col, short position)
        {
            switch (position)
            {
                case (HAUT_GAUCHE):
                    return ligne + HAUT >= 0 && col + GAUCHE >= 0 && ((pion > 0 && grille[ligne + HAUT,col + GAUCHE] < 0)
                        || (pion < 0 && grille[ligne + HAUT,col + GAUCHE] > 0));
                case (HAUT_DROIT):
                    return ligne + HAUT >= 0 && col + DROITE < this.grille.Length
                        && ((pion > 0 && grille[ligne + HAUT,col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + HAUT,col + DROITE] > 0));
                case (BAS_DROIT):
                    return ligne + BAS < grille.Length && col + DROITE < this.grille.Length
                        && ((pion > 0 && grille[ligne + BAS,col + DROITE] < 0)
                            || (pion < 0 && grille[ligne + BAS,col + DROITE] > 0));
                case (BAS_GAUCHE):
                    return ligne + BAS < grille.Length && col + GAUCHE >= 0
                        && ((pion > 0 && grille[ligne + BAS,col + GAUCHE] < 0)
                            || (pion < 0 && grille[ligne + BAS,col + GAUCHE] > 0));
            }
            return false;
        }

        const byte ERR = 101;

        byte getLigneVoisine(byte ligne, short position)
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

        byte getColonneVoisine(byte col, short position)
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
                    switch (grille[i,j])
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