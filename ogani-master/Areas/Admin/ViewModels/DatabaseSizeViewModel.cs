public class DatabaseSizeViewModel
{
    public string TableName { get; set; }
    public string Capacity { get; set; }
}

public class DatabaseSizeInfo
{
    public List<DatabaseSizeViewModel> tableSizes { get; set; }
    public string Capacity { get; set; }
}
