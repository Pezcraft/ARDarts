using UnityEngine;

public class DartboardSection : MonoBehaviour {
    private DartboardManager DartboardManager;
    private DartboardThrowInfo SectionInfo;
    public DartSectionTypes SectionType;
    public int Score;
    
    public void Hit() {
        if (DartboardManager) {
            if (SectionType != DartSectionTypes.Miss) {
                DartboardManager.RegisterHit(SectionInfo);
            } else {
                DartboardManager.RegisterMiss();
            }
        }
    }

    private void Start() {
        DartboardManager = transform.GetComponentInParent<DartboardManager>();
        if (DartboardManager == null) DartboardManager = DartboardManager.GetDartboardManager();
        SectionInfo = new DartboardThrowInfo(SectionType, Score);
        if (transform.GetComponent<MeshCollider>() == null) {
            gameObject.AddComponent<MeshCollider>();
        }
    }
}