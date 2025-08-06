namespace AzureVaultCopy.DTOs
{
    public class ApiKeyMetadataDTO
    {
        public string KeyName { get; set; }
        public DateTime LastRotated { get; set; }
        public DateTime NextRotation => LastRotated.AddHours(RotationHours);
        public short RotationHours { get; set; }
    }
}
