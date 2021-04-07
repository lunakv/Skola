namespace Client
{
    public class Config
    {
        public string Login { get; set; } = "lunakv-mw2-client";
        public int Key { get; set; }
        public int Limit { get; set; } = 10;
        public string Query { get; set; }
        public string Hostname { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
        public bool Debug { get; set; }
    }
}