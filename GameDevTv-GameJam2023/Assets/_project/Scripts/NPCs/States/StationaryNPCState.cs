namespace MB6
{
    public class StationaryNPCState : INPCState
    {
        private NPCController _npcController;

        public StationaryNPCState(NPCController npcController)
        {
            _npcController = npcController;
        }
        public void OnEnter()
        {
            _npcController.MaxSpeed = 0;
        }

        public void Tick()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
}