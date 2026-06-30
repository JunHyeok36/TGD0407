using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace TDG0407.Systems.Generators
{
    using System.Linq;
    using Core.Grid;
    using Core.Utils;
    using Core.Value;
    using Domain.Archive;
    using Domain.Entities;
    using Domain.Map;
    using View.Map;

    /// <summary>
    /// 맵을 생성하는 시스템입니다.
    /// </summary>
    public static class MapGenerator
    {
        #region Methods

        public static async Task<WorldState> GenerateWorld(int? worldSeed = null, int? proceduralSeed = null)
        {
            worldSeed ??= SeedParser.NewIntSeed();
            proceduralSeed ??= SeedParser.NewIntSeed();

            System.Random worldRandom = new(worldSeed.Value);
            MapState mapState = await GenerateMap(worldRandom);
            return new WorldState(worldSeed.Value, proceduralSeed.Value, mapState, null, null);
        }

        private static async Task<MapState> GenerateMap(System.Random worldRandom)
        {
            Ref<int> nextLevelInstanceId = new(0);
            Ref<int> nextEntityInstanceId = new(0);

            List<LevelState> levelStates = new()
            {
                await GenerateLevel(
                    new RandomLevelGenerateParameter(
                        "Test", 
                        nextLevelInstanceId.Value++, 
                        SelectTestLevelScale(worldRandom), 
                        worldRandom, 
                        nextEntityInstanceId
                    )
                ),
                await GenerateLevel(
                    new RandomLevelGenerateParameter(
                        "Test", 
                        nextLevelInstanceId.Value++, 
                        SelectTestLevelScale(worldRandom), 
                        worldRandom, 
                        nextEntityInstanceId
                    )
                ),
                await GenerateLevel(
                    new RandomLevelGenerateParameter(
                        "Test", 
                        nextLevelInstanceId.Value++, 
                        SelectTestLevelScale(worldRandom), 
                        worldRandom, 
                        nextEntityInstanceId
                    )
                ),
            };

            return new MapState(levelStates, nextLevelInstanceId.Value, nextEntityInstanceId.Value);

            static LevelScale SelectTestLevelScale(System.Random random)
            {
                // TODO: 테스트용 레벨 스케일 선택 로직 구현 (현재는 임의로 선택)
                (LevelScale scale, float prob)[] values = new (LevelScale, float)[]
                {
                    (LevelScale.Small, 0.05f),
                    (LevelScale.Medium, 0.45f),
                    (LevelScale.Large, 0.75f),
                    (LevelScale.VeryLarge, 0.95f),
                    (LevelScale.UltraLarge, 1f)
                };

                float randomValue = (float)random.NextDouble();
                foreach (var (scale, prob) in values)
                {
                    if (randomValue <= prob)
                        return scale;
                }
                return LevelScale.Medium; // Default fallback
            }
        }

        private static async Task<LevelState> GenerateLevel(RandomLevelGenerateParameter parameter)
        {
            Ref<int> nextRoomInstanceId = new(0);

            Point levelSize = parameter.levelScale.GetSize();
            List<Point> levelPoints = levelSize.SizeToPoints();

            int halfWidth = levelSize.X / 2, 
                halfHeight = levelSize.Y / 2;
            Point startPoint = levelPoints.GetRandomPoint(
                random: parameter.worldRandom,
                filter: point => point.X == -halfWidth || point.X == halfWidth || point.Y == -halfHeight || point.Y == halfHeight
            );
            levelPoints.Remove(startPoint);
            Point endPoint = levelPoints.GetRandomAroundPoint(startPoint * -1,
                radius: 1,
                isContainTarget: true,
                random: parameter.worldRandom,
                filter: null
            );

            levelPoints = levelSize.SizeToPoints();
            List<Point> pathPoints = PointsExtensions.GetPath(levelPoints, startPoint, endPoint);
            if (pathPoints == null || pathPoints.Count == 0) throw new InvalidOperationException($"Failed to generate a valid path from {startPoint} to {endPoint}.");

            HashSet<Point> additionalPathPoints = new();
            (int cur, int max) branchWeight = parameter.levelScale switch
            {
                LevelScale.UltraSmall => (0, 0),
                LevelScale.VerySmall => (0, 1),
                LevelScale.Small => (1, 2),
                LevelScale.Medium => (2, 3),
                LevelScale.Large => (3, 5),
                LevelScale.VeryLarge => (4, 8),
                _ => throw new NotImplementedException($"Branch weight for levelScale '{parameter.levelScale}' is not implemented."),
            };
            for (int i = 0; i < pathPoints.Count; i++)
            {
                CreateAdditionalRoutePoints(pathPoints[i], branchWeight);

                void CreateAdditionalRoutePoints(Point target, (int cur, int max) weight)
                {
                    if (weight.cur <= 0) return;

                    List<Point> neighborPoints = levelPoints.GetNeighborPoints(target);

                    for (int j = 0; j < neighborPoints.Count; j++)
                    {
                        Point candidate = neighborPoints[j];
                        if (additionalPathPoints.Contains(candidate)) continue;

                        float probability = (float)weight.cur / weight.max;
                        if (parameter.worldRandom.NextDouble() < probability)
                        {
                            additionalPathPoints.Add(candidate);
                            levelPoints.Remove(candidate);
                            CreateAdditionalRoutePoints(candidate, (weight.cur - 1, weight.max));
                        }
                    }
                }
            }
            pathPoints.AddRange(additionalPathPoints);

            Dictionary<Point, RoomState> roomStates = new();
            HashSet<Point> remainingPoints = new(pathPoints);
            remainingPoints.RemoveWhere(point => point.Equals(startPoint) || point.Equals(endPoint));
            //roomStates[startPoint] = GenerateRoom(nextRoomInstanceId.Value++, new Point[] { startPoint }, RoomScale.Single, parameter);
            while (remainingPoints.Count > 0)
            {
                Point anchorPoint = remainingPoints.GetRandomPoint(parameter.worldRandom);

                RoomScale roomScale = RoomScale.Single;
                Point[] groupedPoints = new Point[] { anchorPoint };

                if (TryCreateQuadGroup(anchorPoint, remainingPoints, parameter.worldRandom, out Point[] quadPoints))
                {
                    roomScale = RoomScale.Quad;
                    groupedPoints = quadPoints;
                }
                else if (TryCreateDoubleGroup(anchorPoint, remainingPoints, parameter.worldRandom, out Point[] doublePoints))
                {
                    roomScale = RoomScale.Double;
                    groupedPoints = doublePoints; 
                }

                RoomDocument roomDocument = ArchiveManager.levelCollection.GetLevelDocument(parameter.levelId)
                    .roomCollection.ChoiceOne(roomScale, RoomType.Random, parameter.worldRandom);
                if (roomDocument== null)
                    throw new InvalidOperationException($"No RoomDocument found for levelId '{parameter.levelId}', roomScale '{roomScale}', and roomType 'Random'.");
                RoomView roomView = await roomDocument.InstantiateRoomView(
                    nextRoomInstanceId.Value++,
                    groupedPoints
                );
                RoomState roomState = roomView.State;
                if(roomState.scale == RoomScale.Double)
                {
                    Point size = new(Mathf.Abs(groupedPoints[0].X - groupedPoints[1].X) + 1, Mathf.Abs(groupedPoints[0].Y - groupedPoints[1].Y) + 1);
                    // if y of room size is longer than x, then rotate the room view 90 degrees counterclockwise
                    if(size.Y > size.X)
                        roomView.RotateRoomViews(90);
                }

                foreach (Point point in groupedPoints)
                {
                    roomStates[point] = roomState;
                    remainingPoints.Remove(point);
                }

                static bool TryCreateDoubleGroup(Point anchorPoint, HashSet<Point> availablePoints, System.Random random, out Point[] groupedPoints)
                {
                    groupedPoints = null;

                    if (random.NextDouble() > 0.1f)
                        return false;

                    Point[] directions = new[] {
                        new Point(1, 0),
                        new Point(-1, 0),
                        new Point(0, 1),
                        new Point(0, -1),
                    };

                    List<Point> candidates = new();
                    foreach (Point dirPoint in directions)
                    {
                        Point neighbor = anchorPoint + dirPoint;
                        if (availablePoints.Contains(neighbor))
                            candidates.Add(neighbor);
                    }

                    if (candidates.Count == 0)
                        return false;

                    Point selectedNeighbor = candidates[random.Next(candidates.Count)];
                    groupedPoints = new Point[] { anchorPoint, selectedNeighbor };
                    return true;
                }

                static bool TryCreateQuadGroup(Point anchorPoint, HashSet<Point> availablePoints, System.Random random, out Point[] groupedPoints)
                {
                    groupedPoints = null;

                    if (random.NextDouble() > 0.15f)
                        return false;

                    List<Point[]> candidates = new();
                    for (int offsetY = -1; offsetY <= 0; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 0; offsetX++)
                        {
                            Point topLeft = new(anchorPoint.X + offsetX, anchorPoint.Y + offsetY);
                            Point topRight = topLeft + new Point(1, 0);
                            Point bottomLeft = topLeft + new Point(0, 1);
                            Point bottomRight = topLeft + new Point(1, 1);

                            if (availablePoints.Contains(topLeft) &&
                                availablePoints.Contains(topRight) &&
                                availablePoints.Contains(bottomLeft) &&
                                availablePoints.Contains(bottomRight))
                            {
                                candidates.Add(new Point[] { topLeft, topRight, bottomLeft, bottomRight });
                            }
                        }
                    }

                    if (candidates.Count == 0)
                        return false;

                    groupedPoints = candidates[random.Next(candidates.Count)];
                    return true;
                }
            }
            //roomStates[endPoint] = GenerateRoom(nextRoomInstanceId.Value++, new Point[] { endPoint }, RoomScale.Single, parameter);

            HashSet<string> linkedPairs = new();
            Point[] directions = new[]
            {
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(0, -1),
            };

            foreach (RoomState roomState in roomStates.Values.Distinct())
            {
                foreach (Point roomPosition in roomState.position)
                {
                    foreach (Point directionPoint in directions)
                    {
                        Point neighborPosition = roomPosition + directionPoint;
                        if (!roomStates.TryGetValue(neighborPosition, out RoomState neighborRoom))
                            continue;
                        if (ReferenceEquals(roomState, neighborRoom))
                            continue;

                        int roomA = roomState.roomInstanceId ?? -1;
                        int roomB = neighborRoom.roomInstanceId ?? -1;
                        string pairKey = roomA < roomB ? $"{roomA}:{roomB}" : $"{roomB}:{roomA}";
                        if (!linkedPairs.Add(pairKey))
                            continue;
                        if (roomState.IsLinkedWith(neighborRoom) && neighborRoom.IsLinkedWith(roomState))
                            continue;

                        var warpPoint1 = CalcWarpPointPos(roomState, roomPosition, neighborRoom, neighborPosition);
                        var warpPoint2 = CalcWarpPointPos(neighborRoom, neighborPosition, roomState, roomPosition);

                        WarpPointState warpPointState1 = new(
                            entityId: $"wp_{roomState.roomInstanceId}_{neighborRoom.roomInstanceId}",
                            entityInstanceId: parameter.nextEntityInstanceId.Value++,
                            pos: warpPoint1,
                            target: new LevelPoint(parameter.levelInstanceId, neighborRoom.roomInstanceId, warpPoint2)
                        );
                        WarpPointState warpPointState2 = new(
                            entityId: $"wp_{neighborRoom.roomInstanceId}_{roomState.roomInstanceId}",
                            entityInstanceId: parameter.nextEntityInstanceId.Value++,
                            pos: warpPoint2,
                            target: new LevelPoint(parameter.levelInstanceId, roomState.roomInstanceId, warpPoint1)
                        );

                        static Point CalcWarpPointPos(RoomState fromRoom, Point fromPosition, RoomState toRoom, Point toPosition)
                        {
                            (int width, int height) = (fromRoom.size.X, fromRoom.size.Y);

                            Point directionPoint = fromPosition - toPosition;
                            Direction direction = directionPoint.To4Direction();
                            Point warpPoint = directionPoint * direction switch {
                                Direction.Left or Direction.Right => width / 2 + 1,
                                Direction.Up or Direction.Down => height / 2 + 1,
                                _ => throw new NotImplementedException($"Warp point for direction '{direction}' is not implemented."),
                            };
                            switch (fromRoom.scale)
                            {
                                case RoomScale.Single:
                                    break;
                                case RoomScale.Double or RoomScale.Quad:
                                    Vector2 roomCenter = fromRoom.position.Aggregate(Vector2.zero, (acc, p) => acc + (Vector2)p) / fromRoom.position.Length;
                                    Vector2 relativeNeighborPosition = (Vector2)toPosition - roomCenter;

                                    if(direction == Direction.Left || direction == Direction.Right)
                                        warpPoint.Y = relativeNeighborPosition.y > 0 ? height / 4 : -height / 4;
                                    if (direction == Direction.Up || direction == Direction.Down)
                                        warpPoint.X = relativeNeighborPosition.x > 0 ? width / 4 : -width / 4;

                                    break;
                                default:
                                    throw new NotImplementedException($"Warp point generation for roomScale '{fromRoom.scale}' is not implemented.");
                            }

                            return warpPoint;
                        }

                        roomState.AddWarpPoint(warpPoint1, warpPointState1);
                        neighborRoom.AddWarpPoint(warpPoint2, warpPointState2);

                        break;
                    }
                }
            }

            return new LevelState(parameter.levelInstanceId, parameter.levelId, parameter.levelScale, levelSize, roomStates, startPoint, endPoint, nextRoomInstanceId.Value);
        }

        #endregion
    }

}