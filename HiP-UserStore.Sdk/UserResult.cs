namespace PaderbornUniversity.SILab.Hip.UserStore
{
    public partial class UserResult
    {
        public string FullName => (FirstName == null && LastName == null) ? null : FirstName + ' ' + LastName;
    }
}
