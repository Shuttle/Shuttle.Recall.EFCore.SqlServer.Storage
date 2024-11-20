using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.Storage.Models;

[PrimaryKey(nameof(Id), nameof(System.Version))]
public class PrimitiveEvent
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public byte[] EventEnvelope { get; set; } = null!;

    public Guid EventId { get; set; }

    public Guid EventTypeId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SequenceNumber { get; set; }

    public DateTime DateRegistered { get; set; }

    public Guid? CorrelationId { get; set; }

    public class Specification
    {
        private readonly List<Type> _eventTypes = new();
        private readonly List<Guid> _ids = new();
        public IEnumerable<Type> EventTypes => _eventTypes.AsReadOnly();

        public IEnumerable<Guid> Ids => _ids.AsReadOnly();
        public long SequenceNumberStart { get; private set; }
        public long SequenceNumberEnd { get; private set; }
        public int MaximumRows { get; private set; }

        public Specification AddEventType<T>()
        {
            AddEventType(typeof(T));

            return this;
        }

        public Specification AddEventType(Type type)
        {
            Guard.AgainstNull(type, nameof(type));

            if (!_eventTypes.Contains(type))
            {
                _eventTypes.Add(type);
            }

            return this;
        }

        public Specification AddEventTypes(IEnumerable<Type> types)
        {
            foreach (var type in types ?? Enumerable.Empty<Type>())
            {
                AddEventType(type);
            }

            return this;
        }

        public Specification AddId(Guid id)
        {
            Guard.AgainstNull(id, nameof(id));

            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var type in ids ?? Enumerable.Empty<Guid>())
            {
                AddId(type);
            }

            return this;
        }

        public Specification WithRange(long sequenceNumberStart, int count)
        {
            if (count < 1)
            {
                throw new ArgumentException(Resources.CountMustBeGreaterThanZero);
            }

            SequenceNumberStart = sequenceNumberStart;
            SequenceNumberEnd = sequenceNumberStart + (count - 1);

            return this;
        }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }
    }
}