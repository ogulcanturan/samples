namespace UserManagement.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string? Street { get; init; } = null;

        public string? City { get; init; } = null;

        public string? State { get; init; } = null;

        public string? ZipCode { get; init; } = null;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return ZipCode;
        }
    }
}