using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BabaooTest
{
    public class HomeScore : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text TMPScore;

        private void OnEnable()
        {
            TMPScore.text = $"Meilleur Score : {Taquin.bestScore.ToString()}";
        }

    }
}
