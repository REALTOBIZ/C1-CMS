using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Composite.Types;
using Composite.Security;
using Composite.Serialization;


namespace Composite.Elements
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public sealed class ActionHandle
    {
        private ActionToken _actionToken;
        private string _serializedActionToken;



        public ActionHandle(ActionToken actionToken)
        {
            _actionToken = actionToken;
        }



        public ActionToken ActionToken
        {
            get { return _actionToken; }
        }



        private string SerializedActionToken
        {
            get
            {
                if (_serializedActionToken == null)
                {
                    _serializedActionToken = _actionToken.Serialize();
                }

                return _serializedActionToken;
            }
        }



        public override bool Equals(object obj)
        {
            return Equals(obj as ActionHandle);
        }



        public bool Equals(ActionHandle actionHandle)
        {
            if (actionHandle == null) return false;

            return this.SerializedActionToken == actionHandle.SerializedActionToken;
        }



        public override int GetHashCode()
        {
            return this.SerializedActionToken.GetHashCode();
        }
    }
}
