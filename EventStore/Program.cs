namespace EventStore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var repository = new Repository();
            var company = new Company("My Company");
            var otherCompany = new Company("Other Company");
            repository.Save(company);

            otherCompany.Rename("New Name");
            repository.Save(otherCompany);

            var companyFromDb = repository.GetById(otherCompany.Id);

        }
    }
}