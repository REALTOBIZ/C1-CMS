﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using Composite.Serialization;
using Composite.Types;


namespace Composite.Security
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class ActionTokenSerializer
    {
        public static string Serialize(ActionToken actionToken)
        {
            return Serialize(actionToken, false);
        }
       


        public static string Serialize(ActionToken actionToken, bool includeHashValue)
        {
            StringBuilder sb = new StringBuilder();

            StringConversionServices.SerializeKeyValuePair(sb, "actionTokenType", TypeManager.SerializeType(actionToken.GetType()));

            string serializedActionToken = actionToken.Serialize();
            StringConversionServices.SerializeKeyValuePair(sb, "actionToken", serializedActionToken);

            if (includeHashValue == true)
            {
                StringConversionServices.SerializeKeyValuePair(sb, "actionTokenHash", HashSigner.GetSignedHash(serializedActionToken).Serialize());
            }

            return sb.ToString();
        }



        public static ActionToken Deserialize(string serialziedActionToken)
        {
            return Deserialize(serialziedActionToken, false);
        }



        public static ActionToken Deserialize(string serialziedActionToken, bool includeHashValue)
        {
            if (string.IsNullOrEmpty(serialziedActionToken) == true) throw new ArgumentNullException("serialziedActionToken");

            Dictionary<string, string> dic = StringConversionServices.ParseKeyValueCollection(serialziedActionToken);

            if ((dic.ContainsKey("actionTokenType") == false) ||
                (dic.ContainsKey("actionToken") == false) ||
                ((includeHashValue == true) && (dic.ContainsKey("actionTokenHash") == false)))
            {
                throw new ArgumentException("Failed to deserialize the value. It has to be serialized with ActionTokenSerializer class.", "serialziedActionToken");
            }

            string actionTokenTypeString = StringConversionServices.DeserializeValueString(dic["actionTokenType"]);
            string actionTokenString = StringConversionServices.DeserializeValueString(dic["actionToken"]);

            if (includeHashValue == true)
            {
                string actionTokenHash = StringConversionServices.DeserializeValueString(dic["actionTokenHash"]);

                HashValue hashValue = HashValue.Deserialize(actionTokenHash);
                if (HashSigner.ValidateSignedHash(actionTokenString, hashValue) == false)
                {
                    throw new SecurityException("Serialized action token is tampered");
                }
            }

            Type actionType = TypeManager.GetType(actionTokenTypeString);

            MethodInfo methodInfo = actionType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                throw new InvalidOperationException(string.Format("The action token {0} is missing a public static Deserialize method taking a string as parameter and returning an {1}", actionType, typeof(ActionToken)));
            }


            ActionToken actionToken;
            try
            {
                actionToken = (ActionToken)methodInfo.Invoke(null, new object[] { actionTokenString });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("The action token {0} is missing a public static Deserialize method taking a string as parameter and returning an {1}", actionType, typeof(ActionToken)), ex);
            }

            if (actionToken == null)
            {
                throw new InvalidOperationException(string.Format("public static Deserialize method taking a string as parameter and returning an {1} on the action token {0} did not return an object", actionType, typeof(ActionToken)));
            }

            return actionToken;
        }



        public static T Deserialize<T>(string serialziedActionToken)
            where T : ActionToken
        {
            return (T)Deserialize(serialziedActionToken);
        }
    }
}
