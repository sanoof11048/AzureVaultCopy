namespace AzureVaultCopy.Models
{
    public class ApiKey
    {
        public int ConfigId { get; set; }
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
        public DateTime LastRotated { get; set; }
        public short RotationHours { get; set; }
    }
}
