namespace MvcLayer.Models
{
    public class ModalViewModel
    {
        public string message { get; set; }
        public string header { get; set; }
        public string textButton { get; set; }

        public ModalViewModel()
        {

        }

        public ModalViewModel(string message, string header, string textButton)
        {
            this.message = message;
            this.header = header;
            this.textButton = textButton;
        }
    }
}
