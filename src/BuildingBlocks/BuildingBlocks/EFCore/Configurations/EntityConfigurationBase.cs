using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.EFCore.Configurations;

public abstract class EntityTypeConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    protected EntityTypeBuilder<TEntity> Builder { get; private set; } = default!;
    protected string EntityName => typeof(TEntity).Name;

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    #region Key

    protected void ConfigurePrimaryKey(Expression<Func<TEntity, object>> keyExpression)
    {
        Builder.HasKey(keyExpression);
    }

    #endregion

    #region Properties

    protected void ConfigureString(
        Expression<Func<TEntity, string>> propertyExpression,
        bool isUnicode = false,
        int? maxLength = null,
        bool isFixedLength = false,
        bool isRequired = false,
        bool ignoreOnUpdate = false)
    {
        var propertyBuilder = Builder.Property(propertyExpression)
                                     .IsUnicode(isUnicode);

        if (maxLength.HasValue)
            propertyBuilder.HasMaxLength(maxLength.Value);
        
        if (isFixedLength)
            propertyBuilder.IsFixedLength();

        if (isRequired)
            propertyBuilder.IsRequired();

        if (ignoreOnUpdate)
            propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }

    protected void ConfigureDateTime(
        Expression<Func<TEntity, DateTime?>> propertyExpression,
        bool isRequired = false,
        bool ignoreOnUpdate = false)
    {
        Builder.Property(propertyExpression)
               .HasColumnType("timestamptz");

        if (isRequired) Builder.Property(propertyExpression).IsRequired();
        if (ignoreOnUpdate) Builder.Property(propertyExpression).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }

    protected void ConfigureDate(
        Expression<Func<TEntity, DateTime?>> propertyExpression,
        bool isRequired = false)
    {
        Builder.Property(propertyExpression)
               .HasColumnType("date");

        if (isRequired) Builder.Property(propertyExpression).IsRequired();
    }

    #region ConfigureJsonb

    protected void ConfigureJsonb<TJson>(
        Expression<Func<TEntity, TJson>> propertyExpression,
        bool isRequired = false)
    {
        var propertyBuilder = Builder.Property(propertyExpression)
                                     .HasColumnType("jsonb")
                                     .HasConversion(
                                                    v => JsonSerializer.Serialize(v, JsonOptions),
                                                    v => JsonSerializer.Deserialize<TJson>(v, JsonOptions)!);

        if (isRequired)
            propertyBuilder.IsRequired();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #endregion

    protected void ConfigureTimeSpan(Expression<Func<TEntity, TimeSpan?>> propertyExpression)
    {
        Builder.Property(propertyExpression).HasColumnType("interval");
    }

    protected void Ignore(Expression<Func<TEntity, object>> propertyExpression)
    {
        Builder.Ignore(propertyExpression);
    }

    #endregion

    #region Relationships

    protected void ConfigureOneToOne<TRelatedEntity>(
        Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
        DeleteBehavior deleteBehavior = DeleteBehavior.Restrict,
        string? foreignKeyPropertyName = null,
        string? constraintName = null)
        where TRelatedEntity : class
    {
        var navigationName = GetMemberName(navigationExpression);
        foreignKeyPropertyName ??= BuildForeignKeyPropertyName(navigationName);
        constraintName ??= BuildForeignKeyConstraintName(navigationName);

        var relationship = Builder.HasOne(navigationExpression)
                                  .WithOne()
                                  .HasForeignKey<TEntity>(foreignKeyPropertyName)
                                  .OnDelete(deleteBehavior)
                                  .HasConstraintName(constraintName);
    }

    protected void ConfigureOneToMany<TRelatedEntity>(
        Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
        Expression<Func<TRelatedEntity, IEnumerable<TEntity>>>? inverseNavigationExpression = null,
        DeleteBehavior deleteBehavior = DeleteBehavior.Restrict,
        string? foreignKeyPropertyName = null,
        string? constraintName = null)
        where TRelatedEntity : class
    {
        var navigationName = GetMemberName(navigationExpression);
        foreignKeyPropertyName ??= BuildForeignKeyPropertyName(navigationName);
        constraintName ??= BuildForeignKeyConstraintName(navigationName);

        var builder = Builder.HasOne(navigationExpression);
        
        if (inverseNavigationExpression != null)
        {
            builder.WithMany(inverseNavigationExpression)
                   .HasForeignKey(foreignKeyPropertyName)
                   .OnDelete(deleteBehavior)
                   .HasConstraintName(constraintName);
        }
        else
        {
            builder.WithMany()
                   .HasForeignKey(foreignKeyPropertyName)
                   .OnDelete(deleteBehavior)
                   .HasConstraintName(constraintName);
        }
    }

    protected void ConfigureOneToManyCollection<TRelatedEntity>(
        Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collectionExpression,
        Expression<Func<TRelatedEntity, TEntity>> inverseNavigationExpression,
        Expression<Func<TRelatedEntity, object>> foreignKeyExpression,
        DeleteBehavior deleteBehavior = DeleteBehavior.Restrict,
        string? constraintName = null)
        where TRelatedEntity : class
    {
        // ۱. تنظیم رابطه
        var relation = Builder.HasMany(collectionExpression)
                              .WithOne(inverseNavigationExpression)
                              .HasForeignKey(foreignKeyExpression)
                              .OnDelete(deleteBehavior);

        if (!string.IsNullOrWhiteSpace(constraintName))
            relation.HasConstraintName(constraintName);

        // ۲. شناسایی خودکار نام navigation و تنظیم PropertyAccessMode
        var navigationName = GetMemberName(collectionExpression);
        Builder.Metadata.FindNavigation(navigationName)?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
    #endregion
    
    #region Indexes

    protected IndexBuilder<TEntity> ConfigureIndex(
        Expression<Func<TEntity, object>> indexExpression,
        string? indexName = null,
        bool isUnique = false)
    {
        var propertyNames = GetPropertyNames(indexExpression).ToArray();
    
        // اگر نام ایندکس داده نشده، آن را تولید کن
        indexName ??= BuildIndexName(propertyNames, isUnique);

        var indexBuilder = Builder.HasIndex(propertyNames, indexName);

        // تنظیم unique بودن
        if (isUnique)
        {
            indexBuilder.IsUnique();
        }

        return indexBuilder;
    }
    
    private string BuildIndexName(string[] propertyNames, bool isUnique)
    {
        var prefix = isUnique ? "UQ" : "IX";
        return $"{prefix}_{typeof(TEntity).Name}_{string.Join("_", propertyNames)}";
    }

    #endregion

    #region Table

    protected void ConfigureTable(string? tableName = null)
    {
        var name = tableName ?? EntityName.ToLower(); 
    
        Builder.ToTable(name);
    }

    #endregion

    #region Naming Helpers

    protected string BuildForeignKeyPropertyName(string navigationName)
        => $"{navigationName}Id";

    protected string BuildForeignKeyConstraintName(string navigationName)
        => $"FK_{EntityName}_{navigationName}";

    protected string BuildUniqueIndexName(IEnumerable<string> propertyNames)
        => $"UQ_{EntityName}_{string.Join("_", propertyNames)}";

    protected static string GetMemberName<TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression unaryMemberExpression)
            return unaryMemberExpression.Member.Name;

        throw new InvalidOperationException("Expression must be a member access expression.");
    }

    protected static List<string> GetPropertyNames(Expression<Func<TEntity, object>> expression)
    {
        var propertyAccessList = expression.GetPropertyAccessList();
        return propertyAccessList.Select(x => x.Name).ToList();
    }

    #endregion
}