using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextLipSyncController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer faceRenderer;

    [Header("BlendShape Names")]
    [SerializeField] private string aName = "‚ ";
    [SerializeField] private string iName = "‚¢";
    [SerializeField] private string uName = "‚¤";
    [SerializeField] private string eName = "‚¦";
    [SerializeField] private string oName = "‚¨";

    [Header("Lip Sync")]
    [SerializeField] private float phonemeDuration = 0.06f;
    [SerializeField] private float closeDuration = 0.03f;
    [SerializeField] private float mouthWeight = 70f;

    private int aIndex = -1;
    private int iIndex = -1;
    private int uIndex = -1;
    private int eIndex = -1;
    private int oIndex = -1;

    private readonly Queue<char> charQueue = new Queue<char>();
    private Coroutine lipSyncCoroutine;
    private bool isPlaying;

    private void Awake()
    {
        if (faceRenderer == null || faceRenderer.sharedMesh == null)
        {
            Debug.LogError("TextLipSyncController: faceRenderer ‚Ü‚½‚Í sharedMesh ‚ª–¢Ý’è‚Å‚·B");
            enabled = false;
            return;
        }

        Mesh mesh = faceRenderer.sharedMesh;
        aIndex = mesh.GetBlendShapeIndex(aName);
        iIndex = mesh.GetBlendShapeIndex(iName);
        uIndex = mesh.GetBlendShapeIndex(uName);
        eIndex = mesh.GetBlendShapeIndex(eName);
        oIndex = mesh.GetBlendShapeIndex(oName);

        Debug.Log($"LipSync Index: ‚ ={aIndex}, ‚¢={iIndex}, ‚¤={uIndex}, ‚¦={eIndex}, ‚¨={oIndex}");
    }

    public void EnqueueText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        foreach (char c in text)
        {
            charQueue.Enqueue(c);
        }

        if (!isPlaying)
        {
            lipSyncCoroutine = StartCoroutine(ProcessQueue());
        }
    }

    public void StopLipSync()
    {
        if (lipSyncCoroutine != null)
        {
            StopCoroutine(lipSyncCoroutine);
            lipSyncCoroutine = null;
        }

        charQueue.Clear();
        isPlaying = false;
        ResetMouth();
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (charQueue.Count > 0)
        {
            char c = charQueue.Dequeue();
            int targetIndex = GetBlendShapeIndexFromChar(c);

            if (targetIndex >= 0)
            {
                SetOnlyOneMouth(targetIndex, mouthWeight);
                yield return new WaitForSeconds(phonemeDuration);
                ResetMouth();
                yield return new WaitForSeconds(closeDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.02f);
            }
        }

        ResetMouth();
        isPlaying = false;
        lipSyncCoroutine = null;
    }

    private int GetBlendShapeIndexFromChar(char c)
    {
        switch (c)
        {
            case '‚ ':
            case '‚©':
            case '‚³':
            case '‚½':
            case '‚È':
            case '‚Í':
            case '‚Ü':
            case '‚â':
            case '‚ç':
            case '‚í':
            case '‚ª':
            case '‚´':
            case '‚¾':
            case '‚Î':
            case '‚Ï':
            case '‚Ÿ':
            case '‚á':
                return aIndex;

            case '‚¢':
            case '‚«':
            case '‚µ':
            case '‚¿':
            case '‚É':
            case '‚Ð':
            case '‚Ý':
            case '‚è':
            case '‚¬':
            case '‚¶':
            case '‚À':
            case '‚Ñ':
            case '‚Ò':
            case '‚¡':
                return iIndex;

            case '‚¤':
            case '‚­':
            case '‚·':
            case '‚Â':
            case '‚Ê':
            case '‚Ó':
            case '‚Þ':
            case '‚ä':
            case '‚é':
            case '‚®':
            case '‚¸':
            case '‚Ã':
            case '‚Ô':
            case '‚Õ':
            case '‚£':
            case '‚ã':
                return uIndex;

            case '‚¦':
            case '‚¯':
            case '‚¹':
            case '‚Ä':
            case '‚Ë':
            case '‚Ö':
            case '‚ß':
            case '‚ê':
            case '‚°':
            case '‚º':
            case '‚Å':
            case '‚×':
            case '‚Ø':
            case '‚¥':
                return eIndex;

            case '‚¨':
            case '‚±':
            case '‚»':
            case '‚Æ':
            case '‚Ì':
            case '‚Ù':
            case '‚à':
            case '‚æ':
            case '‚ë':
            case '‚ð':
            case '‚²':
            case '‚¼':
            case '‚Ç':
            case '‚Ú':
            case '‚Û':
            case '‚§':
            case '‚å':
                return oIndex;

            default:
                return -1;
        }
    }

    private void SetOnlyOneMouth(int index, float weight)
    {
        ResetMouth();

        if (index >= 0)
        {
            faceRenderer.SetBlendShapeWeight(index, weight);
        }
    }

    private void ResetMouth()
    {
        if (aIndex >= 0) faceRenderer.SetBlendShapeWeight(aIndex, 0f);
        if (iIndex >= 0) faceRenderer.SetBlendShapeWeight(iIndex, 0f);
        if (uIndex >= 0) faceRenderer.SetBlendShapeWeight(uIndex, 0f);
        if (eIndex >= 0) faceRenderer.SetBlendShapeWeight(eIndex, 0f);
        if (oIndex >= 0) faceRenderer.SetBlendShapeWeight(oIndex, 0f);
    }
}