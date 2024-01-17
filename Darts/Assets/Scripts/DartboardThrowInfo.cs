public class DartboardThrowInfo {
    public DartSectionTypes Type;
    private int Number;
    public int Score;

    public DartboardThrowInfo(DartSectionTypes hitType, int hitNumber) {
        Type = hitType;
        Number = hitNumber;
        SetScore();
    }

    private void SetScore() {
        if (Type == DartSectionTypes.Single) Score = Number;
        if (Type == DartSectionTypes.Double) Score = Number * 2;
        if (Type == DartSectionTypes.Triple) Score = Number * 3;
    }
}