using UnityEngine;

namespace Assets
{
    class TestGrille : MonoBehaviour
    {
        void Awake()
        {
            Grille tg = new Grille();
            tg.grille = new int[10, 10] {
                { 0, 0, 0,-1, 0, 0, 0,-1, 0,-1},
                {-1, 0, 0, 0,-1, 0,-1, 0,-1, 0},
                { 0,-1, 0, 0, 0,-1, 0, 0, 0,-1},
                {-1, 0,-1, 0,-1, 0,-1, 0, 0, 0},
                { 0, 0, 0, 0, 0, 2, 0, 0, 0, 0},
                { 0, 0, 0, 0,-1, 0, 0, 0, 0, 0},
                { 0, 1, 0, 0, 0, 1, 0, 1, 0, 1},
                { 1, 0, 0, 0, 1, 0, 1, 0, 1, 0},
                { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1},
                { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0}
            };
            tg.genereActionsPossibles();
            foreach (Action a in tg.actionsPossibles)
                Debug.Log(a);

        }

    }
}
