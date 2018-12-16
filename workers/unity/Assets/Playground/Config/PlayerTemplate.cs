using System.Collections.Generic;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.QueryBasedInterest;
using Improbable.Gdk.TransformSynchronization;

namespace Playground
{
    public static class PlayerTemplate
    {
        public static EntityTemplate CreatePlayerEntityTemplate(string workerId, Improbable.Vector3f position)
        {
            var clientAttribute = $"workerId:{workerId}";

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(), clientAttribute);
            template.AddComponent(new Metadata.Snapshot { EntityType = "Character" }, WorkerUtils.UnityGameLogic);
            template.AddComponent(new PlayerInput.Snapshot(), clientAttribute);
            template.AddComponent(new Launcher.Snapshot { EnergyLeft = 100, RechargeTimeLeft = 0 },
                WorkerUtils.UnityGameLogic);
            template.AddComponent(new Score.Snapshot(), WorkerUtils.UnityGameLogic);
            template.AddComponent(new CubeSpawner.Snapshot { SpawnedCubes = new List<EntityId>() },
                WorkerUtils.UnityGameLogic);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, clientAttribute);
            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, workerId, clientAttribute,
                WorkerUtils.UnityGameLogic);

            template.SetReadAccess(WorkerUtils.UnityClient, WorkerUtils.UnityGameLogic, WorkerUtils.AndroidClient,
                WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            return template;
        }

        private static void MinimapExample()
        {
            var playerQuery = InterestQuery
                .Query(Constraint.All(
                    Constraint.RelativeSphere(20),
                    Constraint.Component<PlayerInfo.Component>()))
                .Filter(Position.ComponentId, PlayerInfo.ComponentId);

            var miniMapQuery = InterestQuery
                .Query(Constraint.All(
                    Constraint.RelativeBox(50, double.PositiveInfinity, 50),
                    Constraint.Component<MinimapRepresentation.Component>()))
                .Filter(Position.ComponentId, MinimapRepresentation.ComponentId);

            var interest = InterestBuilder.Begin()
                .AddQueries<PlayerControls.Component>(playerQuery, miniMapQuery)
                .Build();
        }

        private static void TeamsExample()
        {
            var isblue = true;
            //some logic to determine which team

            var teamQuery = InterestQuery
                .Query(Constraint.Component(isblue ? BlueTeam.ComponentId : RedTeam.ComponentId));

            var interest = InterestBuilder.Begin()
                .AddQuery<PlayerControls.Component>(teamQuery)
                .Build();
        }

        private static void FrequencyExample()
        {
            var playerQuery = InterestQuery
                .Query(Constraint.All(
                    Constraint.RelativeSphere(20),
                    Constraint.Component<PlayerInfo.Component>()))
                .MaxFrequencyHz(20)
                .Filter(Position.ComponentId, PlayerInfo.ComponentId);

            var miniMapQuery = InterestQuery
                .Query(Constraint.All(
                    Constraint.RelativeBox(50, double.PositiveInfinity, 50),
                    Constraint.Component<MinimapRepresentation.Component>()))
                .MaxFrequencyHz(1)
                .Filter(Position.ComponentId, MinimapRepresentation.ComponentId);

            var interest = InterestBuilder.Begin()
                .AddQueries<PlayerControls.Component>(playerQuery, miniMapQuery)
                .Build();

            var template = new EntityTemplate();
            template.AddComponent(interest, "blah");
        }
    }
}
