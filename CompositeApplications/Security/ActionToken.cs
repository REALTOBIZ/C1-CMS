using System.Collections.Generic;
using System;
using Composite.Types;
using System.Linq;


namespace Composite.Security
{
    public abstract class ActionToken
    {
        public abstract IEnumerable<PermissionType> PermissionTypes { get; }
        public virtual string Serialize() { return ""; }
        public virtual bool IgnoreEntityTokenLocking { get { return false; } }
    }



    internal static class ActionTokenExtensionMethods
    {
        private static Dictionary<Type, bool> _ignoreEntityTokenLockingCache = new Dictionary<Type, bool>();
        private static readonly object _lock = new object();

        public static bool IsIgnoreEntityTokenLocking(this ActionToken actionToken)
        {
            if (actionToken == null) throw new ArgumentNullException("actionToken");
            
            Type type = actionToken.GetType();

            lock (_lock)
            {
                bool ignoreLocking;
                if (_ignoreEntityTokenLockingCache.TryGetValue(type, out ignoreLocking) == false)
                {
                    ignoreLocking = type.GetCustomAttributesRecursively<IgnoreEntityTokenLocking>().Any() == true;

                    _ignoreEntityTokenLockingCache.Add(type, ignoreLocking);
                }

                return ignoreLocking | actionToken.IgnoreEntityTokenLocking;
            }
        }
    }
}
