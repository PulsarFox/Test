using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BabaooTest
{
    public class Timer : MonoBehaviour
    {
        public int Timeleft = 181;

        [SerializeField]
        private TMP_Text TMtext;

        public void BeginTimer()
        {
            StartCoroutine(TimerCoroutine());
        }

        public IEnumerator TimerCoroutine()
        {
            string preText = "Temps restant :";
            while (Timeleft >= 0)
            {
                Timeleft--;
                this.TMtext.text = $"{preText} {Timeleft.ToString()} s";
                yield return new WaitForSeconds(1f);
            }
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

    }
}
