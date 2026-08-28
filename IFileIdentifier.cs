namespace BeeQ;

public partial class FileStorage
{
    public interface IFileIdentifier
    {
        public Guid? Id { get; }
        public string? Key { get; }
    }

    /// <summary>
    /// Partial Resource Identifier
    /// </summary>
    internal class FileIdentifier : IFileIdentifier
    {
        public Guid? Id { get; set; }
        public string? Key { get; set; }

        public FileIdentifier(Guid id)
        {
            this.Id = id;
        }
        public FileIdentifier(string key)
        {
            this.Key = key;
        }
    }
}