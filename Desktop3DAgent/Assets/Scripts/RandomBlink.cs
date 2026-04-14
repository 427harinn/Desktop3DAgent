using System.Collections;
using UnityEngine;

public class RandomBlinkSingle : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private string blinkShapeName = "Ç‹ÇŒÇΩÇ´";

    [SerializeField] private float minInterval = 2.0f;
    [SerializeField] private float maxInterval = 5.0f;

    [SerializeField] private float closeDuration = 0.06f;
    [SerializeField] private float holdDuration = 0.03f;
    [SerializeField] private float openDuration = 0.08f;

    [SerializeField] private float blinkWeight = 100f;

    private int blinkIndex = -1;

    private void Start()
    {
        if (faceRenderer == null || faceRenderer.sharedMesh == null)
        {
            Debug.LogError("faceRenderer Ç‹ÇΩÇÕ sharedMesh Ç™ñ¢ê›íËÇ≈Ç∑ÅB");
            enabled = false;
            return;
        }

        blinkIndex = faceRenderer.sharedMesh.GetBlendShapeIndex(blinkShapeName);

        if (blinkIndex < 0)
        {
            Debug.LogError($"BlendShape '{blinkShapeName}' Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            enabled = false;
            return;
        }

        StartCoroutine(BlinkLoop());
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            yield return StartCoroutine(BlinkOnce());
        }
    }

    private IEnumerator BlinkOnce()
    {
        yield return StartCoroutine(SetWeight(0f, blinkWeight, closeDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(SetWeight(blinkWeight, 0f, openDuration));
    }

    private IEnumerator SetWeight(float from, float to, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float value = Mathf.Lerp(from, to, t);
            faceRenderer.SetBlendShapeWeight(blinkIndex, value);
            yield return null;
        }

        faceRenderer.SetBlendShapeWeight(blinkIndex, to);
    }
}