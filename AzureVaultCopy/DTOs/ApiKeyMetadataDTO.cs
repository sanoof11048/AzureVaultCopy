namespace AzureVaultCopy.DTOs
{
    public class ApiKeyMetadataDTO
    {
        public string KeyName { get; set; }
        public DateTime LastRotated { get; set; }
        public short RotationMinutes { get; set; }

        public DateTime NextRotation => LastRotated.AddMinutes(RotationMinutes);
        public int RotationCount { get; set; }

    }

}
