using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PluginTest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button loadButton;
    [SerializeField] Button saveButton;
    [SerializeField] Button clearButton;
    [SerializeField] Scrollbar scrollbar;

    LoggerBase logger;

    private void Awake() 
    {
        logger = LoggerBase.CreateLogger();

        inputField.onSubmit.AddListener(SendLog);
        clearButton.onClick.AddListener(ClearLog);

        loadButton.onClick.AddListener(LoadLogs);
        saveButton.onClick.AddListener(SaveLogs);

        Application.logMessageReceived += OnLogMessageReceived;
    }


    private void SendLog(string message)
    {
        Debug.Log(message);
        inputField.text = "";

        StartCoroutine(ShowLogs());
    }

    private void ClearLog()
    {
        logger.ClearLogs();

        StartCoroutine(ShowLogs());
    }

    private void LoadLogs() 
    { 
        logger.LoadLogs();
        StartCoroutine(ShowLogs());
    }

    private void SaveLogs() 
    {
        logger.SaveLogs();
        StartCoroutine(ShowLogs());
    }

    IEnumerator ShowLogs()
    {
        string[] logs = logger.GetLogs().Split('\n');

        textMesh.text = "";

        for (int i = 0; i < logs.Length; i++)
        {
            textMesh.text += $"{logs[i]}\n";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textMesh.transform.parent);

        yield return new WaitForEndOfFrame();
        scrollbar.value = 0;
    }

    private void OnDestroy() => Application.logMessageReceived -= OnLogMessageReceived;

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        Color color = Color.white;
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                color = Color.red;
                break;
            
            case LogType.Warning:
                color = Color.yellow;
                break;
            case LogType.Log:
                color = Color.green;
                break;
        }

        logger.Log(
            $"<b><color=#{ColorUtility.ToHtmlStringRGB(color)}>[{type.ToString()}]</color></b>: {condition}."
        );
    }
}

public abstract class LoggerBase
{
    public abstract void Log(string message);

    public abstract string GetLogs();

    public abstract void ClearLogs();

    public abstract void SaveLogs();

    public abstract void LoadLogs();

    public static LoggerBase CreateLogger()
    {
#if UNITY_ANDROID
        return new AndroidLogger();
#else
        return new DefaultLogger();
#endif
    }
}

public class AndroidLogger : LoggerBase
{
    const string pluginName = "com.cryingonion.logger.Logger";

    AndroidJavaClass loggerClass;
    AndroidJavaObject loggerObject;

    public AndroidLogger()
    {
        loggerClass = new AndroidJavaClass(pluginName);
        loggerObject = loggerClass.CallStatic<AndroidJavaObject>("getInstance");
    }

    public override void ClearLogs() => loggerObject.Call("clearLogs");

    public override string GetLogs() => loggerObject.Call<string>("getLogs");

    public override void LoadLogs()
    {
        Debug.LogException(new System.NotImplementedException());
    }

    public override void Log(string message) => loggerObject.Call("sendLog", $"{message}\n");

    public override void SaveLogs()
    {
        Debug.LogException(new System.NotImplementedException());
    }
}

public class DefaultLogger : LoggerBase
{
    private string logs = "";

    public override void ClearLogs() => logs = "";

    public override string GetLogs() => logs;

    public override void LoadLogs()
    {
        Debug.LogException(new System.NotImplementedException());
    }

    public override void Log(string message) => logs += $"{message}\n";

    public override void SaveLogs()
    {
        Debug.LogException(new System.NotImplementedException());
    }
}