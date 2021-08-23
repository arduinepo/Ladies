using System.Collections.Generic;

namespace Assets
{
    public class Case
    {
        public byte ligne, colonne;

        public Case(byte l, byte c)
        {
            ligne = l;
            colonne = c;
        }

        public bool equals(Case c)
        {
            return ligne == c.ligne && colonne == c.colonne;
        }

    }

    public interface Action
    {
        Case cazArrivee();
        void realiser(Grille g);
        bool commenceEtPasse(Case a1, Case a2);

    }

    public abstract class Mouvement : Action
    {
        public Case caseDepart, caseArrivee;

        public Mouvement(Case c1, Case c2)
        {
            caseDepart = c1;
            caseArrivee = c2;
        }

        public Case cazArrivee()
        {
            return caseArrivee;
        }

        public abstract void realiser(Grille g);

        public bool commenceEtPasse(Case a1, Case a2)
        {
            return a1 == caseDepart && a2 == caseArrivee;
        }

    }

    public class MouvementBlanc : Mouvement
    {
        public MouvementBlanc(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.PION_BLANC;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
        }

    }

    public class ArriveeBlanc : Mouvement
    {
        public ArriveeBlanc(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.PION_BLANC;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            d.ennoblirBlanc(caseArrivee);
        }
    }

    public class MouvementBlanche : Mouvement
    {
        public MouvementBlanche(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.DAME_BLANC;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
        }
    }

    public class MouvementNoir : Mouvement
    {
        public MouvementNoir(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.PION_NOIR;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
        }

    }

    public class ArriveeNoir : Mouvement
    {
        public ArriveeNoir(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.PION_NOIR;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            d.ennoblirBlanc(caseArrivee);
        }
    }
    
    public class MouvementNoire : Mouvement
    {
        public MouvementNoire(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.DAME_NOIR;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
        }
    }

    public abstract class Prise : Action
    {
        public List<Case> cases;

        public Prise()
        {
            cases = new List<Case>();
        }

        public Prise(Case c1, Case c2)
        {
            cases = new List<Case>();
            cases.Add(c1);
            cases.Add(c2);
        }

        public Prise(Prise p, Case c)
        {
            cases = new List<Case>();
            for (int i = 0; i < p.cases.Count - 2; i++)
                cases.Add(p.cases[i]);
            cases.Add(c);
        }

        public int nombrePionsPris()
        {
            return cases.Count / 2;
        }

        public Case cazArrivee()
        {
            return cases[cases.Count - 1];
        }

        public bool pionVirtuellementPris(byte l, byte c)
        {
            for (int i = 1; i < cases.Count; i += 2)
                if (l == cases[i].ligne && c == cases[i].colonne)
                    return true;
            return false;
        }

        // mémoire cases pions grille

        public bool commenceEtPasse(Case a1, Case a2)
        {
            if (a1 == cases[0])
                for (int i = 1; i < cases.Count; i++)
                    if (a2.equals(cases[i]))
                        return true;
            return false;
        }

        public abstract void realiser(Grille g);
    }

    public class PriseBlanc : Prise
    {
        public PriseBlanc(Case c1, Case c2) : base(c1, c2)
        {
        }

        public PriseBlanc(Prise p, Case c) : base(p, c)
        {
        }

        public override void realiser(Grille d)
        {
            Case c = null;
            for (int i = 1; i < cases.Count; i += 2)
            {
                c = cases[i];
                switch (d.grille[c.ligne, c.colonne])
                {
                    case Grille.PION_NOIR:
                        d.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        d.nbDamesNoirs--; break;
                }
                d.grille[c.ligne, c.colonne] = 0;
            }
            d.grille[c.ligne, c.colonne] = Grille.PION_BLANC;
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

    }

    public class PriseNoir : Prise
    {
        public PriseNoir(Case c1, Case c2) : base(c1, c2)
        {
        }

        public PriseNoir(Prise p, Case c) : base(p, c)
        {
        }

        public override void realiser(Grille d)
        {
            Case c = null;
            for (int i = 1; i < cases.Count; i += 2)
            {
                c = cases[i];
                switch (d.grille[c.ligne, c.colonne])
                {
                    case Grille.PION_BLANC:
                        d.nbPionsBlancs--;
                        break;
                    case Grille.DAME_BLANC:
                        d.nbDamesBlancs--;
                        break;
                }
                d.grille[c.ligne, c.colonne] = 0;
            }
            d.grille[c.ligne, c.colonne] = Grille.PION_NOIR;
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

    }

    public abstract class PriseDame : Prise
    {
        public PriseDame(PriseDame p, Case c) : base()
        {
            cases.RemoveAt(cases.Count - 1);
            cases.Add(c);
        }

        public PriseDame(Case p, Case c) : base(p, c)
        {
        }

        internal bool prendMemePionsMemeOrdre(PriseDame p2)
        {
            
        }
    }

    public class PriseBlanche : PriseDame
    {
        public PriseBlanche(PriseDame p, Case c) : base(p, c)
        {
        }

        public PriseBlanche(Case p, Case c) : base(p, c)
        {
        }

        public override void realiser(Grille g)
        {
            Case c = null;
            for (int i = 1; i < cases.Count; i += 2)
            {
                c = cases[i];
                switch (g.grille[c.ligne, c.colonne])
                {
                    case Grille.PION_NOIR:
                        g.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        g.nbDamesNoirs--; break;
                }
                g.grille[c.ligne, c.colonne] = 0;
            }
            g.grille[c.ligne, c.colonne] = Grille.DAME_BLANC;
            g.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

    }

    public class PriseNoire : PriseDame
    {
        public PriseNoire(PriseDame p, Case c) : base(p, c)
        {
        }

        public PriseNoire(Case p, Case c) : base(p, c)
        {
        }

        public override void realiser(Grille g)
        {
            Case c = null;
            for (int i = 1; i < cases.Count; i += 2)
            {
                c = cases[i];
                switch (g.grille[c.ligne, c.colonne])
                {
                    case Grille.PION_BLANC:
                        g.nbPionsBlancs--;
                        break;
                    case Grille.DAME_BLANC:
                        g.nbDamesBlancs--; break;
                }
                g.grille[c.ligne, c.colonne] = 0;
            }
            g.grille[c.ligne, c.colonne] = Grille.DAME_NOIR;
            g.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

    }

}
