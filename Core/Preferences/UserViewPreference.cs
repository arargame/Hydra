using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hydra.AccessManagement;

namespace Hydra.Core.Preferences
{
    /// <summary>
    /// Kullanıcının sayfa/grid bazında özelleştirdiği kolon görünürlüklerini,
    /// genişliklerini, sıralamalarını ve aktif filtre tercihlerini saklayan kalıcı model.
    /// </summary>
    public class UserViewPreference : BaseObject<UserViewPreference>
    {
        /// <summary>
        /// Tercihin sahibi olan kullanıcı (Anonim/Genel şablonlar için nullable).
        /// </summary>
        public Guid? SystemUserId { get; set; }

        [ForeignKey("SystemUserId")]
        public SystemUser? SystemUser { get; set; }

        /// <summary>
        /// İlgili view/ekran kimliği (örn: "RequestCategory", "Request", "Employee").
        /// </summary>
        [Required]
        public string ViewKey { get; set; } = string.Empty;

        /// <summary>
        /// Tercih / Preset adı (örn: "Default", "My Custom View", "Support Desk").
        /// </summary>
        public string? PresetName { get; set; }

        /// <summary>
        /// Kullanıcının bu ekran için varsayılan tercihi olup olmadığı.
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// Kolon görünürlükleri, sıraları ve filtre barı tercihlerini tutan JSON konfigürasyonu.
        /// </summary>
        [Required]
        public string ConfigurationJson { get; set; } = string.Empty;

        public UserViewPreference()
        {
        }
    }
}
