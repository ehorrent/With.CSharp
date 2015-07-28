﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace With.ConstructorProvider
{
    /// <summary>
    /// Constructor provider, using expression trees to create compiled constructors
    /// </summary>
    public class ExpressionConstructorProvider : IConstructorProvider
    {
        /// <summary>
        /// Provides a constructor, based on the given signature
        /// </summary>
        /// <typeparam name="T">Type of the instance to be created by the constructor</typeparam>
        /// <param name="constructorSignature">Constructor's signature</param>
        /// <returns>Corresponding constructor (if existing)</returns>
        public Func<object[], T> GetConstructor<T>(Type[] constructorSignature) where T : class
        {
            // Find constructor with matching argument types
            var ctorInfo = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                CallingConventions.HasThis,
                constructorSignature.ToArray(),
                new ParameterModifier[0]);

            // Get arguments
            var argsExpr = Expression.Parameter(typeof(object[]), "arguments");

            // Get constructor parameters values
            var ctorParameters = ctorInfo.GetParameters();
            var ctorParametersExpr = ctorParameters.Select((param, index) =>
            {
                var arrayAccessExpr = Expression.ArrayAccess(
                    argsExpr,
                    Expression.Constant(index, typeof(int)));

                return Expression.Convert(arrayAccessExpr, param.ParameterType);
            }).ToArray();

            var ctorExpr = Expression.New(ctorInfo, ctorParametersExpr);

            var creatorExpr = Expression.Lambda<Func<object[], T>>(
                ctorExpr,
                argsExpr);

            return creatorExpr.Compile();
        }
    }
}