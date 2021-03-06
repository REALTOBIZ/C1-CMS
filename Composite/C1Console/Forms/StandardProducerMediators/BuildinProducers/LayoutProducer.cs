namespace Composite.C1Console.Forms.StandardProducerMediators.BuildinProducers
{
    [ControlValueProperty("UiControl")]
    internal sealed class LayoutProducer : IBuildinProducer
    {
        private IUiControl _uiControl;

        internal LayoutProducer() { }

        public IUiControl UiControl
        {
            get { return _uiControl; }
            set { _uiControl = value; }
        }

        public string label { get; set; }

        public string iconhandle { get; set; }
    }
}
