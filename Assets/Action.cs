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
        bool realiser(Grille g);
        bool commenceEtPasse(Case a1, Case a2);

    }

    public class Mouvement : Action
    {
        private Case caseDepart, caseArrivee;

        public Mouvement(byte l1, byte c1, byte l2, byte c2)
        {
            caseDepart = Grille.CASES[l1, c1];
            caseArrivee = Grille.CASES[l2, c2];
        }

        public Case cazArrivee()
        {
            return caseArrivee;
        }

        public bool realiser(Grille d)
        {
            int pion=d.grille[caseArrivee.ligne, caseArrivee.colonne] = d.grille[caseDepart.ligne, caseDepart.colonne];
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            switch (pion)
            {
                case (Grille.PION_BLANC):
                    d.pionsBlancs.Remove(Grille.CASES[caseDepart.ligne, caseDepart.colonne]);
                    d.pionsBlancs.Add(Grille.CASES[caseArrivee.ligne, caseArrivee.colonne]); break;
                case (Grille.PION_NOIR):
                    d.pionsNoirs.Remove(Grille.CASES[caseDepart.ligne, caseDepart.colonne]);
                    d.pionsNoirs.Add(Grille.CASES[caseArrivee.ligne, caseArrivee.colonne]); break;
                case (Grille.DAME_BLANC):
                    d.damesBlancs.Remove(Grille.CASES[caseDepart.ligne, caseDepart.colonne]);
                    d.damesBlancs.Add(Grille.CASES[caseArrivee.ligne, caseArrivee.colonne]); break;
                case (Grille.DAME_NOIR):
                    d.damesNoirs.Remove(Grille.CASES[caseDepart.ligne, caseDepart.colonne]);
                    d.damesNoirs.Add(Grille.CASES[caseArrivee.ligne, caseArrivee.colonne]); break;
            }
            return (caseArrivee.ligne == d.grille.Length - 1 && d.grille[caseArrivee.ligne, caseArrivee.colonne] == Grille.PION_NOIR)
                || (caseArrivee.ligne == 0 && d.grille[caseArrivee.ligne, caseArrivee.colonne] == Grille.PION_BLANC);

        }

        public bool commenceEtPasse(Case a1,Case a2)
        {
            return a1 == caseDepart && a2 == caseArrivee;
        }

    }

    public class Prise : Action
    {
        public List<Case> cases;

        public Prise(byte l1, byte c1)
        {
            cases = new List<Case>();
            cases[0] = Grille.CASES[l1, c1];
        }

        public Prise(Prise p, byte l, byte c)
        {
            cases = new List<Case>();
            foreach (Case ca in p.cases)
                cases.Add(Grille.CASES[ca.ligne, ca.colonne]);
            cases.Add(Grille.CASES[l, c]);
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

        public bool realiser(Grille d)
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
                    case Grille.PION_NOIR:
                        d.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        d.nbDamesNoirs--; break;
                }
                d.grille[c.ligne, c.colonne] = 0;
            }
            d.grille[c.ligne, c.colonne] = d.grille[cases[0].ligne, cases[0].colonne];
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
            return (c.ligne == d.grille.Length - 1 && d.grille[c.ligne, c.colonne] == Grille.PION_NOIR) ||
                (c.ligne == 0 && d.grille[c.ligne, c.colonne] == Grille.PION_BLANC);
        }

        public bool commenceEtPasse(Case a1,Case a2)
        {
            if (a1 == cases[0])
            {
                for (int i = 1; i < cases.Count; i++)
                    if (a2.equals(cases[i]))
                        return true;
                return false;
            }
            return false;
        }

    }

    public class PriseDame : Prise
    {
        public PriseDame(byte l1, byte c1) : base(l1, c1) { }

        public PriseDame(Prise p, byte l, byte c) : base(p, l, c)
        {
        }

        public bool prendMemePionsMemeOrdre(Prise prise)
        {
            if (!cases[0].equals(prise.cases[0]) || cases.Count != prise.cases.Count
                || !cases[cases.Count - 1].equals(prise.cases[prise.cases.Count - 1]))
                return false;
            int i = 1;
            for (; i < cases.Count - 1 && this.cases[i].equals(prise.cases[i]); i += 2)
            {
            }
            return i == this.cases.Count;
        }

        public new bool realiser(Grille d)
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
                    case Grille.PION_NOIR:
                        d.nbPionsNoirs--;
                        break;
                    case Grille.DAME_NOIR:
                        d.nbDamesNoirs--; break;
                }
                d.grille[c.ligne, c.colonne] = 0;
            }
            d.grille[c.ligne, c.colonne] = d.grille[cases[0].ligne, cases[0].colonne];
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
            return false;
        }
    }

}
