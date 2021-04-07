namespace Client
{
    public class Config
    {
        public string Login { get; set; } = "lunakv-mw2-client";
        public string Query { get; set; } = "ItemA,ItemB";
        public string Hostname { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
        public bool Debug { get; set; } = false;
    }
}