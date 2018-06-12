namespace ETModel
{
    public static class PlayerFactory
    {
        public static Player Create(long id)
        {
            Player player = ObjectFactory.CreateWithId<Player>(id);
            PlayerComponent playerComponent = Game.PlayerComponent;
            playerComponent.Add(player);
            return player;
        }
    }
}