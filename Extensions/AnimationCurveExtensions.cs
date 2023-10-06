using UnityEngine;

static public class AnimationCurveExtensions
{
    public static void AddKey(this AnimationCurve curve, Vector2 key)
    {
        Keyframe keyframe = new Keyframe();
        keyframe.time = key.x;
        keyframe.value = key.y;
        curve.AddKey(keyframe);
    }
    public static void MoveKey(this AnimationCurve curve, int index, Vector2 key)
    {
        Keyframe keyframe = new Keyframe();
        keyframe.time = key.x;
        keyframe.value = key.y;
        curve.MoveKey(index, keyframe);
    }
    public static void Clear(this AnimationCurve curve)
    {

        if (curve.keys.Length > 0)
        {


            for (int i = 0; i < curve.keys.Length; i++)
            {
                curve.RemoveKey(i);
            }
        }
    }
}