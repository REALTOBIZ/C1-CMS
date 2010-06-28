using Composite.Security;


namespace Composite.StandardPlugins.Elements.ElementProviders.UserElementProvider
{
    [SecurityAncestorProvider(typeof(UserElementProviderGroupEntityTokenSecurityAncestorProvider))]
    internal sealed class UserElementProviderGroupEntityToken : EntityToken
	{
        private string _id;

        public UserElementProviderGroupEntityToken(string id)
        {
            _id = id;
        }

        public override string Type
        {
            get { return ""; }
        }

        public override string Source
        {
            get { return ""; }
        }

        public override string Id
        {
            get { return _id; }
        }

        public override string Serialize()
        {
            return _id;
        }

        public static EntityToken Deserialize(string serializedData)
        {
            return new UserElementProviderGroupEntityToken(serializedData);
        }
    }
}
