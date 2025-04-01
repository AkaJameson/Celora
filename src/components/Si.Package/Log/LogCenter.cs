namespace Si.Package.Log
{
    public static class LogCenter
    {
        public static void Write2Log(Loglevel logLevel, string message)
        {
            switch (logLevel)
            {
                case Loglevel.Info:
                    {
                        Logger.Info(message);
                        break;
                    }
                case Loglevel.Fatal:
                    {
                        Logger.Fatal(message);
                        break;
                    }
                case Loglevel.Warning:
                    {
                        Logger.Warning(message);
                        break;
                    }
                case Loglevel.Error:
                    {
                        Logger.Error(message); 
                        break;
                    }

            }
        }
    }
}
