using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BabaooTest
{

    public class GameMager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            DontDestroyOnLoad(this.gameObject);
        }

    }
}
