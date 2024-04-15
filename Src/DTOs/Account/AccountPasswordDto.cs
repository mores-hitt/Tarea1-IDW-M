public class AccountPasswordDto
{
    public required byte[] EncryptedPassword { get; set; }

    public required byte[] PasswordSalt { get; set; }

}