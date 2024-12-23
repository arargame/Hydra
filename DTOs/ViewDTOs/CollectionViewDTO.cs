using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs.ViewDTOs
{

    public interface ICollectionViewDTO
    {
        string? Title { get; set; }

        string? EntityType { get; set; }

        int Priority { get; set; }

        string? CollectionViewDTOName { get; set; }

        bool IsCrossTable { get; set; }

        string? LeftTableKey { get; set; }

        string? LeftTableKeyValue { get; set; }

        string? CrossTableName { get; set; }

        string? RightTableName { get; set; }

        string? RightTableKey { get; set; }


        Dictionary<string, object?> Parameters { get; set; }

        CollectionViewDTO SetParameters(Dictionary<string, object?> parameters);

        CollectionViewDTO SetTitle(string? title);
    }

    public class CollectionViewDTO : ViewDTO, ICollectionViewDTO,ICollectionHandlerDTO
    {
        private string? controllerName { get; set; }

        public override string? ControllerName
        {
            get
            {
                return (controllerName = !string.IsNullOrEmpty(base.ControllerName) ? base.ControllerName : EntityType);
            }
            set
            {
                controllerName = value;
            }
        }

        public string? Title { get; set; } = null;

        public string? EntityType { get; set; } = null;

        public int Priority { get; set; }

        public string? CollectionViewDTOName { get; set; } = null;

        public bool IsCrossTable { get; set; }

        public string? LeftTableName { get; set; } = null;

        public string? LeftTableKey { get; set; } = null;

        public string? LeftTableKeyValue { get; set; } = null;

        public string? CrossTableName { get; set; } = null;

        public string? RightTableName { get; set; } = null;

        public string? RightTableKey { get; set; } = null;

        public Dictionary<string, object?> Parameters { get; set; } = new();

        public CollectionViewDTO()
        {
            CollectionViewDTOName = GetType().Name;
        }

        public CollectionViewDTO SetParameters(Dictionary<string, object?> parameters)
        {
            Parameters = parameters;

            return this;
        }

        public CollectionViewDTO SetPriority(int priority)
        {
            Priority = priority;

            return this;
        }

        public virtual CollectionViewDTO SetEntityType(string? entityType)
        {
            EntityType = entityType;

            return this;
        }

        public CollectionViewDTO SetTitle(string? title)
        {
            Title = title;

            return this;
        }

        public CollectionViewDTO SetLeftTableName(string? leftTableName)
        {
            LeftTableName = leftTableName;

            return this;
        }

        public CollectionViewDTO SetLeftTableKey(string? leftTableKey)
        {
            LeftTableKey = leftTableKey;

            return this;
        }

        public CollectionViewDTO SetLeftTableKeyValue(string? leftTableKeyValue)
        {
            LeftTableKeyValue = leftTableKeyValue?.ToString();

            return this;
        }

        public CollectionViewDTO SetLeftTableKeyValue(Guid leftTableKeyValue)
        {
            return SetLeftTableKeyValue(leftTableKeyValue.ToString());
        }

        public CollectionViewDTO SetRightTableName(string? rightTableName)
        {
            RightTableName = rightTableName;

            SetEntityType(rightTableName);

            return this;
        }

        public CollectionViewDTO SetRightTableKey(string? rightTableKey)
        {
            RightTableKey = rightTableKey;

            return this;
        }
    }
}
