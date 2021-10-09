namespace Assets
{
    public interface Heuristique
    {
        int evalue(Grille e);
        int evalueFin(Grille e);
    }

    public class BaseB : Heuristique
    {
        int vPion, vDame;

        public BaseB(int vP, int vD)
        {
            vPion = vP;
            vDame = vD;
        }

        public int evalue(Grille e)
        {
            return (e.nbPionsBlancs - e.nbPionsNoirs) * vPion + (e.nbDamesBlancs - e.nbDamesNoirs) * vDame;
        }

        public int evalueFin(Grille e)
        {
            switch (e.resultatBlanc())
            {
                case -1: return IAlphaBeta.MIN;
                case 0: return 0;
                case 1: return IAlphaBeta.MAX;
            }
            return 0;
        }

    }

    public class BaseN : Heuristique
    {
        int vPion, vDame;

        public BaseN(int vP, int vD)
        {
            vPion = vP;
            vDame = vD;
        }

        public int evalue(Grille e)
        {
            return (e.nbPionsNoirs - e.nbPionsBlancs) * vPion + (e.nbDamesNoirs - e.nbDamesBlancs) * vDame;
        }

        public int evalueFin(Grille e)
        {
            switch (e.resultatNoir())
            {
                case -1: return IAlphaBeta.MIN;
                case 0: return 0;
                case 1: return IAlphaBeta.MAX;
            }
            return 0;
        }

    }

}