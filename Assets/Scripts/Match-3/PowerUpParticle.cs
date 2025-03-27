using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Este script � respons�vel por ativar as part�culas de efeito visual quando um power-up � ativado. 
/// Ele utiliza um sistema de part�culas para criar uma anima��o visual durante a destrui��o do power-up.
/// </summary>

public class PowerUpParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem powerupDestroyParticle;

    // Ativa o sistema de part�culas para o efeito de destrui��o
    public void ActivateParticle()
    {
        if (powerupDestroyParticle != null)
        {
            powerupDestroyParticle.gameObject.SetActive(true);
            powerupDestroyParticle.Play();
        }
    }
}
