using System;
using System.Collections.Generic;

namespace Hydra.DTOs
{
    /// <summary>
    /// Bir çoka-çok ilişkide köprü tablo üzerinden bağ kurma/koparma isteği.
    ///
    /// Örnek — Role detayındaki "Permissions" sekmesinden üç izin bağlamak:
    ///   CrossTableName = "RolePermission"
    ///   LeftKeyName    = "RoleId",       LeftKeyValue = &lt;rolün Id'si&gt;
    ///   RightKeyName   = "PermissionId", RightIds     = [izin1, izin2, izin3]
    ///
    /// Taraflar bilinçli olarak isimle taşınıyor: böylece tek bir generic uç bütün köprü
    /// tabloları karşılıyor ve her yeni ilişki için yeni endpoint/servis yazılması gerekmiyor.
    /// </summary>
    public class CollectionLinkDTO
    {
        /// <summary>Köprü entity'nin adı, ör. "RolePermission".</summary>
        public string? CrossTableName { get; set; }

        /// <summary>Köprüdeki sol taraf FK alanı, ör. "RoleId".</summary>
        public string? LeftKeyName { get; set; }

        /// <summary>Sol tarafın Id değeri — detay ekranında açık olan kaydın Id'si.</summary>
        public Guid LeftKeyValue { get; set; }

        /// <summary>Köprüdeki sağ taraf FK alanı, ör. "PermissionId".</summary>
        public string? RightKeyName { get; set; }

        /// <summary>Bağlanacak (veya koparılacak) sağ taraf kayıtlarının Id'leri.</summary>
        public List<Guid> RightIds { get; set; } = new();

        public bool IsValid()
            => !string.IsNullOrWhiteSpace(CrossTableName)
               && !string.IsNullOrWhiteSpace(LeftKeyName)
               && !string.IsNullOrWhiteSpace(RightKeyName)
               && LeftKeyValue != Guid.Empty
               && RightIds is { Count: > 0 };
    }
}
