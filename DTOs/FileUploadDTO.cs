using System;
using System.Collections.Generic;

namespace Hydra.DTOs
{
    /// <summary>
    /// Wire contract for POST {Entity}/Upload (see Hydra.WebApi.Controllers.CustomFileController).
    /// Carries one or more raw files plus the polymorphic entity reference they should be
    /// attached to — mirrors CustomFile's own IHasEntityReference fields
    /// (Hydra/Core/IHasEntityReference.cs). Kept separate from CustomFile itself so the wire
    /// format (raw bytes per item, base64 over JSON) doesn't leak into the entity's own shape.
    ///
    /// NOTE: unlike some adaptations of this pattern seen elsewhere, this DTO does not carry a
    /// free-form "Category" tag — Hydra's CustomFile has no settable field for one today (its
    /// only "Category" is the derived FileCategory enum from the extension, see CustomFile.cs).
    /// Adding a taggable field would need an entity change + migration, left for a future pass.
    /// </summary>
    public class FileUploadDTO
    {
        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public List<FileUploadItemDTO> Items { get; set; } = new();

        public FileUploadDTO() { }

        public FileUploadDTO(string? entityType, string? entityId)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// One raw file as sent from the browser. FileName carries the extension
    /// ("photo.png") — CustomFile.SetName already knows how to split that, so this DTO
    /// doesn't duplicate that logic (see CustomFileService.UploadAsync).
    /// </summary>
    public class FileUploadItemDTO
    {
        public string FileName { get; set; } = string.Empty;

        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        public FileUploadItemDTO() { }

        public FileUploadItemDTO(string fileName, byte[] content)
        {
            FileName = fileName;
            FileContent = content;
        }
    }
}
