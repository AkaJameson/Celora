namespace Si.CoreHub.Logging
{
    public static class LogCenter
    {
        public static void Write2Log(Loglevel logLevel, string message)
        {
            switch (logLevel)
            {
                case Loglevel.Info:
                    {
                        SiLog.Info(message);
                        break;
                    }
                case Loglevel.Fatal:
                    {
                        SiLog.Fatal(message);
                        break;
                    }
                case Loglevel.Warning:
                    {
                        SiLog.Warning(message);
                        break;
                    }
                case Loglevel.Error:
                    {
                        SiLog.Error(message); 
                        break;
                    }

            }
        }
    }
}
