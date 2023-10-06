public enum LaunchMode
{
    Arena,
    Dungeon,
}
public class UndestructableData : Singleton<UndestructableData>
{
    public LaunchMode launchMode;
    public ComplexObjectName playerRace = ComplexObjectName.Ciclops;
    private void Start()
    {
        if (playerRace == ComplexObjectName.Null) playerRace = ComplexObjectName.Ciclops;
        DontDestroyOnLoad(this.gameObject);
        if (FindObjectsOfType(GetType()).Length > 1) Destroy(gameObject);

    }

}
