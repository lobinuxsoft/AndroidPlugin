using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PluginTest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button clearButton;
    [SerializeField] Button saveButton;
    [SerializeField] Scrollbar scrollbar;

    LoggerBase logger;

    private void Awake() 
    {
        logger = LoggerBase.CreateLogger();

        inputField.onSubmit.AddListener(SendLog);
        clearButton.onClick.AddListener(ClearLog);
        saveButton.onClick.AddListener(SaveLog);

        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void Start()
    {
        StartCoroutine(ShowLogs());
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
    private void SaveLog()
    {
        logger.SaveLogs();
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

    private void OnDestroy()
    {
        if (logger != null)
            logger = null;

        Application.logMessageReceived -= OnLogMessageReceived;
    }

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

    public abstract void SaveLogs();

    public abstract void ClearLogs();

    public static LoggerBase CreateLogger()
    {
#if UNITY_ANDROID
        return new AndroidLogger($"{Application.persistentDataPath}/Log.txt");
#else
        return new DefaultLogger($"{Application.persistentDataPath}/Log.txt");
#endif
    }
}

public class AndroidLogger : LoggerBase
{
    const string pluginName = "com.cryingonion.logger.Logger";

    AndroidJavaClass loggerClass;
    AndroidJavaObject loggerObject;

    public AndroidLogger(string filepath)
    {
        loggerClass = new AndroidJavaClass(pluginName);
        loggerObject = loggerClass.CallStatic<AndroidJavaObject>("getInstance", filepath);
    }

    ~AndroidLogger()
    {
        loggerObject.Call("saveLog");
    }

    public override void ClearLogs() => loggerObject.Call("clearLogs");

    public override string GetLogs() => loggerObject.Call<string>("getLogs");

    public override void Log(string message) => loggerObject.Call("sendLog", $"{message}\n");

    public override void SaveLogs() => loggerObject.Call("saveLog");
}

public class DefaultLogger : LoggerBase
{
    private string filepath;

    private string logs;

    public DefaultLogger(string filepath)
    {
        this.filepath = filepath;

        if(File.Exists(filepath))
            logs = File.ReadAllText(filepath);
    }

    ~DefaultLogger() => File.WriteAllText(filepath, logs);

    public override void ClearLogs() => logs = "";

    public override string GetLogs() => logs;

    public override void Log(string message) => logs += $"{message}\n";

    public override void SaveLogs()
    {
        throw new System.NotImplementedException();
    }
}