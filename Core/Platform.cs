using System.ComponentModel.DataAnnotations;

namespace Hydra.Core
{
    public enum ProjectType
    {
        Unspecified = 0,
        WebApi = 1,
        Game = 2,
        Mobile = 3,
        Desktop = 4,
        WebApp = 5,
        MicroService = 6
    }

    public class Platform : BaseObject<Platform>
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public ProjectType ProjectType { get; set; }

        [MaxLength(50)]
        public string? FrameworkVersion { get; set; }

        public Platform()
        {
            Name = string.Empty;
        }

        public Platform(string name, ProjectType projectType, string? frameworkVersion = null, string? description = null)
        {
            Initialize();
            Name = name;
            ProjectType = projectType;
            FrameworkVersion = frameworkVersion;
            Description = description;
        }

        public override void Initialize()
        {
            Id = Guid.NewGuid();
            AddedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Name} ({ProjectType}) - {FrameworkVersion}";
        }
    }
}
