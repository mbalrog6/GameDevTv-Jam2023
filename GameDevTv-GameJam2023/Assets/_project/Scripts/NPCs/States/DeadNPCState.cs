namespace MB6.NPCs.States
{
    public class DeadNPCState : INPCState
    {
        private NPCController _npcController;

        public DeadNPCState(NPCController npcController)
        {
            _npcController = npcController;
        }
        public void OnEnter()
        {
            _npcController.MaxSpeed = 0;
            _npcController.Move();
        }

        public void Tick()
        {
            
        }

        public void OnExit()
        {
            
        }
    }
}