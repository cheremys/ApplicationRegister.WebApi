namespace ApplicationRegister.WebApi.Interfaces
{
    internal interface IWorker
    {
        public string SendMessage(string message);
        void Close();
    }
}
