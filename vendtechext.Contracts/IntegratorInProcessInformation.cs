namespace vendtechext.Contracts
{
    public class IntegratorInProcessInformation
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public IntegratorInProcessInformation CopyData(Guid id, string name)
        {
            ID = id;
            Name = name;
            return this;
        }
    }
}
