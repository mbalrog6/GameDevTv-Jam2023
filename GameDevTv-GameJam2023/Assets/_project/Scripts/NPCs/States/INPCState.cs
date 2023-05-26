namespace MB6.NPCs.States
{
    public interface INPCState
    {
        public void OnEnter();
        public void Tick();
        public void OnExit();
    }
}