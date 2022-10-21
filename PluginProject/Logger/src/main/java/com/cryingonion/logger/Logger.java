package com.cryingonion.logger;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class Logger
{
    private static final String LOGTAG = "Crying Onion";

    static String filepath;
    static String logs = "";
    static File file;

    private static final Logger ourInstance = new Logger();

    private static Activity unityActivity;
    AlertDialog.Builder builder;

    private Logger()
    {
        Log.i(LOGTAG, "Created Logger Plugin");
    }

    public static Logger getInstance(String path)
    {
        filepath = path;

        loadLog();

        return  ourInstance;
    }

    public static void receiveUnityActivity(Activity tActivity)
    {
        unityActivity = tActivity;
    }

    public void createAlert(String title, String message, AlertCallback alertCallback)
    {
        builder = new AlertDialog.Builder(unityActivity);
        builder.setTitle(title);
        builder.setMessage(message);
        builder.setCancelable(false);
        builder.setPositiveButton(
                "YES",
                new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Log.i(LOGTAG, "Clicked from plugin - YES");
                        alertCallback.onPositive();
                        dialog.cancel();
                    }
                }
        );

        builder.setNegativeButton(
                "NO",
                new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Log.i(LOGTAG, "Clicked from plugin - NO");
                        alertCallback.onNegative();
                        dialog.cancel();
                    }
                }
        );
    }

    public void showAlert()
    {
        AlertDialog alert = builder.create();
        alert.show();
    }

    public void sendLog(String message)
    {
        Log.i(LOGTAG, message);
        logs += message;
    }

    public String getLogs()
    {
        Log.i(LOGTAG, "All Logs are returned!");

        return logs;
    }

    private static void loadLog()
    {
        File file = new File(filepath);

        if(file.exists())
        {
            try {
                FileInputStream stream = new FileInputStream(file);
                byte[] bytes = new byte[(int) file.length()];
                try {
                    stream.read(bytes);
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
                finally
                {
                    try
                    {
                        stream.close();
                        logs = new String(bytes);
                        Log.i(LOGTAG, "File loaded!");
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }

            } catch (FileNotFoundException e) {
                e.printStackTrace();
            }
        }
    }

    public void saveLog()
    {
        if(file == null)
            file = new File(filepath);
        else
            file.delete();

        try {
            if(file.createNewFile()) {
                FileOutputStream stream = new FileOutputStream(file);
                Log.i(LOGTAG, "File created!");
                try {
                    stream.write(logs.getBytes());
                    Log.i(LOGTAG, "File saved!");
                } finally {
                    stream.close();
                    Log.i(LOGTAG, "File closed!");
                }
            }

        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void clearLogs()
    {
        logs = "";
        file.delete();

        Log.i(LOGTAG, "All Logs are cleared!");
    }
}