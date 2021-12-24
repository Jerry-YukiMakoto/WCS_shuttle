using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mirle.DRCS
{
    public class RouteService
    {
        private const uint _DefaultCost = 10000;

        private readonly Dictionary<ILocation, List<ILocation>> _dvicePath = new Dictionary<ILocation, List<ILocation>>();
        private readonly Dictionary<ILocation, List<ILocation>> _path = new Dictionary<ILocation, List<ILocation>>();
        private readonly Dictionary<ILocation, List<ILocation>> _disablePath = new Dictionary<ILocation, List<ILocation>>();
        private readonly Dictionary<ILocation, Dictionary<ILocation, uint>> _pathCost = new Dictionary<ILocation, Dictionary<ILocation, uint>>();
        private readonly Dictionary<ILocation, Dictionary<ILocation, uint>> _disablePathCost = new Dictionary<ILocation, Dictionary<ILocation, uint>>();
        private readonly Dictionary<ILocation, Dictionary<ILocation, uint>> _pathDefaultCost = new Dictionary<ILocation, Dictionary<ILocation, uint>>();
        private readonly object _lock = new object();

        public RouteService()
        {
        }

        public void AddDevicePath(ILocation node, ILocation nextLocation, uint cost)
        {
            lock (_lock)
            {
                if (_dvicePath.ContainsKey(node) == false)
                {
                    _dvicePath.Add(node, new List<ILocation>());
                }

                if (_path.ContainsKey(node) == false)
                {
                    _path.Add(node, new List<ILocation>());
                }

                if (_disablePath.ContainsKey(node) == false)
                {
                    _disablePath.Add(node, new List<ILocation>());
                }

                if (_pathCost.ContainsKey(node) == false)
                {
                    _pathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_disablePathCost.ContainsKey(node) == false)
                {
                    _disablePathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_pathDefaultCost.ContainsKey(node) == false)
                {
                    _pathDefaultCost.Add(node, new Dictionary<ILocation, uint>());
                }

                _path[node].Add(nextLocation);
                _dvicePath[node].Add(nextLocation);
                _pathCost[node][nextLocation] = _DefaultCost + cost;
                _pathDefaultCost[node][nextLocation] = _DefaultCost + cost;
            }
        }

        public void AddDevicePath(ILocation node, ILocation nextLocation)
        {
            AddDevicePath(node, nextLocation, 0);
        }

        public void AddPath(ILocation node, ILocation nextLocation, uint cost)
        {
            lock (_lock)
            {
                if (_path.ContainsKey(node) == false)
                {
                    _path.Add(node, new List<ILocation>());
                }

                if (_disablePath.ContainsKey(node) == false)
                {
                    _disablePath.Add(node, new List<ILocation>());
                }

                if (_pathCost.ContainsKey(node) == false)
                {
                    _pathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_disablePathCost.ContainsKey(node) == false)
                {
                    _disablePathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_pathDefaultCost.ContainsKey(node) == false)
                {
                    _pathDefaultCost.Add(node, new Dictionary<ILocation, uint>());
                }

                _path[node].Add(nextLocation);
                _pathCost[node][nextLocation] = _DefaultCost + cost;
                _pathDefaultCost[node][nextLocation] = _DefaultCost + cost;
            }
        }

        public void AddPath(ILocation node, ILocation nextLocation)
        {
            AddPath(node, nextLocation, 1);
        }

        public void SetCost(ILocation node, ILocation nextLocation, uint cost)
        {
            lock (_lock)
            {
                if (_pathCost.ContainsKey(node) == false)
                {
                    _pathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_disablePathCost.ContainsKey(node) == false)
                {
                    _disablePathCost.Add(node, new Dictionary<ILocation, uint>());
                }

                if (_pathDefaultCost.ContainsKey(node) == false)
                {
                    _pathDefaultCost.Add(node, new Dictionary<ILocation, uint>());
                }

                _pathCost[node][nextLocation] = _DefaultCost + cost;
                _pathDefaultCost[node][nextLocation] = _DefaultCost + cost;
            }
        }

        public void SetCost(ILocation node, ILocation nextLocation)
        {
            SetCost(node, nextLocation, 1);
        }

        public void AddCost(ILocation node, ILocation nextLocation, uint cost)
        {
            lock (_lock)
            {
                if (_pathCost.TryGetValue(node, out var costList) == false)
                {
                    _pathCost.Add(node, new Dictionary<ILocation, uint>());
                    _pathCost[node][nextLocation] = _DefaultCost + cost;
                }
                else if (costList.TryGetValue(nextLocation, out uint originalCost))
                {
                    costList[nextLocation] = originalCost + cost;
                }
                else
                {
                    costList.Add(nextLocation, _DefaultCost + cost);
                }
            }
        }

        public void EnalePath(ILocation node, ILocation nextLocation, bool enable)
        {
            lock (_lock)
            {
                if (enable)
                {
                    if (_disablePath.TryGetValue(node, out var disableNextLocations))
                    {
                        if (disableNextLocations.Contains(nextLocation))
                        {
                            if (_path.TryGetValue(node, out var enableNextLocations) == false)
                            {
                                _path.Add(node, new List<ILocation>());
                            }

                            _disablePathCost[node].TryGetValue(nextLocation, out uint cost);
                            _disablePathCost[node].Remove(nextLocation);
                            _pathCost[node].Add(nextLocation, cost);
                            _path[node].Add(nextLocation);
                            _disablePath[node].Remove(nextLocation);
                        }
                        else if (_path.ContainsKey(node) == false)
                        {
                            //沒有路徑
                        }
                    }
                }
                else
                {
                    if (_path.TryGetValue(node, out var enableNextLocations))
                    {
                        if (enableNextLocations.Contains(nextLocation))
                        {
                            if (_disablePath.TryGetValue(node, out var disableNextLocations) == false)
                            {
                                _disablePath.Add(node, new List<ILocation>());
                            }

                            if (_disablePathCost.ContainsKey(node) == false)
                            {
                                _disablePathCost.Add(node, new Dictionary<ILocation, uint>());
                            }

                            _pathCost[node].TryGetValue(nextLocation, out uint cost);
                            _disablePathCost[node].Add(nextLocation, cost);
                            _disablePath[node].Add(nextLocation);
                            _path[node].Remove(nextLocation);
                            _pathCost[node].Remove(nextLocation);
                        }
                        else if (_disablePath.ContainsKey(node) == false)
                        {
                            //沒有路徑
                        }
                    }
                }
            }
        }

        public void EnalePath(ILocation node, bool enable)
        {
            lock (_lock)
            {
                if (enable)
                {
                    if (_disablePath.TryGetValue(node, out var disableNextLocations))
                    {
                        if (_path.TryGetValue(node, out var enableNextLocations) == false)
                        {
                            _path.Add(node, new List<ILocation>());
                        }

                        foreach (var nextLocation in enableNextLocations)
                        {
                            _disablePathCost[node].TryGetValue(nextLocation, out uint cost);
                            _disablePathCost[node].Remove(nextLocation);
                            _pathCost[node][nextLocation] = cost;
                        }

                        _path[node].AddRange(disableNextLocations);
                        _disablePath.Remove(node);
                    }
                    else if (_path.ContainsKey(node) == false)
                    {
                        //沒有路徑
                    }
                }
                else
                {
                    if (_path.TryGetValue(node, out var enableNextLocations))
                    {
                        if (_disablePath.TryGetValue(node, out var disableNextLocations) == false)
                        {
                            _disablePath.Add(node, new List<ILocation>());
                        }

                        if (_disablePathCost.ContainsKey(node) == false)
                        {
                            _disablePathCost.Add(node, new Dictionary<ILocation, uint>());
                        }

                        _pathCost.TryGetValue(node, out var enableNextLocationsCost);
                        _disablePathCost[node] = enableNextLocationsCost;
                        _disablePath[node].AddRange(disableNextLocations);
                        _path.Remove(node);
                        _pathCost.Remove(node);
                    }
                    else if (_disablePath.ContainsKey(node) == false)
                    {
                        //沒有路徑
                    }
                }
            }
        }

        public void ResetCost()
        {
            lock (_lock)
            {
                _pathCost.Clear();
                foreach (var node in _pathDefaultCost)
                {
                    foreach (var nextNode in node.Value)
                    {
                        _pathCost.Add(node.Key, new Dictionary<ILocation, uint>());
                        _pathCost[node.Key][nextNode.Key] = nextNode.Value;
                    }
                }
            }
        }

        public void ClearPath()
        {
            lock (_lock)
            {
                _path.Clear();
                _pathCost.Clear();
            }
        }

        public IEnumerable<ILocation> GetPath(ILocation startLocation, ILocation endLocation)
        {
            lock (_lock)
            {
                var path = new List<ILocation>();
                try
                {
                    var prior = Dijkstra(startLocation);
                    AddPathPoint(endLocation, startLocation);

                    void AddPathPoint(ILocation end, ILocation start)
                    {
                        if (end.Equals(start))
                        {
                            return;
                        }

                        if (prior.TryGetValue(end, out var next))
                        {
                            if ((typeof(ILocation).IsValueType && next.Equals(default(ILocation))) || next == null)
                            {
                                return;
                            }

                            path.Add(end);
                            if (next.Equals(start))
                            {
                                path.Add(start);
                                path.Reverse();
                                return;
                            }
                            AddPathPoint(next, start);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }

                return path;
            }
        }

        public IEnumerable<KeyValuePair<ILocation, ILocation>> GetAvailablePath()
        {
            lock (_lock)
            {
                var keyValuePairs = new List<KeyValuePair<ILocation, ILocation>>();
                foreach (var node1 in _path.Keys)
                {
                    foreach (var node2 in _path[node1])
                    {
                        keyValuePairs.Add(new KeyValuePair<ILocation, ILocation>(node1, node2));
                    }
                }

                return keyValuePairs;
            }
        }

        public IEnumerable<KeyValuePair<ILocation, ILocation>> GetUnavailablePath()
        {
            lock (_lock)
            {
                var keyValuePairs = new List<KeyValuePair<ILocation, ILocation>>();
                foreach (var node1 in _disablePath.Keys)
                {
                    foreach (var node2 in _disablePath[node1])
                    {
                        keyValuePairs.Add(new KeyValuePair<ILocation, ILocation>(node1, node2));
                    }
                }

                return keyValuePairs;
            }
        }

        public IEnumerable<KeyValuePair<ILocation, ILocation>> GetAllPath()
        {
            lock (_lock)
            {
                var keyValuePairs = new List<KeyValuePair<ILocation, ILocation>>();
                foreach (var node1 in _path.Keys)
                {
                    foreach (var node2 in _path[node1])
                    {
                        keyValuePairs.Add(new KeyValuePair<ILocation, ILocation>(node1, node2));
                    }
                }

                foreach (var node1 in _disablePath.Keys)
                {
                    foreach (var node2 in _disablePath[node1])
                    {
                        keyValuePairs.Add(new KeyValuePair<ILocation, ILocation>(node1, node2));
                    }
                }

                return keyValuePairs;
            }
        }

        public bool IsDevicePath(ILocation node, ILocation nextLocation)
        {
            lock (_lock)
            {
                if (_dvicePath.ContainsKey(node))
                {
                    return _dvicePath[node].Contains(nextLocation);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsPath(ILocation node, ILocation nextLocation)
        {
            lock (_lock)
            {
                if (_path.ContainsKey(node))
                {
                    return _path[node].Contains(nextLocation);
                }
                else
                {
                    return false;
                }
            }
        }

        public string Print()
        {
            lock (_lock)
            {
                var str = new StringBuilder();
                str.AppendLine($"Locations:");
                foreach (var keyValuePairMap in _path)
                {
                    str.AppendLine($"{keyValuePairMap.Key.LocationId} => {string.Join(", ", keyValuePairMap.Value.Select(x => $"{x.LocationId}"))}");
                }

                str.AppendLine();
                str.AppendLine($"Cost:");
                str.AppendLine($"{string.Empty,-10}\t\t{string.Join("\t\t", _path.Keys.Select(x => $"{x.LocationId}".PadRight(10, ' ')))}");
                foreach (var node in _path.Keys)
                {
                    str.Append($"{node.LocationId}".PadRight(10, ' '));
                    if (_pathCost.TryGetValue(node, out var oLocationCosts))
                    {
                        foreach (var oLocation in _path.Keys)
                        {
                            if (oLocationCosts.TryGetValue(oLocation, out uint cost))
                            {
                                str.Append($"\t\t{cost,-10}");
                            }
                            else
                            {
                                str.Append($"\t\t{"0",-10}");
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _path.Keys.Count; i++)
                        {
                            str.Append($"\t0");
                        }
                    }

                    str.AppendLine();
                }
                return str.ToString();
            }
        }

        private Dictionary<ILocation, ILocation> Dijkstra(ILocation start)
        {
            //Create Map
            var map = new Dictionary<ILocation, List<ILocation>>();
            foreach (var keyValuePairLocation in _path)
            {
                var list = new List<ILocation>();
                map.Add(keyValuePairLocation.Key, list);
                foreach (var oLocation in keyValuePairLocation.Value)
                {
                    if (_path.ContainsKey(oLocation))
                    {
                        list.Add(oLocation);
                    }
                    else
                    {
                        if (!map.ContainsKey(oLocation))
                        {
                            map.Add(oLocation, new List<ILocation>());
                        }

                        if (!_pathCost.ContainsKey(oLocation))
                        {
                            _pathCost.Add(oLocation, new Dictionary<ILocation, uint>());
                        }
                    }
                }
            }

            //point, dist
            var dist = new Dictionary<ILocation, uint>();
            foreach (var p in map)
            {
                dist[p.Key] = _DefaultCost;
            }

            //point, prior
            var prior = new Dictionary<ILocation, ILocation>();
            foreach (var p in map)
            {
                prior[p.Key] = default(ILocation);
            }

            //point, decided
            var decided = new Dictionary<ILocation, bool>();
            foreach (var p in map)
            {
                decided[p.Key] = false;
            }

            foreach (var point in map)
            {
                var key = point.Key;
                if (_pathCost[start].TryGetValue(key, out uint value))
                {
                    dist[key] = value;
                }
                else
                {
                    dist[key] = uint.MaxValue;
                }
                prior[key] = start;
                decided[key] = false;
            }

            try
            {
                //Dijkstra
                decided[start] = true;
                var Vx = default(ILocation);
                foreach (var point in map)
                {
                    find_min(ref Vx);
                    if ((typeof(ILocation).IsValueType && Vx.Equals(default(ILocation))) || Vx == null)
                    {
                        foreach (var d in decided)
                        {
                            if (d.Value == false)
                            {
                                prior[d.Key] = default(ILocation);
                            }
                        }

                        return prior;
                    }
                    decided[Vx] = true;
                    foreach (var w in _pathCost[Vx])
                    {
                        var key = w.Key;
                        if (_pathCost[Vx].TryGetValue(key, out uint couldCost) == false)
                        {
                            couldCost = uint.MaxValue;
                        }

                        if (w.Value < uint.MaxValue && !decided[key] && (dist[key] > (dist[Vx] + couldCost)))
                        {
                            dist[key] = dist[Vx] + couldCost;
                            prior[key] = Vx;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

            foreach (var d in decided)
            {
                if (d.Value == false)
                {
                    prior[d.Key] = default(ILocation);
                }
            }

            return prior;

            void find_min(ref ILocation vx)
            {
                var low = default(ILocation);
                uint lowest = uint.MaxValue;
                foreach (var point in map.Keys)
                {
                    if (decided[point] == false && dist[point] < lowest)
                    {
                        lowest = dist[point];
                        low = point;
                    }
                }

                vx = low;
            }
        }
    }
}
