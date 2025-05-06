namespace Si.AccessControl.Options
{
    public interface ISession
    {
        public object Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Phones { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
