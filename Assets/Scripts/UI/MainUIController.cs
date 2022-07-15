using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// A simple controller for switching between UI panels.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        public GameObject[] panels;
        public GameObject streamerCodeInput;
        public TangiaSpawner tangiaSpawner;
        public Text resultText;

        public void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var g = panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }

        public void SetStreamerCode()
        {
            StartCoroutine(nameof(setStreamerCode));
        }
        private IEnumerator setStreamerCode()
        {
            string text = streamerCodeInput.GetComponent<TMPro.TMP_InputField>().text;
            yield return tangiaSpawner.Login(text);
            resultText.text = tangiaSpawner.LoginResult.Success ? "SUCCESS!" : "Ups: " + tangiaSpawner.LoginResult.ErrorMessage;
        }

        void OnEnable()
        {
            SetActivePanel(0);
            resultText.text = tangiaSpawner.IsLoggedIn ? "you are already logged in" : "you're not logged in";
        }
    }
}