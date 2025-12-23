using Hydra.Core;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hydra.FileOperations
{
    public enum FileCategory
    {
        Image,
        Audio,
        Video,
        Document,
        Unsupported
    }

    public interface ICustomFile : IBaseObject<ICustomFile>, IHasEntityReference
    {
        string? Extension { get; set; }

        string FullName { get; }

        double LengthAsMb { get; set; }

        double LengthAsGb { get; }

        double LengthAsKb { get; }

        double LengthAsByte { get; }

        byte[]? Data { get; set; }

        string? Path { get; set; }

        string? ContainerName { get; set; }

        FileCategory Category { get; }

        string? Base64String { get; }

        string? DataAsString { get; }
    }
    public class CustomFile : BaseObject<CustomFile>,ICustomFile
    {
        public string? Extension { get; set; }

        public string FullName
        {
            get
            {
                return $"{Name}.{Extension}";
            }
        }

        public double LengthAsMb { get; set; }

        private byte[]? data;

        public byte[]? Data
        {
            get => data;
            set
            {
                data = value;

                if (data != null)
                {
                    LengthAsMb = Math.Round(LengthAsKb / 1024, 4);
                }
            }
        }

        public string? Path { get; set; }

        public string? ContainerName { get; set; }

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public CustomFile()
        {

        }

        public CustomFile(byte[] data, string name)
        {
            SetName(name)
                .SetData(data);
        }

        public CustomFile(string path)
        {
            SetPath(path)
            .SetName(FileHelper.GetFileNameFromPath(path))
            .SetData(path);

        }

        public override CustomFile SetName(string? name)
        {

            return base.SetName(FileHelper.GetFileNameWithoutExtension(name ?? ""))
                        .SetExtension(FileHelper.GetFileExtensionWithoutDot(name ?? ""));
        }

        public CustomFile SetExtension(string? extension)
        {
            Extension = extension;

            return this;
        }

        public CustomFile SetPath(string? path)
        {
            Path = path;

            return this;
        }

        public CustomFile SetData(string path)
        {
            Data = FileHelper.ReadFileAsBytesFromPath(path);

            return this;
        }

        public CustomFile SetData(byte[]? data)
        {
            Data = data;

            return this;
        }

        public CustomFile SetContainerName(string containerName)
        {
            ContainerName = containerName;

            return this;
        }

        public CustomFile SetEntityId(string? entityId)
        {
            EntityId = entityId;
            
            return this;
        }

        public CustomFile SetEntityType(string? entityType)
        {
            EntityType = entityType;

            return this;
        }

        public double LengthAsGb
        {
            get
            {
                return Math.Round(LengthAsMb / 1024, 4); 
            }
        }
        public double LengthAsByte
        {
            get
            {
                return data != null ? data.Length : 0;
            }
        }

        public double LengthAsKb
        {
            get
            {
                return data != null ? (data.Length / 1024.0) : 0;
            }
        }

        public FileCategory Category
        {
            get
            {
                return FileExtensions.GetFileCategory(Extension);
            }
        }


        private string? _base64String;

        public string? Base64String
        {
            get
            {
                if (_base64String == null)
                {
                    _base64String = Helper.ConvertToBase64String(Data);
                }
                return _base64String;
            }
        }


        private string? _dataAsString;
        public string? DataAsString
        {
            get
            {
                if (_dataAsString == null)
                {
                    _dataAsString = Helper.ByteArrayToString(Data);
                }
                return _dataAsString;
            }
        }

        public MemoryStream GetMemoryStream()
        {
            return new MemoryStream(data ?? Array.Empty<byte>());
        }
    }
}
