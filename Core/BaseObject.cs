using System;
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

        void Initialize();

        T SetName(string? name);
    }

    public abstract class BaseObject<T> : IBaseObject<T> where T : BaseObject<T>
    {
        public Guid Id { get; set; }
    
        public string? Name { get; set; }

        public string? Description { get; set; }

        public BaseObject()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            Id = Guid.NewGuid();
        }

        public T SetName(string? name)
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
