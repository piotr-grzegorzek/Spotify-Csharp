namespace Studio.Logic {
    public class User {
        public string Name { get; set; }
        public int Permission { get; set; }
        public int Money { get; set; }
        public User(string name, int permission, int money) {
            Name = name;
            Permission = permission;
            Money = money;
        }
    }
}
