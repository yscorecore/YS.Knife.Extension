using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TableValueFunctionAttribute : Attribute, IModelMethodAttribute
    {
        public TableValueFunctionAttribute() { }
        public TableValueFunctionAttribute(string name, string schema = null)
        {
            this.Name = name;
            this.Schema = schema;
        }
        public string Name { get; set; }
        public string Schema { get; set; }
        public void Apply(ModelBuilder modelBuilder, MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                var entityType = returnType.GetGenericArguments()[0];
                modelBuilder.Entity(entityType);

                var functionBuilder = modelBuilder
                        .HasDbFunction(methodInfo)
                        .HasName(string.IsNullOrEmpty(Name) ? methodInfo.Name : this.Name);
                if (!string.IsNullOrEmpty(this.Schema))
                {
                    functionBuilder.HasSchema(this.Schema);
                }

            }
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NoKeyAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ViewAttribute : Attribute, IModelTypeAttribute
    {
        public ViewAttribute(string name, string schema = null)
        {
            this.Name = name;
            this.Schema = schema;
        }
        public string Name { get; set; }
        public string Schema { get; set; }

        public void Apply(EntityTypeBuilder typeBuilder)
        {
            var clrType = typeBuilder.Metadata.ClrType;
            typeBuilder.ToView(this.Name, this.Schema);
            if (IsDefined(clrType, typeof(NoKeyAttribute)))
            {
                typeBuilder.HasNoKey();
            }
        }
    }
}
