package com.cryingonion.logger;

import android.util.Log;

public class Logger
{
    String logs = "";

    private static final Logger ourInstance = new Logger();

    private static final String LOGTAG = "Crying Onion";

    public static Logger getInstance() { return  ourInstance; }

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

    public void clearLogs()
    {
        Log.i(LOGTAG, "All Logs are cleared!");
        logs = "";
    }

    private Logger()
    {
        Log.i(LOGTAG, "Created Logger Plugin");
    }
}