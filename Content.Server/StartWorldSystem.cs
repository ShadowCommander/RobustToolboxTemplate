using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server;

sealed public class StartWorldSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ActorSystem _actorSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    public bool GameStarted;

    public MapId GameMap;

    [ViewVariables]
    private TimeSpan _roundStartTime;

    public override void Initialize()
    {
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;

        _roundStartTime = _gameTiming.CurTime;
        GameStarted = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_roundStartTime < _gameTiming.CurTime)
        {
            StartRound();
        }
    }

    private void StartRound()
    {
        if (GameStarted)
            return;
        GameStarted = true;

        GameMap = _mapManager.CreateMap();
    }

    private void PlayerStatusChanged(object blah, SessionStatusEventArgs args)
    {
        if (args.NewStatus == SessionStatus.Connected)
        {
            StartRound();

            // Spawn player entity
            var playerEntity = EntityManager.SpawnEntity("Player", new EntityCoordinates(_mapManager.GetMapEntityId(GameMap), 0.5f, 0.5f));
            _metaDataSystem.SetEntityName(playerEntity, args.Session.ToString());

            // Attack player to entity and join game 
            _actorSystem.Attach(playerEntity, args.Session);
            args.Session.JoinGame();
        }
    }
}
