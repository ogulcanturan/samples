namespace UserManagement.Domain.Exceptions
{
    public class InvalidEmailException(string value) : Exception($"Email '{value}' is invalid.");
}