using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class OllamaClient : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InputField promptInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Text statusText;
    [SerializeField] private Text responseText;

    [Header("Ollama")]
    [SerializeField] private string apiUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "phi3:mini";

    [Header("Avatar")]
    [SerializeField] private TextLipSyncController lipSyncController;
    [SerializeField] private FaceExpressionController faceExpressionController;

    [Header("Expression Reset")]
    [SerializeField] private float expressionResetDelay = 1.5f;

    private Coroutine resetExpressionCoroutine;

    [Serializable]
    private class GenerateRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [Serializable]
    private class StreamResponse
    {
        public string response;
        public bool done;
    }

    private bool isSending = false;

    // ストリーム中の表情タグ読み取り用
    private bool isReadingTag = false;
    private readonly StringBuilder tagBuffer = new StringBuilder();

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnClickSend);
        }
    }

    public void OnClickSend()
    {
        if (isSending) return;

        string text = promptInput.text;
        if (string.IsNullOrEmpty(text)) return;

        StartCoroutine(SendStream(text));
    }

    private IEnumerator SendStream(string userText)
    {
        isSending = true;

        if (sendButton != null)
        {
            sendButton.interactable = false;
        }

        if (statusText != null)
        {
            statusText.text = "送信中...";
        }

        if (responseText != null)
        {
            responseText.text = "";
        }

        if (lipSyncController != null)
        {
            lipSyncController.StopLipSync();
        }

        // タグ読み取り状態をリセット
        isReadingTag = false;
        tagBuffer.Clear();

        var req = new GenerateRequest
        {
            model = modelName,
            prompt =
                "あなたは友達としてLINEのように自然に会話してください。" +
                "返答には表情タグを入れてください。" +
                "使ってよい表情は [なごみ] [笑い] [はぅ] [びっくり] [おこ] [キラキラ目] [白目] [わるいかお] [涙] です。" +
                "形式は「[表情]本文」です。" +
                "本文の途中で表情を変えたい場合も、同じように [表情] を挿入してください。" +
                "表情タグ自体は会話文として不要なので、表示側ではタグを除去して使います。" +
                "ユーザー: " + userText,
            stream = true
        };

        string json = JsonUtility.ToJson(req);
        byte[] data = Encoding.UTF8.GetBytes(json);

        UnityEngine.Debug.Log("送信JSON: " + json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            int lastLength = 0;
            StringBuilder lineBuffer = new StringBuilder();

            while (!operation.isDone)
            {
                if (request.downloadHandler != null)
                {
                    string currentText = request.downloadHandler.text;

                    if (!string.IsNullOrEmpty(currentText) && currentText.Length > lastLength)
                    {
                        string newChunk = currentText.Substring(lastLength);
                        lastLength = currentText.Length;
                        ProcessChunk(newChunk, lineBuffer);
                    }
                }

                yield return null;
            }

            // 最後の取りこぼし処理
            if (request.downloadHandler != null)
            {
                string finalText = request.downloadHandler.text;

                if (!string.IsNullOrEmpty(finalText) && finalText.Length > lastLength)
                {
                    string newChunk = finalText.Substring(lastLength);
                    ProcessChunk(newChunk, lineBuffer);
                }
            }

            // 行末に改行がなくて、lineBufferに残っている場合の処理
            if (lineBuffer.Length > 0)
            {
                string rest = lineBuffer.ToString().Trim();
                lineBuffer.Clear();

                if (!string.IsNullOrEmpty(rest))
                {
                    TryProcessJsonLine(rest);
                }
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (statusText != null)
                {
                    statusText.text = "エラー";
                }

                string errorMessage =
                    "通信エラー\n" +
                    "Code: " + request.responseCode + "\n" +
                    "Error: " + request.error + "\n" +
                    "Body: " + request.downloadHandler.text;

                if (responseText != null)
                {
                    responseText.text += "\n" + errorMessage;
                }

                UnityEngine.Debug.LogError(errorMessage);
            }
            else
            {
                if (statusText != null)
                {
                    statusText.text = "完了";
                }
            }
        }

        if (sendButton != null)
        {
            sendButton.interactable = true;
        }

        isSending = false;
    }

    private void ProcessChunk(string newChunk, StringBuilder lineBuffer)
    {
        for (int i = 0; i < newChunk.Length; i++)
        {
            char c = newChunk[i];

            if (c == '\n')
            {
                string line = lineBuffer.ToString().Trim();
                lineBuffer.Clear();

                if (!string.IsNullOrEmpty(line))
                {
                    TryProcessJsonLine(line);
                }
            }
            else
            {
                lineBuffer.Append(c);
            }
        }
    }

    private void TryProcessJsonLine(string line)
    {
        try
        {
            StreamResponse res = JsonUtility.FromJson<StreamResponse>(line);

            if (res != null && !string.IsNullOrEmpty(res.response))
            {
                ProcessStreamText(res.response);
            }

            if (res != null && res.done)
            {
                if (statusText != null)
                {
                    statusText.text = "完了";
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning("JSON解析失敗: " + ex.Message + "\nline: " + line);
        }
    }

    private void ProcessStreamText(string chunk)
    {
        for (int i = 0; i < chunk.Length; i++)
        {
            char c = chunk[i];

            // タグ開始
            if (c == '[')
            {
                isReadingTag = true;
                tagBuffer.Clear();
                continue;
            }

            // タグ読み取り中
            if (isReadingTag)
            {
                if (c == ']')
                {
                    isReadingTag = false;

                    string tag = tagBuffer.ToString().Trim();
                    tagBuffer.Clear();

                    ApplyExpression(tag);
                    continue;
                }
                else
                {
                    tagBuffer.Append(c);
                    continue;
                }
            }

            // 通常文字は表示する
            if (responseText != null)
            {
                responseText.text += c;
            }

            if (lipSyncController != null)
            {
                lipSyncController.EnqueueText(c.ToString());
            }
            RestartExpressionResetTimer();
        }
    }

    private void RestartExpressionResetTimer()
    {
        if (resetExpressionCoroutine != null)
        {
            StopCoroutine(resetExpressionCoroutine);
        }

        resetExpressionCoroutine = StartCoroutine(ResetExpressionAfterDelay());
    }

    private IEnumerator ResetExpressionAfterDelay()
    {
        yield return new WaitForSeconds(expressionResetDelay);

        if (faceExpressionController != null)
        {
            faceExpressionController.SetDefault();
            UnityEngine.Debug.Log("表情をデフォルトに戻しました");
        }

        resetExpressionCoroutine = null;
    }

    private void ApplyExpression(string tag)
    {
        if (faceExpressionController == null || string.IsNullOrEmpty(tag))
        {
            return;
        }

        switch (tag)
        {
            case "なごみ":
                faceExpressionController.SetDefault();
                break;

            case "笑い":
                faceExpressionController.SetSmile();
                break;

            case "はぅ":
                faceExpressionController.SetHau();
                break;

            case "びっくり":
                faceExpressionController.SetSurprise();
                break;

            case "おこ":
                faceExpressionController.SetAngry();
                break;

            case "キラキラ目":
                faceExpressionController.SetSparklyEyes();
                break;

            case "白目":
                faceExpressionController.SetWhiteEyes();
                break;

            case "わるいかお":
            case "わるいがお":
                faceExpressionController.SetBadFace();
                break;

            case "涙":
            case "泣":
                faceExpressionController.SetTears();
                break;

            default:
                UnityEngine.Debug.Log("未知の表情タグ: " + tag);
                break;
        }
    }
}