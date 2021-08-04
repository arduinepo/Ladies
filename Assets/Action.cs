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

    public abstract class Action
    {
        public Case caseDepart;

        protected Action(byte l, byte c)
        {
            caseDepart = Grille.CASES[l,c];
        }

        public abstract Case cazArrivee();
        public abstract bool realiser(Grille g);
        
    }

    public class Mouvement : Action
    {
        private Case caseArrivee;

        public Mouvement(byte l1, byte c1, byte l2, byte c2) :base(l1, c1) { 
            caseArrivee = Grille.CASES[l2,c2];
        }

        public override Case cazArrivee()
        {
            return caseArrivee;
        }

        public override bool realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = d.grille[caseDepart.ligne, caseDepart.colonne];
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            return (caseArrivee.ligne == d.grille.Length - 1 && d.grille[caseArrivee.ligne, caseArrivee.colonne] == Grille.PION_NOIR)
                || (caseArrivee.ligne == 0 && d.grille[caseArrivee.ligne, caseArrivee.colonne] == Grille.PION_BLANC);

        }

    }

    public class Prise : Action
    {
        public List<Case> cases;

        public Prise(byte l1, byte c1):base(l1,c1) {
            cases = new List<Case>();
        }

        public Prise(Prise p, byte l, byte c):base(p.caseDepart.ligne,p.caseDepart.colonne)
        {
            cases = new List<Case>();
            foreach (Case ca in p.cases)
                cases.Add(Grille.CASES[ca.ligne, ca.colonne]);
            cases.Add(Grille.CASES[l, c]);
        }

        public int nombrePionsPris()
        {
            return cases.Count;
        }

        public override Case cazArrivee()
        {
            return this.cases.Count > 0 ? this.cases[this.cases.Count - 1] : this.caseDepart;
        }

        public bool pionVirtuellementPris(byte l, byte c)
        {
            if (cases.Count == 0)
                return false;
            Case caseDepart = this.caseDepart;
            for (int i = 0; i < cases.Count; i++)
            {
                if (((caseDepart.ligne == l + 1 && l == cases[i].ligne + 1) || (caseDepart.ligne == l - 1 && l == cases[i].ligne - 1))
                    && ((caseDepart.colonne == c + 1 && c == cases[i].colonne + 1) || (caseDepart.colonne == c - 1 && c == cases[i].colonne - 1)))
                    return true;
                caseDepart = cases[i];
            }
            
            return false;
        }

        public override bool realiser(Grille d)
        {
            Case c = null, casePrec = caseDepart;
            for (int i = 0; i < cases.Count; i++)
            {
                c = cases[i];
                int lignePion = casePrec.ligne > c.ligne ? casePrec.ligne - 1 : c.ligne - 1,
                        colPion = casePrec.colonne > c.colonne ? casePrec.colonne - 1 : c.colonne - 1;
                switch (d.grille[lignePion, colPion])
                {
                    case Grille.PION_BLANC:
                        d.nbPionsBlancs--;
                        break;
                    case Grille.DAME_BLANC:
                        d.nbDamesBlancs--;
                        break;
                    case Grille.PION_NOIR:
                        d.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        d.nbDamesNoirs--; break;
                }
                d.grille[lignePion, colPion] = 0;
                casePrec = c;
            }
            d.grille[c.ligne, c.colonne] = d.grille[caseDepart.ligne, caseDepart.colonne];
            d.grille[caseDepart.ligne, caseDepart.colonne] = 0;
            return (c.ligne ==d.grille.Length - 1 && d.grille[c.ligne, c.colonne] == Grille.PION_NOIR) ||
                (c.ligne == 0 && d.grille[c.ligne, c.colonne] == Grille.PION_BLANC);

        }
    }

    public class PriseDame : Prise
    {
        public PriseDame(byte l1, byte c1) : base(l1, c1) { }

        public new int nombrePionsPris()
        {
            return this.cases.Count/2;
        }

        public new bool pionVirtuellementPris(byte l, byte c)
        {
            if (cases.Count == 0)
                return false;
            for (int i = 0; i < cases.Count; i += 2)
                if (l == cases[i].ligne && c == cases[i].colonne)
                    return true;
            return false;
        }

        public PriseDame(Prise p, byte l, byte c):base(p,l,c)
        {
        }

        public bool prendMemePionsMemeOrdre(Prise prise)
        {
            if (!caseDepart.equals(prise.caseDepart) || cases.Count != prise.cases.Count
                || !cases[cases.Count - 1].equals(prise.cases[prise.cases.Count - 1]))
                return false;
            int i = 0;
            for (; i < cases.Count-1 && this.cases[i].equals(prise.cases[i]); i += 2)
            {
            }
            return i == this.cases.Count;
        }

        public override bool realiser(Grille d)
        {
            Case c = null;
            for (int i = 0; i < cases.Count; i += 2)
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
                    case Grille.PION_NOIR:
                        d.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        d.nbDamesNoirs--; break;
                }
                d.grille[c.ligne, c.colonne] = 0;
            }
            d.grille[c.ligne, c.colonne] = d.grille[caseDepart.ligne, caseDepart.colonne];
            d.grille[caseDepart.ligne, caseDepart.colonne] = 0;
            return false;
        }
    }
}
