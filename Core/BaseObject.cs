﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Core
{
    public interface IBaseObject<T> where T : IBaseObject<T>
    {
        Guid Id { get; set; }

        string? Name { get; set; }

        string? Description {  get; set; }

         DateTime AddedDate { get; set; }

         DateTime ModifiedDate { get; set; }

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
    
        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime AddedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

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

        public override string ToString()
        {
            return $"{Id}/{Name}";
        }
    }
}
