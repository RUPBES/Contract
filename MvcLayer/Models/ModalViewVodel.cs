namespace MvcLayer.Models
{
    public class ModalViewVodel
    {
        public string message { get; set; }
        public string header { get; set; }
        public string textButton { get; set; }

        public ModalViewVodel()
        {

        }

        public ModalViewVodel(string message, string header, string textButton)
        {
            this.message = message;
            this.header = header;
            this.textButton = textButton;
        }
    }
}
