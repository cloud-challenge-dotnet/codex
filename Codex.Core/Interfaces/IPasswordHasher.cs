namespace Codex.Core.Interfaces
{
    public interface IPasswordHasher
    {
        string GenerateHash(string password, string salt);

        string GenerateRandowSalt(int lengthInBytes = 128);
    }
}
