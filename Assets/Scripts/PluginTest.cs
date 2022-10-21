using System;
using System.Collections;
using System.IO;
using TMPro;
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

        logger.textMesh = textMesh;

        inputField.onSubmit.AddListener(SendLog);
        clearButton.onClick.AddListener(ClearLog);
        saveButton.onClick.AddListener(SaveLog);

        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void Start() => StartCoroutine(ShowLogs());

    private void SendLog(string message)
    {
        Debug.Log(message);
        inputField.text = "";
    }

    private void ClearLog()
    {
        logger.ClearLogs();
    }

    private void SaveLog() => logger.SaveLogs();

    IEnumerator ShowLogs()
    {
        logger.ShowLogs();

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

        StartCoroutine(ShowLogs());
    }
}

public abstract class LoggerBase
{
    public TextMeshProUGUI textMesh;

    public abstract void Log(string message);

    public abstract void ShowLogs();

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
    const string interfaceName = "com.cryingonion.logger.AlertCallback";

    public class AlertCallback : AndroidJavaProxy
    {
        public Action positiveAction;
        public Action negativeAction;

        public AlertCallback() : base(interfaceName) { }

        public void onPositive() => positiveAction?.Invoke();

        public void onNegative() => negativeAction?.Invoke();
    }

    AndroidJavaClass loggerClass;
    AndroidJavaObject loggerObject;

    string filepath;

    public AndroidLogger(string filepath)
    {
        this.filepath = filepath;

        AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");

        loggerClass = new AndroidJavaClass(pluginName);
        loggerObject = loggerClass.CallStatic<AndroidJavaObject>("getInstance", filepath);

        loggerObject.CallStatic("receiveUnityActivity", activity);

        Debug.Log($"Activity: {activity}");
    }

    public override void ClearLogs() 
    {
        ShowAlert("Clear Logs and delete Logs.txt", "Are you sure?", () =>
        {
            loggerObject.Call("clearLogs");
            textMesh.text = "";
        });
    }

    public override void ShowLogs() => textMesh.text = loggerObject.Call<string>("getLogs");

    public override void Log(string message) => loggerObject.Call("sendLog", $"{message}\n");

    public override void SaveLogs()
    {
        ShowAlert("Save logs", $"The logs will be saved in {filepath}", () => loggerObject.Call("saveLog"));
    }

    private void ShowAlert(string title, string message, Action positive = null, Action negative = null)
    {
        AlertCallback alertCallback = new AlertCallback();
        alertCallback.positiveAction = positive;
        alertCallback.negativeAction = negative;

        loggerObject.Call("createAlert", new object[] { title, message, alertCallback });
        loggerObject.Call("showAlert");
    }
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

    public override void ClearLogs() 
    {
        logs = "";

        textMesh.text = "";

        if(File.Exists(filepath))
            File.Delete(filepath);
    }

    public override void ShowLogs() => textMesh.text = logs;

    public override void Log(string message) => logs += $"{message}\n";

    public override void SaveLogs() => File.WriteAllText(filepath, logs);
}