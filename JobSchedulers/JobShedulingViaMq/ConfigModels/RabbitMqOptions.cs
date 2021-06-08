namespace JobShedulingViaMq.ConfigModels
{
    public class RabbitMqOptions
    {
        public string Host { get; set; }
        public string Queue { get; set; }
        public string Producer { get; set; }
    }
}