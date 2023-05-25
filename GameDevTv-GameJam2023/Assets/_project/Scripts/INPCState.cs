namespace MB6
{
    public interface INPCState
    {
        public void OnEnter();
        public void Tick();
        public void OnExit();
    }
}