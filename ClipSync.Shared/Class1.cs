namespace ClipSync.Shared;

public class Class1
{

}

public class ClipClass
{
    public string ID { get; set; }
    public string Text { get; set; }
    public string AuthToken { get; set; }
    public string ClipCreatorUsername { get; set; }
}

public class UserClass
{
    public string Username { get; set; }
    public string? Email { get; set; }
    public string HashedPassword { get; set; }
    public List<string> ClipIDList { get; set; }
}

public static class MyShared
{
    public static readonly string BackendUrl = "";
} 

