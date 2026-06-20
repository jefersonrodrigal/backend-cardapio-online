namespace Application.Common.Security;

public static class ClientPasswordHasher
{
    public static string HashPassword(string password) => PasswordHasher.HashPassword(password);

    public static bool VerifyPassword(string password, string storedHash) =>
        PasswordHasher.VerifyPassword(password, storedHash);
}
