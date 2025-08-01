using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Ursaanimation.CubicFarmAnimals.AnimationController))]
public class AnimalAI : MonoBehaviour
{
    [Header("Wandering")]
    public float speed = 3f;
    public float changeDirectionInterval = 4f;

    private CharacterController controller;
    private Ursaanimation.CubicFarmAnimals.AnimationController animCtrl;
    private Vector3 wanderDirection;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animCtrl   = GetComponent<Ursaanimation.CubicFarmAnimals.AnimationController>();
    }

    void Start()
    {
        // choose initial direction and repeat
        PickNewDirection();
        InvokeRepeating(nameof(PickNewDirection), changeDirectionInterval, changeDirectionInterval);
    }

    void PickNewDirection()
    {
        // pick a random horizontal direction
        wanderDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // face that way
        if (wanderDirection.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(wanderDirection);
    }

    void Update()
    {
        // move character
        controller.Move(wanderDirection * speed * Time.deltaTime);

        // play walk animation if moving
        if (wanderDirection.sqrMagnitude > 0.01f)
            animCtrl.animator.Play(animCtrl.walkForwardAnimation);
;
    }
}
