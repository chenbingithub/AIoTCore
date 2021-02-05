namespace AIoT.Core
{
    /// <summary>
    /// Used to standardize soft deleting entities.
    /// Soft-delete entities are not actually deleted,
    /// marked as IsDeleted = true in the database,
    /// but can not be retrieved to the application normally.
    /// </summary>
    public interface IReadonly
    {

    }
}