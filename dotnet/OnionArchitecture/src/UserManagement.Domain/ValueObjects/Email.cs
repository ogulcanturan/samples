using UserManagement.Domain.Exceptions;

namespace UserManagement.Domain.ValueObjects
{
    /// <summary>
    /// Represents an email address.
    /// </summary>
    public class Email : ValueObject
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Email"/> class.
        /// </summary>
        public Email()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidEmailException"></exception>
        public Email(string value)
        {
            if (!IsValid(value))
            {
                throw new InvalidEmailException(value);
            }

            Value = value;
        }

        public string? Value { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string?(Email? email)
        {
            return email?.Value;
        }

        public static bool IsValid(string value)
        {
            var index = value.IndexOf('@');

            return index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@');
        }
    }
}