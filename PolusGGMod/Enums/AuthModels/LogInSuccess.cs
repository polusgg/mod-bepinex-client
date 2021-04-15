namespace PolusGG.Enums.AuthModels
{
    public class LogInSuccess
    {
        public bool Success { get; set; }
        public LogInData Data { get; set; }
    }

    public class LogInData
    {
        public string ClientId { get; set; }
        public string ClientToken { get; set; }
    }
}