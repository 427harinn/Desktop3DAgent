using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    [Serializable]
    private class GenerateRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [Serializable]
    private class GenerateResponse
    {
        public string response;
    }

    private void Start()
    {
        sendButton.onClick.AddListener(OnClickSend);
    }

    public void OnClickSend()
    {
        string text = promptInput.text;

        if (string.IsNullOrEmpty(text)) return;

        StartCoroutine(Send(text));
    }

    private IEnumerator Send(string userText)
    {
        statusText.text = "送信中...";

        var req = new GenerateRequest
        {
            model = modelName,
            prompt = "(ろーるぷれい)私とあなたは友達です。現在私たちはLINEで会話をしています。: " + userText,
            stream = false
        };

        string json = JsonUtility.ToJson(req);
        byte[] data = Encoding.UTF8.GetBytes(json);

        Debug.Log("送信JSON: " + json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            statusText.text = "エラー";

            string errorMessage =
                "通信エラー\n" +
                "Code: " + request.responseCode + "\n" +
                "Error: " + request.error + "\n" +
                "Body: " + request.downloadHandler.text;

            responseText.text = errorMessage;

            Debug.LogError(errorMessage);
        }
        else
        {
            Debug.Log("送信JSON: " + json);
            Debug.Log("受信JSON: " + request.downloadHandler.text);

            var res = JsonUtility.FromJson<GenerateResponse>(request.downloadHandler.text);
            responseText.text = res.response;
            statusText.text = "完了";
        }
    }
}