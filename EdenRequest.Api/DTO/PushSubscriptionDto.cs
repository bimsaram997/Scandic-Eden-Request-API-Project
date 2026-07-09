namespace EdenRequest.Api.DTO
{
    public class PushSubscriptionDto
    {
        public string PushEndpoint { get; set; } = string.Empty;
        public string PushP256DH { get; set; } = string.Empty;
        public string PushAuth { get; set; } = string.Empty;
    }
}
