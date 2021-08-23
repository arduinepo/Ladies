using System;
using System.Collections.Generic;

namespace Assets
{
    public class Etat : Grille
    {
        public int valeur;
        public static int MAX = Int32.MaxValue, MIN = Int32.MinValue;
        public List<int> pionsPris = new List<int>();

        public Action meilleurCoup(int profondeur, Heuristique heuristique)
        {
            valeur = MIN;
            int a = MIN, b = MAX;
            genereActionsPossibles();
            foreach (Action ac in actionsPossibles)
            {
                realiserAction(ac, pionsPris);
                int v = calcValeur(profondeur - 1, heuristique);
                annuleAction(ac);
                pionsPris = new List<int>();
            }

        }

        public int calcValeur(int profondeur, Heuristique heur)
        {
            if (profondeur == 0)
                return heur.evalue(this);

        }

        public void annuleAction(Mouvement a)
        {

        }

        public void annuleAction(Prise p)
        {

        }

    }

    public class EtatMin : Etat
    {

    }

    public class EtatMax : Etat
    {

    }

    public interface Heuristique
    {
        int evalue(Etat e);
    }

    public class Base : Heuristique
    {
        int vPion, vDame;

        public int evalue(Etat e)
        {
            return (e.nbPionsBlancs - e.nbPionsNoirs) * vPion + (e.nbDamesBlancs - e.nbDamesNoirs) * vDame;
        }

    }

}