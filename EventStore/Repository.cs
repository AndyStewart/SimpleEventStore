using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventStore
{
    public class Repository
    {
        public void Save(Company company)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113);
            using (var connection = EventStoreConnection.Create(ConnectionSettings.Default, endPoint))
            {
                connection.Connect();
                var events = company.Events.Cast<object>().Select(q => ToEventData(Guid.NewGuid(), q)).ToList();
                connection.AppendToStream("Company-" + company.Id, ExpectedVersion.NoStream, events);
            }
        }

        public Company GetById(Guid id)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113);
            var company = new Company();
            using (var connection = EventStoreConnection.Create(ConnectionSettings.Default, endPoint))
            {
                connection.Connect();
                
                StreamEventsSlice slice;
                do
                {
                    slice = connection.ReadStreamEventsForward("Company-" + id, 0, 10, false);
                    foreach (var storedEvent in slice.Events)
                    {
                        var data = DeserializeEvent(storedEvent.OriginalEvent.Metadata, storedEvent.OriginalEvent.Data);
                        var applyMethod = company.GetType().GetMethod("Apply", new[] { data.GetType() });
                        applyMethod.Invoke(company, new[] { data });
                    }

                } while (!slice.IsEndOfStream);
            }

            return company;
        }

        private static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property("EventClrTypeName").Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }

        private static EventData ToEventData(Guid eventId, object evnt)
        {
            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, serializerSettings));

            var eventHeaders = new Dictionary<string, object>()
                {
                    {
                        "EventClrTypeName", evnt.GetType().AssemblyQualifiedName
                    }
                };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, serializerSettings));
            var typeName = evnt.GetType().Name;

            return new EventData(eventId, typeName, true, data, metadata);
        }

    }
}