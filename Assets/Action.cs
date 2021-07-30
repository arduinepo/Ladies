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
            caseDepart = new Case(l, c);
        }

        public abstract Case cazArrivee();
        
        public byte ligneDepart()
        {
            return caseDepart.ligne;
        }

        public byte colonneDepart()
        {
            return caseDepart.colonne;
        }

    }

    public class Mouvement : Action
    {
        private Case caseArrivee;

        public Mouvement(byte l1, byte c1, byte l2, byte c2) :base(l1, c1) { 
            caseArrivee = new Case(l2, c2);
        }

        public override Case cazArrivee()
        {
            return caseArrivee;
        }

        byte ligneArrivee() {
            return caseArrivee.ligne;
        }

        byte colonneArrivee() {
            return caseArrivee.colonne;
        }

    }

    public class Prise : Action
    {
        public List<Case> cases;

        public Prise(byte l1, byte c1):base(l1,c1) {
            cases = new List<Case>();
        }

        public static Prise prise(Prise p, byte l, byte c)
        {
            Prise prise = new Prise(p.caseDepart.ligne, p.caseDepart.colonne);
            foreach(Case ca in p.cases)
                prise.cases.Add(new Case(ca.ligne, ca.colonne));
            prise.cases.Add(new Case(l, c));
            return prise;
        }

        public int nombrePionsPris()
        {
            return cases.Count;
        }

        public override Case cazArrivee()
        {
            return this.cases.Count > 0 ? this.cases[this.cases.Count - 1] : this.caseDepart;
        }

        byte ligneArrivee()
        {
            return this.cazArrivee().ligne;
        }

        byte colonneArrivee()
        {
            return this.cazArrivee().colonne;
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

        public new static PriseDame prise(Prise p, byte l, byte c)
        {
            PriseDame prise = new PriseDame(p.caseDepart.ligne, p.caseDepart.colonne);
            foreach (Case ca in p.cases)
                prise.cases.Add(new Case(ca.ligne, ca.colonne));
            prise.cases.Add(new Case(l, c));
            return prise;
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
    }
}
