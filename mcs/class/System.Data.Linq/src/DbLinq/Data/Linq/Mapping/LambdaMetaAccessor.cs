using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Reflection.Emit;

namespace DbLinq.Data.Linq.Mapping
{
    static class LambdaMetaAccessor
    {
        //This will go away with C# 4.0 ActionExpression
        static Delegate MakeSetter(MemberInfo member, Type memberType, Type declaringType)
        {
            Type delegateType = typeof(Action<,>).MakeGenericType(declaringType, memberType);

            var x = Expression.Parameter(declaringType, "x");
            var v = Expression.Parameter(memberType, "v");

            if (member is PropertyInfo)
            {
                if (((PropertyInfo)member).GetSetMethod() != null)
                    return Expression.Lambda(Expression.Block(Expression.Assign(Expression.Property(x, (PropertyInfo)member), v), Expression.Empty()), x, v).CompileDebuggable();
                var ca = member.GetCustomAttribute<ColumnAttribute>();
                member = declaringType.GetRuntimeField(ca.Storage);
            }

            if (member is FieldInfo)
            {
                return Expression.Lambda(Expression.Block(Expression.Assign(Expression.Field(x, (FieldInfo)member), v), Expression.Empty()), x, v).CompileDebuggable();
            }

            if (member is MethodInfo)
            {
                return Expression.Lambda(Expression.Block(Expression.Call(x, (MethodInfo)member, v), Expression.Empty()), x, v).CompileDebuggable();
            }

            throw new InvalidOperationException();

        }

        public static MetaAccessor Create(MemberInfo member, Type declaringType)
        {
            var memberType =
                (member as PropertyInfo)?.PropertyType ??
                (member as FieldInfo)?.FieldType ??
                (member as MethodInfo)?.ReturnParameter.ParameterType;

            Type accessorType = typeof(LambdaMetaAccessor<,>).MakeGenericType(declaringType, memberType);

            ParameterExpression p = Expression.Parameter(declaringType, "e");
            return (MetaAccessor)Activator.CreateInstance(accessorType, new object[]{
                Expression.Lambda(Expression.MakeMemberAccess(p, member), p).CompileDebuggable(),
                MakeSetter(member, memberType, declaringType) }
            );
        }

    }

    class LambdaMetaAccessor<TEntity, TMember> : MetaAccessor<TEntity, TMember>
    {
        Func<TEntity, TMember> _Accessor;
        Action<TEntity, TMember> _Setter;

        public LambdaMetaAccessor(Func<TEntity, TMember> accessor, Action<TEntity, TMember> setter)
        {
            _Accessor = accessor;
            _Setter = setter;
        }

        //
        // Summary:
        //     Specifies the strongly typed value.
        //
        // Parameters:
        //   instance:
        //     The instance from which to get the value.
        public override TMember GetValue(TEntity instance)
        {
            return _Accessor(instance);
        }

        //
        // Summary:
        //     Specifies an instance on which to set the strongly typed value.
        //
        // Parameters:
        //   instance:
        //     The instance into which to set the value.
        //
        //   value:
        //     The strongly typed value to set.
        public override void SetValue(ref TEntity instance, TMember value)
        {
            _Setter(instance, value);
        }
    }
}
