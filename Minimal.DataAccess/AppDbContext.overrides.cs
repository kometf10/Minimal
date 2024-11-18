using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Minimal.Domain.Core;
using Minimal.Domain.Core.Logging;
using Minimal.Domain.Core.TaggingInterfaces;
using Minimal.Domain.Core.Reflection;
using Minimal.DataAccess.Converters;
using System.Reflection;

namespace Minimal.DataAccess
{
    public partial class AppDbContext
    {
        private readonly static bool SafeDelete = true;
        private readonly static List<string> BasicAuditValues = new List<string> { nameof(BaseEntity.CreatedAt), nameof(BaseEntity.CreatedBy), nameof(BaseEntity.LastModefiedAt), nameof(BaseEntity.LastModifiedBy) };


        public override int SaveChanges()
        {
             OnBeforeSaving();

            if (!CheckDataAccessKey())
                return -1;

            var result = base.SaveChanges();

            //OnAfterSaving(auditEntites!);

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();

            if (!CheckDataAccessKey())
                return -1;

            var result = await base.SaveChangesAsync(cancellationToken);

            //OnAfterSaving(auditEntity!);

            return result;
        }


        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");

            builder.Properties<TimeOnly>()
                .HaveConversion<TimeOnlyConverter>()
                .HaveColumnType("time");
            base.ConfigureConventions(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (Type type in GetEntityTypes())
            {
                var method = SetGlobalQueryMethod.MakeGenericMethod(type);
                method.Invoke(this, new object[] { modelBuilder });
            }

            //Set Delete Behavior If Safe Delete not Applyed
            //var relationships = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
            //foreach (var relationship in relationships)            
            //    relationship.DeleteBehavior = DeleteBehavior.Cascade;


            base.OnModelCreating(modelBuilder);
        }

        private void OnBeforeSaving()
        {
            BasicAudit();
            CheckConcurrencyStamp();

            //var auditEntities = FullAuditBefore();
            //return auditEntities;
        }

        //private void OnAfterSaving(List<AuditEntry> auditEntities)
        //{
            //FullAuditAfter(auditEntities);
        //}

        private bool CheckDataAccessKey()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                var dataAccessEnabled = entry.Entity.GetType().GetInterfaces().Any(i => i == typeof(IEnableDataAccess));
                if (entry.Entity is BaseEntity trackable && dataAccessEnabled)
                {
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        var userDataAccessKey = GetUserDataAccessKey();
                        _ = userDataAccessKey ?? throw new ArgumentNullException(nameof(userDataAccessKey));

                        var entryDataAccessKey = entry.CurrentValues.GetValue<string>("DataAccessKey");
                        if (!string.IsNullOrEmpty(entryDataAccessKey) && entryDataAccessKey.StartsWith(userDataAccessKey))
                            return true;

                        return false;
                    }
                    return true;
                }
            }
            return true;
        }

        private void SetGlobalQuery<T>(ModelBuilder modelBuilder) where T : BaseEntity
        {
            modelBuilder.Entity<T>().HasKey(e => e.Id);

            modelBuilder.Entity<T>().HasIndex(e => e.IsDeleted);
            modelBuilder.Entity<T>().HasIndex(e => e.DataAccessKey);

            bool enableDataAccess = typeof(T).GetInterfaces().Any(i => i == typeof(IEnableDataAccess));

            modelBuilder.Entity<T>().HasQueryFilter(e => (e.DataAccessKey!.StartsWith(UserDataAccessKey!) || !enableDataAccess) && !e.IsDeleted);

        }

        private string? GetLoggedUser()
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                if (httpContext.User != null)
                {
                    var user = httpContext.User.FindFirst("Id");
                    if (user != null)
                        return user.Value;
                }
            }
            return null;
        }

        private string? GetUserDataAccessKey()
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                if (httpContext.User != null)
                {
                    var user = httpContext.User.FindFirst("DataAccessKey");
                    if (user != null)
                        return user.Value;
                }
            }
            return null;
        }


        private void CheckConcurrencyStamp()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity trackable)
                {
                    var concurrencyCheckIgnored = trackable.GetType().IsTaggedWith(typeof(IIgnoreConcurrencyCheck));
                    if (concurrencyCheckIgnored)
                        continue;

                    if (entry.State == EntityState.Modified)
                    {
                        var orginalStamp = entry.OriginalValues.GetValue<string>(nameof(BaseDto.ConcurrencyStamp));
                        var currentStamp = entry.CurrentValues.GetValue<string>(nameof(BaseDto.ConcurrencyStamp));

                        if (!string.IsNullOrEmpty(orginalStamp) && orginalStamp != currentStamp)
                            throw new ValidationException("The Current Version of the Data Have Been Updated By Another User (Get Latest Version and Try Update Again)");

                        trackable.ConcurrencyStamp = Guid.NewGuid().ToString();
                    }
                }
            }
        }


        private void BasicAudit()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
                if (entry.Entity is BaseEntity trackable)
                {
                    var now = DateTime.UtcNow;
                    var userId = GetLoggedUser();
                    var userDataAccessKey = string.IsNullOrEmpty(trackable.DataAccessKey) ? GetUserDataAccessKey() : trackable.DataAccessKey;
                    var createdAt = entry.GetDatabaseValues()?.GetValue<DateTime?>("CreatedAt");
                    var createdBy = entry.GetDatabaseValues()?.GetValue<string>("CreatedBy");
                    var currDataAccessKey = entry.GetDatabaseValues()?.GetValue<string>("DataAccessKey");
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.LastModefiedAt = now;
                            trackable.LastModifiedBy = userId;
                            trackable.CreatedAt = createdAt;
                            trackable.CreatedBy = createdBy;
                            trackable.DataAccessKey = currDataAccessKey;
                            break;
                        case EntityState.Added:
                            trackable.CreatedAt = now;
                            trackable.CreatedBy = userId;
                            trackable.DataAccessKey = userDataAccessKey;
                            break;
                        case EntityState.Deleted:
                            if (SafeDelete)
                            {
                                entry.State = EntityState.Modified;
                                trackable.LastModefiedAt = now;
                                trackable.LastModifiedBy = userId;
                                trackable.CreatedAt = createdAt;
                                trackable.CreatedBy = createdBy;
                                trackable.IsDeleted = true;
                            }
                            break;
                    }
                }
        }


        public static MethodInfo SetGlobalQueryMethod = typeof(AppDbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                                            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQuery");

        private static IList<Type>? EntityTypeCache;
        private static IList<Type> GetEntityTypes()
        {
            if (EntityTypeCache != null)
                return EntityTypeCache;

            var domainAssymbly = Assembly.Load(new AssemblyName("ITS.Domain"));

            EntityTypeCache = domainAssymbly.DefinedTypes
                .Where(t => t.BaseType == typeof(BaseEntity))
                .Select(t => t.AsType())
                .ToList();

            return EntityTypeCache;

        }
    }
}
