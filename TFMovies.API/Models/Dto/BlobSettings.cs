namespace TFMovies.API.Models.Dto
{
    public class BlobSettings
    {
        public string ConnectionString { get; set; }
        public string Container { get; set; }
        public ImageSettings ImageSettings { get; set; }
    }
    public class ImageSettings
    {
        public double MaxSizeMb { get; set; }
        public List<string> AllowedExtensions { get; set; }
    }
}