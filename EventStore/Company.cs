using System;
using System.Collections;

namespace EventStore
{
    public class Company
    {
        public ArrayList Events { get; private set; }

        public Guid Id { get; private set; }

        public Company(string companyName) : this()
        {
            var companyCreatedEvent = new CompanyCreatedEvent {CompanyName = companyName};
            Events.Add(companyCreatedEvent);
            Apply(companyCreatedEvent);
        }

        public void Apply(CompanyCreatedEvent companyCreatedEvent)
        {
            Name = companyCreatedEvent.CompanyName;
        }

        public Company()
        {
            Events = new ArrayList();
            Id = Guid.NewGuid();
        }

        public void Rename(string newName)
        {
            Events.Add(new CompanyRenamedEvent { CompanyName = newName});
        }

        public void Apply(CompanyRenamedEvent data)
        {
            Name = data.CompanyName;
        }

        public string Name { get; private set; }
    }
}