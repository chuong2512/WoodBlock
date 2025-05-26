// /*
// Created by Darsan
// */

using System.Collections;
using UnityEngine;

public class Splash : MonoBehaviour
{

    private IEnumerator Start()
    {
        if (!AdsManager.HaveSetupConsent)
        {
            SharedUIManager.ConsentPanel.Show();
            yield return new WaitUntil(() => AdsManager.HaveSetupConsent);
        }

        MyGame.GameManager.LoadScene("Main");
    }
}