using System.Collections.Generic;
using UnityEngine;

public class FaceExpressionController : MonoBehaviour
{
    [Header("BlendShape を持っている SkinnedMeshRenderer")]
    [SerializeField] private SkinnedMeshRenderer faceRenderer;

    [Header("通常顔にしたい場合に使う。不要なら空でOK")]
    [SerializeField] private string defaultFaceName = "なごみ";

    public enum FaceExpression
    {
        None,
        Default,
        Wink,
        Smile,
        Hau,
        Surprise,
        Angry,
        SparklyEyes,
        WhiteEyes,
        BadFace,
        Tears
    }

    private Mesh faceMesh;

    private int defaultFaceIndex = -1;
    private int winkIndex = -1;
    private int smileIndex = -1;
    private int hauIndex = -1;
    private int surpriseIndex = -1;
    private int angryIndex = -1;
    private int sparklyEyesIndex = -1;
    private int whiteEyesIndex = -1;
    private int badFaceIndex = -1;
    private int tearsIndex = -1;

    // 表情用だけをここで管理する
    private readonly List<int> expressionIndices = new();

    private void Awake()
    {
        if (faceRenderer == null)
        {
            Debug.LogError("FaceExpressionController: faceRenderer が未設定です。");
            enabled = false;
            return;
        }

        faceMesh = faceRenderer.sharedMesh;
        if (faceMesh == null)
        {
            Debug.LogError("FaceExpressionController: sharedMesh が取得できません。");
            enabled = false;
            return;
        }

        defaultFaceIndex = GetBlendShapeIndexSafe(defaultFaceName);

        winkIndex = FindFirstValidIndex("ウィンク1", "ウィンク2", "ウィンク右", "ウィンク2右");
        smileIndex = FindFirstValidIndex("笑い");
        hauIndex = FindFirstValidIndex("はぅ");
        surpriseIndex = FindFirstValidIndex("びっくり");
        angryIndex = FindFirstValidIndex("おこ");
        sparklyEyesIndex = FindFirstValidIndex("キラキラ目", "キラ目");
        whiteEyesIndex = FindFirstValidIndex("白目", "目白");
        badFaceIndex = FindFirstValidIndex("わるいがお", "わるいかお");
        tearsIndex = FindFirstValidIndex("泣", "涙", "泣だけ1");

        RegisterExpressionIndex(defaultFaceIndex);
        RegisterExpressionIndex(winkIndex);
        RegisterExpressionIndex(smileIndex);
        RegisterExpressionIndex(hauIndex);
        RegisterExpressionIndex(surpriseIndex);
        RegisterExpressionIndex(angryIndex);
        RegisterExpressionIndex(sparklyEyesIndex);
        RegisterExpressionIndex(whiteEyesIndex);
        RegisterExpressionIndex(badFaceIndex);
        RegisterExpressionIndex(tearsIndex);
    }

    private void RegisterExpressionIndex(int index)
    {
        if (index >= 0 && !expressionIndices.Contains(index))
        {
            expressionIndices.Add(index);
        }
    }

    private int GetBlendShapeIndexSafe(string shapeName)
    {
        if (string.IsNullOrWhiteSpace(shapeName))
        {
            return -1;
        }

        int index = faceMesh.GetBlendShapeIndex(shapeName);
        if (index < 0)
        {
            Debug.LogWarning($"BlendShape '{shapeName}' が見つかりません。");
        }
        return index;
    }

    private int FindFirstValidIndex(params string[] names)
    {
        foreach (string name in names)
        {
            int index = faceMesh.GetBlendShapeIndex(name);
            if (index >= 0)
            {
                return index;
            }
        }

        Debug.LogWarning($"BlendShape が見つかりません: {string.Join(", ", names)}");
        return -1;
    }

    /// <summary>
    /// 表情用BlendShapeだけをリセットする
    /// 口パク・瞬きは触らない
    /// </summary>
    public void ResetExpressionOnly()
    {
        foreach (int index in expressionIndices)
        {
            faceRenderer.SetBlendShapeWeight(index, 0f);
        }
    }

    public void SetExpression(FaceExpression expression, float weight = 100f)
    {
        ResetExpressionOnly();

        int targetIndex = expression switch
        {
            FaceExpression.None => -1,
            FaceExpression.Default => defaultFaceIndex,
            FaceExpression.Wink => winkIndex,
            FaceExpression.Smile => smileIndex,
            FaceExpression.Hau => hauIndex,
            FaceExpression.Surprise => surpriseIndex,
            FaceExpression.Angry => angryIndex,
            FaceExpression.SparklyEyes => sparklyEyesIndex,
            FaceExpression.WhiteEyes => whiteEyesIndex,
            FaceExpression.BadFace => badFaceIndex,
            FaceExpression.Tears => tearsIndex,
            _ => -1
        };

        if (targetIndex >= 0)
        {
            faceRenderer.SetBlendShapeWeight(targetIndex, weight);
        }
    }

    public void ClearExpression()
    {
        ResetExpressionOnly();
    }

    public void SetDefault() => SetExpression(FaceExpression.Default);
    public void SetWink() => SetExpression(FaceExpression.Wink);
    public void SetSmile() => SetExpression(FaceExpression.Smile);
    public void SetHau() => SetExpression(FaceExpression.Hau);
    public void SetSurprise() => SetExpression(FaceExpression.Surprise);
    public void SetAngry() => SetExpression(FaceExpression.Angry);
    public void SetSparklyEyes() => SetExpression(FaceExpression.SparklyEyes);
    public void SetWhiteEyes() => SetExpression(FaceExpression.WhiteEyes);
    public void SetBadFace() => SetExpression(FaceExpression.BadFace);
    public void SetTears() => SetExpression(FaceExpression.Tears);

#if UNITY_EDITOR
    [ContextMenu("Test/Clear Expression")]
    private void TestClearExpression() => ClearExpression();

    [ContextMenu("Test/Default")]
    private void TestDefault() => SetDefault();

    [ContextMenu("Test/Wink")]
    private void TestWink() => SetWink();

    [ContextMenu("Test/Smile")]
    private void TestSmile() => SetSmile();

    [ContextMenu("Test/Hau")]
    private void TestHau() => SetHau();

    [ContextMenu("Test/Surprise")]
    private void TestSurprise() => SetSurprise();

    [ContextMenu("Test/Angry")]
    private void TestAngry() => SetAngry();

    [ContextMenu("Test/SparklyEyes")]
    private void TestSparklyEyes() => SetSparklyEyes();

    [ContextMenu("Test/WhiteEyes")]
    private void TestWhiteEyes() => SetWhiteEyes();

    [ContextMenu("Test/BadFace")]
    private void TestBadFace() => SetBadFace();

    [ContextMenu("Test/Tears")]
    private void TestTears() => SetTears();
#endif
}