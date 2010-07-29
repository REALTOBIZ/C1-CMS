﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Composite.Collections.Generic;
using Composite.Data.Caching;
using Composite.Data.ProcessControlled;
using Composite.Data.Types;
using Composite.Logging;
using Composite.Data.DynamicTypes;
using Composite.StringExtensions;
using Composite.Types;
using System.Reflection;


namespace Composite.Data
{
    internal class DataLocalizationFacadeImpl : IDataLocalizationFacade
    {
        private static readonly string LogTitle = "LocalizationFacade";

        private static readonly Cache<string, CultureInfo> _urlMappingCache = new Cache<string, CultureInfo>("UrlCultureMapping", 70);
        private static readonly Cache<string, string> _cultureUrlCache = new Cache<string, string>("CultureUrlMapping", 70);
        private static ReadOnlyList<string> _urlMappings;
        private static ReadOnlyList<string> _activeCultureNames;
        private static ExtendedNullable<CultureInfo> _defaultCulture;
        private static ExtendedNullable<CultureInfo> _defaultUrlMappingCulture;
        private static readonly object _syncRoot = new object();

        static DataLocalizationFacadeImpl()
        {
            DataEventSystemFacade.SubscribeToDataAfterUpdate<ISystemActiveLocale>(OnSystemActiveLocaleChanged, true);
            DataEventSystemFacade.SubscribeToDataAfterAdd<ISystemActiveLocale>(OnSystemActiveLocaleChanged, true);
            DataEventSystemFacade.SubscribeToDataDeleted<ISystemActiveLocale>(OnSystemActiveLocaleChanged, true);
        }

        private static void OnSystemActiveLocaleChanged(DataEventArgs dataEventArgs)
        {
            var locale = dataEventArgs.Data as ISystemActiveLocale;
            
            if (locale != null)
            {
                _urlMappingCache.Remove(locale.UrlMappingName);
                _cultureUrlCache.Remove(locale.CultureName);

                lock (_syncRoot)
                {
                    _urlMappings = null;
                    _activeCultureNames = null;
                    _defaultUrlMappingCulture = null;
                    _defaultCulture = null;
                }
            }
        }

        public bool UseLocalization
        {
            get
            {
                return DataFacade.GetData<ISystemActiveLocale>().Count() > 1;
            }
        }



        public IEnumerable<CultureInfo> WhiteListedLocales
        {
            get
            {
                IEnumerable<string> whiteListedLocaleNames =
                    (from d in DataFacade.GetData<IWhiteListedLocale>()
                     select d.CultureName).ToList();

                foreach (string whiteListedLocaleName in whiteListedLocaleNames)
                {
                    yield return CultureInfo.CreateSpecificCulture(whiteListedLocaleName);
                }
            }
        }


        public CultureInfo DefaultUrlMappingCulture
        {
            get
            {
                ExtendedNullable<CultureInfo> result = _defaultUrlMappingCulture;

                if (result == null)
                {
                    lock (_syncRoot)
                    {
                        if (_defaultUrlMappingCulture == null)
                        {
                            ISystemActiveLocale systemActiveLocale =
                                (from data in DataFacade.GetData<ISystemActiveLocale>()
                                 where data.UrlMappingName == ""
                                 select data).FirstOrDefault();

                            CultureInfo cultureInfo = systemActiveLocale == null 
                                ? null 
                                : CultureInfo.CreateSpecificCulture(systemActiveLocale.CultureName);

                            _defaultUrlMappingCulture = new ExtendedNullable<CultureInfo> {Value = cultureInfo};
                        }
                        result = _defaultUrlMappingCulture;
                    }
                }
                return result.Value;
            }
        }



        public CultureInfo DefaultLocalizationCulture
        {
            get
            {
                ExtendedNullable<CultureInfo> result = _defaultCulture;

                if (result == null)
                {
                    lock (_syncRoot)
                    {
                        if (_defaultCulture == null)
                        {
                            List<string> culturesMarkedAsDefault =
                                (from data in DataFacade.GetData<ISystemActiveLocale>()
                                 where data.IsDefault
                                 select data.CultureName).ToList();

                            CultureInfo cultureInfo = null;

                            if (culturesMarkedAsDefault.Count > 0)
                            {
                                if (culturesMarkedAsDefault.Count > 1)
                                {
                                    LoggingService.LogWarning(LogTitle, "There's more than one culture marked as 'default'");
                                }

                                cultureInfo = CultureInfo.CreateSpecificCulture(culturesMarkedAsDefault[0]);
                            }

                            _defaultCulture = new ExtendedNullable<CultureInfo> { Value = cultureInfo };
                        }
                        result = _defaultCulture;
                    }
                }
                return result.Value;
            }
            set
            {
                Verify.ArgumentNotNull(value, "value");

                string cultureName = value.Name;

                lock (_syncRoot)
                {
                    using(var transactionScope = Transactions.TransactionsFacade.CreateNewScope())
                    {
                        List<ISystemActiveLocale> systemLocalesMarkedAsDefault =
                        (from data in DataFacade.GetData<ISystemActiveLocale>()
                         where data.IsDefault
                         select data).ToList();

                        bool alreadyMarkedAsDefault = false;
                        foreach(ISystemActiveLocale locale in systemLocalesMarkedAsDefault)
                        {
                            if(locale.CultureName == cultureName)
                            {
                                alreadyMarkedAsDefault = true;
                                continue;
                            }

                            locale.IsDefault = false;
                            DataFacade.Update(locale);
                        }

                        if(!alreadyMarkedAsDefault)
                        {
                            ISystemActiveLocale newDefaultCulture =
                            (from data in DataFacade.GetData<ISystemActiveLocale>()
                             where data.CultureName == cultureName
                             select data).FirstOrDefault();

                            if (newDefaultCulture == null)
                            {
                                LoggingService.LogError(LogTitle, "Failed to get a locale by culture name '{0}'".FormatWith(cultureName));
                                return;
                            }

                            newDefaultCulture.IsDefault = true;
                            DataFacade.Update(newDefaultCulture);
                        }
                        
                        transactionScope.Complete();
                    }
                }

                LoggingService.LogVerbose(LogTitle, "Default localization culture changed to '{0}'".FormatWith(cultureName));
            }
        }

        public IEnumerable<string> ActiveLocalizationNames
        {
            get
            {
                ReadOnlyList<string> _result = _activeCultureNames;

                if (_result == null)
                {
                    lock (_syncRoot)
                    {
                        if (_activeCultureNames == null)
                        {
                            _activeCultureNames = new ReadOnlyList<string>(
                                (from d in DataFacade.GetData<ISystemActiveLocale>()
                                 select d.CultureName).ToList());
                        }
                        _result = _activeCultureNames;

                    }
                }
                return _result;
            }
        }



        public string GetUrlMappingName(CultureInfo cultureInfo)
        {
            string urlMappingName = _cultureUrlCache.Get(cultureInfo.Name);

            if (urlMappingName == null)
            {
                urlMappingName =
                (from sal in DataFacade.GetData<ISystemActiveLocale>()
                 where sal.CultureName == cultureInfo.Name
                 select sal.UrlMappingName).SingleOrDefault();

                _cultureUrlCache.Add(cultureInfo.Name, urlMappingName);
            }

            return urlMappingName;
        }



        public CultureInfo GetCultureInfoByUrlMappingName(string urlMappingName)
        {
            CultureInfo cultureInfo = _urlMappingCache.Get(urlMappingName);

            if (cultureInfo == null)
            {
                string cultureName = (from sal in DataFacade.GetData<ISystemActiveLocale>()
                                      where sal.UrlMappingName.ToLower() == urlMappingName.ToLower()
                                      select sal.CultureName).Single();

                cultureInfo = CultureInfo.CreateSpecificCulture(cultureName);

                _urlMappingCache.Add(urlMappingName, cultureInfo);
            }

            return cultureInfo;
        }



        public IEnumerable<string> UrlMappingNames
        {
            get
            {
                ReadOnlyList<string> _result = _urlMappings;

                if (_result == null)
                {
                    lock (_syncRoot)
                    {
                        if (_urlMappings == null)
                        {
                            List<string> mappings = (from sal in DataFacade.GetData<ISystemActiveLocale>()
                                                     select sal.UrlMappingName).ToList();

                            // Adding lower cased values
                            for(int i=0; i<mappings.Count; i++)
                            {
                                string loweredValue = mappings[i].ToLower();
                                if (mappings[i] != loweredValue)
                                {
                                    mappings.Add(mappings[i].ToLower());
                                }
                            }

                            _urlMappings = new ReadOnlyList<string>(mappings);
                        }
                        _result = _urlMappings;
                    }
                }
                return _result;
            }
        }



        public bool IsLocalized(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (type == typeof(IPage)) return true;
            if (type == typeof(IPagePlaceholderContent)) return true;

            if (type.IsGenerated() == true)
            {
                if (PageMetaDataFacade.GetAllMetaDataTypes().Contains(type) == true)
                {
                    return true;
                }
            }

            return typeof(ILocalizedControlled).IsAssignableFrom(type);
        }



        public bool IsLocalizable(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (type == typeof(IPage)) return true;
            if (type == typeof(IPagePlaceholderContent)) return true;
            if (type.IsGenerated() == true) return true;

            return typeof(ILocalizedControlled).IsAssignableFrom(type);
        }



        public IEnumerable<ReferenceFailingPropertyInfo> GetReferencingLocalizeFailingProperties(ILocalizedControlled data)
        {
            DataTypeDescriptor dataTypeDescriptor = DynamicTypeManager.GetDataTypeDescriptor(data.DataSourceId.InterfaceType);

            IEnumerable<DataFieldDescriptor> requiredDataFieldDescriptors = dataTypeDescriptor.Fields.Where(f => f.ForeignKeyReferenceTypeName != null);

            foreach (DataFieldDescriptor dataFieldDescriptor in requiredDataFieldDescriptors)
            {
                Type referencedType = TypeManager.GetType(dataFieldDescriptor.ForeignKeyReferenceTypeName);
                if (DataLocalizationFacade.IsLocalized(referencedType) == false) continue; // No speciel handling for non localized datas.

                IData referencedData = data.GetReferenced(dataFieldDescriptor.Name);
                if (referencedData != null) continue; // Data has already been localized               


                bool optionalReferenceWithValue = false;
                if (dataFieldDescriptor.IsNullable == true)
                {
                    PropertyInfo propertyInfo = data.DataSourceId.InterfaceType.GetPropertiesRecursively().Where(f => f.Name == dataFieldDescriptor.Name).Single();
                    object value = propertyInfo.GetValue(data, null);

                    if (value == null) continue; // Optional reference is null;
                    if (object.Equals(value, dataFieldDescriptor.DefaultValue) == true) continue; // Optional reference is null;

                    optionalReferenceWithValue = true;
                }

                using (new DataScope(CultureInfo.CreateSpecificCulture(data.CultureName)))
                {
                    referencedData = data.GetReferenced(dataFieldDescriptor.Name);
                }

                ReferenceFailingPropertyInfo referenceFailingPropertyInfo = new ReferenceFailingPropertyInfo
                (
                    dataFieldDescriptor,
                    referencedType,
                    referencedData,
                    optionalReferenceWithValue
                );

                yield return referenceFailingPropertyInfo;
            }
        }     
    }



    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public sealed class ReferenceFailingPropertyInfo
    {
        public ReferenceFailingPropertyInfo(DataFieldDescriptor dataFieldDescriptor, Type referencedType, IData originLocaleDataValue, bool optionalReferenceWithValue)
        {
            this.DataFieldDescriptor = dataFieldDescriptor;
            this.ReferencedType = referencedType;
            this.OriginLocaleDataValue = originLocaleDataValue;
            this.OptionalReferenceWithValue = optionalReferenceWithValue;
        }

        /// <summary>
        /// DataFieldDescriptor of the property that are the reference
        /// </summary>
        public DataFieldDescriptor DataFieldDescriptor { get; private set; }

        /// <summary>
        /// Data type that are refernced
        /// </summary>
        public Type ReferencedType { get; private set; }

        /// <summary>
        /// This holds the value of the reference in the same locale
        /// as the IData value given to the call to
        /// GetReferencingLocalizeFailingProperties
        /// This may be null in case of optional references
        /// </summary>
        public IData OriginLocaleDataValue { get; private set; }

        /// <summary>
        /// This is true if the reference is optional and 
        /// if the referenced item is existing in the old locale.
        /// </summary>
        public bool OptionalReferenceWithValue { get; private set; }
    }
}
