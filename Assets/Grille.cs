using System;
using System.Collections.Generic;

namespace Assets
{
    //Collection de tous les mouvements et prises uniques par pion possibles
    //1 : passer en matrice 1D
    //2 : passer en matrice 1D sans les cases injouables

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

        const short HAUT_GAUCHE = 1, HAUT_DROIT = 2, BAS_DROIT = 3, BAS_GAUCHE = 4, HAUT = -1, GAUCHE = -1, BAS = 1, DROITE = 1;
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
                                prises.Add(p2);
                        }
                    }
                }
                    
            }
            actionsPossibles = actionsPossibles.filter(p => !prises.includes(p));
        }

        /*Génère tous les mouvements possibles dans le configuration courante suivant le tour des joueurs :
    * parcourt les cases voisines immédiates des pions simples, et les cases situées sur les diagonales des dames.
    */
        void mouvementsPossibles()
        {
            if (this.tourBlanc === BLANC)
            {
                for (let i = 0; i < this.grille.length; i++)
                    for (let j = (i % 2 === 0) ? 0 : 1; j < this.grille.length; j = j + 2)
                        if (this.grille[i][j] === PION_BLANC)
                            for (let pos = HAUT_GAUCHE; pos <= HAUT_DROIT; pos++)
                            {
                                let l = this.getLigneVoisine(i, pos), c = this.getColonneVoisine(j, pos);
                                if (l >= 0 && l < this.grille.length && c >= 0 && c < this.grille.length && this.grille[l][c] === CASE_VIDE)
                                    this.actionsPossibles.push(new Mouvement(i, j, l, c));
                            }
                        else if (this.grille[i][j] === DAME_BLANC)
                            for (let pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                            {
                                let ligne = this.getLigneVoisine(i, pos), col = this.getColonneVoisine(j, pos);
                                while (ligne >= 0 && ligne < this.grille.length && col >= 0 && col < this.grille.length
                                && this.grille[ligne][col] === CASE_VIDE)
                                {
                                    this.actionsPossibles.push(new Mouvement(i, j, ligne, col));
                                    ligne = this.getLigneVoisine(ligne, pos);
                                    col = this.getColonneVoisine(col, pos);
                                }
                            }
            }
            else {
                for (let i = 0; i < this.grille.length; i++)
                    for (let j = (i % 2 === 0) ? 0 : 1; j < this.grille.length; j = j + 2)
                        if (this.grille[i][j] === PION_NOIR)
                            for (let pos = BAS_GAUCHE; pos <= BAS_DROIT; pos++)
                            {
                                let l = this.getLigneVoisine(i, pos), c = this.getColonneVoisine(j, pos);
                                if (l >= 0 && l < this.grille.length && c >= 0 && c < this.grille.length && this.grille[l][c] === CASE_VIDE)
                                    this.actionsPossibles.push(new Mouvement(i, j, l, c));
                            }
                        else if (this.grille[i][j] === DAME_NOIR)
                            for (let pos = HAUT_GAUCHE; pos <= BAS_GAUCHE; pos++)
                            {
                                let ligne = this.getLigneVoisine(i, pos), col = this.getColonneVoisine(j, pos);
                                while (ligne >= 0 && ligne < this.grille.length && col >= 0 && col < this.grille.length
                                && this.grille[ligne][col] === CASE_VIDE)
                                {
                                    this.actionsPossibles.push(new Mouvement(i, j, ligne, col));
                                    ligne = this.getLigneVoisine(ligne, pos);
                                    col = this.getColonneVoisine(col, pos);
                                }
                            }
            }
        }

    }

}