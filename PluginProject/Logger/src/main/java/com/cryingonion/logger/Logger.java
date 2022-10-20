package com.cryingonion.logger;

import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class Logger
{
    static String filepath;
    static String logs = "";

    private static final Logger ourInstance = new Logger();

    private static final String LOGTAG = "Crying Onion";

    public static Logger getInstance(String path)
    {
        filepath = path;

        loadLog();

        return  ourInstance;
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
        File file = new File(filepath);

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
        // TODO armar el popup para tomar la decicion de borrar o no los logs
        Log.i(LOGTAG, "All Logs are cleared!");
        logs = "";
    }

    private Logger()
    {
        Log.i(LOGTAG, "Created Logger Plugin");
    }
}