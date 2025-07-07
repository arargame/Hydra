using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hydra.Core
{
    public interface IHasId
    {
        Guid Id { get; set; }
    }
    public interface IHasAuditFields
    {
        DateTime AddedDate { get; set; }

        DateTime? ModifiedDate { get; set; }

        byte[]? RowVersion { get; set; }
    }


    public interface IBaseObject<T> : IHasId, IHasAuditFields where T : IBaseObject<T>
    {
        string? Name { get; set; }

        string? Description {  get; set; }

        bool IsActive { get; set; }

        //void Initialize();

        //T SetName(string? name);
    }

    public interface IHierarchicalObject<T> where T : IHierarchicalObject<T>
    {
        Guid? ParentId { get; set; }
        T? Parent { get; set; }
        List<T> Children { get; set; }

        IEnumerable<T> GetAncestors()
        {
            var current = Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        IEnumerable<T> GetAllDescendants()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var descendant in child.GetAllDescendants())
                    yield return descendant;
            }
        }
    }

    public abstract class BaseObject<T> : IBaseObject<T> where T : BaseObject<T>
    {
        public Guid Id { get; set; }

        public string? Name { get; set; } = null;

        public string? Description { get; set; } = null;

        public DateTime AddedDate { get; set; }

        public DateTime? ModifiedDate { get; set; } = null;

        public bool IsActive { get; set; } = true;

        [Timestamp]
        [JsonIgnore]
        public byte[]? RowVersion { get; set; } = null;

        [NotMapped]
        public bool IsPersistent = true;

        public BaseObject()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            Id = Guid.NewGuid();
        }

        public virtual T SetName(string? name)
        {
            Name = name;

            return (T)this;
        }

        public T SetDescription(string? description)
        {
            Description = description;

            return (T)this;
        }

        public override string ToString()
        {
            return $"{Id}/{Name}";
        }

        public T MakeNonPersistent()
        {
            IsPersistent = false;

            return (T)this;
        }

        public virtual string UniqueProperty
        {
            get
            {
                return Id.ToString();
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (BaseObject<T>)obj;
            return UniqueProperty == other.UniqueProperty;
        }

        public override int GetHashCode()
        {
            return UniqueProperty?.GetHashCode() ?? 0;
        }
    }
}
