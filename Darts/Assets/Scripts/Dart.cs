using UnityEngine;

public class Dart : MonoBehaviour {
    public DartboardManager DartboardManager;
    public AudioSource ThrowAudio;
    public AudioSource ImpactAudio;

    private Rigidbody RigidBody;
    private float Speed = 100f;
    private bool isThrown = false;
    private bool isHit = false;
    private bool hitDivider = false;
    private int hitDividerCount = 0;
    RaycastHit RayHit;

    public void Throw(float speed) {
        Speed = speed;
        RigidBody.useGravity = true;
        RigidBody.isKinematic = false;
        RigidBody.velocity = Speed * transform.forward;
        isThrown = true;
        isHit = false;
        PlayThrowAudio();
    }
    
    void Update() {
        if (isThrown && !isHit) {
            hitDivider = false;
            hitDividerCount = 0;
            do {
                if (Physics.Linecast(transform.position, transform.position + transform.forward * (Speed * Time.deltaTime * 2), out RayHit)) {
                    if (!RayHit.transform.GetComponent<Collider>().isTrigger) {
                        DartboardDivider divider = RayHit.transform.GetComponent<DartboardDivider>();
                        if (divider != null) {
                            // if we hit a divider, we want to move the dart slightly to the left or right
                            var rando = Random.insideUnitCircle;
                            var adjustd = new Vector3(rando.x, rando.y, 0) * 0.2f;
                            transform.position += adjustd;
                            hitDivider = true;
                            hitDividerCount++;
                        } else {
                            hitDivider = false;
                            RigidBody.useGravity = false;
                            RigidBody.isKinematic = true;
                            RigidBody.velocity = Vector3.zero;
                            transform.position = RayHit.point;
                            PlayImpactAudio();
                            isHit = true;

                            DartboardSection section = RayHit.transform.GetComponent<DartboardSection>();
                            if (section != null) {
                                section.Hit();
                            } else {
                                DartboardManager.RegisterMiss();
                            }
                        }
                    }
                }
            } while (hitDivider && hitDividerCount < 5);
        }
    }

    private void PlayThrowAudio() {
        if (ThrowAudio) ThrowAudio.Play();
    }

    private void PlayImpactAudio() {
        if (ImpactAudio) ImpactAudio.Play();
    }

    private void Awake() {
        RigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable() {
        isThrown = false;
        isHit = false;
    }
}