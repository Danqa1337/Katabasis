using UnityEngine;
using Debug = UnityEngine.Debug;

public class TimeTester : Singleton<TimeTester>
{
    private float testStartTime;

    public static void StartTest(string str = "")
    {
        if (str != "") Debug.Log(str);
        instance.testStartTime = Time.realtimeSinceStartup;
    }

    public static void EndTest(string str)
    {
        var testTime = Time.realtimeSinceStartup - instance.testStartTime;
        Debug.Log(str + testTime * 1000 + " ms");
    }
}
