using UnityEngine;

public class parallax : MonoBehaviour
{
    public Transform target; // Objeto que será seguido para determinar o movimento do parallax
    public float parallaxEffect = 0.5f; // Fator de efeito de parallax, quanto maior, mais rápido será o movimento do plano de fundo

    private Vector3 lastTargetPosition; // Posição anterior do objeto de destino

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        lastTargetPosition = target.position;
    }

    void Update()
    {
        // Calcula o movimento do parallax baseado na diferença entre a posição atual e a posição anterior do objeto de destino
        float deltaX = (target.position.x - lastTargetPosition.x) * parallaxEffect;
        float deltaY = (target.position.y - lastTargetPosition.y) * parallaxEffect;

        // Move o plano de fundo com base no movimento calculado
        transform.position -= new Vector3(deltaX, deltaY, 0);

        // Atualiza a posição anterior do objeto de destino
        lastTargetPosition = target.position;
    }
}
