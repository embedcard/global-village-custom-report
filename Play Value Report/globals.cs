public class globals
{
    private static string _connString;
    public static string ConnectionString
    {
        get
        {
            return _connString;
        }
        set
        {
            _connString = value;
        }
    }

    private static string _catString;
    public static string GameCatString
    {
        get
        {
            return _catString;
        }
        set
        {
            _catString = value;
        }
    }

    private static string _path;
    public static string ExportPath
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
        }
    }

    private static string _dir;
    public static string ExportDir
    {
        get
        {
            return _dir;
        }
        set
        {
            _dir = value;
        }
    }

}