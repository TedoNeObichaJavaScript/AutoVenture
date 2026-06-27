namespace AutoVenture.Models
{
    /// <summary>View model for the responsive _CarImage partial.</summary>
    public class CarImageVm
    {
        public required string Src { get; init; }   // app-relative PNG, e.g. /carsphoto/mx5.png
        public string Alt { get; init; } = "";
        public string CssClass { get; init; } = "";
        public bool Eager { get; init; }            // true for above-the-fold (LCP) images
        public int Width { get; init; } = 640;
        public int Height { get; init; } = 360;
    }
}
