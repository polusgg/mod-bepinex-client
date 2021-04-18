namespace PolusggSlim.Api.Response
{
    public class GenericResponse<T> where T : new()
    {
        public string Success { get; set; } = "";
        public T Data { get; set; } = new();
    }
}