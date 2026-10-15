using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instancia;

    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        Instancia = this;
    }

    public void AgitarCamara(float fuerza, float duracion)
{
    var definition = impulseSource.ImpulseDefinition;


    definition.TimeEnvelope.SustainTime = duracion;

    impulseSource.GenerateImpulseWithForce(fuerza);
}
}