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

        public override string ToString()
        {
            return ligne + "-" + colonne;
        }

    }

    public interface Action
    {
        Case cazArrivee();
        void realiser(Grille g);
        bool commenceEtPasse(Case a1, Case a2);

        string ToString();
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

        public override string ToString()
        {
            return "" + caseDepart.ligne + "-" + caseDepart.colonne + " -> " + caseArrivee.ligne + "-" + caseArrivee.colonne;
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

        public override string ToString()
        {
            return "Pion Blanc : " + base.ToString();
        }

    }

    public class ArriveeBlanc : Mouvement
    {
        public ArriveeBlanc(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.DAME_BLANC;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            d.nbPionsBlancs--;
            d.nbDamesBlancs++;
        }

        public override string ToString()
        {
            return "Pion Blanc : " + base.ToString();
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

        public override string ToString()
        {
            return "Dame Blanc : " + base.ToString();
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

        public override string ToString()
        {
            return "Pion noir : " + base.ToString();
        }

    }

    public class ArriveeNoir : Mouvement
    {
        public ArriveeNoir(Case c1, Case c2) : base(c1, c2)
        {
        }

        public override void realiser(Grille d)
        {
            d.grille[caseArrivee.ligne, caseArrivee.colonne] = Grille.DAME_NOIR;
            d.grille[caseDepart.ligne, caseDepart.colonne] = Grille.VIDE;
            d.nbPionsNoirs--;
            d.nbDamesNoirs++;
        }

        public override string ToString()
        {
            return "Pion noir : " + base.ToString();
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

        public override string ToString()
        {
            return "Dame noir : " + base.ToString();
        }
    }

    public abstract class Prise : Action
    {
        public List<Case> cases;
        public int lastDir;

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
            cases = new List<Case>(p.cases.GetRange(0,p.cases.Count-1));
            cases.Add(c);
            lastDir = p.lastDir;
        }

        public void poursuit(Case c1,Case c2,int pos)
        {
            cases.Add(c1);
            cases.Add(c2);
            lastDir = pos;
        }

        public int nombrePionsPris()
        {
            return cases.Count / 2;
        }

        public Case cazArrivee()
        {
            return cases[cases.Count - 1];
        }

        public bool pionVirtuellementPris(int l, int c)
        {
            for (int i = 1; i < cases.Count; i += 2)
            {
                if (l == cases[i].ligne && c == cases[i].colonne)
                    return true;
            }
            return false;
        }

        public bool commenceEtPasse(Case a1, Case a2)
        {
            if (a1 == cases[0])
                for (int i = 1; i < cases.Count; i++)
                    if (a2==cases[i])
                        return true;
            return false;
        }

        public abstract void realiser(Grille g);

        public override string ToString()
        {
            string s = "";
            foreach (Case c in cases)
                s += c.ligne + "-" + c.colonne + " -> ";
            return s.Substring(0, s.Length - 4);
        }

    }

    public class PriseBlanc : Prise
    {
        
        public PriseBlanc(Prise p, Case c,Case c2,int pos) : base(p, c)
        {
            cases.Add(c2);
            lastDir = pos;
        }

        public PriseBlanc(Case c1, Case c2,Case c3,int pos) : base(c1, c2)
        {
            cases.Add(c3);
            lastDir = pos;
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
            c = cases[cases.Count - 1];
            if (c.ligne == 0)
            {
                d.grille[c.ligne, c.colonne] = Grille.DAME_BLANC;
                d.nbPionsBlancs--;
                d.nbDamesBlancs++;
            }
            else
                d.grille[c.ligne, c.colonne] = Grille.PION_BLANC;
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

        public override string ToString()
        {
            return "Pion Blanc : "+base.ToString();
        }
    }

    public class PriseNoir : Prise
    {
        public PriseNoir(Prise p, Case c, Case c2,int pos) : base(p, c)
        {
            cases.Add(c2);
            lastDir = pos;
        }

        public PriseNoir(Case c1, Case c2, Case c3, int pos) : base(c1, c2)
        {
            cases.Add(c3);
            lastDir = pos;
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
            c = cases[cases.Count - 1];
            if (c.ligne == d.taille - 1)
            {
                d.grille[c.ligne, c.colonne] = Grille.DAME_NOIR;
                d.nbPionsNoirs--;
                d.nbDamesNoirs++;
            }
            else
                d.grille[c.ligne, c.colonne] = Grille.PION_NOIR;
            d.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

        public override string ToString()
        {
            return "Pion Noir : " + base.ToString();
        }
    }

    public abstract class PriseDame : Prise
    {
        public PriseDame(PriseDame p, Case c) : base(p,c)
        {
        }

        public PriseDame(Case c1, Case c2,Case c3,int pos) : base(c1, c2)
        {
            cases.Add(c3);
            lastDir = pos;
        }

        public PriseDame() : base() { }

        public bool prendMemePionsMemeOrdre(PriseDame p2)
        {
            int s1 = cases.Count, s2 = p2.cases.Count;
            if( s1 != s2 || cases[0] != p2.cases[0]||cases[s1-1]!=p2.cases[s2-1])
                return false;
            for(int i = 1; i < s1; i += 2)
            {
                if (cases[i] != p2.cases[i])
                    return false;
            }
            return true;
        }

    }

    public class PriseBlanche : PriseDame
    {
        public PriseBlanche(PriseDame p, Case c) : base(p, c)
        {
        }

        public PriseBlanche() : base() { }

        public PriseBlanche(PriseDame p, Case c1,Case c2,int pos) : base()
        {
            cases = new List<Case>(p.cases.GetRange(0, p.cases.Count - 2));
            cases.Add(c1);
            cases.Add(c2);
            lastDir = pos;
        }

        public PriseBlanche(Case c1, Case c2,Case c3,int pos) : base(c1, c2,c3,pos)
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
                g.grille[c.ligne, c.colonne] = Grille.VIDE;
            }
            c = cases[cases.Count - 1];
            g.grille[c.ligne, c.colonne] = Grille.DAME_BLANC;
            g.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

        public override string ToString()
        {
            return "Dame Blanc : " + base.ToString();
        }
    }

    public class PriseNoire : PriseDame
    {
        public PriseNoire(PriseDame p, Case c) : base(p, c)
        {
        }

        public PriseNoire() : base()
        {
        }

        public PriseNoire(PriseDame p, Case c1, Case c2, int pos) : base()
        {
            cases = new List<Case>(p.cases.GetRange(0, p.cases.Count - 2));
            cases.Add(c1);
            cases.Add(c2);
            lastDir = pos;
        }

        public PriseNoire(Case c1, Case c2,Case c3,int pos) : base(c1, c2,c3,pos)
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
            c = cases[cases.Count - 1];
            g.grille[c.ligne, c.colonne] = Grille.DAME_NOIR;
            g.grille[cases[0].ligne, cases[0].colonne] = 0;
        }

        public override string ToString()
        {
            return "Dame noirs : " + base.ToString();
        }

    }

}
