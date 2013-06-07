using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Microsoft.CSharp.RuntimeBinder;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// ASP.NET MVC Default Dictionary Binder
    /// </summary>
    public class DefaultDictionaryBinder : DefaultModelBinder
    {
        private readonly IModelBinder nextBinder;

        /// <summary>
        /// Create an instance of DefaultDictionaryBinder.
        /// </summary>
        public DefaultDictionaryBinder()
            : this(null)
        {
        }

        /// <summary>
        /// Create an instance of DefaultDictionaryBinder.
        /// </summary>
        /// <param name="nextBinder">The next model binder to chain call. If null, by default, the DefaultModelBinder is called.</param>
        public DefaultDictionaryBinder(IModelBinder nextBinder)
        {
            this.nextBinder = nextBinder;
        }

#if !ASPNETMVC1
        /// <summary>
        /// Get the keys of the value provider in the given controller context.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <returns>Returns a list of keys representing available values.</returns>
        private IEnumerable<string> GetValueProviderKeys(ControllerContext controllerContext)
        {
            IDictionary contextItems = HttpContext.Current.Items;

            // Do not reference this key elsewhere, this is strictly for cache.
            if (!contextItems.Contains("ValueProviderKeys"))
            {
                List<string> keys = new List<string>();
                keys.AddRange(controllerContext.HttpContext.Request.Form.Keys.Cast<string>());
                keys.AddRange(((IDictionary<string, object>)controllerContext.RouteData.Values).Keys);
                keys.AddRange(controllerContext.HttpContext.Request.QueryString.Keys.Cast<string>());
                keys.AddRange(controllerContext.HttpContext.Request.Files.Keys.Cast<string>());
                contextItems["ValueProviderKeys"] = keys;
            }

            return (IEnumerable<string>)contextItems["ValueProviderKeys"];
        }
#else
    /// <summary>
    /// Get the keys of the value provider in the given controller context.
    /// </summary>
    /// <param name="bindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
    /// <returns>Returns a list of keys representing available values.</returns>
    private IEnumerable<string> GetValueProviderKeys(ModelBindingContext bindingContext)
    {
        return bindingContext.ValueProvider.Keys;
    }
#endif

        /// <summary>
        /// Converts the given string to the destination type using TypeDescriptor class.
        /// </summary>
        /// <param name="stringValue">String representing the value of the given type.</param>
        /// <param name="type">The type of data represented as a string.</param>
        /// <returns>Returns the object of the given type, containing the value represented by the given string value.</returns>
        protected virtual object ConvertType(string stringValue, Type type)
        {
            return TypeDescriptor.GetConverter(type).ConvertFrom(stringValue);
        }

        class MetaData
        {
            public MetaData()
            {
            }

            public MetaData(Type dictType, Type listType, List<Type[]> listDictionaryGenericArgs, bool toArray)
            {
                this.dictType = dictType;
                this.listType = listType;
                this.listDictionaryGenericArgs = listDictionaryGenericArgs;
                this.toArray = toArray;
            }

            // This does not need thread synchronization... if value is wrong, the code just runs slower
            public bool typeConverterThrewNotSupportedException;
            public readonly Type dictType;
            public readonly Type listType;
            public readonly List<Type[]> listDictionaryGenericArgs;
            public readonly bool toArray;

            // must lock this before using
            public readonly HashSet<TypePair> dictMethodCallThrewException = new HashSet<TypePair>(TypePairEqualityComparer.instance);
        }

        struct TypePair
        {
            public TypePair(Type a, Type b)
            {
                this.a = a;
                this.b = b;
            }

            public Type a;
            public Type b;
        }

        class TypePairEqualityComparer : IEqualityComparer<TypePair>
        {
            public static readonly TypePairEqualityComparer instance = new TypePairEqualityComparer();

            public bool Equals(TypePair x, TypePair y)
            {
                return x.a == y.a && x.b == y.b;
            }

            public int GetHashCode(TypePair obj)
            {
                return obj.a.GetHashCode() ^ obj.b.GetHashCode();
            }
        }

        private static readonly ConcurrentDictionary<Type, MetaData> modelTypeToMetadata = new ConcurrentDictionary<Type, MetaData>();
        private static readonly ConcurrentDictionary<Type, MetaData> keyTypeToMetadata = new ConcurrentDictionary<Type, MetaData>();

        /// <summary>
        /// Binds the model by using the specified controller context and binding context.
        /// </summary>
        /// <returns> The bound object. </returns>
        /// <param name="controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <param name="bindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="bindingContext "/>parameter is null.</exception>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Type modelType = bindingContext.ModelType;
            MetaData metaData = modelTypeToMetadata.GetOrAdd(modelType, CreateMetaData);

            // skipping collections containing a Index property
            bool isOldCollection = metaData.listType != null
                && bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Index") != null;

            if (metaData.dictType != null && !isOldCollection)
            {
                dynamic result = null;
                HashSet<string> dictionaryKeys = new HashSet<string>();

                foreach (var genericArgs in metaData.listDictionaryGenericArgs)
                {
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    TypePair typePair = new TypePair(keyType, valueType);
                    IModelBinder valueBinder = this.Binders.GetBinder(valueType);
                    MetaData keyTypeMetaData = keyTypeToMetadata.GetOrAdd(keyType, t => new MetaData());

#if !ASPNETMVC1
                    foreach (string key in this.GetValueProviderKeys(controllerContext))
#else
                foreach (string key in this.GetValueProviderKeys(bindingContext))
#endif
                    {
                        if (key.StartsWith(bindingContext.ModelName + "[", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int endbracket = key.IndexOf("]", bindingContext.ModelName.Length + 1, StringComparison.Ordinal);
                            if (endbracket == -1)
                                continue;

                            // if type conversion ever throws a NotSupportedException, it will never be done again
                            if (keyTypeMetaData.typeConverterThrewNotSupportedException)
                                continue;

                            lock (metaData.dictMethodCallThrewException)
                                if (metaData.dictMethodCallThrewException.Contains(typePair))
                                    continue;

                            dynamic dictKey;
                            string dictKeyStr = key.Substring(
                                bindingContext.ModelName.Length + 1, endbracket - bindingContext.ModelName.Length - 1);

                            try
                            {
                                dictKey = this.ConvertType(dictKeyStr, keyType);
                            }
                            catch (NotSupportedException)
                            {
                                keyTypeMetaData.typeConverterThrewNotSupportedException = true;
                                continue;
                            }

                            if (dictionaryKeys.Contains(dictKeyStr))
                            {
                                continue;
                            }

                            dictionaryKeys.Add(dictKeyStr);

                            ModelBindingContext innerBindingContext = new ModelBindingContext()
                                {
#if ASPNETMVC1
                            Model = null,
                            ModelType = valueType,
#else
                                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, valueType),
#endif
                                    ModelName = key.Substring(0, endbracket + 1),
                                    ModelState = bindingContext.ModelState,
                                    PropertyFilter = bindingContext.PropertyFilter,
                                    ValueProvider = bindingContext.ValueProvider
                                };
                            dynamic newPropertyValue = valueBinder.BindModel(controllerContext, innerBindingContext);

                            result = this.CreateModel(controllerContext, bindingContext, metaData.dictType);

                            try
                            {
                                if ((bool)result.ContainsKey(dictKey))
                                    result[dictKey] = newPropertyValue;
                                else
                                    result.Add(dictKey, newPropertyValue);
                            }
                            catch (RuntimeBinderException)
                            {
                                lock (metaData.dictMethodCallThrewException)
                                    metaData.dictMethodCallThrewException.Add(typePair);
                            }
                        }
                    }
                }

                if (result == null)
                    return null;

                if (metaData.listType != null)
                {
                    // Here is where we convert back to a list.
                    IEnumerable collectionResult = (IEnumerable)result.Values;
                    dynamic listObject = Activator.CreateInstance(metaData.listType);
                    foreach (dynamic item in collectionResult)
                        listObject.Add(item);

                    if (metaData.toArray)
                        return listObject.ToArray();

                    return listObject;
                }

                return result;
            }

            if (this.nextBinder != null)
            {
                return this.nextBinder.BindModel(controllerContext, bindingContext);
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        /// <summary>
        /// Creates metadata for a given model type.
        /// </summary>
        /// <param name="modelType">Model type to create metadata for.</param>
        /// <returns>Returns a metadata object containing information about the given model type.</returns>
        private static MetaData CreateMetaData(Type modelType)
        {
            Type dictType = null;
            Type listType = null;
            bool toArray = false;

            List<Type[]> listDictionaryGenericArgs = new List<Type[]>();

            // For collection classes, proceed as dictionary, then convert back to list.
            if (modelType.IsArray && modelType.GetArrayRank() == 1)
            {
                Type itemType = modelType.GetElementType();
                listType = typeof(List<>).MakeGenericType(itemType);
                Type[] genericArgs = new Type[] { typeof(int), itemType };
                listDictionaryGenericArgs.Add(genericArgs);
                dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                toArray = true;
            }
            else if (modelType.IsGenericType)
            {
                TypeFilter typeFilter = (t, fc) => t.IsGenericType && t.GetGenericTypeDefinition() == (Type)fc;
                Type genericTypeDefinition = modelType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IDictionary<,>) || genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var genericArgs = modelType.GetGenericArguments();
                    listDictionaryGenericArgs.Add(genericArgs);
                    dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                }
                else if (genericTypeDefinition == typeof(IEnumerable<>)
                    || genericTypeDefinition == typeof(ICollection<>)
                    || genericTypeDefinition == typeof(IList<>)
                    || genericTypeDefinition == typeof(List<>))
                {
                    Type itemType = modelType.GetGenericArguments()[0];
                    listType = typeof(List<>).MakeGenericType(itemType);
                    Type[] genericArgs = new Type[] { typeof(int), itemType };
                    listDictionaryGenericArgs.Add(genericArgs);
                    dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                }
                else if (modelType.IsClass && !modelType.IsAbstract && !modelType.IsGenericTypeDefinition)
                {
                    Type[] implementedDictionaries = modelType.FindInterfaces(typeFilter, typeof(IDictionary<,>));
                    if (implementedDictionaries.Length > 0)
                    {
                        dictType = modelType;
                        listDictionaryGenericArgs.AddRange(
                            implementedDictionaries.Select(id => id.GetGenericArguments()));
                    }
                    else
                    {
                        Type[] implementedCollections = modelType.FindInterfaces(typeFilter, typeof(ICollection<>));
                        if (implementedCollections.Length > 0)
                        {
                            listType = modelType;
                            dictType = typeof(Dictionary<int, object>);
                            listDictionaryGenericArgs.AddRange(
                                implementedCollections.Select(ic => new Type[] { typeof(int), ic.GetGenericArguments()[0] }));
                        }
                    }
                }
            }

            if (listDictionaryGenericArgs.Count == 0)
                listDictionaryGenericArgs = null;

            MetaData result = new MetaData(dictType, listType, listDictionaryGenericArgs, toArray);

            return result;
        }
    }
}
