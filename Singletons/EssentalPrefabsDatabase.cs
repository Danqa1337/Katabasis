using UnityEngine;
[CreateAssetMenu(fileName = "essential", menuName = "Generation/essential")]
public class EssentalPrefabsDatabase : ScriptableObject
{
    public GameObject blood;
    public GameObject OnHitAnimation;

    // public Dictionary<prefabCode, List<GameObject> prefab> getPrefab = new Dictionary<Material, PrefabItem>();
    public static EssentalPrefabsDatabase _instance;
    public static EssentalPrefabsDatabase Instance
    {
        get
        {
            _instance = Resources.Load<EssentalPrefabsDatabase>("essential");
            return _instance;
        }
    }


    //public void OnBeforeSerialize()
    //{
    //    Debug.Log("Seserialize");
    //    getPrefab = new Dictionary<prefabCode, PrefabItem>();

    //    for (int i = 0; i < prefabItems.Length; i++)
    //    {

    //        GetItem.Add(prefabItems[i].prefabCode, );
    //    }
    //    //Debug.Log("Serialize");
    //    getPrefab = new Dictionary<int, ItemObject>();
    //}
}






