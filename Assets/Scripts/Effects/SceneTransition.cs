using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PrimeTween;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        // Começa invisível
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void TransitionToScene(string sceneName)
    {
        // Faz o fade
        Tween.Color(fadeImage, new Color(0, 0, 0, 1), fadeDuration)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
    }
}
