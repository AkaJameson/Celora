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
                        SimpleLog.Info(message);
                        break;
                    }
                case Loglevel.Fatal:
                    {
                        SimpleLog.Fatal(message);
                        break;
                    }
                case Loglevel.Warning:
                    {
                        SimpleLog.Warning(message);
                        break;
                    }
                case Loglevel.Error:
                    {
                        SimpleLog.Error(message); 
                        break;
                    }

            }
        }
    }
}
