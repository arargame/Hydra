using Hydra.Core.Http;
using Hydra.DTOs;
using Hydra.Http;
using Hydra.Services.Core;
using Hydra.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public interface ICollectionLinkService
    {
        Task<IResponseObject> LinkAsync(CollectionLinkDTO dto);

        Task<IResponseObject> UnlinkAsync(CollectionLinkDTO dto);
    }

    /// <summary>
    /// Çoka-çok ilişkilerde köprü kayıtlarını kuran/koparan tek generic servis.
    ///
    /// Neden tek servis: her ilişki için ayrı endpoint yazmak (Role'e izin bağla, kullanıcıya rol
    /// bağla, kullanıcıya izin bağla...) aynı kodu üç kere çoğaltmak demekti. Köprü tablonun adı
    /// istekte geldiği için burası hepsini karşılıyor; yeni bir ilişki eklendiğinde yazılacak
    /// sunucu kodu yok.
    ///
    /// Yazma işlemi normal Service&lt;T&gt; üzerinden yapılıyor (doğrudan DbContext'e inilmiyor), ki
    /// audit/log/cache davranışı elle açılan kayıtlarla birebir aynı kalsın.
    /// </summary>
    public class CollectionLinkService : ICollectionLinkService
    {
        /// <summary>
        /// IService&lt;T&gt; metot adları. nameof kullanılamıyor: T çalışma zamanında belli olduğu
        /// için arayüzü derleme zamanında somut bir tiple kapatmak gerekirdi ve bu, sadece isim
        /// almak için yapay bir generic argüman uydurmak demekti.
        /// </summary>
        private static class MethodNames
        {
            public const string Create = "CreateAsync";
            public const string DeleteRange = "DeleteRangeAsync";
            public const string SelectThenCache = "SelectThenCache";
        }

        private readonly IServiceFactory _serviceFactory;

        public CollectionLinkService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<IResponseObject> LinkAsync(CollectionLinkDTO dto)
        {
            if (dto is null || !dto.IsValid())
                return ResponseFactory.BadRequest("Collection link request is missing required fields.", actionName: "Link");

            var context = Resolve(dto);

            if (context is null)
                return ResponseFactory.BadRequest($"Unknown cross table '{dto!.CrossTableName}'.", actionName: "Link");

            var existing = await LoadExistingAsync(context, dto);

            //Zaten bağlı olanları atla. GedenLines'ın bu kontrolü yoktu: aynı çifti iki kez
            //bağlamak ya mükerrer satır bırakıyor ya da DB'deki unique index'e çarpıyordu.
            var alreadyLinked = existing
                .Select(row => GetGuid(row, dto.RightKeyName!))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var toLink = dto.RightIds.Distinct().Where(id => id != Guid.Empty && !alreadyLinked.Contains(id)).ToList();

            var linked = 0;

            foreach (var rightId in toLink)
            {
                var bridge = Activator.CreateInstance(context.CrossType);

                if (bridge is null)
                    continue;

                SetValue(bridge, dto.LeftKeyName!, dto.LeftKeyValue);
                SetValue(bridge, dto.RightKeyName!, rightId);

                var response = await InvokeAsync<IResponseObject>(context, MethodNames.Create, new[] { context.CrossType }, bridge);

                if (response?.Success == true)
                    linked++;
            }

            var skipped = dto.RightIds.Distinct().Count() - toLink.Count;

            return ResponseFactory.Success(
                data: new { Linked = linked, AlreadyLinked = skipped },
                id: dto.LeftKeyValue,
                actionName: "Link",
                title: "Linked",
                text: skipped > 0
                    ? $"{linked} record(s) linked, {skipped} were already linked."
                    : $"{linked} record(s) linked.");
        }

        public async Task<IResponseObject> UnlinkAsync(CollectionLinkDTO dto)
        {
            if (dto is null || !dto.IsValid())
                return ResponseFactory.BadRequest("Collection unlink request is missing required fields.", actionName: "Unlink");

            var context = Resolve(dto);

            if (context is null)
                return ResponseFactory.BadRequest($"Unknown cross table '{dto!.CrossTableName}'.", actionName: "Unlink");

            var existing = await LoadExistingAsync(context, dto);

            var wanted = dto.RightIds.ToHashSet();

            //Kullanıcı SAĞ tarafın (ör. Permission) Id'lerini seçiyor, ama silinecek olan KÖPRÜ
            //kayıtlarıdır. GedenLines bu çeviriyi istemcide ekstra bir sorgu turuyla yapıyordu;
            //burada sunucuda, elimizdeki listeden yapılıyor — fazladan gidiş-dönüş yok.
            var bridgeIds = existing
                .Where(row =>
                {
                    var rightId = GetGuid(row, dto.RightKeyName!);
                    return rightId.HasValue && wanted.Contains(rightId.Value);
                })
                .Select(row => GetGuid(row, "Id"))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (bridgeIds.Count == 0)
                return ResponseFactory.Success(id: dto.LeftKeyValue, actionName: "Unlink", title: "Unlinked", text: "Nothing to unlink.");

            var response = await InvokeAsync<IResponseObject>(context, MethodNames.DeleteRange, new[] { typeof(List<Guid>) }, bridgeIds);

            return ResponseFactory.Success(
                data: new { Unlinked = bridgeIds.Count },
                id: dto.LeftKeyValue,
                actionName: "Unlink",
                title: "Unlinked",
                text: $"{bridgeIds.Count} record(s) unlinked.");
        }

        // ------------------------------------------------------------------------------------
        // Reflection yardımcıları. Köprü tipi ancak çalışma zamanında bilindiği için Service<T>
        // generic'ine derleme zamanında bağlanamıyoruz.
        // ------------------------------------------------------------------------------------

        private sealed record LinkContext(Type CrossType, Type ServiceType, object ServiceInstance);

        private LinkContext? Resolve(CollectionLinkDTO dto)
        {
            var crossType = EntityTypeResolver.Resolve(dto.CrossTableName);

            if (crossType is null)
                return null;

            var serviceType = typeof(IService<>).MakeGenericType(crossType);

            object serviceInstance;

            try
            {
                serviceInstance = _serviceFactory.GetService(serviceType);
            }
            catch
            {
                return null;
            }

            return new LinkContext(crossType, serviceType, serviceInstance);
        }

        /// <summary>Sol tarafa bağlı bütün köprü satırlarını getirir (hem duplicate kontrolü hem unlink için).</summary>
        private async Task<List<object>> LoadExistingAsync(LinkContext context, CollectionLinkDTO dto)
        {
            // x => x.<LeftKeyName> == <LeftKeyValue>
            var parameter = Expression.Parameter(context.CrossType, "x");
            var property = Expression.Property(parameter, dto.LeftKeyName!);
            var body = Expression.Equal(property, Expression.Constant(dto.LeftKeyValue, property.Type));

            var delegateType = typeof(Func<,>).MakeGenericType(context.CrossType, typeof(bool));
            var predicate = Expression.Lambda(delegateType, body, parameter);

            var result = await InvokeAsync<object>(context,
                                                   MethodNames.SelectThenCache,
                                                   new[] { typeof(Expression<>).MakeGenericType(delegateType) },
                                                   predicate);

            return result is IEnumerable rows
                ? rows.Cast<object>().Where(r => r is not null).ToList()
                : new List<object>();
        }

        private async Task<TResult?> InvokeAsync<TResult>(LinkContext context, string methodName, Type[] argumentTypes, params object[] arguments)
            where TResult : class
        {
            var method = context.ServiceType.GetMethod(methodName, argumentTypes)
                         ?? context.ServiceType.GetMethods().FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argumentTypes.Length);

            if (method is null)
                return null;

            if (method.Invoke(context.ServiceInstance, arguments) is not Task task)
                return null;

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");

            return resultProperty?.GetValue(task) as TResult;
        }

        private static void SetValue(object target, string propertyName, object value)
            => target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.SetValue(target, value);

        private static Guid? GetGuid(object source, string propertyName)
        {
            var raw = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(source);

            return raw is Guid guid ? guid : null;
        }
    }
}
