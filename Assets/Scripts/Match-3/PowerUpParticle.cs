using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Este script é responsável por ativar as partículas de efeito visual quando um power-up é ativado. 
/// Ele utiliza um sistema de partículas para criar uma animação visual durante a destruição do power-up.
/// </summary>

public class PowerUpParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem powerupDestroyParticle;

    // Ativa o sistema de partículas para o efeito de destruição
    public void ActivateParticle()
    {
        if (powerupDestroyParticle != null)
        {
            powerupDestroyParticle.gameObject.SetActive(true);
            powerupDestroyParticle.Play();
        }
    }
}
