namespace vendtechext.Contracts
{
    public class IntegratorInProcessInformation
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public IntegratorInProcessInformation CopyData(string id, string name)
        {
            ID = id;
            Name = name;
            return this;
        }
    }
}
