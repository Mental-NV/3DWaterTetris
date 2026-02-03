using System.Collections.Generic;

namespace Floodline.Core;

public sealed record WaterSettleResult(int TotalUnits, int FilledUnits, int OverflowUnits);

public static class WaterSolver
{
    private static readonly Int3[] NeighborOffsets =
    [
        new Int3(1, 0, 0),
        new Int3(-1, 0, 0),
        new Int3(0, 1, 0),
        new Int3(0, -1, 0),
        new Int3(0, 0, 1),
        new Int3(0, 0, -1)
    ];

    public static WaterSettleResult Settle(Grid grid, GravityDirection gravity, IReadOnlyList<Int3> displacedSources) =>
        Settle(grid, gravity, displacedSources, null);

    public static WaterSettleResult Settle(
        Grid grid,
        GravityDirection gravity,
        IReadOnlyList<Int3> displacedSources,
        ISet<Int3>? blockedCells)
    {
        if (grid is null)
        {
            throw new ArgumentNullException(nameof(grid));
        }

        if (displacedSources is null)
        {
            throw new ArgumentNullException(nameof(displacedSources));
        }

        List<Int3> sources = [];
        int waterUnits = 0;

        int sizeX = grid.Size.X;
        int sizeY = grid.Size.Y;
        int sizeZ = grid.Size.Z;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Int3 pos = new(x, y, z);
                    Voxel voxel = grid.GetVoxel(pos);
                    if (voxel.Type != OccupancyType.Water)
                    {
                        continue;
                    }

                    sources.Add(pos);
                    waterUnits++;
                    grid.SetVoxel(pos, Voxel.Empty);
                }
            }
        }

        foreach (Int3 source in displacedSources)
        {
            if (!grid.IsInBounds(source))
            {
                continue;
            }

            sources.Add(source);
            waterUnits++;
        }

        if (waterUnits == 0)
        {
            return new WaterSettleResult(0, 0, 0);
        }

        int[,,] req = new int[sizeX, sizeY, sizeZ];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    req[x, y, z] = int.MaxValue;
                }
            }
        }

        MinHeap queue = new();
        foreach (Int3 source in sources)
        {
            if (!grid.IsInBounds(source) || IsBlocked(source, blockedCells))
            {
                continue;
            }

            Voxel voxel = grid.GetVoxel(source);
            if (!IsPassable(voxel))
            {
                continue;
            }

            int elev = DeterministicOrdering.GetGravElev(source, gravity);
            int currentReq = req[source.X, source.Y, source.Z];
            if (elev < currentReq)
            {
                req[source.X, source.Y, source.Z] = elev;
                queue.Enqueue(new QueueNode(source, elev, elev, DeterministicOrdering.GetTieCoord(source, gravity)));
            }
        }

        while (queue.Count > 0)
        {
            QueueNode node = queue.Dequeue();
            Int3 pos = node.Pos;
            if (node.Req != req[pos.X, pos.Y, pos.Z])
            {
                continue;
            }

            foreach (Int3 offset in NeighborOffsets)
            {
                Int3 next = pos + offset;
                if (!grid.IsInBounds(next) || IsBlocked(next, blockedCells))
                {
                    continue;
                }

                Voxel nextVoxel = grid.GetVoxel(next);
                if (!IsPassable(nextVoxel))
                {
                    continue;
                }

                int nextElev = DeterministicOrdering.GetGravElev(next, gravity);
                int candidateReq = node.Req > nextElev ? node.Req : nextElev;

                if (candidateReq < req[next.X, next.Y, next.Z])
                {
                    req[next.X, next.Y, next.Z] = candidateReq;
                    queue.Enqueue(new QueueNode(next, candidateReq, nextElev, DeterministicOrdering.GetTieCoord(next, gravity)));
                }
            }
        }

        List<FillCandidate> candidates = [];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Int3 pos = new(x, y, z);
                    if (IsBlocked(pos, blockedCells))
                    {
                        continue;
                    }

                    Voxel voxel = grid.GetVoxel(pos);
                    if (!IsOccupiable(voxel))
                    {
                        continue;
                    }

                    int reqVal = req[x, y, z];
                    if (reqVal == int.MaxValue)
                    {
                        continue;
                    }

                    int elev = DeterministicOrdering.GetGravElev(pos, gravity);
                    TieCoord tie = DeterministicOrdering.GetTieCoord(pos, gravity);
                    candidates.Add(new FillCandidate(pos, reqVal, elev, tie));
                }
            }
        }

        candidates.Sort(CompareCandidates);

        int filled = waterUnits <= candidates.Count ? waterUnits : candidates.Count;
        for (int i = 0; i < filled; i++)
        {
            grid.SetVoxel(candidates[i].Pos, Voxel.Water);
        }

        int overflow = waterUnits - filled;
        if (overflow < 0)
        {
            overflow = 0;
        }

        return new WaterSettleResult(waterUnits, filled, overflow);
    }

    private static bool IsBlocked(Int3 pos, ISet<Int3>? blockedCells) =>
        blockedCells != null && blockedCells.Contains(pos);

    private static bool IsPassable(Voxel voxel) =>
        voxel.Type is OccupancyType.Empty or OccupancyType.Porous;

    private static bool IsOccupiable(Voxel voxel) => voxel.Type == OccupancyType.Empty;

    private static int CompareCandidates(FillCandidate left, FillCandidate right)
    {
        int reqComp = left.RequiredSurface.CompareTo(right.RequiredSurface);
        if (reqComp != 0)
        {
            return reqComp;
        }

        int elevComp = left.Elevation.CompareTo(right.Elevation);
        return elevComp != 0 ? elevComp : left.Tie.CompareTo(right.Tie);
    }

    private sealed class MinHeap
    {
        private readonly List<QueueNode> _items = [];

        public int Count => _items.Count;

        public void Enqueue(QueueNode node)
        {
            _items.Add(node);
            SiftUp(_items.Count - 1);
        }

        public QueueNode Dequeue()
        {
            QueueNode root = _items[0];
            int lastIndex = _items.Count - 1;
            QueueNode last = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (_items.Count > 0)
            {
                _items[0] = last;
                SiftDown(0);
            }

            return root;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (Compare(_items[index], _items[parent]) >= 0)
                {
                    return;
                }

                (_items[index], _items[parent]) = (_items[parent], _items[index]);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = _items.Count;
            while (true)
            {
                int left = (index * 2) + 1;
                int right = left + 1;
                int smallest = index;

                if (left < count && Compare(_items[left], _items[smallest]) < 0)
                {
                    smallest = left;
                }

                if (right < count && Compare(_items[right], _items[smallest]) < 0)
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    return;
                }

                (_items[index], _items[smallest]) = (_items[smallest], _items[index]);
                index = smallest;
            }
        }

        private static int Compare(QueueNode left, QueueNode right)
        {
            int reqComp = left.Req.CompareTo(right.Req);
            if (reqComp != 0)
            {
                return reqComp;
            }

            int elevComp = left.Elevation.CompareTo(right.Elevation);
            return elevComp != 0 ? elevComp : left.Tie.CompareTo(right.Tie);
        }
    }

    private readonly record struct QueueNode(Int3 Pos, int Req, int Elevation, TieCoord Tie);

    private readonly record struct FillCandidate(Int3 Pos, int RequiredSurface, int Elevation, TieCoord Tie);
}
