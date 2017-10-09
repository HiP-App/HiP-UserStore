namespace PaderbornUniversity.SILab.Hip.UserStore.Model.Entity
{
    public interface IEntity<TKey>
    {
        TKey Id { get; }
    }
}
