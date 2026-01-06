namespace localshopyNew.ViewModel
{
    public class ReviewViewModel
    {
        public Guid ProductId { get; set; }   // Hidden input
        public int Rating { get; set; }      // Hidden input set via JS
        public string? Comment { get; set; }  // Textarea
    }

}
