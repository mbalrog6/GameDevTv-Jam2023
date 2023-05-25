namespace MB6
{
    public interface IHealth
    {
        public int Health { get; }
        public int MaxHealth { get; set; }
        public void TakeDamage(int amount);
        public void Heal(int amount);
        public float NormalizedHealth { get; }
    }
}