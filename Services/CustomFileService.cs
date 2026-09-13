using Hydra.DI;
using Hydra.DTOs;
using Hydra.FileOperations;
using Hydra.Http;
using Hydra.Services.Core;
using System.Linq.Expressions;

namespace Hydra.Services
{
    /// <summary>
    /// Adds the two operations CustomFile needs beyond the generic Service&lt;T&gt; surface:
    /// building entities out of an upload payload, and listing everything attached to a given
    /// entity. Both are consumed by Hydra.WebApi.Controllers.CustomFileController.
    ///
    /// Adapted from a working upload/download pattern in the GedenLines codebase
    /// (WebApi.GedenLines/Controllers/CustomFileController.cs), simplified to Hydra's own
    /// CustomFile shape (string EntityId/EntityType, not Guid?) and IResponseObject/async
    /// conventions.
    /// </summary>
    [RegisterAsService(typeof(IService<CustomFile>))]
    public class CustomFileService : Service<CustomFile>
    {
        public CustomFileService(ServiceInjector injector) : base(injector) { }

        /// <summary>
        /// Creates one CustomFile per item in the upload, all attached to the same entity
        /// reference. Each create goes through the normal Service&lt;T&gt;.CreateAsync path
        /// (validation, commit, cache, logging) — nothing here bypasses it.
        /// </summary>
        public async Task<IResponseObject> UploadAsync(FileUploadDTO uploadDTO)
        {
            var response = new ResponseObject()
                                .SetActionName("Upload")
                                .UseDefaultMessages();

            if (uploadDTO.Items.Count == 0)
                return response.SetSuccess(false)
                                .AddExtraMessage(new ResponseObjectMessage("Upload", "No file was sent.", false));

            var createdFiles = new List<CustomFile>();

            foreach (var item in uploadDTO.Items)
            {
                var file = new CustomFile()
                                .SetName(item.FileName)
                                .SetData(item.FileContent);

                file.EntityType = uploadDTO.EntityType;
                file.EntityId = uploadDTO.EntityId;

                var createResponse = await CreateAsync(file);

                if (createResponse.Success)
                {
                    createdFiles.Add(file);
                }
                else
                {
                    response.AddExtraMessage(new ResponseObjectMessage("Upload", $"{item.FileName}: upload failed.", false));
                }
            }

            return response.SetSuccess(createdFiles.Count == uploadDTO.Items.Count)
                            .SetData(createdFiles);
        }

        /// <summary>
        /// Every file attached to one entity — e.g. the attachments on a single Request.
        /// Uses the same EntityType/EntityId pair CustomFile itself carries
        /// (Hydra.Core.IHasEntityReference), no join table involved.
        /// </summary>
        public async Task<List<CustomFile>> GetByEntityAsync(string entityType, string entityId)
        {
            Expression<Func<CustomFile, bool>> filter = f =>
                f.EntityType == entityType && f.EntityId == entityId;

            return await SelectWithLinqAsync(filter: filter);
        }

        public async Task<IResponseObject> RemoveAllFilesByEntityAsync(string entityType, string entityId)
        {
            var files = await GetByEntityAsync(entityType, entityId);

            return await DeleteRangeAsync(files);
        }
    }
}
